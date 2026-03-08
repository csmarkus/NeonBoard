import { ProjectRole } from './member.model';

export type InvitationStatus = 'Pending' | 'Accepted' | 'Expired' | 'Revoked';

export interface ProjectInvitation {
  id: string;
  email: string;
  role: ProjectRole;
  status: InvitationStatus;
  expiresAt: string;
  invitedByName: string;
  createdAt: string;
}

export interface InvitationDetails {
  id: string;
  projectName: string;
  inviterName: string;
  role: ProjectRole;
  status: InvitationStatus;
  isExpired: boolean;
  expiresAt: string;
}
