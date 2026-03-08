import { Card } from '../../features/project-detail/models/card.model';
import { ActivityFeed } from '../../features/project-detail/models/activity.model';
import { Label } from '../../features/project-detail/models/label.model';

export interface CardDrawerConfig {
  type: 'card-detail';
  card: Card;
  projectId: string;
  boardId: string;
  boardLabels: Label[];
  initialActivity: ActivityFeed | null;
}

export interface CreateBoardDrawerConfig {
  type: 'create-board';
  projectId: string;
}

export type DrawerConfig = CardDrawerConfig | CreateBoardDrawerConfig;
