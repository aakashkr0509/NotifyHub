import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { NotificationBellComponent } from './features/notifications/notification-bell/notification-bell.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'notifications', component: NotificationBellComponent, canActivate: [authGuard] },
];
