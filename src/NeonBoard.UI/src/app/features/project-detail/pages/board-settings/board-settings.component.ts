import { Component, inject, signal, computed, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Observable, Subject } from 'rxjs';
import { PageHeaderComponent, BreadcrumbItem } from '../../../../shared/components/page-header/page-header.component';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { HasUnsavedChanges } from '../../../../core/guards/unsaved-changes.guard';
import { ProjectService } from '../../../projects/services/project.service';
import { BoardSettingsFacade } from '../../services/board-settings.facade';
import { GeneralSettingsSectionComponent } from '../../components/general-settings-section/general-settings-section.component';
import { LabelManagementSectionComponent } from '../../components/label-management-section/label-management-section.component';
import { DangerZoneSectionComponent } from '../../components/danger-zone-section/danger-zone-section.component';

@Component({
  selector: 'app-board-settings',
  imports: [
    RouterLink,
    PageHeaderComponent,
    ButtonComponent,
    ConfirmationModalComponent,
    GeneralSettingsSectionComponent,
    LabelManagementSectionComponent,
    DangerZoneSectionComponent,
  ],
  host: {
    class: 'flex flex-col h-full'
  },
  templateUrl: './board-settings.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BoardSettingsComponent implements OnInit, HasUnsavedChanges {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private projectService = inject(ProjectService);
  facade = inject(BoardSettingsFacade);

  projectId = signal('');
  boardId = signal('');
  projectName = signal('');
  isDeleting = signal(false);
  showDiscardModal = signal(false);

  breadcrumbs = computed<BreadcrumbItem[]>(() => [
    { label: this.projectName(), link: ['/project', this.projectId()] },
    { label: this.facade.originalBoardName(), link: ['/project', this.projectId(), 'b', this.boardId()] },
    { label: 'Settings' }
  ]);

  private discardSubject: Subject<boolean> | null = null;

  ngOnInit(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (projectId) {
      this.projectId.set(projectId);
      this.projectService.getProject(projectId).subscribe({
        next: (project) => this.projectName.set(project.name),
      });
    }

    const boardId = this.route.snapshot.paramMap.get('boardId');
    if (boardId) {
      this.boardId.set(boardId);
      this.facade.loadBoardSettings(projectId!, boardId);
    }
  }

  hasUnsavedChanges(): boolean {
    return this.facade.hasChanges();
  }

  confirmDiscard(): Observable<boolean> {
    this.discardSubject = new Subject<boolean>();
    this.showDiscardModal.set(true);
    return this.discardSubject.asObservable();
  }

  onConfirmDiscard(): void {
    this.showDiscardModal.set(false);
    this.discardSubject?.next(true);
    this.discardSubject?.complete();
    this.discardSubject = null;
  }

  onCancelDiscard(): void {
    this.showDiscardModal.set(false);
    this.discardSubject?.next(false);
    this.discardSubject?.complete();
    this.discardSubject = null;
  }

  saveChanges(): void {
    this.facade.saveBoardSettings(this.projectId(), this.boardId());
  }

  onDeleteBoard(): void {
    this.isDeleting.set(true);

    this.facade.deleteBoard(this.projectId(), this.boardId()).subscribe({
      next: () => {
        this.router.navigate(['/project', this.projectId()]);
      },
      error: () => {
        this.isDeleting.set(false);
      }
    });
  }
}
