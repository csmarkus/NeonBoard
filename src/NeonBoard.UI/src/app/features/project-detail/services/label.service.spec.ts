import { initTestEnvironment } from '../../../../test-setup';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { LabelService } from './label.service';

initTestEnvironment();

const API_URL = 'http://localhost:5000/api';

describe('LabelService', () => {
  let service: LabelService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [LabelService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(LabelService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('addLabel → POST /projects/p-1/boards/b-1/labels and emits labelsUpdated$', () => {
    const mockLabel = { id: 'l-1', name: 'Bug', color: 'red' };
    let emitted = false;
    service.labelsUpdated$.subscribe(() => (emitted = true));

    service.addLabel('p-1', 'b-1', { name: 'Bug', color: 'red' }).subscribe();

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards/b-1/labels`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ name: 'Bug', color: 'red' });
    req.flush(mockLabel);

    expect(emitted).toBe(true);
  });

  it('updateLabel → PUT /projects/p-1/boards/b-1/labels/l-1 and emits labelsUpdated$', () => {
    let emitted = false;
    service.labelsUpdated$.subscribe(() => (emitted = true));

    service.updateLabel('p-1', 'b-1', 'l-1', { name: 'Feature', color: 'blue' }).subscribe();

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards/b-1/labels/l-1`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ name: 'Feature', color: 'blue' });
    req.flush(null);

    expect(emitted).toBe(true);
  });

  it('removeLabel → DELETE /projects/p-1/boards/b-1/labels/l-1 and emits labelsUpdated$', () => {
    let emitted = false;
    service.labelsUpdated$.subscribe(() => (emitted = true));

    service.removeLabel('p-1', 'b-1', 'l-1').subscribe();

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards/b-1/labels/l-1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);

    expect(emitted).toBe(true);
  });
});
