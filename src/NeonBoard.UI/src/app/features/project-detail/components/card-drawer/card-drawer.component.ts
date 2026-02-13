import { Component, input, output, inject, signal, effect, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { CardService } from '../../services/card.service';
import { DrawerService } from '../../services/drawer.service';
import { Card } from '../../models/card.model';
import { getLabelColorClasses } from '../../models/label.model';

@Component({
  selector: 'app-card-drawer',
  imports: [CommonModule, FormsModule, DrawerComponent, ButtonComponent],
  templateUrl: './card-drawer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CardDrawerComponent {
  open = input.required<boolean>();
  projectId = input.required<string>();
  boardId = input.required<string>();
  columnId = input<string | null>(null);
  card = input<Card | null>(null);

  close = output<void>();
  cardSaved = output<void>();
  cardDeleted = output<void>();

  private cardService = inject(CardService);
  protected drawerService = inject(DrawerService);

  cardTitle = signal('');
  cardDescription = signal('');
  originalTitle = signal('');
  originalDescription = signal('');
  error = signal<string | null>(null);
  isSaving = signal(false);
  isDeleting = signal(false);
  showLabelPicker = signal(false);
  togglingLabelId = signal<string | null>(null);

  cardLabelIds = signal<string[]>([]);

  constructor() {
    effect(() => {
      const existingCard = this.card();
      if (existingCard) {
        this.cardTitle.set(existingCard.title);
        this.cardDescription.set(existingCard.description);
        this.originalTitle.set(existingCard.title);
        this.originalDescription.set(existingCard.description);
        this.cardLabelIds.set(existingCard.labels?.map(l => l.id) ?? []);
      } else {
        this.cardTitle.set('');
        this.cardDescription.set('');
        this.originalTitle.set('');
        this.originalDescription.set('');
        this.cardLabelIds.set([]);
      }
      this.showLabelPicker.set(false);
    });
  }

  isEditMode = computed(() => this.card() !== null);

  drawerTitle = computed(() => this.isEditMode() ? 'Card Details' : 'Add Card');

  descriptionChanged = computed(() =>
    this.cardDescription() !== this.originalDescription()
  );

  assignedLabels = computed(() => {
    const ids = this.cardLabelIds();
    const allLabels = this.drawerService.boardLabels();
    return allLabels.filter(l => ids.includes(l.id));
  });

  isLabelAssigned(labelId: string): boolean {
    return this.cardLabelIds().includes(labelId);
  }

  getLabelClasses(color: string): string {
    const classes = getLabelColorClasses(color);
    return `${classes.bg} ${classes.text} ${classes.border}`;
  }

  onClose(): void {
    this.resetForm();
    this.close.emit();
  }

  saveTitle(): void {
    if (!this.isEditMode()) return;
    const title = this.cardTitle().trim();
    if (!title || title === this.originalTitle()) return;

    const cardId = this.card()!.id;
    this.cardService.updateCard(
      this.projectId(), this.boardId(), cardId,
      { title, description: this.originalDescription() }
    ).subscribe({
      next: () => {
        this.originalTitle.set(title);
        this.cardSaved.emit();
      },
      error: () => {
        this.error.set('Failed to update title.');
      }
    });
  }

  saveDescription(): void {
    if (!this.isEditMode() || !this.descriptionChanged()) return;

    this.isSaving.set(true);
    const cardId = this.card()!.id;
    this.cardService.updateCard(
      this.projectId(), this.boardId(), cardId,
      { title: this.originalTitle(), description: this.cardDescription() }
    ).subscribe({
      next: () => {
        this.originalDescription.set(this.cardDescription());
        this.isSaving.set(false);
        this.cardSaved.emit();
      },
      error: () => {
        this.error.set('Failed to update description.');
        this.isSaving.set(false);
      }
    });
  }

  addCard(): void {
    if (!this.cardTitle().trim()) return;

    const targetColumnId = this.columnId();
    if (!targetColumnId) {
      this.error.set('Column ID is required');
      return;
    }

    this.isSaving.set(true);
    this.error.set(null);

    this.cardService.addCard(
      this.projectId(), this.boardId(),
      { columnId: targetColumnId, title: this.cardTitle(), description: this.cardDescription() }
    ).subscribe({
      next: () => {
        this.cardSaved.emit();
        this.resetForm();
        this.isSaving.set(false);
        this.close.emit();
      },
      error: () => {
        this.error.set('Failed to add card. Please try again.');
        this.isSaving.set(false);
      }
    });
  }

  toggleLabel(labelId: string): void {
    if (!this.isEditMode() || this.togglingLabelId()) return;

    const cardId = this.card()!.id;
    const isAssigned = this.isLabelAssigned(labelId);
    this.togglingLabelId.set(labelId);

    if (isAssigned) {
      this.cardLabelIds.update(ids => ids.filter(id => id !== labelId));
      this.cardService.removeCardLabel(this.projectId(), this.boardId(), cardId, labelId).subscribe({
        next: () => {
          this.togglingLabelId.set(null);
          this.cardSaved.emit();
        },
        error: () => {
          this.cardLabelIds.update(ids => [...ids, labelId]);
          this.togglingLabelId.set(null);
          this.error.set('Failed to remove label.');
        }
      });
    } else {
      this.cardLabelIds.update(ids => [...ids, labelId]);
      this.cardService.addCardLabel(this.projectId(), this.boardId(), cardId, labelId).subscribe({
        next: () => {
          this.togglingLabelId.set(null);
          this.cardSaved.emit();
        },
        error: () => {
          this.cardLabelIds.update(ids => ids.filter(id => id !== labelId));
          this.togglingLabelId.set(null);
          this.error.set('Failed to add label.');
        }
      });
    }
  }

  toggleLabelPicker(): void {
    this.showLabelPicker.update(v => !v);
  }

  deleteCard(): void {
    if (!this.isEditMode()) return;
    if (!confirm('Are you sure you want to delete this card?')) return;

    this.isDeleting.set(true);
    this.error.set(null);

    const cardId = this.card()!.id;
    this.cardService.deleteCard(this.projectId(), this.boardId(), cardId).subscribe({
      next: () => {
        this.cardDeleted.emit();
        this.resetForm();
        this.isDeleting.set(false);
        this.close.emit();
      },
      error: () => {
        this.error.set('Failed to delete card. Please try again.');
        this.isDeleting.set(false);
      }
    });
  }

  private resetForm(): void {
    this.cardTitle.set('');
    this.cardDescription.set('');
    this.originalTitle.set('');
    this.originalDescription.set('');
    this.error.set(null);
    this.showLabelPicker.set(false);
    this.cardLabelIds.set([]);
  }
}
