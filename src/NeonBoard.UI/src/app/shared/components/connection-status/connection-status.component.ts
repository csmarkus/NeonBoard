import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { SignalRService } from '../../../core/services/signalr.service';

@Component({
  selector: 'app-connection-status',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (showIndicator()) {
      <div class="flex items-center gap-1.5 text-xs" [attr.aria-live]="'polite'">
        <span
          class="inline-block h-2 w-2 rounded-full"
          [class]="dotClass()"
        ></span>
        <span [class]="textClass()">{{ statusText() }}</span>
      </div>
    }
  `,
})
export class ConnectionStatusComponent {
  private signalR = inject(SignalRService);

  protected showIndicator = computed(() => {
    const state = this.signalR.connectionState();
    return state === 'reconnecting' || state === 'disconnected';
  });

  protected statusText = computed(() => {
    const state = this.signalR.connectionState();
    if (state === 'reconnecting') return 'Reconnecting...';
    if (state === 'disconnected') return 'Offline';
    return '';
  });

  protected dotClass = computed(() => {
    const state = this.signalR.connectionState();
    if (state === 'reconnecting') return 'bg-amber-400 animate-pulse';
    if (state === 'disconnected') return 'bg-red-500';
    return '';
  });

  protected textClass = computed(() => {
    const state = this.signalR.connectionState();
    if (state === 'reconnecting') return 'text-amber-400';
    if (state === 'disconnected') return 'text-red-400';
    return '';
  });
}
