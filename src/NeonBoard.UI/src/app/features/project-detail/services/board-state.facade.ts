import { Injectable, inject, signal, computed } from '@angular/core';
import { BoardService } from './board.service';
import { ColumnService } from './column.service';
import { CardService } from './card.service';
import { DrawerService } from './drawer.service';
import { BoardHubService } from './board-hub.service';
import { ToastService } from '../../../core/services/toast.service';
import { BoardDetails } from '../models/board.model';
import { Column } from '../models/column.model';
import { Card } from '../models/card.model';
import { Label } from '../models/label.model';
import { ActivityEntry } from '../models/activity.model';

@Injectable({
  providedIn: 'root'
})
export class BoardStateFacade {
  private boardService = inject(BoardService);
  private columnService = inject(ColumnService);
  private cardService = inject(CardService);
  private drawerService = inject(DrawerService);
  private boardHub = inject(BoardHubService);
  private toastService = inject(ToastService);

  private _board = signal<BoardDetails | null>(null);
  private _isLoading = signal<boolean>(false);
  private _error = signal<string | null>(null);
  private _currentProjectId = signal<string>('');
  private _currentBoardId = signal<string>('');
  private _selectedLabelIds = signal<Set<string>>(new Set());
  private _showArchivePanel = signal<boolean>(false);
  private _archivedCards = signal<Card[]>([]);
  private _isLoadingArchive = signal<boolean>(false);
  private _showActivityPanel = signal(false);
  private _activityEntries = signal<ActivityEntry[]>([]);
  private _activityNextCursor = signal<string | null>(null);
  private _isLoadingActivity = signal(false);

  readonly board = this._board.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly selectedLabelIds = this._selectedLabelIds.asReadonly();
  readonly showArchivePanel = this._showArchivePanel.asReadonly();
  readonly archivedCards = this._archivedCards.asReadonly();
  readonly isLoadingArchive = this._isLoadingArchive.asReadonly();
  readonly showActivityPanel = this._showActivityPanel.asReadonly();
  readonly activityEntries = this._activityEntries.asReadonly();
  readonly activityNextCursor = this._activityNextCursor.asReadonly();
  readonly isLoadingActivity = this._isLoadingActivity.asReadonly();

  readonly columns = computed(() => this._board()?.columns ?? []);
  readonly labels = computed(() => this._board()?.labels ?? []);
  readonly isFilterActive = computed(() => this._selectedLabelIds().size > 0);

  readonly cardsByColumn = computed(() => {
    const cards = this._board()?.cards ?? [];
    const result: Record<string, Card[]> = {};

    this.columns().forEach(col => {
      result[col.id] = [];
    });

    cards.forEach(card => {
      if (result[card.columnId]) {
        result[card.columnId].push(card);
      }
    });

    Object.keys(result).forEach(columnId => {
      result[columnId].sort((a, b) => (a.position < b.position ? -1 : a.position > b.position ? 1 : 0));
    });

    return result;
  });

  readonly filteredCardsByColumn = computed(() => {
    const ids = this._selectedLabelIds();
    if (ids.size === 0) return this.cardsByColumn();
    return Object.fromEntries(
      Object.entries(this.cardsByColumn()).map(([colId, cards]) => [
        colId,
        cards.filter(c => c.labels.some(l => ids.has(l.id)))
      ])
    );
  });

  constructor() {
    this.drawerService.cardUpdated$.subscribe(() => {
      if (this._currentProjectId() && this._currentBoardId()) {
        this.loadBoard(this._currentProjectId(), this._currentBoardId(), false);
      }
    });

    this.drawerService.cardDeleted$.subscribe(() => {
      if (this._currentProjectId() && this._currentBoardId()) {
        this.loadBoard(this._currentProjectId(), this._currentBoardId(), false);
        if (this._showArchivePanel()) {
          this.loadArchivedCards();
        }
      }
    });

    this.drawerService.cardArchived$.subscribe(() => {
      if (this._currentProjectId() && this._currentBoardId()) {
        this.loadBoard(this._currentProjectId(), this._currentBoardId(), false);
        if (this._showArchivePanel()) {
          this.loadArchivedCards();
        }
      }
    });
  }

  private get isConnected(): boolean {
    return this.boardHub.connectionState() === 'connected';
  }

