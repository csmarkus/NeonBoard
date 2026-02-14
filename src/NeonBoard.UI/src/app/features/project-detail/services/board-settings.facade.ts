import { Injectable, inject, signal, computed } from '@angular/core';
import { Observable } from 'rxjs';
import { BoardService } from './board.service';
import { LabelService } from './label.service';
import { Label } from '../models/label.model';

@Injectable({
  providedIn: 'root'
})
export class BoardSettingsFacade {
  private boardService = inject(BoardService);
  private labelService = inject(LabelService);

  private _boardName = signal<string>('');
  private _originalBoardName = signal<string>('');
  private _boardLabels = signal<Label[]>([]);
  private _isLoading = signal<boolean>(false);
  private _isSaving = signal<boolean>(false);
  private _error = signal<string | null>(null);

  readonly boardName = this._boardName.asReadonly();
  readonly originalBoardName = this._originalBoardName.asReadonly();
  readonly boardLabels = this._boardLabels.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly isSaving = this._isSaving.asReadonly();
  readonly error = this._error.asReadonly();

  readonly hasChanges = computed(() => {
    return this._boardName().trim() !== this._originalBoardName();
  });

  readonly sortedLabels = computed(() => {
    return this._boardLabels().slice().sort((a, b) => a.name.localeCompare(b.name));
  });

  loadBoardSettings(projectId: string, boardId: string): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.boardService.getBoardDetails(projectId, boardId).subscribe({
      next: (board) => {
        this._boardName.set(board.name);
        this._originalBoardName.set(board.name);
        this._boardLabels.set(board.labels ?? []);
        this._isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading board settings:', err);
        this._error.set('Failed to load board settings');
        this._isLoading.set(false);
      }
    });
  }

  updateBoardName(name: string): void {
    this._boardName.set(name);
  }

  saveBoardSettings(projectId: string, boardId: string): void {
    const name = this._boardName().trim();
    if (!name || !this.hasChanges() || this._isSaving()) return;

    this._isSaving.set(true);

    this.boardService.updateBoardSettings(projectId, boardId, { name }).subscribe({
      next: (board) => {
        this._originalBoardName.set(board.name);
        this._boardName.set(board.name);
        this._isSaving.set(false);
      },
      error: (err) => {
        console.error('Error saving board settings:', err);
        this._isSaving.set(false);
      }
    });
  }

  deleteBoard(projectId: string, boardId: string): Observable<void> {
    return this.boardService.deleteBoard(projectId, boardId);
  }

  addLabel(projectId: string, boardId: string, name: string, color: string): void {
    this.labelService.addLabel(projectId, boardId, { name, color }).subscribe({
      next: (label) => {
        this._boardLabels.update(labels => [...labels, label]);
      },
      error: (err) => {
        console.error('Error adding label:', err);
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
      error: (err) => {
        console.error('Error updating label:', err);
      }
    });
  }

  deleteLabel(projectId: string, boardId: string, labelId: string): void {
    this.labelService.removeLabel(projectId, boardId, labelId).subscribe({
      next: () => {
        this._boardLabels.update(labels => labels.filter(l => l.id !== labelId));
      },
      error: (err) => {
        console.error('Error deleting label:', err);
      }
    });
  }
}
