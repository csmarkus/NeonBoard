import { initTestEnvironment } from '../../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { signal } from '@angular/core';
import { convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { Title } from '@angular/platform-browser';
import { BoardSettingsComponent } from './board-settings.component';
import { ActivatedRoute, Router } from '@angular/router';
import { ProjectService } from '../../../projects/services/project.service';
import { BoardSettingsFacade } from '../../services/board-settings.facade';
import { Project } from '../../../projects/models/project.model';

initTestEnvironment();

const mockProject: Project = {
  id: 'p-1',
  name: 'Test Project',
  ownerId: 'user-1',
  createdAt: '',
  updatedAt: '',
} as Project;

describe('BoardSettingsComponent', () => {
  let fixture: ComponentFixture<BoardSettingsComponent>;
  let component: BoardSettingsComponent;
  let mockFacade: {
    loadBoardSettings: ReturnType<typeof vi.fn>;
    saveBoardSettings: ReturnType<typeof vi.fn>;
    deleteBoard: ReturnType<typeof vi.fn>;
    hasChanges: ReturnType<typeof signal<boolean>>;
    originalBoardName: ReturnType<typeof signal<string>>;
    boardName: ReturnType<typeof signal<string>>;
    boardLabels: ReturnType<typeof signal<never[]>>;
    isLoading: ReturnType<typeof signal<boolean>>;
    isSaving: ReturnType<typeof signal<boolean>>;
    error: ReturnType<typeof signal<null>>;
    sortedLabels: ReturnType<typeof signal<never[]>>;
  };
  let mockRouter: { navigate: ReturnType<typeof vi.fn> };
  let mockTitle: { setTitle: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    mockFacade = {
      loadBoardSettings: vi.fn(),
      saveBoardSettings: vi.fn(),
      deleteBoard: vi.fn().mockReturnValue(of(undefined)),
      hasChanges: signal(false),
      originalBoardName: signal(''),
      boardName: signal(''),
      boardLabels: signal([]),
      isLoading: signal(false),
      isSaving: signal(false),
      error: signal(null),
      sortedLabels: signal([]),
    };
    mockRouter = { navigate: vi.fn() };
    mockTitle = { setTitle: vi.fn() };

    const mockRoute = {
      parent: { snapshot: { paramMap: convertToParamMap({ projectId: 'p-1' }) } },
      snapshot: { paramMap: convertToParamMap({ boardId: 'b-1' }) },
    };

    TestBed.configureTestingModule({
      imports: [BoardSettingsComponent],
      providers: [
        { provide: ActivatedRoute, useValue: mockRoute },
        { provide: Router, useValue: mockRouter },
        { provide: ProjectService, useValue: { getProject: vi.fn().mockReturnValue(of(mockProject)) } },
        { provide: BoardSettingsFacade, useValue: mockFacade },
        { provide: Title, useValue: mockTitle },
      ],
    });
    TestBed.overrideTemplate(BoardSettingsComponent, '');

    fixture = TestBed.createComponent(BoardSettingsComponent);
    component = fixture.componentInstance;
  });

  it('calls facade.loadBoardSettings with projectId and boardId on init', () => {
    component.ngOnInit();

    expect(mockFacade.loadBoardSettings).toHaveBeenCalledWith('p-1', 'b-1');
  });

  it('saveChanges delegates to facade.saveBoardSettings', () => {
    component.projectId.set('p-1');
    component.boardId.set('b-1');

    component.saveChanges();

    expect(mockFacade.saveBoardSettings).toHaveBeenCalledWith('p-1', 'b-1');
  });

  it('onDeleteBoard calls facade.deleteBoard and navigates to /project/:id on success', () => {
    component.projectId.set('p-1');
    component.boardId.set('b-1');
    mockFacade.deleteBoard.mockReturnValue(of(undefined));

    component.onDeleteBoard();

    expect(mockFacade.deleteBoard).toHaveBeenCalledWith('p-1', 'b-1');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/project', 'p-1']);
  });

  it('hasUnsavedChanges returns facade.hasChanges() value', () => {
    mockFacade.hasChanges.set(true);
    expect(component.hasUnsavedChanges()).toBe(true);

    mockFacade.hasChanges.set(false);
    expect(component.hasUnsavedChanges()).toBe(false);
  });
});
