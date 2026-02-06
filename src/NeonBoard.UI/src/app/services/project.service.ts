import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Project, CreateProjectRequest, UpdateProjectRequest } from '../models/project.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/projects`;

  // Temporary user ID until we implement authentication
  private readonly TEMP_USER_ID = '00000000-0000-0000-0000-000000000001';

  getProjects(): Observable<Project[]> {
    return this.http.get<Project[]>(`${this.apiUrl}/user/${this.TEMP_USER_ID}`);
  }

  getProject(id: string): Observable<Project> {
    return this.http.get<Project>(`${this.apiUrl}/${id}`);
  }

  createProject(request: Omit<CreateProjectRequest, 'ownerId'>): Observable<Project> {
    const payload: CreateProjectRequest = {
      ...request,
      ownerId: this.TEMP_USER_ID
    };
    return this.http.post<Project>(this.apiUrl, payload);
  }

  updateProject(id: string, request: UpdateProjectRequest): Observable<Project> {
    return this.http.put<Project>(`${this.apiUrl}/${id}`, request);
  }

  deleteProject(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
