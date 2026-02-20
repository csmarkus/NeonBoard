import { initTestEnvironment } from '../../../../test-setup';
import { TestBed } from '@angular/core/testing';
import { of, throwError, Subject } from 'rxjs';
import { BoardStateFacade } from './board-state.facade';

initTestEnvironment();
import { BoardService } from './board.service';
import { ColumnService } from './column.service';
import { CardService } from './card.service';
import { DrawerService } from './drawer.service';
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
      { id: 'col-1', name: 'To Do', position: 0, boardId: 'board-1' },
      { id: 'col-2', name: 'Done', position: 1, boardId: 'board-1' },
    ],
    cards: [
      { id: 'card-1', cardNumber: 1, friendlyId: 'TST-1', title: 'Card 1', description: '', columnId: 'col-1', position: 1, labels: [], createdAt: '', updatedAt: '' },
      { id: 'card-2', cardNumber: 2, friendlyId: 'TST-2', title: 'Card 2', description: '', columnId: 'col-1', position: 0, labels: [], createdAt: '', updatedAt: '' },
      { id: 'card-3', cardNumber: 3, friendlyId: 'TST-3', title: 'Card 3', description: '', columnId: 'col-2', position: 0, labels: [], createdAt: '', updatedAt: '' },
    ],
    labels: [{ id: 'label-1', name: 'Bug', color: 'red' }],
    ...overrides,
  };
}

describe('BoardStateFacade', () => {
  let facade: BoardStateFacade;
  let cardUpdated$: Subject<void>;
  let cardDeleted$: Subject<void>;

  let boardService: {
    getBoardDetails: ReturnType<typeof vi.fn>;
  };
  let columnService: {
    reorderColumns: ReturnType<typeof vi.fn>;
    addColumn: ReturnType<typeof vi.fn>;
    renameColumn: ReturnType<typeof vi.fn>;
    deleteColumn: ReturnType<typeof vi.fn>;
  };
  let cardService: {
    moveCard: ReturnType<typeof vi.fn>;
    addCard: ReturnType<typeof vi.fn>;
  };
  let drawerService: {
    setBoardLabels: ReturnType<typeof vi.fn>;
    openCardDrawer: ReturnType<typeof vi.fn>;
    cardUpdated$: Subject<void>;
    cardDeleted$: Subject<void>;
  };

  beforeEach(() => {
    cardUpdated$ = new Subject<void>();
    cardDeleted$ = new Subject<void>();

    boardService = { getBoardDetails: vi.fn() };
    columnService = {
      reorderColumns: vi.fn(),
      addColumn: vi.fn(),
      renameColumn: vi.fn(),
      deleteColumn: vi.fn(),
    };
    cardService = { moveCard: vi.fn(), addCard: vi.fn() };
    drawerService = {
      setBoardLabels: vi.fn(),
      openCardDrawer: vi.fn(),
      cardUpdated$: cardUpdated$.asObservable() as never,
      cardDeleted$: cardDeleted$.asObservable() as never,
    };

    TestBed.configureTestingModule({
      providers: [
        BoardStateFacade,
        { provide: BoardService, useValue: boardService },
        { provide: ColumnService, useValue: columnService },
        { provide: CardService, useValue: cardService },
        { provide: DrawerService, useValue: drawerService },
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
      columnService.addColumn.mockReturnValue(of({ id: 'col-3', name: 'New', position: 2, boardId: 'board-1' }));

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

      facade.moveCard('project-1', 'board-1', 'card-1', 'col-2', 0);

      expect(cardService.moveCard).toHaveBeenCalledWith('project-1', 'board-1', 'card-1', {
        targetColumnId: 'col-2',
        targetPosition: 0,
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
    it('should delegate to drawerService', () => {
      const card: Card = { id: 'card-1', cardNumber: 1, friendlyId: 'TST-1', title: 'Test', description: '', columnId: 'col-1', position: 0, labels: [], createdAt: '', updatedAt: '' };

      facade.openCardDrawer(card, 'project-1', 'board-1');

      expect(drawerService.openCardDrawer).toHaveBeenCalledWith(card, 'project-1', 'board-1');
    });
  });

  describe('cardsByColumn computed', () => {
    it('should group and sort cards by column', () => {
      const mockBoard = createMockBoardDetails();
      boardService.getBoardDetails.mockReturnValue(of(mockBoard));
      facade.loadBoard('project-1', 'board-1');

      const result = facade.cardsByColumn();

      expect(Object.keys(result)).toEqual(['col-1', 'col-2']);
      // col-1 cards sorted by position: card-2 (pos 0) before card-1 (pos 1)
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
});
