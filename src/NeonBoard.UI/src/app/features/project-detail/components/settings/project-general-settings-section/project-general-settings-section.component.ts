import { Component, inject, signal, effect, ChangeDetectionStrategy } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { ProjectSettingsFacade } from '../../../services/project-settings.facade';
import { InputComponent } from '../../../../../shared/components/input/input.component';
import { SettingsSectionComponent } from '../../../../../shared/components/settings-section/settings-section.component';

@Component({
  selector: 'app-project-general-settings-section',
  imports: [FormField, InputComponent, SettingsSectionComponent],
  templateUrl: './project-general-settings-section.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectGeneralSettingsSectionComponent {
  facade = inject(ProjectSettingsFacade);

  formModel = signal({ projectName: '', description: '' });
  settingsForm = form(this.formModel);

  constructor() {
    effect(() => {
      this.formModel.set({
        projectName: this.facade.projectName(),
        description: this.facade.projectDescription(),
      });
    });
  }

  onNameInput(value: string): void {
    this.facade.updateProjectName(value);
  }

  onDescriptionInput(value: string): void {
    this.facade.updateProjectDescription(value);
  }
}
