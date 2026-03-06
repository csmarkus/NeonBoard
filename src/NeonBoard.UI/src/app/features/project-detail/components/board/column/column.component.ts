import { Component, input, output, signal, viewChild, afterNextRender, inject, Injector, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CdkDragDrop, CdkDragStart, DragDropModule } from '@angular/cdk/drag-drop';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faGripVertical, faCheck, faXmark } from '@fortawesome/free-solid-svg-icons';
import { Column } from '../../../models/column.model';
import { Card } from '../../../models/card.model';
import { CardComponent } from '../card/card.component';

@Component({
  selector: 'app-column',
  imports: [CommonModule, FormsModule, DragDropModule, FontAwesomeModule, CardComponent],
  templateUrl: './column.component.html',
  styleUrl: './column.component.css',
  host: {
    '(document:click)': 'onDocumentClick($event)',
  },
})
export class ColumnComponent {
  private injector = inject(Injector);
  private elementRef = inject(ElementRef);

  faGripVertical = faGripVertical;
  faCheck = faCheck;
  faXmark = faXmark;

  column = input.required<Column>();
  cards = input.required<Card[]>();
  columnIds = input.required<string[]>();
  accentClass = input.required<string>();

  cardDropped = output<CdkDragDrop<Card[]>>();
  columnRenamed = output<{ columnId: string; newName: string }>();
  columnDeleted = output<string>();
  cardSelected = output<Card>();
  cardAdded = output<{ columnId: string; title: string }>();

  private menuTrigger = viewChild<ElementRef>('menuTrigger');
  private menuDropdown = viewChild<ElementRef>('menuDropdown');
  private addCardForm = viewChild<ElementRef>('addCardForm');
  private addCardTextarea = viewChild<ElementRef>('addCardTextarea');

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

  onDocumentClick(event: MouseEvent): void {
    const target = event.target as Node;

    if (this.menuOpen()) {
      const trigger = this.menuTrigger()?.nativeElement;
      const dropdown = this.menuDropdown()?.nativeElement;

      if (
        (!trigger || !trigger.contains(target)) &&
        (!dropdown || !dropdown.contains(target))
      ) {
        this.menuOpen.set(false);
      }
    }

    if (this.addingCard()) {
      const form = this.addCardForm()?.nativeElement;
      if (form && !form.contains(target)) {
        this.cancelAddCard();
      }
    }
  }

  onCardDrop(event: CdkDragDrop<Card[]>): void {
    this.cardDropped.emit(event);
  }

  onCardDragStarted(event: CdkDragStart): void {
    const cardHeight = event.source.element.nativeElement.offsetHeight;
    event.source.getPlaceholderElement().style.height = `${cardHeight}px`;
  }

  selectCard(card: Card): void {
    this.cardSelected.emit(card);
  }

  openAddCard(): void {
    this.addingCard.set(true);
    this.newCardTitle.set('');

    afterNextRender(() => {
      this.addCardTextarea()?.nativeElement.focus();
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
