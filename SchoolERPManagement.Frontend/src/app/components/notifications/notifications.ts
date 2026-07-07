import { Component, OnInit, OnDestroy, inject, signal, effect, computed } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { NotificationService, UserNotificationResponseDTO } from '../../services/notification.service';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './notifications.html',
  styleUrls: ['./notifications.css']
})
export class NotificationsComponent implements OnInit, OnDestroy {
  private notificationService = inject(NotificationService);

  notifications = signal<UserNotificationResponseDTO[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  
  // Filter and expansion state
  activeFilter = signal<'all' | 'unread' | 'read'>('all');
  expandedNotifications = signal<Set<number>>(new Set());
  
  userId = Number(sessionStorage.getItem('userId'));

  unreadCount = computed(() => this.notifications().filter(n => !n.isRead).length);
  readCount = computed(() => this.notifications().filter(n => n.isRead).length);

  filteredNotifications = computed(() => {
    const list = this.notifications();
    const filter = this.activeFilter();
    if (filter === 'unread') return list.filter(n => !n.isRead);
    if (filter === 'read') return list.filter(n => n.isRead);
    return list;
  });

  constructor() {
    // Listen for real-time notifications
    effect(() => {
      const newNotif = this.notificationService.latestNotification();
      if (newNotif) {
        // Prepend the new notification
        this.notifications.update(list => {
          // Prevent duplicates if signal effect fires multiple times
          if (list.find(n => n.notificationId === newNotif.notificationId)) {
            return list;
          }
          return [newNotif, ...list];
        });
      }
    });
  }

  ngOnInit(): void {
    if (this.userId) {
      this.loadNotifications();
    } else {
      this.error.set('Could not determine user ID.');
      this.loading.set(false);
    }
  }

  ngOnDestroy(): void {
  }

  loadNotifications(): void {
    this.loading.set(true);
    this.notificationService.getUserNotifications(this.userId!).subscribe({
      next: (data) => {
        this.notifications.set(data);
        // Calculate initial unread count
        const unread = data.filter(n => !n.isRead).length;
        this.notificationService.unreadCount.set(unread);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error fetching notifications:', err);
        this.error.set('Failed to load notifications.');
        this.loading.set(false);
      }
    });
  }

  toggleExpand(notifId: number, notification: UserNotificationResponseDTO): void {
    this.expandedNotifications.update(set => {
      const newSet = new Set(set);
      if (newSet.has(notifId)) {
        newSet.delete(notifId);
      } else {
        newSet.add(notifId);
      }
      return newSet;
    });

    if (!notification.isRead) {
      this.markAsRead(notification);
    }
  }

  isExpanded(notifId: number): boolean {
    return this.expandedNotifications().has(notifId);
  }

  getNotificationConfig(title: string): { icon: string, colorClass: string } {
    const t = title.toLowerCase();
    if (t.includes('homework') || t.includes('assignment')) {
      return { icon: 'bi-journal-code', colorClass: 'homework-notif' };
    }
    if (t.includes('mark') || t.includes('result') || t.includes('grade') || t.includes('score')) {
      return { icon: 'bi-award', colorClass: 'result-notif' };
    }
    if (t.includes('fee') || t.includes('payment') || t.includes('due')) {
      return { icon: 'bi-credit-card', colorClass: 'fee-notif' };
    }
    if (t.includes('absent') || t.includes('attendance')) {
      return { icon: 'bi-person-x', colorClass: 'attendance-notif' };
    }
    if (t.includes('document') || t.includes('verify') || t.includes('approved') || t.includes('rejected')) {
      return { icon: 'bi-file-earmark-check', colorClass: 'document-notif' };
    }
    return { icon: 'bi-bell', colorClass: 'default-notif' };
  }

  markAsRead(notification: UserNotificationResponseDTO): void {
    if (notification.isRead || !this.userId) return;

    this.notificationService.markAsRead(this.userId, notification.notificationId).subscribe({
      next: () => {
        this.notifications.update(list => 
          list.map(n => n.notificationId === notification.notificationId ? { ...n, isRead: true } : n)
        );
        this.notificationService.unreadCount.update(c => Math.max(0, c - 1));
      },
      error: (err) => console.error('Failed to mark as read:', err)
    });
  }

  markAllAsRead(): void {
    if (!this.userId) return;
    
    this.notificationService.markAllAsRead(this.userId).subscribe({
      next: () => {
        this.notifications.update(list => list.map(n => ({ ...n, isRead: true })));
        this.notificationService.unreadCount.set(0);
      },
      error: (err) => console.error('Failed to mark all as read:', err)
    });
  }
}
