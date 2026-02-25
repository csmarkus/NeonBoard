import { initTestEnvironment } from '../../../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { signal } from '@angular/core';
import { of } from 'rxjs';
import { CardDrawerComponent } from './card-drawer.component';
import { CardService } from '../../../services/card.service';
import { DrawerService } from '../../../services/drawer.service';
import { ModalService } from '../../../../../core/services/modal.service';
import { Card } from '../../../models/card.model';

initTestEnvironment();

const baseCard: Card = {
  id: 'card-1',
  cardNumber: 1,
  displayId: 'TST-1',
  title: 'Original Title',
  description: 'Original Desc',
  columnId: 'col-1',
  position: 0,
  labels: [],
  createdAt: '',
  updatedAt: '',
};

describe('CardDrawerComponent', () => {
  let fixture: ComponentFixture<CardDrawerComponent>;
  let component: CardDrawerComponent;
  let mockCardService: {
    updateCard: ReturnType<typeof vi.fn>;
    addCard: ReturnType<typeof vi.fn>;
    deleteCard: ReturnType<typeof vi.fn>;
    addCardLabel: ReturnType<typeof vi.fn>;
    removeCardLabel: ReturnType<typeof vi.fn>;
  };
  let mockDrawerService: {
    boardLabels: ReturnType<typeof signal>;
  };

  beforeEach(() => {
    mockCardService = {
      updateCard: vi.fn().mockReturnValue(of(undefined)),
      addCard: vi.fn().mockReturnValue(of({ id: 'card-new', cardNumber: 2, displayId: 'TST-2', title: 'New', description: '', columnId: 'col-1', position: 0, labels: [], createdAt: '', updatedAt: '' })),
      deleteCard: vi.fn().mockReturnValue(of(undefined)),
      addCardLabel: vi.fn().mockReturnValue(of(undefined)),
      removeCardLabel: vi.fn().mockReturnValue(of(undefined)),
    };
    mockDrawerService = {
      boardLabels: signal([]),
    };

    TestBed.configureTestingModule({
      imports: [CardDrawerComponent],
      providers: [
        { provide: CardService, useValue: mockCardService },
        { provide: DrawerService, useValue: mockDrawerService },
        { provide: ModalService, useValue: { confirm: vi.fn().mockResolvedValue(true) } },
      ],
    });
    TestBed.overrideTemplate(CardDrawerComponent, '');

    fixture = TestBed.createComponent(CardDrawerComponent);
    component = fixture.componentInstance;

    fixture.componentRef.setInput('open', false);
    fixture.componentRef.setInput('projectId', 'p-1');
    fixture.componentRef.setInput('boardId', 'b-1');
    fixture.componentRef.setInput('card', null);
  });

  describe('edit mode', () => {
    it('isEditMode is true when card input is a Card object', () => {
      fixture.componentRef.setInput('card', baseCard);
      TestBed.flushEffects();

      expect(component.isEditMode()).toBe(true);
    });

    it('isEditMode is false when card input is null', () => {
      fixture.componentRef.setInput('card', null);
      TestBed.flushEffects();

      expect(component.isEditMode()).toBe(false);
    });

    it('effect populates cardModel from card input', () => {
      fixture.componentRef.setInput('card', baseCard);
      TestBed.flushEffects();

      expect(component.cardModel().title).toBe('Original Title');
      expect(component.cardModel().description).toBe('Original Desc');
    });
  });

  describe('saveTitle', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('card', baseCard);
      TestBed.flushEffects();
    });

    it('calls cardService.updateCard when title has changed and is non-empty', () => {
      component.cardModel.set({ title: 'Updated Title', description: 'Original Desc' });

      component.saveTitle();

      expect(mockCardService.updateCard).toHaveBeenCalledWith(
        'p-1', 'b-1', 'card-1',
        { title: 'Updated Title', description: 'Original Desc' },
      );
    });

    it('does NOT call cardService.updateCard when title is unchanged', () => {
      component.saveTitle();

      expect(mockCardService.updateCard).not.toHaveBeenCalled();
    });

    it('does NOT call cardService.updateCard when title is blank', () => {
      component.cardModel.set({ title: '   ', description: 'Original Desc' });

      component.saveTitle();

      expect(mockCardService.updateCard).not.toHaveBeenCalled();
    });
  });

  describe('saveDescription', () => {
    it('calls cardService.updateCard with current description value', () => {
      fixture.componentRef.setInput('card', baseCard);
      TestBed.flushEffects();
      component.cardModel.set({ title: 'Original Title', description: 'New Description' });

      component.saveDescription();

      expect(mockCardService.updateCard).toHaveBeenCalledWith(
        'p-1', 'b-1', 'card-1',
        { title: 'Original Title', description: 'New Description' },
      );
    });
  });

  describe('label management', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('card', baseCard);
      TestBed.flushEffects();
    });

    it('toggleLabel calls cardService.addCardLabel when label is not assigned', () => {
      component.toggleLabel('label-1');

      expect(mockCardService.addCardLabel).toHaveBeenCalledWith('p-1', 'b-1', 'card-1', 'label-1');
    });

    it('toggleLabel calls cardService.removeCardLabel when label is already assigned', () => {
      const cardWithLabel: Card = { ...baseCard, labels: [{ id: 'label-1', name: 'Bug', color: 'red' }] };
      fixture.componentRef.setInput('card', cardWithLabel);
      TestBed.flushEffects();

      component.toggleLabel('label-1');

      expect(mockCardService.removeCardLabel).toHaveBeenCalledWith('p-1', 'b-1', 'card-1', 'label-1');
    });
  });

  describe('add mode', () => {
    it('addCard calls cardService.addCard and emits cardSaved output', () => {
      fixture.componentRef.setInput('card', null);
      fixture.componentRef.setInput('columnId', 'col-1');
      TestBed.flushEffects();
      component.cardModel.set({ title: 'My New Card', description: '' });

      let cardSavedEmitted = false;
      fixture.componentInstance.cardSaved.subscribe(() => (cardSavedEmitted = true));

      component.addCard();

      expect(mockCardService.addCard).toHaveBeenCalledWith('p-1', 'b-1', {
        columnId: 'col-1',
        title: 'My New Card',
        description: '',
      });
      expect(cardSavedEmitted).toBe(true);
    });
  });

  describe('deleteCard', () => {
    it('calls cardService.deleteCard and emits cardDeleted output', async () => {
      fixture.componentRef.setInput('card', baseCard);
      TestBed.flushEffects();

      let cardDeletedEmitted = false;
      fixture.componentInstance.cardDeleted.subscribe(() => (cardDeletedEmitted = true));

      await component.requestDeleteCard();

      expect(mockCardService.deleteCard).toHaveBeenCalledWith('p-1', 'b-1', 'card-1');
      expect(cardDeletedEmitted).toBe(true);
    });
  });
});
