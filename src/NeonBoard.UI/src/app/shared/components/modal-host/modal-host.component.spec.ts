import { initTestEnvironment } from '../../../../test-setup';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { signal, computed } from '@angular/core';
import { ModalHostComponent } from './modal-host.component';
import { ModalService, ConfirmationModalConfig } from '../../../core/services/modal.service';

initTestEnvironment();

describe('ModalHostComponent', () => {
  let fixture: ComponentFixture<ModalHostComponent>;
  let mockModalService: {
    config: ReturnType<typeof signal<ConfirmationModalConfig | null>>;
    isOpen: ReturnType<typeof computed>;
    resolve: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    const configSignal = signal<ConfirmationModalConfig | null>(null);
    mockModalService = {
      config: configSignal,
      isOpen: computed(() => configSignal() !== null),
      resolve: vi.fn(),
    };

    TestBed.configureTestingModule({
      imports: [ModalHostComponent],
      providers: [
        { provide: ModalService, useValue: mockModalService },
      ],
    });

    fixture = TestBed.createComponent(ModalHostComponent);
    fixture.detectChanges();
  });

  it('should not render modal when config is null', () => {
    const modal = fixture.nativeElement.querySelector('app-confirmation-modal');
    expect(modal).toBeNull();
  });

  it('should render modal when config is set', () => {
    mockModalService.config.set({
      title: 'Delete?',
      message: 'Are you sure?',
      confirmText: 'Yes',
      cancelText: 'No',
      variant: 'danger',
      gradientVariant: 'pink',
    });
    fixture.detectChanges();

    const modal = fixture.nativeElement.querySelector('app-confirmation-modal');
    expect(modal).not.toBeNull();
  });
});
