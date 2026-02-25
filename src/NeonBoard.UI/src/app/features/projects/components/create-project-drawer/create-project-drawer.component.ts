import { Component, input, output, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { ErrorBannerComponent } from '../../../../shared/components/error-banner/error-banner.component';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { ProjectService } from '../../services/project.service';
import { Project } from '../../models/project.model';

@Component({
  selector: 'app-create-project-drawer',
  imports: [FormField, DrawerComponent, ButtonComponent, ErrorBannerComponent, InputComponent],
  templateUrl: './create-project-drawer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateProjectDrawerComponent {
  open = input(false);
  close = output<void>();
  projectCreated = output<Project>();

  private projectService = inject(ProjectService);

  formModel = signal({ name: '', description: '' });
  projectForm = form(this.formModel, (f) => {
    required(f.name, { message: 'Project name is required' });
  });

  error = signal<string | null>(null);
  isCreating = signal(false);

  onClose(): void {
    this.resetForm();
    this.close.emit();
  }

  createProject(): void {
    if (this.projectForm().invalid()) return;

    this.isCreating.set(true);
    this.error.set(null);

    const { name, description } = this.formModel();
    this.projectService.createProject({
      name: name.trim(),
      description
    }).subscribe({
      next: (project) => {
        this.projectCreated.emit(project);
        this.resetForm();
        this.isCreating.set(false);
        this.close.emit();
      },
      error: (err) => {
        console.error('Error creating project:', err);
        this.error.set('Failed to create project. Please try again.');
        this.isCreating.set(false);
      }
    });
  }

  private resetForm(): void {
    this.formModel.set({ name: '', description: '' });
    this.error.set(null);
  }
}
