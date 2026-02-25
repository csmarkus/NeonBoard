import { Component, input, output, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { DrawerComponent } from '../../../../../shared/components/drawer/drawer.component';
import { ButtonComponent } from '../../../../../shared/components/button/button.component';
import { ErrorBannerComponent } from '../../../../../shared/components/error-banner/error-banner.component';
import { InputComponent } from '../../../../../shared/components/input/input.component';
import { BoardService } from '../../../services/board.service';
import { Board, CreateBoardRequest } from '../../../models/board.model';

@Component({
  selector: 'app-create-board-drawer',
  imports: [DrawerComponent, ButtonComponent, ErrorBannerComponent, InputComponent],
  templateUrl: './create-board-drawer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateBoardDrawerComponent {
  open = input.required<boolean>();
  projectId = input.required<string>();
  close = output<void>();
  boardCreated = output<Board>();

  private boardService = inject(BoardService);

  newBoardName = signal('');
  newBoardPrefix = signal('');
  error = signal<string | null>(null);
  isCreating = signal(false);

  onClose(): void {
    this.resetForm();
    this.close.emit();
  }

  createBoard(): void {
    if (!this.newBoardName().trim()) return;

    this.isCreating.set(true);
    this.error.set(null);

    const request: CreateBoardRequest = {
      name: this.newBoardName(),
      ...(this.newBoardPrefix().trim() && { prefix: this.newBoardPrefix().trim().toUpperCase() })
    };

    this.boardService.createBoard(this.projectId(), request).subscribe({
      next: (board) => {
        this.boardCreated.emit(board);
        this.resetForm();
        this.isCreating.set(false);
        this.close.emit();
      },
      error: (err) => {
        console.error('Error creating board:', err);
        this.error.set('Failed to create board. Please try again.');
        this.isCreating.set(false);
      }
    });
  }

  private resetForm(): void {
    this.newBoardName.set('');
    this.newBoardPrefix.set('');
    this.error.set(null);
  }
}
