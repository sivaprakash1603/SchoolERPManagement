import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LayoutService } from '../../services/layout.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  layoutService = inject(LayoutService);

  menuItems = [
    { label: 'Dashboard', route: '/dashboard', icon: 'bi-grid-fill' },
    { label: 'Students', route: '/students', icon: 'bi-people-fill' },
    { label: 'Teachers', route: '/teachers', icon: 'bi-person-badge-fill' },
    { label: 'Parents', route: '/parents', icon: 'bi-person-hearts' },
    { label: 'Classes', route: '/classes', icon: 'bi-building' },
    { label: 'Subjects', route: '/subjects', icon: 'bi-book-half' },
    { label: 'Attendance', route: '/attendance', icon: 'bi-calendar-check-fill' },
    { label: 'Timetable', route: '/timetable', icon: 'bi-clock-fill' },
    { label: 'Homework', route: '/homework', icon: 'bi-journal-text' },
    { label: 'Exams', route: '/exams', icon: 'bi-file-earmark-text-fill' },
    { label: 'Fees', route: '/fees', icon: 'bi-cash-coin' },
    { label: 'Assets', route: '/assets', icon: 'bi-pc-display' },
    { label: 'Documents', route: '/documents', icon: 'bi-folder-fill' },
    { label: 'Academic Sessions', route: '/academic-sessions', icon: 'bi-calendar-range' }
  ];

  onMenuClick() {
    // Automatically close sidebar on mobile when navigating
    if (window.innerWidth < 768) {
      this.layoutService.closeSidebar();
    }
  }
}
