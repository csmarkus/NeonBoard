import { Component, inject, signal, OnInit, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../../../../layout/sidebar/sidebar.component';
import { ArchivePanelComponent } from '../../components/board/archive-panel/archive-panel.component';
import { ActivityPanelComponent } from '../../components/board/activity-panel/activity-panel.component';
import { ProjectService } from '../../../projects/services/project.service';

@Component({
  selector: 'app-project-layout',
  imports: [RouterOutlet, SidebarComponent, ArchivePanelComponent, ActivityPanelComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block h-screen'
  },
  template: `
    <div class="h-full bg-void flex">
      <app-sidebar [projectId]="projectId()" [shortId]="shortId()"></app-sidebar>
      <div class="flex-1 flex flex-col min-w-0">
        <router-outlet></router-outlet>
      </div>
    </div>

    <app-archive-panel />
    <app-activity-panel />
  `
})
export class ProjectLayoutComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private projectService = inject(ProjectService);

  shortId = signal<string>('');
  projectId = signal<string>('');
  private destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('shortId');
    if (id) {
      this.shortId.set(id);
      this.projectService.getProjectByShortId(id).pipe(
        takeUntilDestroyed(this.destroyRef)
      ).subscribe({
        next: (project) => this.projectId.set(project.id),
      });
    }
  }
}
