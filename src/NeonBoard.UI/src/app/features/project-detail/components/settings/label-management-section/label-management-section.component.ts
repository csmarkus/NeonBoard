import { Component, input, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BoardSettingsFacade } from '../../../services/board-settings.facade';
import { LabelListItemComponent } from '../label-list-item/label-list-item.component';
import { ButtonComponent } from '../../../../../shared/components/button/button.component';
import { SettingsSectionComponent } from '../../../../../shared/components/settings-section/settings-section.component';
import { ModalService } from '../../../../../core/services/modal.service';
import { Label, LABEL_COLORS, getColorSwatchClass as getColorSwatch } from '../../../models/label.model';

@Component({
  selector: 'app-label-management-section',
  imports: [FormsModule, LabelListItemComponent, ButtonComponent, SettingsSectionComponent],
  templateUrl: './label-management-section.component.html',
})
export class LabelManagementSectionComponent {
  facade = inject(BoardSettingsFacade);
  private modalService = inject(ModalService);

  projectId = input.required<string>();
  boardId = input.required<string>();

  editingLabelId = signal<string | null>(null);
  newLabelName = signal('');
  newLabelColor = signal<string>(LABEL_COLORS[0]);
  isAddingLabel = signal(false);
  isSavingLabel = signal(false);

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

  async openDeleteModal(labelId: string): Promise<void> {
    const confirmed = await this.modalService.confirm({
      title: 'Delete Label',
      message: 'Are you sure you want to delete this label? It will be removed from all cards. This action cannot be undone.',
      confirmText: 'Delete Label',
    });
    if (confirmed) {
      this.facade.deleteLabel(this.projectId(), this.boardId(), labelId);
    }
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
