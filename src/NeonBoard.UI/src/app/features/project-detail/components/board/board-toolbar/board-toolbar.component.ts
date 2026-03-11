import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faGear, faXmark, faBoxArchive, faClockRotateLeft } from '@fortawesome/free-solid-svg-icons';
import { ConnectionStatusComponent } from '../../../../../shared/components/connection-status/connection-status.component';
import { BoardStateFacade } from '../../../services/board-state.facade';
import { ProjectContext } from '../../../services/project-context.service';
import { getLabelColorClasses } from '../../../models/label.model';

@Component({
  selector: 'app-board-toolbar',
  imports: [RouterLink, FontAwesomeModule, ConnectionStatusComponent],
  templateUrl: './board-toolbar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(document:click)': 'onDocumentClick()',
  },
})
export class BoardToolbarComponent {
  private facade = inject(BoardStateFacade);
  protected projectContext = inject(ProjectContext);

  shortId = input.required<string>();
  slug = input.required<string>();

  faGear = faGear;
  faXmark = faXmark;
  faBoxArchive = faBoxArchive;
  faClockRotateLeft = faClockRotateLeft;
  getLabelDotClass(color: string): string {
    const { bg, border } = getLabelColorClasses(color);
    return `w-2.5 h-2.5 rounded-full flex-shrink-0 border ${bg} ${border}`;
  }

  getLabelChipClass(color: string): string {
    const { bg, text, border } = getLabelColorClasses(color);
    return `inline-flex items-center px-1.5 py-0.5 text-xs font-medium rounded border ${bg} ${text} ${border}`;
  }

  isDropdownOpen = signal(false);

  labels = this.facade.labels;
  selectedLabelIds = this.facade.selectedLabelIds;
  isFilterActive = this.facade.isFilterActive;

  hasLabels = computed(() => this.labels().length > 0);

  selectedLabels = computed(() =>
    this.labels().filter(l => this.selectedLabelIds().has(l.id))
  );

  filterButtonText = computed(() => {
    const count = this.selectedLabelIds().size;
    if (count === 0) return 'All cards';
    return count === 1 ? '1 label' : `${count} labels`;
  });

  isLabelSelected(labelId: string): boolean {
    return this.selectedLabelIds().has(labelId);
  }

  toggleDropdown(event: Event): void {
    event.stopPropagation();
    this.isDropdownOpen.update(v => !v);
  }

  toggleLabel(labelId: string, event: Event): void {
    event.stopPropagation();
    this.facade.toggleLabelFilter(labelId);
  }

  clearFilter(event: Event): void {
    event.stopPropagation();
    this.facade.clearLabelFilter();
  }

  openArchivePanel(): void {
    this.facade.openArchivePanel();
  }

  openActivityPanel(): void {
    this.facade.openActivityPanel();
  }

  onDocumentClick(): void {
    this.isDropdownOpen.set(false);
  }
}
