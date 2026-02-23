import { Injectable, inject, signal, computed } from '@angular/core';
import { Observable, EMPTY } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { ProjectService } from '../../projects/services/project.service';
import { ToastService } from '../../../core/services/toast.service';
import { Project } from '../../projects/models/project.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectSettingsFacade {
  private projectService = inject(ProjectService);
  private toastService = inject(ToastService);

  private _projectName = signal<string>('');
  private _originalProjectName = signal<string>('');
  private _projectDescription = signal<string>('');
  private _originalProjectDescription = signal<string>('');
  private _isLoading = signal<boolean>(false);
  private _isSaving = signal<boolean>(false);
  private _error = signal<string | null>(null);

  readonly projectName = this._projectName.asReadonly();
  readonly originalProjectName = this._originalProjectName.asReadonly();
  readonly projectDescription = this._projectDescription.asReadonly();
  readonly originalProjectDescription = this._originalProjectDescription.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly isSaving = this._isSaving.asReadonly();
  readonly error = this._error.asReadonly();

  readonly hasChanges = computed(() => {
    return this._projectName().trim() !== this._originalProjectName() ||
      this._projectDescription().trim() !== this._originalProjectDescription();
  });

  loadProjectSettings(projectId: string): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.projectService.getProject(projectId).subscribe({
      next: (project) => {
        this._projectName.set(project.name);
        this._originalProjectName.set(project.name);
        this._projectDescription.set(project.description);
        this._originalProjectDescription.set(project.description);
        this._isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading project settings:', err);
        this._error.set('Failed to load project settings');
        this._isLoading.set(false);
      }
    });
  }

  updateProjectName(name: string): void {
    this._projectName.set(name);
  }

  updateProjectDescription(description: string): void {
    this._projectDescription.set(description);
  }

  saveProjectSettings(projectId: string): Observable<Project> {
    const name = this._projectName().trim();
    const description = this._projectDescription().trim();
    if (!name || !this.hasChanges() || this._isSaving()) return EMPTY;

    this._isSaving.set(true);

    return this.projectService.updateProject(projectId, { name, description }).pipe(
      tap((project) => {
        this._originalProjectName.set(project.name);
        this._projectName.set(project.name);
        this._originalProjectDescription.set(project.description);
        this._projectDescription.set(project.description);
        this._isSaving.set(false);
        this.toastService.success('Project settings saved');
      }),
      catchError((err) => {
        console.error('Error saving project settings:', err);
        this._isSaving.set(false);
        this.toastService.error('Failed to save project settings');
        return EMPTY;
      })
    );
  }

  deleteProject(projectId: string): Observable<void> {
    return this.projectService.deleteProject(projectId);
  }
}
