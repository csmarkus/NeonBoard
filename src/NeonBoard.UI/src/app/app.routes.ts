import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'projects', pathMatch: 'full' },
  {
    path: 'projects',
    loadComponent: () => import('./pages/projects/projects.component').then(m => m.ProjectsComponent),
    canActivate: [authGuard],
    data: { animation: 'ProjectsPage' }
  },
  {
    path: 'board/:projectId',
    loadComponent: () => import('./pages/board/board.component').then(m => m.BoardComponent),
    canActivate: [authGuard],
    data: { animation: 'BoardPage' }
  },
];
