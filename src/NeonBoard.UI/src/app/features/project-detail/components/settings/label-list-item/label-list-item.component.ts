import { Component, input, output, signal, effect, ChangeDetectionStrategy } from '@angular/core';
import { form } from '@angular/forms/signals';
import { Label, LABEL_COLORS, getLabelClassString, getColorSwatchClass as getColorSwatch } from '../../../models/label.model';

@Component({
  selector: 'app-label-list-item',
  imports: [],
  host: {
    class: 'block'
  },
  templateUrl: './label-list-item.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabelListItemComponent {
  label = input.required<Label>();
  isEditing = input.required<boolean>();
  isSaving = input.required<boolean>();

  edit = output<Label>();
  save = output<{ labelId: string; name: string; color: string }>();
  delete = output<string>();
  cancel = output<void>();

  editModel = signal({ name: '' });
  editForm = form(this.editModel);
  editColor = signal('');
  labelColors = LABEL_COLORS;

  constructor() {
    effect(() => {
      if (this.isEditing()) {
        this.editModel.set({ name: this.label().name });
        this.editColor.set(this.label().color);
      }
    });
  }

  getLabelClasses = getLabelClassString;

  getColorSwatchClass = getColorSwatch;

  onEdit(): void {
    this.edit.emit(this.label());
  }

  onEditNameInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.editModel.set({ name: value });
  }

  onSave(): void {
    const name = this.editModel().name.trim();
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
