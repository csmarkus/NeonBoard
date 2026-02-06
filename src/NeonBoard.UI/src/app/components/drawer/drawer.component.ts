import { Component, Input, Output, EventEmitter, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faTimes } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-drawer',
  standalone: true,
  imports: [CommonModule, FontAwesomeModule],
  templateUrl: './drawer.component.html',
})
export class DrawerComponent {
  @Input() open = false;
  @Output() close = new EventEmitter<void>();

  faTimes = faTimes;

  @HostListener('document:keydown.escape')
  onEscapeKey() {
    if (this.open) {
      this.close.emit();
    }
  }

  get backdropClasses(): string {
    return `fixed inset-0 bg-black/60 z-40 transition-opacity duration-200 ${this.open ? 'opacity-100' : 'opacity-0 pointer-events-none'}`;
  }

  get panelClasses(): string {
    return `fixed top-0 right-0 h-full w-full max-w-lg z-50 bg-surface border-l border-dim transform transition-transform duration-200 ease-out ${this.open ? 'translate-x-0' : 'translate-x-full'}`;
  }
}
