export interface Project {
  id: string;
  name: string;
  description: string;
  ownerId: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProjectRequest {
  name: string;
  description: string;
  ownerId: string;
}

export interface UpdateProjectRequest {
  name: string;
  description: string;
}
