import { Component, input, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BoardSettingsFacade } from '../../../services/board-settings.facade';
import { LabelListItemComponent } from '../label-list-item/label-list-item.component';
import { ButtonComponent } from '../../../../../shared/components/button/button.component';
import { ConfirmationModalComponent } from '../../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { SettingsSectionComponent } from '../../../../../shared/components/settings-section/settings-section.component';
import { Label, LABEL_COLORS, getColorSwatchClass as getColorSwatch } from '../../../models/label.model';

@Component({
  selector: 'app-label-management-section',
  imports: [FormsModule, LabelListItemComponent, ButtonComponent, ConfirmationModalComponent, SettingsSectionComponent],
  templateUrl: './label-management-section.component.html',
})
export class LabelManagementSectionComponent {
  facade = inject(BoardSettingsFacade);

  projectId = input.required<string>();
  boardId = input.required<string>();

  editingLabelId = signal<string | null>(null);
  newLabelName = signal('');
  newLabelColor = signal<string>(LABEL_COLORS[0]);
  isAddingLabel = signal(false);
  isSavingLabel = signal(false);
  showDeleteModal = signal(false);
  deletingLabelId = signal<string | null>(null);

  labelColors = LABEL_COLORS;

  getColorSwatchClass = getColorSwatch;

  startEdit(label: Label): void {
    this.editingLabelId.set(label.id);
  }

  cancelEdit(): void {
    this.editingLabelId.set(null);
    this.isSavingLabel.set(false);
  }

  saveEdit(data: { labelId: string; name: string; color: string }): void {
    this.isSavingLabel.set(true);
    this.facade.updateLabel(this.projectId(), this.boardId(), data.labelId, data.name, data.color);
    this.cancelEdit();
  }

  openDeleteModal(labelId: string): void {
    this.deletingLabelId.set(labelId);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
    this.deletingLabelId.set(null);
  }

  confirmDelete(): void {
    const labelId = this.deletingLabelId();
    if (!labelId) return;

    this.facade.deleteLabel(this.projectId(), this.boardId(), labelId);
    this.closeDeleteModal();
  }

  addLabel(): void {
    const name = this.newLabelName().trim();
    if (!name || this.isAddingLabel()) return;

    this.isAddingLabel.set(true);
    this.facade.addLabel(this.projectId(), this.boardId(), name, this.newLabelColor());
    this.newLabelName.set('');
    this.newLabelColor.set(LABEL_COLORS[0]);
    this.isAddingLabel.set(false);
  }
}
