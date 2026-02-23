import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ProjectSettingsFacade } from '../../services/project-settings.facade';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { SettingsSectionComponent } from '../../../../shared/components/settings-section/settings-section.component';

@Component({
  selector: 'app-project-general-settings-section',
  imports: [FormsModule, InputComponent, SettingsSectionComponent],
  templateUrl: './project-general-settings-section.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectGeneralSettingsSectionComponent {
  facade = inject(ProjectSettingsFacade);

  onNameInput(value: string): void {
    this.facade.updateProjectName(value);
  }

  onDescriptionInput(value: string): void {
    this.facade.updateProjectDescription(value);
  }
}
