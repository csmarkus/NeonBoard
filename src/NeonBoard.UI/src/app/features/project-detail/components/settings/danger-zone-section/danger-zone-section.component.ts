import { Component, inject, input, output, ChangeDetectionStrategy } from '@angular/core';
import { ButtonComponent } from '../../../../../shared/components/button/button.component';
import { SettingsSectionComponent } from '../../../../../shared/components/settings-section/settings-section.component';
import { ModalService } from '../../../../../core/services/modal.service';

@Component({
  selector: 'app-danger-zone-section',
  imports: [ButtonComponent, SettingsSectionComponent],
  templateUrl: './danger-zone-section.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DangerZoneSectionComponent {
  private modalService = inject(ModalService);

  entityName = input.required<string>();
  deleteDescription = input.required<string>();
  deleteMessage = input.required<string>();
  isDeleting = input.required<boolean>();

  delete = output<void>();

  async openDeleteModal(): Promise<void> {
    const confirmed = await this.modalService.confirm({
      title: `Delete ${this.entityName()}`,
      message: this.deleteMessage(),
      confirmText: `Delete ${this.entityName()}`,
    });
    if (confirmed) {
      this.delete.emit();
    }
  }
}
