import { Component, input, output, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GradientAccentComponent, GradientVariant } from '../gradient-accent/gradient-accent.component';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule, GradientAccentComponent],
  templateUrl: './modal.component.html',
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

  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    if (this.open()) {
      this.close.emit();
    }
  }
}
