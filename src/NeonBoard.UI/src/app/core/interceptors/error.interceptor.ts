import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toastService = inject(ToastService);

  return next(req).pipe(
    catchError((error) => {
      const status = error.status;

      if (status === 0) {
        toastService.error('Unable to connect to the server. Check your internet connection.');
      } else if (status >= 500) {
        toastService.error('An unexpected error occurred. Please try again.');
      }
      // 4xx errors are NOT toasted here — handled by individual facades/components.

      return throwError(() => error);
    })
  );
};
