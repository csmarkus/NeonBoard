import { Component, input, output, computed, inject, ChangeDetectionStrategy } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faAlignLeft } from '@fortawesome/free-solid-svg-icons';
import { GradientAccentComponent } from '../../../../../shared/components/gradient-accent/gradient-accent.component';
import { Card } from '../../../models/card.model';
import { getLabelClassString } from '../../../models/label.model';
import { DrawerService } from '../../../services/drawer.service';

@Component({
  selector: 'app-card',
  imports: [FontAwesomeModule, GradientAccentComponent],
  templateUrl: './card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CardComponent {
  private drawerService = inject(DrawerService);

  faAlignLeft = faAlignLeft;

  card = input.required<Card>();
  cardClick = output<void>();

  cardLabels = computed(() => {
    const labels = this.card().labels ?? [];
    return labels.slice().sort((a, b) => a.name.localeCompare(b.name));
  });

  getLabelClasses = getLabelClassString;
}
