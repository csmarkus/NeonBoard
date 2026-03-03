import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CardDrawerEventsService {
  private cardUpdatedSubject = new Subject<void>();
  private cardDeletedSubject = new Subject<void>();
  private cardArchivedSubject = new Subject<void>();

  cardUpdated$ = this.cardUpdatedSubject.asObservable();
  cardDeleted$ = this.cardDeletedSubject.asObservable();
  cardArchived$ = this.cardArchivedSubject.asObservable();

  notifyCardUpdated(): void {
    this.cardUpdatedSubject.next();
  }

  notifyCardDeleted(): void {
    this.cardDeletedSubject.next();
  }

  notifyCardArchived(): void {
    this.cardArchivedSubject.next();
  }
}
