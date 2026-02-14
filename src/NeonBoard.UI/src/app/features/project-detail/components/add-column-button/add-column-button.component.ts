import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-add-column-button',
  imports: [FormsModule],
  templateUrl: './add-column-button.component.html',
})
export class AddColumnButtonComponent {
  isAdding = input.required<boolean>();
  columnName = input.required<string>();

  openAdd = output<void>();
  cancelAdd = output<void>();
  addColumn = output<string>();
  nameChange = output<string>();

  onOpenAdd(): void {
    this.openAdd.emit();
  }

  onCancelAdd(): void {
    this.cancelAdd.emit();
  }

  onAddColumn(): void {
    const name = this.columnName().trim();
    if (name) {
      this.addColumn.emit(name);
    }
  }

  onNameChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.nameChange.emit(target.value);
  }
}
