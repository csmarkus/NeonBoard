import { Component, input, inject, signal, computed, OnInit, ChangeDetectionStrategy, DestroyRef, ChangeDetectorRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ButtonComponent } from '../../../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../../../shared/components/badge/badge.component';
import { SettingsSectionComponent } from '../../../../../shared/components/settings-section/settings-section.component';
import { ErrorBannerComponent } from '../../../../../shared/components/error-banner/error-banner.component';
import { MemberService } from '../../../../../core/services/member.service';
import { InvitationService } from '../../../../../core/services/invitation.service';
import { ModalService } from '../../../../../core/services/modal.service';
import { ToastService } from '../../../../../core/services/toast.service';
import { DrawerService } from '../../../services/drawer.service';
import { ProjectMember, ProjectRole } from '../../../models/member.model';
import { ProjectInvitation } from '../../../models/invitation.model';
import { RelativeTimePipe } from '../../../../../shared/pipes/relative-time.pipe';

@Component({
  selector: 'app-members-section',
  imports: [
    ButtonComponent,
    BadgeComponent,
    SettingsSectionComponent,
    ErrorBannerComponent,
    RelativeTimePipe,
  ],
  templateUrl: './members-section.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MembersSectionComponent implements OnInit {
  private memberService = inject(MemberService);
  private invitationService = inject(InvitationService);
  private modalService = inject(ModalService);
  private toastService = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);
  protected drawerService = inject(DrawerService);
  private destroyRef = inject(DestroyRef);

  projectId = input.required<string>();
  currentUserRole = input<ProjectRole>();

  members = signal<ProjectMember[]>([]);
  invitations = signal<ProjectInvitation[]>([]);
  isLoading = signal(true);
  error = signal<string | null>(null);

  isOwner = computed(() => this.currentUserRole() === 'Owner');

  pendingInvitations = computed(() =>
    this.invitations().filter(inv => inv.status === 'Pending')
  );

  ngOnInit(): void {
    this.loadMembers();
    if (this.currentUserRole() === 'Owner') {
      this.loadInvitations();
    }

    this.drawerService.memberInvited$.pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      this.onInvitationSent();
    });
  }

  loadMembers(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.memberService.getMembers(this.projectId()).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (members) => {
        this.members.set(members);
        this.isLoading.set(false);
        this.cdr.markForCheck();
      },
      error: () => {
        this.error.set('Failed to load members.');
        this.isLoading.set(false);
        this.cdr.markForCheck();
      }
    });
  }

  loadInvitations(): void {
    this.invitationService.getInvitations(this.projectId()).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (invitations) => {
        this.invitations.set(invitations);
        this.cdr.markForCheck();
      },
      error: () => {
        // Silently fail for invitations loading
        this.cdr.markForCheck();
      }
    });
  }

  openInviteDrawer(): void {
    this.drawerService.openInviteMemberDrawer(this.projectId());
  }

  getInitial(name: string): string {
    return name.charAt(0).toUpperCase();
  }

  getRoleBadgeVariant(role: ProjectRole): 'default' | 'cyan' | 'amber' | 'violet' | 'green' {
    switch (role) {
      case 'Owner': return 'violet';
      case 'Editor': return 'cyan';
      case 'Viewer': return 'default';
    }
  }

  async onRoleChange(member: ProjectMember, newRole: ProjectRole): Promise<void> {
    if (member.role === newRole) return;

    this.memberService.updateMemberRole(this.projectId(), member.userId, newRole).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.members.update(members =>
          members.map(m => m.userId === member.userId ? { ...m, role: newRole } : m)
        );
        this.toastService.success(`Updated ${member.displayName}'s role to ${newRole}.`);
        this.cdr.markForCheck();
      },
      error: () => {
        this.toastService.error(`Failed to update role for ${member.displayName}.`);
        this.cdr.markForCheck();
      }
    });
  }

  async onRemoveMember(member: ProjectMember): Promise<void> {
    const confirmed = await this.modalService.confirm({
      title: 'Remove Member',
      message: `Are you sure you want to remove ${member.displayName} from this project? They will lose access to all boards and cards.`,
      confirmText: 'Remove',
    });

    if (!confirmed) return;

    this.memberService.removeMember(this.projectId(), member.userId).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.members.update(members => members.filter(m => m.userId !== member.userId));
        this.toastService.success(`${member.displayName} has been removed.`);
        this.cdr.markForCheck();
      },
      error: () => {
        this.toastService.error(`Failed to remove ${member.displayName}.`);
        this.cdr.markForCheck();
      }
    });
  }

  async onRevokeInvitation(invitation: ProjectInvitation): Promise<void> {
    const confirmed = await this.modalService.confirm({
      title: 'Revoke Invitation',
      message: `Are you sure you want to revoke the invitation to ${invitation.email}?`,
      confirmText: 'Revoke',
    });

    if (!confirmed) return;

    this.invitationService.revokeInvitation(this.projectId(), invitation.id).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.invitations.update(invitations => invitations.filter(i => i.id !== invitation.id));
        this.toastService.success(`Invitation to ${invitation.email} has been revoked.`);
        this.cdr.markForCheck();
      },
      error: () => {
        this.toastService.error(`Failed to revoke invitation to ${invitation.email}.`);
        this.cdr.markForCheck();
      }
    });
  }

  onInvitationSent(): void {
    this.loadInvitations();
    this.loadMembers();
  }
}
