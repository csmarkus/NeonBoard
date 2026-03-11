import { ChangeDetectionStrategy, Component, input, inject, signal, computed, effect, untracked } from '@angular/core';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';
import { BoardStateFacade } from '../../../services/board-state.facade';
import { ProjectContext } from '../../../services/project-context.service';
import { ColumnComponent } from '../column/column.component';
import { AddColumnButtonComponent } from '../add-column-button/add-column-button.component';
import { Column } from '../../../models/column.model';
import { Card } from '../../../models/card.model';
import { getPositionBetween } from '../../../../../core/utils/fractional-index';

@Component({
  selector: 'app-board-canvas',
  imports: [DragDropModule, FontAwesomeModule, ColumnComponent, AddColumnButtonComponent],
  host: {
    class: 'flex-1 flex flex-col'
  },
  templateUrl: './board-canvas.component.html',
  styleUrl: './board-canvas.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BoardCanvasComponent {
  private facade = inject(BoardStateFacade);
  protected projectContext = inject(ProjectContext);

  faTriangleExclamation = faTriangleExclamation;

  projectId = input.required<string>();
  boardId = input.required<string>();

  isAddingColumn = signal<boolean>(false);
  newColumnName = signal<string>('');

  board = this.facade.board;
  columns = this.facade.columns;
  cardsByColumn = this.facade.filteredCardsByColumn;
  labels = this.facade.labels;
  isLoading = this.facade.isLoading;
  error = this.facade.error;

  columnIds = computed(() => this.columns().map(c => c.id));

  constructor() {
    effect(() => {
      const projectId = this.projectId();
      const boardId = this.boardId();

      if (projectId && boardId) {
        untracked(() => this.facade.loadBoard(projectId, boardId, true));
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

    const column = columns[event.currentIndex];
    const before = event.currentIndex > 0 ? columns[event.currentIndex - 1].position : null;
    const after = event.currentIndex < columns.length - 1 ? columns[event.currentIndex + 1].position : null;
    const newPosition = getPositionBetween(before, after);

    this.facade.moveColumn(this.projectId(), this.boardId(), column.id, newPosition);
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

    const cards = event.container.data;
    const index = event.currentIndex;
    const before = index > 0 ? cards[index - 1].position : null;
    const after = index < cards.length - 1 ? cards[index + 1].position : null;
    const newPosition = getPositionBetween(before, after);
    const card = cards[index];

    this.facade.moveCard(this.projectId(), this.boardId(), card.id, targetColumnId, newPosition);
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

}
