import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import {
  faFolderOpen,
  faGripVertical,
  faCog,
  faUser,
  faLock,
  faBell,
  faQuestionCircle,
  faSignOutAlt,
  faChevronRight,
  faChevronLeft,
} from '@fortawesome/free-solid-svg-icons';

interface NavItem {
  icon: IconDefinition;
  label: string;
  route?: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, FontAwesomeModule],
  host: {
    class: 'shrink-0'
  },
  templateUrl: './sidebar.component.html',
})
export class SidebarComponent {
  @Input() projectId?: string;

  collapsed = false;
  userMenuOpen = false;

  // Icons for template
  faChevronRight = faChevronRight;
  faChevronLeft = faChevronLeft;
  faUser = faUser;
  faLock = faLock;
  faBell = faBell;
  faQuestionCircle = faQuestionCircle;
  faSignOutAlt = faSignOutAlt;
  faFolderOpen = faFolderOpen;
  faGripVertical = faGripVertical;
  faCog = faCog;

  get sidebarClasses(): string {
    return `bg-void border-r border-subtle flex flex-col shrink-0 transition-all duration-200 ease-out ${this.collapsed ? 'w-16' : 'w-64'}`;
  }

  getNavItemClasses(isActive: boolean): string {
    const base = 'w-full flex items-center gap-3 px-3 py-2 rounded-lg transition-colors duration-150';
    const active = isActive ? 'bg-surface-elevated text-primary' : 'text-muted hover:text-secondary hover:bg-surface';
    const collapsed = this.collapsed ? 'justify-center' : '';
    return `${base} ${active} ${collapsed}`;
  }

  get userButtonClasses(): string {
    return `w-full flex items-center gap-3 px-2 py-2 rounded-lg hover:bg-surface transition-colors duration-150 ${this.collapsed ? 'justify-center' : ''} ${this.userMenuOpen ? 'bg-surface' : ''}`;
  }

  get userMenuClasses(): string {
    return `absolute z-50 bottom-full mb-2 ${this.collapsed ? 'left-full ml-2' : 'left-3 right-3'} bg-surface border border-dim rounded-lg shadow-md overflow-hidden min-w-[200px]`;
  }

  get collapseButtonClasses(): string {
    return `w-full flex items-center gap-3 px-3 py-2 rounded-lg text-muted hover:text-secondary hover:bg-surface transition-colors duration-150 ${this.collapsed ? 'justify-center' : ''}`;
  }
}
