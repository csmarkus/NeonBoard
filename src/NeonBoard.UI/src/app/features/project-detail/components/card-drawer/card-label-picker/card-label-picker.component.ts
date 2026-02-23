import { Component, input, output, computed, ChangeDetectionStrategy } from '@angular/core';
import { Label, getLabelClassString } from '../../../models/label.model';

@Component({
  selector: 'app-card-label-picker',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div>
      <label class="block text-sm font-medium text-secondary mb-2">Labels</label>

      <!-- Assigned labels -->
      <div class="flex flex-wrap gap-1.5 mb-2">
        @for (label of assignedLabels(); track label.id) {
          <button
            (click)="toggleLabel.emit(label.id)"
            [class]="'inline-flex items-center gap-1 px-2 py-1 text-xs font-medium rounded border cursor-pointer transition-opacity ' + getLabelClasses(label.color)"
            [disabled]="togglingLabelId() === label.id"
            [attr.aria-label]="'Remove label ' + label.name"
          >
            {{ label.name }}
            <svg class="w-3 h-3 opacity-60" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
            </svg>
          </button>
        }

        @if (!assignedLabels().length) {
          <span class="text-xs text-muted">No labels assigned</span>
        }
      </div>

      <!-- Toggle label picker -->
      <button
        (click)="togglePicker.emit()"
        class="text-xs text-muted hover:text-primary transition-colors"
      >
        {{ showPicker() ? 'Hide labels' : '+ Add label' }}
      </button>

      <!-- Label picker dropdown -->
      @if (showPicker()) {
        <div class="mt-2 bg-void border border-dim rounded-lg p-2 space-y-1">
          @for (label of sortedLabels(); track label.id) {
            <button
              (click)="toggleLabel.emit(label.id)"
              [disabled]="togglingLabelId() === label.id"
              class="w-full flex items-center gap-2 px-2 py-1.5 rounded-md hover:bg-surface transition-colors text-left"
              [attr.aria-label]="(isLabelAssigned(label.id) ? 'Remove' : 'Add') + ' label ' + label.name"
            >
              <div
                [class]="'w-4 h-4 rounded border flex items-center justify-center shrink-0 ' +
                  (isLabelAssigned(label.id)
                    ? 'bg-accent border-accent'
                    : 'border-dim')"
              >
                @if (isLabelAssigned(label.id)) {
                  <svg class="w-3 h-3 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                  </svg>
                }
              </div>
              <span
                [class]="'inline-flex items-center px-2 py-0.5 text-xs font-medium rounded border ' + getLabelClasses(label.color)"
              >{{ label.name }}</span>
            </button>
          }
        </div>
      }
    </div>
  `,
})
export class CardLabelPickerComponent {
  boardLabels = input.required<Label[]>();
  assignedLabelIds = input.required<string[]>();
  togglingLabelId = input<string | null>(null);
  showPicker = input(false);

  toggleLabel = output<string>();
  togglePicker = output<void>();

  assignedLabels = computed(() => {
    const ids = this.assignedLabelIds();
    return this.boardLabels()
      .filter(l => ids.includes(l.id))
      .sort((a, b) => a.name.localeCompare(b.name));
  });

  sortedLabels = computed(() => {
    return this.boardLabels().slice().sort((a, b) => a.name.localeCompare(b.name));
  });

  isLabelAssigned(labelId: string): boolean {
    return this.assignedLabelIds().includes(labelId);
  }

  getLabelClasses = getLabelClassString;
}
