import { initTestEnvironment } from '../../../../test-setup';
import { TestBed } from '@angular/core/testing';
import { of, throwError, Subject } from 'rxjs';
import { BoardStateFacade } from './board-state.facade';

initTestEnvironment();
import { BoardService } from './board.service';
import { ColumnService } from './column.service';
import { CardService } from './card.service';
import { DrawerService } from './drawer.service';
import { BoardHubService } from './board-hub.service';
import { BoardDetails } from '../models/board.model';
import { Card } from '../models/card.model';

function createMockBoardDetails(overrides: Partial<BoardDetails> = {}): BoardDetails {
  return {
    id: 'board-1',
    name: 'Test Board',
    prefix: 'TST',
    projectId: 'project-1',
    createdAt: '2024-01-01',
    updatedAt: '2024-01-01',
    columns: [
      { id: 'col-1', name: 'To Do', position: 'a0', boardId: 'board-1' },
      { id: 'col-2', name: 'Done', position: 'a1', boardId: 'board-1' },
    ],
    cards: [
      { id: 'card-1', cardNumber: 1, displayId: 'TST-1', title: 'Card 1', description: '', columnId: 'col-1', position: 'a1', labels: [], createdAt: '', updatedAt: '', archivedAt: null },
      { id: 'card-2', cardNumber: 2, displayId: 'TST-2', title: 'Card 2', description: '', columnId: 'col-1', position: 'a0', labels: [], createdAt: '', updatedAt: '', archivedAt: null },
      { id: 'card-3', cardNumber: 3, displayId: 'TST-3', title: 'Card 3', description: '', columnId: 'col-2', position: 'a0', labels: [], createdAt: '', updatedAt: '', archivedAt: null },
    ],
    labels: [{ id: 'label-1', name: 'Bug', color: 'red' }],
    ...overrides,
  };
}

