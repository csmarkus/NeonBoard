import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

type ColumnAccent = 'cyan' | 'amber' | 'violet' | 'green';

@Component({
  selector: 'app-column',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './column.component.html',
})
export class ColumnComponent {
  @Input() title = '';
  @Input() count?: number;
  @Input() accent: ColumnAccent = 'cyan';

  private accentStyles: Record<ColumnAccent, string> = {
    cyan: 'bg-status-todo',
    amber: 'bg-status-progress',
    violet: 'bg-status-review',
    green: 'bg-status-done',
  };

  get dotClasses(): string {
    return `w-2 h-2 rounded-full ${this.accentStyles[this.accent]}`;
  }
}
