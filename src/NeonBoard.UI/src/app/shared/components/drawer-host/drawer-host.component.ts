import { Component, inject, signal, effect, ChangeDetectionStrategy } from '@angular/core';
import { DrawerService } from '../../../core/services/drawer.service';
import { CardDrawerEventsService } from '../../../features/project-detail/services/card-drawer-events.service';
import { CreateBoardDrawerEventsService } from '../../../features/project-detail/services/create-board-drawer-events.service';
import { CardDrawerComponent } from '../../../features/project-detail/components/board/card-drawer/card-drawer.component';
import { CreateBoardDrawerComponent } from '../../../features/project-detail/components/project/create-board-drawer/create-board-drawer.component';
import { DrawerConfig } from '../../../core/models/drawer.model';

const ANIMATION_DURATION = 200;

@Component({
  selector: 'app-drawer-host',
  imports: [CardDrawerComponent, CreateBoardDrawerComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (renderedConfig(); as config) {
      @switch (config.type) {
        @case ('card-detail') {
          <app-card-drawer
            [open]="animOpen()"
            [projectId]="config.projectId"
            [boardId]="config.boardId"
            [card]="config.card"
            [boardLabels]="config.boardLabels"
            [initialActivity]="config.initialActivity"
            (close)="requestClose()"
            (cardSaved)="onCardSaved()"
            (cardDeleted)="onCardDeleted()"
          />
        }
        @case ('create-board') {
          <app-create-board-drawer
            [open]="animOpen()"
            [projectId]="config.projectId"
            (close)="requestClose()"
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

  protected renderedConfig = signal<DrawerConfig | null>(null);
  protected animOpen = signal(false);

  private closeTimer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    effect(() => {
      const config = this.drawerService.config();

      if (config) {
        if (this.closeTimer) {
          clearTimeout(this.closeTimer);
          this.closeTimer = null;
        }
        this.renderedConfig.set(config);
        requestAnimationFrame(() => this.animOpen.set(true));
      } else {
        this.animOpen.set(false);
        this.closeTimer = setTimeout(() => {
          this.renderedConfig.set(null);
          this.closeTimer = null;
        }, ANIMATION_DURATION);
      }
    });
  }

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

  private requestClose(): void {
    this.drawerService.close();
  }
}
