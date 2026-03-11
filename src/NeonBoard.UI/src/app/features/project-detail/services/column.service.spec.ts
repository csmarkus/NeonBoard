import { initTestEnvironment } from '../../../../test-setup';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ColumnService } from './column.service';

initTestEnvironment();

const API_URL = 'http://localhost:5000/api';

describe('ColumnService', () => {
  let service: ColumnService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ColumnService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ColumnService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('addColumn → POST /projects/p-1/boards/b-1/columns', () => {
    const mockColumn = { id: 'col-1', name: 'To Do', position: 'a0', boardId: 'b-1' };

    service.addColumn('p-1', 'b-1', { name: 'To Do' }).subscribe(col => {
      expect(col).toEqual(mockColumn);
    });

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards/b-1/columns`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ name: 'To Do' });
    req.flush(mockColumn);
  });

  it('renameColumn → PUT /projects/p-1/boards/b-1/columns/col-1', () => {
    service.renameColumn('p-1', 'b-1', 'col-1', { newName: 'In Progress' }).subscribe();

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards/b-1/columns/col-1`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ newName: 'In Progress' });
    req.flush(null);
  });

  it('deleteColumn → DELETE /projects/p-1/boards/b-1/columns/col-1', () => {
    service.deleteColumn('p-1', 'b-1', 'col-1').subscribe();

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards/b-1/columns/col-1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('reorderColumns → PATCH /projects/p-1/boards/b-1/columns/reorder', () => {
    service.reorderColumns('p-1', 'b-1', { columnIds: ['col-2', 'col-1'] }).subscribe();

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards/b-1/columns/reorder`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ columnIds: ['col-2', 'col-1'] });
    req.flush(null);
  });
});