describe('BoardStateFacade', () => {
  let facade: BoardStateFacade;
  let cardUpdated$: Subject<void>;
  let cardDeleted$: Subject<void>;
  let cardArchived$: Subject<void>;

  let boardService: {
    getBoardDetails: ReturnType<typeof vi.fn>;
  };
  let columnService: {
    reorderColumns: ReturnType<typeof vi.fn>;
    moveColumn: ReturnType<typeof vi.fn>;
    addColumn: ReturnType<typeof vi.fn>;
    renameColumn: ReturnType<typeof vi.fn>;
    deleteColumn: ReturnType<typeof vi.fn>;
  };
  let cardService: {
    moveCard: ReturnType<typeof vi.fn>;
    addCard: ReturnType<typeof vi.fn>;
    getCardDetail: ReturnType<typeof vi.fn>;
  };
  let drawerService: {
    setBoardLabels: ReturnType<typeof vi.fn>;
    openCardDrawer: ReturnType<typeof vi.fn>;
    initialCardActivity: { set: ReturnType<typeof vi.fn> };
    cardUpdated$: Subject<void>;
    cardDeleted$: Subject<void>;
    cardArchived$: Subject<void>;
  };
  let boardHubService: {
    joinBoard: ReturnType<typeof vi.fn>;
    leaveBoard: ReturnType<typeof vi.fn>;
    onEvent: ReturnType<typeof vi.fn>;
    offAllEvents: ReturnType<typeof vi.fn>;
    currentUserId: ReturnType<typeof vi.fn>;
    connectionState: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    cardUpdated$ = new Subject<void>();
    cardDeleted$ = new Subject<void>();
    cardArchived$ = new Subject<void>();

    boardService = { getBoardDetails: vi.fn() };
    columnService = {
      reorderColumns: vi.fn(),
      moveColumn: vi.fn(),
      addColumn: vi.fn(),
      renameColumn: vi.fn(),
      deleteColumn: vi.fn(),
    };
    cardService = { moveCard: vi.fn(), addCard: vi.fn(), getCardDetail: vi.fn() };
    drawerService = {
      setBoardLabels: vi.fn(),
      openCardDrawer: vi.fn(),
      initialCardActivity: { set: vi.fn() },
      cardUpdated$: cardUpdated$.asObservable() as never,
      cardDeleted$: cardDeleted$.asObservable() as never,
      cardArchived$: cardArchived$.asObservable() as never,
    };
    boardHubService = {
      joinBoard: vi.fn().mockResolvedValue(undefined),
      leaveBoard: vi.fn().mockResolvedValue(undefined),
      onEvent: vi.fn(),
      offAllEvents: vi.fn(),
      currentUserId: vi.fn().mockReturnValue(null),
      connectionState: vi.fn().mockReturnValue('disconnected'),
    };

    TestBed.configureTestingModule({
      providers: [
        BoardStateFacade,
        { provide: BoardService, useValue: boardService },
        { provide: ColumnService, useValue: columnService },
        { provide: CardService, useValue: cardService },
        { provide: DrawerService, useValue: drawerService },
        { provide: BoardHubService, useValue: boardHubService },
      ],
    });

    facade = TestBed.inject(BoardStateFacade);
  });

  describe('loadBoard', () => {
    it('should set board data and call setBoardLabels on success', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));

      facade.loadBoard('project-1', 'board-1');

      expect(facade.board()).toEqual(mockBoard);
      expect(facade.isLoading()).toBe(false);
      expect(facade.error()).toBeNull();
      expect(drawerService.setBoardLabels).toHaveBeenCalledWith(mockBoard.labels);
    });

    it('should set isLoading to true when showLoading is true', () => {
      boardService.getBoardDetails.mockReturnValue(new Subject());

      facade.loadBoard('project-1', 'board-1', true);

      expect(facade.isLoading()).toBe(true);
    });

    it('should not set isLoading when showLoading is false', () => {
      boardService.getBoardDetails.mockReturnValue(new Subject());

      facade.loadBoard('project-1', 'board-1', false);

      expect(facade.isLoading()).toBe(false);
    });

    it('should set error and clear loading on failure', () => {
      boardService.getBoardDetails.mockReturnValue(throwError(() => new Error('fail')));

      facade.loadBoard('project-1', 'board-1');

      expect(facade.error()).toBe('Failed to load board');
      expect(facade.isLoading()).toBe(false);
      expect(facade.board()).toBeNull();
    });

    it('should clear selected label filters when navigating to a different board', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoard('project-1', 'board-1');
      facade.toggleLabelFilter('label-1');
      expect(facade.isFilterActive()).toBe(true);

      facade.loadBoard('project-1', 'board-2');

      expect(facade.isFilterActive()).toBe(false);
      expect(facade.selectedLabelIds().size).toBe(0);
    });

    it('should not clear selected label filters when reloading the same board', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoard('project-1', 'board-1');
      facade.toggleLabelFilter('label-1');
      expect(facade.isFilterActive()).toBe(true);

      facade.loadBoard('project-1', 'board-1', false);

      expect(facade.isFilterActive()).toBe(true);
    });
  });

  describe('reorderColumns', () => {
    it('should optimistically reorder columns and call columnService', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoard('project-1', 'board-1');

      columnService.reorderColumns.mockReturnValue(of(undefined));

      facade.reorderColumns('project-1', 'board-1', ['col-2', 'col-1']);

      expect(facade.board()!.columns[0].id).toBe('col-2');
      expect(facade.board()!.columns[1].id).toBe('col-1');
      expect(columnService.reorderColumns).toHaveBeenCalledWith('project-1', 'board-1', {
        columnIds: ['col-2', 'col-1'],
      });
    });

    it('should reload board on reorder error', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoard('project-1', 'board-1');

      columnService.reorderColumns.mockReturnValue(throwError(() => new Error('fail')));

      facade.reorderColumns('project-1', 'board-1', ['col-2', 'col-1']);

      // getBoardDetails called once for initial load, once for reload after error
      expect(boardService.getBoardDetails).toHaveBeenCalledTimes(2);
    });

    it('should do nothing if board is null', () => {
      facade.reorderColumns('project-1', 'board-1', ['col-1']);

      expect(columnService.reorderColumns).not.toHaveBeenCalled();
    });
  });

  describe('addColumn', () => {
    it('should call columnService and reload board on success', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      columnService.addColumn.mockReturnValue(of({ id: 'col-3', name: 'New', position: 'a2', boardId: 'board-1' }));

      facade.addColumn('project-1', 'board-1', 'New');

      expect(columnService.addColumn).toHaveBeenCalledWith('project-1', 'board-1', { name: 'New' });
      expect(boardService.getBoardDetails).toHaveBeenCalled();
    });
  });

  describe('renameColumn', () => {
    it('should call columnService and reload board on success', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      columnService.renameColumn.mockReturnValue(of(undefined));

      facade.renameColumn('project-1', 'board-1', 'col-1', 'Renamed');

      expect(columnService.renameColumn).toHaveBeenCalledWith('project-1', 'board-1', 'col-1', { newName: 'Renamed' });
      expect(boardService.getBoardDetails).toHaveBeenCalled();
    });
  });

  describe('deleteColumn', () => {
    it('should call columnService and reload board on success', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      columnService.deleteColumn.mockReturnValue(of(undefined));

      facade.deleteColumn('project-1', 'board-1', 'col-1');

      expect(columnService.deleteColumn).toHaveBeenCalledWith('project-1', 'board-1', 'col-1');
      expect(boardService.getBoardDetails).toHaveBeenCalled();
    });
  });

  describe('moveCard', () => {
    it('should call cardService.moveCard', () => {
      cardService.moveCard.mockReturnValue(of(undefined));

      facade.moveCard('project-1', 'board-1', 'card-1', 'col-2', 'a0');

      expect(cardService.moveCard).toHaveBeenCalledWith('project-1', 'board-1', 'card-1', {
        targetColumnId: 'col-2',
        newPosition: 'a0',
      });
    });
  });

  describe('addCard', () => {
    it('should call cardService and reload board on success', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      cardService.addCard.mockReturnValue(of({ id: 'card-new' }));

      facade.addCard('project-1', 'board-1', 'col-1', 'New Card');

      expect(cardService.addCard).toHaveBeenCalledWith('project-1', 'board-1', {
        columnId: 'col-1',
        title: 'New Card',
        description: '',
      });
      expect(boardService.getBoardDetails).toHaveBeenCalled();
    });
  });

  describe('openCardDrawer', () => {
    it('should delegate to drawerService and fetch card detail', () => {
      const card: Card = { id: 'card-1', cardNumber: 1, displayId: 'TST-1', title: 'Test', description: '', columnId: 'col-1', position: 'a0', labels: [], createdAt: '', updatedAt: '', archivedAt: null };
      const mockActivity = { entries: [], nextCursor: null };
      cardService.getCardDetail.mockReturnValue(of({ ...card, activity: mockActivity }));

      facade.openCardDrawer(card, 'project-1', 'board-1');

      expect(drawerService.openCardDrawer).toHaveBeenCalledWith(card, 'project-1', 'board-1');
      expect(cardService.getCardDetail).toHaveBeenCalledWith('project-1', 'board-1', 'card-1');
      expect(drawerService.initialCardActivity.set).toHaveBeenCalledWith(mockActivity);
    });
  });

  describe('cardsByColumn computed', () => {
    it('should group and sort cards by column', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoard('project-1', 'board-1');

      const result = facade.cardsByColumn();

      expect(Object.keys(result)).toEqual(['col-1', 'col-2']);
      // col-1 cards sorted by position: card-2 (pos a0) before card-1 (pos a1)
      expect(result['col-1'].map(c => c.id)).toEqual(['card-2', 'card-1']);
      expect(result['col-2'].map(c => c.id)).toEqual(['card-3']);
    });

    it('should return empty record when no board is loaded', () => {
      expect(facade.cardsByColumn()).toEqual({});
    });
  });

  describe('constructor subscriptions', () => {
    it('should reload board when cardUpdated$ emits', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoard('project-1', 'board-1');

      boardService.getBoardDetails.mockClear();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));

      cardUpdated$.next();

      expect(boardService.getBoardDetails).toHaveBeenCalledWith('project-1', 'board-1');
    });

    it('should reload board when cardDeleted$ emits', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoard('project-1', 'board-1');

      boardService.getBoardDetails.mockClear();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));

      cardDeleted$.next();

      expect(boardService.getBoardDetails).toHaveBeenCalledWith('project-1', 'board-1');
    });

    it('should not reload when no board is loaded and events emit', () => {
      cardUpdated$.next();
      cardDeleted$.next();

      expect(boardService.getBoardDetails).not.toHaveBeenCalled();
    });
  });

  describe('toggleLabelFilter', () => {
    it('should add a label id to selectedLabelIds', () => {
      facade.toggleLabelFilter('label-1');
      expect(facade.selectedLabelIds().has('label-1')).toBe(true);
    });

    it('should remove a label id when toggled a second time', () => {
      facade.toggleLabelFilter('label-1');
      facade.toggleLabelFilter('label-1');
      expect(facade.selectedLabelIds().has('label-1')).toBe(false);
    });

    it('should support multiple selected labels independently', () => {
      facade.toggleLabelFilter('label-1');
      facade.toggleLabelFilter('label-2');
      expect(facade.selectedLabelIds().has('label-1')).toBe(true);
      expect(facade.selectedLabelIds().has('label-2')).toBe(true);
    });
  });

  describe('clearLabelFilter', () => {
    it('should clear all selected labels', () => {
      facade.toggleLabelFilter('label-1');
      facade.toggleLabelFilter('label-2');
      facade.clearLabelFilter();
      expect(facade.selectedLabelIds().size).toBe(0);
    });
  });

  describe('isFilterActive computed', () => {
    it('should be false when no labels are selected', () => {
      expect(facade.isFilterActive()).toBe(false);
    });

    it('should be true when at least one label is selected', () => {
      facade.toggleLabelFilter('label-1');
      expect(facade.isFilterActive()).toBe(true);
    });

    it('should return false after clearing', () => {
      facade.toggleLabelFilter('label-1');
      facade.clearLabelFilter();
      expect(facade.isFilterActive()).toBe(false);
    });
  });

  describe('filteredCardsByColumn computed', () => {
    it('should return the same result as cardsByColumn when no filter is active', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoard('project-1', 'board-1');

      expect(facade.filteredCardsByColumn()).toEqual(facade.cardsByColumn());
    });

    it('should show only cards that have a selected label', () => {
      const mockBoard = createMockBoardDetails({
        cards: [
          { id: 'card-1', cardNumber: 1, displayId: 'TST-1', title: 'Card 1', description: '', columnId: 'col-1', position: 'a0', labels: [{ id: 'label-1', name: 'Bug', color: 'red' }], createdAt: '', updatedAt: '', archivedAt: null },
          { id: 'card-2', cardNumber: 2, displayId: 'TST-2', title: 'Card 2', description: '', columnId: 'col-1', position: 'a1', labels: [], createdAt: '', updatedAt: '', archivedAt: null },
        ],
      });
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoard('project-1', 'board-1');

      facade.toggleLabelFilter('label-1');

      expect(facade.filteredCardsByColumn()['col-1'].map(c => c.id)).toEqual(['card-1']);
    });

    it('should return an empty array for a column where no cards match', () => {
      const mockBoard = createMockBoardDetails({
        cards: [
          { id: 'card-1', cardNumber: 1, displayId: 'TST-1', title: 'Card 1', description: '', columnId: 'col-1', position: 'a0', labels: [], createdAt: '', updatedAt: '', archivedAt: null },
        ],
      });
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoard('project-1', 'board-1');

      facade.toggleLabelFilter('label-1');

      expect(facade.filteredCardsByColumn()['col-1']).toEqual([]);
    });

    it('should match cards with any of the selected labels (OR logic)', () => {
      const mockBoard = createMockBoardDetails({
        cards: [
          { id: 'card-1', cardNumber: 1, displayId: 'TST-1', title: 'Card 1', description: '', columnId: 'col-1', position: 'a0', labels: [{ id: 'label-1', name: 'Bug', color: 'red' }], createdAt: '', updatedAt: '', archivedAt: null },
          { id: 'card-2', cardNumber: 2, displayId: 'TST-2', title: 'Card 2', description: '', columnId: 'col-1', position: 'a1', labels: [{ id: 'label-2', name: 'Feature', color: 'blue' }], createdAt: '', updatedAt: '', archivedAt: null },
          { id: 'card-3', cardNumber: 3, displayId: 'TST-3', title: 'Card 3', description: '', columnId: 'col-1', position: 'a2', labels: [], createdAt: '', updatedAt: '', archivedAt: null },
        ],
        labels: [
          { id: 'label-1', name: 'Bug', color: 'red' },
          { id: 'label-2', name: 'Feature', color: 'blue' },
        ],
      });
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoard('project-1', 'board-1');

      facade.toggleLabelFilter('label-1');
      facade.toggleLabelFilter('label-2');

      expect(facade.filteredCardsByColumn()['col-1'].map(c => c.id)).toEqual(['card-1', 'card-2']);
    });

    it('should show all cards again after clearing the filter', () => {
      const mockBoard = createMockBoardDetails({
        cards: [
          { id: 'card-1', cardNumber: 1, displayId: 'TST-1', title: 'Card 1', description: '', columnId: 'col-1', position: 'a0', labels: [{ id: 'label-1', name: 'Bug', color: 'red' }], createdAt: '', updatedAt: '', archivedAt: null },
          { id: 'card-2', cardNumber: 2, displayId: 'TST-2', title: 'Card 2', description: '', columnId: 'col-1', position: 'a1', labels: [], createdAt: '', updatedAt: '', archivedAt: null },
        ],
      });
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoard('project-1', 'board-1');

      facade.toggleLabelFilter('label-1');
      facade.clearLabelFilter();

      expect(facade.filteredCardsByColumn()['col-1'].map(c => c.id)).toEqual(['card-1', 'card-2']);
    });
  });
});
