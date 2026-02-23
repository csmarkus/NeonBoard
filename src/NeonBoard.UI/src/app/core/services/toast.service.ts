import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error';

export interface Toast {
  id: string;
  type: ToastType;
  message: string;
}

const MAX_TOASTS = 3;
const AUTO_DISMISS_MS = 3000;

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private _toasts = signal<Toast[]>([]);
  private _nextId = 0;

  readonly toasts = this._toasts.asReadonly();

  success(message: string): void {
    this.add('success', message);
  }

  error(message: string): void {
    this.add('error', message);
  }

  remove(id: string): void {
    this._toasts.update(toasts => toasts.filter(t => t.id !== id));
  }

  private add(type: ToastType, message: string): void {
    const id = String(this._nextId++);
    const toast: Toast = { id, type, message };

    this._toasts.update(toasts => {
      const updated = [...toasts, toast];
      return updated.length > MAX_TOASTS ? updated.slice(updated.length - MAX_TOASTS) : updated;
    });

    setTimeout(() => this.remove(id), AUTO_DISMISS_MS);
  }
}
