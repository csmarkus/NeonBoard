import { Component, input, computed, ChangeDetectionStrategy, forwardRef, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-input',
  templateUrl: './input.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputComponent),
      multi: true,
    },
  ],
})
export class InputComponent implements ControlValueAccessor {
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

  value = signal('');
  isDisabled = signal(false);

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  inputClasses = computed(() => {
    const base = 'w-full px-3 py-2 text-sm bg-surface text-primary placeholder:text-muted border border-dim rounded-lg hover:border-secondary/30 focus:outline-none focus:border-accent focus:ring-2 focus:ring-accent/20 transition-colors duration-150';
    const errorStyles = this.error() ? 'border-danger focus:border-danger focus:ring-danger/20' : '';
    const resizeStyle = this.element() === 'textarea' ? (this.resize() === 'y' ? 'resize-y' : 'resize-none') : '';
    return `${base} ${errorStyles} ${resizeStyle} ${this.extraClass()}`.trim();
  });

  writeValue(value: string): void {
    this.value.set(value ?? '');
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isDisabled.set(isDisabled);
  }

  onInput(event: Event): void {
    const target = event.target as HTMLInputElement | HTMLTextAreaElement;
    this.value.set(target.value);
    this.onChange(target.value);
  }

  onBlur(): void {
    this.onTouched();
  }
}
