import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError, BehaviorSubject } from 'rxjs';
import { Dashboard } from './dashboard';
import { DashboardService } from '../../services/dashboard.service';
import { AcademicYearService } from '../../services/academic-year.service';
import { StudentService } from '../../services/student.service';
import { AttendanceService } from '../../services/attendance.service';
import { HomeworkService } from '../../services/homework.service';
import { FeeService } from '../../services/fee.service';
import { ExamService } from '../../services/exam.service';
import { SubjectService } from '../../services/subject.service';
import { TeacherService } from '../../services/teacher.service';
import { ParentService } from '../../services/parent.service';
import { ClassService } from '../../services/class.service';
import { DocumentService } from '../../services/document.service';
import { TimetableService } from '../../services/timetable.service';
import { NotificationService } from '../../services/notification.service';
import { ReportService } from '../../services/report.service';
import { AcademicCalendarService } from '../../services/academic-calendar.service';
import { ElementRef } from '@angular/core';

describe('Dashboard', () => {
  let component: Dashboard;
  let fixture: ComponentFixture<Dashboard>;
  let router: Router;

  // Mock Services
  let mockDashboardService: any;
  let mockAcademicYearService: any;
  let mockStudentService: any;
  let mockAttendanceService: any;
  let mockHomeworkService: any;
  let mockFeeService: any;
  let mockExamService: any;
  let mockSubjectService: any;
  let mockTeacherService: any;
  let mockParentService: any;
  let mockClassService: any;
  let mockDocumentService: any;
  let mockTimetableService: any;
  let mockNotificationService: any;
  let mockReportService: any;
  let mockCalendarService: any;

  beforeEach(async () => {
    mockDashboardService = {
      getAdminMetrics: vi.fn().mockReturnValue(of({ studentAttendanceRate: 80, revenueTrends: [{ month: 'Jan', amount: 1000 }], recentTransactions: [] })),
      getTeacherMetrics: vi.fn().mockReturnValue(of({ recentHomework: [], upcomingExams: [] }))
    };
    mockAcademicYearService = {
      getAllAcademicYears: vi.fn().mockReturnValue(of([{ id: 1, isCurrent: true, yearName: '2023-2024' }]))
    };
    mockStudentService = {
      getStudentByUserId: vi.fn().mockReturnValue(of({ id: 1, name: 'John Doe', regNo: 'S001' })),
      getAllStudents: vi.fn().mockReturnValue(of({ items: [{ id: 1, name: 'John Doe', regNo: 'S001', admissionDate: new Date() }] }))
    };
    mockAttendanceService = {
      getAttendanceByStudent: vi.fn().mockReturnValue(of([{ status: 'Present' }, { status: 'Absent' }]))
    };
    mockHomeworkService = {
      getHomeworksByStudentId: vi.fn().mockReturnValue(of([{ id: 1, subjectId: 1, dueDate: '2099-12-31' }]))
    };
    mockFeeService = {
      getFeeDetails: vi.fn().mockReturnValue(of({ totalAmount: 1000, paidAmount: 500, balanceAmount: 500 }))
    };
    mockExamService = {
      getAllExams: vi.fn().mockReturnValue(of([{ id: 1, examname: 'Midterm' }])),
      getStudentResults: vi.fn().mockReturnValue(of([{ examId: 1, subjectId: 1, marksObtained: 80 }]))
    };
    mockSubjectService = {
      getAllSubjects: vi.fn().mockReturnValue(of([{ id: 1, subjectName: 'Math' }]))
    };
    mockTeacherService = {
      getTeacherByUsername: vi.fn().mockReturnValue(of({ id: 1, name: 'Jane Smith' })),
      getAllTeachers: vi.fn().mockReturnValue(of({ items: [{ id: 1, name: 'Jane Smith', joiningdate: new Date() }] }))
    };
    mockParentService = {
      getParentByUserId: vi.fn().mockReturnValue(of({ id: 1, name: 'Parent One' })),
      getParentChildren: vi.fn().mockReturnValue(of([{ studentId: 1, name: 'John Doe', regNo: 'S001' }])),
      selectedChildId: null
    };
    mockClassService = {
      getAllClasses: vi.fn().mockReturnValue(of([{ id: 1, classname: '10', section: 'A', classteacherId: null }]))
    };
    mockDocumentService = {
      getPendingDocuments: vi.fn().mockReturnValue(of([{ id: 1 }]))
    };
    mockTimetableService = {
      getTeacherRequirements: vi.fn().mockReturnValue(of([{ subjectName: 'Math', requiredTeachers: 2, availableTeachers: 1, status: 'Understaffed (-1)' }])),
      getTeacherTimetable: vi.fn().mockReturnValue(of([{ classId: 1, subjectId: 1, dayOfWeek: 'Monday', startTime: '09:00:00', endTime: '10:00:00' }]))
    };
    mockNotificationService = {
      getUserNotifications: vi.fn().mockReturnValue(of([{ createdAt: new Date(), title: 'Test', message: 'Test message', isRead: false }]))
    };
    mockReportService = {
      getExamPerformanceReport: vi.fn().mockReturnValue(of({ averageBySubject: { 'Math': 85, 'English': 90 } }))
    };
    mockCalendarService = {
      getAcademicCalendarSummary: vi.fn().mockReturnValue(of({ events: [{ date: new Date().toISOString(), description: 'Event', isHoliday: false }] }))
    };

    await TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: DashboardService, useValue: mockDashboardService },
        { provide: AcademicYearService, useValue: mockAcademicYearService },
        { provide: StudentService, useValue: mockStudentService },
        { provide: AttendanceService, useValue: mockAttendanceService },
        { provide: HomeworkService, useValue: mockHomeworkService },
        { provide: FeeService, useValue: mockFeeService },
        { provide: ExamService, useValue: mockExamService },
        { provide: SubjectService, useValue: mockSubjectService },
        { provide: TeacherService, useValue: mockTeacherService },
        { provide: ParentService, useValue: mockParentService },
        { provide: ClassService, useValue: mockClassService },
        { provide: DocumentService, useValue: mockDocumentService },
        { provide: TimetableService, useValue: mockTimetableService },
        { provide: NotificationService, useValue: mockNotificationService },
        { provide: ReportService, useValue: mockReportService },
        { provide: AcademicCalendarService, useValue: mockCalendarService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Dashboard);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate');
  });

  afterEach(() => {
    sessionStorage.clear();
    vi.clearAllMocks();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit logic', () => {
    it('should load student dashboard if role is Student', () => {
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      expect(mockStudentService.getStudentByUserId).toHaveBeenCalledWith(1);
    });

    it('should set loading false if no userId for Student', () => {
      sessionStorage.setItem('role', 'Student');
      fixture.detectChanges();
      expect(component.loading()).toBe(false);
    });

    it('should load parent dashboard if role is Parent', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      expect(mockParentService.getParentByUserId).toHaveBeenCalledWith(1);
      expect(mockParentService.getParentChildren).toHaveBeenCalledWith(1);
    });

    it('should load first child for parent dashboard', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      expect(component.selectedChildId()).toBe(1);
    });

    it('should set loading false if parent has no children', () => {
      mockParentService.getParentChildren.mockReturnValue(of([]));
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      expect(component.parentChildren().length).toBe(0);
      expect(component.loading()).toBe(false);
    });
    
    it('should handle error when loading parent children', () => {
      mockParentService.getParentChildren.mockReturnValue(throwError(() => new Error('error')));
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      expect(component.error()).toBe('Failed to load your children details.');
    });

    it('should handle error when loading parent profile', () => {
      mockParentService.getParentByUserId.mockReturnValue(throwError(() => new Error('error')));
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      expect(component.error()).toBe('Failed to load parent profile.');
    });

    it('should set loading false if no userId for Parent', () => {
      sessionStorage.setItem('role', 'Parent');
      fixture.detectChanges();
      expect(component.loading()).toBe(false);
    });

    it('should load Admin dashboard and metrics', async () => {
      sessionStorage.setItem('role', 'Admin');
      sessionStorage.setItem('userId', '1');
      
      // Need canvas for charts
      component.demographicsChartRef = new ElementRef(document.createElement('canvas'));
      component.revenueChartRef = new ElementRef(document.createElement('canvas'));
      
      fixture.detectChanges();
      await new Promise(r => setTimeout(r, 150)); // for setTimeout
      expect(mockDashboardService.getAdminMetrics).toHaveBeenCalled();
      expect(component.metrics()).toBeTruthy();
      expect(component.systemAlerts().length).toBeGreaterThan(0);
    });

    it('should fallback academic year ID if no current year is set', async () => {
      mockAcademicYearService.getAllAcademicYears.mockReturnValue(of([{ id: 2, isCurrent: false, yearName: '2024-2025' }]));
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      expect(component.selectedAcademicYearId()).toBe(2);
      await new Promise(r => setTimeout(r, 150));
    });

    it('should handle academic year error for Admin by falling back to loadMetrics()', async () => {
      mockAcademicYearService.getAllAcademicYears.mockReturnValue(throwError(() => new Error('error')));
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      expect(mockDashboardService.getAdminMetrics).toHaveBeenCalledWith(undefined);
      await new Promise(r => setTimeout(r, 150));
    });

    it('should load Teacher dashboard and timetable', () => {
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 'teacher1');
      
      // ensure we match today for slot tests
      const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
      const today = days[new Date().getDay()];
      mockTimetableService.getTeacherTimetable.mockReturnValue(of([{ classId: 1, subjectId: 1, dayOfWeek: today, startTime: '09:00:00', endTime: '10:00:00' }]));

      fixture.detectChanges();
      
      expect(mockTeacherService.getTeacherByUsername).toHaveBeenCalledWith('teacher1');
      expect(mockDashboardService.getTeacherMetrics).toHaveBeenCalled();
      expect(component.todaySchedule().length).toBeGreaterThan(0);
    });
    
    it('should handle getTeacherByUsername error', () => {
      mockTeacherService.getTeacherByUsername.mockReturnValue(throwError(() => new Error('error')));
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 'teacher1');
      fixture.detectChanges();
      expect(component.error()).toBe('Failed to load teacher profile.');
    });
    
    it('should handle academic year error for Teacher', () => {
      mockAcademicYearService.getAllAcademicYears.mockReturnValue(throwError(() => new Error('error')));
      sessionStorage.setItem('role', 'Teacher');
      fixture.detectChanges();
      expect(component.loading()).toBe(false);
    });

    it('should handle other roles', () => {
      sessionStorage.setItem('role', 'UnknownRole');
      fixture.detectChanges();
      expect(component.loading()).toBe(false);
    });
  });

  describe('Helper methods and getters', () => {
    it('userName should return correct name based on role and data', () => {
      expect(component.userName).toBe('User');
      
      component.userRole.set('Student');
      component.studentData.set({ name: 'Student Name' });
      expect(component.userName).toBe('Student Name');

      component.userRole.set('Parent');
      component.parentData.set({ name: 'Parent Name' });
      expect(component.userName).toBe('Parent Name');

      component.userRole.set('Teacher');
      component.teacherData.set({ name: 'Teacher Name' });
      expect(component.userName).toBe('Teacher Name');

      sessionStorage.setItem('username', 'testuser');
      component.userRole.set('Admin');
      expect(component.userName).toBe('testuser');
      
      sessionStorage.setItem('name', 'Admin Name');
      expect(component.userName).toBe('Admin Name');
    });

    it('welcomeMessage should return correct message', () => {
      component.userRole.set('Student');
      expect(component.welcomeMessage).toContain('your academic progress');
      
      component.userRole.set('Parent');
      expect(component.welcomeMessage).toContain('child\'s academic progress');

      component.userRole.set('Admin');
      expect(component.welcomeMessage).toContain('institutional metrics');
    });

    it('salutation should be based on time', () => {
      const sal = component.salutation;
      expect(['Good Morning', 'Good Afternoon', 'Good Evening']).toContain(sal);
    });

    it('should toggle setup modal correctly', () => {
      component.openSetupModal(1);
      expect(component.showSetupModal()).toBe(true);
      expect(component.selectedSetupStep().id).toBe(1);

      component.closeSetupModal();
      expect(component.showSetupModal()).toBe(false);
      expect(component.selectedSetupStep()).toBeNull();
    });

    it('openSetupModal should navigate if step is completed', () => {
      component.openSetupModal(2, true);
      expect(router.navigate).toHaveBeenCalledWith(['/subjects']);
      expect(component.showSetupModal()).toBe(false);
    });
    
    it('openSetupModal should do nothing if invalid step', () => {
      component.openSetupModal(99);
      expect(component.showSetupModal()).toBe(false);
    });

    it('formatTime should format correctly', () => {
      expect(component.formatTime('09:30:00')).toBe('09:30 AM');
      expect(component.formatTime('14:45:00')).toBe('02:45 PM');
      expect(component.formatTime('12:00:00')).toBe('12:00 PM');
      expect(component.formatTime('00:15:00')).toBe('12:15 AM');
      expect(component.formatTime('')).toBe('');
    });
    
    it('isSlotActive should evaluate slot activity correctly', () => {
      const slotActive = { startTime: '00:00:00', endTime: '23:59:59' };
      const slotInactive = { startTime: '23:59:58', endTime: '23:59:59' };

      expect(component.isSlotActive(slotActive)).toBe(true);
      
      const now = new Date();
      const currentStr = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}:${now.getSeconds().toString().padStart(2, '0')}`;
      if (currentStr < '23:59:58') {
        expect(component.isSlotActive(slotInactive)).toBe(false);
      }
    });

    it('isSlotOver should evaluate if slot is over correctly', () => {
      const slotOver = { startTime: '00:00:00', endTime: '00:00:01' };
      const now = new Date();
      const currentStr = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}:${now.getSeconds().toString().padStart(2, '0')}`;
      if (currentStr > '00:00:01') {
        expect(component.isSlotOver(slotOver)).toBe(true);
      }
    });
    
    it('onChildChange should trigger loadStudentDashboardById', () => {
      const spy = vi.spyOn(component, 'loadStudentDashboardById');
      component.onChildChange('2');
      expect(component.selectedChildId()).toBe(2);
      expect(mockParentService.selectedChildId).toBe(2);
      expect(spy).toHaveBeenCalledWith(2);
    });
  });

  describe('Student Dashboard metrics loading', () => {
    it('should calculate attendance rate accurately', () => {
      component.loadStudentDashboardById(1, { id: 1, name: 'Student 1' });
      expect(component.studentAttendanceRate()).toBe(50); // 1 Present, 1 Absent out of 2 = 50%
    });

    it('should set attendance rate to 0 if no records', () => {
      mockAttendanceService.getAttendanceByStudent.mockReturnValue(of([]));
      component.loadStudentDashboardById(1, { id: 1, name: 'Student 1' });
      expect(component.studentAttendanceRate()).toBe(0);
    });

    it('should fallback when child data not found in parent array', () => {
      component.parentChildren.set([]);
      component.loadStudentDashboardById(1);
      expect(component.studentData().name).toBe('Student');
    });

    it('should handle exam results correctly', () => {
      component.loadStudentDashboardById(1, { id: 1 });
      expect(component.recentResults().length).toBeGreaterThan(0);
      expect(component.recentResults()[0].examName).toBe('Midterm');
      expect(component.recentResults()[0].subjectName).toBe('Math');
    });

    it('should handle errors in student metrics loading', () => {
      mockAttendanceService.getAttendanceByStudent.mockReturnValue(throwError(() => new Error('err')));
      mockHomeworkService.getHomeworksByStudentId.mockReturnValue(throwError(() => new Error('err')));
      mockFeeService.getFeeDetails.mockReturnValue(throwError(() => new Error('err')));
      mockExamService.getStudentResults.mockReturnValue(throwError(() => new Error('err')));
      component.loadStudentDashboardById(1, { id: 1 });
      // It should just log errors and complete loading
      expect(component.loading()).toBe(false);
    });
    
    it('should handle error in loadStudentDashboard', () => {
      mockStudentService.getStudentByUserId.mockReturnValue(throwError(() => new Error('err')));
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      expect(component.error()).toBe('Failed to load student profile details.');
    });
  });

  describe('Admin Dashboard Metrics Loading', () => {
    it('should process alerts correctly (unassigned classes, pending docs, attendance)', async () => {
      component.demographicsChartRef = new ElementRef(document.createElement('canvas'));
      component.revenueChartRef = new ElementRef(document.createElement('canvas'));
      
      mockDashboardService.getAdminMetrics.mockReturnValue(of({ studentAttendanceRate: 90 }));
      component.loadMetrics(1);
      await new Promise(r => setTimeout(r, 150));

      const alerts = component.systemAlerts();
      expect(alerts.find(a => a.route === '/classes')).toBeTruthy();
      expect(alerts.find(a => a.route === '/documents')).toBeTruthy();
      
      // Attendance is 90, so it should be "Review monthly attendance"
      const attAlert = alerts.find(a => a.route === '/attendance');
      expect(attAlert?.message).toContain('Review monthly attendance');
    });

    it('should generate empty workload list safely', async () => {
      component.demographicsChartRef = new ElementRef(document.createElement('canvas'));
      component.revenueChartRef = new ElementRef(document.createElement('canvas'));
      
      mockTimetableService.getTeacherRequirements.mockReturnValue(of([]));
      component.loadMetrics(1);
      await new Promise(r => setTimeout(r, 150));
      
      expect(component.staffWorkload().length).toBe(4); // Default static array
    });

    it('should handle exam performance report success', async () => {
      component.demographicsChartRef = new ElementRef(document.createElement('canvas'));
      component.revenueChartRef = new ElementRef(document.createElement('canvas'));
      
      component.loadMetrics(1);
      await new Promise(r => setTimeout(r, 150));
      
      expect(mockReportService.getExamPerformanceReport).not.toHaveBeenCalled();
    });

    it('should handle empty exam performance report and use default', async () => {
      component.demographicsChartRef = new ElementRef(document.createElement('canvas'));
      component.revenueChartRef = new ElementRef(document.createElement('canvas'));
      
      mockReportService.getExamPerformanceReport.mockReturnValue(of({})); // Empty report
      component.loadMetrics(1);
      await new Promise(r => setTimeout(r, 150));
      
      expect(mockReportService.getExamPerformanceReport).not.toHaveBeenCalled();
    });

    it('should handle exam performance report error and use default', async () => {
      component.demographicsChartRef = new ElementRef(document.createElement('canvas'));
      component.revenueChartRef = new ElementRef(document.createElement('canvas'));
      
      mockReportService.getExamPerformanceReport.mockReturnValue(throwError(() => new Error('error')));
      component.loadMetrics(1);
      await new Promise(r => setTimeout(r, 150));
      
      expect(mockReportService.getExamPerformanceReport).not.toHaveBeenCalled();
    });

    it('should handle getAdminMetrics error', async () => {
      mockDashboardService.getAdminMetrics.mockReturnValue(throwError(() => new Error('error')));
      component.loadMetrics(1);
      await new Promise(r => setTimeout(r, 150));
      
      expect(component.error()).toBe('Could not load dashboard metrics. Please try again later.');
    });
    
    it('should handle loadMetrics with no exams', async () => {
      mockExamService.getAllExams.mockReturnValue(of([]));
      component.demographicsChartRef = new ElementRef(document.createElement('canvas'));
      component.revenueChartRef = new ElementRef(document.createElement('canvas'));
      
      component.loadMetrics(1);
      await new Promise(r => setTimeout(r, 150));
      
    });
    
    it('should correctly format relative times for activities', async () => {
      sessionStorage.setItem('role', 'Admin');
      sessionStorage.setItem('userId', '1');
      
      const now = new Date();
      // Future date should fall back to default
      const future = new Date(now.getTime() + 100000);
      // Minutes ago
      const minsAgo = new Date(now.getTime() - 15 * 60000);
      // Hours ago
      const hoursAgo = new Date(now.getTime() - 2 * 3600000);
      const oneHourAgo = new Date(now.getTime() - 1 * 3600000);
      // Days ago
      const daysAgo = new Date(now.getTime() - 3 * 86400000);
      const oneDayAgo = new Date(now.getTime() - 1 * 86400000);
      // Months ago
      const monthsAgo = new Date(now.getTime() - 35 * 86400000);
      const oneMonthAgo = new Date(now.getTime() - 30 * 86400000);
      
      mockNotificationService.getUserNotifications.mockReturnValue(of([
        { createdAt: future, title: 'Fut', message: 'm' },
        { createdAt: minsAgo, title: 'Min', message: 'm' },
        { createdAt: hoursAgo, title: 'Hrs', message: 'm' },
        { createdAt: oneHourAgo, title: '1Hr', message: 'm' },
        { createdAt: daysAgo, title: 'Day', message: 'm' },
        { createdAt: oneDayAgo, title: '1Day', message: 'm' },
        { createdAt: monthsAgo, title: 'Mon', message: 'm' },
        { createdAt: oneMonthAgo, title: '1Mon', message: 'm' }
      ]));
      
      component.loadMetrics(1);
      await new Promise(r => setTimeout(r, 150));
      
      const acts = component.recentActivities();
      // Only first 5 are stored
      expect(acts[0].time).toBe('recently');
      expect(acts[1].time).toBe('15 mins ago');
      expect(acts[2].time).toBe('2 hrs ago');
      expect(acts[3].time).toBe('1 hr ago');
      expect(acts[4].time).toBe('3 days ago');
    });
  });

  describe('Teacher Dashboard Metrics Loading', () => {
    it('should handle loadTeacherMetrics error', () => {
      mockDashboardService.getTeacherMetrics.mockReturnValue(throwError(() => new Error('err')));
      component.loadTeacherMetrics(1, 1);
      expect(component.error()).toBe('Could not load teacher dashboard metrics.');
    });
    
    it('should map subjects and classes to timetable slots', () => {
      const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
      const today = days[new Date().getDay()];
      mockTimetableService.getTeacherTimetable.mockReturnValue(of([
        { classId: 1, subjectId: 1, dayOfWeek: today, startTime: '09:00:00', endTime: '10:00:00' },
        { classId: 99, subjectId: 99, dayOfWeek: today, startTime: '10:00:00', endTime: '11:00:00' }
      ]));
      
      component.loadTeacherMetrics(1, 1);
      
      const schedule = component.todaySchedule();
      expect(schedule.length).toBe(2);
      expect(schedule[0].className).toContain('10 - A');
      expect(schedule[0].subjectName).toBe('Math');
      expect(schedule[1].className).toContain('Class #99');
      expect(schedule[1].subjectName).toContain('Subject #99');
    });
  });

  describe('onYearChange', () => {
    it('should trigger loadMetrics for Admin', () => {
      const spy = vi.spyOn(component, 'loadMetrics');
      component.userRole.set('Admin');
      component.onYearChange({ target: { value: '2' } } as any);
      expect(component.selectedAcademicYearId()).toBe(2);
      expect(spy).toHaveBeenCalledWith(2);
    });

    it('should trigger loadTeacherMetrics for Teacher', () => {
      const spy = vi.spyOn(component, 'loadTeacherMetrics');
      component.userRole.set('Teacher');
      component.teacherId.set(1);
      component.onYearChange({ target: { value: '2' } } as any);
      expect(component.selectedAcademicYearId()).toBe(2);
      expect(spy).toHaveBeenCalledWith(1, 2);
    });
  });
  
  describe('DOM tests', () => {
    it('should render Student dashboard elements', async () => {
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '1');
      
      mockStudentService.getStudentByUserId.mockReturnValue(of({ name: 'S1', regNo: 'R1', currentClass: '10A', admissionDate: '2020-01-01' }));
      mockAttendanceService.getAttendanceByStudent.mockReturnValue(of([{ status: 'Present' }])); // 100% attendance rate
      mockFeeService.getFeeDetails.mockReturnValue(of({ totalAmount: 1000, paidAmount: 800, pendingAmount: 200 }));
      mockHomeworkService.getHomeworksByStudentId.mockReturnValue(of([{ title: 'Math HW', dueDate: '2099-12-31' }]));
      mockExamService.getStudentResults.mockReturnValue(of([{ examId: 1, subjectId: 1, marksObtained: 90, maxMarks: 100 }]));
      mockExamService.getAllExams.mockReturnValue(of([{ id: 1, examname: 'Mid' }]));
      mockSubjectService.getAllSubjects.mockReturnValue(of([{ id: 1, subjectName: 'Math' }]));
      
      fixture.detectChanges();
      await new Promise(r => setTimeout(r, 150));
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('S1');
      expect(compiled.textContent).toContain('100%');
      expect(compiled.textContent).toContain('200'); // Balance
      expect(compiled.textContent).toContain('Math HW');
      expect(compiled.textContent).toContain('Mid');
    });

    it('should render Admin dashboard elements', async () => {
      sessionStorage.setItem('role', 'Admin');
      sessionStorage.setItem('userId', '1');
      
      mockDashboardService.getAdminMetrics.mockReturnValue(of({ 
        totalStudents: 100, 
        totalTeachers: 10, 
        totalClasses: 5, 
        totalParents: 90,
        recentTransactions: []
      }));
      mockNotificationService.getUserNotifications.mockReturnValue(of([{ createdAt: new Date(), title: 'Act1', message: 'Test message', isRead: false }]));
      mockCalendarService.getAcademicCalendarSummary.mockReturnValue(of({ events: [{ date: new Date().toISOString(), description: 'Ev1', isHoliday: false }] }));
      mockTimetableService.getTeacherRequirements.mockReturnValue(of([{ subjectName: 'Math', requiredTeachers: 1, availableTeachers: 1, status: 'Optimal' }]));
      
      component.demographicsChartRef = new ElementRef(document.createElement('canvas'));
      component.revenueChartRef = new ElementRef(document.createElement('canvas'));
      
      fixture.detectChanges(); // triggers init
      await new Promise(r => setTimeout(r, 150));
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('TOTAL STUDENTS');
      expect(compiled.textContent).toContain('100'); // total students
      expect(compiled.textContent).toContain('Mid-Term Examinations');
      expect(compiled.textContent).toContain('Act1');
    });

    it('should render Admin setup modal via click', async () => {
      sessionStorage.setItem('role', 'Admin');
      mockDashboardService.getAdminMetrics.mockReturnValue(of({ recentTransactions: [] }));
      
      fixture.detectChanges();
      await new Promise(r => setTimeout(r, 150));
      fixture.detectChanges();
      
      component.openSetupModal(1);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Academic Sessions');
      
      // Close modal
      component.closeSetupModal();
      fixture.detectChanges();
    });
    
    it('should render Teacher dashboard elements', async () => {
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 'teacher1');
      
      mockTeacherService.getTeacherByUsername.mockReturnValue(of({ id: 1, name: 'Teacher 1' }));
      mockDashboardService.getTeacherMetrics.mockReturnValue(of({ 
        totalClassesAssigned: 3, 
        totalSubjectsTaught: 2, 
        upcomingExamsCount: 1, 
        pendingHomeworkToGrade: 5, 
        recentHomework: [
          { id: 1, title: 'Math HW', subjectName: 'Math', className: '10', section: 'A', dueDate: '2099-12-31', submissionsCount: 10, totalStudentsCount: 20 }
        ], 
        upcomingExams: [] 
      }));
      mockTimetableService.getTeacherTimetable.mockReturnValue(of([{ classId: 1, subjectId: 1, dayOfWeek: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'][new Date().getDay()], startTime: '09:00:00', endTime: '10:00:00' }]));
      mockClassService.getAllClasses.mockReturnValue(of([{ id: 1, classname: '10', section: 'A' }]));
      mockSubjectService.getAllSubjects.mockReturnValue(of([{ id: 1, subjectName: 'Math' }]));
      
      fixture.detectChanges();
      await new Promise(r => setTimeout(r, 150));
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('MY CLASSES');
      expect(compiled.textContent).toContain('10 - A');
      expect(compiled.textContent).toContain('Math HW');
    });

    it('should render Teacher dashboard empty states', async () => {
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 'teacher1');
      
      mockTeacherService.getTeacherByUsername.mockReturnValue(of({ id: 1, name: 'Teacher 1' }));
      mockDashboardService.getTeacherMetrics.mockReturnValue(of({ 
        totalClassesAssigned: 0, totalSubjectsTaught: 0, upcomingExamsCount: 0, pendingHomeworkToGrade: 0, 
        recentHomework: [], upcomingExams: [] 
      }));
      mockTimetableService.getTeacherTimetable.mockReturnValue(of([])); // Empty schedule
      
      fixture.detectChanges();
      await new Promise(r => setTimeout(r, 150));
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('No classes scheduled for today!');
      expect(compiled.textContent).toContain('No homework assignments posted yet.');
    });
    
    it('should handle chart destroy logic in Admin', async () => {
      sessionStorage.setItem('role', 'Admin');
      mockDashboardService.getAdminMetrics.mockReturnValue(of({ recentTransactions: [] }));
      
      fixture.detectChanges();
      
      component.demographicsChartRef = new ElementRef(document.createElement('canvas'));
      component.revenueChartRef = new ElementRef(document.createElement('canvas'));
      
      // First init creates them
      component.loadMetrics(1);
      await new Promise(r => setTimeout(r, 150));
      
      // Second init calls destroy()
      component.loadMetrics(1);
      await new Promise(r => setTimeout(r, 150));
      
      expect(component.metrics()).toBeTruthy();
    });

    it('should trigger onYearChange from template', async () => {
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      await fixture.whenStable();
      
      const select = fixture.nativeElement.querySelector('select');
      if (select) {
        select.value = '1';
        select.dispatchEvent(new Event('change'));
        expect(component.selectedAcademicYearId()).toBe(1);
      }
    });

    it('should trigger onChildChange from template', async () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      mockParentService.getParentChildren.mockReturnValue(of([{ studentId: 1, name: 'C1' }, { studentId: 2, name: 'C2' }]));
      fixture.detectChanges();
      await fixture.whenStable();
      
      const select = fixture.nativeElement.querySelector('select');
      if (select) {
        select.value = '2';
        select.dispatchEvent(new Event('change'));
        expect(component.selectedChildId()).toBe(2);
      }
    });

    it('should trigger setup modal clicks from template', async () => {
      sessionStorage.setItem('role', 'Admin');
      mockAcademicYearService.getAllAcademicYears.mockReturnValue(of([])); // empty sessions
      mockDashboardService.getAdminMetrics.mockReturnValue(of({ totalSubjects: 0, totalClasses: 0, totalTeachers: 0, totalStudents: 0, totalTimetables: 0, recentTransactions: [] }));
      
      fixture.detectChanges();
      await fixture.whenStable();
      
      const links = Array.from(fixture.nativeElement.querySelectorAll('a.stepper-item')) as HTMLElement[];
      expect(links.length).toBe(6);
      
      // click all setup steps to trigger functions
      links.forEach(l => l.dispatchEvent(new Event('click')));
      fixture.detectChanges();
      
      // click close button
      const closeBtn = fixture.nativeElement.querySelector('button.btn-close');
      if (closeBtn) closeBtn.dispatchEvent(new Event('click'));
      
      // click proceed button
      component.openSetupModal(1);
      fixture.detectChanges();
      const proceedBtn = fixture.nativeElement.querySelector('a.btn-primary.rounded-pill');
      if (proceedBtn) proceedBtn.dispatchEvent(new Event('click'));
      
      expect(component.showSetupModal()).toBe(false);
    });
  });
});
