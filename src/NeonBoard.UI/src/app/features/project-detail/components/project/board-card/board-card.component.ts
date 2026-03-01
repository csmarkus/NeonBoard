import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Board } from '../../../models/board.model';
import { GradientAccentComponent } from '../../../../../shared/components/gradient-accent/gradient-accent.component';
import { RelativeTimePipe } from '../../../../../shared/pipes/relative-time.pipe';

@Component({
  selector: 'app-board-card',
  imports: [CommonModule, RouterLink, GradientAccentComponent, RelativeTimePipe],
  templateUrl: './board-card.component.html',
})
export class BoardCardComponent {
  board = input.required<Board>();
  index = input.required<number>();
  shortId = input.required<string>();
}
