import { initTestEnvironment } from '../../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { convertToParamMap } from '@angular/router';
import { of, throwError, Subject } from 'rxjs';
import { Title } from '@angular/platform-browser';
import { ProjectComponent } from './project.component';
import { ActivatedRoute } from '@angular/router';
import { ProjectService } from '../../../projects/services/project.service';
import { BoardService } from '../../services/board.service';
import { DrawerService } from '../../services/drawer.service';
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
  let mockProjectService: { getProjectByShortId: ReturnType<typeof vi.fn> };
  let mockBoardService: { getBoardsByProject: ReturnType<typeof vi.fn> };
  let boardCreated$: Subject<void>;
  let mockDrawerService: { boardCreated$: Subject<void>; openCreateBoardDrawer: ReturnType<typeof vi.fn> };
  let mockTitle: { setTitle: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    boardCreated$ = new Subject<void>();
    mockProjectService = { getProjectByShortId: vi.fn().mockReturnValue(of(mockProject)) };
    mockBoardService = { getBoardsByProject: vi.fn().mockReturnValue(of(mockBoards)) };
    mockDrawerService = {
      boardCreated$,
      openCreateBoardDrawer: vi.fn(),
    };
    mockTitle = { setTitle: vi.fn() };

    const mockRoute = {
      snapshot: { paramMap: convertToParamMap({ shortId: 'p-short-1' }) },
    };

    TestBed.configureTestingModule({
      imports: [ProjectComponent],
      providers: [
        { provide: ActivatedRoute, useValue: mockRoute },
        { provide: ProjectService, useValue: mockProjectService },
        { provide: BoardService, useValue: mockBoardService },
        { provide: DrawerService, useValue: mockDrawerService },
        { provide: Title, useValue: mockTitle },
      ],
    });
    TestBed.overrideTemplate(ProjectComponent, '');

    fixture = TestBed.createComponent(ProjectComponent);
    component = fixture.componentInstance;
  });

  it('loads project and sets browser title via Title service on init', () => {
    component.ngOnInit();

    expect(component.project()).toEqual(mockProject);
    expect(mockTitle.setTitle).toHaveBeenCalledWith('Test Project | NeonBoard');
  });

  it('loads boards after project is loaded', () => {
    component.ngOnInit();

    expect(mockBoardService.getBoardsByProject).toHaveBeenCalledWith('p-1');
    expect(component.boards()).toEqual(mockBoards);
  });

  it('reloads boards when drawerService.boardCreated$ emits', () => {
    component.ngOnInit();
    mockBoardService.getBoardsByProject.mockClear();
    mockBoardService.getBoardsByProject.mockReturnValue(of(mockBoards));

    boardCreated$.next();

    expect(mockBoardService.getBoardsByProject).toHaveBeenCalledWith('p-1');
  });

  it('sets error signal when project load fails', () => {
    mockProjectService.getProjectByShortId.mockReturnValue(throwError(() => new Error('fail')));

    component.ngOnInit();

    expect(component.error()).toBe('Failed to load project');
  });

  it('sets isLoading to false after fetch completes on success', () => {
    component.ngOnInit();
    expect(component.isLoading()).toBe(false);
  });
});
