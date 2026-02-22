import { Component, inject, signal, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { ProjectService } from '../../../projects/services/project.service';
import { BoardService } from '../../services/board.service';
import { DrawerService } from '../../services/drawer.service';
import { Project } from '../../../projects/models/project.model';
import { Board } from '../../models/board.model';
import { BoardCardComponent } from '../../components/board-card/board-card.component';

@Component({
  selector: 'app-project',
  imports: [
    CommonModule,
    ButtonComponent,
    BoardCardComponent,
  ],
  host: {
    class: 'flex flex-col h-full'
  },
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './project.component.html',
  styleUrl: './project.component.css',
})
export class ProjectComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private projectService = inject(ProjectService);
  private boardService = inject(BoardService);
  private drawerService = inject(DrawerService);
  private titleService = inject(Title);

  shortId = signal<string>('');
  projectId = signal<string>('');
  project = signal<Project | null>(null);
  boards = signal<Board[]>([]);
  isLoading = signal<boolean>(true);
  error = signal<string | null>(null);

  ngOnInit(): void {
    const shortId = this.route.snapshot.paramMap.get('shortId');
    if (shortId) {
      this.projectService.getProjectByShortId(shortId).subscribe({
        next: (project) => {
          this.shortId.set(project.shortId);
          this.projectId.set(project.id);
          this.project.set(project);
          this.titleService.setTitle(`${project.name} | NeonBoard`);
          this.loadBoards();
        },
        error: (err) => {
          console.error('Error loading project:', err);
          this.error.set('Failed to load project');
          this.isLoading.set(false);
        }
      });
    }

    // Subscribe to board creation to reload boards list
    this.drawerService.boardCreated$.subscribe(() => {
      this.loadBoards();
    });
  }

  private loadBoards(): void {
    this.boardService.getBoardsByProject(this.projectId()).subscribe({
      next: (boards) => {
        this.boards.set(boards);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading boards:', err);
        this.error.set('Failed to load boards');
        this.isLoading.set(false);
      }
    });
  }

  openCreateBoardDrawer(): void {
    this.drawerService.openCreateBoardDrawer(this.projectId());
  }
}
