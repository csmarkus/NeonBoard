import { initTestEnvironment } from '../../../../test-setup';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { BoardService } from './board.service';

initTestEnvironment();

const API_URL = 'http://localhost:5000/api';

describe('BoardService', () => {
  let service: BoardService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [BoardService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(BoardService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('getBoardsByProject → GET /projects/p-1/boards', () => {
    const mockBoards = [{ id: 'b-1', name: 'Board 1', prefix: 'BRD', projectId: 'p-1', createdAt: '', updatedAt: '', columnCount: 0 }];

    service.getBoardsByProject('p-1').subscribe(boards => {
      expect(boards).toEqual(mockBoards);
    });

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards`);
    expect(req.request.method).toBe('GET');
    req.flush(mockBoards);
  });

  it('getBoardDetails → GET /projects/p-1/boards/b-1', () => {
    const mockDetails = { id: 'b-1', name: 'Board 1', prefix: 'BRD', projectId: 'p-1', createdAt: '', updatedAt: '', columns: [], cards: [], labels: [] };

    service.getBoardDetails('p-1', 'b-1').subscribe(details => {
      expect(details).toEqual(mockDetails);
    });

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards/b-1`);
    expect(req.request.method).toBe('GET');
    req.flush(mockDetails);
  });

  it('createBoard → POST /projects/p-1/boards and emits boardsUpdated$', () => {
    const mockBoard = { id: 'b-1', name: 'New Board', prefix: 'NEW', projectId: 'p-1', createdAt: '', updatedAt: '', columnCount: 0 };
    let emitted = false;
    service.boardsUpdated$.subscribe(() => (emitted = true));

    service.createBoard('p-1', { name: 'New Board' }).subscribe();

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ name: 'New Board' });
    req.flush(mockBoard);

    expect(emitted).toBe(true);
  });

  it('updateBoardSettings → PUT /projects/p-1/boards/b-1 and emits boardsUpdated$', () => {
    const mockBoard = { id: 'b-1', name: 'Updated', prefix: 'UPD', projectId: 'p-1', createdAt: '', updatedAt: '', columnCount: 0 };
    let emitted = false;
    service.boardsUpdated$.subscribe(() => (emitted = true));

    service.updateBoardSettings('p-1', 'b-1', { name: 'Updated' }).subscribe();

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards/b-1`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ name: 'Updated' });
    req.flush(mockBoard);

    expect(emitted).toBe(true);
  });

  it('deleteBoard → DELETE /projects/p-1/boards/b-1 and emits boardsUpdated$', () => {
    let emitted = false;
    service.boardsUpdated$.subscribe(() => (emitted = true));

    service.deleteBoard('p-1', 'b-1').subscribe();

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards/b-1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);

    expect(emitted).toBe(true);
  });
});
