import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  IconDefinition,
  faPlus, faPencil, faTrash, faArrowsLeftRight, faArrowsUpDown,
  faBoxArchive, faRotateLeft, faTag, faCircleInfo
} from '@fortawesome/free-solid-svg-icons';
import { DrawerComponent } from '../../../../../shared/components/drawer/drawer.component';
import { BoardStateFacade } from '../../../services/board-state.facade';
import { getActivityMessage } from '../../../models/activity-messages';
import { ActivityEntry } from '../../../models/activity.model';

interface MessagePart {
  text: string;
  bold: boolean;
  cardId?: string;
}

interface GroupedEntry {
  entry: ActivityEntry;
  icon: IconDefinition;
  messageParts: MessagePart[];
  relativeTime: string;
}

interface DayGroup {
  label: string;
  entries: GroupedEntry[];
}

@Component({
  selector: 'app-activity-panel',
  imports: [DrawerComponent, FontAwesomeModule],
  templateUrl: './activity-panel.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ActivityPanelComponent {
  private facade = inject(BoardStateFacade);

  open = this.facade.showActivityPanel;
  entries = this.facade.activityEntries;
  nextCursor = this.facade.activityNextCursor;
  isLoading = this.facade.isLoadingActivity;

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

  groupedEntries = computed<DayGroup[]>(() => {
    const entries = this.entries();
    if (entries.length === 0) return [];

    const now = new Date();
    const groups = new Map<string, GroupedEntry[]>();

    for (const entry of entries) {
      const message = getActivityMessage(entry);
      const label = this.getDayLabel(entry.occurredAt, now);

      const grouped: GroupedEntry = {
        entry,
        icon: this.iconMap[message.icon] ?? faCircleInfo,
        messageParts: this.parseMessageParts(message.text, message.cardEntityId, message.cardDisplayId),
        relativeTime: this.getRelativeTime(entry.occurredAt, now),
      };

      const existing = groups.get(label);
      if (existing) {
        existing.push(grouped);
      } else {
        groups.set(label, [grouped]);
      }
    }

    return Array.from(groups.entries()).map(([label, items]) => ({
      label,
      entries: items,
    }));
  });

  close(): void {
    this.facade.closeActivityPanel();
  }

  loadMore(): void {
    this.facade.loadMoreActivity();
  }

  onCardClick(cardId: string): void {
    this.facade.openCardFromActivity(cardId);
  }

  private getDayLabel(occurredAt: string, now: Date): string {
    const date = new Date(occurredAt);
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const entryDay = new Date(date.getFullYear(), date.getMonth(), date.getDate());
    const diffDays = Math.floor((today.getTime() - entryDay.getTime()) / (1000 * 60 * 60 * 24));

    if (diffDays === 0) return 'Today';
    if (diffDays === 1) return 'Yesterday';

    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }

  private getRelativeTime(occurredAt: string, now: Date): string {
    const date = new Date(occurredAt);
    const diffMs = now.getTime() - date.getTime();
    const diffSeconds = Math.floor(diffMs / 1000);
    const diffMinutes = Math.floor(diffSeconds / 60);
    const diffHours = Math.floor(diffMinutes / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffSeconds < 60) return 'just now';
    if (diffMinutes < 60) return `${diffMinutes}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    return `${diffDays}d ago`;
  }

  private parseMessageParts(text: string, cardEntityId?: string, cardDisplayId?: string): MessagePart[] {
    const parts: MessagePart[] = [];
    const regex = /\*\*(.+?)\*\*/g;
    let lastIndex = 0;
    let match: RegExpExecArray | null;

    while ((match = regex.exec(text)) !== null) {
      if (match.index > lastIndex) {
        parts.push({ text: text.slice(lastIndex, match.index), bold: false });
      }
      const boldText = match[1];
      const isCardLink = cardEntityId && cardDisplayId && boldText === cardDisplayId;
      parts.push({ text: boldText, bold: true, cardId: isCardLink ? cardEntityId : undefined });
      lastIndex = regex.lastIndex;
    }

    if (lastIndex < text.length) {
      parts.push({ text: text.slice(lastIndex), bold: false });
    }

    return parts;
  }
}
