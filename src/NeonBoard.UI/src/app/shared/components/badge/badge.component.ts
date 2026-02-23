import { Component, input, computed, ChangeDetectionStrategy } from '@angular/core';

type BadgeVariant = 'default' | 'cyan' | 'amber' | 'violet' | 'green';

@Component({
  selector: 'app-badge',
  templateUrl: './badge.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BadgeComponent {
  variant = input<BadgeVariant>('default');

  private variantStyles: Record<BadgeVariant, string> = {
    default: 'bg-surface-elevated text-secondary',
    cyan: 'bg-status-todo/15 text-status-todo',
    amber: 'bg-status-progress/15 text-status-progress',
    violet: 'bg-status-review/15 text-status-review',
    green: 'bg-status-done/15 text-status-done',
  };

  badgeClasses = computed(() =>
    `inline-flex items-center px-2 py-0.5 text-xs font-medium rounded-md ${this.variantStyles[this.variant()]}`
  );
}
