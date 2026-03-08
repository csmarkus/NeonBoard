import { Injectable, computed, signal } from '@angular/core';
import { DrawerConfig } from '../models/drawer.model';

@Injectable({
  providedIn: 'root'
})
export class DrawerService {
  private _config = signal<DrawerConfig | null>(null);

  readonly config = this._config.asReadonly();
  readonly isOpen = computed(() => this._config() !== null);

  open(config: DrawerConfig): void {
    this._config.set(config);
  }

  close(): void {
    this._config.set(null);
  }
}
