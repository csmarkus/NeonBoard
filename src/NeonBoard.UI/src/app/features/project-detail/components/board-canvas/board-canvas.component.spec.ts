import { initTestEnvironment } from '../../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { signal } from '@angular/core';
import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { BoardCanvasComponent } from './board-canvas.component';
import { BoardStateFacade } from '../../services/board-state.facade';
import { Column } from '../../models/column.model';
import { Card } from '../../models/card.model';

initTestEnvironment();

function mockColumnDrop(
  previousIndex: number,
  currentIndex: number,
): CdkDragDrop<Column[]> {
  return {
    previousIndex,
    currentIndex,
    item: { data: {} },
    container: { data: [], id: 'container' },
    previousContainer: { data: [], id: 'container' },
    isPointerOverContainer: true,
    distance: { x: 0, y: 0 },
    dropPoint: { x: 0, y: 0 },
    event: new MouseEvent('drop'),
  } as unknown as CdkDragDrop<Column[]>;
}

function mockCardDrop(
  card: Card,
  previousIndex: number,
  currentIndex: number,
  containerData: Card[],
): CdkDragDrop<Card[]> {
  const container = { data: containerData, id: 'container' };
  return {
    item: { data: card },
    container,
    previousContainer: container,
    previousIndex,
    currentIndex,
    isPointerOverContainer: true,
    distance: { x: 0, y: 0 },
    dropPoint: { x: 0, y: 0 },
    event: new MouseEvent('drop'),
  } as unknown as CdkDragDrop<Card[]>;
}

