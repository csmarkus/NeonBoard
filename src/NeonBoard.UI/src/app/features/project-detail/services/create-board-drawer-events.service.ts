import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CreateBoardDrawerEventsService {
  private boardCreatedSubject = new Subject<void>();

  boardCreated$ = this.boardCreatedSubject.asObservable();

  notifyBoardCreated(): void {
    this.boardCreatedSubject.next();
  }
}
