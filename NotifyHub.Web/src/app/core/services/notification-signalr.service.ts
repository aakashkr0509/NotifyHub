import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from './auth.service';
import { Notification } from '../models/notification.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class NotificationSignalrService {
  private connection!: signalR.HubConnection;

  private _notifications$ = new BehaviorSubject<Notification[]>([]);
  public notifications$ = this._notifications$.asObservable();

  private _unreadCount$ = new BehaviorSubject<number>(0);
  public unreadCount$ = this._unreadCount$.asObservable();

  constructor(private authService: AuthService) {}

  startConnection(): void {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl, {
        accessTokenFactory: () => this.authService.getToken() ?? '',
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.registerHandlers();
    this.connect();
  }

  private registerHandlers(): void {
    this.connection.on('ReceiveNotification', (notification: Notification) => {
      const current = this._notifications$.getValue();
      this._notifications$.next([notification, ...current]);
      this.updateUnreadCount();
    });
    this.connection.onreconnected(() => {
      this.joinTenantGroup();
    });
  }

  private connect(): void {
    this.connection
      .start()
      .then(() => this.joinTenantGroup())
      .catch((err) => console.error('SignalR error:', err));
  }

  private joinTenantGroup(): void {
    this.connection
      .invoke('JoinTenantGroup')
      .catch((err) => console.error('JoinTenantGroup error:', err));
  }

  setInitialNotifications(notifiations: Notification[]): void {
    this._notifications$.next(notifiations);
    this.updateUnreadCount();
  }

  private updateUnreadCount(): void {
    const count = this._notifications$.getValue().filter((n) => n.status === 'Unread').length;
    this._unreadCount$.next(count);
  }

  stopConnection(): void {
    this.connection?.stop();
  }

  getCurrentNotifications(): Notification[] {
    return this._notifications$.getValue();
  }
}
