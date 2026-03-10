import { initTestEnvironment } from '../../../../test-setup';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { ProjectContext } from './project-context.service';

initTestEnvironment();
import { ProjectService } from '../../projects/services/project.service';
import { BoardService } from './board.service';
import { Project } from '../../projects/models/project.model';
import { Board } from '../models/board.model';

const mockProject: Project = {
  id: 'p-1',
  shortId: 'abc1234',
  name: 'Test Project',
  description: '',
  ownerId: 'user-1',
  currentUserRole: 'Owner',
  createdAt: '',
  updatedAt: '',
} as Project;

const mockProject2: Project = {
  id: 'p-2',
  shortId: 'xyz5678',
  name: 'Other Project',
  description: '',
  ownerId: 'user-1',
  createdAt: '',
  updatedAt: '',
} as Project;

const mockBoards: Board[] = [
  { id: 'b-1', name: 'Sprint Board', slug: 'sprint-board', prefix: 'SPR', projectId: 'p-1', createdAt: '', updatedAt: '', columnCount: 3 },
  { id: 'b-2', name: 'Backlog', slug: 'backlog', prefix: 'BKL', projectId: 'p-1', createdAt: '', updatedAt: '', columnCount: 1 },
];

describe('ProjectContext', () => {
  let context: ProjectContext;
  let projectService: { getProjectByShortId: ReturnType<typeof vi.fn> };
  let boardService: { getBoardsByProject: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    projectService = {
      getProjectByShortId: vi.fn().mockReturnValue(of(mockProject)),
    };
    boardService = {
      getBoardsByProject: vi.fn().mockReturnValue(of(mockBoards)),
    };

    TestBed.configureTestingModule({
      providers: [
        ProjectContext,
        { provide: ProjectService, useValue: projectService },
        { provide: BoardService, useValue: boardService },
      ],
    });

    context = TestBed.inject(ProjectContext);
  });

  describe('initial state', () => {
    it('should have null project', () => {
      expect(context.project()).toBeNull();
    });

    it('should have empty boards', () => {
      expect(context.boards()).toEqual([]);
    });

    it('should have boardsLoaded as false', () => {
      expect(context.boardsLoaded()).toBe(false);
    });

    it('should have empty computed strings', () => {
      expect(context.projectId()).toBe('');
      expect(context.projectName()).toBe('');
      expect(context.shortId()).toBe('');
      expect(context.currentUserRole()).toBeUndefined();
    });

    it('should have canEdit as false when no project', () => {
      expect(context.canEdit()).toBe(false);
    });

    it('should have isOwner as false when no project', () => {
      expect(context.isOwner()).toBe(false);
    });
  });

  describe('resolve', () => {
    it('should fetch project by shortId and set project signal', () => {
      context.resolve('abc1234');

      expect(projectService.getProjectByShortId).toHaveBeenCalledWith('abc1234');
      expect(context.project()).toEqual(mockProject);
    });

    it('should set computed signals from resolved project', () => {
      context.resolve('abc1234');

      expect(context.projectId()).toBe('p-1');
      expect(context.projectName()).toBe('Test Project');
      expect(context.shortId()).toBe('abc1234');
      expect(context.currentUserRole()).toBe('Owner');
    });

    it('should load boards after resolving project', () => {
      context.resolve('abc1234');

      expect(boardService.getBoardsByProject).toHaveBeenCalledWith('p-1');
      expect(context.boards()).toEqual(mockBoards);
      expect(context.boardsLoaded()).toBe(true);
    });

    it('should skip fetch if already resolved for same shortId', () => {
      context.resolve('abc1234');
      projectService.getProjectByShortId.mockClear();
      boardService.getBoardsByProject.mockClear();

      context.resolve('abc1234');

      expect(projectService.getProjectByShortId).not.toHaveBeenCalled();
      expect(boardService.getBoardsByProject).not.toHaveBeenCalled();
    });

    it('should clear and re-fetch when resolving a different shortId', () => {
      context.resolve('abc1234');

      projectService.getProjectByShortId.mockReturnValue(of(mockProject2));
      boardService.getBoardsByProject.mockReturnValue(of([]));

      context.resolve('xyz5678');

      expect(context.project()).toEqual(mockProject2);
      expect(context.projectId()).toBe('p-2');
      expect(context.boards()).toEqual([]);
    });

    it('should set canEdit to true for Owner role', () => {
      context.resolve('abc1234');
      expect(context.canEdit()).toBe(true);
    });

    it('should set isOwner to true for Owner role', () => {
      context.resolve('abc1234');
      expect(context.isOwner()).toBe(true);
    });

    it('should set canEdit to true for Editor role', () => {
      const editorProject = { ...mockProject, currentUserRole: 'Editor' as const };
      projectService.getProjectByShortId.mockReturnValue(of(editorProject));

      context.resolve('abc1234');
      expect(context.canEdit()).toBe(true);
      expect(context.isOwner()).toBe(false);
    });

    it('should set canEdit to false for Viewer role', () => {
      const viewerProject = { ...mockProject, currentUserRole: 'Viewer' as const };
      projectService.getProjectByShortId.mockReturnValue(of(viewerProject));

      context.resolve('abc1234');
      expect(context.canEdit()).toBe(false);
      expect(context.isOwner()).toBe(false);
    });
  });

  describe('reloadBoards', () => {
    it('should re-fetch boards for the current project', () => {
      context.resolve('abc1234');
      boardService.getBoardsByProject.mockClear();

      const updatedBoards = [mockBoards[0]];
      boardService.getBoardsByProject.mockReturnValue(of(updatedBoards));

      context.reloadBoards();

      expect(boardService.getBoardsByProject).toHaveBeenCalledWith('p-1');
      expect(context.boards()).toEqual(updatedBoards);
    });

    it('should not fetch if no project is resolved', () => {
      context.reloadBoards();

      expect(boardService.getBoardsByProject).not.toHaveBeenCalled();
    });
  });

  describe('findBoardBySlug', () => {
    it('should return the board matching the slug', () => {
      context.resolve('abc1234');

      const board = context.findBoardBySlug('sprint-board');

      expect(board).toEqual(mockBoards[0]);
    });

    it('should return undefined for a non-existent slug', () => {
      context.resolve('abc1234');

      expect(context.findBoardBySlug('non-existent')).toBeUndefined();
    });
  });

  describe('clear', () => {
    it('should reset all state', () => {
      context.resolve('abc1234');
      expect(context.project()).not.toBeNull();

      context.clear();

      expect(context.project()).toBeNull();
      expect(context.boards()).toEqual([]);
      expect(context.boardsLoaded()).toBe(false);
      expect(context.projectId()).toBe('');
      expect(context.projectName()).toBe('');
      expect(context.shortId()).toBe('');
    });
  });
});
