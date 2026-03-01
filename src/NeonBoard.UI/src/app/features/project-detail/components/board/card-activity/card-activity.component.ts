import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faPlus, faPencil, faTrash, faArrowsLeftRight, faArrowsUpDown,
  faBoxArchive, faRotateLeft, faTag, faCircleInfo,
  IconDefinition,
} from '@fortawesome/free-solid-svg-icons';
import { formatRelativeTime } from '../../../../../shared/pipes/relative-time.pipe';
import { CardService } from '../../../services/card.service';
import { ActivityEntry, ActivityFeed } from '../../../models/activity.model';
import { getActivityMessage } from '../../../models/activity-messages';
import { getLabelClassString } from '../../../models/label.model';

interface MessagePart {
  text: string;
  bold: boolean;
  labelClasses?: string;
}

@Component({
  selector: 'app-card-activity',
  imports: [FontAwesomeModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="border-t border-subtle pt-4 mt-4">
      <h3 class="text-sm font-medium text-primary mb-3">Activity</h3>

      @if (entryMessages().length > 0) {
        <div class="flow-root">
          <ul role="list" class="-mb-6">
            @for (item of entryMessages(); track item.entry.id; let last = $last) {
              <li>
                <div class="relative pb-6">
                  @if (!last) {
                    <span class="absolute left-3 top-6 -ml-px h-full w-0.5 bg-dim" aria-hidden="true"></span>
                  }
                  <div class="relative flex gap-2.5">
                    <div>
                      <span class="flex h-6 w-6 items-center justify-center rounded-full bg-surface-elevated ring-4 ring-surface">
                        <fa-icon [icon]="item.icon" size="2xs" class="text-muted"></fa-icon>
                      </span>
                    </div>
                    <div class="flex min-w-0 flex-1 justify-between gap-3 pt-0.5">
                      <p class="text-xs text-secondary leading-snug">
                        @for (part of item.messageParts; track $index) {
                          @if (part.labelClasses) {
                            <span [class]="'inline-flex items-center px-1 py-0.5 text-xs font-medium rounded border ' + part.labelClasses">{{ part.text }}</span>
                          } @else if (part.bold) {
                            <span class="font-medium text-primary">{{ part.text }}</span>
                          } @else {
                            <span>{{ part.text }}</span>
                          }
                        }
                      </p>
                      <span class="whitespace-nowrap text-xs text-muted">{{ item.relativeTime }}</span>
                    </div>
                  </div>
                </div>
              </li>
            }
          </ul>
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
  private cardService = inject(CardService);

  projectId = input.required<string>();
  boardId = input.required<string>();
  cardId = input.required<string>();
  initialActivity = input<ActivityFeed | null>(null);

  entries = signal<ActivityEntry[]>([]);
  nextCursor = signal<string | null>(null);
  isLoading = signal(false);

  private iconMap: Record<string, IconDefinition> = {
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
        icon: this.iconMap[message.icon] ?? faCircleInfo,
        messageParts: this.parseMessageParts(message.text, message.labelName, message.labelColor),
        relativeTime: formatRelativeTime(entry.occurredAt, 'short'),
      };
    })
  );

  constructor() {
    effect(() => {
      const cardId = this.cardId();
      const initial = this.initialActivity();
      if (cardId) {
        if (initial) {
          this.entries.set(initial.entries);
          this.nextCursor.set(initial.nextCursor);
        } else {
          this.entries.set([]);
          this.nextCursor.set(null);
          this.loadActivity();
        }
      }
    });
  }

  loadMore(): void {
    if (this.nextCursor() && !this.isLoading()) {
      this.loadActivity();
    }
  }

  private parseMessageParts(text: string, labelName?: string, labelColor?: string): MessagePart[] {
    const parts: MessagePart[] = [];
    const regex = /\*\*(.+?)\*\*/g;
    let lastIndex = 0;
    let match: RegExpExecArray | null;

    while ((match = regex.exec(text)) !== null) {
      if (match.index > lastIndex) {
        parts.push({ text: text.slice(lastIndex, match.index), bold: false });
      }
      const boldText = match[1];
      const isLabel = labelName && boldText === labelName;
      parts.push({
        text: boldText,
        bold: true,
        labelClasses: isLabel ? getLabelClassString(labelColor ?? 'grey') : undefined,
      });
      lastIndex = regex.lastIndex;
    }

    if (lastIndex < text.length) {
      parts.push({ text: text.slice(lastIndex), bold: false });
    }

    return parts;
  }

  private loadActivity(): void {
    this.isLoading.set(true);
    this.cardService.getCardActivity(
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
