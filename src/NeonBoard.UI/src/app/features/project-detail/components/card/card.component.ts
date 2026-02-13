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
    const card = this.card();
    const allLabels = this.drawerService.boardLabels();
    if (!card.labelIds?.length || !allLabels.length) return [];
    return allLabels.filter(l => card.labelIds.includes(l.id));
  });

  getLabelClasses(color: string): string {
    const classes = getLabelColorClasses(color);
    return `${classes.bg} ${classes.text} ${classes.border}`;
  }
}
