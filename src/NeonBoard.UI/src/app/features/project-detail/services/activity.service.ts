import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ActivityFeed } from '../models/activity.model';

@Injectable({ providedIn: 'root' })
export class ActivityService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

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

  getCardActivity(projectId: string, boardId: string, cardId: string, pageSize = 20, cursor?: string): Observable<ActivityFeed> {
    let params = new HttpParams().set('pageSize', pageSize);
    if (cursor) {
      params = params.set('cursor', cursor);
    }
    return this.http.get<ActivityFeed>(
      `${this.apiUrl}/projects/${projectId}/boards/${boardId}/cards/${cardId}/activity`,
      { params }
    );
  }
}
