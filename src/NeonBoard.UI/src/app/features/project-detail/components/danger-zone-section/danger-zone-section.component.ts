import { Component, input, output, signal } from '@angular/core';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';

@Component({
  selector: 'app-danger-zone-section',
  imports: [ButtonComponent, ConfirmationModalComponent],
  templateUrl: './danger-zone-section.component.html',
})
export class DangerZoneSectionComponent {
  isDeleting = input.required<boolean>();

  deleteBoard = output<void>();

  showDeleteModal = signal(false);

  openDeleteModal(): void {
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
  }

  onConfirmDelete(): void {
    this.showDeleteModal.set(false);
    this.deleteBoard.emit();
  }
}
