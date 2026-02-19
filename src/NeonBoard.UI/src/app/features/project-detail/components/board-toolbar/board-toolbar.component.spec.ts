import { initTestEnvironment } from '../../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { BoardToolbarComponent } from './board-toolbar.component';

initTestEnvironment();

describe('BoardToolbarComponent', () => {
  let fixture: ComponentFixture<BoardToolbarComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [BoardToolbarComponent],
      providers: [provideRouter([])],
    });
    fixture = TestBed.createComponent(BoardToolbarComponent);
    fixture.componentRef.setInput('projectId', 'p-1');
    fixture.componentRef.setInput('boardId', 'b-1');
    fixture.detectChanges();
  });

  it('renders the settings link with correct aria-label', () => {
    const settingsLink = fixture.nativeElement.querySelector('[aria-label="Board settings"]');
    expect(settingsLink).toBeTruthy();
  });

  it('renders the filter section', () => {
    const filterButton = fixture.nativeElement.querySelector('button');
    expect(filterButton?.textContent?.trim()).toBe('All cards');
  });
});
