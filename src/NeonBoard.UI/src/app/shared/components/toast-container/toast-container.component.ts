import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faCircleCheck, faCircleExclamation } from '@fortawesome/free-solid-svg-icons';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  imports: [FontAwesomeModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="fixed bottom-4 right-4 z-[60] flex flex-col gap-2">
      @for (toast of toastService.toasts(); track toast.id) {
        <div
          class="flex items-center gap-3 px-4 py-3 rounded-lg shadow-lg bg-surface-elevated text-sm text-primary animate-slide-in-right border-l-4"
          [class.border-accent]="toast.type === 'success'"
          [class.border-danger]="toast.type === 'error'"
          role="status"
          aria-live="polite"
        >
          <fa-icon
            [icon]="toast.type === 'success' ? faCircleCheck : faCircleExclamation"
            [class.text-accent]="toast.type === 'success'"
            [class.text-danger]="toast.type === 'error'"
          ></fa-icon>
          <span>{{ toast.message }}</span>
        </div>
      }
    </div>
  `,
})
export class ToastContainerComponent {
  protected toastService = inject(ToastService);
  protected faCircleCheck = faCircleCheck;
  protected faCircleExclamation = faCircleExclamation;
}