  loadBoard(projectId: string, boardId: string, showLoading = true): void {
    const boardChanged = this._currentBoardId() !== boardId;
    this._currentProjectId.set(projectId);
    this._currentBoardId.set(boardId);
    if (boardChanged) {
      this.boardHub.leaveBoard();
      this._selectedLabelIds.set(new Set());
      this._showArchivePanel.set(false);
      this._archivedCards.set([]);
      this._isLoadingArchive.set(false);
      this._showActivityPanel.set(false);
      this._activityEntries.set([]);
      this._activityNextCursor.set(null);
      this._isLoadingActivity.set(false);

      this.boardHub.joinBoard(boardId).then(() => {
        this.subscribeToBoardEvents();
      });
    }

    if (showLoading) {
      this._isLoading.set(true);
    }
    this._error.set(null);

    this.boardService.getBoardDetails(projectId, boardId).subscribe({
      next: (board) => {
        this._board.set(board);
        this.drawerService.setBoardLabels(board.labels);
        this._isLoading.set(false);
      },
      error: () => {
        this._error.set('Failed to load board');
        this._isLoading.set(false);
      }
    });
  }

  reorderColumns(projectId: string, boardId: string, columnIds: string[]): void {
    const currentBoard = this._board();
    if (!currentBoard) return;

    const reorderedColumns = columnIds
      .map(id => currentBoard.columns.find(col => col.id === id))
      .filter((col): col is Column => col !== undefined);

    // Optimistic update for instant UI feedback
    this._board.set({
      ...currentBoard,
      columns: reorderedColumns
    });

    if (this.isConnected) {
      this.boardHub.reorderColumns(columnIds).catch(() => {
        this.loadBoard(projectId, boardId, false);
      });
    } else {
      this.columnService.reorderColumns(projectId, boardId, { columnIds }).subscribe({
        error: () => {
          this.loadBoard(projectId, boardId, false);
        }
      });
    }
  }

  addColumn(projectId: string, boardId: string, name: string): void {
    if (this.isConnected) {
      this.boardHub.addColumn(name).then(() => {
        this.loadBoard(projectId, boardId, false);
      }).catch(() => {
        this.toastService.error('Failed to add column');
      });
    } else {
      this.columnService.addColumn(projectId, boardId, { name }).subscribe({
        next: () => {
          this.loadBoard(projectId, boardId, false);
        },
        error: () => {
          this.toastService.error('Failed to add column');
        }
      });
    }
  }

  renameColumn(projectId: string, boardId: string, columnId: string, newName: string): void {
    if (this.isConnected) {
      this.boardHub.renameColumn(columnId, newName).then(() => {
        this.loadBoard(projectId, boardId, false);
      }).catch(() => {
        this.toastService.error('Failed to rename column');
      });
    } else {
      this.columnService.renameColumn(projectId, boardId, columnId, { newName }).subscribe({
        next: () => {
          this.loadBoard(projectId, boardId, false);
        },
        error: () => {
          this.toastService.error('Failed to rename column');
        }
      });
    }
  }

  deleteColumn(projectId: string, boardId: string, columnId: string): void {
    if (this.isConnected) {
      this.boardHub.deleteColumn(columnId).then(() => {
        this.loadBoard(projectId, boardId, false);
      }).catch((err) => {
        const errorMessage = err?.message || 'Failed to delete column';
        this.toastService.error(errorMessage);
      });
    } else {
      this.columnService.deleteColumn(projectId, boardId, columnId).subscribe({
        next: () => {
          this.loadBoard(projectId, boardId, false);
        },
        error: (err) => {
          const errorMessage = err.error?.title || err.error?.detail || 'Failed to delete column';
          this.toastService.error(errorMessage);
        }
      });
    }
  }

  moveCard(projectId: string, boardId: string, cardId: string, targetColumnId: string, newPosition: string): void {
    // Optimistic update: patch card position in signal so self-events don't revert CDK visual move
    this._board.update(board => {
      if (!board) return board;
      return {
        ...board,
        cards: board.cards.map(c =>
          c.id === cardId ? { ...c, columnId: targetColumnId, position: newPosition } : c
        )
      };
    });

    if (this.isConnected) {
      this.boardHub.moveCard(cardId, targetColumnId, newPosition).catch(() => {
        this.loadBoard(projectId, boardId, false);
      });
    } else {
      this.cardService.moveCard(projectId, boardId, cardId, {
        targetColumnId,
        newPosition
      }).subscribe({
        error: () => {
          this.loadBoard(projectId, boardId, false);
        }
      });
    }
  }

