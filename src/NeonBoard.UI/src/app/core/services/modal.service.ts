import { Injectable, computed, signal } from '@angular/core';
import { GradientVariant } from '../../shared/components/gradient-accent/gradient-accent.component';

export interface ConfirmationModalConfig {
  title: string;
  message: string;
  confirmText: string;
  cancelText: string;
  variant: 'danger' | 'primary';
  gradientVariant: GradientVariant;
}

export type ConfirmationModalOptions = Partial<ConfirmationModalConfig> & Pick<ConfirmationModalConfig, 'message'>;

const DEFAULTS: Omit<ConfirmationModalConfig, 'message'> = {
  title: 'Confirm Action',
  confirmText: 'Confirm',
  cancelText: 'Cancel',
  variant: 'danger',
  gradientVariant: 'pink',
};

@Injectable({
  providedIn: 'root'
})
export class ModalService {
  private _config = signal<ConfirmationModalConfig | null>(null);
  private _resolve: ((result: boolean) => void) | null = null;

  readonly config = this._config.asReadonly();
  readonly isOpen = computed(() => this._config() !== null);

  confirm(options: ConfirmationModalOptions): Promise<boolean> {
    if (this._resolve) {
      this._resolve(false);
      this._resolve = null;
    }

    const config: ConfirmationModalConfig = { ...DEFAULTS, ...options };
    this._config.set(config);

    return new Promise<boolean>((resolve) => {
      this._resolve = resolve;
    });
  }

  resolve(result: boolean): void {
    if (this._resolve) {
      this._resolve(result);
      this._resolve = null;
    }
    this._config.set(null);
  }
}
