import { Component, input, output, computed, ChangeDetectionStrategy } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faAlignLeft } from '@fortawesome/free-solid-svg-icons';
import { Card } from '../../../models/card.model';
import { getLabelClassString } from '../../../models/label.model';

@Component({
  selector: 'app-card',
  imports: [FontAwesomeModule],
  templateUrl: './card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CardComponent {
  faAlignLeft = faAlignLeft;

  card = input.required<Card>();
  cardClick = output<void>();

  cardLabels = computed(() => {
    const labels = this.card().labels ?? [];
    return labels.slice().sort((a, b) => a.name.localeCompare(b.name));
  });

  getLabelClasses = getLabelClassString;
}
