import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '../../components/button/button.component';
import { UserMenuComponent } from '../../components/user-menu/user-menu.component';
import { ProjectCardComponent } from '../../components/project-card/project-card.component';
import { CreateProjectDrawerComponent } from '../../components/create-project-drawer/create-project-drawer.component';
import { ProjectService } from '../../services/project.service';
import { Project } from '../../models/project.model';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [
    CommonModule,
    ButtonComponent,
    UserMenuComponent,
    ProjectCardComponent,
    CreateProjectDrawerComponent,
  ],
  templateUrl: './projects.component.html',
})
export class ProjectsComponent implements OnInit {
  private projectService = inject(ProjectService);
  private cdr = inject(ChangeDetectorRef);

  projects: Project[] = [];
  showCreateDrawer = false;
  error: string | null = null;

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.error = null;

    this.projectService.getProjects().subscribe({
      next: (projects) => {
        this.projects = projects;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading projects:', err);
        this.error = 'Failed to load projects. Please try again.';
        this.cdr.detectChanges();
      }
    });
  }

  openCreateDrawer(): void {
    this.showCreateDrawer = true;
  }

  closeCreateDrawer(): void {
    this.showCreateDrawer = false;
  }

  onProjectCreated(project: Project): void {
    this.projects.unshift(project);
    this.cdr.detectChanges();
  }

  onProjectDelete(project: Project): void {
    this.error = null;

    this.projectService.deleteProject(project.id).subscribe({
      next: () => {
        this.projects = this.projects.filter(p => p.id !== project.id);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error deleting project:', err);
        this.error = 'Failed to delete project. Please try again.';
        this.cdr.detectChanges();
      }
    });
  }
}
