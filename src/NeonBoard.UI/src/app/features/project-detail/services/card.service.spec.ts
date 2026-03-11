import { initTestEnvironment } from '../../../../test-setup';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { CardService } from './card.service';

initTestEnvironment();

const API_URL = 'http://localhost:5000/api';

describe('CardService', () => {
  let service: CardService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CardService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(CardService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('addCard → POST /projects/p-1/boards/b-1/cards', () => {
    const mockCard = { id: 'card-1', cardNumber: 1, displayId: 'TST-1', title: 'New Card', description: '', columnId: 'col-1', position: 'a0', labels: [], createdAt: '', updatedAt: '' };

    service.addCard('p-1', 'b-1', { columnId: 'col-1', title: 'New Card', description: '' }).subscribe(card => {
      expect(card).toEqual(mockCard);
    });

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards/b-1/cards`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ columnId: 'col-1', title: 'New Card', description: '' });
    req.flush(mockCard);
  });

  it('updateCard → PUT /projects/p-1/boards/b-1/cards/card-1', () => {
    service.updateCard('p-1', 'b-1', 'card-1', { title: 'Updated', description: 'Desc' }).subscribe();

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards/b-1/cards/card-1`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ title: 'Updated', description: 'Desc' });
    req.flush(null);
  });

  it('moveCard → PATCH /projects/p-1/boards/b-1/cards/card-1/move', () => {
    service.moveCard('p-1', 'b-1', 'card-1', { targetColumnId: 'col-2', newPosition: 'a0' }).subscribe();

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards/b-1/cards/card-1/move`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ targetColumnId: 'col-2', newPosition: 'a0' });
    req.flush(null);
  });

  it('deleteCard → DELETE /projects/p-1/boards/b-1/cards/card-1', () => {
    service.deleteCard('p-1', 'b-1', 'card-1').subscribe();

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards/b-1/cards/card-1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('addCardLabel → PUT /projects/p-1/boards/b-1/cards/card-1/labels/label-1', () => {
    service.addCardLabel('p-1', 'b-1', 'card-1', 'label-1').subscribe();

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards/b-1/cards/card-1/labels/label-1`);
    expect(req.request.method).toBe('PUT');
    req.flush(null);
  });

  it('removeCardLabel → DELETE /projects/p-1/boards/b-1/cards/card-1/labels/label-1', () => {
    service.removeCardLabel('p-1', 'b-1', 'card-1', 'label-1').subscribe();

    const req = httpMock.expectOne(`${API_URL}/projects/p-1/boards/b-1/cards/card-1/labels/label-1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
