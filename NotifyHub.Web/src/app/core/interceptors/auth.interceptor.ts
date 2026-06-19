import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();
  const tenantId = authService.getTenantId();

  if (!token) return next(req);

  const cloned = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
      'X-Tenant-Id': tenantId ?? '',
    },
  });

  return next(cloned);
};
