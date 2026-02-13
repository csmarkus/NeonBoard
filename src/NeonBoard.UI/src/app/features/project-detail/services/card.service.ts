import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Card, AddCardRequest, UpdateCardRequest, MoveCardRequest } from '../models/card.model';

@Injectable({
  providedIn: 'root'
})
export class CardService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  addCard(projectId: string, boardId: string, request: AddCardRequest): Observable<Card> {
    return this.http.post<Card>(`${this.apiUrl}/projects/${projectId}/boards/${boardId}/cards`, request);
  }

  updateCard(projectId: string, boardId: string, cardId: string, request: UpdateCardRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/projects/${projectId}/boards/${boardId}/cards/${cardId}`, request);
  }

  moveCard(projectId: string, boardId: string, cardId: string, request: MoveCardRequest): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/projects/${projectId}/boards/${boardId}/cards/${cardId}/move`, request);
  }

  deleteCard(projectId: string, boardId: string, cardId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/projects/${projectId}/boards/${boardId}/cards/${cardId}`);
  }

  addCardLabel(projectId: string, boardId: string, cardId: string, labelId: string): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/projects/${projectId}/boards/${boardId}/cards/${cardId}/labels/${labelId}`,
      {}
    );
  }

  removeCardLabel(projectId: string, boardId: string, cardId: string, labelId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/projects/${projectId}/boards/${boardId}/cards/${cardId}/labels/${labelId}`
    );
  }
}
