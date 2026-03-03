import { Component, inject, computed, ChangeDetectionStrategy } from '@angular/core';
import { DrawerService } from '../../core/services/drawer.service';
import { CardDrawerEventsService } from '../../features/project-detail/services/card-drawer-events.service';
import { CreateBoardDrawerEventsService } from '../../features/project-detail/services/create-board-drawer-events.service';
import { CardDrawerComponent } from '../../features/project-detail/components/board/card-drawer/card-drawer.component';
import { CreateBoardDrawerComponent } from '../../features/project-detail/components/project/create-board-drawer/create-board-drawer.component';
import { CardDrawerConfig, CreateBoardDrawerConfig } from '../../core/models/drawer.model';

@Component({
  selector: 'app-drawer-host',
  imports: [CardDrawerComponent, CreateBoardDrawerComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-card-drawer
      [open]="isCardDrawerOpen()"
      [projectId]="cardConfig()?.projectId ?? ''"
      [boardId]="cardConfig()?.boardId ?? ''"
      [card]="cardConfig()?.card ?? null"
      [boardLabels]="cardConfig()?.boardLabels ?? []"
      [initialActivity]="cardConfig()?.initialActivity ?? null"
      (close)="drawerService.close()"
      (cardSaved)="onCardSaved()"
      (cardDeleted)="onCardDeleted()"
    />

    <app-create-board-drawer
      [open]="isCreateBoardOpen()"
      [projectId]="createBoardConfig()?.projectId ?? ''"
      (close)="drawerService.close()"
      (boardCreated)="onBoardCreated()"
    />
  `,
})
export class DrawerHostComponent {
  protected drawerService = inject(DrawerService);
  private cardDrawerEvents = inject(CardDrawerEventsService);
  private createBoardDrawerEvents = inject(CreateBoardDrawerEventsService);

  protected cardConfig = computed<CardDrawerConfig | null>(() => {
    const config = this.drawerService.config();
    return config?.type === 'card-detail' ? config : null;
  });

  protected createBoardConfig = computed<CreateBoardDrawerConfig | null>(() => {
    const config = this.drawerService.config();
    return config?.type === 'create-board' ? config : null;
  });

  protected isCardDrawerOpen = computed(() => this.cardConfig() !== null);
  protected isCreateBoardOpen = computed(() => this.createBoardConfig() !== null);

  onCardSaved(): void {
    this.cardDrawerEvents.notifyCardUpdated();
  }

  onCardDeleted(): void {
    this.drawerService.close();
    this.cardDrawerEvents.notifyCardDeleted();
  }

  onBoardCreated(): void {
    this.drawerService.close();
    this.createBoardDrawerEvents.notifyBoardCreated();
  }
}
