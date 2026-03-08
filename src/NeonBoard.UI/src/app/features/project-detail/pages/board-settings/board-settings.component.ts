import { Component, inject, signal, computed, ChangeDetectionStrategy, effect, DestroyRef } from '@angular/core';
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
import { BoardSettingsFacade } from '../../services/board-settings.facade';
import { ProjectContext } from '../../services/project-context.service';
import { GeneralSettingsSectionComponent } from '../../components/settings/general-settings-section/general-settings-section.component';
import { LabelManagementSectionComponent } from '../../components/settings/label-management-section/label-management-section.component';
import { DangerZoneSectionComponent } from '../../components/settings/danger-zone-section/danger-zone-section.component';

@Component({
  selector: 'app-board-settings',
  imports: [
    RouterLink,
    FontAwesomeModule,
    PageHeaderComponent,
    ButtonComponent,
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
export class BoardSettingsComponent implements HasUnsavedChanges {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private projectContext = inject(ProjectContext);
  facade = inject(BoardSettingsFacade);
  private titleService = inject(Title);
  private destroyRef = inject(DestroyRef);
  private modalService = inject(ModalService);

  faChevronLeft = faChevronLeft;

  shortId = this.projectContext.shortId;
  projectId = this.projectContext.projectId;
  projectName = this.projectContext.projectName;
  slug = signal('');
  boardId = signal('');
  isDeleting = signal(false);

  breadcrumbs = computed<BreadcrumbItem[]>(() => [
    { label: this.projectName(), link: ['/p', this.shortId()] },
    { label: this.facade.originalBoardName(), link: ['/p', this.shortId(), 'b', this.slug()] },
    { label: 'Settings' }
  ]);

  constructor() {
    const slug = this.route.snapshot.paramMap.get('slug') ?? '';
    this.slug.set(slug);

    this.facade.resetState();

    effect(() => {
      const name = this.facade.originalBoardName();
      if (name) {
        this.titleService.setTitle(`Settings - ${name} | NeonBoard`);
      }
    });

    effect(() => {
      const boards = this.projectContext.boards();
      const currentSlug = this.slug();
      if (boards.length > 0 && currentSlug) {
        const board = boards.find(b => b.slug === currentSlug);
        if (board) {
          this.boardId.set(board.id);
          this.facade.loadBoardSettings(this.projectContext.projectId(), board.id);
        }
      }
    });
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
    this.facade.saveBoardSettings(this.projectId(), this.boardId()).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (board) => {
        if (board.slug !== this.slug()) {
          this.slug.set(board.slug);
          this.router.navigate(['/p', this.shortId(), 'b', board.slug, 'settings']);
        }
      }
    });
  }

  onDeleteBoard(): void {
    this.isDeleting.set(true);

    this.facade.deleteBoard(this.projectId(), this.boardId()).subscribe({
      next: () => {
        this.router.navigate(['/p', this.shortId()]);
      },
      error: () => {
        this.isDeleting.set(false);
      }
    });
  }
}
