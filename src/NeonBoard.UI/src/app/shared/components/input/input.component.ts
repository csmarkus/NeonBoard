import { Component, input, computed, ChangeDetectionStrategy, model } from '@angular/core';
import { FormValueControl, ValidationError } from '@angular/forms/signals';

@Component({
  selector: 'app-input',
  templateUrl: './input.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InputComponent implements FormValueControl<string> {
  label = input<string>();
  placeholder = input('');
  type = input('text');
  error = input<string>();
  extraClass = input('');
  element = input<'input' | 'textarea'>('input');
  rows = input(3);
  resize = input<'none' | 'y'>('none');
  maxlength = input<number>();
  inputId = input<string>();

  // FormValueControl protocol
  readonly value = model('');
  readonly touched = model(false);
  readonly disabled = input(false);
  readonly invalid = input(false);
  readonly errors = input<readonly ValidationError[]>([]);

  hasError = computed(() => !!this.error() || (this.touched() && this.invalid()));

  inputClasses = computed(() => {
    const base = 'w-full px-3 py-2 text-sm bg-surface text-primary placeholder:text-muted border border-dim rounded-lg hover:border-secondary/30 focus:outline-none focus:border-accent focus:ring-2 focus:ring-accent/20 transition-colors duration-150';
    const errorStyles = this.hasError() ? 'border-danger focus:border-danger focus:ring-danger/20' : '';
    const resizeStyle = this.element() === 'textarea' ? (this.resize() === 'y' ? 'resize-y' : 'resize-none') : '';
    return `${base} ${errorStyles} ${resizeStyle} ${this.extraClass()}`.trim();
  });

  onInput(event: Event): void {
    const target = event.target as HTMLInputElement | HTMLTextAreaElement;
    this.value.set(target.value);
  }

  onBlur(): void {
    this.touched.set(true);
  }
}
