import { Component, input, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BoardSettingsFacade } from '../../services/board-settings.facade';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { SettingsSectionComponent } from '../../../../shared/components/settings-section/settings-section.component';

@Component({
  selector: 'app-general-settings-section',
  imports: [FormsModule, InputComponent, SettingsSectionComponent],
  templateUrl: './general-settings-section.component.html',
})
export class GeneralSettingsSectionComponent {
  facade = inject(BoardSettingsFacade);

  projectId = input.required<string>();
  boardId = input.required<string>();

  onNameInput(value: string): void {
    this.facade.updateBoardName(value);
  }

  onPrefixChange(value: string): void {
    this.facade.updateBoardPrefix(value.toUpperCase().replace(/[^A-Z]/g, '').slice(0, 5));
  }
}
