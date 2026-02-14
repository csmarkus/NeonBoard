import { Component, input, output, ChangeDetectionStrategy } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { SIDEBAR_ICONS } from '../../sidebar.icons';

@Component({
  selector: 'app-sidebar-collapse-button',
  imports: [FontAwesomeModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './sidebar-collapse-button.component.html'
})
export class SidebarCollapseButtonComponent {
  collapsed = input.required<boolean>();
  buttonClasses = input.required<string>();
  toggle = output<void>();

  protected icons = SIDEBAR_ICONS;
}
