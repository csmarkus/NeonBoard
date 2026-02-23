import { Component, inject, OnInit, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { ErrorBannerComponent } from '../../../../shared/components/error-banner/error-banner.component';
import { UserMenuComponent } from '../../../../layout/user-menu/user-menu.component';
import { ProjectCardComponent } from '../../components/project-card/project-card.component';
import { CreateProjectDrawerComponent } from '../../components/create-project-drawer/create-project-drawer.component';
import { ProjectService } from '../../services/project.service';
import { LoadingService } from '../../../../core/services/loading.service';
import { Project } from '../../models/project.model';

@Component({
  selector: 'app-projects',
  imports: [
    CommonModule,
    ButtonComponent,
    ErrorBannerComponent,
    UserMenuComponent,
    ProjectCardComponent,
    CreateProjectDrawerComponent,
  ],
  templateUrl: './projects.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectsComponent implements OnInit {
  private projectService = inject(ProjectService);
  private loadingService = inject(LoadingService);

  projects = signal<Project[]>([]);
  showCreateDrawer = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.error.set(null);
    this.loadingService.show();

    this.projectService.getProjects().subscribe({
      next: (projects) => {
        this.projects.set(projects);
        this.loadingService.hide();
      },
      error: (err) => {
        console.error('Error loading projects:', err);
        this.error.set('Failed to load projects. Please try again.');
        this.loadingService.hide();
      }
    });
  }

  openCreateDrawer(): void {
    this.showCreateDrawer.set(true);
  }

  closeCreateDrawer(): void {
    this.showCreateDrawer.set(false);
  }

  onProjectCreated(project: Project): void {
    this.projects.update(list => [project, ...list]);
  }

  onProjectDelete(project: Project): void {
    this.error.set(null);

    this.projectService.deleteProject(project.id).subscribe({
      next: () => {
        this.projects.update(list => list.filter(p => p.id !== project.id));
      },
      error: (err) => {
        console.error('Error deleting project:', err);
        this.error.set('Failed to delete project. Please try again.');
      }
    });
  }
}
