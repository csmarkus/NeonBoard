import { Component, input, output, ChangeDetectionStrategy } from '@angular/core';
import { Card } from '../../models/card.model';

@Component({
  selector: 'app-card',
  templateUrl: './card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CardComponent {
  card = input.required<Card>();
  cardClick = output<void>();
}
