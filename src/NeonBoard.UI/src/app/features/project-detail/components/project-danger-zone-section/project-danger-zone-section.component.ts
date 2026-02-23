import { Component, input, output, signal, ChangeDetectionStrategy } from '@angular/core';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';

@Component({
  selector: 'app-project-danger-zone-section',
  imports: [ButtonComponent, ConfirmationModalComponent],
  templateUrl: './project-danger-zone-section.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectDangerZoneSectionComponent {
  isDeleting = input.required<boolean>();

  deleteProject = output<void>();

  showDeleteModal = signal(false);

  openDeleteModal(): void {
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
  }

  onConfirmDelete(): void {
    this.showDeleteModal.set(false);
    this.deleteProject.emit();
  }
}
