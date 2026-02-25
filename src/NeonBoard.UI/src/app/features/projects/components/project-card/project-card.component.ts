import { Component, inject, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faTrashCan } from '@fortawesome/free-solid-svg-icons';
import { Project } from '../../models/project.model';
import { GradientAccentComponent } from '../../../../shared/components/gradient-accent/gradient-accent.component';
import { ModalService } from '../../../../core/services/modal.service';

@Component({
  selector: 'app-project-card',
  imports: [CommonModule, RouterLink, FontAwesomeModule, GradientAccentComponent],
  templateUrl: './project-card.component.html',
})
export class ProjectCardComponent {
  private modalService = inject(ModalService);

  faTrashCan = faTrashCan;

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

  async onDelete(event: Event): Promise<void> {
    event.stopPropagation();
    event.preventDefault();
    const confirmed = await this.modalService.confirm({
      title: 'Delete Project',
      message: `Are you sure you want to delete "${this.project.name}"? All boards, columns, and cards will be permanently removed.`,
      confirmText: 'Delete',
    });
    if (confirmed) {
      this.delete.emit(this.project);
    }
  }
}
