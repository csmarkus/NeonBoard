import { ProjectRole } from '../../project-detail/models/member.model';

export interface Project {
  id: string;
  shortId: string;
  name: string;
  description: string;
  ownerId: string;
  currentUserRole?: ProjectRole;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProjectRequest {
  name: string;
  description: string;
}

export interface UpdateProjectRequest {
  name: string;
  description: string;
}
