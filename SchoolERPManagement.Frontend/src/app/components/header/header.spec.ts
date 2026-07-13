import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { Header } from './header';
import { StudentService } from '../../services/student.service';
import { TeacherService } from '../../services/teacher.service';
import { ParentService } from '../../services/parent.service';
import { NotificationService } from '../../services/notification.service';
import { environment } from '../../../environments/environment';

describe('Header', () => {
  let component: Header;
  let fixture: ComponentFixture<Header>;
  let router: Router;
  
  let mockStudentService: any;
  let mockTeacherService: any;
  let mockParentService: any;
  let mockNotificationService: any;

  beforeEach(async () => {
    mockStudentService = {
      getStudentByUserId: vi.fn()
    };
    mockTeacherService = {
      getTeacherByUsername: vi.fn()
    };
    mockParentService = {
      getParentByUserId: vi.fn()
    };
    mockNotificationService = {
      startConnection: vi.fn(),
      getUserNotifications: vi.fn().mockReturnValue(of([])),
      unreadCount: Object.assign(vi.fn().mockReturnValue(0), {
        set: vi.fn()
      })
    };

    await TestBed.configureTestingModule({
      imports: [Header],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: StudentService, useValue: mockStudentService },
        { provide: TeacherService, useValue: mockTeacherService },
        { provide: ParentService, useValue: mockParentService },
        { provide: NotificationService, useValue: mockNotificationService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Header);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    sessionStorage.clear();
  });

  afterEach(() => {
    sessionStorage.clear();
    vi.clearAllMocks();
  });

  describe('ngOnInit role logic', () => {
    it('should initialize Admin role correctly', () => {
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      
      expect(component.userRole()).toBe('Admin');
      expect(component.displayName()).toBe('Administrator');
      expect(component.displayPhotoUrl()).toBeNull();
    });

    it('should initialize Student role successfully with photo', () => {
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '1');
      mockStudentService.getStudentByUserId.mockReturnValue(of({ name: 'Alice', profilePhotoUrl: '/alice.png' }));
      
      fixture.detectChanges();
      
      expect(component.displayName()).toBe('Alice');
      expect(component.displayPhotoUrl()).toBe(environment.baseUrl + '/alice.png');
    });

    it('should initialize Student role successfully without photo', () => {
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '1');
      mockStudentService.getStudentByUserId.mockReturnValue(of({ name: 'Alice', profilePhotoUrl: null }));
      
      fixture.detectChanges();
      
      expect(component.displayName()).toBe('Alice');
      expect(component.displayPhotoUrl()).toBeNull();
    });

    it('should handle Student profile error with username fallback', () => {
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '1');
      sessionStorage.setItem('username', 'alice_stu');
      mockStudentService.getStudentByUserId.mockReturnValue(throwError(() => new Error('err')));
      
      fixture.detectChanges();
      
      expect(component.displayName()).toBe('alice_stu');
    });
    
    it('should handle Student profile error without username fallback', () => {
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '1');
      mockStudentService.getStudentByUserId.mockReturnValue(throwError(() => new Error('err')));
      
      fixture.detectChanges();
      
      expect(component.displayName()).toBe('Student');
    });

    it('should initialize Teacher role successfully with photo', () => {
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 'teacher_bob');
      mockTeacherService.getTeacherByUsername.mockReturnValue(of({ name: 'Mr. Bob', profilePhotoUrl: '/bob.png' }));
      
      fixture.detectChanges();
      
      expect(component.displayName()).toBe('Mr. Bob');
      expect(component.displayPhotoUrl()).toBe(environment.baseUrl + '/bob.png');
    });

    it('should initialize Teacher role successfully without photo', () => {
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 'teacher_bob');
      mockTeacherService.getTeacherByUsername.mockReturnValue(of({ name: 'Mr. Bob', profilePhotoUrl: null }));
      
      fixture.detectChanges();
      
      expect(component.displayName()).toBe('Mr. Bob');
      expect(component.displayPhotoUrl()).toBeNull();
    });

    it('should handle Teacher profile error with username fallback', () => {
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 'teacher_bob');
      mockTeacherService.getTeacherByUsername.mockReturnValue(throwError(() => new Error('err')));
      
      fixture.detectChanges();
      
      expect(component.displayName()).toBe('teacher_bob');
    });
    
    it('should handle Teacher profile error without username fallback', () => {
      sessionStorage.setItem('role', 'Teacher');
      mockTeacherService.getTeacherByUsername.mockReturnValue(throwError(() => new Error('err')));
      
      fixture.detectChanges();
      
      // Falls through to else block because username is empty
      expect(component.displayName()).toBe('User');
    });

    it('should initialize Parent role successfully', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      mockParentService.getParentByUserId.mockReturnValue(of({ name: 'Carol Parent' }));
      
      fixture.detectChanges();
      
      expect(component.displayName()).toBe('Carol Parent');
      expect(component.displayPhotoUrl()).toBeNull();
    });

    it('should handle Parent profile error with username fallback', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      sessionStorage.setItem('username', 'carol_p');
      mockParentService.getParentByUserId.mockReturnValue(throwError(() => new Error('err')));
      
      fixture.detectChanges();
      
      expect(component.displayName()).toBe('carol_p');
    });

    it('should handle Parent profile error without username fallback', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      mockParentService.getParentByUserId.mockReturnValue(throwError(() => new Error('err')));
      
      fixture.detectChanges();
      
      expect(component.displayName()).toBe('Parent');
    });

    it('should handle unknown role with username fallback', () => {
      sessionStorage.setItem('role', 'Guest');
      sessionStorage.setItem('username', 'guest123');
      fixture.detectChanges();
      
      expect(component.displayName()).toBe('guest123');
    });
    
    it('should handle unknown role without username fallback', () => {
      sessionStorage.setItem('role', 'Guest');
      fixture.detectChanges();
      
      expect(component.displayName()).toBe('User');
    });
  });

  describe('Notifications', () => {
    it('should connect SignalR and update unread count if userId is present', () => {
      sessionStorage.setItem('userId', '5');
      mockNotificationService.getUserNotifications.mockReturnValue(of([
        { isRead: false },
        { isRead: true },
        { isRead: false }
      ]));
      
      fixture.detectChanges();
      
      expect(mockNotificationService.startConnection).toHaveBeenCalled();
      expect(mockNotificationService.getUserNotifications).toHaveBeenCalledWith(5);
      expect(mockNotificationService.unreadCount.set).toHaveBeenCalledWith(2);
    });

    it('should not connect SignalR if no userId is present', () => {
      fixture.detectChanges();
      expect(mockNotificationService.startConnection).not.toHaveBeenCalled();
    });
  });

  describe('Actions and DOM events', () => {
    beforeEach(() => {
      fixture.detectChanges(); // Init admin
    });

    it('should toggle layout sidebar', () => {
      const isSidebarOpenBefore = component.layoutService.isSidebarOpen();
      const sidebarBtn = fixture.nativeElement.querySelector('button[aria-label="Toggle Menu"]') as HTMLButtonElement;
      sidebarBtn.click();
      expect(component.layoutService.isSidebarOpen()).toBe(!isSidebarOpenBefore);
    });

    it('should toggle theme on button click', () => {
      const isDarkBefore = component.themeService.isDarkMode();
      const themeBtn = fixture.nativeElement.querySelector('button[aria-label="Toggle theme"]') as HTMLButtonElement;
      themeBtn.click();
      expect(component.themeService.isDarkMode()).toBe(!isDarkBefore);
    });

    it('should clear session storage and navigate to login on logout click', () => {
      const navigateSpy = vi.spyOn(router, 'navigate');
      sessionStorage.setItem('token', 'dummy-token');

      const logoutBtn = fixture.nativeElement.querySelector('button[aria-label="Logout"]') as HTMLButtonElement;
      logoutBtn.click();

      expect(sessionStorage.getItem('token')).toBeNull();
      expect(navigateSpy).toHaveBeenCalledWith(['/login']);
    });
  });
  
  describe('DOM Rendering', () => {
    it('should render notification badge if unreadCount > 0', () => {
      mockNotificationService.unreadCount = vi.fn().mockReturnValue(3);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      const badge = compiled.querySelector('.badge');
      expect(badge).toBeTruthy();
      expect(badge?.textContent?.trim()).toBe('3');
    });

    it('should not render notification badge if unreadCount is 0', () => {
      mockNotificationService.unreadCount = vi.fn().mockReturnValue(0);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      const badge = compiled.querySelector('.badge');
      expect(badge).toBeFalsy();
    });

    it('should render dark mode icon when dark mode is true', () => {
      vi.spyOn(component.themeService, 'isDarkMode').mockReturnValue(true);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      const icon = compiled.querySelector('button[aria-label="Toggle theme"] i.bi-sun-fill');
      expect(icon).toBeTruthy();
    });
    
    it('should render light mode icon when dark mode is false', () => {
      vi.spyOn(component.themeService, 'isDarkMode').mockReturnValue(false);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      const icon = compiled.querySelector('button[aria-label="Toggle theme"] i.bi-moon-fill');
      expect(icon).toBeTruthy();
    });

    it('should render default avatar if no displayPhotoUrl', () => {
      fixture.detectChanges(); // Init
      component.displayPhotoUrl.set(null);
      component.displayName.set('Test User');
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      const img = compiled.querySelector('img[alt="Avatar"]') as HTMLImageElement;
      expect(img).toBeTruthy();
      expect(img.src).toContain('https://ui-avatars.com/api/?name=Test%20User');
    });

    it('should render actual profile photo if displayPhotoUrl exists', () => {
      fixture.detectChanges(); // Init
      component.displayPhotoUrl.set('http://example.com/photo.jpg');
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      const img = compiled.querySelector('img[alt="Profile Photo"]') as HTMLImageElement;
      expect(img).toBeTruthy();
      expect(img.src).toBe('http://example.com/photo.jpg');
    });
  });
});
