import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { Label, AddLabelRequest, UpdateLabelRequest } from '../models/label.model';

@Injectable({
  providedIn: 'root'
})
export class LabelService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;
  private labelsUpdatedSubject = new Subject<void>();

  labelsUpdated$ = this.labelsUpdatedSubject.asObservable();

  addLabel(projectId: string, boardId: string, request: AddLabelRequest): Observable<Label> {
    return this.http.post<Label>(
      `${this.apiUrl}/projects/${projectId}/boards/${boardId}/labels`,
      request
    ).pipe(tap(() => this.labelsUpdatedSubject.next()));
  }

  updateLabel(projectId: string, boardId: string, labelId: string, request: UpdateLabelRequest): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/projects/${projectId}/boards/${boardId}/labels/${labelId}`,
      request
    ).pipe(tap(() => this.labelsUpdatedSubject.next()));
  }

  removeLabel(projectId: string, boardId: string, labelId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/projects/${projectId}/boards/${boardId}/labels/${labelId}`
    ).pipe(tap(() => this.labelsUpdatedSubject.next()));
  }
}
