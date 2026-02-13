import { Component, inject, signal, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SidebarComponent } from '../../../../layout/sidebar/sidebar.component';
import { CreateBoardDrawerComponent } from '../../components/create-board-drawer/create-board-drawer.component';
import { CardDrawerComponent } from '../../components/card-drawer/card-drawer.component';
import { DrawerService } from '../../services/drawer.service';
import { BoardService } from '../../services/board.service';

@Component({
  selector: 'app-project-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, SidebarComponent, CreateBoardDrawerComponent, CardDrawerComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block h-screen'
  },
  template: `
    <div class="h-full bg-void flex">
      <!-- Sidebar -->
      <app-sidebar [projectId]="projectId()"></app-sidebar>

      <!-- Content area (router outlet) -->
      <div class="flex-1 flex flex-col min-w-0">
        <router-outlet></router-outlet>
      </div>
    </div>

    <!-- Drawers (persisted across routes) -->
    <app-create-board-drawer
      [open]="drawerService.showCreateBoardDrawer()"
      [projectId]="drawerService.createBoardProjectId() ?? ''"
      (close)="drawerService.closeCreateBoardDrawer()"
      (boardCreated)="onBoardCreated()">
    </app-create-board-drawer>

    <app-card-drawer
      [open]="drawerService.selectedCard() !== null"
      [projectId]="drawerService.cardDrawerProjectId()"
      [boardId]="drawerService.cardDrawerBoardId()"
      [columnId]="drawerService.selectedCard()?.columnId ?? null"
      [card]="drawerService.selectedCard()"
      (close)="drawerService.closeCardDrawer()"
      (cardSaved)="onCardUpdated()"
      (cardDeleted)="onCardDeleted()">
    </app-card-drawer>
  `
})
export class ProjectLayoutComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private boardService = inject(BoardService);

  protected drawerService = inject(DrawerService);

  projectId = signal<string>('');

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('projectId');
    if (id) {
      this.projectId.set(id);
    }
  }

  onBoardCreated(): void {
    this.drawerService.closeCreateBoardDrawer();
    this.drawerService.notifyBoardCreated();
  }

  onCardUpdated(): void {
    this.drawerService.notifyCardUpdated();
  }

  onCardDeleted(): void {
    this.drawerService.closeCardDrawer();
    this.drawerService.notifyCardDeleted();
  }
}
