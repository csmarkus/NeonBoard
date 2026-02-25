import { Component, input, output, inject, signal, effect, computed, ChangeDetectionStrategy } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { DrawerComponent } from '../../../../../shared/components/drawer/drawer.component';
import { ButtonComponent } from '../../../../../shared/components/button/button.component';
import { ErrorBannerComponent } from '../../../../../shared/components/error-banner/error-banner.component';
import { InputComponent } from '../../../../../shared/components/input/input.component';
import { CardLabelPickerComponent } from './card-label-picker/card-label-picker.component';
import { CardActionsComponent } from './card-actions/card-actions.component';
import { CardService } from '../../../services/card.service';
import { DrawerService } from '../../../services/drawer.service';
import { ModalService } from '../../../../../core/services/modal.service';
import { Card } from '../../../models/card.model';

@Component({
  selector: 'app-card-drawer',
  imports: [FormField, DrawerComponent, ButtonComponent, ErrorBannerComponent, InputComponent, CardLabelPickerComponent, CardActionsComponent],
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
  private modalService = inject(ModalService);

  cardModel = signal({ title: '', description: '' });
  cardForm = form(this.cardModel, (f) => {
    required(f.title, { message: 'Card title is required' });
  });

  // Track the last-saved values for auto-save comparison
  savedTitle = signal('');
  savedDescription = signal('');

  error = signal<string | null>(null);
  isSaving = signal(false);
  isDeleting = signal(false);
  isArchiving = signal(false);
  showLabelPicker = signal(false);
  togglingLabelId = signal<string | null>(null);

  cardLabelIds = signal<string[]>([]);

  constructor() {
    effect(() => {
      const existingCard = this.card();
      if (existingCard) {
        this.cardModel.set({ title: existingCard.title, description: existingCard.description });
        this.savedTitle.set(existingCard.title);
        this.savedDescription.set(existingCard.description);
        this.cardLabelIds.set(existingCard.labels?.map(l => l.id) ?? []);
      } else {
        this.cardModel.set({ title: '', description: '' });
        this.savedTitle.set('');
        this.savedDescription.set('');
        this.cardLabelIds.set([]);
      }
      this.showLabelPicker.set(false);
    });
  }

  isEditMode = computed(() => this.card() !== null);
  drawerTitle = computed(() => this.isEditMode() ? 'Card Details' : 'Add Card');
  descriptionChanged = computed(() =>
    this.cardModel().description !== this.savedDescription()
  );

  onClose(): void {
    this.resetForm();
    this.close.emit();
  }

  saveTitle(): void {
    if (!this.isEditMode()) return;
    const title = this.cardModel().title.trim();
    if (!title || title === this.savedTitle()) return;

    const cardId = this.card()!.id;
    this.cardService.updateCard(
      this.projectId(), this.boardId(), cardId,
      { title, description: this.savedDescription() }
    ).subscribe({
      next: () => {
        this.savedTitle.set(title);
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
    const description = this.cardModel().description;
    this.cardService.updateCard(
      this.projectId(), this.boardId(), cardId,
      { title: this.savedTitle(), description }
    ).subscribe({
      next: () => {
        this.savedDescription.set(description);
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
    if (this.cardForm().invalid()) return;

    const targetColumnId = this.columnId();
    if (!targetColumnId) {
      this.error.set('Column ID is required');
      return;
    }

    this.isSaving.set(true);
    this.error.set(null);

    const { title, description } = this.cardModel();
    this.cardService.addCard(
      this.projectId(), this.boardId(),
      { columnId: targetColumnId, title: title.trim(), description }
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
    const isAssigned = this.cardLabelIds().includes(labelId);
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

  async requestDeleteCard(): Promise<void> {
    if (!this.isEditMode()) return;

    const confirmed = await this.modalService.confirm({
      title: 'Delete Card',
      message: 'Are you sure you want to delete this card? This action cannot be undone.',
      confirmText: 'Delete',
    });
    if (confirmed) {
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
  }

  archiveCard(): void {
    if (!this.isEditMode() || this.isArchiving()) return;

    this.isArchiving.set(true);
    this.error.set(null);

    const cardId = this.card()!.id;
    this.cardService.archiveCard(this.projectId(), this.boardId(), cardId).subscribe({
      next: (updatedCard) => {
        this.drawerService.selectedCard.set(updatedCard);
        this.isArchiving.set(false);
        this.drawerService.notifyCardArchived();
      },
      error: () => {
        this.error.set('Failed to archive card. Please try again.');
        this.isArchiving.set(false);
      }
    });
  }

  restoreCard(): void {
    if (!this.isEditMode() || this.isArchiving()) return;

    this.isArchiving.set(true);
    this.error.set(null);

    const cardId = this.card()!.id;
    this.cardService.restoreCard(this.projectId(), this.boardId(), cardId).subscribe({
      next: (updatedCard) => {
        this.drawerService.selectedCard.set(updatedCard);
        this.isArchiving.set(false);
        this.drawerService.notifyCardArchived();
      },
      error: () => {
        this.error.set('Failed to restore card. Please try again.');
        this.isArchiving.set(false);
      }
    });
  }

  private resetForm(): void {
    this.cardModel.set({ title: '', description: '' });
    this.savedTitle.set('');
    this.savedDescription.set('');
    this.error.set(null);
    this.showLabelPicker.set(false);
    this.cardLabelIds.set([]);
  }
}
