import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NotificationsComponent } from './notifications';
import { UserNotificationResponseDTO } from '../../services/notification.service';
import { of } from 'rxjs';

describe('NotificationsComponent', () => {
  let component: NotificationsComponent;
  let fixture: ComponentFixture<NotificationsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotificationsComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(NotificationsComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle notification expansion', () => {
    const notif: UserNotificationResponseDTO = {
      id: 1,
      notificationId: 1,
      title: 'New Homework Posted',
      message: 'Math homework is due.',
      isRead: true,
      createdAt: new Date().toISOString()
    };

    expect(component.isExpanded(1)).toBe(false);
    component.toggleExpand(1, notif);
    expect(component.isExpanded(1)).toBe(true);
    component.toggleExpand(1, notif);
    expect(component.isExpanded(1)).toBe(false);
  });

  it('should return correct category config based on title', () => {
    const homeworkConfig = component.getNotificationConfig('New Homework Posted');
    expect(homeworkConfig.icon).toBe('bi-journal-code');
    expect(homeworkConfig.colorClass).toBe('homework-notif');

    const feeConfig = component.getNotificationConfig('Fee Payment Due');
    expect(feeConfig.icon).toBe('bi-credit-card');
    expect(feeConfig.colorClass).toBe('fee-notif');

    const resultConfig = component.getNotificationConfig('Subject Mark Published');
    expect(resultConfig.icon).toBe('bi-award');
    expect(resultConfig.colorClass).toBe('result-notif');
  });

  it('should filter notifications based on read/unread status', () => {
    const notifs: UserNotificationResponseDTO[] = [
      { id: 1, notificationId: 1, title: 'HW 1', message: 'M1', isRead: false, createdAt: new Date().toISOString() },
      { id: 2, notificationId: 2, title: 'Fee 1', message: 'M2', isRead: true, createdAt: new Date().toISOString() }
    ];

    component.notifications.set(notifs);

    // Default 'all'
    component.activeFilter.set('all');
    expect(component.filteredNotifications().length).toBe(2);

    // 'unread' filter
    component.activeFilter.set('unread');
    expect(component.filteredNotifications().length).toBe(1);
    expect(component.filteredNotifications()[0].notificationId).toBe(1);

    // 'read' filter
    component.activeFilter.set('read');
    expect(component.filteredNotifications().length).toBe(1);
    expect(component.filteredNotifications()[0].notificationId).toBe(2);
  });
});
