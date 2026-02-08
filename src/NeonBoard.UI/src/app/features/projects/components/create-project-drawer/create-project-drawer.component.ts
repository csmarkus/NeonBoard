import { Component, Input, Output, EventEmitter, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { ProjectService } from '../../services/project.service';
import { Project } from '../../models/project.model';

@Component({
  selector: 'app-create-project-drawer',
  standalone: true,
  imports: [CommonModule, FormsModule, DrawerComponent, ButtonComponent],
  templateUrl: './create-project-drawer.component.html',
})
export class CreateProjectDrawerComponent {
  @Input() open = false;
  @Output() close = new EventEmitter<void>();
  @Output() projectCreated = new EventEmitter<Project>();

  private projectService = inject(ProjectService);
  private cdr = inject(ChangeDetectorRef);

  newProjectName = '';
  newProjectDescription = '';
  error: string | null = null;
  isCreating = false;

  onClose(): void {
    this.resetForm();
    this.close.emit();
  }

  createProject(): void {
    if (!this.newProjectName.trim()) return;

    this.isCreating = true;
    this.error = null;

    this.projectService.createProject({
      name: this.newProjectName,
      description: this.newProjectDescription || ''
    }).subscribe({
      next: (project) => {
        this.projectCreated.emit(project);
        this.resetForm();
        this.isCreating = false;
        this.close.emit();
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error creating project:', err);
        this.error = 'Failed to create project. Please try again.';
        this.isCreating = false;
        this.cdr.detectChanges();
      }
    });
  }

  private resetForm(): void {
    this.newProjectName = '';
    this.newProjectDescription = '';
    this.error = null;
  }
}
