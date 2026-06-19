import { Component, OnInit, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable, Subscription } from 'rxjs';
import { NotificationSignalrService } from '../../../core/services/notification-signalr.service';
import { AuthService } from '../../../core/services/auth.service';
import { Notification } from '../../../core/models/notification.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-bell.component.html',
  styleUrls: ['./notification-bell.component.scss'],
  changeDetection: ChangeDetectionStrategy.Default,
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  notifications$: Observable<Notification[]>;
  unreadCount$: Observable<number>;
  panelOpen = false;
  private subs: Subscription[] = [];

  constructor(
    private signalrService: NotificationSignalrService,
    public authService: AuthService,
    private http: HttpClient,
  ) {
    this.notifications$ = this.signalrService.notifications$;
    this.unreadCount$ = this.signalrService.unreadCount$;
  }

  ngOnInit(): void {
    this.http.get<Notification[]>(`${environment.apiUrl}/notifications`).subscribe((data) => {
      this.signalrService.setInitialNotifications(data);
    });

    this.signalrService.startConnection();
  }

  togglePanel(): void {
    this.panelOpen = !this.panelOpen;
  }

  markAllRead(): void {
    this.http.patch(`${environment.apiUrl}/notifications/read-all`, {}).subscribe(() => {
      const current = this.signalrService.getCurrentNotifications();
      const updated = current.map((n) => ({ ...n, status: 'Read' as const }));
      this.signalrService.setInitialNotifications(updated);
    });
  }

  logout(): void {
    this.signalrService.stopConnection();
    this.authService.logout();
  }

  ngOnDestroy(): void {
    this.subs.forEach((s) => s.unsubscribe());
    this.signalrService.stopConnection();
  }
}
