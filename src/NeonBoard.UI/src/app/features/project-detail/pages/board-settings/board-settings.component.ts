import { Component, inject, signal, computed, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Observable, Subject } from 'rxjs';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { HasUnsavedChanges } from '../../../../core/guards/unsaved-changes.guard';
import { BoardService } from '../../services/board.service';
import { ProjectService } from '../../../projects/services/project.service';

@Component({
  selector: 'app-board-settings',
  imports: [
    RouterLink,
    ButtonComponent,
    ConfirmationModalComponent,
  ],
  host: {
    class: 'flex flex-col h-full'
  },
  templateUrl: './board-settings.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BoardSettingsComponent implements OnInit, HasUnsavedChanges {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private boardService = inject(BoardService);
  private projectService = inject(ProjectService);

  projectId = signal('');
  boardId = signal('');
  projectName = signal('');
  boardName = signal('');
  originalName = signal('');
  isLoading = signal(true);
  isSaving = signal(false);
  isDeleting = signal(false);
  showDeleteModal = signal(false);
  showDiscardModal = signal(false);

  hasChanges = computed(() => this.boardName().trim() !== this.originalName());

  private discardSubject: Subject<boolean> | null = null;

  ngOnInit(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (projectId) {
      this.projectId.set(projectId);
      this.projectService.getProject(projectId).subscribe({
        next: (project) => this.projectName.set(project.name),
      });
    }

    const boardId = this.route.snapshot.paramMap.get('boardId');
    if (boardId) {
      this.boardId.set(boardId);
      this.loadBoard();
    }
  }

  hasUnsavedChanges(): boolean {
    return this.hasChanges();
  }

  confirmDiscard(): Observable<boolean> {
    this.discardSubject = new Subject<boolean>();
    this.showDiscardModal.set(true);
    return this.discardSubject.asObservable();
  }

  onConfirmDiscard(): void {
    this.showDiscardModal.set(false);
    this.discardSubject?.next(true);
    this.discardSubject?.complete();
    this.discardSubject = null;
  }

  onCancelDiscard(): void {
    this.showDiscardModal.set(false);
    this.discardSubject?.next(false);
    this.discardSubject?.complete();
    this.discardSubject = null;
  }

  private loadBoard(): void {
    this.boardService.getBoardDetails(this.projectId(), this.boardId()).subscribe({
      next: (board) => {
        this.boardName.set(board.name);
        this.originalName.set(board.name);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  onNameInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.boardName.set(input.value);
  }

  saveChanges(): void {
    const name = this.boardName().trim();
    if (!name || !this.hasChanges() || this.isSaving()) return;

    this.isSaving.set(true);
    this.boardService.renameBoard(this.projectId(), this.boardId(), { name }).subscribe({
      next: (board) => {
        this.originalName.set(board.name);
        this.boardName.set(board.name);
        this.isSaving.set(false);
      },
      error: () => {
        this.isSaving.set(false);
      }
    });
  }

  openDeleteModal(): void {
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
  }

  confirmDelete(): void {
    this.isDeleting.set(true);
    this.showDeleteModal.set(false);

    this.boardService.deleteBoard(this.projectId(), this.boardId()).subscribe({
      next: () => {
        this.router.navigate(['/project', this.projectId()]);
      },
      error: () => {
        this.isDeleting.set(false);
      }
    });
  }
}
