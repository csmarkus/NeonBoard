import { Component, inject, signal, OnInit, computed, afterNextRender, Injector } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faGear } from '@fortawesome/free-solid-svg-icons';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { CardComponent } from '../../components/card/card.component';
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
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DragDropModule,
    RouterLink,
    FontAwesomeModule,
    InputComponent,
    CardComponent,
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
  private injector = inject(Injector);

  faGear = faGear;

  projectId = signal<string>('');
  boardId = signal<string>('');
  projectName = signal<string>('');
  board = signal<BoardDetails | null>(null);
  isLoading = signal<boolean>(true);
  error = signal<string | null>(null);

  // Column operations state
  isAddingColumn = signal<boolean>(false);
  newColumnName = signal<string>('');
  editingColumnId = signal<string | null>(null);
  editingColumnName = signal<string>('');
  columnMenuOpen = signal<string | null>(null);

  // Card operations state
  addingCardColumnId = signal<string | null>(null);
  newCardTitle = signal<string>('');

  // Computed values
  columns = computed(() => this.board()?.columns ?? []);
  columnIds = computed(() => this.columns().map(c => c.id));

  // Cards organized by column
  cardsByColumn = computed(() => {
    const cards = this.board()?.cards ?? [];
    const result: Record<string, Card[]> = {};

    // Initialize empty arrays for each column
    this.columns().forEach(col => {
      result[col.id] = [];
    });

    // Group cards by column
    cards.forEach(card => {
      if (result[card.columnId]) {
        result[card.columnId].push(card);
      }
    });

    // Sort cards by position within each column
    Object.keys(result).forEach(columnId => {
      result[columnId].sort((a, b) => a.position - b.position);
    });

    return result;
  });

  ngOnInit(): void {
    // Get projectId from parent route (doesn't change)
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (projectId) {
      this.projectId.set(projectId);
      this.projectService.getProject(projectId).subscribe({
        next: (project) => this.projectName.set(project.name),
      });
    }

    // Subscribe to boardId changes - show loading for route transitions
    this.route.paramMap.subscribe(params => {
      const boardId = params.get('boardId');
      if (boardId) {
        this.boardId.set(boardId);
        this.loadBoard(true);
      }
    });

    // Subscribe to card updates/deletes to reload board silently
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

    // Update local state optimistically
    const updatedBoard = { ...this.board()!, columns };
    this.board.set(updatedBoard);

    // Call API to persist
    const columnIds = columns.map(c => c.id);
    this.columnService.reorderColumns(this.projectId(), this.boardId(), { columnIds }).subscribe({
      error: (err) => {
        console.error('Error reordering columns:', err);
        this.loadBoard(false); // Reload on error
      }
    });
  }

  dropCard(event: CdkDragDrop<Card[]>, targetColumnId: string): void {
    const sourceColumnId = event.previousContainer.id;

    if (event.previousContainer === event.container) {
      // Reorder within same column
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      // Move to different column
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    }

    // Get the card that was moved
    const card = event.container.data[event.currentIndex];

    // Call API to persist the move
    this.cardService.moveCard(this.projectId(), this.boardId(), card.id, {
      targetColumnId,
      targetPosition: event.currentIndex
    }).subscribe({
      error: (err) => {
        console.error('Error moving card:', err);
        this.loadBoard(false); // Reload on error
      }
    });
  }

  // Column operations
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

  startEditColumn(column: Column): void {
    this.editingColumnId.set(column.id);
    this.editingColumnName.set(column.name);
    this.columnMenuOpen.set(null);
  }

  cancelEditColumn(): void {
    this.editingColumnId.set(null);
    this.editingColumnName.set('');
  }

  saveColumnName(columnId: string): void {
    const newName = this.editingColumnName().trim();
    if (!newName) return;

    this.columnService.renameColumn(this.projectId(), this.boardId(), columnId, { newName }).subscribe({
      next: () => {
        this.cancelEditColumn();
        this.loadBoard(false);
      },
      error: (err) => {
        console.error('Error renaming column:', err);
      }
    });
  }

  deleteColumn(columnId: string): void {
    this.columnService.deleteColumn(this.projectId(), this.boardId(), columnId).subscribe({
      next: () => {
        this.columnMenuOpen.set(null);
        this.loadBoard(false);
      },
      error: (err) => {
        console.error('Error deleting column:', err);
        const errorMessage = err.error?.title || err.error?.detail || 'Failed to delete column';
        alert(errorMessage); // Simple alert for now
      }
    });
  }

  toggleColumnMenu(columnId: string): void {
    this.columnMenuOpen.set(this.columnMenuOpen() === columnId ? null : columnId);
  }

  // Card operations
  selectCard(card: Card): void {
    this.drawerService.openCardDrawer(card, this.projectId(), this.boardId());
  }

  openAddCard(columnId: string): void {
    this.addingCardColumnId.set(columnId);
    this.newCardTitle.set('');

    // Focus the input after the DOM updates
    afterNextRender(() => {
      const input = document.querySelector(`input[placeholder="Card title..."]`) as HTMLInputElement;
      if (input) {
        input.focus();
      }
    }, { injector: this.injector });
  }

  cancelAddCard(): void {
    this.addingCardColumnId.set(null);
    this.newCardTitle.set('');
  }

  saveNewCard(columnId: string): void {
    const title = this.newCardTitle().trim();
    if (!title) return;

    this.cardService.addCard(this.projectId(), this.boardId(), {
      columnId,
      title,
      description: ''
    }).subscribe({
      next: () => {
        this.cancelAddCard();
        this.loadBoard(false);
      },
      error: (err) => {
        console.error('Error adding card:', err);
      }
    });
  }
}
