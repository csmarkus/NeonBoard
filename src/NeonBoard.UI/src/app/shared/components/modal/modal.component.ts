import { Component, input, output } from '@angular/core';
import { GradientAccentComponent, GradientVariant } from '../gradient-accent/gradient-accent.component';

@Component({
  selector: 'app-modal',
  imports: [GradientAccentComponent],
  host: {
    '(document:keydown.escape)': 'onEscapeKey()',
  },
  template: `
    @if (open()) {
      <div class="fixed inset-0 z-50 flex items-center justify-center">
        <!-- Backdrop -->
        <div
          class="fixed inset-0 bg-void/80 backdrop-blur-sm transition-opacity"
          (click)="onBackdropClick($event)"
        ></div>

        <!-- Modal Container -->
        <div
          class="relative bg-surface border border-subtle rounded-lg shadow-2xl max-w-lg w-full mx-4 transform transition-all overflow-hidden"
          role="dialog"
          aria-modal="true"
        >
          <app-gradient-accent [variant]="gradientVariant()"></app-gradient-accent>
          <ng-content></ng-content>
        </div>
      </div>
    }
  `,
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
