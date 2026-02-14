import { Component, input, output, signal, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Label, LABEL_COLORS, getLabelColorClasses } from '../../models/label.model';

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

  getLabelClasses(color: string): string {
    const classes = getLabelColorClasses(color);
    return `${classes.bg} ${classes.text} ${classes.border}`;
  }

  getColorSwatchClass(color: string): string {
    const map: Record<string, string> = {
      red: 'bg-red-500',
      orange: 'bg-orange-500',
      yellow: 'bg-yellow-600',
      lime: 'bg-lime-500',
      cyan: 'bg-cyan-500',
      blue: 'bg-blue-500',
      purple: 'bg-purple-500',
      violet: 'bg-violet-500',
      magenta: 'bg-fuchsia-500',
      pink: 'bg-pink-500',
      grey: 'bg-gray-500',
    };
    return map[color] ?? 'bg-gray-500';
  }

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
