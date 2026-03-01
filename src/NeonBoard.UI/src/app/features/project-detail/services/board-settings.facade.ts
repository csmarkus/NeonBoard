import { Injectable, inject, signal, computed } from '@angular/core';
import { Observable, EMPTY } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { BoardService } from './board.service';
import { LabelService } from './label.service';
import { ToastService } from '../../../core/services/toast.service';
import { Board } from '../models/board.model';
import { Label } from '../models/label.model';

@Injectable({
  providedIn: 'root'
})
export class BoardSettingsFacade {
  private boardService = inject(BoardService);
  private labelService = inject(LabelService);
  private toastService = inject(ToastService);

  private _boardName = signal<string>('');
  private _originalBoardName = signal<string>('');
  private _boardPrefix = signal<string>('');
  private _originalBoardPrefix = signal<string>('');
  private _boardLabels = signal<Label[]>([]);
  private _isLoading = signal<boolean>(false);
  private _isSaving = signal<boolean>(false);
  private _error = signal<string | null>(null);

  readonly boardName = this._boardName.asReadonly();
  readonly originalBoardName = this._originalBoardName.asReadonly();
  readonly boardPrefix = this._boardPrefix.asReadonly();
  readonly originalBoardPrefix = this._originalBoardPrefix.asReadonly();
  readonly boardLabels = this._boardLabels.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly isSaving = this._isSaving.asReadonly();
  readonly error = this._error.asReadonly();

  readonly hasChanges = computed(() => {
    return this._boardName().trim() !== this._originalBoardName() ||
      this._boardPrefix().trim() !== this._originalBoardPrefix();
  });

  readonly sortedLabels = computed(() => {
    return this._boardLabels().slice().sort((a, b) => a.name.localeCompare(b.name));
  });

  resetState(): void {
    this._boardName.set('');
    this._originalBoardName.set('');
    this._boardPrefix.set('');
    this._originalBoardPrefix.set('');
    this._boardLabels.set([]);
    this._isLoading.set(true);
    this._isSaving.set(false);
    this._error.set(null);
  }

  loadBoardSettings(projectId: string, boardId: string): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.boardService.getBoardDetails(projectId, boardId).subscribe({
      next: (board) => {
        this._boardName.set(board.name);
        this._originalBoardName.set(board.name);
        this._boardPrefix.set(board.prefix);
        this._originalBoardPrefix.set(board.prefix);
        this._boardLabels.set(board.labels ?? []);
        this._isLoading.set(false);
      },
      error: () => {
        this._error.set('Failed to load board settings');
        this._isLoading.set(false);
      }
    });
  }

  updateBoardName(name: string): void {
    this._boardName.set(name);
  }

  updateBoardPrefix(prefix: string): void {
    this._boardPrefix.set(prefix.toUpperCase());
  }

  saveBoardSettings(projectId: string, boardId: string): Observable<Board> {
    const name = this._boardName().trim();
    const prefix = this._boardPrefix().trim();
    if (!name || !this.hasChanges() || this._isSaving()) return EMPTY;

    this._isSaving.set(true);

    return this.boardService.updateBoardSettings(projectId, boardId, { name, prefix: prefix || undefined }).pipe(
      tap((board) => {
        this._originalBoardName.set(board.name);
        this._boardName.set(board.name);
        this._originalBoardPrefix.set(board.prefix);
        this._boardPrefix.set(board.prefix);
        this._isSaving.set(false);
        this.toastService.success('Board settings saved');
      }),
      catchError(() => {
        this._isSaving.set(false);
        this.toastService.error('Failed to save board settings');
        return EMPTY;
      })
    );
  }

  deleteBoard(projectId: string, boardId: string): Observable<void> {
    return this.boardService.deleteBoard(projectId, boardId);
  }

  addLabel(projectId: string, boardId: string, name: string, color: string): void {
    this.labelService.addLabel(projectId, boardId, { name, color }).subscribe({
      next: (label) => {
        this._boardLabels.update(labels => [...labels, label]);
      },
      error: () => {
        this.toastService.error('Failed to add label');
      }
    });
  }

  updateLabel(projectId: string, boardId: string, labelId: string, name: string, color: string): void {
    this.labelService.updateLabel(projectId, boardId, labelId, { name, color }).subscribe({
      next: () => {
        this._boardLabels.update(labels =>
          labels.map(l => l.id === labelId ? { ...l, name, color } : l)
        );
      },
      error: () => {
        this.toastService.error('Failed to update label');
      }
    });
  }

  deleteLabel(projectId: string, boardId: string, labelId: string): void {
    this.labelService.removeLabel(projectId, boardId, labelId).subscribe({
      next: () => {
        this._boardLabels.update(labels => labels.filter(l => l.id !== labelId));
      },
      error: () => {
        this.toastService.error('Failed to delete label');
      }
    });
  }
}
