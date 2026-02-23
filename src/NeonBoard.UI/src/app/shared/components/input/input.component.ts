import { Component, input, computed, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-input',
  templateUrl: './input.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InputComponent {
  label = input<string>();
  placeholder = input('');
  type = input('text');
  error = input<string>();
  extraClass = input('');

  inputClasses = computed(() => {
    const base = 'w-full px-3 py-2 text-sm bg-surface text-primary placeholder:text-muted border border-dim rounded-lg hover:border-secondary/30 focus:outline-none focus:border-accent focus:ring-2 focus:ring-accent/20 transition-colors duration-150';
    const errorStyles = this.error() ? 'border-danger focus:border-danger focus:ring-danger/20' : '';
    return `${base} ${errorStyles} ${this.extraClass()}`;
  });
}
