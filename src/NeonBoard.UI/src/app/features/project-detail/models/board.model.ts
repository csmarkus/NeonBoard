export interface Board {
  id: string;
  name: string;
  projectId: string;
  createdAt: string;
  updatedAt: string;
  columnCount: number;
}

export interface CreateBoardRequest {
  name: string;
}
