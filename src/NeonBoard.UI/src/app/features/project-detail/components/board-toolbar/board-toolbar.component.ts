import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faGear } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-board-toolbar',
  imports: [RouterLink, FontAwesomeModule],
  templateUrl: './board-toolbar.component.html',
})
export class BoardToolbarComponent {
  projectId = input.required<string>();
  boardId = input.required<string>();
  faGear = faGear;
}
