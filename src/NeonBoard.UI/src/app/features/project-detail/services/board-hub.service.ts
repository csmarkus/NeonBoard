import { Injectable, inject, signal } from '@angular/core';
import { SignalRService } from '../../../core/services/signalr.service';

@Injectable({
  providedIn: 'root',
})
export class BoardHubService {
  private signalR = inject(SignalRService);
  private currentBoardId = signal<string | null>(null);
  private currentProjectId = signal<string | null>(null);
  private eventCallbacks = new Map<string, ((data: unknown) => void)[]>();
  private _currentUserId = signal<string | null>(null);

  readonly connectionState = this.signalR.connectionState;
  readonly currentUserId = this._currentUserId.asReadonly();

  private reconnectHandler = (): void => {
    const boardId = this.currentBoardId();
    const projectId = this.currentProjectId();
    if (boardId) {
      this.signalR.invoke<string>('JoinBoard', boardId)
        .then(userId => { if (userId) this._currentUserId.set(userId); })
        .catch(console.error);
    } else if (projectId) {
      this.signalR.invoke('JoinProject', projectId).catch(console.error);
    }
    this.triggerEvent('Reconnected', {});
  };

  async joinBoard(boardId: string): Promise<void> {
    await this.signalR.connect();
    this.signalR.onReconnected(this.reconnectHandler);
    this.currentBoardId.set(boardId);

    try {
      const userId = await this.signalR.invoke<string>('JoinBoard', boardId);
      if (userId) {
        this._currentUserId.set(userId);
      }
    } catch (err) {
      console.error('Failed to join board:', err);
    }
  }

  async leaveBoard(): Promise<void> {
    const boardId = this.currentBoardId();
    if (boardId) {
      try {
        await this.signalR.invoke('LeaveBoard', boardId);
      } catch {
        // Connection may already be closed
      }
      this.currentBoardId.set(null);
    }
    this.signalR.offReconnected(this.reconnectHandler);
  }

  async joinProject(projectId: string): Promise<void> {
    await this.signalR.connect();
    this.currentProjectId.set(projectId);

    try {
      await this.signalR.invoke('JoinProject', projectId);
    } catch (err) {
      console.error('Failed to join project:', err);
    }
  }

  async leaveProject(): Promise<void> {
    const projectId = this.currentProjectId();
    if (projectId) {
      try {
        await this.signalR.invoke('LeaveProject', projectId);
      } catch {
        // Connection may already be closed
      }
      this.currentProjectId.set(null);
    }
  }

  onEvent<T>(eventType: string, callback: (data: T) => void): void {
    this.signalR.on<T>(eventType, callback);

    if (!this.eventCallbacks.has(eventType)) {
      this.eventCallbacks.set(eventType, []);
    }
    this.eventCallbacks.get(eventType)!.push(callback as (data: unknown) => void);
  }

  offEvent(eventType: string): void {
    this.signalR.off(eventType);
    this.eventCallbacks.delete(eventType);
  }

  offAllEvents(): void {
    for (const eventType of this.eventCallbacks.keys()) {
      this.signalR.off(eventType);
    }
    this.eventCallbacks.clear();
  }

  private triggerEvent(type: string, data: unknown): void {
    const callbacks = this.eventCallbacks.get(type);
    if (callbacks) {
      callbacks.forEach((cb) => cb(data));
    }
  }
}
