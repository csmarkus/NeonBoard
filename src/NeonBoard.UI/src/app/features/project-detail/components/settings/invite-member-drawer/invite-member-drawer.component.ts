import { Component, input, output, inject, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DrawerComponent } from '../../../../../shared/components/drawer/drawer.component';
import { ButtonComponent } from '../../../../../shared/components/button/button.component';
import { ErrorBannerComponent } from '../../../../../shared/components/error-banner/error-banner.component';
import { InputComponent } from '../../../../../shared/components/input/input.component';
import { MemberService } from '../../../../../core/services/member.service';
import { ToastService } from '../../../../../core/services/toast.service';
import { ProjectRole } from '../../../models/member.model';

@Component({
  selector: 'app-invite-member-drawer',
  imports: [FormsModule, DrawerComponent, ButtonComponent, ErrorBannerComponent, InputComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-drawer [open]="open()" (close)="onClose()">
      <div class="space-y-6">
        <div>
          <h2 id="drawer-title" class="text-lg font-semibold text-primary">Invite Member</h2>
          <p class="text-sm text-muted mt-1">Send an invitation to join this project</p>
        </div>

        <app-error-banner [message]="error" />

        <div class="space-y-4">
          <div>
            <label for="invite-email" class="block text-sm text-secondary mb-1.5">Email address</label>
            <app-input
              inputId="invite-email"
              [(ngModel)]="email"
              type="email"
              placeholder="colleague@example.com"
              (keyup.enter)="invite()" />
          </div>

          <div>
            <label for="invite-role" class="block text-sm text-secondary mb-1.5">Role</label>
            <select
              id="invite-role"
              [(ngModel)]="selectedRole"
              class="w-full px-3 py-2 text-sm bg-surface text-primary border border-dim rounded-lg hover:border-secondary/30 focus:outline-none focus:border-accent focus:ring-2 focus:ring-accent/20 transition-colors duration-150">
              <option value="Editor">Editor - Can create and edit boards and cards</option>
              <option value="Viewer">Viewer - Can view boards and cards</option>
            </select>
          </div>
        </div>

        <div class="flex gap-3 pt-4 border-t border-subtle">
          <app-button
            variant="primary"
            (click)="invite()"
            [disabled]="isInviting || !email.trim()">
            {{ isInviting ? 'Sending...' : 'Send Invitation' }}
          </app-button>
          <app-button
            variant="secondary"
            (click)="onClose()"
            [disabled]="isInviting">
            Cancel
          </app-button>
        </div>
      </div>
    </app-drawer>
  `,
})
export class InviteMemberDrawerComponent {
  open = input.required<boolean>();
  projectId = input.required<string>();
  close = output<void>();
  invited = output<void>();

  private memberService = inject(MemberService);
  private toastService = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  email = '';
  selectedRole: ProjectRole = 'Editor';
  error: string | null = null;
  isInviting = false;

  onClose(): void {
    this.resetForm();
    this.close.emit();
  }

  invite(): void {
    const trimmedEmail = this.email.trim();
    if (!trimmedEmail) return;

    this.isInviting = true;
    this.error = null;

    this.memberService.inviteMember(this.projectId(), trimmedEmail, this.selectedRole).subscribe({
      next: () => {
        this.toastService.success(`Invitation sent to ${trimmedEmail}.`);
        this.invited.emit();
        this.resetForm();
        this.isInviting = false;
        this.close.emit();
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = 'Failed to send invitation. Please check the email and try again.';
        this.isInviting = false;
        this.cdr.detectChanges();
      }
    });
  }

  private resetForm(): void {
    this.email = '';
    this.selectedRole = 'Editor';
    this.error = null;
  }
}
