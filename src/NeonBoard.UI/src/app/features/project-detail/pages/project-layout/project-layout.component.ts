import { Component, inject, signal, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, RouterOutlet, Router, NavigationStart, NavigationEnd, NavigationCancel, NavigationError } from '@angular/router';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';
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
      <div class="flex-1 flex flex-col min-w-0 relative">
        <!-- Loading overlay -->
        @if (isNavigating()) {
          <div class="absolute inset-0 bg-void z-[100] flex items-center justify-center">
            <div class="inline-block w-8 h-8 border-4 border-accent/20 border-t-accent rounded-full animate-spin"></div>
          </div>
        }

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
  private router = inject(Router);
  private boardService = inject(BoardService);

  protected drawerService = inject(DrawerService);

  projectId = signal<string>('');
  isNavigating = signal<boolean>(false);

  ngOnInit(): void {
    // Set projectId once on init
    const id = this.route.snapshot.paramMap.get('projectId');
    if (id) {
      this.projectId.set(id);
    }

    // Listen to router events for loading state - only for child routes
    let lastNavigatingState = false;
    this.router.events.pipe(
      filter(event =>
        event instanceof NavigationStart ||
        event instanceof NavigationEnd ||
        event instanceof NavigationCancel ||
        event instanceof NavigationError
      )
    ).subscribe(event => {
      if (event instanceof NavigationStart) {
        // Only show loading for child route navigations (within the same project)
        const currentProjectId = this.route.snapshot.paramMap.get('projectId');
        if (event.url.includes(`/project/${currentProjectId}`) && !lastNavigatingState) {
          lastNavigatingState = true;
          this.isNavigating.set(true);
        }
      } else if (event instanceof NavigationEnd || event instanceof NavigationCancel || event instanceof NavigationError) {
        if (lastNavigatingState) {
          lastNavigatingState = false;
          this.isNavigating.set(false);
        }
      }
    });
  }

  onBoardCreated(): void {
    this.drawerService.closeCreateBoardDrawer();
    this.drawerService.notifyBoardCreated();
  }

  onCardUpdated(): void {
    this.drawerService.closeCardDrawer();
    this.drawerService.notifyCardUpdated();
  }

  onCardDeleted(): void {
    this.drawerService.closeCardDrawer();
    this.drawerService.notifyCardDeleted();
  }
}
