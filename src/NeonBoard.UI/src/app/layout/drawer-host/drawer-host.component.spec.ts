import { initTestEnvironment } from '../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { signal, computed } from '@angular/core';
import { DrawerHostComponent } from './drawer-host.component';
import { DrawerService } from '../../core/services/drawer.service';
import { CardDrawerEventsService } from '../../features/project-detail/services/card-drawer-events.service';
import { CreateBoardDrawerEventsService } from '../../features/project-detail/services/create-board-drawer-events.service';
import { DrawerConfig } from '../../core/models/drawer.model';

initTestEnvironment();

describe('DrawerHostComponent', () => {
  let fixture: ComponentFixture<DrawerHostComponent>;
  let mockDrawerService: {
    config: ReturnType<typeof signal<DrawerConfig | null>>;
    isOpen: ReturnType<typeof computed>;
    close: ReturnType<typeof vi.fn>;
  };
  let mockCardEvents: {
    notifyCardUpdated: ReturnType<typeof vi.fn>;
    notifyCardDeleted: ReturnType<typeof vi.fn>;
  };
  let mockBoardEvents: {
    notifyBoardCreated: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    const configSignal = signal<DrawerConfig | null>(null);
    mockDrawerService = {
      config: configSignal,
      isOpen: computed(() => configSignal() !== null),
      close: vi.fn(),
    };
    mockCardEvents = {
      notifyCardUpdated: vi.fn(),
      notifyCardDeleted: vi.fn(),
    };
    mockBoardEvents = {
      notifyBoardCreated: vi.fn(),
    };

    TestBed.configureTestingModule({
      imports: [DrawerHostComponent],
      providers: [
        { provide: DrawerService, useValue: mockDrawerService },
        { provide: CardDrawerEventsService, useValue: mockCardEvents },
        { provide: CreateBoardDrawerEventsService, useValue: mockBoardEvents },
      ],
    });
    TestBed.overrideTemplate(DrawerHostComponent, '');

    fixture = TestBed.createComponent(DrawerHostComponent);
    fixture.detectChanges();
  });

  it('onCardSaved notifies card updated', () => {
    fixture.componentInstance.onCardSaved();
    expect(mockCardEvents.notifyCardUpdated).toHaveBeenCalled();
  });

  it('onCardDeleted closes drawer and notifies card deleted', () => {
    fixture.componentInstance.onCardDeleted();
    expect(mockDrawerService.close).toHaveBeenCalled();
    expect(mockCardEvents.notifyCardDeleted).toHaveBeenCalled();
  });

  it('onBoardCreated closes drawer and notifies board created', () => {
    fixture.componentInstance.onBoardCreated();
    expect(mockDrawerService.close).toHaveBeenCalled();
    expect(mockBoardEvents.notifyBoardCreated).toHaveBeenCalled();
  });
});
