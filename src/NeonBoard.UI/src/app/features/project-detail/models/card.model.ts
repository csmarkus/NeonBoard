import { ActivityFeed } from './activity.model';
import { Label } from './label.model';

export interface Card {
  id: string;
  cardNumber: number;
  displayId: string;
  title: string;
  description: string;
  columnId: string;
  position: string;
  labels: Label[];
  createdAt: string;
  updatedAt: string;
  archivedAt: string | null;
  holdAt: string | null;
}

export interface CardDetail extends Card {
  activity: ActivityFeed;
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
  newPosition: string;
}
