import { initTestEnvironment } from '../../../../test-setup';
import { TestBed } from '@angular/core/testing';
import { of, throwError, Subject } from 'rxjs';
import { BoardSettingsFacade } from './board-settings.facade';

initTestEnvironment();
import { BoardService } from './board.service';
import { LabelService } from './label.service';
import { BoardDetails } from '../models/board.model';

function createMockBoardDetails(overrides: Partial<BoardDetails> = {}): BoardDetails {
  return {
    id: 'board-1',
    name: 'Test Board',
    prefix: 'TST',
    projectId: 'project-1',
    createdAt: '2024-01-01',
    updatedAt: '2024-01-01',
    columns: [],
    cards: [],
    labels: [
      { id: 'label-1', name: 'Bug', color: 'red' },
      { id: 'label-2', name: 'Feature', color: 'blue' },
    ],
    ...overrides,
  };
}

describe('BoardSettingsFacade', () => {
  let facade: BoardSettingsFacade;

  let boardService: {
    getBoardDetails: ReturnType<typeof vi.fn>;
    updateBoardSettings: ReturnType<typeof vi.fn>;
    deleteBoard: ReturnType<typeof vi.fn>;
  };
  let labelService: {
    addLabel: ReturnType<typeof vi.fn>;
    updateLabel: ReturnType<typeof vi.fn>;
    removeLabel: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    boardService = {
      getBoardDetails: vi.fn(),
      updateBoardSettings: vi.fn(),
      deleteBoard: vi.fn(),
    };
    labelService = {
      addLabel: vi.fn(),
      updateLabel: vi.fn(),
      removeLabel: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        BoardSettingsFacade,
        { provide: BoardService, useValue: boardService },
        { provide: LabelService, useValue: labelService },
      ],
    });

    facade = TestBed.inject(BoardSettingsFacade);
  });

  describe('loadBoardSettings', () => {
    it('should set name, prefix, labels, and loading on success', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));

      facade.loadBoardSettings('project-1', 'board-1');

      expect(facade.boardName()).toBe('Test Board');
      expect(facade.originalBoardName()).toBe('Test Board');
      expect(facade.boardPrefix()).toBe('TST');
      expect(facade.originalBoardPrefix()).toBe('TST');
      expect(facade.boardLabels()).toEqual(mockBoard.labels);
      expect(facade.isLoading()).toBe(false);
      expect(facade.error()).toBeNull();
    });

    it('should set error on failure', () => {
      boardService.getBoardDetails.mockReturnValue(throwError(() => new Error('fail')));

      facade.loadBoardSettings('project-1', 'board-1');

      expect(facade.error()).toBe('Failed to load board settings');
      expect(facade.isLoading()).toBe(false);
    });

    it('should set isLoading to true while loading', () => {
      boardService.getBoardDetails.mockReturnValue(new Subject());

      facade.loadBoardSettings('project-1', 'board-1');

      expect(facade.isLoading()).toBe(true);
    });
  });

  describe('updateBoardName', () => {
    it('should update the boardName signal', () => {
      facade.updateBoardName('New Name');

      expect(facade.boardName()).toBe('New Name');
    });
  });

  describe('updateBoardPrefix', () => {
    it('should update the boardPrefix signal with uppercased value', () => {
      facade.updateBoardPrefix('abc');

      expect(facade.boardPrefix()).toBe('ABC');
    });
  });

  describe('hasChanges computed', () => {
    it('should detect name differences', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoardSettings('project-1', 'board-1');

      expect(facade.hasChanges()).toBe(false);

      facade.updateBoardName('Changed Name');
      expect(facade.hasChanges()).toBe(true);
    });

    it('should detect prefix differences', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoardSettings('project-1', 'board-1');

      expect(facade.hasChanges()).toBe(false);

      facade.updateBoardPrefix('NEW');
      expect(facade.hasChanges()).toBe(true);
    });

    it('should trim name for comparison', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoardSettings('project-1', 'board-1');

      facade.updateBoardName('Test Board   ');
      expect(facade.hasChanges()).toBe(false);
    });
  });

  describe('saveBoardSettings', () => {
    it('should call boardService and update originalName and prefix on success', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoardSettings('project-1', 'board-1');

      facade.updateBoardName('Updated Name');

      boardService.updateBoardSettings.mockReturnValue(of({ id: 'board-1', name: 'Updated Name', prefix: 'TST' }));

      facade.saveBoardSettings('project-1', 'board-1');

      expect(boardService.updateBoardSettings).toHaveBeenCalledWith('project-1', 'board-1', { name: 'Updated Name', prefix: 'TST' });
      expect(facade.originalBoardName()).toBe('Updated Name');
      expect(facade.originalBoardPrefix()).toBe('TST');
      expect(facade.isSaving()).toBe(false);
    });

    it('should skip when no changes', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoardSettings('project-1', 'board-1');

      facade.saveBoardSettings('project-1', 'board-1');

      expect(boardService.updateBoardSettings).not.toHaveBeenCalled();
    });

    it('should skip when name is empty', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoardSettings('project-1', 'board-1');

      facade.updateBoardName('   ');
      facade.saveBoardSettings('project-1', 'board-1');

      expect(boardService.updateBoardSettings).not.toHaveBeenCalled();
    });

    it('should skip when already saving', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoardSettings('project-1', 'board-1');

      facade.updateBoardName('New Name');
      boardService.updateBoardSettings.mockReturnValue(new Subject()); // never completes

      facade.saveBoardSettings('project-1', 'board-1');
      facade.saveBoardSettings('project-1', 'board-1');

      expect(boardService.updateBoardSettings).toHaveBeenCalledTimes(1);
    });
  });

  describe('deleteBoard', () => {
    it('should delegate to boardService', () => {
      boardService.deleteBoard.mockReturnValue(of(undefined));

      const result = facade.deleteBoard('project-1', 'board-1');

      expect(boardService.deleteBoard).toHaveBeenCalledWith('project-1', 'board-1');
      expect(result).toBeDefined();
    });
  });

  describe('addLabel', () => {
    it('should append label to boardLabels on success', () => {
      const mockBoard = createMockBoardDetails({ labels: [] });
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoardSettings('project-1', 'board-1');

      const newLabel = { id: 'label-new', name: 'Urgent', color: 'orange' };
      labelService.addLabel.mockReturnValue(of(newLabel));

      facade.addLabel('project-1', 'board-1', 'Urgent', 'orange');

      expect(labelService.addLabel).toHaveBeenCalledWith('project-1', 'board-1', { name: 'Urgent', color: 'orange' });
      expect(facade.boardLabels()).toEqual([newLabel]);
    });
  });

  describe('updateLabel', () => {
    it('should update label in-place on success', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoardSettings('project-1', 'board-1');

      labelService.updateLabel.mockReturnValue(of(undefined));

      facade.updateLabel('project-1', 'board-1', 'label-1', 'Critical', 'orange');

      expect(labelService.updateLabel).toHaveBeenCalledWith('project-1', 'board-1', 'label-1', { name: 'Critical', color: 'orange' });
      const updated = facade.boardLabels().find(l => l.id === 'label-1');
      expect(updated?.name).toBe('Critical');
      expect(updated?.color).toBe('orange');
    });
  });

  describe('deleteLabel', () => {
    it('should remove label from list on success', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoardSettings('project-1', 'board-1');

      labelService.removeLabel.mockReturnValue(of(undefined));

      facade.deleteLabel('project-1', 'board-1', 'label-1');

      expect(labelService.removeLabel).toHaveBeenCalledWith('project-1', 'board-1', 'label-1');
      expect(facade.boardLabels().find(l => l.id === 'label-1')).toBeUndefined();
    });
  });

  describe('sortedLabels computed', () => {
    it('should return labels sorted alphabetically', () => {
      const mockBoard = createMockBoardDetails({
        labels: [
          { id: '1', name: 'Zebra', color: 'red' },
          { id: '2', name: 'Alpha', color: 'blue' },
          { id: '3', name: 'Middle', color: 'green' },
        ],
      });
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoardSettings('project-1', 'board-1');

      const sorted = facade.sortedLabels();

      expect(sorted.map(l => l.name)).toEqual(['Alpha', 'Middle', 'Zebra']);
    });
  });
});
