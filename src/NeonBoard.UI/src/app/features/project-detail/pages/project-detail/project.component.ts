import { Component, inject, signal, computed, effect, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faTriangleExclamation, faTableColumns } from '@fortawesome/free-solid-svg-icons';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { DrawerService } from '../../services/drawer.service';
import { ProjectContext } from '../../services/project-context.service';
import { BoardCardComponent } from '../../components/project/board-card/board-card.component';

@Component({
  selector: 'app-project',
  imports: [
    CommonModule,
    FontAwesomeModule,
    ButtonComponent,
    BoardCardComponent,
  ],
  host: {
    class: 'flex flex-col h-full'
  },
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './project.component.html',
  styleUrl: './project.component.css',
})
export class ProjectComponent {
  private projectContext = inject(ProjectContext);
  private drawerService = inject(DrawerService);
  private titleService = inject(Title);
  private destroyRef = inject(DestroyRef);

  faTriangleExclamation = faTriangleExclamation;
  faTableColumns = faTableColumns;

  shortId = this.projectContext.shortId;
  projectId = this.projectContext.projectId;
  project = this.projectContext.project;
  boards = this.projectContext.boards;
  isLoading = computed(() => !this.projectContext.boardsLoaded());
  error = signal<string | null>(null);

  constructor() {
    effect(() => {
      const project = this.projectContext.project();
      if (project) {
        this.titleService.setTitle(`${project.name} | NeonBoard`);
      }
    });

    this.drawerService.boardCreated$.pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.projectContext.reloadBoards();
    });
  }

  openCreateBoardDrawer(): void {
    this.drawerService.openCreateBoardDrawer(this.projectId());
  }
}
