import { Column } from './column.model';
import { Card } from './card.model';

export interface Board {
  id: string;
  name: string;
  projectId: string;
  createdAt: string;
  updatedAt: string;
  columnCount: number;
}

export interface BoardDetails {
  id: string;
  name: string;
  projectId: string;
  createdAt: string;
  updatedAt: string;
  columns: Column[];
  cards: Card[];
}

export interface CreateBoardRequest {
  name: string;
}
