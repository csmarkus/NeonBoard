import { Component, input, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-error-banner',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (message()) {
      <div class="px-4 py-3 bg-red-500/10 border border-red-500/20 rounded-lg">
        <p class="text-sm text-red-400">{{ message() }}</p>
      </div>
    }
  `,
})
export class ErrorBannerComponent {
  message = input<string | null>(null);
}
