import { initTestEnvironment } from '../../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { signal } from '@angular/core';
import { convertToParamMap } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { BoardViewComponent } from './board-view.component';
import { ActivatedRoute } from '@angular/router';
import { BoardStateFacade } from '../../services/board-state.facade';
import { ProjectContext } from '../../services/project-context.service';
import { Project } from '../../../projects/models/project.model';
import { Board } from '../../models/board.model';

initTestEnvironment();

const mockProject: Project = {
  id: 'p-1',
  shortId: 'p-short-1',
  name: 'My Project',
  ownerId: 'user-1',
  createdAt: '',
  updatedAt: '',
} as Project;

const mockBoards: Board[] = [
  { id: 'b-1', slug: 'sprint-board', name: 'Sprint Board', prefix: 'SPR', projectId: 'p-1', createdAt: '', updatedAt: '', columnCount: 0 },
];

describe('BoardViewComponent', () => {
  let fixture: ComponentFixture<BoardViewComponent>;
  let component: BoardViewComponent;
  let mockFacade: { board: ReturnType<typeof signal> };
  let mockTitle: { setTitle: ReturnType<typeof vi.fn> };
  let mockProjectContext: {
    project: ReturnType<typeof signal>;
    boards: ReturnType<typeof signal>;
    boardsLoaded: ReturnType<typeof signal>;
    projectId: ReturnType<typeof signal>;
    projectName: ReturnType<typeof signal>;
    shortId: ReturnType<typeof signal>;
    currentUserRole: ReturnType<typeof signal>;
  };

  beforeEach(() => {
    mockFacade = { board: signal(null) };
    mockTitle = { setTitle: vi.fn() };
    mockProjectContext = {
      project: signal(mockProject),
      boards: signal(mockBoards),
      boardsLoaded: signal(true),
      projectId: signal('p-1'),
      projectName: signal('My Project'),
      shortId: signal('p-short-1'),
      currentUserRole: signal(undefined),
      canEdit: () => true,
      isOwner: () => true,
    };

    const mockRoute = {
      snapshot: { paramMap: convertToParamMap({ slug: 'sprint-board' }) },
    };

    TestBed.configureTestingModule({
      imports: [BoardViewComponent],
      providers: [
        { provide: ActivatedRoute, useValue: mockRoute },
        { provide: ProjectContext, useValue: mockProjectContext },
        { provide: BoardStateFacade, useValue: mockFacade },
        { provide: Title, useValue: mockTitle },
      ],
    });
    TestBed.overrideTemplate(BoardViewComponent, '');

    fixture = TestBed.createComponent(BoardViewComponent);
    component = fixture.componentInstance;
  });

  it('reads shortId from ProjectContext', () => {
    expect(component.shortId()).toBe('p-short-1');
  });

  it('reads project name from ProjectContext', () => {
    expect(component.projectName()).toBe('My Project');
  });

  it('resolves slug to boardId from context boards', () => {
    TestBed.flushEffects();
    expect(component.boardId()).toBe('b-1');
  });

  it('extracts slug from route snapshot', () => {
    expect(component.slug()).toBe('sprint-board');
  });

  it('sets browser title when boardName signal has a value', () => {
    mockFacade.board.set({ id: 'b-1', name: 'Sprint Board', prefix: 'SPR', projectId: 'p-1', columns: [], cards: [], labels: [], createdAt: '', updatedAt: '' });
    TestBed.flushEffects();

    expect(mockTitle.setTitle).toHaveBeenCalledWith('Sprint Board | NeonBoard');
  });
});
