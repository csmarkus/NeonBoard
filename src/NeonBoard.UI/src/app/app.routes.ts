import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'projects', pathMatch: 'full' },
  {
    path: 'projects',
    loadComponent: () => import('./features/projects/pages/projects-list/projects.component').then(m => m.ProjectsComponent),
    canActivate: [authGuard],
    data: { animation: 'ProjectsPage' }
  },
  {
    path: 'project/:projectId',
    loadComponent: () => import('./features/project-detail/pages/project-detail/project.component').then(m => m.ProjectComponent),
    canActivate: [authGuard],
    data: { animation: 'ProjectPage' }
  },
  {
    path: 'project/:projectId/b/:boardId',
    loadComponent: () => import('./features/project-detail/pages/board-view/board-view.component').then(m => m.BoardViewComponent),
    canActivate: [authGuard],
    data: { animation: 'BoardPage' }
  },
];
