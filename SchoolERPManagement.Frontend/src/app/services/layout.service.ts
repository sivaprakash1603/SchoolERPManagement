import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LayoutService {
  isSidebarOpen = signal(false);

  toggleSidebar() {
    this.isSidebarOpen.update(val => !val);
  }

  closeSidebar() {
    this.isSidebarOpen.set(false);
  }

  openSidebar() {
    this.isSidebarOpen.set(true);
  }
}
