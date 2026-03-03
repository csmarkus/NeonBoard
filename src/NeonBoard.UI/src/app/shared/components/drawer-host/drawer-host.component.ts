import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { DrawerService } from '../../../core/services/drawer.service';
import { CardDrawerEventsService } from '../../../features/project-detail/services/card-drawer-events.service';
import { CreateBoardDrawerEventsService } from '../../../features/project-detail/services/create-board-drawer-events.service';
import { CardDrawerComponent } from '../../../features/project-detail/components/board/card-drawer/card-drawer.component';
import { CreateBoardDrawerComponent } from '../../../features/project-detail/components/project/create-board-drawer/create-board-drawer.component';

@Component({
  selector: 'app-drawer-host',
  imports: [CardDrawerComponent, CreateBoardDrawerComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (drawerService.config(); as config) {
      @switch (config.type) {
        @case ('card-detail') {
          <app-card-drawer
            [open]="true"
            [projectId]="config.projectId"
            [boardId]="config.boardId"
            [card]="config.card"
            [boardLabels]="config.boardLabels"
            [initialActivity]="config.initialActivity"
            (close)="drawerService.close()"
            (cardSaved)="onCardSaved()"
            (cardDeleted)="onCardDeleted()"
          />
        }
        @case ('create-board') {
          <app-create-board-drawer
            [open]="true"
            [projectId]="config.projectId"
            (close)="drawerService.close()"
            (boardCreated)="onBoardCreated()"
          />
        }
      }
    }
  `,
})
export class DrawerHostComponent {
  protected drawerService = inject(DrawerService);
  private cardDrawerEvents = inject(CardDrawerEventsService);
  private createBoardDrawerEvents = inject(CreateBoardDrawerEventsService);

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
