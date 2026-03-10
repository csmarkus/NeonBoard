import { initTestEnvironment } from '../../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { signal } from '@angular/core';
import { Subject } from 'rxjs';
import { Title } from '@angular/platform-browser';
import { ProjectComponent } from './project.component';
import { DrawerService } from '../../services/drawer.service';
import { ProjectContext } from '../../services/project-context.service';
import { Project } from '../../../projects/models/project.model';
import { Board } from '../../models/board.model';

initTestEnvironment();

const mockProject: Project = {
  id: 'p-1',
  shortId: 'p-short-1',
  name: 'Test Project',
  ownerId: 'user-1',
  createdAt: '',
  updatedAt: '',
} as Project;

const mockBoards: Board[] = [
  { id: 'b-1', name: 'Sprint 1', projectId: 'p-1', createdAt: '', updatedAt: '' } as Board,
];

describe('ProjectComponent', () => {
  let fixture: ComponentFixture<ProjectComponent>;
  let component: ProjectComponent;
  let boardCreated$: Subject<void>;
  let mockDrawerService: { boardCreated$: Subject<void>; openCreateBoardDrawer: ReturnType<typeof vi.fn> };
  let mockTitle: { setTitle: ReturnType<typeof vi.fn> };
  let mockProjectContext: {
    project: ReturnType<typeof signal>;
    boards: ReturnType<typeof signal>;
    boardsLoaded: ReturnType<typeof signal>;
    projectId: ReturnType<typeof signal>;
    projectName: ReturnType<typeof signal>;
    shortId: ReturnType<typeof signal>;
    currentUserRole: ReturnType<typeof signal>;
    reloadBoards: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    boardCreated$ = new Subject<void>();
    mockDrawerService = {
      boardCreated$,
      openCreateBoardDrawer: vi.fn(),
    };
    mockTitle = { setTitle: vi.fn() };
    mockProjectContext = {
      project: signal(mockProject),
      boards: signal(mockBoards),
      boardsLoaded: signal(true),
      projectId: signal('p-1'),
      projectName: signal('Test Project'),
      shortId: signal('p-short-1'),
      currentUserRole: signal(undefined),
      canEdit: () => true,
      isOwner: () => true,
      reloadBoards: vi.fn(),
    };

    TestBed.configureTestingModule({
      imports: [ProjectComponent],
      providers: [
        { provide: ProjectContext, useValue: mockProjectContext },
        { provide: DrawerService, useValue: mockDrawerService },
        { provide: Title, useValue: mockTitle },
      ],
    });
    TestBed.overrideTemplate(ProjectComponent, '');

    fixture = TestBed.createComponent(ProjectComponent);
    component = fixture.componentInstance;
  });

  it('reads project from ProjectContext and sets browser title', () => {
    TestBed.flushEffects();

    expect(component.project()).toEqual(mockProject);
    expect(mockTitle.setTitle).toHaveBeenCalledWith('Test Project | NeonBoard');
  });

  it('reads boards from ProjectContext', () => {
    expect(component.boards()).toEqual(mockBoards);
  });

  it('reloads boards when drawerService.boardCreated$ emits', () => {
    boardCreated$.next();

    expect(mockProjectContext.reloadBoards).toHaveBeenCalled();
  });

  it('isLoading is false when boardsLoaded is true', () => {
    expect(component.isLoading()).toBe(false);
  });

  it('isLoading is true when boardsLoaded is false', () => {
    mockProjectContext.boardsLoaded.set(false);
    expect(component.isLoading()).toBe(true);
  });
});
