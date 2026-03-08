export type ProjectRole = 'Viewer' | 'Editor' | 'Owner';

export interface ProjectMember {
  userId: string;
  displayName: string;
  email: string;
  role: ProjectRole;
  joinedAt: string;
}
