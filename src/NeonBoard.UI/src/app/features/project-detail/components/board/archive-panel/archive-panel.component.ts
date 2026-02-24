import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DrawerComponent } from '../../../../../shared/components/drawer/drawer.component';
import { InputComponent } from '../../../../../shared/components/input/input.component';
import { ConfirmationModalComponent } from '../../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { BoardStateFacade } from '../../../services/board-state.facade';
import { Card } from '../../../models/card.model';

@Component({
  selector: 'app-archive-panel',
  imports: [FormsModule, DrawerComponent, InputComponent, ConfirmationModalComponent, DatePipe],
  templateUrl: './archive-panel.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArchivePanelComponent {
  private facade = inject(BoardStateFacade);

  open = this.facade.showArchivePanel;
  archivedCards = this.facade.archivedCards;
  isLoading = this.facade.isLoadingArchive;

  searchQuery = signal('');
  cardToDelete = signal<Card | null>(null);

  filteredCards = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const cards = this.archivedCards();
    if (!query) return cards;
    return cards.filter(card =>
      card.title.toLowerCase().includes(query) ||
      card.displayId.toLowerCase().includes(query)
    );
  });

  onClose(): void {
    this.facade.closeArchivePanel();
    this.searchQuery.set('');
  }

  onSearchChange(value: string): void {
    this.searchQuery.set(value);
  }

  viewCard(card: Card): void {
    this.facade.openArchivedCardInDrawer(card);
  }

  restoreCard(card: Card, event: Event): void {
    event.stopPropagation();
    this.facade.restoreArchivedCard(card.id);
  }

  requestDeleteCard(card: Card, event: Event): void {
    event.stopPropagation();
    this.cardToDelete.set(card);
  }

  confirmDeleteCard(): void {
    const card = this.cardToDelete();
    if (card) {
      this.facade.deleteArchivedCard(card.id);
      this.cardToDelete.set(null);
    }
  }

  cancelDeleteCard(): void {
    this.cardToDelete.set(null);
  }
}
