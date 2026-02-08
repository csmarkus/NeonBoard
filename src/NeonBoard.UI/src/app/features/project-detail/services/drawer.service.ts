import { Injectable, signal } from '@angular/core';
import { Subject } from 'rxjs';
import { Card } from '../models/card.model';

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

  // Events
  private cardUpdatedSubject = new Subject<void>();
  private cardDeletedSubject = new Subject<void>();
  private boardCreatedSubject = new Subject<void>();

  cardUpdated$ = this.cardUpdatedSubject.asObservable();
  cardDeleted$ = this.cardDeletedSubject.asObservable();
  boardCreated$ = this.boardCreatedSubject.asObservable();

  openCreateBoardDrawer(projectId: string): void {
    this.createBoardProjectId.set(projectId);
    this.showCreateBoardDrawer.set(true);
  }

  closeCreateBoardDrawer(): void {
    this.showCreateBoardDrawer.set(false);
    this.createBoardProjectId.set(null);
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
}
