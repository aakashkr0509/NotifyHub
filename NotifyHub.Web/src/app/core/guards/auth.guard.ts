import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = () => {
  const authsService = inject(AuthService);
  const router = inject(Router);

  if (authsService.isLoggedIn()) {
    return true;
  }

  router.navigate(['/login']);
  return false;
};
