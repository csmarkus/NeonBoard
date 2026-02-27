import { Injectable, inject, signal, computed } from '@angular/core';
import { BoardService } from './board.service';
import { ColumnService } from './column.service';
import { CardService } from './card.service';
import { DrawerService } from './drawer.service';
import { ActivityService } from './activity.service';
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
  private activityService = inject(ActivityService);
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
      result[columnId].sort((a, b) => a.position - b.position);
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

  loadBoard(projectId: string, boardId: string, showLoading = true): void {
    const boardChanged = this._currentBoardId() !== boardId;
    this._currentProjectId.set(projectId);
    this._currentBoardId.set(boardId);
    if (boardChanged) {
      this._selectedLabelIds.set(new Set());
      this._showArchivePanel.set(false);
      this._archivedCards.set([]);
      this._isLoadingArchive.set(false);
      this._showActivityPanel.set(false);
      this._activityEntries.set([]);
      this._activityNextCursor.set(null);
      this._isLoadingActivity.set(false);
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
      error: (err) => {
        console.error('Error loading board:', err);
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

    this.columnService.reorderColumns(projectId, boardId, { columnIds }).subscribe({
      error: (err) => {
        console.error('Error reordering columns:', err);
        this.loadBoard(projectId, boardId, false);
      }
    });
  }

  addColumn(projectId: string, boardId: string, name: string): void {
    this.columnService.addColumn(projectId, boardId, { name }).subscribe({
      next: () => {
        this.loadBoard(projectId, boardId, false);
      },
      error: (err) => {
        console.error('Error adding column:', err);
      }
    });
  }

  renameColumn(projectId: string, boardId: string, columnId: string, newName: string): void {
    this.columnService.renameColumn(projectId, boardId, columnId, { newName }).subscribe({
      next: () => {
        this.loadBoard(projectId, boardId, false);
      },
      error: (err) => {
        console.error('Error renaming column:', err);
      }
    });
  }

  deleteColumn(projectId: string, boardId: string, columnId: string): void {
    this.columnService.deleteColumn(projectId, boardId, columnId).subscribe({
      next: () => {
        this.loadBoard(projectId, boardId, false);
      },
      error: (err) => {
        console.error('Error deleting column:', err);
        const errorMessage = err.error?.title || err.error?.detail || 'Failed to delete column';
        alert(errorMessage);
      }
    });
  }

  moveCard(projectId: string, boardId: string, cardId: string, targetColumnId: string, targetPosition: number): void {
    this.cardService.moveCard(projectId, boardId, cardId, {
      targetColumnId,
      targetPosition
    }).subscribe({
      error: (err) => {
        console.error('Error moving card:', err);
        this.loadBoard(projectId, boardId, false);
      }
    });
  }

  addCard(projectId: string, boardId: string, columnId: string, title: string): void {
    this.cardService.addCard(projectId, boardId, {
      columnId,
      title,
      description: ''
    }).subscribe({
      next: () => {
        this.loadBoard(projectId, boardId, false);
      },
      error: (err) => {
        console.error('Error adding card:', err);
      }
    });
  }

  openCardDrawer(card: Card, projectId: string, boardId: string): void {
    this.drawerService.openCardDrawer(card, projectId, boardId);
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
      error: (err) => {
        console.error('Error restoring card:', err);
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
      error: (err) => {
        console.error('Error deleting card:', err);
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
    this.activityService.getBoardActivity(projectId, boardId, 20, this._activityNextCursor() ?? undefined)
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
      error: (err) => {
        console.error('Error loading archived cards:', err);
        this._isLoadingArchive.set(false);
      }
    });
  }
}
