import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LayoutService } from '../../services/layout.service';

interface MenuItem {
  label: string;
  route: string;
  icon: string;
  roles: string[]; // which roles can see this item
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar implements OnInit {
  layoutService = inject(LayoutService);

  private allMenuItems: MenuItem[] = [
    { label: 'Dashboard',         route: '/dashboard',         icon: 'bi-grid-fill',              roles: ['Admin', 'Teacher', 'Student', 'Parent'] },
    { label: 'Students',          route: '/students',          icon: 'bi-people-fill',            roles: ['Admin', 'Teacher'] },
    { label: 'Teachers',          route: '/teachers',          icon: 'bi-person-badge-fill',      roles: ['Admin'] },
    { label: 'Parents',           route: '/parents',           icon: 'bi-person-hearts',          roles: ['Admin'] },
    { label: 'Classes',           route: '/classes',           icon: 'bi-building',               roles: ['Admin', 'Teacher'] },
    { label: 'Subjects',          route: '/subjects',          icon: 'bi-book-half',              roles: ['Admin', 'Teacher'] },
    { label: 'Attendance',        route: '/attendance',        icon: 'bi-calendar-check-fill',    roles: ['Admin', 'Teacher', 'Student', 'Parent'] },
    { label: 'Timetable',         route: '/timetable',         icon: 'bi-clock-fill',             roles: ['Admin', 'Teacher', 'Student', 'Parent'] },
    { label: 'Homework',          route: '/homework',          icon: 'bi-journal-text',           roles: ['Admin', 'Teacher', 'Student', 'Parent'] },
    { label: 'Exams',             route: '/exams',             icon: 'bi-file-earmark-text-fill', roles: ['Admin', 'Teacher', 'Student', 'Parent'] },
    { label: 'Fees',              route: '/fees',              icon: 'bi-cash-coin',              roles: ['Admin', 'Teacher', 'Student', 'Parent'] },
    { label: 'Assets',            route: '/assets',            icon: 'bi-pc-display',             roles: ['Admin', 'Teacher', 'Student'] },
    { label: 'Documents',         route: '/documents',         icon: 'bi-folder-fill',            roles: ['Admin', 'Teacher', 'Student', 'Parent'] },
    { label: 'Academic Calendar', route: '/academic-calendar', icon: 'bi-calendar3',              roles: ['Admin', 'Teacher', 'Student', 'Parent'] },
    { label: 'Academic Sessions', route: '/academic-sessions', icon: 'bi-calendar-range',         roles: ['Admin'] }
  ];

  menuItems = signal<MenuItem[]>([]);

  ngOnInit() {
    const role = sessionStorage.getItem('role') || 'Student';
    this.menuItems.set(this.allMenuItems.filter(item => item.roles.includes(role)));
  }

  onMenuClick() {
    // Automatically close sidebar on mobile when navigating
    if (window.innerWidth < 768) {
      this.layoutService.closeSidebar();
    }
  }
}

