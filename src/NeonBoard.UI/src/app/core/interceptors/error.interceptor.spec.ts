import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { initTestEnvironment } from '../../../test-setup';
import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { errorInterceptor } from './error.interceptor';
import { ToastService } from '../services/toast.service';

initTestEnvironment();

describe('errorInterceptor', () => {
  let httpClient: HttpClient;
  let httpTesting: HttpTestingController;
  let toastService: ToastService;

  beforeEach(() => {
    vi.useFakeTimers();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
    toastService = TestBed.inject(ToastService);
  });

  afterEach(() => {
    httpTesting.verify();
    vi.useRealTimers();
  });

  it('should pass through successful responses', () => {
    let result: unknown;
    httpClient.get('/api/test').subscribe(res => (result = res));

    httpTesting.expectOne('/api/test').flush({ data: 'ok' });
    expect(result).toEqual({ data: 'ok' });
  });

  it('should not toast on 400 errors', () => {
    const errorSpy = vi.spyOn(toastService, 'error');
    httpClient.get('/api/test').subscribe({ error: () => {} });

    httpTesting.expectOne('/api/test').flush(
      { status: 400, title: 'Validation Error' },
      { status: 400, statusText: 'Bad Request' }
    );

    expect(errorSpy).not.toHaveBeenCalled();
  });

  it('should toast on 500 errors with generic message', () => {
    const errorSpy = vi.spyOn(toastService, 'error');
    httpClient.get('/api/test').subscribe({ error: () => {} });

    httpTesting.expectOne('/api/test').flush(
      { status: 500, title: 'Internal Server Error' },
      { status: 500, statusText: 'Internal Server Error' }
    );

    expect(errorSpy).toHaveBeenCalledWith('An unexpected error occurred. Please try again.');
  });

  it('should toast on 0 status (network error)', () => {
    const errorSpy = vi.spyOn(toastService, 'error');
    httpClient.get('/api/test').subscribe({ error: () => {} });

    httpTesting.expectOne('/api/test').error(
      new ProgressEvent('error'),
      { status: 0, statusText: 'Unknown Error' }
    );

    expect(errorSpy).toHaveBeenCalledWith('Unable to connect to the server. Check your internet connection.');
  });

  it('should not toast on 401 errors', () => {
    const errorSpy = vi.spyOn(toastService, 'error');
    httpClient.get('/api/test').subscribe({ error: () => {} });

    httpTesting.expectOne('/api/test').flush(
      { status: 401, title: 'Unauthorized' },
      { status: 401, statusText: 'Unauthorized' }
    );

    expect(errorSpy).not.toHaveBeenCalled();
  });

  it('should re-throw the error so subscribers still receive it', () => {
    let caughtError: unknown;
    httpClient.get('/api/test').subscribe({
      error: (err) => (caughtError = err),
    });

    httpTesting.expectOne('/api/test').flush(
      { status: 500, title: 'Internal Server Error' },
      { status: 500, statusText: 'Internal Server Error' }
    );

    expect(caughtError).toBeDefined();
  });
});
