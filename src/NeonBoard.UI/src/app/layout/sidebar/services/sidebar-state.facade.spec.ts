import { initTestEnvironment } from '../../../../test-setup';
import { TestBed } from '@angular/core/testing';
import { of, throwError, Subject } from 'rxjs';
import { Router } from '@angular/router';
import { SidebarStateFacade } from './sidebar-state.facade';

initTestEnvironment();
import { BoardService } from '../../../features/project-detail/services/board.service';
import { Board } from '../../../features/project-detail/models/board.model';

describe('SidebarStateFacade', () => {
  let facade: SidebarStateFacade;
  let boardsUpdated$: Subject<void>;

  let boardService: {
    getBoardsByProject: ReturnType<typeof vi.fn>;
    boardsUpdated$: ReturnType<typeof Subject.prototype.asObservable>;
  };
  let router: {
    url: string;
  };

  beforeEach(() => {
    boardsUpdated$ = new Subject<void>();

    boardService = {
      getBoardsByProject: vi.fn(),
      boardsUpdated$: boardsUpdated$.asObservable(),
    };
    router = { url: '/projects/project-1' };

    TestBed.configureTestingModule({
      providers: [
        SidebarStateFacade,
        { provide: BoardService, useValue: boardService },
        { provide: Router, useValue: router },
      ],
    });

    facade = TestBed.inject(SidebarStateFacade);
  });

  const mockBoards: Board[] = [
    { id: 'board-1', name: 'Sprint Board', projectId: 'project-1', createdAt: '', updatedAt: '', columnCount: 3 },
    { id: 'board-2', name: 'Backlog', projectId: 'project-1', createdAt: '', updatedAt: '', columnCount: 1 },
  ];

  describe('loadBoards', () => {
    it('should set boards and open boards menu on success', () => {
      boardService.getBoardsByProject.mockReturnValue(of(mockBoards));

      facade.loadBoards('project-1');

      expect(facade.boards()).toEqual(mockBoards);
      expect(facade.boardsMenuOpen()).toBe(true);
    });

    it('should set empty boards on error', () => {
      boardService.getBoardsByProject.mockReturnValue(throwError(() => new Error('fail')));

      facade.loadBoards('project-1');

      expect(facade.boards()).toEqual([]);
    });

    it('should not open boards menu when no boards returned', () => {
      boardService.getBoardsByProject.mockReturnValue(of([]));

      facade.loadBoards('project-1');

      expect(facade.boards()).toEqual([]);
      expect(facade.boardsMenuOpen()).toBe(false);
    });
  });

  describe('clearBoards', () => {
    it('should reset state', () => {
      boardService.getBoardsByProject.mockReturnValue(of(mockBoards));
      facade.loadBoards('project-1');

      facade.clearBoards();

      expect(facade.boards()).toEqual([]);
      expect(facade.boardsMenuOpen()).toBe(false);
    });
  });

  describe('toggleCollapsed', () => {
    it('should toggle collapsed state', () => {
      expect(facade.collapsed()).toBe(false);

      facade.toggleCollapsed();
      expect(facade.collapsed()).toBe(true);

      facade.toggleCollapsed();
      expect(facade.collapsed()).toBe(false);
    });
  });

  describe('toggleUserMenu', () => {
    it('should toggle user menu state', () => {
      expect(facade.userMenuOpen()).toBe(false);

      facade.toggleUserMenu();
      expect(facade.userMenuOpen()).toBe(true);

      facade.toggleUserMenu();
      expect(facade.userMenuOpen()).toBe(false);
    });
  });

  describe('closeUserMenu', () => {
    it('should close user menu', () => {
      facade.toggleUserMenu(); // open
      expect(facade.userMenuOpen()).toBe(true);

      facade.closeUserMenu();
      expect(facade.userMenuOpen()).toBe(false);
    });
  });

  describe('toggleBoardsMenu', () => {
    it('should toggle boards menu state', () => {
      expect(facade.boardsMenuOpen()).toBe(false);

      facade.toggleBoardsMenu();
      expect(facade.boardsMenuOpen()).toBe(true);

      facade.toggleBoardsMenu();
      expect(facade.boardsMenuOpen()).toBe(false);
    });
  });

  describe('getNavItemClasses', () => {
    it('should return active classes when active', () => {
      const classes = facade.getNavItemClasses(true);

      expect(classes).toContain('bg-surface-elevated');
      expect(classes).toContain('text-primary');
    });

    it('should return inactive classes when not active', () => {
      const classes = facade.getNavItemClasses(false);

      expect(classes).toContain('text-muted');
      expect(classes).toContain('hover:text-secondary');
    });

    it('should include justify-center when collapsed', () => {
      facade.toggleCollapsed();

      const classes = facade.getNavItemClasses(false);

      expect(classes).toContain('justify-center');
    });
  });

  describe('isBoardActive', () => {
    it('should return true when router URL contains board path', () => {
      router.url = '/projects/project-1/b/board-1';

      expect(facade.isBoardActive('board-1')).toBe(true);
    });

    it('should return false when router URL does not contain board path', () => {
      router.url = '/projects/project-1/b/board-2';

      expect(facade.isBoardActive('board-1')).toBe(false);
    });
  });

  describe('computed signals', () => {
    it('sidebarClasses should include w-64 when not collapsed', () => {
      expect(facade.sidebarClasses()).toContain('w-64');
    });

    it('sidebarClasses should include w-16 when collapsed', () => {
      facade.toggleCollapsed();

      expect(facade.sidebarClasses()).toContain('w-16');
    });

    it('collapseButtonClasses should include justify-center when collapsed', () => {
      facade.toggleCollapsed();

      expect(facade.collapseButtonClasses()).toContain('justify-center');
    });

    it('userButtonClasses should include bg-surface when menu open', () => {
      facade.toggleUserMenu();

      expect(facade.userButtonClasses()).toContain('bg-surface');
    });
  });
});
