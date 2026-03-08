import { Component, inject, signal, computed, OnInit, ChangeDetectionStrategy, effect, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faChevronLeft } from '@fortawesome/free-solid-svg-icons';
import { Observable, from } from 'rxjs';
import { PageHeaderComponent, BreadcrumbItem } from '../../../../shared/components/page-header/page-header.component';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { HasUnsavedChanges } from '../../../../core/guards/unsaved-changes.guard';
import { ModalService } from '../../../../core/services/modal.service';
import { ProjectService } from '../../../projects/services/project.service';
import { ProjectSettingsFacade } from '../../services/project-settings.facade';
import { ProjectGeneralSettingsSectionComponent } from '../../components/settings/project-general-settings-section/project-general-settings-section.component';
import { DangerZoneSectionComponent } from '../../components/settings/danger-zone-section/danger-zone-section.component';
import { MembersSectionComponent } from '../../components/settings/members-section/members-section.component';
import { ProjectRole } from '../../models/member.model';

@Component({
  selector: 'app-project-settings',
  imports: [
    RouterLink,
    FontAwesomeModule,
    PageHeaderComponent,
    ButtonComponent,
    ProjectGeneralSettingsSectionComponent,
    MembersSectionComponent,
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
  private modalService = inject(ModalService);
  facade = inject(ProjectSettingsFacade);
  private titleService = inject(Title);

  faChevronLeft = faChevronLeft;
  private destroyRef = inject(DestroyRef);

  shortId = signal('');
  projectId = signal('');
  currentUserRole = signal<ProjectRole | undefined>(undefined);
  isDeleting = signal(false);

  breadcrumbs = computed<BreadcrumbItem[]>(() => [
    { label: this.facade.originalProjectName(), link: ['/p', this.shortId()] },
    { label: 'Settings' }
  ]);

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

    this.facade.resetState();

    if (shortId) {
      this.shortId.set(shortId);

      this.projectService.getProjectByShortId(shortId).pipe(
        takeUntilDestroyed(this.destroyRef)
      ).subscribe({
        next: (project) => {
          this.projectId.set(project.id);
          this.currentUserRole.set(project.currentUserRole);
          this.facade.loadProjectSettings(project.id);
        }
      });
    }
  }

  hasUnsavedChanges(): boolean {
    return this.facade.hasChanges();
  }

  confirmDiscard(): Observable<boolean> {
    return from(this.modalService.confirm({
      title: 'Unsaved Changes',
      message: 'You have unsaved changes. Are you sure you want to leave? Your changes will be lost.',
      confirmText: 'Discard',
      cancelText: 'Keep editing',
    }));
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
