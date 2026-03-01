import { Component, inject, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faTrashCan } from '@fortawesome/free-solid-svg-icons';
import { Project } from '../../models/project.model';
import { GradientAccentComponent } from '../../../../shared/components/gradient-accent/gradient-accent.component';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time.pipe';
import { ModalService } from '../../../../core/services/modal.service';

@Component({
  selector: 'app-project-card',
  imports: [CommonModule, RouterLink, FontAwesomeModule, GradientAccentComponent, RelativeTimePipe],
  templateUrl: './project-card.component.html',
})
export class ProjectCardComponent {
  private modalService = inject(ModalService);

  faTrashCan = faTrashCan;

  @Input({ required: true }) project!: Project;
  @Input({ required: true }) index!: number;
  @Output() delete = new EventEmitter<Project>();

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
