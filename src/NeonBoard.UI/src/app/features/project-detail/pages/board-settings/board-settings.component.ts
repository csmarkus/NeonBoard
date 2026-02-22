import { Component, inject, signal, computed, OnInit, ChangeDetectionStrategy, effect, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { Observable, Subject, tap, switchMap } from 'rxjs';
import { PageHeaderComponent, BreadcrumbItem } from '../../../../shared/components/page-header/page-header.component';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import { HasUnsavedChanges } from '../../../../core/guards/unsaved-changes.guard';
import { ProjectService } from '../../../projects/services/project.service';
import { BoardService } from '../../services/board.service';
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
  private boardService = inject(BoardService);
  facade = inject(BoardSettingsFacade);
  private titleService = inject(Title);
  private destroyRef = inject(DestroyRef);

  shortId = signal('');
  slug = signal('');
  projectId = signal('');
  boardId = signal('');
  projectName = signal('');
  isDeleting = signal(false);
  showDiscardModal = signal(false);

  breadcrumbs = computed<BreadcrumbItem[]>(() => [
    { label: this.projectName(), link: ['/p', this.shortId()] },
    { label: this.facade.originalBoardName(), link: ['/p', this.shortId(), 'b', this.slug()] },
    { label: 'Settings' }
  ]);

  private discardSubject: Subject<boolean> | null = null;

  constructor() {
    effect(() => {
      const name = this.facade.originalBoardName();
      if (name) {
        this.titleService.setTitle(`Settings - ${name} | NeonBoard`);
      }
    });
  }

  ngOnInit(): void {
    const shortId = this.route.parent?.snapshot.paramMap.get('shortId') ?? '';
    const slug = this.route.snapshot.paramMap.get('slug') ?? '';

    if (shortId) this.shortId.set(shortId);
    if (slug) this.slug.set(slug);

    if (shortId && slug) {
      this.projectService.getProjectByShortId(shortId).pipe(
        tap(project => {
          this.projectId.set(project.id);
          this.projectName.set(project.name);
        }),
        switchMap(project => this.boardService.getBoardsByProject(project.id)),
        takeUntilDestroyed(this.destroyRef)
      ).subscribe({
        next: (boards) => {
          const board = boards.find(b => b.slug === slug);
          if (board) {
            this.boardId.set(board.id);
            this.facade.loadBoardSettings(this.projectId(), board.id);
          }
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
    this.facade.saveBoardSettings(this.projectId(), this.boardId()).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (board) => {
        // If slug changed (board was renamed), navigate to new URL
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
