import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ThemeService } from '../../services/theme';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  themeService = inject(ThemeService);
  
  constructor(private router: Router) {}

  get role() {
    return sessionStorage.getItem('role') || 'User';
  }

  logout() {
    sessionStorage.clear();
    this.router.navigate(['/login']);
  }
}
