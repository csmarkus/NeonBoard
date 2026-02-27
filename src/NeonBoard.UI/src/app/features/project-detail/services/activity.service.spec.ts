import { initTestEnvironment } from '../../../../test-setup';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ActivityService } from './activity.service';

initTestEnvironment();

const API_URL = 'http://localhost:5000/api';

describe('ActivityService', () => {
  let service: ActivityService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ActivityService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ActivityService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('getBoardActivity → GET /projects/p-1/boards/b-1/activity with pageSize param', () => {
    const mockFeed = { entries: [], nextCursor: null };

    service.getBoardActivity('p-1', 'b-1').subscribe(feed => {
      expect(feed).toEqual(mockFeed);
    });

    const req = httpMock.expectOne(r => r.url === `${API_URL}/projects/p-1/boards/b-1/activity`);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('pageSize')).toBe('20');
    expect(req.request.params.has('cursor')).toBe(false);
    req.flush(mockFeed);
  });

  it('getBoardActivity → includes cursor param when provided', () => {
    const mockFeed = {
      entries: [{ id: 'a-1', boardId: 'b-1', userId: 'u-1', userName: 'Alice', entityType: 'Card', entityId: 'c-1', actionType: 'Created', data: {}, occurredAt: '2026-01-01T00:00:00Z' }],
      nextCursor: 'next-abc',
    };

    service.getBoardActivity('p-1', 'b-1', 10, 'cursor-123').subscribe(feed => {
      expect(feed).toEqual(mockFeed);
    });

    const req = httpMock.expectOne(r => r.url === `${API_URL}/projects/p-1/boards/b-1/activity`);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('pageSize')).toBe('10');
    expect(req.request.params.get('cursor')).toBe('cursor-123');
    req.flush(mockFeed);
  });

  it('getCardActivity → GET /projects/p-1/boards/b-1/cards/c-1/activity with pageSize param', () => {
    const mockFeed = { entries: [], nextCursor: null };

    service.getCardActivity('p-1', 'b-1', 'c-1').subscribe(feed => {
      expect(feed).toEqual(mockFeed);
    });

    const req = httpMock.expectOne(r => r.url === `${API_URL}/projects/p-1/boards/b-1/cards/c-1/activity`);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('pageSize')).toBe('20');
    expect(req.request.params.has('cursor')).toBe(false);
    req.flush(mockFeed);
  });

  it('getCardActivity → includes cursor param when provided', () => {
    const mockFeed = { entries: [], nextCursor: 'next-xyz' };

    service.getCardActivity('p-1', 'b-1', 'c-1', 15, 'cursor-456').subscribe(feed => {
      expect(feed).toEqual(mockFeed);
    });

    const req = httpMock.expectOne(r => r.url === `${API_URL}/projects/p-1/boards/b-1/cards/c-1/activity`);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('pageSize')).toBe('15');
    expect(req.request.params.get('cursor')).toBe('cursor-456');
    req.flush(mockFeed);
  });
});
