import { IconDefinition } from '@fortawesome/free-solid-svg-icons';
import { getLabelClassString } from './label.model';

export interface MessagePart {
  text: string;
  bold: boolean;
  cardId?: string;
  labelClasses?: string;
}

export interface GroupedEntry {
  entry: { id: string; occurredAt: string };
  icon: IconDefinition;
  messageParts: MessagePart[];
  relativeTime: string;
}

export interface DayGroup {
  label: string;
  entries: GroupedEntry[];
}

export function getDayLabel(occurredAt: string, now: Date): string {
  const date = new Date(occurredAt);
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
  const entryDay = new Date(date.getFullYear(), date.getMonth(), date.getDate());
  const diffDays = Math.floor((today.getTime() - entryDay.getTime()) / (1000 * 60 * 60 * 24));

  if (diffDays === 0) return 'Today';
  if (diffDays === 1) return 'Yesterday';

  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
}

export function parseMessageParts(
  text: string,
  cardEntityId?: string,
  cardDisplayId?: string,
  labelName?: string,
  labelColor?: string,
): MessagePart[] {
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
    const isLabel = labelName && boldText === labelName;
    parts.push({
      text: boldText,
      bold: true,
      cardId: isCardLink ? cardEntityId : undefined,
      labelClasses: isLabel ? getLabelClassString(labelColor ?? 'grey') : undefined,
    });
    lastIndex = regex.lastIndex;
  }

  if (lastIndex < text.length) {
    parts.push({ text: text.slice(lastIndex), bold: false });
  }

  return parts;
}
