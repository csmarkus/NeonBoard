import { Component, input, output, ChangeDetectionStrategy } from '@angular/core';
import { A11yModule } from '@angular/cdk/a11y';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faTimes } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-drawer',
  imports: [FontAwesomeModule, A11yModule],
  templateUrl: './drawer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(document:keydown.escape)': 'onEscapeKey()',
  },
})
export class DrawerComponent {
  open = input(false);
  close = output<void>();

  faTimes = faTimes;

  onEscapeKey(): void {
    if (this.open()) {
      this.close.emit();
    }
  }

  get backdropClasses(): string {
    return `fixed inset-0 bg-black/60 z-40 transition-opacity duration-200 ${this.open() ? 'opacity-100' : 'opacity-0 pointer-events-none'}`;
  }

  get panelClasses(): string {
    return `fixed top-0 right-0 h-full w-full max-w-lg z-50 bg-surface border-l border-dim transform transition-transform duration-200 ease-out ${this.open() ? 'translate-x-0' : 'translate-x-full'}`;
  }
}
