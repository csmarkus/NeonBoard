import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet, ChildrenOutletContexts, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { routeAnimations } from './animations/route-animations';
import { LoadingOverlayComponent } from './shared/components/loading-overlay/loading-overlay.component';
import { LoadingService } from './core/services/loading.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, LoadingOverlayComponent],
  templateUrl: './app.html',
  animations: [routeAnimations]
})
export class App implements OnInit {
  private contexts = inject(ChildrenOutletContexts);
  private router = inject(Router);
  private loadingService = inject(LoadingService);

  ngOnInit(): void {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.loadingService.hide();
      });
  }

  getRouteAnimationData() {
    return this.contexts.getContext('primary')?.route?.snapshot?.data?.['animation'];
  }
}

