import { initTestEnvironment } from '../../../test-setup';
import { TestBed } from '@angular/core/testing';
import { DrawerService } from './drawer.service';
import { DrawerConfig } from '../models/drawer.model';

initTestEnvironment();

describe('DrawerService', () => {
  let service: DrawerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DrawerService);
  });

  it('starts with null config and isOpen false', () => {
    expect(service.config()).toBeNull();
    expect(service.isOpen()).toBe(false);
  });

  it('open sets config and isOpen becomes true', () => {
    const config: DrawerConfig = { type: 'create-board', projectId: 'p-1' };
    service.open(config);

    expect(service.config()).toEqual(config);
    expect(service.isOpen()).toBe(true);
  });

  it('close resets config to null and isOpen becomes false', () => {
    service.open({ type: 'create-board', projectId: 'p-1' });
    service.close();

    expect(service.config()).toBeNull();
    expect(service.isOpen()).toBe(false);
  });

  it('opening a new drawer replaces the previous one', () => {
    service.open({ type: 'create-board', projectId: 'p-1' });
    const cardConfig: DrawerConfig = {
      type: 'card-detail',
      card: { id: 'c-1', cardNumber: 1, displayId: 'TST-1', title: 'Card', description: '', columnId: 'col-1', position: 0, labels: [], createdAt: '', updatedAt: '', archivedAt: null },
      projectId: 'p-1',
      boardId: 'b-1',
      boardLabels: [],
      initialActivity: null,
    };
    service.open(cardConfig);

    expect(service.config()).toEqual(cardConfig);
  });
});
