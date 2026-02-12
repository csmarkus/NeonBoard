import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faGear } from '@fortawesome/free-solid-svg-icons';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { ColumnComponent } from '../../components/column/column.component';
import { BoardService } from '../../services/board.service';
import { ColumnService } from '../../services/column.service';
import { CardService } from '../../services/card.service';
import { DrawerService } from '../../services/drawer.service';
import { ProjectService } from '../../../projects/services/project.service';
import { BoardDetails } from '../../models/board.model';
import { Column } from '../../models/column.model';
import { Card } from '../../models/card.model';

@Component({
  selector: 'app-board-view',
  imports: [
    CommonModule,
    FormsModule,
    DragDropModule,
    RouterLink,
    FontAwesomeModule,
    InputComponent,
    ColumnComponent,
  ],
  host: {
    class: 'flex flex-col h-full'
  },
  templateUrl: './board-view.component.html',
  styleUrl: './board-view.component.css',
})
export class BoardViewComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private boardService = inject(BoardService);
  private columnService = inject(ColumnService);
  private cardService = inject(CardService);
  private drawerService = inject(DrawerService);
  private projectService = inject(ProjectService);

  faGear = faGear;

  projectId = signal<string>('');
  boardId = signal<string>('');
  projectName = signal<string>('');
  board = signal<BoardDetails | null>(null);
  isLoading = signal<boolean>(true);
  error = signal<string | null>(null);

  isAddingColumn = signal<boolean>(false);
  newColumnName = signal<string>('');

  columns = computed(() => this.board()?.columns ?? []);
  columnIds = computed(() => this.columns().map(c => c.id));

  cardsByColumn = computed(() => {
    const cards = this.board()?.cards ?? [];
    const result: Record<string, Card[]> = {};

    this.columns().forEach(col => {
      result[col.id] = [];
    });

    cards.forEach(card => {
      if (result[card.columnId]) {
        result[card.columnId].push(card);
      }
    });

    Object.keys(result).forEach(columnId => {
      result[columnId].sort((a, b) => a.position - b.position);
    });

    return result;
  });

  ngOnInit(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (projectId) {
      this.projectId.set(projectId);
      this.projectService.getProject(projectId).subscribe({
        next: (project) => this.projectName.set(project.name),
      });
    }

    this.route.paramMap.subscribe(params => {
      const boardId = params.get('boardId');
      if (boardId) {
        this.boardId.set(boardId);
        this.loadBoard(true);
      }
    });

    this.drawerService.cardUpdated$.subscribe(() => {
      this.loadBoard(false);
    });

    this.drawerService.cardDeleted$.subscribe(() => {
      this.loadBoard(false);
    });
  }

  private loadBoard(showLoading = true): void {
    if (showLoading) {
      this.isLoading.set(true);
    }
    this.error.set(null);

    this.boardService.getBoardDetails(this.projectId(), this.boardId()).subscribe({
      next: (board) => {
        this.board.set(board);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading board:', err);
        this.error.set('Failed to load board');
        this.isLoading.set(false);
      }
    });
  }

  getTotalCardCount(): number {
    return this.board()?.cards.length ?? 0;
  }

  getAccentClass(position: number): string {
    const accents = ['bg-status-todo', 'bg-status-progress', 'bg-status-review', 'bg-status-done'];
    return accents[position % accents.length];
  }

  dropColumn(event: CdkDragDrop<Column[]>): void {
    if (event.previousIndex === event.currentIndex) return;

    const columns = [...this.columns()];
    moveItemInArray(columns, event.previousIndex, event.currentIndex);

    const updatedBoard = { ...this.board()!, columns };
    this.board.set(updatedBoard);

    const columnIds = columns.map(c => c.id);
    this.columnService.reorderColumns(this.projectId(), this.boardId(), { columnIds }).subscribe({
      error: (err) => {
        console.error('Error reordering columns:', err);
        this.loadBoard(false); // Reload on error
      }
    });
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

    this.cardService.moveCard(this.projectId(), this.boardId(), card.id, {
      targetColumnId,
      targetPosition: event.currentIndex
    }).subscribe({
      error: (err) => {
        console.error('Error moving card:', err);
        this.loadBoard(false); 
      }
    });
  }

  openAddColumn(): void {
    this.isAddingColumn.set(true);
    this.newColumnName.set('');
  }

  cancelAddColumn(): void {
    this.isAddingColumn.set(false);
    this.newColumnName.set('');
  }

  addColumn(): void {
    const name = this.newColumnName().trim();
    if (!name) return;

    this.columnService.addColumn(this.projectId(), this.boardId(), { name }).subscribe({
      next: () => {
        this.cancelAddColumn();
        this.loadBoard(false);
      },
      error: (err) => {
        console.error('Error adding column:', err);
      }
    });
  }

  onColumnRenamed(data: { columnId: string; newName: string }): void {
    this.columnService.renameColumn(this.projectId(), this.boardId(), data.columnId, { newName: data.newName }).subscribe({
      next: () => {
        this.loadBoard(false);
      },
      error: (err) => {
        console.error('Error renaming column:', err);
      }
    });
  }

  onColumnDeleted(columnId: string): void {
    this.columnService.deleteColumn(this.projectId(), this.boardId(), columnId).subscribe({
      next: () => {
        this.loadBoard(false);
      },
      error: (err) => {
        console.error('Error deleting column:', err);
        const errorMessage = err.error?.title || err.error?.detail || 'Failed to delete column';
        alert(errorMessage);
      }
    });
  }

  onCardSelected(card: Card): void {
    this.drawerService.openCardDrawer(card, this.projectId(), this.boardId());
  }

  onCardAdded(data: { columnId: string; title: string }): void {
    this.cardService.addCard(this.projectId(), this.boardId(), {
      columnId: data.columnId,
      title: data.title,
      description: ''
    }).subscribe({
      next: () => {
        this.loadBoard(false);
      },
      error: (err) => {
        console.error('Error adding card:', err);
      }
    });
  }
}
