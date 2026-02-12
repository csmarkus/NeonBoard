import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger';
type ButtonSize = 'sm' | 'md';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './button.component.html',
})
export class ButtonComponent {
  @Input() variant: ButtonVariant = 'secondary';
  @Input() size: ButtonSize = 'md';
  @Input() disabled = false;

  private variantStyles: Record<ButtonVariant, string> = {
    primary: 'bg-accent text-void font-medium hover:bg-accent/90 focus:outline-none focus:ring-2 focus:ring-accent/50 focus:ring-offset-2 focus:ring-offset-void',
    secondary: 'bg-surface text-primary border border-dim hover:bg-surface-elevated focus:outline-none focus:ring-2 focus:ring-dim focus:ring-offset-2 focus:ring-offset-void',
    ghost: 'text-muted hover:text-primary hover:bg-surface focus:outline-none focus:ring-2 focus:ring-dim',
    danger: 'bg-danger/10 text-danger border border-danger/20 hover:bg-danger/20 focus:outline-none focus:ring-2 focus:ring-danger/50 focus:ring-offset-2 focus:ring-offset-void',
  };

  private sizeStyles: Record<ButtonSize, string> = {
    sm: 'px-3 py-1.5 text-xs',
    md: 'px-4 py-2 text-sm',
  };

  get buttonClasses(): string {
    return `inline-flex items-center justify-center gap-2 rounded-lg transition-colors duration-150 disabled:opacity-50 disabled:cursor-not-allowed ${this.variantStyles[this.variant]} ${this.sizeStyles[this.size]}`;
  }
}
