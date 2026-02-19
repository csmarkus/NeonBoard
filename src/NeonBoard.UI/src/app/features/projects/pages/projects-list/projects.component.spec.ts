import { initTestEnvironment } from '../../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ProjectsComponent } from './projects.component';
import { ProjectService } from '../../services/project.service';
import { LoadingService } from '../../../../core/services/loading.service';
import { Project } from '../../models/project.model';

initTestEnvironment();

function makeProject(id: string, name = `Project ${id}`): Project {
  return { id, name, ownerId: 'user-1', createdAt: '', updatedAt: '' } as Project;
}

describe('ProjectsComponent', () => {
  let fixture: ComponentFixture<ProjectsComponent>;
  let component: ProjectsComponent;
  let mockProjectService: {
    getProjects: ReturnType<typeof vi.fn>;
    deleteProject: ReturnType<typeof vi.fn>;
  };
  let mockLoadingService: {
    show: ReturnType<typeof vi.fn>;
    hide: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    mockProjectService = {
      getProjects: vi.fn().mockReturnValue(of([])),
      deleteProject: vi.fn().mockReturnValue(of(undefined)),
    };
    mockLoadingService = {
      show: vi.fn(),
      hide: vi.fn(),
    };

    TestBed.configureTestingModule({
      imports: [ProjectsComponent],
      providers: [
        { provide: ProjectService, useValue: mockProjectService },
        { provide: LoadingService, useValue: mockLoadingService },
      ],
    });
    TestBed.overrideTemplate(ProjectsComponent, '');

    fixture = TestBed.createComponent(ProjectsComponent);
    component = fixture.componentInstance;
  });

  it('loads projects and calls loadingService.show then hide on ngOnInit', () => {
    const projects = [makeProject('p-1'), makeProject('p-2')];
    mockProjectService.getProjects.mockReturnValue(of(projects));

    component.ngOnInit();

    expect(mockLoadingService.show).toHaveBeenCalled();
    expect(mockLoadingService.hide).toHaveBeenCalled();
    expect(component.projects).toEqual(projects);
  });

  it('openCreateDrawer sets showCreateDrawer to true', () => {
    component.openCreateDrawer();
    expect(component.showCreateDrawer).toBe(true);
  });

  it('closeCreateDrawer sets showCreateDrawer to false', () => {
    component.showCreateDrawer = true;
    component.closeCreateDrawer();
    expect(component.showCreateDrawer).toBe(false);
  });

  it('onProjectCreated prepends the new project to the list', () => {
    const existing = makeProject('p-1');
    const newProject = makeProject('p-2', 'New Project');
    component.projects = [existing];

    component.onProjectCreated(newProject);

    expect(component.projects[0]).toEqual(newProject);
    expect(component.projects[1]).toEqual(existing);
  });

  it('onProjectDelete removes the project from the list by id', () => {
    const p1 = makeProject('p-1');
    const p2 = makeProject('p-2');
    component.projects = [p1, p2];
    mockProjectService.deleteProject.mockReturnValue(of(undefined));

    component.onProjectDelete(p1);

    expect(component.projects).toEqual([p2]);
  });

  it('sets error when ProjectService.getProjects fails', () => {
    mockProjectService.getProjects.mockReturnValue(throwError(() => new Error('fail')));

    component.ngOnInit();

    expect(component.error).toBeTruthy();
    expect(mockLoadingService.hide).toHaveBeenCalled();
  });
});
