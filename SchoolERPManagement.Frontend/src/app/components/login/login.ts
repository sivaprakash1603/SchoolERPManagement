import { Component, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Auth } from '../../services/auth';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  loginForm: FormGroup;
  progress = signal(false);
  errorMessage = signal<string | null>(null);
  showPassword = signal(false);

  constructor(
    private fb: FormBuilder,
    private authService: Auth,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(4)]],
      password: ['', [Validators.required]]
    });
  }

  togglePasswordVisibility() {
    this.showPassword.update(v => !v);
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
      password: this.loginForm.value.password
    };

    this.authService.loginApiCall(loginData).subscribe({
      next: (response) => {
        sessionStorage.setItem('token', response.accessToken);
        sessionStorage.setItem('role', response.roleName);
        this.progress.set(false);
        // Navigate based on role (default to dashboard for now)
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.progress.set(false);
        if (err.status === 401) {
            this.errorMessage.set('Invalid username or password.');
        } else {
            this.errorMessage.set('An error occurred. Please try again later.');
        }
      }
    });
  }
}
