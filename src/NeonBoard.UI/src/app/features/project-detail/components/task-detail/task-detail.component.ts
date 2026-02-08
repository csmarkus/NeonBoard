import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Task } from '../../models/task.model';
import { BadgeComponent } from '../../../../shared/components/badge/badge.component';
import { ButtonComponent } from '../../../../shared/components/button/button.component';

type TagColor = 'cyan' | 'amber' | 'violet' | 'green' | 'red' | 'neutral';

@Component({
  selector: 'app-task-detail',
  standalone: true,
  imports: [CommonModule, BadgeComponent, ButtonComponent],
  templateUrl: './task-detail.component.html',
})
export class TaskDetailComponent {
  @Input() task!: Task;

  private priorityStyles = {
    low: 'bg-surface-elevated text-secondary',
    medium: 'bg-status-progress/15 text-status-progress',
    high: 'bg-danger/15 text-danger',
  };

  private statusLabels = {
    todo: 'Backlog',
    'in-progress': 'In Progress',
    review: 'Review',
    done: 'Done',
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

  getStatusVariant(): 'cyan' | 'amber' | 'violet' | 'green' {
    const map = { todo: 'cyan', 'in-progress': 'amber', review: 'violet', done: 'green' } as const;
    return map[this.task.status];
  }

  getStatusLabel(): string {
    return this.statusLabels[this.task.status];
  }

  getPriorityClasses(): string {
    return `px-2 py-0.5 text-[11px] rounded-md ${this.priorityStyles[this.task.priority!]}`;
  }

  getTagClasses(tag: string): string {
    const color = this.tagColorMap[tag.toLowerCase()] || 'neutral';
    return `px-2 py-0.5 text-[11px] rounded-md ${this.tagColorStyles[color]}`;
  }
}
