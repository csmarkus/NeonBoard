import { Component, input, output, signal, afterNextRender, inject, Injector } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CdkDragDrop, DragDropModule } from '@angular/cdk/drag-drop';
import { Column } from '../../models/column.model';
import { Card } from '../../models/card.model';
import { CardComponent } from '../card/card.component';

@Component({
  selector: 'app-column',
  imports: [CommonModule, FormsModule, DragDropModule, CardComponent],
  host: {
    class: 'h-full'
  },
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

  menuOpen = signal(false);
  editingName = signal(false);
  editingValue = signal('');
  addingCard = signal(false);
  newCardTitle = signal('');

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

  selectCard(card: Card): void {
    this.cardSelected.emit(card);
  }

  openAddCard(): void {
    this.addingCard.set(true);
    this.newCardTitle.set('');

    afterNextRender(() => {
      const input = document.querySelector(`#add-card-${this.column().id}`) as HTMLInputElement;
      if (input) {
        input.focus();
      }
    }, { injector: this.injector });
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
