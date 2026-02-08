import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalComponent } from '../modal/modal.component';
import { ButtonComponent } from '../button/button.component';
import { GradientVariant } from '../gradient-accent/gradient-accent.component';

@Component({
  selector: 'app-confirmation-modal',
  standalone: true,
  imports: [CommonModule, ModalComponent, ButtonComponent],
  templateUrl: './confirmation-modal.component.html',
})
export class ConfirmationModalComponent {
  open = input.required<boolean>();
  title = input<string>('Confirm Action');
  message = input<string>('Are you sure you want to proceed?');
  confirmText = input<string>('Confirm');
  cancelText = input<string>('Cancel');
  variant = input<'danger' | 'primary'>('danger');
  gradientVariant = input<GradientVariant>('pink');

  confirm = output<void>();
  cancel = output<void>();

  onConfirm(): void {
    this.confirm.emit();
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
