import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  email = '';
  password = '';
  subdomain = '';
  error = '';
  loading = false;

  constructor(
    private authService: AuthService,
    private router: Router,
  ) {
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/notifications']);
    }
  }

  onSubmit(): void {
    this.loading = true;
    this.error = '';

    this.authService
      .login({
        email: this.email,
        password: this.password,
        subdomain: this.subdomain,
      })
      .subscribe({
        next: () => {
          this.router.navigate(['/notifications']);
        },
        error: () => {
          this.error = 'Invalid credentials. Please try again.';
          this.loading = false;
        },
      });
  }
}
