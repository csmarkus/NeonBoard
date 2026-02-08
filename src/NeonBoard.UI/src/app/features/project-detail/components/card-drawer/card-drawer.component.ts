import { Component, input, output, inject, ChangeDetectorRef, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { CardService } from '../../services/card.service';
import { Card } from '../../models/card.model';

@Component({
  selector: 'app-card-drawer',
  standalone: true,
  imports: [CommonModule, FormsModule, DrawerComponent, ButtonComponent],
  templateUrl: './card-drawer.component.html',
})
export class CardDrawerComponent {
  open = input.required<boolean>();
  projectId = input.required<string>();
  boardId = input.required<string>();
  columnId = input<string | null>(null);
  card = input<Card | null>(null); // If provided, edit mode; otherwise add mode

  close = output<void>();
  cardSaved = output<void>();
  cardDeleted = output<void>();

  private cardService = inject(CardService);
  private cdr = inject(ChangeDetectorRef);

  cardTitle = '';
  cardDescription = '';
  error: string | null = null;
  isSaving = false;
  isDeleting = false;

  constructor() {
    // Reset form when card input changes
    effect(() => {
      const existingCard = this.card();
      if (existingCard) {
        this.cardTitle = existingCard.title;
        this.cardDescription = existingCard.description;
      } else {
        this.cardTitle = '';
        this.cardDescription = '';
      }
    });
  }

  get isEditMode(): boolean {
    return this.card() !== null;
  }

  get drawerTitle(): string {
    return this.isEditMode ? 'Edit Card' : 'Add Card';
  }

  onClose(): void {
    this.resetForm();
    this.close.emit();
  }

  saveCard(): void {
    if (!this.cardTitle.trim()) return;

    this.isSaving = true;
    this.error = null;

    if (this.isEditMode) {
      // Edit existing card
      const cardId = this.card()!.id;
      this.cardService.updateCard(
        this.projectId(),
        this.boardId(),
        cardId,
        {
          title: this.cardTitle,
          description: this.cardDescription
        }
      ).subscribe({
        next: () => {
          this.cardSaved.emit();
          this.resetForm();
          this.isSaving = false;
          this.close.emit();
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Error updating card:', err);
          this.error = 'Failed to update card. Please try again.';
          this.isSaving = false;
          this.cdr.detectChanges();
        }
      });
    } else {
      // Add new card
      const targetColumnId = this.columnId();
      if (!targetColumnId) {
        this.error = 'Column ID is required';
        this.isSaving = false;
        return;
      }

      this.cardService.addCard(
        this.projectId(),
        this.boardId(),
        {
          columnId: targetColumnId,
          title: this.cardTitle,
          description: this.cardDescription
        }
      ).subscribe({
        next: () => {
          this.cardSaved.emit();
          this.resetForm();
          this.isSaving = false;
          this.close.emit();
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Error adding card:', err);
          this.error = 'Failed to add card. Please try again.';
          this.isSaving = false;
          this.cdr.detectChanges();
        }
      });
    }
  }

  deleteCard(): void {
    if (!this.isEditMode) return;

    if (!confirm('Are you sure you want to delete this card?')) return;

    this.isDeleting = true;
    this.error = null;

    const cardId = this.card()!.id;
    this.cardService.deleteCard(this.projectId(), this.boardId(), cardId).subscribe({
      next: () => {
        this.cardDeleted.emit();
        this.resetForm();
        this.isDeleting = false;
        this.close.emit();
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error deleting card:', err);
        this.error = 'Failed to delete card. Please try again.';
        this.isDeleting = false;
        this.cdr.detectChanges();
      }
    });
  }

  private resetForm(): void {
    this.cardTitle = '';
    this.cardDescription = '';
    this.error = null;
  }
}
