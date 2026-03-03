import { initTestEnvironment } from '../../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { ProjectLayoutComponent } from './project-layout.component';
import { ActivatedRoute } from '@angular/router';
import { ProjectService } from '../../../projects/services/project.service';

initTestEnvironment();

describe('ProjectLayoutComponent', () => {
  let fixture: ComponentFixture<ProjectLayoutComponent>;
  let component: ProjectLayoutComponent;

  beforeEach(() => {
    const mockRoute = {
      snapshot: { paramMap: convertToParamMap({ shortId: 'p-short-1' }) },
    };

    TestBed.configureTestingModule({
      imports: [ProjectLayoutComponent],
      providers: [
        { provide: ActivatedRoute, useValue: mockRoute },
        { provide: ProjectService, useValue: { getProjectByShortId: vi.fn().mockReturnValue(of({ id: 'p-1', shortId: 'p-short-1', name: 'Test Project', ownerId: 'user-1', createdAt: '', updatedAt: '' })) } },
      ],
    });
    TestBed.overrideTemplate(ProjectLayoutComponent, '');

    fixture = TestBed.createComponent(ProjectLayoutComponent);
    component = fixture.componentInstance;
  });

  it('sets shortId and projectId on init', () => {
    component.ngOnInit();

    expect(component.shortId()).toBe('p-short-1');
    expect(component.projectId()).toBe('p-1');
  });
});
