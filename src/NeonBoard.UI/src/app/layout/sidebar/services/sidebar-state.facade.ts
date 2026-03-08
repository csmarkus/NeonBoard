import { Injectable, inject, signal, computed, effect, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { BoardService } from '../../../features/project-detail/services/board.service';
import { ProjectContext } from '../../../features/project-detail/services/project-context.service';

@Injectable({
  providedIn: 'root'
})
export class SidebarStateFacade {
  private boardService = inject(BoardService);
  private projectContext = inject(ProjectContext);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  private _collapsed = signal(false);
  private _userMenuOpen = signal(false);
  private _boardsMenuOpen = signal(false);

  readonly collapsed = this._collapsed.asReadonly();
  readonly userMenuOpen = this._userMenuOpen.asReadonly();
  readonly boardsMenuOpen = this._boardsMenuOpen.asReadonly();
  readonly boards = this.projectContext.boards;

  readonly sidebarClasses = computed(() => {
    const width = this._collapsed() ? 'w-16' : 'w-64';
    return `bg-void border-r border-subtle flex flex-col shrink-0 transition-all duration-200 ease-out ${width}`;
  });

  readonly collapseButtonClasses = computed(() => {
    const base = 'w-full flex items-center gap-3 px-3 py-2 rounded-lg text-muted hover:text-secondary hover:bg-surface transition-colors duration-150';
    const collapsed = this._collapsed() ? 'justify-center' : '';
    return `${base} ${collapsed}`;
  });

  readonly userButtonClasses = computed(() => {
    const base = 'w-full flex items-center gap-3 py-2 rounded-lg hover:bg-surface transition-colors duration-150';
    const collapsed = this._collapsed() ? 'justify-center px-0' : 'px-2';
    const menuOpen = this._userMenuOpen() ? 'bg-surface' : '';
    return `${base} ${collapsed} ${menuOpen}`;
  });

  readonly userMenuClasses = computed(() => {
    const position = this._collapsed() ? 'left-full ml-2' : 'left-3 right-3';
    return `absolute z-50 bottom-full mb-2 ${position} bg-surface border border-dim rounded-lg shadow-md overflow-hidden min-w-[200px]`;
  });

  constructor() {
    effect(() => {
      const boards = this.projectContext.boards();
      if (boards.length > 0) {
        this._boardsMenuOpen.set(true);
      }
    });

    this.boardService.boardsUpdated$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.projectContext.reloadBoards();
      });
  }

  toggleCollapsed(): void {
    this._collapsed.update(value => !value);
  }

  toggleUserMenu(): void {
    this._userMenuOpen.update(value => !value);
  }

  closeUserMenu(): void {
    this._userMenuOpen.set(false);
  }

  toggleBoardsMenu(): void {
    this._boardsMenuOpen.update(value => !value);
  }

  getNavItemClasses(isActive: boolean): string {
    const base = 'w-full flex items-center gap-3 px-3 py-2 rounded-lg transition-colors duration-150';
    const active = isActive ? 'bg-surface-elevated text-primary' : 'text-muted hover:text-secondary hover:bg-surface';
    const collapsed = this._collapsed() ? 'justify-center' : '';
    return `${base} ${active} ${collapsed}`;
  }

  isBoardActive(slug: string): boolean {
    return this.router.url.includes(`/b/${slug}`);
  }
}