  moveColumn(projectId: string, boardId: string, columnId: string, newPosition: string): void {
    // Optimistic update: patch column position and sort by position so self-events don't revert CDK visual move
    this._board.update(board => {
      if (!board) return board;
      const updatedColumns = board.columns
        .map(col => col.id === columnId ? { ...col, position: newPosition } : col)
        .sort((a, b) => (a.position < b.position ? -1 : a.position > b.position ? 1 : 0));
      return { ...board, columns: updatedColumns };
    });

    if (this.isConnected) {
      this.boardHub.moveColumn(columnId, newPosition).catch(() => {
        this.loadBoard(projectId, boardId, false);
      });
    } else {
      this.columnService.moveColumn(projectId, boardId, columnId, { newPosition }).subscribe({
        error: () => {
          this.loadBoard(projectId, boardId, false);
        }
      });
    }
  }

  addCard(projectId: string, boardId: string, columnId: string, title: string): void {
    if (this.isConnected) {
      this.boardHub.addCard(columnId, title, '').then(() => {
        this.loadBoard(projectId, boardId, false);
      }).catch(() => {
        this.toastService.error('Failed to add card');
      });
    } else {
      this.cardService.addCard(projectId, boardId, {
        columnId,
        title,
        description: ''
      }).subscribe({
        next: () => {
          this.loadBoard(projectId, boardId, false);
        },
        error: () => {
          this.toastService.error('Failed to add card');
        }
      });
    }
  }

  openCardDrawer(card: Card, projectId: string, boardId: string): void {
    this.drawerService.openCardDrawer(card, projectId, boardId);
    this.cardService.getCardDetail(projectId, boardId, card.id).subscribe({
      next: (detail) => {
        this.drawerService.initialCardActivity.set(detail.activity);
      },
    });
  }

  toggleLabelFilter(labelId: string): void {
    this._selectedLabelIds.update(ids => {
      const next = new Set(ids);
      if (next.has(labelId)) {
        next.delete(labelId);
      } else {
        next.add(labelId);
      }
      return next;
    });
  }

  clearLabelFilter(): void {
    this._selectedLabelIds.set(new Set());
  }

  openArchivePanel(): void {
    this._showArchivePanel.set(true);
    this.loadArchivedCards();
  }

  closeArchivePanel(): void {
    this._showArchivePanel.set(false);
  }

  openArchivedCardInDrawer(card: Card): void {
    this.closeArchivePanel();
    this.drawerService.openCardDrawer(card, this._currentProjectId(), this._currentBoardId());
  }

  restoreArchivedCard(cardId: string): void {
    const projectId = this._currentProjectId();
    const boardId = this._currentBoardId();
    if (!projectId || !boardId) return;

    this.cardService.restoreCard(projectId, boardId, cardId).subscribe({
      next: () => {
        this._archivedCards.update(cards => cards.filter(c => c.id !== cardId));
        this.loadBoard(projectId, boardId, false);
        this.toastService.success('Card restored');
      },
      error: () => {
        this.toastService.error('Failed to restore card');
      }
    });
  }

  deleteArchivedCard(cardId: string): void {
    const projectId = this._currentProjectId();
    const boardId = this._currentBoardId();
    if (!projectId || !boardId) return;

    this.cardService.deleteCard(projectId, boardId, cardId).subscribe({
      next: () => {
        this._archivedCards.update(cards => cards.filter(c => c.id !== cardId));
        this.toastService.success('Card deleted');
      },
      error: () => {
        this.toastService.error('Failed to delete card');
      }
    });
  }

  openActivityPanel(): void {
    this._showActivityPanel.set(true);
    this._activityEntries.set([]);
    this._activityNextCursor.set(null);
    this.loadActivity();
  }

  closeActivityPanel(): void {
    this._showActivityPanel.set(false);
  }

  openCardFromActivity(cardId: string): void {
    const card = this._board()?.cards.find(c => c.id === cardId);
    if (card) {
      this.closeActivityPanel();
      this.drawerService.openCardDrawer(card, this._currentProjectId(), this._currentBoardId());
    }
  }

  loadMoreActivity(): void {
    if (this._activityNextCursor() && !this._isLoadingActivity()) {
      this.loadActivity();
    }
  }

