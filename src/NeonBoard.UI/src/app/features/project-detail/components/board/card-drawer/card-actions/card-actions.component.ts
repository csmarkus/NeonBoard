import { Component, input, output, ChangeDetectionStrategy } from '@angular/core';
import { ButtonComponent } from '../../../../../../shared/components/button/button.component';

@Component({
  selector: 'app-card-actions',
  imports: [ButtonComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="pt-4 border-t border-subtle flex gap-3">
      @if (isArchived()) {
        <app-button
          variant="secondary"
          (click)="restore.emit()"
          [disabled]="isArchiving()"
        >
          {{ isArchiving() ? 'Restoring...' : 'Restore card' }}
        </app-button>
      } @else {
        <app-button
          variant="secondary"
          (click)="archive.emit()"
          [disabled]="isArchiving()"
        >
          {{ isArchiving() ? 'Archiving...' : 'Archive card' }}
        </app-button>
      }
      <app-button
        variant="danger"
        (click)="delete.emit()"
        [disabled]="isDeleting()"
      >
        {{ isDeleting() ? 'Deleting...' : 'Delete card' }}
      </app-button>
    </div>
  `,
})
export class CardActionsComponent {
  isArchived = input(false);
  isArchiving = input(false);
  isDeleting = input(false);

  archive = output<void>();
  restore = output<void>();
  delete = output<void>();
}
