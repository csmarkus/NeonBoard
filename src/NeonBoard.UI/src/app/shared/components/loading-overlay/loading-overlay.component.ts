import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../../core/services/loading.service';

@Component({
  selector: 'app-loading-overlay',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loading-overlay.component.html',
})
export class LoadingOverlayComponent {
  private loadingService = inject(LoadingService);

  get isLoading(): boolean {
    return this.loadingService.isLoading();
  }
}
