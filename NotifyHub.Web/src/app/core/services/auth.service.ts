import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../../environments/environment';
import {Observable, tap} from 'rxjs';
import { Router } from '@angular/router';
import {AuthResponse, LoginRequest} from '../models/notification.model';


@Injectable({ providedIn: 'root'})
export class AuthService {

    private readonly tokenKey = 'access_token';
    private readonly tenantKey = 'tenant_id';
    private readonly emailKey = 'email';

    constructor(private http: HttpClient, private router: Router) {}

    login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(
        `${environment.apiUrl}/auth/login`, request)
      .pipe(
        tap(response => {
          localStorage.setItem(
            this.tokenKey, response.accessToken);
          localStorage.setItem(
            this.tenantKey, response.tenantId);
          localStorage.setItem(
            this.emailKey, response.email);
        })
      );
    }

    logout(): void {
        localStorage.removeItem(this.tokenKey);
        localStorage.removeItem(this.tenantKey);
        localStorage.removeItem(this.emailKey);
        this.router.navigate(['/login']);
    }

    getToken(): string | null {
        return localStorage.getItem(this.tokenKey);
    }

    getTenantId(): string | null {
        return localStorage.getItem(this.tenantKey);
    }

    getEmail(): string | null {
        return localStorage.getItem(this.emailKey);
    }

    isLoggedIn(): boolean {
        return !!this.getToken();
    }
  }