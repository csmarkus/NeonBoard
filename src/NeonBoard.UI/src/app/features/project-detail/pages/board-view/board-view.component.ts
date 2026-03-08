import { Component, inject, signal, computed, effect, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { PageHeaderComponent, BreadcrumbItem } from '../../../../shared/components/page-header/page-header.component';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { BoardToolbarComponent } from '../../components/board/board-toolbar/board-toolbar.component';
import { BoardCanvasComponent } from '../../components/board/board-canvas/board-canvas.component';
import { BoardStateFacade } from '../../services/board-state.facade';
import { ProjectContext } from '../../services/project-context.service';

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
export class BoardViewComponent {
  private route = inject(ActivatedRoute);
  private projectContext = inject(ProjectContext);
  private facade = inject(BoardStateFacade);
  private titleService = inject(Title);

  shortId = this.projectContext.shortId;
  projectId = this.projectContext.projectId;
  projectName = this.projectContext.projectName;

  slug = signal<string>('');
  boardId = signal<string>('');

  boardName = computed(() => this.facade.board()?.name ?? '');

  breadcrumbs = computed<BreadcrumbItem[]>(() => [
    { label: this.projectName(), link: ['/p', this.shortId()] },
    { label: this.boardName() }
  ]);

  constructor() {
    const slug = this.route.snapshot.paramMap.get('slug') ?? '';
    this.slug.set(slug);

    effect(() => {
      const name = this.boardName();
      if (name) {
        this.titleService.setTitle(`${name} | NeonBoard`);
      }
    });

    effect(() => {
      const boards = this.projectContext.boards();
      const currentSlug = this.slug();
      if (boards.length > 0 && currentSlug) {
        const board = boards.find(b => b.slug === currentSlug);
        if (board) {
          this.boardId.set(board.id);
        }
      }
    });
  }
}
