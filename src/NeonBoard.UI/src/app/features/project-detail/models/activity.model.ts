export interface ActivityEntry {
  id: string;
  boardId: string;
  entityType: string;
  entityId: string;
  actionType: string;
  data: Record<string, unknown>;
  occurredAt: string;
}

export interface ActivityFeed {
  entries: ActivityEntry[];
  nextCursor: string | null;
}
