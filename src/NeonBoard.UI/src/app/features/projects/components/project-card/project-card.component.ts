import { Component, Input, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Project } from '../../models/project.model';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { GradientAccentComponent } from '../../../../shared/components/gradient-accent/gradient-accent.component';

@Component({
  selector: 'app-project-card',
  standalone: true,
  imports: [CommonModule, RouterLink, ConfirmationModalComponent, GradientAccentComponent],
  templateUrl: './project-card.component.html',
})
export class ProjectCardComponent {
  @Input({ required: true }) project!: Project;
  @Input({ required: true }) index!: number;
  @Output() delete = new EventEmitter<Project>();

  showDeleteConfirmation = signal(false);

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

  onDelete(event: Event): void {
    event.stopPropagation();
    event.preventDefault();
    this.showDeleteConfirmation.set(true);
  }

  onConfirmDelete(): void {
    this.showDeleteConfirmation.set(false);
    this.delete.emit(this.project);
  }

  onCancelDelete(): void {
    this.showDeleteConfirmation.set(false);
  }
}
