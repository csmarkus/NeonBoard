import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterOutlet, NavigationStart, ChildrenOutletContexts } from '@angular/router';
import { LoadingOverlayComponent } from './shared/components/loading-overlay/loading-overlay.component';
import { LoadingService } from './core/services/loading.service';
import { routeAnimations } from './animations/route-animations';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, LoadingOverlayComponent],
  templateUrl: './app.html',
  animations: [routeAnimations]
})
export class App implements OnInit {
  private router = inject(Router);
  private loadingService = inject(LoadingService);
  private contexts = inject(ChildrenOutletContexts);

  ngOnInit() {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationStart) {
        this.loadingService.show();
      }
    });
  }

  getRouteAnimationData() {
    return this.contexts.getContext('primary')?.route?.snapshot?.data?.['animation'];
  }
}

