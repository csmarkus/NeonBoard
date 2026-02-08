import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

export type GradientVariant = 'cyan' | 'orange' | 'violet' | 'pink' | number;

@Component({
  selector: 'app-gradient-accent',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="absolute inset-x-0 top-0"
      [style.height.px]="height()"
      [style.background-image]="gradient()"
    ></div>
  `,
})
export class GradientAccentComponent {
  variant = input<GradientVariant>(0);
  height = input<number>(2);

  private gradients: Record<string, string> = {
    cyan: 'linear-gradient(90deg, rgba(9,9,11,0) 0%, rgba(94,240,255,0.75) 28%, rgba(94,240,255,0.32) 52%, rgba(94,240,255,0) 82%)',
    orange: 'linear-gradient(90deg, rgba(9,9,11,0) 0%, rgba(255,184,99,0.65) 30%, rgba(255,107,156,0.35) 60%, rgba(255,184,99,0) 90%)',
    violet: 'linear-gradient(90deg, rgba(9,9,11,0) 0%, rgba(167,139,250,0.65) 24%, rgba(34,211,238,0.45) 52%, rgba(34,211,238,0) 85%)',
    pink: 'linear-gradient(90deg, rgba(9,9,11,0) 0%, rgba(255,58,191,0.7) 26%, rgba(255,138,76,0.4) 54%, rgba(255,58,191,0) 86%)',
  };

  private variantOrder: GradientVariant[] = ['cyan', 'orange', 'violet', 'pink'];

  gradient = computed(() => {
    const v = this.variant();

    if (typeof v === 'number') {
      const key = this.variantOrder[v % this.variantOrder.length];
      return this.gradients[key as string];
    }

    return this.gradients[v];
  });
}
