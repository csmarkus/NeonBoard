import { Column } from './column.model';
import { Card } from './card.model';
import { Label } from './label.model';

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
  labels: Label[];
}

export interface CreateBoardRequest {
  name: string;
}

export interface UpdateBoardSettingsRequest {
  name: string;
}
