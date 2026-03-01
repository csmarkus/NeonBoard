import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { Board, BoardDetails, CreateBoardRequest, UpdateBoardSettingsRequest } from '../models/board.model';
import { ActivityFeed } from '../models/activity.model';

@Injectable({
  providedIn: 'root'
})
export class BoardService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;
  private boardsUpdated = new Subject<void>();

  boardsUpdated$ = this.boardsUpdated.asObservable();

  getBoardsByProject(projectId: string): Observable<Board[]> {
    return this.http.get<Board[]>(`${this.apiUrl}/projects/${projectId}/boards`);
  }

  getBoardDetails(projectId: string, boardId: string): Observable<BoardDetails> {
    return this.http.get<BoardDetails>(`${this.apiUrl}/projects/${projectId}/boards/${boardId}`);
  }

  createBoard(projectId: string, request: CreateBoardRequest): Observable<Board> {
    return this.http.post<Board>(`${this.apiUrl}/projects/${projectId}/boards`, request).pipe(
      tap(() => this.boardsUpdated.next())
    );
  }

  updateBoardSettings(projectId: string, boardId: string, request: UpdateBoardSettingsRequest): Observable<Board> {
    return this.http.put<Board>(`${this.apiUrl}/projects/${projectId}/boards/${boardId}`, request).pipe(
      tap(() => this.boardsUpdated.next())
    );
  }

  deleteBoard(projectId: string, boardId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/projects/${projectId}/boards/${boardId}`).pipe(
      tap(() => this.boardsUpdated.next())
    );
  }

  getBoardActivity(projectId: string, boardId: string, pageSize = 20, cursor?: string): Observable<ActivityFeed> {
    let params = new HttpParams().set('pageSize', pageSize);
    if (cursor) {
      params = params.set('cursor', cursor);
    }
    return this.http.get<ActivityFeed>(
      `${this.apiUrl}/projects/${projectId}/boards/${boardId}/activity`,
      { params }
    );
  }
}
