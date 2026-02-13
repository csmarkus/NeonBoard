import { Label } from './label.model';

export interface Card {
  id: string;
  title: string;
  description: string;
  columnId: string;
  position: number;
  labels: Label[];
  createdAt: string;
  updatedAt: string;
}

export interface AddCardRequest {
  columnId: string;
  title: string;
  description?: string;
}

export interface UpdateCardRequest {
  title: string;
  description?: string;
}

export interface MoveCardRequest {
  targetColumnId: string;
  targetPosition: number;
}
