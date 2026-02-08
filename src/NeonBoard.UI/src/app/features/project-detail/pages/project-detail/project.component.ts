import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { SidebarComponent } from '../../../../layout/sidebar/sidebar.component';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { ProjectService } from '../../../projects/services/project.service';
import { BoardService } from '../../services/board.service';
import { Project } from '../../../projects/models/project.model';
import { Board } from '../../models/board.model';
import { BoardCardComponent } from '../../components/board-card/board-card.component';
import { CreateBoardDrawerComponent } from '../../components/create-board-drawer/create-board-drawer.component';

@Component({
  selector: 'app-project',
  standalone: true,
  imports: [
    CommonModule,
    SidebarComponent,
    ButtonComponent,
    BoardCardComponent,
    CreateBoardDrawerComponent,
  ],
  host: {
    class: 'block h-screen'
  },
  templateUrl: './project.component.html',
  styleUrl: './project.component.css',
})
export class ProjectComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private projectService = inject(ProjectService);
  private boardService = inject(BoardService);

  projectId = signal<string>('');
  project = signal<Project | null>(null);
  boards = signal<Board[]>([]);
  isLoading = signal<boolean>(true);
  error = signal<string | null>(null);
  showCreateBoardDrawer = signal<boolean>(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('projectId');
    if (id) {
      this.projectId.set(id);
      this.loadProjectData();
    }
  }

  private loadProjectData(): void {
    this.isLoading.set(true);
    this.error.set(null);

    // First, load project info
    this.projectService.getProject(this.projectId()).subscribe({
      next: (project) => {
        this.project.set(project);
        // Then load boards for this project
        this.loadBoards();
      },
      error: (err) => {
        console.error('Error loading project:', err);
        this.error.set('Failed to load project');
        this.isLoading.set(false);
      }
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
    this.showCreateBoardDrawer.set(true);
  }

  closeCreateBoardDrawer(): void {
    this.showCreateBoardDrawer.set(false);
  }

  onBoardCreated(): void {
    this.closeCreateBoardDrawer();
    this.loadBoards();
  }
}
