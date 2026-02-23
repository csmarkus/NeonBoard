import { Component, input, ChangeDetectionStrategy } from '@angular/core';

type SettingsSectionVariant = 'default' | 'danger';

@Component({
  selector: 'app-settings-section',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="grid grid-cols-[220px_1fr] gap-12">
      <div>
        <h2 [class]="'text-base font-semibold ' + (variant() === 'danger' ? 'text-red-400' : 'text-primary')">
          {{ title() }}
        </h2>
        <p class="text-sm text-muted mt-1">{{ description() }}</p>
      </div>
      <div>
        <ng-content />
      </div>
    </section>
  `,
})
export class SettingsSectionComponent {
  title = input.required<string>();
  description = input.required<string>();
  variant = input<SettingsSectionVariant>('default');
}
