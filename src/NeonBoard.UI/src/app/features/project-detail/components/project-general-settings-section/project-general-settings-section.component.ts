import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { ProjectSettingsFacade } from '../../services/project-settings.facade';

@Component({
  selector: 'app-project-general-settings-section',
  imports: [],
  templateUrl: './project-general-settings-section.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectGeneralSettingsSectionComponent {
  facade = inject(ProjectSettingsFacade);

  onNameInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.facade.updateProjectName(input.value);
  }

  onDescriptionInput(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    this.facade.updateProjectDescription(textarea.value);
  }
}
