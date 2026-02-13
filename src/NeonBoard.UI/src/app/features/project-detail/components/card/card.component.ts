import { Component, input, output, computed, inject, ChangeDetectionStrategy } from '@angular/core';
import { Card } from '../../models/card.model';
import { getLabelColorClasses } from '../../models/label.model';
import { DrawerService } from '../../services/drawer.service';

@Component({
  selector: 'app-card',
  templateUrl: './card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CardComponent {
  private drawerService = inject(DrawerService);

  card = input.required<Card>();
  cardClick = output<void>();

  cardLabels = computed(() => {
    return this.card().labels ?? [];
  });

  getLabelClasses(color: string): string {
    const classes = getLabelColorClasses(color);
    return `${classes.bg} ${classes.text} ${classes.border}`;
  }
}
