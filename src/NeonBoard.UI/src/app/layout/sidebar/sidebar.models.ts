import { IconDefinition } from '@fortawesome/fontawesome-svg-core';

export interface NavItem {
  icon: IconDefinition;
  label: string;
  route?: string;
}

export interface SidebarState {
  collapsed: boolean;
  userMenuOpen: boolean;
  boardsMenuOpen: boolean;
}
