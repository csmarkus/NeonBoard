import { Component, input, output, signal, afterNextRender, inject, Injector, ElementRef, ChangeDetectionStrategy } from '@angular/core';
import { form } from '@angular/forms/signals';
import { CdkDragDrop, CdkDragStart, DragDropModule } from '@angular/cdk/drag-drop';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faGripVertical, faCheck, faXmark } from '@fortawesome/free-solid-svg-icons';
import { Column } from '../../../models/column.model';
import { Card } from '../../../models/card.model';
import { CardComponent } from '../card/card.component';

@Component({
  selector: 'app-column',
  imports: [DragDropModule, FontAwesomeModule, CardComponent],
  templateUrl: './column.component.html',
  styleUrl: './column.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
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
  cardDragStarted = output<number>();

  menuOpen = signal(false);
  editingName = signal(false);
  addingCard = signal(false);
  draggedCardHeight = signal(0);

  renameModel = signal({ name: '' });
  renameForm = form(this.renameModel);

  addCardModel = signal({ title: '' });
  addCardForm = form(this.addCardModel);

  startEdit(): void {
    this.editingName.set(true);
    this.renameModel.set({ name: this.column().name });
    this.menuOpen.set(false);
  }

  cancelEdit(): void {
    this.editingName.set(false);
    this.renameModel.set({ name: '' });
  }

  saveName(): void {
    const newName = this.renameModel().name.trim();
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
    if (!this.menuOpen()) return;

    const menuButton = this.elementRef.nativeElement.querySelector('.column-menu-trigger');
    const menuDropdown = this.elementRef.nativeElement.querySelector('.column-menu-dropdown');
    const target = event.target as Node;

    if (
      (!menuButton || !menuButton.contains(target)) &&
      (!menuDropdown || !menuDropdown.contains(target))
    ) {
      this.menuOpen.set(false);
    }
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
    this.addCardModel.set({ title: '' });

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
    this.addCardModel.set({ title: '' });
  }

  saveCard(): void {
    const title = this.addCardModel().title.trim();
    if (title) {
      this.cardAdded.emit({ columnId: this.column().id, title });
      this.cancelAddCard();
    }
  }

  onRenameInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.renameModel.set({ name: value });
  }

  onCardTitleInput(event: Event): void {
    const value = (event.target as HTMLTextAreaElement).value;
    this.addCardModel.set({ title: value });
  }
}
