import { Component, signal } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
  FormsModule,
} from '@angular/forms';
import { Auth } from '../../services/auth';
import { Router } from '@angular/router';

import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  loginForm: FormGroup;
  progress = signal(false);
  errorMessage = signal<string | null>(null);
  showPassword = signal(false);

  isForgotPassword = signal(false);
  forgotPasswordEmail = signal('');

  constructor(
    private fb: FormBuilder,
    private authService: Auth,
    private router: Router,
    private toastService: ToastService,
  ) {
    this.loginForm = this.fb.group({
      username: [
        '',
        [
          Validators.required,
          Validators.pattern(/^(admin|ST\d{3}[a-zA-Z0-9]+|T\d{3}\d{4}|P\d{7,15})$/),
        ],
      ],
      password: ['', [Validators.required]],
    });
  }

  togglePasswordVisibility() {
    this.showPassword.update((v) => !v);
  }

  handleLoginClick() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.progress.set(true);
    this.errorMessage.set(null);

    const loginData = {
      username: this.loginForm.value.username,
      password: this.loginForm.value.password,
    };

    this.authService.loginApiCall(loginData).subscribe({
      next: (response) => {
        sessionStorage.setItem('token', response.accessToken);
        
        // Decode JWT payload to get user data securely
        try {
          const base64Url = response.accessToken.split('.')[1];
          const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
          const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
              return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
          }).join(''));
          const payload = JSON.parse(jsonPayload);

          sessionStorage.setItem('role', payload.role);
          sessionStorage.setItem('userId', payload.sub.toString());
          sessionStorage.setItem('username', payload.unique_name);
        } catch (e) {
          console.error('Failed to parse JWT', e);
          this.errorMessage.set('Invalid login token received.');
          this.progress.set(false);
          return;
        }

        this.progress.set(false);
        // Navigate based on role (default to dashboard for now)
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.progress.set(false);
        const serverMessage = err.error?.message || err.error?.Message;
        if (serverMessage) {
          this.errorMessage.set(serverMessage);
        } else if (err.status === 401) {
          this.errorMessage.set('Invalid username or password.');
        } else {
          this.errorMessage.set('An error occurred. Please try again later.');
        }
      },
    });
  }

  handleForgotPasswordClick() {
    if (!this.forgotPasswordEmail()) {
      this.errorMessage.set('Please enter your email address.');
      return;
    }

    this.progress.set(true);
    this.errorMessage.set(null);

    this.authService.forgotPassword(this.forgotPasswordEmail()).subscribe({
      next: () => {
        this.progress.set(false);
        this.toastService.success('Reset link sent to your email.');
        this.isForgotPassword.set(false);
      },
      error: (err) => {
        this.progress.set(false);
        this.errorMessage.set(err.error?.message || 'Failed to send reset link.');
      },
    });
  }
}
