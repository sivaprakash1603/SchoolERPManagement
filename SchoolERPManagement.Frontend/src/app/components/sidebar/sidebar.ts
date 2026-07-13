import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LayoutService } from '../../services/layout.service';
import { NotificationService } from '../../services/notification.service';

interface MenuItem {
  label: string;
  route: string;
  icon: string;
  roles: string[]; // which roles can see this item
  category: string;
}

interface MenuCategory {
  category: string;
  items: MenuItem[];
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
  notificationService = inject(NotificationService);

  private allMenuItems: MenuItem[] = [
    { label: 'Dashboard',         route: '/dashboard',         icon: 'bi-grid-fill',              roles: ['Admin', 'Teacher', 'Student', 'Parent'], category: 'Core' },
    { label: 'Academic Calendar', route: '/academic-calendar', icon: 'bi-calendar3',              roles: ['Admin', 'Teacher', 'Student', 'Parent'], category: 'Core' },
    { label: 'PT Meetings',       route: '/parent-teacher-meetings', icon: 'bi-people',            roles: ['Admin', 'Teacher', 'Parent'], category: 'Core' },
    { label: 'Classes',           route: '/classes',           icon: 'bi-building',               roles: ['Admin'], category: 'Academics' },
    { label: 'Subjects',          route: '/subjects',          icon: 'bi-book-half',              roles: ['Admin'], category: 'Academics' },
    { label: 'Timetable',         route: '/timetable',         icon: 'bi-clock-fill',             roles: ['Admin', 'Teacher', 'Student', 'Parent'], category: 'Academics' },
    { label: 'Exams',             route: '/exams',             icon: 'bi-file-earmark-text-fill', roles: ['Admin', 'Teacher', 'Student', 'Parent'], category: 'Academics' },
    { label: 'Homework',          route: '/homework',          icon: 'bi-journal-text',           roles: ['Admin', 'Teacher', 'Student', 'Parent'], category: 'Academics' },
    { label: 'Students',          route: '/students',          icon: 'bi-people-fill',            roles: ['Admin', 'Teacher'], category: 'People' },
    { label: 'Teachers',          route: '/teachers',          icon: 'bi-person-badge-fill',      roles: ['Admin'], category: 'People' },
    { label: 'Parents',           route: '/parents',           icon: 'bi-person-hearts',          roles: ['Admin'], category: 'People' },
    { label: 'Attendance',        route: '/attendance',        icon: 'bi-calendar-check-fill',    roles: ['Admin', 'Teacher', 'Student', 'Parent'], category: 'Operations' },
    { label: 'Fees',              route: '/fees',              icon: 'bi-cash-coin',              roles: ['Admin', 'Teacher', 'Student', 'Parent'], category: 'Operations' },
    { label: 'Assets',            route: '/assets',            icon: 'bi-pc-display',             roles: ['Admin', 'Teacher', 'Student'], category: 'Operations' },
    { label: 'Documents',         route: '/documents',         icon: 'bi-folder-fill',            roles: ['Admin', 'Teacher', 'Student', 'Parent'], category: 'Operations' },
    { label: 'Academic Sessions', route: '/academic-sessions', icon: 'bi-calendar-range',         roles: ['Admin'], category: 'Operations' }
  ];

  groupedMenu = signal<MenuCategory[]>([]);

  ngOnInit() {
    const role = sessionStorage.getItem('role') || 'Student';
    const allowedItems = this.allMenuItems.filter(item => item.roles.includes(role));
    
    const groups: { [key: string]: MenuItem[] } = {};
    for (const item of allowedItems) {
      if (!groups[item.category]) {
        groups[item.category] = [];
      }
      groups[item.category].push(item);
    }
    
    const categoriesOrder = ['Core', 'Academics', 'People', 'Operations'];
    const finalGroups: MenuCategory[] = [];
    for (const cat of categoriesOrder) {
      if (groups[cat] && groups[cat].length > 0) {
        finalGroups.push({ category: cat, items: groups[cat] });
      }
    }
    this.groupedMenu.set(finalGroups);
  }

  onMenuClick() {
    // Automatically close sidebar on mobile when navigating
    if (window.innerWidth < 768) {
      this.layoutService.closeSidebar();
    }
  }
}

