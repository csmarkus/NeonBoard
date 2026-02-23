import { Component, input, output, signal, ChangeDetectionStrategy } from '@angular/core';
import { ButtonComponent } from '../../../../../shared/components/button/button.component';
import { ConfirmationModalComponent } from '../../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { SettingsSectionComponent } from '../../../../../shared/components/settings-section/settings-section.component';

@Component({
  selector: 'app-danger-zone-section',
  imports: [ButtonComponent, ConfirmationModalComponent, SettingsSectionComponent],
  templateUrl: './danger-zone-section.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DangerZoneSectionComponent {
  entityName = input.required<string>();
  deleteDescription = input.required<string>();
  deleteMessage = input.required<string>();
  isDeleting = input.required<boolean>();

  delete = output<void>();

  showDeleteModal = signal(false);

  openDeleteModal(): void {
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
  }

  onConfirmDelete(): void {
    this.showDeleteModal.set(false);
    this.delete.emit();
  }
}
