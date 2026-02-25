import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormField, form } from '@angular/forms/signals';
import { DrawerComponent } from '../../../../../shared/components/drawer/drawer.component';
import { InputComponent } from '../../../../../shared/components/input/input.component';
import { ModalService } from '../../../../../core/services/modal.service';
import { BoardStateFacade } from '../../../services/board-state.facade';
import { Card } from '../../../models/card.model';

@Component({
  selector: 'app-archive-panel',
  imports: [FormField, DrawerComponent, InputComponent, DatePipe],
  templateUrl: './archive-panel.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArchivePanelComponent {
  private facade = inject(BoardStateFacade);
  private modalService = inject(ModalService);

  open = this.facade.showArchivePanel;
  archivedCards = this.facade.archivedCards;
  isLoading = this.facade.isLoadingArchive;

  formModel = signal({ searchQuery: '' });
  searchForm = form(this.formModel);

  filteredCards = computed(() => {
    const query = this.formModel().searchQuery.toLowerCase().trim();
    const cards = this.archivedCards();
    if (!query) return cards;
    return cards.filter(card =>
      card.title.toLowerCase().includes(query) ||
      card.displayId.toLowerCase().includes(query)
    );
  });

  onClose(): void {
    this.facade.closeArchivePanel();
    this.formModel.set({ searchQuery: '' });
  }

  viewCard(card: Card): void {
    this.facade.openArchivedCardInDrawer(card);
  }

  restoreCard(card: Card, event: Event): void {
    event.stopPropagation();
    this.facade.restoreArchivedCard(card.id);
  }

  async requestDeleteCard(card: Card, event: Event): Promise<void> {
    event.stopPropagation();
    const confirmed = await this.modalService.confirm({
      title: 'Delete Card',
      message: `Are you sure you want to permanently delete "${card.title}"? This action cannot be undone.`,
      confirmText: 'Delete',
    });
    if (confirmed) {
      this.facade.deleteArchivedCard(card.id);
    }
  }
}
