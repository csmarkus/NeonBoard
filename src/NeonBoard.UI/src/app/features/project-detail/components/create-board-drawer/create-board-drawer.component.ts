import { Component, input, output, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { BoardService } from '../../services/board.service';
import { Board } from '../../models/board.model';

@Component({
  selector: 'app-create-board-drawer',
  standalone: true,
  imports: [CommonModule, FormsModule, DrawerComponent, ButtonComponent],
  templateUrl: './create-board-drawer.component.html',
})
export class CreateBoardDrawerComponent {
  open = input.required<boolean>();
  projectId = input.required<string>();
  close = output<void>();
  boardCreated = output<Board>();

  private boardService = inject(BoardService);
  private cdr = inject(ChangeDetectorRef);

  newBoardName = '';
  error: string | null = null;
  isCreating = false;

  onClose(): void {
    this.resetForm();
    this.close.emit();
  }

  createBoard(): void {
    if (!this.newBoardName.trim()) return;

    this.isCreating = true;
    this.error = null;

    this.boardService.createBoard(this.projectId(), {
      name: this.newBoardName
    }).subscribe({
      next: (board) => {
        this.boardCreated.emit(board);
        this.resetForm();
        this.isCreating = false;
        this.close.emit();
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error creating board:', err);
        this.error = 'Failed to create board. Please try again.';
        this.isCreating = false;
        this.cdr.detectChanges();
      }
    });
  }

  private resetForm(): void {
    this.newBoardName = '';
    this.error = null;
  }
}
