import { Component, input, output } from '@angular/core';
import { GradientAccentComponent, GradientVariant } from '../gradient-accent/gradient-accent.component';

@Component({
  selector: 'app-modal',
  imports: [GradientAccentComponent],
  templateUrl: './modal.component.html',
  host: {
    '(document:keydown.escape)': 'onEscapeKey()',
  },
})
export class ModalComponent {
  open = input.required<boolean>();
  closeOnBackdrop = input<boolean>(true);
  gradientVariant = input<GradientVariant>('cyan');

  close = output<void>();

  onBackdropClick(event: MouseEvent): void {
    if (this.closeOnBackdrop() && event.target === event.currentTarget) {
      this.close.emit();
    }
  }

  onEscapeKey(): void {
    if (this.open()) {
      this.close.emit();
    }
  }
}
