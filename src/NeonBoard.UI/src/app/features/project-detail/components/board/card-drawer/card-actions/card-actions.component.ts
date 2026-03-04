import { Component, input, output, signal, ChangeDetectionStrategy } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faPause, faPlay, faBoxArchive, faRotateLeft, faTrash, faEllipsis } from '@fortawesome/free-solid-svg-icons';
import { ButtonComponent } from '../../../../../../shared/components/button/button.component';

@Component({
  selector: 'app-card-actions',
  imports: [ButtonComponent, FontAwesomeModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="pt-4 border-t border-subtle relative flex justify-end">
      <app-button
        variant="secondary"
        (click)="toggleMenu()"
        aria-haspopup="true"
        [attr.aria-expanded]="menuOpen()"
      >
        <fa-icon [icon]="icons.ellipsis" class="text-sm"></fa-icon>
        Actions
      </app-button>

      @if (menuOpen()) {
        <div class="fixed inset-0 z-40" (click)="closeMenu()"></div>
        <div
          class="absolute right-0 top-full mt-1 bg-surface border border-subtle rounded-lg shadow-lg py-1 z-50 min-w-[180px]"
          role="menu"
        >
          @if (isOnHold()) {
            <button
              class="w-full px-3 py-2 text-left text-sm text-secondary hover:bg-surface-elevated hover:text-primary transition-colors flex items-center gap-2"
              role="menuitem"
              [disabled]="isHolding()"
              (click)="onResume()"
            >
              <fa-icon [icon]="icons.play" class="w-4"></fa-icon>
              {{ isHolding() ? 'Resuming...' : 'Resume card' }}
            </button>
          } @else if (!isArchived()) {
            <button
              class="w-full px-3 py-2 text-left text-sm text-secondary hover:bg-surface-elevated hover:text-primary transition-colors flex items-center gap-2"
              role="menuitem"
              [disabled]="isHolding()"
              (click)="onHold()"
            >
              <fa-icon [icon]="icons.pause" class="w-4"></fa-icon>
              {{ isHolding() ? 'Holding...' : 'Put on hold' }}
            </button>
          }

          @if (isArchived()) {
            <button
              class="w-full px-3 py-2 text-left text-sm text-secondary hover:bg-surface-elevated hover:text-primary transition-colors flex items-center gap-2"
              role="menuitem"
              [disabled]="isArchiving()"
              (click)="onRestore()"
            >
              <fa-icon [icon]="icons.rotateLeft" class="w-4"></fa-icon>
              {{ isArchiving() ? 'Restoring...' : 'Restore card' }}
            </button>
          } @else {
            <button
              class="w-full px-3 py-2 text-left text-sm text-secondary hover:bg-surface-elevated hover:text-primary transition-colors flex items-center gap-2"
              role="menuitem"
              [disabled]="isArchiving()"
              (click)="onArchive()"
            >
              <fa-icon [icon]="icons.boxArchive" class="w-4"></fa-icon>
              {{ isArchiving() ? 'Archiving...' : 'Archive card' }}
            </button>
          }

          <div class="border-t border-subtle my-1"></div>

          <button
            class="w-full px-3 py-2 text-left text-sm text-red-400 hover:bg-red-500/10 transition-colors flex items-center gap-2"
            role="menuitem"
            [disabled]="isDeleting()"
            (click)="onDelete()"
          >
            <fa-icon [icon]="icons.trash" class="w-4"></fa-icon>
            {{ isDeleting() ? 'Deleting...' : 'Delete card' }}
          </button>
        </div>
      }
    </div>
  `,
})
export class CardActionsComponent {
  isArchived = input(false);
  isOnHold = input(false);
  isArchiving = input(false);
  isHolding = input(false);
  isDeleting = input(false);

  archive = output<void>();
  restore = output<void>();
  hold = output<void>();
  resume = output<void>();
  delete = output<void>();

  menuOpen = signal(false);

  protected icons = {
    ellipsis: faEllipsis,
    pause: faPause,
    play: faPlay,
    boxArchive: faBoxArchive,
    rotateLeft: faRotateLeft,
    trash: faTrash,
  };

  toggleMenu(): void {
    this.menuOpen.update(open => !open);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }

  onHold(): void {
    this.hold.emit();
    this.closeMenu();
  }

  onResume(): void {
    this.resume.emit();
    this.closeMenu();
  }

  onArchive(): void {
    this.archive.emit();
    this.closeMenu();
  }

  onRestore(): void {
    this.restore.emit();
    this.closeMenu();
  }

  onDelete(): void {
    this.delete.emit();
    this.closeMenu();
  }
}
