import { Component, input, inject, signal, effect, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';
import { AuthService } from '@auth0/auth0-angular';
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
  faChevronDown,
} from '@fortawesome/free-solid-svg-icons';
import { BoardService } from '../../features/project-detail/services/board.service';
import { Board } from '../../features/project-detail/models/board.model';

interface NavItem {
  icon: IconDefinition;
  label: string;
  route?: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, FontAwesomeModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'shrink-0'
  },
  templateUrl: './sidebar.component.html',
})
export class SidebarComponent {
  projectId = input.required<string>();

  protected auth = inject(AuthService);
  private boardService = inject(BoardService);
  private router = inject(Router);

  collapsed = false;
  userMenuOpen = false;
  boardsMenuOpen = false;
  boards = signal<Board[]>([]);

  // Icons for template
  faChevronRight = faChevronRight;
  faChevronLeft = faChevronLeft;
  faChevronDown = faChevronDown;
  faUser = faUser;
  faLock = faLock;
  faBell = faBell;
  faQuestionCircle = faQuestionCircle;
  faSignOutAlt = faSignOutAlt;
  faFolderOpen = faFolderOpen;
  faGripVertical = faGripVertical;
  faCog = faCog;

  private lastLoadedProjectId = '';

  constructor() {
    effect(() => {
      const currentProjectId = this.projectId();
      if (currentProjectId && currentProjectId !== this.lastLoadedProjectId) {
        this.lastLoadedProjectId = currentProjectId;
        this.loadBoards(currentProjectId);
      } else if (!currentProjectId) {
        this.boards.set([]);
        this.lastLoadedProjectId = '';
      }
    });

    // Subscribe to board updates
    this.boardService.boardsUpdated$.subscribe(() => {
      const currentProjectId = this.projectId();
      if (currentProjectId) {
        this.loadBoards(currentProjectId);
      }
    });
  }

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

  logout(): void {
    this.auth.logout({
      logoutParams: {
        returnTo: window.location.origin
      }
    });
  }

  getUserInitials(name: string | undefined): string {
    if (!name) return '?';
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .substring(0, 2);
  }

  private loadBoards(projectId: string): void {
    this.boardService.getBoardsByProject(projectId).subscribe({
      next: (boards) => {
        this.boards.set(boards);
        this.boardsMenuOpen = boards.length > 0;
      },
      error: (err) => {
        console.error('Error loading boards:', err);
        this.boards.set([]);
      }
    });
  }

  isBoardActive(boardId: string): boolean {
    return this.router.url.includes(`/b/${boardId}`);
  }
}
