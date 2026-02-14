import { Injectable, inject, signal, computed } from '@angular/core';
import { BoardService } from './board.service';
import { ColumnService } from './column.service';
import { CardService } from './card.service';
import { DrawerService } from './drawer.service';
import { BoardDetails } from '../models/board.model';
import { Column } from '../models/column.model';
import { Card } from '../models/card.model';
import { Label } from '../models/label.model';

@Injectable({
  providedIn: 'root'
})
export class BoardStateFacade {
  private boardService = inject(BoardService);
  private columnService = inject(ColumnService);
  private cardService = inject(CardService);
  private drawerService = inject(DrawerService);

  private _board = signal<BoardDetails | null>(null);
  private _isLoading = signal<boolean>(false);
  private _error = signal<string | null>(null);
  private _currentProjectId = signal<string>('');
  private _currentBoardId = signal<string>('');

  readonly board = this._board.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();

  readonly columns = computed(() => this._board()?.columns ?? []);
  readonly labels = computed(() => this._board()?.labels ?? []);

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

  constructor() {
    this.drawerService.cardUpdated$.subscribe(() => {
      if (this._currentProjectId() && this._currentBoardId()) {
        this.loadBoard(this._currentProjectId(), this._currentBoardId(), false);
      }
    });

    this.drawerService.cardDeleted$.subscribe(() => {
      if (this._currentProjectId() && this._currentBoardId()) {
        this.loadBoard(this._currentProjectId(), this._currentBoardId(), false);
      }
    });
  }

  loadBoard(projectId: string, boardId: string, showLoading = true): void {
    this._currentProjectId.set(projectId);
    this._currentBoardId.set(boardId);

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
}
