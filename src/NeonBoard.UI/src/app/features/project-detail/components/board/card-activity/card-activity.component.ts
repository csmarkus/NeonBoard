import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faPlus, faPencil, faTrash, faArrowsLeftRight, faArrowsUpDown,
  faBoxArchive, faRotateLeft, faTag, faCircleInfo,
  IconDefinition,
} from '@fortawesome/free-solid-svg-icons';
import { ActivityService } from '../../../services/activity.service';
import { ActivityEntry } from '../../../models/activity.model';
import { getActivityMessage } from '../../../models/activity-messages';

@Component({
  selector: 'app-card-activity',
  imports: [FontAwesomeModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="border-t border-subtle pt-4 mt-4">
      <h3 class="text-sm font-medium text-primary mb-3">Activity</h3>

      @for (item of entryMessages(); track item.entry.id) {
        <div class="flex items-start gap-2 py-1.5">
          <div class="mt-0.5 w-4 h-4 flex items-center justify-center text-muted flex-shrink-0">
            <fa-icon [icon]="iconMap[item.message.icon] || iconMap['circle-info']" class="text-xs"></fa-icon>
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-xs text-secondary">{{ item.plainText }}</p>
            <p class="text-xs text-muted">{{ item.relativeTime }}</p>
          </div>
        </div>
      }

      @if (isLoading()) {
        <div class="flex justify-center py-2">
          <div class="w-4 h-4 border-2 border-accent/30 border-t-accent rounded-full animate-spin"></div>
        </div>
      }

      @if (entries().length === 0 && !isLoading()) {
        <p class="text-xs text-muted py-2">No activity yet</p>
      }

      @if (nextCursor() && !isLoading()) {
        <button
          (click)="loadMore()"
          class="text-xs text-accent hover:text-accent/80 transition-colors mt-1">
          Show more
        </button>
      }
    </div>
  `,
})
export class CardActivityComponent {
  private activityService = inject(ActivityService);

  projectId = input.required<string>();
  boardId = input.required<string>();
  cardId = input.required<string>();

  entries = signal<ActivityEntry[]>([]);
  nextCursor = signal<string | null>(null);
  isLoading = signal(false);

  iconMap: Record<string, IconDefinition> = {
    'plus': faPlus,
    'pencil': faPencil,
    'trash': faTrash,
    'arrows-left-right': faArrowsLeftRight,
    'arrows-up-down': faArrowsUpDown,
    'box-archive': faBoxArchive,
    'rotate-left': faRotateLeft,
    'tag': faTag,
    'circle-info': faCircleInfo,
  };

  entryMessages = computed(() =>
    this.entries().map(entry => {
      const message = getActivityMessage(entry);
      return {
        entry,
        message,
        plainText: message.text.replace(/\*\*/g, ''),
        relativeTime: this.getRelativeTime(entry.occurredAt),
      };
    })
  );

  constructor() {
    effect(() => {
      const cardId = this.cardId();
      if (cardId) {
        this.entries.set([]);
        this.nextCursor.set(null);
        this.loadActivity();
      }
    });
  }

  loadMore(): void {
    if (this.nextCursor() && !this.isLoading()) {
      this.loadActivity();
    }
  }

  getRelativeTime(occurredAt: string): string {
    const diffMs = Date.now() - new Date(occurredAt).getTime();
    const diffMin = Math.floor(diffMs / 60000);
    if (diffMin < 1) return 'just now';
    if (diffMin < 60) return `${diffMin}m ago`;
    const diffHr = Math.floor(diffMin / 60);
    if (diffHr < 24) return `${diffHr}h ago`;
    return `${Math.floor(diffHr / 24)}d ago`;
  }

  private loadActivity(): void {
    this.isLoading.set(true);
    this.activityService.getCardActivity(
      this.projectId(), this.boardId(), this.cardId(), 10,
      this.nextCursor() ?? undefined
    ).subscribe({
      next: (feed) => {
        this.entries.update(prev => [...prev, ...feed.entries]);
        this.nextCursor.set(feed.nextCursor);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }
}
