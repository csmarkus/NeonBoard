import { Component, input, inject, signal, computed, effect } from '@angular/core';
import { CommonModule, NgStyle } from '@angular/common';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { BoardStateFacade } from '../../services/board-state.facade';
import { ColumnComponent } from '../column/column.component';
import { AddColumnButtonComponent } from '../add-column-button/add-column-button.component';
import { Column } from '../../models/column.model';
import { Card } from '../../models/card.model';

@Component({
  selector: 'app-board-canvas',
  imports: [CommonModule, NgStyle, DragDropModule, ColumnComponent, AddColumnButtonComponent],
  host: {
    class: 'flex-1 flex flex-col'
  },
  templateUrl: './board-canvas.component.html',
  styleUrl: './board-canvas.component.css',
})
export class BoardCanvasComponent {
  private facade = inject(BoardStateFacade);

  projectId = input.required<string>();
  boardId = input.required<string>();

  isAddingColumn = signal<boolean>(false);
  newColumnName = signal<string>('');
  draggedCardHeight = signal<number>(0);

  board = this.facade.board;
  columns = this.facade.columns;
  cardsByColumn = this.facade.cardsByColumn;
  labels = this.facade.labels;
  isLoading = this.facade.isLoading;
  error = this.facade.error;

  columnIds = computed(() => this.columns().map(c => c.id));

  cardHeightStyle = computed(() => ({
    '--dragged-card-height': `${this.draggedCardHeight()}px`
  }));

  constructor() {
    effect(() => {
      const projectId = this.projectId();
      const boardId = this.boardId();

      if (projectId && boardId) {
        this.facade.loadBoard(projectId, boardId, true);
      }
    });
  }

  getAccentClass(position: number): string {
    const accents = ['bg-status-todo', 'bg-status-progress', 'bg-status-review', 'bg-status-done'];
    return accents[position % accents.length];
  }

  dropColumn(event: CdkDragDrop<Column[]>): void {
    if (event.previousIndex === event.currentIndex) return;

    const columns = [...this.columns()];
    moveItemInArray(columns, event.previousIndex, event.currentIndex);

    const columnIds = columns.map(c => c.id);
    this.facade.reorderColumns(this.projectId(), this.boardId(), columnIds);
  }

  onCardDropped(event: CdkDragDrop<Card[]>, targetColumnId: string): void {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    }

    const card = event.container.data[event.currentIndex];

    this.facade.moveCard(
      this.projectId(),
      this.boardId(),
      card.id,
      targetColumnId,
      event.currentIndex
    );
  }

  openAddColumn(): void {
    this.isAddingColumn.set(true);
    this.newColumnName.set('');
  }

  cancelAddColumn(): void {
    this.isAddingColumn.set(false);
    this.newColumnName.set('');
  }

  addColumn(name: string): void {
    this.facade.addColumn(this.projectId(), this.boardId(), name);
    this.cancelAddColumn();
  }

  onColumnRenamed(data: { columnId: string; newName: string }): void {
    this.facade.renameColumn(this.projectId(), this.boardId(), data.columnId, data.newName);
  }

  onColumnDeleted(columnId: string): void {
    this.facade.deleteColumn(this.projectId(), this.boardId(), columnId);
  }

  onCardSelected(card: Card): void {
    this.facade.openCardDrawer(card, this.projectId(), this.boardId());
  }

  onCardAdded(data: { columnId: string; title: string }): void {
    this.facade.addCard(this.projectId(), this.boardId(), data.columnId, data.title);
  }

  onColumnNameChange(name: string): void {
    this.newColumnName.set(name);
  }

  onCardDragStarted(height: number): void {
    this.draggedCardHeight.set(height);
  }
}
