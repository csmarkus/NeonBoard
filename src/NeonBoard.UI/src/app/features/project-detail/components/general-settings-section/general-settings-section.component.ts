import { Component, input, inject } from '@angular/core';
import { BoardSettingsFacade } from '../../services/board-settings.facade';

@Component({
  selector: 'app-general-settings-section',
  imports: [],
  templateUrl: './general-settings-section.component.html',
})
export class GeneralSettingsSectionComponent {
  facade = inject(BoardSettingsFacade);

  projectId = input.required<string>();
  boardId = input.required<string>();

  onNameInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.facade.updateBoardName(input.value);
  }
}
