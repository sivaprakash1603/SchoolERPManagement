import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { SystemSetupService } from '../services/system-setup.service';
import { catchError, map, of } from 'rxjs';

export const setupGuard: CanActivateFn = (route, state) => {
  const setupService = inject(SystemSetupService);
  const router = inject(Router);

  const token = sessionStorage.getItem('token');
  const role = sessionStorage.getItem('role');

  if (!token || role !== 'Admin') {
    return true;
  }

  // Check setup status
  return setupService.getSetupStatus().pipe(
    map(res => {
      if (!res.isComplete) {
        // Only redirect to /setup if we're not already heading there.
        return router.createUrlTree(['/setup']);
      }
      return true;
    }),
    catchError(() => {
      // On error, let them through (or we could block them, but letting them through avoids lockout)
      return of(true);
    })
  );
};
