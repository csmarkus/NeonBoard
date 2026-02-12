import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { unsavedChangesGuard } from './core/guards/unsaved-changes.guard';

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
    loadComponent: () => import('./features/project-detail/pages/project-layout/project-layout.component').then(m => m.ProjectLayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./features/project-detail/pages/project-detail/project.component').then(m => m.ProjectComponent),
        data: { animation: 'ProjectPage' }
      },
      {
        path: 'b/:boardId',
        loadComponent: () => import('./features/project-detail/pages/board-view/board-view.component').then(m => m.BoardViewComponent),
        data: { animation: 'BoardPage' }
      },
      {
        path: 'b/:boardId/settings',
        loadComponent: () => import('./features/project-detail/pages/board-settings/board-settings.component').then(m => m.BoardSettingsComponent),
        canDeactivate: [unsavedChangesGuard],
        data: { animation: 'BoardSettingsPage' }
      }
    ]
  },
];
