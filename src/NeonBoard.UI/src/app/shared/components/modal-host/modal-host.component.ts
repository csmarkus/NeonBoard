import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { ModalService } from '../../../core/services/modal.service';
import { ConfirmationModalComponent } from '../confirmation-modal/confirmation-modal.component';

@Component({
  selector: 'app-modal-host',
  imports: [ConfirmationModalComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (modalService.config(); as config) {
      <app-confirmation-modal
        [open]="true"
        [title]="config.title"
        [message]="config.message"
        [confirmText]="config.confirmText"
        [cancelText]="config.cancelText"
        [variant]="config.variant"
        [gradientVariant]="config.gradientVariant"
        (confirm)="modalService.resolve(true)"
        (cancel)="modalService.resolve(false)"
      />
    }
  `,
})
export class ModalHostComponent {
  protected modalService = inject(ModalService);
}
