import { Component, input, output, signal, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Label, LABEL_COLORS, getLabelClassString, getColorSwatchClass as getColorSwatch } from '../../models/label.model';

@Component({
  selector: 'app-label-list-item',
  imports: [FormsModule],
  host: {
    class: 'block'
  },
  templateUrl: './label-list-item.component.html',
})
export class LabelListItemComponent {
  label = input.required<Label>();
  isEditing = input.required<boolean>();
  isSaving = input.required<boolean>();

  edit = output<Label>();
  save = output<{ labelId: string; name: string; color: string }>();
  delete = output<string>();
  cancel = output<void>();

  editName = signal('');
  editColor = signal('');
  labelColors = LABEL_COLORS;

  constructor() {
    effect(() => {
      if (this.isEditing()) {
        this.editName.set(this.label().name);
        this.editColor.set(this.label().color);
      }
    });
  }

  getLabelClasses = getLabelClassString;

  getColorSwatchClass = getColorSwatch;

  onEdit(): void {
    this.edit.emit(this.label());
  }

  onSave(): void {
    const name = this.editName().trim();
    if (name) {
      this.save.emit({
        labelId: this.label().id,
        name,
        color: this.editColor()
      });
    }
  }

  onDelete(): void {
    this.delete.emit(this.label().id);
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
