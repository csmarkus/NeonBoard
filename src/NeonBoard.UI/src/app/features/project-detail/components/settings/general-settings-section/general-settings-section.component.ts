import { Component, input, inject, signal, effect, ChangeDetectionStrategy } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { BoardSettingsFacade } from '../../../services/board-settings.facade';
import { InputComponent } from '../../../../../shared/components/input/input.component';
import { SettingsSectionComponent } from '../../../../../shared/components/settings-section/settings-section.component';

@Component({
  selector: 'app-general-settings-section',
  imports: [FormField, InputComponent, SettingsSectionComponent],
  templateUrl: './general-settings-section.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GeneralSettingsSectionComponent {
  facade = inject(BoardSettingsFacade);

  projectId = input.required<string>();
  boardId = input.required<string>();

  formModel = signal({ boardName: '', boardPrefix: '' });
  settingsForm = form(this.formModel);

  constructor() {
    effect(() => {
      this.formModel.set({
        boardName: this.facade.boardName(),
        boardPrefix: this.facade.boardPrefix(),
      });
    });
  }

  onNameInput(value: string): void {
    this.facade.updateBoardName(value);
  }

  onPrefixChange(value: string): void {
    this.facade.updateBoardPrefix(value.toUpperCase().replace(/[^A-Z]/g, '').slice(0, 5));
  }
}
