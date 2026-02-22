import { initTestEnvironment } from '../../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { ProjectLayoutComponent } from './project-layout.component';
import { ActivatedRoute } from '@angular/router';
import { DrawerService } from '../../services/drawer.service';
import { ProjectService } from '../../../projects/services/project.service';

initTestEnvironment();

describe('ProjectLayoutComponent', () => {
  let fixture: ComponentFixture<ProjectLayoutComponent>;
  let component: ProjectLayoutComponent;
  let mockDrawerService: {
    closeCreateBoardDrawer: ReturnType<typeof vi.fn>;
    notifyBoardCreated: ReturnType<typeof vi.fn>;
    notifyCardUpdated: ReturnType<typeof vi.fn>;
    closeCardDrawer: ReturnType<typeof vi.fn>;
    notifyCardDeleted: ReturnType<typeof vi.fn>;
    showCreateBoardDrawer: ReturnType<typeof vi.fn>;
    createBoardProjectId: ReturnType<typeof vi.fn>;
    selectedCard: ReturnType<typeof vi.fn>;
    cardDrawerProjectId: ReturnType<typeof vi.fn>;
    cardDrawerBoardId: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    mockDrawerService = {
      closeCreateBoardDrawer: vi.fn(),
      notifyBoardCreated: vi.fn(),
      notifyCardUpdated: vi.fn(),
      closeCardDrawer: vi.fn(),
      notifyCardDeleted: vi.fn(),
      showCreateBoardDrawer: vi.fn().mockReturnValue(false),
      createBoardProjectId: vi.fn().mockReturnValue(null),
      selectedCard: vi.fn().mockReturnValue(null),
      cardDrawerProjectId: vi.fn().mockReturnValue(''),
      cardDrawerBoardId: vi.fn().mockReturnValue(''),
    };

    const mockRoute = {
      snapshot: { paramMap: convertToParamMap({ shortId: 'p-short-1' }) },
    };

    TestBed.configureTestingModule({
      imports: [ProjectLayoutComponent],
      providers: [
        { provide: ActivatedRoute, useValue: mockRoute },
        { provide: DrawerService, useValue: mockDrawerService },
        { provide: ProjectService, useValue: { getProjectByShortId: vi.fn().mockReturnValue(of({ id: 'p-1', shortId: 'p-short-1', name: 'Test Project', ownerId: 'user-1', createdAt: '', updatedAt: '' })) } },
      ],
    });
    TestBed.overrideTemplate(ProjectLayoutComponent, '');

    fixture = TestBed.createComponent(ProjectLayoutComponent);
    component = fixture.componentInstance;
  });

  it('onBoardCreated closes the create board drawer and notifies drawerService', () => {
    component.onBoardCreated();

    expect(mockDrawerService.closeCreateBoardDrawer).toHaveBeenCalled();
    expect(mockDrawerService.notifyBoardCreated).toHaveBeenCalled();
  });

  it('onCardUpdated calls drawerService.notifyCardUpdated', () => {
    component.onCardUpdated();

    expect(mockDrawerService.notifyCardUpdated).toHaveBeenCalled();
  });

  it('onCardDeleted closes the card drawer and calls drawerService.notifyCardDeleted', () => {
    component.onCardDeleted();

    expect(mockDrawerService.closeCardDrawer).toHaveBeenCalled();
    expect(mockDrawerService.notifyCardDeleted).toHaveBeenCalled();
  });
});
