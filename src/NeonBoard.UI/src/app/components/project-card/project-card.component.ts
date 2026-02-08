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

  getProjectGradient(index: number): string {
    const gradients = [
      'linear-gradient(90deg, rgba(9,9,11,0) 0%, rgba(94,240,255,0.75) 28%, rgba(94,240,255,0.32) 52%, rgba(94,240,255,0) 82%)',
      'linear-gradient(90deg, rgba(9,9,11,0) 0%, rgba(255,184,99,0.65) 30%, rgba(255,107,156,0.35) 60%, rgba(255,184,99,0) 90%)',
      'linear-gradient(90deg, rgba(9,9,11,0) 0%, rgba(167,139,250,0.65) 24%, rgba(34,211,238,0.45) 52%, rgba(34,211,238,0) 85%)',
      'linear-gradient(90deg, rgba(9,9,11,0) 0%, rgba(255,58,191,0.7) 26%, rgba(255,138,76,0.4) 54%, rgba(255,58,191,0) 86%)',
    ];
    return gradients[index % gradients.length];
  }

  onDelete(event: Event): void {
    event.stopPropagation();
    event.preventDefault();
    if (confirm(`Are you sure you want to delete "${this.project.name}"?`)) {
      this.delete.emit(this.project);
    }
  }
}
