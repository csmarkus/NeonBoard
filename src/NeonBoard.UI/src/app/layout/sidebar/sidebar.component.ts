import { Component, input, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '@auth0/auth0-angular';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { SidebarStateFacade } from './services/sidebar-state.facade';
import { SIDEBAR_ICONS } from './sidebar.icons';
import { SidebarLogoComponent } from './components/sidebar-logo/sidebar-logo.component';
import { SidebarUserMenuComponent } from './components/sidebar-user-menu/sidebar-user-menu.component';
import { SidebarCollapseButtonComponent } from './components/sidebar-collapse-button/sidebar-collapse-button.component';

@Component({
  selector: 'app-sidebar',
  imports: [
    CommonModule,
    RouterLink,
    RouterLinkActive,
    FontAwesomeModule,
    SidebarLogoComponent,
    SidebarUserMenuComponent,
    SidebarCollapseButtonComponent
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'shrink-0'
  },
  templateUrl: './sidebar.component.html',
})
export class SidebarComponent {
  projectId = input.required<string>();
  shortId = input.required<string>();

  protected auth = inject(AuthService);
  protected facade = inject(SidebarStateFacade);
  protected icons = SIDEBAR_ICONS;

  logout(): void {
    this.auth.logout({
      logoutParams: {
        returnTo: window.location.origin
      }
    });
  }
}
