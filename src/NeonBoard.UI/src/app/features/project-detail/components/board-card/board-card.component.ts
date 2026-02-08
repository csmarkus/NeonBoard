import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Board } from '../../models/board.model';
import { GradientAccentComponent } from '../../../../shared/components/gradient-accent/gradient-accent.component';

@Component({
  selector: 'app-board-card',
  standalone: true,
  imports: [CommonModule, RouterLink, GradientAccentComponent],
  templateUrl: './board-card.component.html',
})
export class BoardCardComponent {
  board = input.required<Board>();
  index = input.required<number>();

  getRelativeTime(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffInMs = now.getTime() - date.getTime();
    const diffInMinutes = Math.floor(diffInMs / (1000 * 60));
    const diffInHours = Math.floor(diffInMs / (1000 * 60 * 60));
    const diffInDays = Math.floor(diffInMs / (1000 * 60 * 60 * 24));

    if (diffInMinutes < 1) return 'Just now';
    if (diffInMinutes < 60) return `${diffInMinutes} minute${diffInMinutes > 1 ? 's' : ''} ago`;
    if (diffInHours < 24) return `${diffInHours} hour${diffInHours > 1 ? 's' : ''} ago`;
    if (diffInDays === 1) return '1 day ago';
    if (diffInDays < 30) return `${diffInDays} days ago`;

    return date.toLocaleDateString();
  }
}
