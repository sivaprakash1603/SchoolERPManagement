import { Component, signal, OnInit, OnDestroy, AfterViewInit, ElementRef, inject } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
  FormsModule,
} from '@angular/forms';
import { Auth } from '../../services/auth';
import { Router, RouterLink } from '@angular/router';
import { ThemeService } from '../../services/theme';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements OnInit, OnDestroy, AfterViewInit {
  loginForm: FormGroup;
  progress = signal(false);
  errorMessage = signal<string | null>(null);
  showPassword = signal(false);

  isForgotPassword = signal(false);
  forgotPasswordEmail = signal('');

  themeService = inject(ThemeService);
  private el = inject(ElementRef);

  // ─── Quote Carousel ───
  activeQuoteIndex = signal(0);
  private quoteInterval: any;

  quotes = [
    { text: 'Education is the most powerful weapon which you can use to change the world.', author: 'Nelson Mandela' },
    { text: 'The beautiful thing about learning is that nobody can take it away from you.', author: 'B.B. King' },
    { text: 'An investment in knowledge pays the best interest.', author: 'Benjamin Franklin' },
    { text: 'The mind is not a vessel to be filled, but a fire to be kindled.', author: 'Plutarch' },
  ];

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

  ngOnInit() {
    this.quoteInterval = setInterval(() => {
      this.activeQuoteIndex.update(i => (i + 1) % this.quotes.length);
    }, 5000);
  }

  ngAfterViewInit() {
    const observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          entry.target.classList.add('revealed');
          observer.unobserve(entry.target);
        }
      });
    }, { threshold: 0.1 });

    const revealItems = this.el.nativeElement.querySelectorAll('.reveal-item');
    revealItems.forEach((item: HTMLElement) => observer.observe(item));
  }

  ngOnDestroy() {
    if (this.quoteInterval) {
      clearInterval(this.quoteInterval);
    }
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