describe('BoardCanvasComponent', () => {
  let fixture: ComponentFixture<BoardCanvasComponent>;
  let component: BoardCanvasComponent;
  let mockFacade: {
    board: ReturnType<typeof signal>;
    columns: ReturnType<typeof signal<Column[]>>;
    filteredCardsByColumn: ReturnType<typeof signal>;
    labels: ReturnType<typeof signal>;
    isLoading: ReturnType<typeof signal<boolean>>;
    error: ReturnType<typeof signal>;
    loadBoard: ReturnType<typeof vi.fn>;
    addColumn: ReturnType<typeof vi.fn>;
    reorderColumns: ReturnType<typeof vi.fn>;
    renameColumn: ReturnType<typeof vi.fn>;
    deleteColumn: ReturnType<typeof vi.fn>;
    moveCard: ReturnType<typeof vi.fn>;
    addCard: ReturnType<typeof vi.fn>;
    openCardDrawer: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    mockFacade = {
      board: signal(null),
      columns: signal<Column[]>([]),
      filteredCardsByColumn: signal({}),
      labels: signal([]),
      isLoading: signal(false),
      error: signal(null),
      loadBoard: vi.fn(),
      addColumn: vi.fn(),
      reorderColumns: vi.fn(),
      renameColumn: vi.fn(),
      deleteColumn: vi.fn(),
      moveCard: vi.fn(),
      addCard: vi.fn(),
      openCardDrawer: vi.fn(),
    };

    TestBed.configureTestingModule({
      imports: [BoardCanvasComponent],
      providers: [
        { provide: BoardStateFacade, useValue: mockFacade },
        provideNoopAnimations(),
      ],
    });
    TestBed.overrideTemplate(BoardCanvasComponent, '');

    fixture = TestBed.createComponent(BoardCanvasComponent);
    component = fixture.componentInstance;
  });

  describe('initialization', () => {
    it('calls facade.loadBoard(projectId, boardId, true) when inputs are set', () => {
      fixture.componentRef.setInput('projectId', 'p-1');
      fixture.componentRef.setInput('boardId', 'b-1');
      TestBed.flushEffects();

      expect(mockFacade.loadBoard).toHaveBeenCalledWith('p-1', 'b-1', true);
    });
  });

  describe('addColumn', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('projectId', 'p-1');
      fixture.componentRef.setInput('boardId', 'b-1');
      TestBed.flushEffects();
      mockFacade.loadBoard.mockClear();
    });

    it('calls facade.addColumn with the provided name', () => {
      component.addColumn('New Column');

      expect(mockFacade.addColumn).toHaveBeenCalledWith('p-1', 'b-1', 'New Column');
    });

    it('resets isAddingColumn to false and clears newColumnName after adding', () => {
      component.isAddingColumn.set(true);
      component.newColumnName.set('My Column');

      component.addColumn('My Column');

      expect(component.isAddingColumn()).toBe(false);
      expect(component.newColumnName()).toBe('');
    });
  });

  describe('cancelAddColumn', () => {
    it('sets isAddingColumn to false and clears newColumnName', () => {
      component.isAddingColumn.set(true);
      component.newColumnName.set('Draft');

      component.cancelAddColumn();

      expect(component.isAddingColumn()).toBe(false);
      expect(component.newColumnName()).toBe('');
    });
  });

  describe('column events', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('projectId', 'p-1');
      fixture.componentRef.setInput('boardId', 'b-1');
      TestBed.flushEffects();
    });

    it('onColumnRenamed delegates columnId and newName to facade.renameColumn', () => {
      component.onColumnRenamed({ columnId: 'col-1', newName: 'Renamed' });

      expect(mockFacade.renameColumn).toHaveBeenCalledWith('p-1', 'b-1', 'col-1', 'Renamed');
    });

    it('onColumnDeleted delegates columnId to facade.deleteColumn', () => {
      component.onColumnDeleted('col-1');

      expect(mockFacade.deleteColumn).toHaveBeenCalledWith('p-1', 'b-1', 'col-1');
    });

    it('dropColumn calls facade.reorderColumns with the reordered column ID array', () => {
      const col1: Column = { id: 'col-1', name: 'To Do', position: 0, boardId: 'b-1' };
      const col2: Column = { id: 'col-2', name: 'Done', position: 1, boardId: 'b-1' };
      mockFacade.columns.set([col1, col2]);

      const event = mockColumnDrop(0, 1);
      component.dropColumn(event);

      expect(mockFacade.reorderColumns).toHaveBeenCalledWith('p-1', 'b-1', ['col-2', 'col-1']);
    });

    it('dropColumn does nothing when previousIndex equals currentIndex', () => {
      const col1: Column = { id: 'col-1', name: 'To Do', position: 0, boardId: 'b-1' };
      mockFacade.columns.set([col1]);

      const event = mockColumnDrop(0, 0);
      component.dropColumn(event);

      expect(mockFacade.reorderColumns).not.toHaveBeenCalled();
    });
  });

  describe('card events', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('projectId', 'p-1');
      fixture.componentRef.setInput('boardId', 'b-1');
      TestBed.flushEffects();
    });

    it('onCardAdded calls facade.addCard with projectId, boardId, columnId, title', () => {
      component.onCardAdded({ columnId: 'col-1', title: 'New Card' });

      expect(mockFacade.addCard).toHaveBeenCalledWith('p-1', 'b-1', 'col-1', 'New Card');
    });

    it('onCardSelected calls facade.openCardDrawer with card, projectId, boardId', () => {
      const card: Card = { id: 'card-1', cardNumber: 1, displayId: 'TST-1', title: 'Test', description: '', columnId: 'col-1', position: 0, labels: [], createdAt: '', updatedAt: '' };

      component.onCardSelected(card);

      expect(mockFacade.openCardDrawer).toHaveBeenCalledWith(card, 'p-1', 'b-1');
    });

    it('onCardDropped calls facade.moveCard with card id, target column id, target position', () => {
      const card: Card = { id: 'card-1', cardNumber: 1, displayId: 'TST-1', title: 'Test', description: '', columnId: 'col-1', position: 0, labels: [], createdAt: '', updatedAt: '' };
      const containerData = [card];
      const event = mockCardDrop(card, 0, 0, containerData);

      component.onCardDropped(event, 'col-2');

      expect(mockFacade.moveCard).toHaveBeenCalledWith('p-1', 'b-1', 'card-1', 'col-2', 0);
    });

    it('onCardDragStarted sets draggedCardHeight signal to event height', () => {
      component.onCardDragStarted(120);

      expect(component.draggedCardHeight()).toBe(120);
    });
  });
});
