import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ProjectMember, ProjectRole } from '../../features/project-detail/models/member.model';

@Injectable({ providedIn: 'root' })
export class MemberService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  getMembers(projectId: string): Observable<ProjectMember[]> {
    return this.http.get<ProjectMember[]>(`${this.apiUrl}/projects/${projectId}/members`);
  }

  inviteMember(projectId: string, email: string, role: ProjectRole): Observable<unknown> {
    return this.http.post(`${this.apiUrl}/projects/${projectId}/members/invite`, { email, role });
  }

  removeMember(projectId: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/projects/${projectId}/members/${userId}`);
  }

  updateMemberRole(projectId: string, userId: string, role: ProjectRole): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/projects/${projectId}/members/${userId}/role`, { role });
  }

  leaveProject(projectId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/projects/${projectId}/members/leave`, {});
  }
}
