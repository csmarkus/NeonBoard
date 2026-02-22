import { Component, inject, signal, OnInit, computed, effect, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { EMPTY, tap, switchMap } from 'rxjs';
import { ProjectService } from '../../../projects/services/project.service';
import { BoardService } from '../../services/board.service';
import { PageHeaderComponent, BreadcrumbItem } from '../../../../shared/components/page-header/page-header.component';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { BoardToolbarComponent } from '../../components/board-toolbar/board-toolbar.component';
import { BoardCanvasComponent } from '../../components/board-canvas/board-canvas.component';
import { BoardStateFacade } from '../../services/board-state.facade';

@Component({
  selector: 'app-board-view',
  imports: [
    PageHeaderComponent,
    InputComponent,
    BoardToolbarComponent,
    BoardCanvasComponent,
  ],
  host: {
    class: 'flex flex-col h-full'
  },
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './board-view.component.html',
  styleUrl: './board-view.component.css',
})
export class BoardViewComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private projectService = inject(ProjectService);
  private boardService = inject(BoardService);
  private facade = inject(BoardStateFacade);
  private titleService = inject(Title);
  private destroyRef = inject(DestroyRef);

  shortId = signal<string>('');
  slug = signal<string>('');
  projectId = signal<string>('');
  boardId = signal<string>('');
  projectName = signal<string>('');

  boardName = computed(() => this.facade.board()?.name ?? '');

  breadcrumbs = computed<BreadcrumbItem[]>(() => [
    { label: this.projectName(), link: ['/p', this.shortId()] },
    { label: this.boardName() }
  ]);

  constructor() {
    effect(() => {
      const name = this.boardName();
      if (name) {
        this.titleService.setTitle(`${name} | NeonBoard`);
      }
    });
  }

  ngOnInit(): void {
    const shortId = this.route.parent?.snapshot.paramMap.get('shortId') ?? '';
    this.shortId.set(shortId);

    this.route.paramMap.pipe(
      switchMap(params => {
        const slug = params.get('slug') ?? '';
        this.slug.set(slug);

        if (!shortId || !slug) return EMPTY;

        return this.projectService.getProjectByShortId(shortId).pipe(
          tap(project => {
            this.projectId.set(project.id);
            this.projectName.set(project.name);
          }),
          switchMap(project => this.boardService.getBoardsByProject(project.id)),
          tap(boards => {
            const board = boards.find(b => b.slug === slug);
            if (board) {
              this.boardId.set(board.id);
            }
          })
        );
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe();
  }
}
