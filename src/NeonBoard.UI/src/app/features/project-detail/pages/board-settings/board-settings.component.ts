import { Component, inject, signal, computed, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Observable, Subject } from 'rxjs';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { HasUnsavedChanges } from '../../../../core/guards/unsaved-changes.guard';
import { BoardService } from '../../services/board.service';
import { LabelService } from '../../services/label.service';
import { ProjectService } from '../../../projects/services/project.service';
import { Label, LABEL_COLORS, getLabelColorClasses } from '../../models/label.model';

@Component({
  selector: 'app-board-settings',
  imports: [
    RouterLink,
    FormsModule,
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
  private labelService = inject(LabelService);
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

  // Labels
  boardLabels = signal<Label[]>([]);
  newLabelName = signal('');
  newLabelColor = signal<string>(LABEL_COLORS[0]);
  isAddingLabel = signal(false);
  editingLabelId = signal<string | null>(null);
  editingLabelName = signal('');
  editingLabelColor = signal<string>('');
  isSavingLabel = signal(false);
  showDeleteLabelModal = signal(false);
  deletingLabelId = signal<string | null>(null);

  labelColors = LABEL_COLORS;

  hasChanges = computed(() => {
    const nameChanged = this.boardName().trim() !== this.originalName();
    return nameChanged;
  });

  sortedBoardLabels = computed(() => {
    return this.boardLabels().slice().sort((a, b) => a.name.localeCompare(b.name));
  });

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
        this.boardLabels.set(board.labels ?? []);
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
    this.boardService.updateBoardSettings(this.projectId(), this.boardId(), { name }).subscribe({
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

  // Label management
  getLabelClasses(color: string): string {
    const classes = getLabelColorClasses(color);
    return `${classes.bg} ${classes.text} ${classes.border}`;
  }

  getColorSwatchClass(color: string): string {
    const map: Record<string, string> = {
      red: 'bg-red-500',
      pink: 'bg-pink-500',
      purple: 'bg-purple-500',
      cyan: 'bg-cyan-500',
      blue: 'bg-blue-500',
      magenta: 'bg-fuchsia-500',
      violet: 'bg-violet-500',
      lime: 'bg-lime-500',
    };
    return map[color] ?? 'bg-gray-500';
  }

  addLabel(): void {
    const name = this.newLabelName().trim();
    if (!name || this.isAddingLabel()) return;

    this.isAddingLabel.set(true);
    this.labelService.addLabel(this.projectId(), this.boardId(), {
      name,
      color: this.newLabelColor()
    }).subscribe({
      next: (label) => {
        this.boardLabels.update(labels => [...labels, label]);
        this.newLabelName.set('');
        this.newLabelColor.set(LABEL_COLORS[0]);
        this.isAddingLabel.set(false);
      },
      error: () => {
        this.isAddingLabel.set(false);
      }
    });
  }

  startEditLabel(label: Label): void {
    this.editingLabelId.set(label.id);
    this.editingLabelName.set(label.name);
    this.editingLabelColor.set(label.color);
  }

  cancelEditLabel(): void {
    this.editingLabelId.set(null);
    this.editingLabelName.set('');
    this.editingLabelColor.set('');
  }

  saveEditLabel(): void {
    const labelId = this.editingLabelId();
    const name = this.editingLabelName().trim();
    if (!labelId || !name || this.isSavingLabel()) return;

    this.isSavingLabel.set(true);
    this.labelService.updateLabel(this.projectId(), this.boardId(), labelId, {
      name,
      color: this.editingLabelColor()
    }).subscribe({
      next: () => {
        this.boardLabels.update(labels =>
          labels.map(l => l.id === labelId ? { ...l, name, color: this.editingLabelColor() } : l)
        );
        this.cancelEditLabel();
        this.isSavingLabel.set(false);
      },
      error: () => {
        this.isSavingLabel.set(false);
      }
    });
  }

  openDeleteLabelModal(labelId: string): void {
    this.deletingLabelId.set(labelId);
    this.showDeleteLabelModal.set(true);
  }

  closeDeleteLabelModal(): void {
    this.showDeleteLabelModal.set(false);
    this.deletingLabelId.set(null);
  }

  confirmDeleteLabel(): void {
    const labelId = this.deletingLabelId();
    if (!labelId) return;

    this.showDeleteLabelModal.set(false);
    this.labelService.removeLabel(this.projectId(), this.boardId(), labelId).subscribe({
      next: () => {
        this.boardLabels.update(labels => labels.filter(l => l.id !== labelId));
        this.deletingLabelId.set(null);
      },
      error: () => {
        this.deletingLabelId.set(null);
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
