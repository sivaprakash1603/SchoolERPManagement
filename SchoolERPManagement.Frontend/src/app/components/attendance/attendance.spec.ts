import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { Attendance } from './attendance';
import { AttendanceService } from '../../services/attendance.service';
import { StudentService } from '../../services/student.service';
import { TeacherService } from '../../services/teacher.service';
import { ClassService } from '../../services/class.service';
import { AcademicYearService } from '../../services/academic-year.service';
import { AcademicCalendarService } from '../../services/academic-calendar.service';
import { ToastService } from '../../services/toast.service';
import { TimetableService } from '../../services/timetable.service';
import { ParentService } from '../../services/parent.service';
import { FilterStateService } from '../../services/filter-state.service';

describe('Attendance', () => {
  let component: Attendance;
  let fixture: ComponentFixture<Attendance>;
  let mockAttendanceService: any;
  let mockStudentService: any;
  let mockTeacherService: any;
  let mockClassService: any;
  let mockAcademicYearService: any;
  let mockCalendarService: any;
  let mockToastService: any;
  let mockTimetableService: any;
  let mockParentService: any;
  let mockFilterStateService: any;

  beforeEach(async () => {
    mockAttendanceService = {
      getAttendanceByClass: () => of([{ id: 1, studentId: 10, status: 'present', remarks: 'Good' }]),
      getStaffAttendanceByDate: () => of([{ id: 2, userId: 20, status: 'present', remarks: 'Good' }]),
      getStaffAttendanceByUser: () => of([{ id: 3, userId: 20, status: 'present', date: '2026-06-01' }]),
      markStaffAttendance: (dto: any) => of({ status: dto.status }),
      markAttendance: (dto: any) => of({ status: dto.status }),
      getAttendanceByStudent: () => of([
        { id: 1, studentId: 10, status: 'present', date: '2026-06-01' },
        { id: 2, studentId: 10, status: 'absent', date: '2026-06-02' },
        { id: 3, studentId: 10, status: 'late', date: '2026-06-03' },
        { id: 4, studentId: 10, status: 'onleave', date: '2026-06-04' }
      ])
    };
    mockStudentService = {
      getStudentByUserId: () => of({ id: 10, classId: 100 }),
      getStudentsByClassId: () => of([{ id: 10, name: 'Student Ten' }])
    };
    mockTeacherService = {
      getTeacherByUsername: () => of({ id: 20, className: 'Class 3-A', section: 'A' }),
      getAllTeachers: () => of({ items: [{ id: 20, name: 'Teacher Twenty', userId: 20 }] })
    };
    mockClassService = {
      getAllClasses: () => of([{ id: 100, classname: 'Class 3-A', section: 'A' }])
    };
    mockAcademicYearService = {
      getAllAcademicYears: () => of([{ id: 1, yearName: '2026-2027', isCurrent: true, startDate: '2026-06-01', endDate: '2027-05-31' }])
    };
    mockCalendarService = {
      getAcademicCalendarSummary: () => of({
        startDate: '2026-06-01',
        endDate: '2027-05-31',
        events: [{ id: 5, date: '2026-07-01', description: 'Summer holiday', isHoliday: true }]
      })
    };
    mockToastService = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn(),
      info: vi.fn()
    };
    mockTimetableService = {
      getTeacherTimetable: () => of([{ classId: 100 }])
    };
    mockParentService = {
      getParentByUserId: () => of({ id: 5 }),
      getParentChildren: () => of([{ studentId: 10 }]),
      selectedChildId: 10
    };
    mockFilterStateService = {
      getState: () => ({ activeTab: 'students', selectedAcademicYearId: 1, selectedClassId: 100 }),
      saveState: vi.fn()
    };

    sessionStorage.setItem('role', 'Admin');
    sessionStorage.setItem('userId', '1');

    await TestBed.configureTestingModule({
      imports: [Attendance],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AttendanceService, useValue: mockAttendanceService },
        { provide: StudentService, useValue: mockStudentService },
        { provide: TeacherService, useValue: mockTeacherService },
        { provide: ClassService, useValue: mockClassService },
        { provide: AcademicYearService, useValue: mockAcademicYearService },
        { provide: AcademicCalendarService, useValue: mockCalendarService },
        { provide: ToastService, useValue: mockToastService },
        { provide: TimetableService, useValue: mockTimetableService },
        { provide: ParentService, useValue: mockParentService },
        { provide: FilterStateService, useValue: mockFilterStateService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Attendance);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  afterEach(() => {
    sessionStorage.clear();
  });

  it('should create and load initial dashboard for Admin', () => {
    expect(component).toBeTruthy();
    fixture.detectChanges();
    expect(component.academicYears().length).toBe(1);
    expect(component.classes().length).toBe(1);
    expect(component.students().length).toBe(1);
  });

  it('should handle student profile loading and statistics calculation', () => {
    sessionStorage.setItem('role', 'Student');
    component.ngOnInit();
    fixture.detectChanges();
    expect(component.userRole()).toBe('Student');
    expect(component.studentAttendanceStats().rate).toBe(50);
  });

  it('should handle parent profile loading and child select changes', () => {
    sessionStorage.setItem('role', 'Parent');
    component.ngOnInit();
    fixture.detectChanges();
    expect(component.userRole()).toBe('Parent');

    component.onChildChange(10);
    expect(component.selectedChildId()).toBe(10);
  });

  it('should handle teacher profile loading and assigned classes timetable mapping', () => {
    sessionStorage.setItem('role', 'Teacher');
    sessionStorage.setItem('username', 'teacher1');
    component.ngOnInit();
    fixture.detectChanges();
    expect(component.userRole()).toBe('Teacher');
    expect(component.classes().length).toBe(1);
  });

  it('should handle load initial details with errors gracefully', () => {
    vi.spyOn(mockAcademicYearService, 'getAllAcademicYears').mockReturnValue(throwError(() => new Error('API Fail')));
    component.fetchAcademicYears();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to load academic sessions.');

    vi.spyOn(mockClassService, 'getAllClasses').mockReturnValue(throwError(() => new Error('Class Fail')));
    component.fetchClasses();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to load classes.');
  });

  it('should handle teacher profile/timetable load errors during fetchClasses', () => {
    component.userRole.set('Teacher');
    sessionStorage.setItem('username', 'teacher1');
    vi.spyOn(mockTimetableService, 'getTeacherTimetable').mockReturnValue(throwError(() => new Error('Timetable Fail')));
    component.fetchClasses();
    expect(component.classes().length).toBe(1);

    vi.spyOn(mockTeacherService, 'getTeacherByUsername').mockReturnValue(throwError(() => new Error('Teacher Fail')));
    component.fetchClasses();
    expect(component.classes().length).toBe(0);
  });

  it('should handle student profile resolve failure', () => {
    sessionStorage.setItem('role', 'Student');
    vi.spyOn(mockStudentService, 'getStudentByUserId').mockReturnValue(throwError(() => new Error('Student Profile Fail')));
    component.ngOnInit();
    expect(component.error()).toBe('Failed to resolve student profile.');
  });

  it('should handle parent profile resolve failure', () => {
    sessionStorage.setItem('role', 'Parent');
    vi.spyOn(mockParentService, 'getParentByUserId').mockReturnValue(throwError(() => new Error('Parent Profile Fail')));
    component.ngOnInit();
    expect(component.error()).toBe('Failed to resolve parent profile.');
  });

  it('should handle calendar summary fail safely', () => {
    vi.spyOn(mockCalendarService, 'getAcademicCalendarSummary').mockReturnValue(throwError(() => new Error('Cal summary fail')));
    component.loadYearDetails({ id: 1, yearName: '2026-2027', isCurrent: true, startDate: '2026-06-01', endDate: '2027-05-31' });
    expect(component.classes().length).toBe(1);
  });

  it('should restrict date picker changes outside bounds or on holidays', () => {
    component.holidays.set(['2026-07-01']);
    component.minDate.set('2026-06-01');
    component.maxDate.set('2027-05-31');
    component.selectedDate.set('2026-06-02');
    component.previousDate = '2026-06-02';

    // 1. Holiday select
    component.selectedDate.set('2026-07-01');
    component.onDateChange();
    expect(mockToastService.error).toHaveBeenCalledWith('Cannot select a holiday. Please select a working day.');

    // 2. Out of bounds select
    component.selectedDate.set('2026-05-01');
    component.onDateChange();
    expect(mockToastService.error).toHaveBeenCalledWith('Selected date must be between 2026-06-01 and 2027-05-31.');

    // 3. Valid date select (lines 607-608)
    component.selectedDate.set('2026-06-05');
    component.onDateChange();
    expect(component.previousDate).toBe('2026-06-05');
  });

  it('should toggle teacher and student active tab sheets', () => {
    component.activeTab.set('students');
    component.onTabChange('teachers');
    expect(component.activeTab()).toBe('teachers');
  });

  it('should set isHoliday when fetching teacher attendance on a holiday date', () => {
    // The mock calendar returns event on '2026-07-01' as isHoliday
    component.selectedDate.set('2026-07-01');
    component.selectedAcademicYearId.set(1);
    component.fetchTeachersAndAttendance();
    // holiday branch should have been triggered (lines 483-484)
    expect(component.isHoliday()).toBe(true);
    expect(component.holidayDescription()).toBe('Summer holiday');
  });

  it('should handle calendar fetch error in fetchTeachersAndAttendance (line 490)', () => {
    vi.spyOn(mockCalendarService, 'getAcademicCalendarSummary').mockReturnValue(throwError(() => new Error('Calendar fail')));
    component.selectedDate.set('2026-06-10');
    component.selectedAcademicYearId.set(1);
    component.fetchTeachersAndAttendance();
    // isHoliday stays false (error path covers line 490)
    expect(component.isHoliday()).toBe(false);
  });

  it('should set isHoliday when fetching student attendance on a holiday date (line 424-426)', () => {
    component.selectedDate.set('2026-07-01');
    component.selectedAcademicYearId.set(1);
    component.selectedClassId.set(100);
    component.fetchStudentsAndAttendance();
    expect(component.isHoliday()).toBe(true);
    expect(component.holidayDescription()).toBe('Summer holiday');
  });

  it('should handle calendar fetch error in fetchStudentsAndAttendance (line 432)', () => {
    vi.spyOn(mockCalendarService, 'getAcademicCalendarSummary').mockReturnValue(throwError(() => new Error('Calendar fail')));
    component.selectedDate.set('2026-06-10');
    component.selectedAcademicYearId.set(1);
    component.selectedClassId.set(100);
    component.fetchStudentsAndAttendance();
    expect(component.isHoliday()).toBe(false);
  });

  it('should handle fetchTeachersAndAttendance staff-records error path (line 514-517)', () => {
    vi.spyOn(mockAttendanceService, 'getStaffAttendanceByDate').mockReturnValue(throwError(() => new Error('Staff records fail')));
    component.selectedDate.set('2026-06-10');
    component.fetchTeachersAndAttendance();
    expect(component.error()).toBe('Failed to load staff attendance records.');
  });

  it('should handle fetchTeachersAndAttendance getAllTeachers error path (line 521-524)', () => {
    vi.spyOn(mockTeacherService, 'getAllTeachers').mockReturnValue(throwError(() => new Error('Teachers fail')));
    component.selectedDate.set('2026-06-10');
    component.fetchTeachersAndAttendance();
    expect(component.error()).toBe('Failed to load teachers list.');
  });


  it('should handle academic year selections and class changes', () => {
    component.onYearSelect(1);
    expect(component.selectedAcademicYearId()).toBe(1);

    component.onClassChange();
    expect(component.students().length).toBe(1);
  });

  it('should set selectedClassId null when Admin has no classes (line 382)', () => {
    vi.spyOn(mockClassService, 'getAllClasses').mockReturnValue(of([]));
    component.userRole.set('Admin');
    component.selectedAcademicYearId.set(1);
    component.fetchClasses();
    expect(component.selectedClassId()).toBeNull();
  });

  it('should early-return from fetchClasses when no yearId is selected (lines 321-322)', () => {
    component.selectedAcademicYearId.set(null as any);
    const spy = vi.spyOn(mockClassService, 'getAllClasses');
    component.fetchClasses();
    expect(spy).not.toHaveBeenCalled();
    expect(component.loading()).toBe(false);
  });

  it('should trigger fetchPersonalStaffAttendance when Teacher is on teachers tab (lines 401-402)', () => {
    const spy = vi.spyOn(component, 'fetchPersonalStaffAttendance');
    component.userRole.set('Teacher');
    component.activeTab.set('teachers');
    component.selectedDate.set('2026-06-10');
    component.fetchAttendanceSheet();
    expect(spy).toHaveBeenCalled();
  });

  it('should set selectedClassId null when Teacher has no matching classes (line 362)', () => {
    component.userRole.set('Teacher');
    sessionStorage.setItem('username', 'teacher1');
    // Teacher class 'Class 3-A' section 'A', timetable has classId 100, but classes list is empty after filter
    vi.spyOn(mockClassService, 'getAllClasses').mockReturnValue(of([]));
    component.selectedAcademicYearId.set(1);
    component.fetchClasses();
    expect(component.selectedClassId()).toBeNull();
  });



  it('should mark student attendance and handle save remarks successfully', () => {
    component.userRole.set('Admin');
    component.isAdminOrTeacher.set(true);
    const studentItem = { id: 10, name: 'Student Ten', status: 'unmarked', remarks: '', isUpdatingStatus: false, isSavingRemarks: false } as any;

    // Mark status
    component.updateStudentStatus(studentItem, 'present');
    expect(mockToastService.success).toHaveBeenCalledWith('Marked Student Ten as present');

    // Save remarks
    studentItem.remarks = 'Late bus';
    component.saveRemarks(studentItem);
    expect(mockToastService.success).toHaveBeenCalledWith('Saved remarks for Student Ten');
  });

  it('should handle student attendance marking and remarks save failures', () => {
    component.userRole.set('Admin');
    component.isAdminOrTeacher.set(true);
    const studentItem = { id: 10, name: 'Student Ten', status: 'unmarked', remarks: '', isUpdatingStatus: false, isSavingRemarks: false } as any;
    
    vi.spyOn(mockAttendanceService, 'markAttendance').mockReturnValue(throwError(() => ({ error: { message: 'Save Failed' } })));
    
    component.updateStudentStatus(studentItem, 'present');
    expect(mockToastService.error).toHaveBeenCalledWith('Save Failed');

    component.saveRemarks(studentItem);
    expect(mockToastService.error).toHaveBeenCalledWith('Save Failed');
  });

  it('should block attendance marking if user lacks permissions', () => {
    component.userRole.set('Student');
    component.isAdminOrTeacher.set(false);
    const studentItem = { id: 10, name: 'Student Ten', status: 'unmarked', remarks: '', isUpdatingStatus: false, isSavingRemarks: false } as any;

    component.updateStudentStatus(studentItem, 'present');
    expect(mockToastService.error).toHaveBeenCalledWith('You do not have permission to mark attendance.');
  });

  it('should mark teacher attendance and save staff remarks successfully', () => {
    component.userRole.set('Admin');
    const teacherItem = { id: 20, name: 'Teacher Twenty', userId: 20, status: 'unmarked', remarks: '', isUpdatingStatus: false, isSavingRemarks: false } as any;

    // Mark status
    component.updateTeacherStatus(teacherItem, 'present');
    expect(mockToastService.success).toHaveBeenCalledWith('Marked Teacher Twenty as present');

    // Save remarks
    teacherItem.remarks = 'Doctor appointment';
    component.saveTeacherRemarks(teacherItem);
    expect(mockToastService.success).toHaveBeenCalledWith('Saved remarks for Teacher Twenty');
  });

  it('should handle teacher attendance marking and remarks save failures', () => {
    component.userRole.set('Admin');
    const teacherItem = { id: 20, name: 'Teacher Twenty', userId: 20, status: 'unmarked', remarks: '', isUpdatingStatus: false, isSavingRemarks: false } as any;

    vi.spyOn(mockAttendanceService, 'markStaffAttendance').mockReturnValue(throwError(() => ({ error: { Errors: { key: ['Save Staff Fail'] } } })));
    
    component.updateTeacherStatus(teacherItem, 'present');
    expect(mockToastService.error).toHaveBeenCalledWith('Save Staff Fail');

    component.saveTeacherRemarks(teacherItem);
    expect(mockToastService.error).toHaveBeenCalledWith('Save Staff Fail');
  });

  it('should handle lowercase validation errors path in getErrorMessage (line 304)', () => {
    component.userRole.set('Admin');
    const teacherItem = { id: 20, name: 'Teacher Twenty', userId: 20, status: 'unmarked', remarks: '', isUpdatingStatus: false, isSavingRemarks: false } as any;

    // lowercase 'errors' key (line 301-305)
    vi.spyOn(mockAttendanceService, 'markStaffAttendance').mockReturnValue(throwError(() => ({ error: { errors: { field: ['Validation failed'] } } })));
    component.updateTeacherStatus(teacherItem, 'present');
    expect(mockToastService.error).toHaveBeenCalledWith('Validation failed');
  });


  it('should prevent marking staff attendance if not Admin', () => {
    component.userRole.set('Teacher');
    const teacherItem = { id: 20, name: 'Teacher Twenty', userId: 20, status: 'unmarked', remarks: '', isUpdatingStatus: false, isSavingRemarks: false } as any;

    component.updateTeacherStatus(teacherItem, 'present');
    expect(mockToastService.error).toHaveBeenCalledWith('Only administrators can mark staff attendance.');
  });

  it('should quick mark all unmarked students as present successfully', () => {
    component.userRole.set('Admin');
    component.isAdminOrTeacher.set(true);
    component.students.set([
      { id: 10, name: 'Student Ten', status: 'unmarked', remarks: '', isUpdatingStatus: false, isSavingRemarks: false } as any
    ]);

    component.markAllAsPresent();
    expect(mockToastService.success).toHaveBeenCalledWith('All unmarked students set to Present.');
  });

  it('should handle markAllAsPresent failure path', () => {
    component.userRole.set('Admin');
    component.isAdminOrTeacher.set(true);
    component.students.set([
      { id: 10, name: 'Student Ten', status: 'unmarked', remarks: '', isUpdatingStatus: false, isSavingRemarks: false } as any
    ]);
    vi.spyOn(mockAttendanceService, 'markAttendance').mockReturnValue(throwError(() => new Error('Quick mark error')));
    component.markAllAsPresent();
    expect(component.students()[0].isUpdatingStatus).toBe(false);
  });

  it('should guard quick mark all when all students are already marked', () => {
    component.userRole.set('Admin');
    component.isAdminOrTeacher.set(true);
    component.students.set([
      { id: 10, name: 'Student Ten', status: 'present', remarks: '', isUpdatingStatus: false, isSavingRemarks: false } as any
    ]);

    component.markAllAsPresent();
    expect(mockToastService.info).toHaveBeenCalledWith('All students are already marked.');
  });

  it('should navigate calendar months and months filters', () => {
    component.calendarMonth.set(5); // June
    component.calendarYear.set(2026);
    
    component.navigateMonth(1);
    expect(component.calendarMonth()).toBe(6); // July

    component.navigateMonth(-1);
    expect(component.calendarMonth()).toBe(5); // June

    component.goToCurrentMonth();
    expect(component.calendarMonth()).toBe(new Date().getMonth());

    // Filter available months list
    component.studentAttendanceRecords.set([
      { id: 1, date: '2026-06-01', status: 'present' },
      { id: 2, date: '2026-05-01', status: 'present' }
    ]);

    const months = component.getAvailableMonths();
    expect(months.length).toBe(2);

    component.onHistoryFilterChange('2026-06');
    expect(component.filteredAttendanceRecords().length).toBe(1);

    component.onHistoryFilterChange('all');
    expect(component.filteredAttendanceRecords().length).toBe(2);
  });

  it('should filter teacher logs by months', () => {
    component.personalStaffAttendance.set([
      { id: 1, date: '2026-06-01', status: 'present' },
      { id: 2, date: '2026-05-01', status: 'present' }
    ]);

    const months = component.getAvailableTeacherMonths();
    expect(months.length).toBe(2);

    component.onTeacherHistoryFilterChange('2026-06');
    expect(component.filteredTeacherAttendanceRecords().length).toBe(1);

    component.onTeacherHistoryFilterChange('all');
    expect(component.filteredTeacherAttendanceRecords().length).toBe(2);
  });

  it('should return helper calendar statuses classes', () => {
    expect(component.getCalendarStatusClass('present')).toBe('cal-present');
    expect(component.getCalendarStatusClass('absent')).toBe('cal-absent');
    expect(component.getCalendarStatusClass('late')).toBe('cal-late');
    expect(component.getCalendarStatusClass('leave')).toBe('cal-leave');
    expect(component.getCalendarStatusClass('unmarked')).toBe('');
  });

  it('should render DOM elements and click buttons', () => {
    component.userRole.set('Admin');
    component.isAdminOrTeacher.set(true);
    component.loading.set(false);
    component.students.set([
      { id: 10, name: 'Student Ten', status: 'unmarked', remarks: '', isUpdatingStatus: false, isSavingRemarks: false } as any
    ]);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    
    // Test all status buttons: present, absent, leave (lines 540-561)
    const studentButtons = compiled.querySelectorAll('table button');
    studentButtons.forEach((btn) => {
      (btn as HTMLButtonElement).click();
      fixture.detectChanges();
    });

    // Toggle remarks input (line 578)
    const remarksInput = compiled.querySelector('input[placeholder="Add remark..."]') as HTMLInputElement;
    if (remarksInput) {
      remarksInput.value = 'Late bus';
      remarksInput.dispatchEvent(new Event('input'));
      remarksInput.dispatchEvent(new KeyboardEvent('keyup', { key: 'Enter' }));
      fixture.detectChanges();
    }

    // Click save remarks button (line 584)
    const saveRemarksBtn = compiled.querySelector('button.btn-outline-secondary') as HTMLButtonElement;
    if (saveRemarksBtn) {
      saveRemarksBtn.click();
      fixture.detectChanges();
    }

    // Trigger isSavingRemarks spinner (lines 587-591)
    component.students.set([
      { id: 10, name: 'Student Ten', status: 'present', remarks: '', isUpdatingStatus: false, isSavingRemarks: true } as any
    ]);
    fixture.detectChanges();

    // Trigger isUpdatingStatus spinner (lines 564-568)
    component.students.set([
      { id: 10, name: 'Student Ten', status: 'unmarked', remarks: '', isUpdatingStatus: true, isSavingRemarks: false } as any
    ]);
    fixture.detectChanges();

    // Trigger empty students list (lines 601-608)
    component.students.set([]);
    fixture.detectChanges();
    expect(compiled.innerHTML).toContain('No students found in this class.');

    // Click Mark All as Present header button (line 53-60 in admin toolbar)
    component.students.set([
      { id: 10, name: 'Student Ten', status: 'unmarked', remarks: '', isUpdatingStatus: false, isSavingRemarks: false } as any
    ]);
    fixture.detectChanges();

    const markAllBtn = compiled.querySelector('button.btn-success') as HTMLButtonElement;
    if (markAllBtn) {
      markAllBtn.click();
      fixture.detectChanges();
    }

    // Tab changes
    const tabBtns = compiled.querySelectorAll('button.flex-grow-1');
    if (tabBtns.length >= 2) {
      const staffTab = tabBtns[1] as HTMLButtonElement;
      staffTab.click();
      fixture.detectChanges();
      expect(component.activeTab()).toBe('teachers');
    }
  });

  it('should render Teacher role personal logs and calendar', () => {
    component.userRole.set('Teacher');
    component.activeTab.set('teachers');
    component.currentUserId.set(20);
    component.loading.set(false);
    
    // Call fetchPersonalStaffAttendance to execute subscription (lines 530-565)
    component.fetchPersonalStaffAttendance();
    expect(component.personalStaffAttendance().length).toBe(1);

    // Set records in July 2026 (same month as current testing calendar date) to cover dot mapping (line 903)
    component.personalStaffAttendance.set([
      { id: 1, date: '2026-07-01', status: 'present', remarks: 'Good day' }
    ]);
    component.calendarMonth.set(6); // July
    component.calendarYear.set(2026);
    component.buildCalendar();
    component.applyTeacherHistoryFilter();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.innerHTML).toContain('My Attendance Logs');
    expect(compiled.innerHTML).toContain('Attendance History');

    // Trigger select ngModelChange for teacher logs filter (line 952)
    const selectEl = compiled.querySelector('select') as HTMLSelectElement;
    if (selectEl) {
      selectEl.value = 'all';
      selectEl.dispatchEvent(new Event('change'));
      fixture.detectChanges();
    }

    // Render empty teacher logs list (lines 1018-1024)
    component.personalStaffAttendance.set([]);
    component.teacherHistoryFilterMonth.set('2026-06');
    component.applyTeacherHistoryFilter();
    fixture.detectChanges();
    expect(compiled.innerHTML).toContain('No attendance records found');

    // Click calendar month navigation buttons
    const navButtons = compiled.querySelectorAll('.btn-outline-secondary');
    if (navButtons.length >= 2) {
      const prevBtn = navButtons[0] as HTMLButtonElement;
      prevBtn.click();
      fixture.detectChanges();
    }

    // Click next month button and Today button
    const nextBtn = compiled.querySelector('.btn-outline-secondary:last-of-type') as HTMLButtonElement;
    if (nextBtn) {
      nextBtn.click();
      fixture.detectChanges();
    }
    const todayBtn = compiled.querySelector('.btn-link') as HTMLButtonElement;
    if (todayBtn) {
      todayBtn.click();
      fixture.detectChanges();
    }
  });

  it('should handle fetchPersonalStaffAttendance empty logs state', () => {
    component.userRole.set('Teacher');
    component.currentUserId.set(20);
    vi.spyOn(mockAttendanceService, 'getStaffAttendanceByUser').mockReturnValue(of([]));
    component.fetchPersonalStaffAttendance();
    expect(component.teacherAttendanceStats().total).toBe(0);
  });

  it('should handle fetchPersonalStaffAttendance error path', () => {
    component.userRole.set('Teacher');
    component.currentUserId.set(20);
    vi.spyOn(mockAttendanceService, 'getStaffAttendanceByUser').mockReturnValue(throwError(() => new Error('Staff logs fail')));
    component.fetchPersonalStaffAttendance();
    expect(component.error()).toBe('Failed to load personal staff attendance.');
  });

  it('should handle fetchTeachersAndAttendance success and fail paths', () => {
    component.selectedAcademicYearId.set(1);
    component.selectedDate.set('2026-06-01');

    // 1. Success path
    component.fetchTeachersAndAttendance();
    expect(component.teachers().length).toBe(1);

    // 2. getStaffAttendanceByDate fails (runs fallback path)
    vi.spyOn(mockAttendanceService, 'getStaffAttendanceByDate').mockReturnValue(throwError(() => new Error('Staff attendance records fail')));
    component.fetchTeachersAndAttendance();
    expect(component.teachers().length).toBe(1);

    // 3. getAllTeachers fails
    vi.spyOn(mockTeacherService, 'getAllTeachers').mockReturnValue(throwError(() => new Error('Teachers list fail')));
    component.fetchTeachersAndAttendance();
    expect(component.error()).toBe('Failed to load teachers list.');
  });

  it('should handle student attendance list loading error paths', () => {
    // 1. getStudentsByClassId fails
    vi.spyOn(mockStudentService, 'getStudentsByClassId').mockReturnValue(throwError(() => new Error('Students list fail')));
    component.fetchStudentsAndAttendance();
    expect(component.error()).toBe('Failed to load class students.');

    // 2. getAttendanceByClass fails (runs fallback path)
    vi.spyOn(mockStudentService, 'getStudentsByClassId').mockReturnValue(of([{ id: 10, name: 'Student Ten' }]));
    vi.spyOn(mockAttendanceService, 'getAttendanceByClass').mockReturnValue(throwError(() => new Error('Records fail')));
    component.fetchStudentsAndAttendance();
    expect(component.students().length).toBe(1);
  });

  it('should handle student attendance fetch empty state and logs error', () => {
    // 1. Empty logs
    vi.spyOn(mockAttendanceService, 'getAttendanceByStudent').mockReturnValue(of([]));
    component.fetchStudentAttendance(10);
    expect(component.studentAttendanceStats().total).toBe(0);

    // 2. Fetch logs fails (lines 785-787)
    vi.spyOn(mockAttendanceService, 'getAttendanceByStudent').mockReturnValue(throwError(() => new Error('Logs fail')));
    component.fetchStudentAttendance(10);
    expect(component.error()).toBe('Failed to fetch attendance logs.');
  });

  it('should render Student/Parent role calendar and history logs', () => {
    component.userRole.set('Student');
    component.loading.set(false);
    component.studentAttendanceRecords.set([
      { id: 1, date: '2026-06-01', status: 'present', remarks: 'On time' }
    ]);
    component.buildCalendar();
    component.applyHistoryFilter();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.innerHTML).toContain('My Attendance Dashboard');
    
    // Check calendar days
    expect(compiled.querySelector('.calendar-grid')).toBeTruthy();
  });

  it('should render teacher sheet with remarks input, submit remarks and handle empty list', () => {
    component.userRole.set('Admin');
    component.activeTab.set('teachers');
    component.loading.set(false);
    component.teachers.set([
      { id: 20, name: 'Teacher Twenty', userId: 20, status: 'unmarked', remarks: '', isUpdatingStatus: false, isSavingRemarks: false } as any
    ]);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    // Click all teacher status buttons: Present, Absent, Leave (lines 660-690)
    const teacherButtons = compiled.querySelectorAll('table button');
    teacherButtons.forEach((btn) => {
      (btn as HTMLButtonElement).click();
      fixture.detectChanges();
    });

    // Test remarks input Enter key (line 707)
    const remarksInput = compiled.querySelector('input[placeholder="Add remark..."]') as HTMLInputElement;
    if (remarksInput) {
      remarksInput.value = 'Doctor visit';
      remarksInput.dispatchEvent(new Event('input'));
      remarksInput.dispatchEvent(new KeyboardEvent('keyup', { key: 'Enter' }));
      fixture.detectChanges();
    }

    // Click save remarks button (line 713)
    const saveBtn = compiled.querySelector('button.btn-outline-secondary') as HTMLButtonElement;
    if (saveBtn) {
      saveBtn.click();
      fixture.detectChanges();
    }

    // Trigger isSavingRemarks spinner (line 716)
    component.teachers.set([
      { id: 20, name: 'Teacher Twenty', userId: 20, status: 'present', remarks: '', isUpdatingStatus: false, isSavingRemarks: true } as any
    ]);
    fixture.detectChanges();

    // Trigger isUpdatingStatus spinner (lines 693-696)
    component.teachers.set([
      { id: 20, name: 'Teacher Twenty', userId: 20, status: 'unmarked', remarks: '', isUpdatingStatus: true, isSavingRemarks: false } as any
    ]);
    fixture.detectChanges();

    // Trigger empty teachers list (lines 731-735)
    component.teachers.set([]);
    fixture.detectChanges();
    expect(compiled.innerHTML).toContain('No staff members found.');
  });

  it('should render holiday state in both student and teacher tabs', () => {
    component.userRole.set('Admin');
    component.loading.set(false);
    component.isHoliday.set(true);
    component.holidayDescription.set('National Holiday');
    component.students.set([
      { id: 10, name: 'Student Ten', status: 'unmarked', remarks: '', isUpdatingStatus: false, isSavingRemarks: false } as any
    ]);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.innerHTML).toContain('National Holiday');

    // Switch to teacher tab with holiday
    component.activeTab.set('teachers');
    component.teachers.set([
      { id: 20, name: 'Teacher Twenty', userId: 20, status: 'unmarked', remarks: '', isUpdatingStatus: false, isSavingRemarks: false } as any
    ]);
    fixture.detectChanges();
    expect(compiled.innerHTML).toContain('Teacher Attendance');
  });

  it('should render Parent role with child selector when children exist', () => {
    component.userRole.set('Parent');
    component.loading.set(false);
    component.parentChildren.set([{ studentId: 10, name: 'Child One', className: 'Class 3-A' }]);
    component.selectedChildId.set(10);
    component.studentAttendanceRecords.set([
      { id: 1, date: '2026-06-01', status: 'present', remarks: '' }
    ]);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.innerHTML).toContain('Select Child:');

    // Trigger child change from DOM select
    const childSelect = compiled.querySelector('select') as HTMLSelectElement;
    if (childSelect) {
      childSelect.value = '10';
      childSelect.dispatchEvent(new Event('change'));
      fixture.detectChanges();
    }
  });
});
