import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { SidebarComponent } from '../../../../layout/sidebar/sidebar.component';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { BadgeComponent } from '../../../../shared/components/badge/badge.component';
import { CardDrawerComponent } from '../../components/card-drawer/card-drawer.component';
import { BoardService } from '../../services/board.service';
import { ColumnService } from '../../services/column.service';
import { CardService } from '../../services/card.service';
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
    SidebarComponent,
    ButtonComponent,
    InputComponent,
    BadgeComponent,
    CardDrawerComponent,
  ],
  host: {
    class: 'block h-screen'
  },
  templateUrl: './board-view.component.html',
  styleUrl: './board-view.component.css',
})
export class BoardViewComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private boardService = inject(BoardService);
  private columnService = inject(ColumnService);
  private cardService = inject(CardService);

  projectId = signal<string>('');
  boardId = signal<string>('');
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
  selectedCard = signal<Card | null>(null);
  isAddingCard = signal<boolean>(false);
  addingCardColumnId = signal<string | null>(null);

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
    const projectId = this.route.snapshot.paramMap.get('projectId');
    const boardId = this.route.snapshot.paramMap.get('boardId');

    if (projectId && boardId) {
      this.projectId.set(projectId);
      this.boardId.set(boardId);
      this.loadBoard();
    }
  }

  private loadBoard(): void {
    this.isLoading.set(true);
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
        this.loadBoard(); // Reload on error
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
        this.loadBoard(); // Reload on error
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
        this.loadBoard();
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
        this.loadBoard();
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
        this.loadBoard();
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
    this.selectedCard.set(card);
  }

  closeCardDetail(): void {
    this.selectedCard.set(null);
  }

  openAddCard(columnId: string): void {
    this.isAddingCard.set(true);
    this.addingCardColumnId.set(columnId);
  }

  closeAddCard(): void {
    this.isAddingCard.set(false);
    this.addingCardColumnId.set(null);
  }

  onCardCreated(): void {
    this.closeAddCard();
    this.loadBoard();
  }

  onCardUpdated(): void {
    this.closeCardDetail();
    this.loadBoard();
  }

  onCardDeleted(): void {
    this.closeCardDetail();
    this.loadBoard();
  }
}
