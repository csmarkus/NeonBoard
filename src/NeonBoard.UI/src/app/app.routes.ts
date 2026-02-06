import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'projects', pathMatch: 'full' },
  {
    path: 'projects',
    loadComponent: () => import('./pages/projects/projects.component').then(m => m.ProjectsComponent),
    data: { animation: 'ProjectsPage' }
  },
  {
    path: 'board/:projectId',
    loadComponent: () => import('./pages/board/board.component').then(m => m.BoardComponent),
    data: { animation: 'BoardPage' }
  },
];
