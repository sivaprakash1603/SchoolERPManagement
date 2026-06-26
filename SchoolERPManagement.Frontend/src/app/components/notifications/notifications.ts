import { Component, OnInit, OnDestroy, inject, signal, effect } from '@angular/core';
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
  
  userId = Number(sessionStorage.getItem('userId'));

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
    // We do NOT disconnect SignalR here because the service maintains the connection 
    // across the whole application lifecycle (for the sidebar badge).
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
