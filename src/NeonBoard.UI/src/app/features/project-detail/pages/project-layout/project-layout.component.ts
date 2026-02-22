import { Component, inject, signal, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SidebarComponent } from '../../../../layout/sidebar/sidebar.component';
import { CreateBoardDrawerComponent } from '../../components/create-board-drawer/create-board-drawer.component';
import { CardDrawerComponent } from '../../components/card-drawer/card-drawer.component';
import { DrawerService } from '../../services/drawer.service';
import { ProjectService } from '../../../projects/services/project.service';

@Component({
  selector: 'app-project-layout',
  imports: [CommonModule, RouterOutlet, SidebarComponent, CreateBoardDrawerComponent, CardDrawerComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block h-screen'
  },
  template: `
    <div class="h-full bg-void flex">
      <app-sidebar [projectId]="projectId()" [shortId]="shortId()"></app-sidebar>
      <div class="flex-1 flex flex-col min-w-0">
        <router-outlet></router-outlet>
      </div>
    </div>

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
  private projectService = inject(ProjectService);

  protected drawerService = inject(DrawerService);

  shortId = signal<string>('');
  projectId = signal<string>('');

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('shortId');
    if (id) {
      this.shortId.set(id);
      this.projectService.getProjectByShortId(id).subscribe({
        next: (project) => this.projectId.set(project.id),
      });
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
