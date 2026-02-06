import { Component, inject, afterNextRender } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from '../../components/button/button.component';
import { DrawerComponent } from '../../components/drawer/drawer.component';
import { LoadingService } from '../../services/loading.service';

interface Project {
  id: string;
  name: string;
  description?: string;
  color: string;
  taskCount: number;
  updatedAt: string;
}

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
export class ProjectsComponent {
  private loadingService = inject(LoadingService);

  projects: Project[] = [
    {
      id: 'project-alpha',
      name: 'Project Alpha',
      description: 'Main product development and feature work',
      color: 'cyan',
      taskCount: 12,
      updatedAt: '2 hours ago',
    },
    {
      id: 'project-beta',
      name: 'Marketing Site',
      description: 'Company website redesign project',
      color: 'violet',
      taskCount: 8,
      updatedAt: '1 day ago',
    },
    {
      id: 'project-gamma',
      name: 'Mobile App',
      description: 'iOS and Android application',
      color: 'amber',
      taskCount: 24,
      updatedAt: '3 days ago',
    },
  ];

  showCreateDrawer = false;
  newProjectName = '';
  newProjectDescription = '';

  private colorClasses: Record<string, string> = {
    cyan: 'bg-status-todo',
    amber: 'bg-status-progress',
    violet: 'bg-status-review',
    green: 'bg-status-done',
  };

  constructor() {
    // Hide loading after the component is fully rendered
    afterNextRender(() => {
      this.loadingService.hide();
    });
  }

  getColorClass(color: string): string {
    return this.colorClasses[color] || 'bg-status-todo';
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

    const newProject: Project = {
      id: `project-${Date.now()}`,
      name: this.newProjectName,
      description: this.newProjectDescription || undefined,
      color: ['cyan', 'amber', 'violet', 'green'][Math.floor(Math.random() * 4)],
      taskCount: 0,
      updatedAt: 'Just now',
    };

    this.projects.unshift(newProject);
    this.closeCreateDrawer();
  }
}
