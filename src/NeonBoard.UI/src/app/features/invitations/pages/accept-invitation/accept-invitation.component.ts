import { Component, inject, signal, OnInit, ChangeDetectionStrategy, DestroyRef, ChangeDetectorRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../../shared/components/badge/badge.component';
import { GradientAccentComponent } from '../../../../shared/components/gradient-accent/gradient-accent.component';
import { InvitationService } from '../../../../core/services/invitation.service';
import { InvitationDetails } from '../../../project-detail/models/invitation.model';
import { ProjectRole } from '../../../project-detail/models/member.model';

type PageState = 'loading' | 'ready' | 'accepting' | 'accepted' | 'expired' | 'error';

@Component({
  selector: 'app-accept-invitation',
  imports: [ButtonComponent, BadgeComponent, GradientAccentComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'flex items-center justify-center min-h-screen bg-void'
  },
  template: `
    <div class="w-full max-w-md mx-auto px-4">
      @switch (state()) {
        @case ('loading') {
          <div class="bg-surface border border-dim rounded-xl p-8 text-center">
            <div class="inline-block w-8 h-8 border-4 border-accent/20 border-t-accent rounded-full animate-spin mb-4"></div>
            <p class="text-sm text-muted">Loading invitation details...</p>
          </div>
        }

        @case ('ready') {
          @if (invitation()) {
            <div class="bg-surface border border-dim rounded-xl overflow-hidden">
              <app-gradient-accent variant="cyan" />
              <div class="p-8">
                <h1 class="text-xl font-semibold text-primary mb-2">You have been invited</h1>
                <p class="text-sm text-muted mb-6">
                  {{ invitation()!.inviterName }} has invited you to join a project.
                </p>

                <div class="space-y-4 mb-8">
                  <div class="flex items-center justify-between py-3 border-b border-subtle">
                    <span class="text-sm text-muted">Project</span>
                    <span class="text-sm font-medium text-primary">{{ invitation()!.projectName }}</span>
                  </div>
                  <div class="flex items-center justify-between py-3 border-b border-subtle">
                    <span class="text-sm text-muted">Invited by</span>
                    <span class="text-sm text-primary">{{ invitation()!.inviterName }}</span>
                  </div>
                  <div class="flex items-center justify-between py-3">
                    <span class="text-sm text-muted">Your role</span>
                    <app-badge [variant]="getRoleBadgeVariant(invitation()!.role)">
                      {{ invitation()!.role }}
                    </app-badge>
                  </div>
                </div>

                <div class="flex gap-3">
                  <app-button
                    variant="primary"
                    (click)="acceptInvitation()"
                    [disabled]="state() === 'accepting'">
                    {{ state() === 'accepting' ? 'Joining...' : 'Accept Invitation' }}
                  </app-button>
                  <app-button
                    variant="secondary"
                    (click)="decline()">
                    Decline
                  </app-button>
                </div>
              </div>
            </div>
          }
        }

        @case ('accepting') {
          @if (invitation()) {
            <div class="bg-surface border border-dim rounded-xl overflow-hidden">
              <app-gradient-accent variant="cyan" />
              <div class="p-8">
                <h1 class="text-xl font-semibold text-primary mb-2">You have been invited</h1>
                <p class="text-sm text-muted mb-6">
                  {{ invitation()!.inviterName }} has invited you to join a project.
                </p>

                <div class="space-y-4 mb-8">
                  <div class="flex items-center justify-between py-3 border-b border-subtle">
                    <span class="text-sm text-muted">Project</span>
                    <span class="text-sm font-medium text-primary">{{ invitation()!.projectName }}</span>
                  </div>
                  <div class="flex items-center justify-between py-3 border-b border-subtle">
                    <span class="text-sm text-muted">Invited by</span>
                    <span class="text-sm text-primary">{{ invitation()!.inviterName }}</span>
                  </div>
                  <div class="flex items-center justify-between py-3">
                    <span class="text-sm text-muted">Your role</span>
                    <app-badge [variant]="getRoleBadgeVariant(invitation()!.role)">
                      {{ invitation()!.role }}
                    </app-badge>
                  </div>
                </div>

                <div class="flex gap-3">
                  <app-button
                    variant="primary"
                    [disabled]="true">
                    Joining...
                  </app-button>
                  <app-button
                    variant="secondary"
                    [disabled]="true">
                    Decline
                  </app-button>
                </div>
              </div>
            </div>
          }
        }

        @case ('accepted') {
          <div class="bg-surface border border-dim rounded-xl overflow-hidden">
            <app-gradient-accent variant="cyan" />
            <div class="p-8 text-center">
              <div class="w-12 h-12 rounded-full bg-status-done/15 text-status-done flex items-center justify-center mx-auto mb-4 text-xl">
                &#10003;
              </div>
              <h1 class="text-xl font-semibold text-primary mb-2">You are in!</h1>
              <p class="text-sm text-muted mb-6">
                You have successfully joined the project. Redirecting...
              </p>
            </div>
          </div>
        }

        @case ('expired') {
          <div class="bg-surface border border-dim rounded-xl overflow-hidden">
            <app-gradient-accent variant="pink" />
            <div class="p-8 text-center">
              <h1 class="text-xl font-semibold text-primary mb-2">Invitation Expired</h1>
              <p class="text-sm text-muted mb-6">
                This invitation is no longer valid. It may have expired or been revoked. Please ask the project owner to send a new invitation.
              </p>
              <app-button variant="secondary" (click)="goToProjects()">
                Go to Projects
              </app-button>
            </div>
          </div>
        }

        @case ('error') {
          <div class="bg-surface border border-dim rounded-xl overflow-hidden">
            <app-gradient-accent variant="pink" />
            <div class="p-8 text-center">
              <h1 class="text-xl font-semibold text-primary mb-2">Something Went Wrong</h1>
              <p class="text-sm text-muted mb-6">
                {{ errorMessage() }}
              </p>
              <app-button variant="secondary" (click)="goToProjects()">
                Go to Projects
              </app-button>
            </div>
          </div>
        }
      }
    </div>
  `,
})
export class AcceptInvitationComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private invitationService = inject(InvitationService);
  private cdr = inject(ChangeDetectorRef);
  private destroyRef = inject(DestroyRef);

  state = signal<PageState>('loading');
  invitation = signal<InvitationDetails | null>(null);
  errorMessage = signal('Unable to load invitation. The link may be invalid.');

  private token = '';

  ngOnInit(): void {
    this.token = this.route.snapshot.paramMap.get('token') ?? '';

    if (!this.token) {
      this.state.set('error');
      this.errorMessage.set('Invalid invitation link.');
      return;
    }

    this.invitationService.getInvitationByToken(this.token).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (details) => {
        this.invitation.set(details);

        if (details.isExpired || details.status === 'Expired' || details.status === 'Revoked') {
          this.state.set('expired');
        } else if (details.status === 'Accepted') {
          this.state.set('accepted');
          setTimeout(() => this.goToProjects(), 2000);
        } else {
          this.state.set('ready');
        }
        this.cdr.markForCheck();
      },
      error: () => {
        this.state.set('error');
        this.errorMessage.set('Unable to load invitation. The link may be invalid or the invitation has been revoked.');
        this.cdr.markForCheck();
      }
    });
  }

  getRoleBadgeVariant(role: ProjectRole): 'default' | 'cyan' | 'amber' | 'violet' | 'green' {
    switch (role) {
      case 'Owner': return 'violet';
      case 'Editor': return 'cyan';
      case 'Viewer': return 'default';
    }
  }

  acceptInvitation(): void {
    this.state.set('accepting');

    this.invitationService.acceptInvitation(this.token).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.state.set('accepted');
        this.cdr.markForCheck();
        setTimeout(() => this.goToProjects(), 2000);
      },
      error: () => {
        this.state.set('error');
        this.errorMessage.set('Failed to accept invitation. Please try again.');
        this.cdr.markForCheck();
      }
    });
  }

  decline(): void {
    this.goToProjects();
  }

  goToProjects(): void {
    this.router.navigate(['/projects']);
  }
}
