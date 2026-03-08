import { Injectable, inject, signal, computed } from '@angular/core';
import { ProjectService } from '../../projects/services/project.service';
import { BoardService } from './board.service';
import { Project } from '../../projects/models/project.model';
import { Board } from '../models/board.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectContext {
  private projectService = inject(ProjectService);
  private boardService = inject(BoardService);

  private _project = signal<Project | null>(null);
  private _boards = signal<Board[]>([]);
  private _boardsLoaded = signal(false);

  readonly project = this._project.asReadonly();
  readonly boards = this._boards.asReadonly();
  readonly boardsLoaded = this._boardsLoaded.asReadonly();
  readonly projectId = computed(() => this._project()?.id ?? '');
  readonly projectName = computed(() => this._project()?.name ?? '');
  readonly shortId = computed(() => this._project()?.shortId ?? '');
  readonly currentUserRole = computed(() => this._project()?.currentUserRole);

  resolve(shortId: string): void {
    if (this._project()?.shortId === shortId) return;

    this.clear();
    this.projectService.getProjectByShortId(shortId).subscribe({
      next: (project) => {
        this._project.set(project);
        this.reloadBoards();
      },
    });
  }

  reloadBoards(): void {
    const id = this.projectId();
    if (!id) return;

    this.boardService.getBoardsByProject(id).subscribe({
      next: (boards) => {
        this._boards.set(boards);
        this._boardsLoaded.set(true);
      },
    });
  }

  findBoardBySlug(slug: string): Board | undefined {
    return this._boards().find(b => b.slug === slug);
  }

  clear(): void {
    this._project.set(null);
    this._boards.set([]);
    this._boardsLoaded.set(false);
  }
}
