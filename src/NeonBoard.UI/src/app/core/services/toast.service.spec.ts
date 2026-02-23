import { initTestEnvironment } from '../../../test-setup';
import { TestBed } from '@angular/core/testing';
import { ToastService } from './toast.service';

initTestEnvironment();

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    vi.useFakeTimers();
    TestBed.configureTestingModule({});
    service = TestBed.inject(ToastService);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should start with no toasts', () => {
    expect(service.toasts()).toEqual([]);
  });

  it('should add a success toast', () => {
    service.success('Saved');
    expect(service.toasts().length).toBe(1);
    expect(service.toasts()[0].type).toBe('success');
    expect(service.toasts()[0].message).toBe('Saved');
  });

  it('should add an error toast', () => {
    service.error('Failed');
    expect(service.toasts().length).toBe(1);
    expect(service.toasts()[0].type).toBe('error');
    expect(service.toasts()[0].message).toBe('Failed');
  });

  it('should auto-remove toast after 3000ms', () => {
    service.success('Temporary');
    expect(service.toasts().length).toBe(1);
    vi.advanceTimersByTime(3000);
    expect(service.toasts().length).toBe(0);
  });

  it('should cap at 3 toasts, removing oldest', () => {
    service.success('First');
    service.success('Second');
    service.success('Third');
    service.success('Fourth');
    expect(service.toasts().length).toBe(3);
    expect(service.toasts()[0].message).toBe('Second');
    expect(service.toasts()[2].message).toBe('Fourth');
  });

  it('should assign unique ids to each toast', () => {
    service.success('A');
    service.success('B');
    const ids = service.toasts().map(t => t.id);
    expect(ids[0]).not.toBe(ids[1]);
  });

  it('should remove a specific toast by id', () => {
    service.success('A');
    service.success('B');
    const idToRemove = service.toasts()[0].id;
    service.remove(idToRemove);
    expect(service.toasts().length).toBe(1);
    expect(service.toasts()[0].message).toBe('B');
  });
});
