import { Component, inject, signal, computed, OnInit, ChangeDetectionStrategy, effect, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Observable, Subject } from 'rxjs';
import { PageHeaderComponent, BreadcrumbItem } from '../../../../shared/components/page-header/page-header.component';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { HasUnsavedChanges } from '../../../../core/guards/unsaved-changes.guard';
import { ProjectService } from '../../../projects/services/project.service';
import { ProjectSettingsFacade } from '../../services/project-settings.facade';
import { ProjectGeneralSettingsSectionComponent } from '../../components/project-general-settings-section/project-general-settings-section.component';
import { DangerZoneSectionComponent } from '../../components/danger-zone-section/danger-zone-section.component';

@Component({
  selector: 'app-project-settings',
  imports: [
    RouterLink,
    PageHeaderComponent,
    ButtonComponent,
    ConfirmationModalComponent,
    ProjectGeneralSettingsSectionComponent,
    DangerZoneSectionComponent,
  ],
  host: {
    class: 'flex flex-col h-full'
  },
  templateUrl: './project-settings.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectSettingsComponent implements OnInit, HasUnsavedChanges {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private projectService = inject(ProjectService);
  facade = inject(ProjectSettingsFacade);
  private titleService = inject(Title);
  private destroyRef = inject(DestroyRef);

  shortId = signal('');
  projectId = signal('');
  isDeleting = signal(false);
  showDiscardModal = signal(false);

  breadcrumbs = computed<BreadcrumbItem[]>(() => [
    { label: this.facade.originalProjectName(), link: ['/p', this.shortId()] },
    { label: 'Settings' }
  ]);

  private discardSubject: Subject<boolean> | null = null;

  constructor() {
    effect(() => {
      const name = this.facade.originalProjectName();
      if (name) {
        this.titleService.setTitle(`Settings - ${name} | NeonBoard`);
      }
    });
  }

  ngOnInit(): void {
    const shortId = this.route.parent?.snapshot.paramMap.get('shortId') ?? '';

    if (shortId) {
      this.shortId.set(shortId);

      this.projectService.getProjectByShortId(shortId).pipe(
        takeUntilDestroyed(this.destroyRef)
      ).subscribe({
        next: (project) => {
          this.projectId.set(project.id);
          this.facade.loadProjectSettings(project.id);
        }
      });
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
    this.facade.saveProjectSettings(this.projectId()).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe();
  }

  onDeleteProject(): void {
    this.isDeleting.set(true);

    this.facade.deleteProject(this.projectId()).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.router.navigate(['/projects']);
      },
      error: () => {
        this.isDeleting.set(false);
      }
    });
  }
}
