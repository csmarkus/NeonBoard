import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Project } from '../../models/project.model';

@Component({
  selector: 'app-project-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './project-card.component.html',
})
export class ProjectCardComponent {
  @Input({ required: true }) project!: Project;
  @Input({ required: true }) index!: number;
  @Output() delete = new EventEmitter<Project>();

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

  onDelete(event: Event): void {
    event.stopPropagation();
    event.preventDefault();
    if (confirm(`Are you sure you want to delete "${this.project.name}"?`)) {
      this.delete.emit(this.project);
    }
  }
}
