import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ProjectInvitation, InvitationDetails } from '../../features/project-detail/models/invitation.model';

@Injectable({ providedIn: 'root' })
export class InvitationService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  getInvitations(projectId: string): Observable<ProjectInvitation[]> {
    return this.http.get<ProjectInvitation[]>(`${this.apiUrl}/projects/${projectId}/invitations`);
  }

  revokeInvitation(projectId: string, invitationId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/projects/${projectId}/invitations/${invitationId}`);
  }

  getInvitationByToken(token: string): Observable<InvitationDetails> {
    return this.http.get<InvitationDetails>(`${this.apiUrl}/invitations/${token}`);
  }

  acceptInvitation(token: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/invitations/${token}/accept`, {});
  }
}
