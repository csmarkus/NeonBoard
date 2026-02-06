import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from '../../components/button/button.component';
import { DrawerComponent } from '../../components/drawer/drawer.component';
import { ProjectService } from '../../services/project.service';
import { Project } from '../../models/project.model';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    ButtonComponent,
    DrawerComponent,
  ],
  templateUrl: './projects.component.html',
})
export class ProjectsComponent implements OnInit {
  private projectService = inject(ProjectService);
  private cdr = inject(ChangeDetectorRef);

  projects: Project[] = [];
  showCreateDrawer = false;
  newProjectName = '';
  newProjectDescription = '';
  error: string | null = null;
  isCreating = false;

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
    this.newProjectName = '';
    this.newProjectDescription = '';
  }

  closeCreateDrawer(): void {
    this.showCreateDrawer = false;
  }

  createProject(): void {
    if (!this.newProjectName.trim()) return;

    this.isCreating = true;
    this.error = null;

    this.projectService.createProject({
      name: this.newProjectName,
      description: this.newProjectDescription || ''
    }).subscribe({
      next: (project) => {
        this.projects.unshift(project);
        this.closeCreateDrawer();
        this.isCreating = false;
      },
      error: (err) => {
        console.error('Error creating project:', err);
        this.error = 'Failed to create project. Please try again.';
        this.isCreating = false;
      }
    });
  }

  deleteProject(project: Project): void {
    if (!confirm(`Are you sure you want to delete "${project.name}"?`)) {
      return;
    }

    this.error = null;

    this.projectService.deleteProject(project.id).subscribe({
      next: () => {
        this.projects = this.projects.filter(p => p.id !== project.id);
      },
      error: (err) => {
        console.error('Error deleting project:', err);
        this.error = 'Failed to delete project. Please try again.';
      }
    });
  }

  getRelativeTime(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffInMs = now.getTime() - date.getTime();
    const diffInMinutes = Math.floor(diffInMs / (1000 * 60));
    const diffInHours = Math.floor(diffInMs / (1000 * 60 * 60));
    const diffInDays = Math.floor(diffInMs / (1000 * 60 * 60 * 24));

    if (diffInMinutes < 1) return 'Just now';
    if (diffInMinutes < 60) return `${diffInMinutes} minute${diffInMinutes > 1 ? 's' : ''} ago`;
    if (diffInHours < 24) return `${diffInHours} hour${diffInHours > 1 ? 's' : ''} ago`;
    if (diffInDays === 1) return '1 day ago';
    if (diffInDays < 30) return `${diffInDays} days ago`;

    return date.toLocaleDateString();
  }

  // Color assignment based on project index (since we removed taskCount and color from API)
  getProjectColor(index: number): string {
    const colors = ['cyan', 'amber', 'violet', 'green'];
    return colors[index % colors.length];
  }

  getColorClass(color: string): string {
    const colorClasses: Record<string, string> = {
      cyan: 'bg-status-todo',
      amber: 'bg-status-progress',
      violet: 'bg-status-review',
      green: 'bg-status-done',
    };
    return colorClasses[color] || 'bg-status-todo';
  }
}
