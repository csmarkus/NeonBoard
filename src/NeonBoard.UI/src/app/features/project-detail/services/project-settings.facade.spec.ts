import { initTestEnvironment } from '../../../../test-setup';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ProjectSettingsFacade } from './project-settings.facade';
import { ProjectService } from '../../projects/services/project.service';
import { ToastService } from '../../../core/services/toast.service';

initTestEnvironment();

function createMockProject(overrides: Record<string, unknown> = {}) {
  return {
    id: 'project-1',
    name: 'Test Project',
    description: 'A test project',
    ...overrides,
  };
}

describe('ProjectSettingsFacade', () => {
  let facade: ProjectSettingsFacade;
  let projectService: {
    getProject: ReturnType<typeof vi.fn>;
    updateProject: ReturnType<typeof vi.fn>;
    deleteProject: ReturnType<typeof vi.fn>;
  };
  let toastService: {
    success: ReturnType<typeof vi.fn>;
    error: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    projectService = {
      getProject: vi.fn(),
      updateProject: vi.fn(),
      deleteProject: vi.fn(),
    };
    toastService = {
      success: vi.fn(),
      error: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        ProjectSettingsFacade,
        { provide: ProjectService, useValue: projectService },
        { provide: ToastService, useValue: toastService },
      ],
    });

    facade = TestBed.inject(ProjectSettingsFacade);
  });

  describe('saveProjectSettings', () => {
    it('should show success toast on save', () => {
      const mockProject = createMockProject();
      projectService.getProject.mockReturnValue(of(mockProject));
      facade.loadProjectSettings('project-1');

      facade.updateProjectName('Updated Name');
      projectService.updateProject.mockReturnValue(of({ ...mockProject, name: 'Updated Name' }));

      facade.saveProjectSettings('project-1').subscribe();

      expect(toastService.success).toHaveBeenCalledWith('Project settings saved');
    });

    it('should show error toast on save failure', () => {
      const mockProject = createMockProject();
      projectService.getProject.mockReturnValue(of(mockProject));
      facade.loadProjectSettings('project-1');

      facade.updateProjectName('Updated Name');
      projectService.updateProject.mockReturnValue(throwError(() => new Error('fail')));

      facade.saveProjectSettings('project-1').subscribe();

      expect(toastService.error).toHaveBeenCalledWith('Failed to save project settings');
    });

    it('should skip when no changes', () => {
      const mockProject = createMockProject();
      projectService.getProject.mockReturnValue(of(mockProject));
      facade.loadProjectSettings('project-1');

      facade.saveProjectSettings('project-1').subscribe();

      expect(projectService.updateProject).not.toHaveBeenCalled();
      expect(toastService.success).not.toHaveBeenCalled();
    });
  });
});
