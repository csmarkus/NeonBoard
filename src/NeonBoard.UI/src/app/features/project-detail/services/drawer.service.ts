import { Injectable, signal } from '@angular/core';
import { Subject } from 'rxjs';
import { Card } from '../models/card.model';
import { ActivityFeed } from '../models/activity.model';
import { Label } from '../models/label.model';

@Injectable({
  providedIn: 'root'
})
export class DrawerService {
  // Create board drawer
  showCreateBoardDrawer = signal<boolean>(false);
  createBoardProjectId = signal<string | null>(null);

  // Card drawer
  selectedCard = signal<Card | null>(null);
  cardDrawerProjectId = signal<string>('');
  cardDrawerBoardId = signal<string>('');
  boardLabels = signal<Label[]>([]);
  initialCardActivity = signal<ActivityFeed | null>(null);

  // Invite member drawer
  showInviteMemberDrawer = signal<boolean>(false);
  inviteMemberProjectId = signal<string | null>(null);

  // Events
  private cardUpdatedSubject = new Subject<void>();
  private cardDeletedSubject = new Subject<void>();
  private boardCreatedSubject = new Subject<void>();
  private cardArchivedSubject = new Subject<void>();
  private memberInvitedSubject = new Subject<void>();

  cardUpdated$ = this.cardUpdatedSubject.asObservable();
  cardDeleted$ = this.cardDeletedSubject.asObservable();
  boardCreated$ = this.boardCreatedSubject.asObservable();
  cardArchived$ = this.cardArchivedSubject.asObservable();
  memberInvited$ = this.memberInvitedSubject.asObservable();

  openCreateBoardDrawer(projectId: string): void {
    this.createBoardProjectId.set(projectId);
    this.showCreateBoardDrawer.set(true);
  }

  closeCreateBoardDrawer(): void {
    this.showCreateBoardDrawer.set(false);
    this.createBoardProjectId.set(null);
  }

  setBoardLabels(labels: Label[]): void {
    this.boardLabels.set(labels);
  }

  openCardDrawer(card: Card, projectId: string, boardId: string): void {
    this.selectedCard.set(card);
    this.cardDrawerProjectId.set(projectId);
    this.cardDrawerBoardId.set(boardId);
  }

  closeCardDrawer(): void {
    this.selectedCard.set(null);
    this.cardDrawerProjectId.set('');
    this.cardDrawerBoardId.set('');
    this.initialCardActivity.set(null);
  }

  notifyCardUpdated(): void {
    this.cardUpdatedSubject.next();
  }

  notifyCardDeleted(): void {
    this.cardDeletedSubject.next();
  }

  notifyBoardCreated(): void {
    this.boardCreatedSubject.next();
  }

  notifyCardArchived(): void {
    this.cardArchivedSubject.next();
  }

  openInviteMemberDrawer(projectId: string): void {
    this.inviteMemberProjectId.set(projectId);
    this.showInviteMemberDrawer.set(true);
  }

  closeInviteMemberDrawer(): void {
    this.showInviteMemberDrawer.set(false);
    this.inviteMemberProjectId.set(null);
  }

  notifyMemberInvited(): void {
    this.memberInvitedSubject.next();
  }
}
