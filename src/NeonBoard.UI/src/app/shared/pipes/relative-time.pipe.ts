import { Pipe, PipeTransform } from '@angular/core';

export function formatRelativeTime(value: string | Date, format: 'long' | 'short' = 'long'): string {
  const date = typeof value === 'string' ? new Date(value) : value;
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMinutes = Math.floor(diffMs / (1000 * 60));
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
  const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

  if (format === 'short') {
    if (diffMinutes < 1) return 'just now';
    if (diffMinutes < 60) return `${diffMinutes}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    return `${diffDays}d ago`;
  }

  if (diffMinutes < 1) return 'Just now';
  if (diffMinutes < 60) return `${diffMinutes} minute${diffMinutes > 1 ? 's' : ''} ago`;
  if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
  if (diffDays === 1) return '1 day ago';
  if (diffDays < 30) return `${diffDays} days ago`;

  return date.toLocaleDateString();
}

@Pipe({
  name: 'relativeTime',
  pure: false,
})
export class RelativeTimePipe implements PipeTransform {
  transform(value: string | Date | null | undefined, format: 'long' | 'short' = 'long'): string {
    if (!value) return '';
    return formatRelativeTime(value, format);
  }
}
