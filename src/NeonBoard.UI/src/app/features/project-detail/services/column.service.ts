import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Column, AddColumnRequest, RenameColumnRequest, ReorderColumnsRequest } from '../models/column.model';

@Injectable({
  providedIn: 'root'
})
export class ColumnService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  addColumn(projectId: string, boardId: string, request: AddColumnRequest): Observable<Column> {
    return this.http.post<Column>(`${this.apiUrl}/projects/${projectId}/boards/${boardId}/columns`, request);
  }

  renameColumn(projectId: string, boardId: string, columnId: string, request: RenameColumnRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/projects/${projectId}/boards/${boardId}/columns/${columnId}`, request);
  }

  deleteColumn(projectId: string, boardId: string, columnId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/projects/${projectId}/boards/${boardId}/columns/${columnId}`);
  }

  reorderColumns(projectId: string, boardId: string, request: ReorderColumnsRequest): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/projects/${projectId}/boards/${boardId}/columns/reorder`, request);
  }
}
