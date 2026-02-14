import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProjectService } from '../../../projects/services/project.service';
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
  templateUrl: './board-view.component.html',
  styleUrl: './board-view.component.css',
})
export class BoardViewComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private projectService = inject(ProjectService);
  private facade = inject(BoardStateFacade);

  projectId = signal<string>('');
  boardId = signal<string>('');
  projectName = signal<string>('');

  boardName = computed(() => this.facade.board()?.name ?? '');

  breadcrumbs = computed<BreadcrumbItem[]>(() => [
    { label: this.projectName(), link: ['/project', this.projectId()] },
    { label: this.boardName() }
  ]);

  ngOnInit(): void {
    const projectId = this.route.parent?.snapshot.paramMap.get('projectId');
    if (projectId) {
      this.projectId.set(projectId);
      this.projectService.getProject(projectId).subscribe({
        next: (project) => this.projectName.set(project.name),
      });
    }

    this.route.paramMap.subscribe(params => {
      const boardId = params.get('boardId');
      if (boardId) {
        this.boardId.set(boardId);
      }
    });
  }
}
