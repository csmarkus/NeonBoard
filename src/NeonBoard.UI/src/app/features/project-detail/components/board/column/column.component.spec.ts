import { initTestEnvironment } from '../../../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { CdkDragStart } from '@angular/cdk/drag-drop';
import { ColumnComponent } from './column.component';
import { Column } from '../../../models/column.model';
import { Card } from '../../../models/card.model';

initTestEnvironment();

const mockColumn: Column = { id: 'col-1', name: 'To Do', position: 0, boardId: 'b-1' };

const mockCards: Card[] = [
  { id: 'card-1', cardNumber: 1, displayId: 'TST-1', title: 'Card One', description: '', columnId: 'col-1', position: 0, labels: [], createdAt: '', updatedAt: '' },
  { id: 'card-2', cardNumber: 2, displayId: 'TST-2', title: 'Card Two', description: '', columnId: 'col-1', position: 1, labels: [], createdAt: '', updatedAt: '' },
];

describe('ColumnComponent', () => {
  let fixture: ComponentFixture<ColumnComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ColumnComponent],
      providers: [provideNoopAnimations()],
    });
    fixture = TestBed.createComponent(ColumnComponent);
    fixture.componentRef.setInput('column', mockColumn);
    fixture.componentRef.setInput('cards', mockCards);
    fixture.componentRef.setInput('columnIds', ['col-1']);
    fixture.componentRef.setInput('accentClass', 'bg-cyan-400');
    fixture.detectChanges();
  });

  it('renders the column name', () => {
    const heading = fixture.nativeElement.querySelector('h2');
    expect(heading.textContent.trim()).toBe('To Do');
  });

  it('renders the card count', () => {
    const countSpan = fixture.nativeElement.querySelector('span.tabular-nums');
    expect(countSpan.textContent.trim()).toBe('2');
  });

  it('renders the add card button', () => {
    const buttons: HTMLButtonElement[] = Array.from(fixture.nativeElement.querySelectorAll('button'));
    const addCardBtn = buttons.find(b => b.textContent?.includes('Add card'));
    expect(addCardBtn).toBeTruthy();
  });

  it('onCardDragStarted sets placeholder height from pre-captured pointerdown height', () => {
    const mockPlaceholder = document.createElement('div');

    // Simulate pointerdown capturing the card height
    const cardWrapper = document.createElement('div');
    cardWrapper.setAttribute('cdkDrag', '');
    Object.defineProperty(cardWrapper, 'offsetHeight', { value: 120 });
    document.body.appendChild(cardWrapper);

    const pointerEvent = new PointerEvent('pointerdown', { bubbles: true });
    Object.defineProperty(pointerEvent, 'target', { value: cardWrapper });
    fixture.componentInstance.onCardPointerDown(pointerEvent);

    document.body.removeChild(cardWrapper);

    // Simulate drag start using the pre-captured height
    const mockSource = {
      element: { nativeElement: document.createElement('div') },
      getPlaceholderElement: () => mockPlaceholder,
    };
    const dragEvent = { source: mockSource } as unknown as CdkDragStart;
    fixture.componentInstance.onCardDragStarted(dragEvent);

    expect(mockPlaceholder.style.height).toBe('120px');
  });
});
