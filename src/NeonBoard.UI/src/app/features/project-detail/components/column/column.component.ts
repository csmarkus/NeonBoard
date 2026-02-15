import { Component, input, output, signal, afterNextRender, inject, Injector } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CdkDragDrop, CdkDragStart, DragDropModule } from '@angular/cdk/drag-drop';
import { Column } from '../../models/column.model';
import { Card } from '../../models/card.model';
import { CardComponent } from '../card/card.component';

@Component({
  selector: 'app-column',
  imports: [CommonModule, FormsModule, DragDropModule, CardComponent],
  templateUrl: './column.component.html',
  styleUrl: './column.component.css',
})
export class ColumnComponent {
  private injector = inject(Injector);

  column = input.required<Column>();
  cards = input.required<Card[]>();
  columnIds = input.required<string[]>();
  accentClass = input.required<string>();

  cardDropped = output<CdkDragDrop<Card[]>>();
  columnRenamed = output<{ columnId: string; newName: string }>();
  columnDeleted = output<string>();
  cardSelected = output<Card>();
  cardAdded = output<{ columnId: string; title: string }>();
  cardDragStarted = output<number>();

  menuOpen = signal(false);
  editingName = signal(false);
  editingValue = signal('');
  addingCard = signal(false);
  newCardTitle = signal('');
  draggedCardHeight = signal(0);

  startEdit(): void {
    this.editingName.set(true);
    this.editingValue.set(this.column().name);
    this.menuOpen.set(false);
  }

  cancelEdit(): void {
    this.editingName.set(false);
    this.editingValue.set('');
  }

  saveName(): void {
    const newName = this.editingValue().trim();
    if (newName && newName !== this.column().name) {
      this.columnRenamed.emit({ columnId: this.column().id, newName });
    }
    this.cancelEdit();
  }

  deleteColumn(): void {
    this.columnDeleted.emit(this.column().id);
    this.menuOpen.set(false);
  }

  toggleMenu(): void {
    this.menuOpen.update(open => !open);
  }

  onCardDrop(event: CdkDragDrop<Card[]>): void {
    this.cardDropped.emit(event);
  }

  onCardDragStarted(event: CdkDragStart): void {
    const cardElement = event.source.element.nativeElement;
    const cardHeight = cardElement.offsetHeight;
    this.draggedCardHeight.set(cardHeight);
    this.cardDragStarted.emit(cardHeight);
  }

  selectCard(card: Card): void {
    this.cardSelected.emit(card);
  }

  openAddCard(): void {
    this.addingCard.set(true);
    this.newCardTitle.set('');

    afterNextRender(() => {
      const textarea = document.querySelector(`#add-card-${this.column().id}`) as HTMLTextAreaElement;
      if (textarea) {
        textarea.focus();
      }
    }, { injector: this.injector });
  }

  autoResize(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    textarea.style.height = 'auto';
    textarea.style.height = textarea.scrollHeight + 'px';
  }

  cancelAddCard(): void {
    this.addingCard.set(false);
    this.newCardTitle.set('');
  }

  saveCard(): void {
    const title = this.newCardTitle().trim();
    if (title) {
      this.cardAdded.emit({ columnId: this.column().id, title });
      this.cancelAddCard();
    }
  }
}
