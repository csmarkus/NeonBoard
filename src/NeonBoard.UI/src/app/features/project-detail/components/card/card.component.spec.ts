import { initTestEnvironment } from '../../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { CardComponent } from './card.component';
import { Card } from '../../models/card.model';

initTestEnvironment();

const baseCard: Card = {
  id: 'card-1',
  title: 'Test Card',
  description: '',
  columnId: 'col-1',
  position: 0,
  labels: [],
  createdAt: '',
  updatedAt: '',
};

describe('CardComponent', () => {
  let fixture: ComponentFixture<CardComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [CardComponent],
    });
    fixture = TestBed.createComponent(CardComponent);
    fixture.componentRef.setInput('card', baseCard);
    fixture.detectChanges();
  });

  it('renders the card title', () => {
    const title = fixture.nativeElement.querySelector('h3');
    expect(title.textContent.trim()).toBe('Test Card');
  });

  it('renders labels sorted alphabetically when present', () => {
    fixture.componentRef.setInput('card', {
      ...baseCard,
      labels: [
        { id: 'l-2', name: 'Feature', color: 'blue' },
        { id: 'l-1', name: 'Bug', color: 'red' },
      ],
    });
    fixture.detectChanges();

    const labelSpans = fixture.nativeElement.querySelectorAll('span[title]');
    const names = Array.from(labelSpans).map((el: any) => el.textContent.trim());
    expect(names).toEqual(['Bug', 'Feature']);
  });

  it('emits cardClick when the card is clicked', () => {
    let clicked = false;
    fixture.componentInstance.cardClick.subscribe(() => (clicked = true));

    fixture.nativeElement.querySelector('div').click();

    expect(clicked).toBe(true);
  });
});
