export interface Column {
  id: string;
  name: string;
  position: number;
  boardId: string;
}

export interface AddColumnRequest {
  name: string;
}

export interface RenameColumnRequest {
  newName: string;
}

export interface ReorderColumnsRequest {
  columnIds: string[];
}
