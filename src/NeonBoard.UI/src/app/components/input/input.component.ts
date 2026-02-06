import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-input',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './input.component.html',
})
export class InputComponent {
  @Input() label?: string;
  @Input() placeholder = '';
  @Input() type = 'text';
  @Input() error?: string;
  @Input() class = '';

  get inputClasses(): string {
    const base = 'w-full px-3 py-2 text-[13px] bg-surface text-primary placeholder:text-muted border border-dim rounded-lg hover:border-secondary/30 focus:outline-none focus:border-accent focus:ring-2 focus:ring-accent/20 transition-colors duration-150';
    const errorStyles = this.error ? 'border-danger focus:border-danger focus:ring-danger/20' : '';
    return `${base} ${errorStyles} ${this.class}`;
  }
}
