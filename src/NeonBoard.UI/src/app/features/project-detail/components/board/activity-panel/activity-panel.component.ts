import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  IconDefinition,
  faPlus, faPencil, faTrash, faArrowsLeftRight, faArrowsUpDown,
  faBoxArchive, faRotateLeft, faTag, faCircleInfo
} from '@fortawesome/free-solid-svg-icons';
import { DrawerComponent } from '../../../../../shared/components/drawer/drawer.component';
import { formatRelativeTime } from '../../../../../shared/pipes/relative-time.pipe';
import { BoardStateFacade } from '../../../services/board-state.facade';
import { getActivityMessage } from '../../../models/activity-messages';
import { ActivityEntry } from '../../../models/activity.model';
import { DayGroup, getDayLabel, GroupedEntry, MessagePart, parseMessageParts } from '../../../models/activity-utils';

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
      const label = getDayLabel(entry.occurredAt, now);

      const grouped: GroupedEntry = {
        entry,
        icon: this.iconMap[message.icon] ?? faCircleInfo,
        messageParts: parseMessageParts(message.text, message.cardEntityId, message.cardDisplayId, message.labelName, message.labelColor),
        relativeTime: formatRelativeTime(entry.occurredAt, 'short'),
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
}
