import { Injectable, signal, effect } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  isDarkMode = signal<boolean>(true); // Default to luxury dark mode

  constructor() {
    const saved = localStorage.getItem('app-theme');
    if (saved) {
      this.isDarkMode.set(saved === 'dark');
    }
    
    // Apply theme to the entire document body whenever the signal changes
    effect(() => {
      const theme = this.isDarkMode() ? 'dark' : 'light';
      document.body.setAttribute('data-bs-theme', theme);
      localStorage.setItem('app-theme', theme);
    });
  }

  toggleTheme() {
    this.isDarkMode.update(v => !v);
  }
}
