import { initTestEnvironment } from '../../../../test-setup';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ProjectService } from './project.service';

initTestEnvironment();

const API_URL = 'http://localhost:5000/api';

describe('ProjectService', () => {
  let service: ProjectService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ProjectService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ProjectService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('getProjects → GET /projects', () => {
    const mockProjects = [{ id: 'p-1', name: 'Project 1', description: '', ownerId: 'u-1', createdAt: '', updatedAt: '' }];

    service.getProjects().subscribe(projects => {
      expect(projects).toEqual(mockProjects);
    });

    const req = httpMock.expectOne(`${API_URL}/projects`);
    expect(req.request.method).toBe('GET');
    req.flush(mockProjects);
  });

  it('getProject → GET /projects/p-1', () => {
    const mockProject = { id: 'p-1', name: 'Project 1', description: '', ownerId: 'u-1', createdAt: '', updatedAt: '' };

    service.getProject('p-1').subscribe(project => {
      expect(project).toEqual(mockProject);
    });

    const req = httpMock.expectOne(`${API_URL}/projects/p-1`);
    expect(req.request.method).toBe('GET');
    req.flush(mockProject);
  });

  it('createProject → POST /projects', () => {
    const mockProject = { id: 'p-1', name: 'New Project', description: 'Desc', ownerId: 'u-1', createdAt: '', updatedAt: '' };

    service.createProject({ name: 'New Project', description: 'Desc' }).subscribe(project => {
      expect(project).toEqual(mockProject);
    });

    const req = httpMock.expectOne(`${API_URL}/projects`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ name: 'New Project', description: 'Desc' });
    req.flush(mockProject);
  });

  it('updateProject → PUT /projects/p-1', () => {
    const mockProject = { id: 'p-1', name: 'Updated', description: 'New Desc', ownerId: 'u-1', createdAt: '', updatedAt: '' };

    service.updateProject('p-1', { name: 'Updated', description: 'New Desc' }).subscribe(project => {
      expect(project).toEqual(mockProject);
    });

    const req = httpMock.expectOne(`${API_URL}/projects/p-1`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ name: 'Updated', description: 'New Desc' });
    req.flush(mockProject);
  });

  it('deleteProject → DELETE /projects/p-1', () => {
    service.deleteProject('p-1').subscribe();

    const req = httpMock.expectOne(`${API_URL}/projects/p-1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
