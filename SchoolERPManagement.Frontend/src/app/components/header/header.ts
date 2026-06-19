import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ThemeService } from '../../services/theme';
import { LayoutService } from '../../services/layout.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  themeService = inject(ThemeService);
  layoutService = inject(LayoutService);
  
  constructor(private router: Router) {}

  get userName() {
    return sessionStorage.getItem('name') || 'Admin';
  }

  logout() {
    sessionStorage.clear();
    this.router.navigate(['/login']);
  }
}
