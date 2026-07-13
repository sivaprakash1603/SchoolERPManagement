import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NotificationsComponent } from './notifications';
import { NotificationService, UserNotificationResponseDTO } from '../../services/notification.service';
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

  describe('HTML Template rendering', () => {
    let mockNotificationService: any;
    
    beforeEach(() => {
      mockNotificationService = TestBed.inject(NotificationService);
      vi.spyOn(mockNotificationService, 'getUserNotifications').mockReturnValue(of([]));
    });

    it('should cover empty state and error state', () => {
      // Mock loading false, empty list
      component.loading.set(false);
      component.error.set(null);
      component.notifications.set([]);
      fixture.detectChanges();
      
      let emptyState = fixture.nativeElement.querySelector('.empty-state');
      expect(emptyState).toBeTruthy();

      // Mock error state
      component.error.set('Failed to load');
      fixture.detectChanges();
      let errorAlert = fixture.nativeElement.querySelector('.alert-danger');
      expect(errorAlert).toBeTruthy();
    });
    
    it('should cover filter tabs and mark all read button click', () => {
      // Mock data
      component.loading.set(false);
      component.error.set(null);
      component.notifications.set([
        { id: 1, notificationId: 1, title: 'Notif 1', message: 'M1', isRead: false, createdAt: new Date().toISOString() },
      ]);
      fixture.detectChanges();
      
      const filterTabs = fixture.nativeElement.querySelectorAll('.filter-tab');
      filterTabs.forEach((tab: HTMLElement) => tab.click());
      
      const markAllBtn = fixture.nativeElement.querySelector('.btn-gradient-custom');
      vi.spyOn(component, 'markAllAsRead').mockImplementation(() => {});
      if (markAllBtn) markAllBtn.click();
      expect(component.markAllAsRead).toHaveBeenCalled();
    });

    it('should dispatch click on notification card', () => {
      component.loading.set(false);
      component.error.set(null);
      component.notifications.set([
        { id: 1, notificationId: 1, title: 'Notif 1', message: 'M1', isRead: false, createdAt: new Date().toISOString() },
      ]);
      fixture.detectChanges();
      
      const card = fixture.nativeElement.querySelector('.notification-card');
      expect(card).toBeTruthy(); // Debug: ensure card exists

      const expandSpy = vi.spyOn(component, 'toggleExpand').mockImplementation(() => {});
      card.click();
      expect(expandSpy).toHaveBeenCalled();
    });
  });

  describe('markAsRead and markAllAsRead', () => {
    let mockNotificationService: any;
    
    beforeEach(() => {
      mockNotificationService = TestBed.inject(NotificationService);
    });

    it('should call markAsRead API if notification is unread', () => {
      const notif: UserNotificationResponseDTO = {
        id: 1, notificationId: 1, title: 'HW 1', message: 'M1', isRead: false, createdAt: new Date().toISOString()
      };
      
      component.notifications.set([notif]);
      component.userId = 1;
      
      const markAsReadSpy = vi.spyOn(mockNotificationService, 'markAsRead').mockReturnValue(of({}));
      component.markAsRead(notif);
      
      expect(markAsReadSpy).toHaveBeenCalledWith(1, 1);
      expect(component.notifications()[0].isRead).toBe(true);
    });

    it('should not call markAsRead API if already read', () => {
      const notif: UserNotificationResponseDTO = {
        id: 1, notificationId: 1, title: 'HW 1', message: 'M1', isRead: true, createdAt: new Date().toISOString()
      };
      
      const markAsReadSpy = vi.spyOn(mockNotificationService, 'markAsRead');
      component.markAsRead(notif);
      
      expect(markAsReadSpy).not.toHaveBeenCalled();
    });

    it('should call markAllAsRead API', () => {
      component.userId = 1;
      component.notifications.set([
        { id: 1, notificationId: 1, title: 'HW 1', message: 'M1', isRead: false, createdAt: new Date().toISOString() }
      ]);
      
      const markAllAsReadSpy = vi.spyOn(mockNotificationService, 'markAllAsRead').mockReturnValue(of({}));
      component.markAllAsRead();
      
      expect(markAllAsReadSpy).toHaveBeenCalledWith(1);
      expect(component.notifications()[0].isRead).toBe(true);
    });
  });
  
  describe('loadNotifications and Error handling', () => {
    let mockNotificationService: any;
    
    beforeEach(() => {
      mockNotificationService = TestBed.inject(NotificationService);
    });

    it('should set error if loadNotifications fails', () => {
      vi.spyOn(mockNotificationService, 'getUserNotifications').mockReturnValue({ subscribe: (obs: any) => obs.error(new Error('error')) } as any);
      component.userId = 1;
      component.loadNotifications();
      expect(component.error()).toBe('Failed to load notifications.');
      expect(component.loading()).toBe(false);
    });
    
    it('should load notifications successfully', () => {
      const notifs: UserNotificationResponseDTO[] = [
        { id: 1, notificationId: 1, title: 'HW', message: 'M', isRead: false, createdAt: new Date().toISOString() }
      ];
      vi.spyOn(mockNotificationService, 'getUserNotifications').mockReturnValue(of(notifs));
      component.userId = 1;
      component.loadNotifications();
      
      expect(component.notifications().length).toBe(1);
      expect(component.loading()).toBe(false);
      expect(mockNotificationService.unreadCount()).toBe(1);
    });

    it('should handle userId not present in ngOnInit', () => {
      component.userId = 0;
      component.ngOnInit();
      expect(component.error()).toBe('Could not determine user ID.');
      expect(component.loading()).toBe(false);
    });
    
    it('should handle errors in markAsRead and markAllAsRead', () => {
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      
      const notif: UserNotificationResponseDTO = { id: 1, notificationId: 1, title: 'T', message: 'M', isRead: false, createdAt: new Date().toISOString() };
      component.userId = 1;
      
      vi.spyOn(mockNotificationService, 'markAsRead').mockReturnValue({ subscribe: (obs: any) => obs.error(new Error('err')) } as any);
      component.markAsRead(notif);
      expect(consoleSpy).toHaveBeenCalledWith('Failed to mark as read:', expect.any(Error));
      
      vi.spyOn(mockNotificationService, 'markAllAsRead').mockReturnValue({ subscribe: (obs: any) => obs.error(new Error('err')) } as any);
      component.markAllAsRead();
      expect(consoleSpy).toHaveBeenCalledWith('Failed to mark all as read:', expect.any(Error));
    });

    it('should add new notification from effect', () => {
      TestBed.flushEffects();
      const notif: UserNotificationResponseDTO = { id: 2, notificationId: 2, title: 'New', message: 'M', isRead: false, createdAt: new Date().toISOString() };
      
      // trigger signal change
      mockNotificationService.latestNotification.set(notif);
      TestBed.flushEffects();
      
      expect(component.notifications().some(n => n.notificationId === 2)).toBe(true);
      
      // trigger same signal again to test duplicate prevention
      mockNotificationService.latestNotification.set(notif);
      TestBed.flushEffects();
      expect(component.notifications().filter(n => n.notificationId === 2).length).toBe(1);
    });
  });

  describe('getNotificationConfig additional cases', () => {
    it('should return correct configs', () => {
      expect(component.getNotificationConfig('document verified').colorClass).toBe('document-notif');
      expect(component.getNotificationConfig('absent today').colorClass).toBe('attendance-notif');
      expect(component.getNotificationConfig('unknown').colorClass).toBe('default-notif');
    });
  });
});
