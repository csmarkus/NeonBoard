import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskStatus } from '../../models/task.model';

type TagColor = 'cyan' | 'amber' | 'violet' | 'green' | 'red' | 'neutral';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './card.component.html',
})
export class CardComponent {
  @Input() title = '';
  @Input() description?: string;
  @Input() status: TaskStatus | 'default' = 'default';
  @Input() tags: string[] = [];
  @Input() assignee?: string;
  @Output() cardClick = new EventEmitter<void>();

  private statusStyles: Record<string, string> = {
    default: '',
    todo: 'status-todo',
    'in-progress': 'status-progress',
    review: 'status-review',
    done: 'status-done',
  };

  private tagColorStyles: Record<TagColor, string> = {
    cyan: 'bg-status-todo/15 text-status-todo',
    amber: 'bg-status-progress/15 text-status-progress',
    violet: 'bg-status-review/15 text-status-review',
    green: 'bg-status-done/15 text-status-done',
    red: 'bg-danger/15 text-danger',
    neutral: 'bg-surface-elevated text-secondary',
  };

  private tagColorMap: Record<string, TagColor> = {
    feature: 'cyan',
    enhancement: 'cyan',
    ui: 'cyan',
    ux: 'cyan',
    bug: 'red',
    critical: 'red',
    hotfix: 'red',
    docs: 'violet',
    documentation: 'violet',
    refactor: 'violet',
    a11y: 'violet',
    devops: 'amber',
    infrastructure: 'amber',
    config: 'amber',
    done: 'green',
    shipped: 'green',
    released: 'green',
  };

  get cardClasses(): string {
    return `bg-surface rounded-lg p-3.5 border border-subtle hover:border-dim hover:bg-surface-elevated transition-colors duration-150 cursor-pointer ${this.statusStyles[this.status]}`;
  }

  getTagClasses(tag: string): string {
    const color = this.tagColorMap[tag.toLowerCase()] || 'neutral';
    return `px-2 py-0.5 text-xs rounded-md ${this.tagColorStyles[color]}`;
  }
}
