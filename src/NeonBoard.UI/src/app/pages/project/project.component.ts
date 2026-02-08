import { Component, inject, afterNextRender, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, NavigationStart } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { Task, TaskStatus, Column } from '../../models/task.model';
import { columns as defaultColumns, mockTasks } from '../../data/mock-tasks';
import { SidebarComponent } from '../../components/sidebar/sidebar.component';
import { CardComponent } from '../../components/card/card.component';
import { DrawerComponent } from '../../components/drawer/drawer.component';
import { TaskDetailComponent } from '../../components/task-detail/task-detail.component';
import { ButtonComponent } from '../../components/button/button.component';
import { InputComponent } from '../../components/input/input.component';
import { BadgeComponent } from '../../components/badge/badge.component';
import { LoadingService } from '../../services/loading.service';

@Component({
  selector: 'app-project',
  standalone: true,
  imports: [
    CommonModule,
    DragDropModule,
    SidebarComponent,
    CardComponent,
    DrawerComponent,
    TaskDetailComponent,
    ButtonComponent,
    InputComponent,
    BadgeComponent,
  ],
  host: {
    class: 'block h-screen'
  },
  templateUrl: './project.component.html',
  styleUrl: './project.component.css',
})
export class ProjectComponent implements OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private loadingService = inject(LoadingService);
  private navigationSubscription?: Subscription;

  projectId = '';
  projectName = 'Project Alpha';
  columns: Column[] = [...defaultColumns];
  selectedTask: Task | null = null;

  // Store tasks organized by status for drag and drop
  tasksByStatus: Record<TaskStatus, Task[]> = {
    'todo': [],
    'in-progress': [],
    'review': [],
    'done': []
  };

  // Column IDs for connecting drop lists
  get columnIds(): string[] {
    return this.columns.map(c => c.id);
  }

  private accentClasses: Record<string, string> = {
    cyan: 'bg-status-todo',
    amber: 'bg-status-progress',
    violet: 'bg-status-review',
    green: 'bg-status-done',
  };

  constructor() {
    this.projectId = this.route.snapshot.paramMap.get('projectId') || '';
    this.initializeTasks();

    // Hide loading after the component is fully rendered
    afterNextRender(() => {
      this.loadingService.hide();
    });

    // Close drawer when navigation starts
    this.navigationSubscription = this.router.events.pipe(
      filter(event => event instanceof NavigationStart)
    ).subscribe(() => {
      this.selectedTask = null;
    });
  }

  ngOnDestroy(): void {
    this.navigationSubscription?.unsubscribe();
  }

  private initializeTasks(): void {
    // Group tasks by status
    for (const task of mockTasks) {
      this.tasksByStatus[task.status].push({ ...task });
    }
  }

  getAccentClass(accent: string): string {
    return this.accentClasses[accent] || '';
  }

  getTotalTaskCount(): number {
    return Object.values(this.tasksByStatus).reduce((sum, tasks) => sum + tasks.length, 0);
  }

  dropColumn(event: CdkDragDrop<Column[]>): void {
    moveItemInArray(this.columns, event.previousIndex, event.currentIndex);
  }

  dropCard(event: CdkDragDrop<Task[]>, newStatus: TaskStatus): void {
    if (event.previousContainer === event.container) {
      // Reorder within the same column
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      // Move to a different column
      const task = event.previousContainer.data[event.previousIndex];
      task.status = newStatus;

      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    }
  }

  selectTask(task: Task): void {
    this.selectedTask = task;
  }
}
