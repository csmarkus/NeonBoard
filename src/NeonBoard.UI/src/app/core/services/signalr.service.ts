import { Injectable, inject, signal } from '@angular/core';
import { AuthService } from '@auth0/auth0-angular';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';

export type ConnectionState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  private auth = inject(AuthService);
  private connection: HubConnection | null = null;
  private reconnectedCallbacks: (() => void)[] = [];

  readonly connectionState = signal<ConnectionState>('disconnected');

  async connect(): Promise<void> {
    if (this.connection?.state === HubConnectionState.Connected) return;

    const baseUrl = environment.apiUrl.replace('/api', '');

    this.connection = new HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/board`, {
        accessTokenFactory: () => firstValueFrom(this.auth.getAccessTokenSilently()),
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(environment.enableDebugLogging ? LogLevel.Information : LogLevel.Warning)
      .build();

    this.connection.onreconnecting(() => {
      this.connectionState.set('reconnecting');
    });

    this.connection.onreconnected(() => {
      this.connectionState.set('connected');
      this.reconnectedCallbacks.forEach((cb) => cb());
    });

    this.connection.onclose(() => {
      this.connectionState.set('disconnected');
    });

    try {
      this.connectionState.set('connecting');
      await this.connection.start();
      this.connectionState.set('connected');
    } catch (err) {
      this.connectionState.set('disconnected');
      console.error('SignalR connection failed:', err);
    }
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      this.connectionState.set('disconnected');
    }
  }

  async invoke(method: string, ...args: unknown[]): Promise<void> {
    if (this.connection?.state === HubConnectionState.Connected) {
      await this.connection.invoke(method, ...args);
    }
  }

  on<T>(eventName: string, callback: (data: T) => void): void {
    this.connection?.on(eventName, callback);
  }

  off(eventName: string): void {
    this.connection?.off(eventName);
  }

  onReconnected(callback: () => void): void {
    this.reconnectedCallbacks.push(callback);
  }

  offReconnected(callback: () => void): void {
    this.reconnectedCallbacks = this.reconnectedCallbacks.filter((cb) => cb !== callback);
  }
}
