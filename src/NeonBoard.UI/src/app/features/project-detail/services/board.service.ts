import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { Board, BoardDetails, CreateBoardRequest } from '../models/board.model';

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
}
