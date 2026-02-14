import { Component, input, output, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { SIDEBAR_ICONS } from '../../sidebar.icons';

export interface UserInfo {
  name?: string;
  email?: string;
  picture?: string;
}

@Component({
  selector: 'app-sidebar-user-menu',
  imports: [CommonModule, FontAwesomeModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './sidebar-user-menu.component.html'
})
export class SidebarUserMenuComponent {
  user = input.required<UserInfo>();
  collapsed = input.required<boolean>();
  menuOpen = input.required<boolean>();
  buttonClasses = input.required<string>();
  menuClasses = input.required<string>();

  toggleMenu = output<void>();
  closeMenu = output<void>();
  logoutClick = output<void>();

  protected icons = SIDEBAR_ICONS;

  getUserInitials(name: string | undefined): string {
    if (!name) return '?';
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .substring(0, 2);
  }
}
