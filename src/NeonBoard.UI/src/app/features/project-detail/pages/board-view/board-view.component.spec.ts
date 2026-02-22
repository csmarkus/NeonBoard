import { initTestEnvironment } from '../../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { signal } from '@angular/core';
import { convertToParamMap } from '@angular/router';
import { of, Subject } from 'rxjs';
import { Title } from '@angular/platform-browser';
import { BoardViewComponent } from './board-view.component';
import { ActivatedRoute } from '@angular/router';
import { ProjectService } from '../../../projects/services/project.service';
import { BoardService } from '../../services/board.service';
import { BoardStateFacade } from '../../services/board-state.facade';
import { Project } from '../../../projects/models/project.model';

initTestEnvironment();

const mockProject: Project = {
  id: 'p-1',
  shortId: 'p-short-1',
  name: 'My Project',
  ownerId: 'user-1',
  createdAt: '',
  updatedAt: '',
} as Project;

describe('BoardViewComponent', () => {
  let fixture: ComponentFixture<BoardViewComponent>;
  let component: BoardViewComponent;
  let mockFacade: { board: ReturnType<typeof signal> };
  let mockTitle: { setTitle: ReturnType<typeof vi.fn> };
  let paramMap$: Subject<ReturnType<typeof convertToParamMap>>;

  beforeEach(() => {
    paramMap$ = new Subject();
    mockFacade = { board: signal(null) };
    mockTitle = { setTitle: vi.fn() };

    const mockRoute = {
      parent: { snapshot: { paramMap: convertToParamMap({ shortId: 'p-short-1' }) } },
      snapshot: { paramMap: convertToParamMap({ slug: 'sprint-board' }) },
      paramMap: paramMap$.asObservable(),
    };

    TestBed.configureTestingModule({
      imports: [BoardViewComponent],
      providers: [
        { provide: ActivatedRoute, useValue: mockRoute },
        { provide: ProjectService, useValue: { getProjectByShortId: vi.fn().mockReturnValue(of(mockProject)) } },
        { provide: BoardService, useValue: { getBoardsByProject: vi.fn().mockReturnValue(of([{ id: 'b-1', slug: 'sprint-board', name: 'Sprint Board', prefix: 'SPR', projectId: 'p-1', createdAt: '', updatedAt: '', columnCount: 0 }])) } },
        { provide: BoardStateFacade, useValue: mockFacade },
        { provide: Title, useValue: mockTitle },
      ],
    });
    TestBed.overrideTemplate(BoardViewComponent, '');

    fixture = TestBed.createComponent(BoardViewComponent);
    component = fixture.componentInstance;
  });

  it('extracts shortId from parent route snapshot', () => {
    component.ngOnInit();
    expect(component.shortId()).toBe('p-short-1');
  });

  it('loads project name from ProjectService using shortId', () => {
    component.ngOnInit();
    expect(component.projectName()).toBe('My Project');
  });

  it('resolves slug to boardId via BoardService', () => {
    component.ngOnInit();
    expect(component.boardId()).toBe('b-1');
  });

  it('extracts slug from paramMap and sets slug signal', () => {
    component.ngOnInit();
    paramMap$.next(convertToParamMap({ slug: 'sprint-board' }));
    expect(component.slug()).toBe('sprint-board');
  });

  it('sets browser title when boardName signal has a value', () => {
    component.ngOnInit();
    mockFacade.board.set({ id: 'b-1', name: 'Sprint Board', prefix: 'SPR', projectId: 'p-1', columns: [], cards: [], labels: [], createdAt: '', updatedAt: '' });
    TestBed.flushEffects();

    expect(mockTitle.setTitle).toHaveBeenCalledWith('Sprint Board | NeonBoard');
  });
});
