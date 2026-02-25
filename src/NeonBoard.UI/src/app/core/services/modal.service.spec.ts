import { initTestEnvironment } from '../../../test-setup';
import { TestBed } from '@angular/core/testing';
import { ModalService } from './modal.service';

initTestEnvironment();

describe('ModalService', () => {
  let service: ModalService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ModalService);
  });

  it('should start with no config', () => {
    expect(service.config()).toBeNull();
    expect(service.isOpen()).toBe(false);
  });

  it('confirm() sets config and returns a promise', () => {
    const promise = service.confirm({ message: 'Delete this?' });

    expect(service.config()).not.toBeNull();
    expect(service.config()!.message).toBe('Delete this?');
    expect(service.isOpen()).toBe(true);
    expect(promise).toBeInstanceOf(Promise);
  });

  it('confirm() applies default values', () => {
    service.confirm({ message: 'Sure?' });

    const config = service.config()!;
    expect(config.title).toBe('Confirm Action');
    expect(config.confirmText).toBe('Confirm');
    expect(config.cancelText).toBe('Cancel');
    expect(config.variant).toBe('danger');
    expect(config.gradientVariant).toBe('pink');
  });

  it('resolve(true) resolves promise with true and clears config', async () => {
    const promise = service.confirm({ message: 'Delete?' });

    service.resolve(true);

    const result = await promise;
    expect(result).toBe(true);
    expect(service.config()).toBeNull();
    expect(service.isOpen()).toBe(false);
  });

  it('resolve(false) resolves promise with false and clears config', async () => {
    const promise = service.confirm({ message: 'Delete?' });

    service.resolve(false);

    const result = await promise;
    expect(result).toBe(false);
    expect(service.config()).toBeNull();
  });

  it('calling confirm() while open resolves previous with false', async () => {
    const first = service.confirm({ message: 'First' });
    const second = service.confirm({ message: 'Second' });

    const firstResult = await first;
    expect(firstResult).toBe(false);
    expect(service.config()!.message).toBe('Second');

    service.resolve(true);
    const secondResult = await second;
    expect(secondResult).toBe(true);
  });
});