  private loadActivity(): void {
    const projectId = this._currentProjectId();
    const boardId = this._currentBoardId();
    if (!projectId || !boardId) return;

    this._isLoadingActivity.set(true);
    this.boardService.getBoardActivity(projectId, boardId, 20, this._activityNextCursor() ?? undefined)
      .subscribe({
        next: (feed) => {
          this._activityEntries.update(prev => [...prev, ...feed.entries]);
          this._activityNextCursor.set(feed.nextCursor);
          this._isLoadingActivity.set(false);
        },
        error: () => this._isLoadingActivity.set(false),
      });
  }

  private loadArchivedCards(): void {
    const projectId = this._currentProjectId();
    const boardId = this._currentBoardId();
    if (!projectId || !boardId) return;

    this._isLoadingArchive.set(true);

    this.cardService.getArchivedCards(projectId, boardId).subscribe({
      next: (cards) => {
        this._archivedCards.set(cards);
        this._isLoadingArchive.set(false);
      },
      error: () => {
        this._isLoadingArchive.set(false);
      }
    });
  }

  private subscribeToBoardEvents(): void {
    this.boardHub.offAllEvents();

    const isSelf = (data: { actingUserId?: string }): boolean =>
      data.actingUserId === this.boardHub.currentUserId();

    const refetchIfNotSelf = (data: { actingUserId?: string }): void => {
      if (!isSelf(data)) {
        this.loadBoard(this._currentProjectId(), this._currentBoardId(), false);
      }
    };

    // --- Card events with in-place patching ---

    this.boardHub.onEvent<{
      cardId: string; sourceColumnId: string; targetColumnId: string;
      newPosition: string; actingUserId: string;
    }>('CardMoved', (data) => {
      if (isSelf(data)) return;
      this._board.update(board => {
        if (!board) return board;
        return {
          ...board,
          cards: board.cards.map(c =>
            c.id === data.cardId
              ? { ...c, columnId: data.targetColumnId, position: data.newPosition }
              : c
          )
        };
      });
    });

    this.boardHub.onEvent<{ cardId: string; title: string; description: string; actingUserId: string }>(
      'CardUpdated', (data) => {
        if (isSelf(data)) return;
        this._board.update(board => {
          if (!board) return board;
          return {
            ...board,
            cards: board.cards.map(c =>
              c.id === data.cardId ? { ...c, title: data.title, description: data.description } : c
            )
          };
        });
      }
    );

    this.boardHub.onEvent<{ cardId: string; actingUserId: string }>('CardDeleted', (data) => {
      if (isSelf(data)) return;
      this._board.update(board => {
        if (!board) return board;
        return { ...board, cards: board.cards.filter(c => c.id !== data.cardId) };
      });
    });

    this.boardHub.onEvent<{ cardId: string; actingUserId: string }>('CardArchived', (data) => {
      if (isSelf(data)) return;
      this._board.update(board => {
        if (!board) return board;
        return { ...board, cards: board.cards.filter(c => c.id !== data.cardId) };
      });
    });

    this.boardHub.onEvent<{ cardId: string; actingUserId: string }>('CardHeld', (data) => {
      if (isSelf(data)) return;
      this._board.update(board => {
        if (!board) return board;
        return {
          ...board,
          cards: board.cards.map(c =>
            c.id === data.cardId ? { ...c, holdAt: new Date().toISOString() } : c
          )
        };
      });
    });

    this.boardHub.onEvent<{ cardId: string; actingUserId: string }>('CardResumed', (data) => {
      if (isSelf(data)) return;
      this._board.update(board => {
        if (!board) return board;
        return {
          ...board,
          cards: board.cards.map(c =>
            c.id === data.cardId ? { ...c, holdAt: null } : c
          )
        };
      });
    });

    this.boardHub.onEvent<{
      cardId: string; labelId: string; labelName: string; labelColor: string; actingUserId: string;
    }>('CardLabelAdded', (data) => {
      if (isSelf(data)) return;
      this._board.update(board => {
        if (!board) return board;
        return {
          ...board,
          cards: board.cards.map(c =>
            c.id === data.cardId
              ? { ...c, labels: [...c.labels, { id: data.labelId, name: data.labelName, color: data.labelColor }] }
              : c
          )
        };
      });
    });

    this.boardHub.onEvent<{ cardId: string; labelId: string; actingUserId: string }>(
      'CardLabelRemoved', (data) => {
        if (isSelf(data)) return;
        this._board.update(board => {
          if (!board) return board;
          return {
            ...board,
            cards: board.cards.map(c =>
              c.id === data.cardId
                ? { ...c, labels: c.labels.filter(l => l.id !== data.labelId) }
                : c
            )
          };
        });
      }
    );

    // Cards created/restored need full object data — refetch
    this.boardHub.onEvent<{ actingUserId: string }>('CardCreated', refetchIfNotSelf);
    this.boardHub.onEvent<{ actingUserId: string }>('CardRestored', refetchIfNotSelf);

    // --- Column events with in-place patching ---

    this.boardHub.onEvent<{ columnId: string; name: string; position: string; actingUserId: string }>(
      'ColumnAdded', (data) => {
        if (isSelf(data)) return;
        this._board.update(board => {
          if (!board) return board;
          return {
            ...board,
            columns: [...board.columns, { id: data.columnId, name: data.name, position: data.position, boardId: board.id }]
          };
        });
      }
    );

    this.boardHub.onEvent<{ columnId: string; newName: string; actingUserId: string }>('ColumnRenamed', (data) => {
      if (isSelf(data)) return;
      this._board.update(board => {
        if (!board) return board;
        return {
          ...board,
          columns: board.columns.map(col =>
            col.id === data.columnId ? { ...col, name: data.newName } : col
          )
        };
      });
    });

    this.boardHub.onEvent<{ columnId: string; newPosition: string; actingUserId: string }>(
      'ColumnMoved', (data) => {
        if (isSelf(data)) return;
        this._board.update(board => {
          if (!board) return board;
          return {
            ...board,
            columns: board.columns
              .map(col => col.id === data.columnId ? { ...col, position: data.newPosition } : col)
              .sort((a, b) => (a.position < b.position ? -1 : a.position > b.position ? 1 : 0))
          };
        });
      }
    );

    this.boardHub.onEvent<{ newPositions: Record<string, string>; actingUserId: string }>(
      'ColumnsReordered', (data) => {
        if (isSelf(data)) return;
        this._board.update(board => {
          if (!board) return board;
          return {
            ...board,
            columns: board.columns
              .map(col =>
                data.newPositions[col.id] !== undefined
                  ? { ...col, position: data.newPositions[col.id] }
                  : col
              )
              .sort((a, b) => (a.position < b.position ? -1 : a.position > b.position ? 1 : 0))
          };
        });
      }
    );

    // Column deletion can move cards between columns — refetch
    this.boardHub.onEvent<{ actingUserId: string }>('ColumnDeleted', refetchIfNotSelf);

    // --- Label events with in-place patching ---

    this.boardHub.onEvent<{ labelId: string; name: string; color: string; actingUserId: string }>(
      'LabelCreated', (data) => {
        if (isSelf(data)) return;
        this._board.update(board => {
          if (!board) return board;
          return {
            ...board,
            labels: [...board.labels, { id: data.labelId, name: data.name, color: data.color }]
          };
        });
      }
    );

    this.boardHub.onEvent<{ labelId: string; name: string; color: string; actingUserId: string }>(
      'LabelUpdated', (data) => {
        if (isSelf(data)) return;
        this._board.update(board => {
          if (!board) return board;
          return {
            ...board,
            labels: board.labels.map(l =>
              l.id === data.labelId ? { ...l, name: data.name, color: data.color } : l
            ),
            cards: board.cards.map(c => ({
              ...c,
              labels: c.labels.map(l =>
                l.id === data.labelId ? { ...l, name: data.name, color: data.color } : l
              )
            }))
          };
        });
      }
    );

    this.boardHub.onEvent<{ labelId: string; actingUserId: string }>('LabelRemoved', (data) => {
      if (isSelf(data)) return;
      this._board.update(board => {
        if (!board) return board;
        return {
          ...board,
          labels: board.labels.filter(l => l.id !== data.labelId),
          cards: board.cards.map(c => ({
            ...c,
            labels: c.labels.filter(l => l.id !== data.labelId)
          }))
        };
      });
    });

    // --- Board-level events ---

    this.boardHub.onEvent<{ boardId: string; newName: string; actingUserId: string }>('BoardRenamed', (data) => {
      if (isSelf(data)) return;
      this._board.update(board => {
        if (!board) return board;
        return { ...board, name: data.newName };
      });
    });

    this.boardHub.onEvent<{ boardId: string; actingUserId: string }>('BoardDeleted', (data) => {
      if (!isSelf(data)) {
        this._board.set(null);
        this._error.set('This board has been deleted.');
      }
    });

    // Full refetch on reconnect to sync any missed events
    this.boardHub.onEvent('Reconnected', () => {
      this.loadBoard(this._currentProjectId(), this._currentBoardId(), false);
    });
  }
}
