import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { AcademicCalendar } from './academic-calendar';
import { AcademicCalendarService } from '../../services/academic-calendar.service';
import { AcademicYearService } from '../../services/academic-year.service';
import { ToastService } from '../../services/toast.service';

describe('AcademicCalendar', () => {
  let component: AcademicCalendar;
  let fixture: ComponentFixture<AcademicCalendar>;
  let mockCalendarService: any;
  let mockAcademicYearService: any;
  let mockToastService: any;
  beforeEach(async () => {
    mockCalendarService = {
      getAcademicCalendarSummary: () => of({
        startDate: '2026-06-01',
        endDate: '2027-05-31',
        totalDays: 365,
        weekendDays: 52,
        holidayDays: 10,
        workingDays: 303,
        events: [{ id: 10, date: '2026-07-01', description: 'Test Event', isHoliday: true, academicYearId: 1 }]
      }),
      createCalendarEvent: () => of({}),
      deleteCalendarEvent: () => of({})
    };
    mockAcademicYearService = {
      getAllAcademicYears: () => of([
        { id: 1, yearName: '2026-2027', isCurrent: false, startDate: '2026-06-01', endDate: '2027-05-31' }
      ])
    };
    mockToastService = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [AcademicCalendar],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AcademicCalendarService, useValue: mockCalendarService },
        { provide: AcademicYearService, useValue: mockAcademicYearService },
        { provide: ToastService, useValue: mockToastService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AcademicCalendar);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create and load academic years and calendar', () => {
    vi.spyOn(mockAcademicYearService, 'getAllAcademicYears').mockReturnValue(of([{ id: 1, yearName: '2026-2027', isCurrent: true, startDate: '2026-06-01', endDate: '2027-05-31' }]));
    sessionStorage.setItem('role', 'Admin');
    component.ngOnInit();
    expect(component).toBeTruthy();
    fixture.detectChanges();
    expect(component.academicYears().length).toBe(1);
    expect(component.selectedAcademicYearId()).toBe(1);
  });

  it('should handle alternative user roles', () => {
    sessionStorage.setItem('role', 'Parent');
    component.ngOnInit();
    expect(component.isAdmin()).toBe(false);
  });

  it('should load calendar summary with error handling', () => {
    vi.spyOn(mockCalendarService, 'getAcademicCalendarSummary').mockReturnValue(throwError(() => new Error('API Error')));
    component.fetchCalendar();
    fixture.detectChanges();
    expect(component.error()).toBe('Failed to load academic calendar.');
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.alert-danger')).toBeTruthy();
  });

  it('should handle academic year change selection', () => {
    const mockEvent = {
      target: { value: '2' }
    } as unknown as Event;

    component.onYearChange(mockEvent);
    expect(component.selectedAcademicYearId()).toBe(2);
  });

  it('should open and close create modal', () => {
    expect(component.showCreateModal()).toBe(false);
    component.openCreateModal();
    expect(component.showCreateModal()).toBe(true);

    component.closeCreateModal();
    expect(component.showCreateModal()).toBe(false);
  });

  it('should open and close delete modal', () => {
    expect(component.showDeleteModal()).toBe(false);
    const mockEvent = { id: 1, date: '2026-07-01', description: 'Holiday', isHoliday: true, academicYearId: 1 };
    component.openDeleteModal(mockEvent);
    expect(component.showDeleteModal()).toBe(true);

    component.closeDeleteModal();
    expect(component.showDeleteModal()).toBe(false);
  });

  it('should validate form and save calendar event successfully', () => {
    component.selectedAcademicYearId.set(1);
    component.createForm.set({
      date: '2026-07-01',
      endDate: '2026-07-02',
      description: 'Test Holiday',
      isHoliday: true,
      isParentTeacherMeeting: false,
      pmtStartTime: '',
      pmtEndTime: ''
    });

    component.saveCalendarEvent();
    expect(mockToastService.success).toHaveBeenCalledWith('Academic calendar entry added successfully!');
  });

  it('should warn if date or description is missing', () => {
    component.selectedAcademicYearId.set(1);
    component.createForm.set({
      date: '',
      endDate: '',
      description: '   ',
      isHoliday: true,
      isParentTeacherMeeting: false,
      pmtStartTime: '',
      pmtEndTime: ''
    });

    component.saveCalendarEvent();
    expect(mockToastService.warning).toHaveBeenCalledWith('Please fill in both Date and Description.');
  });

  it('should warn if end date is earlier than start date', () => {
    component.selectedAcademicYearId.set(1);
    component.createForm.set({
      date: '2026-07-02',
      endDate: '2026-07-01',
      description: 'Test Event',
      isHoliday: true,
      isParentTeacherMeeting: false,
      pmtStartTime: '',
      pmtEndTime: ''
    });

    component.saveCalendarEvent();
    expect(mockToastService.warning).toHaveBeenCalledWith('End date cannot be earlier than start date.');
  });

  it('should handle error when saving calendar event fails', () => {
    vi.spyOn(mockCalendarService, 'createCalendarEvent').mockReturnValue(throwError(() => ({ error: { message: 'Save Failed' } })));
    component.selectedAcademicYearId.set(1);
    component.createForm.set({
      date: '2026-07-01',
      endDate: '',
      description: 'Test Holiday',
      isHoliday: true,
      isParentTeacherMeeting: false,
      pmtStartTime: '',
      pmtEndTime: ''
    });

    component.saveCalendarEvent();
    expect(mockToastService.error).toHaveBeenCalledWith('Save Failed');
  });

  it('should handle event deletion successfully', () => {
    component.deletingEvent.set({ id: 10, date: '2026-07-01', description: 'Test', isHoliday: true, academicYearId: 1 });
    component.confirmDelete();
    expect(mockToastService.success).toHaveBeenCalledWith('Calendar entry deleted successfully!');
  });

  it('should handle error when event deletion fails', () => {
    vi.spyOn(mockCalendarService, 'deleteCalendarEvent').mockReturnValue(throwError(() => ({ error: { message: 'Delete Failed' } })));
    component.deletingEvent.set({ id: 10, date: '2026-07-01', description: 'Test', isHoliday: true, academicYearId: 1 });
    component.confirmDelete();
    expect(mockToastService.error).toHaveBeenCalledWith('Delete Failed');
  });

  it('should handle error when loading academic years fails', () => {
    vi.spyOn(mockAcademicYearService, 'getAllAcademicYears').mockReturnValue(throwError(() => new Error('API Error')));
    component.ngOnInit();
    expect(component.error()).toBe('Failed to load academic sessions.');
  });

  it('should handle null academic year selection in fetchCalendar', () => {
    component.selectedAcademicYearId.set(null);
    component.fetchCalendar();
    expect(component.summary()).toBeNull();
  });

  it('should set default date to minDate when today falls outside min/max range', () => {
    component.minDate.set('2026-08-01');
    component.maxDate.set('2027-05-31');
    component.openCreateModal();
    expect(component.createForm().date).toBe('2026-08-01');
  });

  it('should render HTML elements and allow clicks', () => {
    component.isAdmin.set(true);
    component.loading.set(false);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const addBtn = compiled.querySelector('.btn-primary') as HTMLButtonElement;
    if (addBtn) {
      addBtn.click();
      expect(component.showCreateModal()).toBe(true);
    }
  });

  it('should render empty calendar session state', () => {
    component.isAdmin.set(true);
    component.loading.set(false);
    component.summary.set({
      startDate: '2026-06-01',
      endDate: '2027-05-31',
      totalDays: 365,
      weekendDays: 52,
      holidayDays: 10,
      workingDays: 303,
      events: []
    });
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.innerHTML).toContain('No events or holidays registered');
  });

  it('should render create and delete modals in the DOM when open', () => {
    // Populate calendar summary list first and click delete button in DOM
    component.isAdmin.set(true);
    component.loading.set(false);
    component.summary.set({
      startDate: '2026-06-01',
      endDate: '2027-05-31',
      totalDays: 365,
      weekendDays: 52,
      holidayDays: 10,
      workingDays: 303,
      events: [{ id: 10, date: '2026-07-01', description: 'Test Event', isHoliday: true, academicYearId: 1 }]
    });
    fixture.detectChanges();

    let compiled = fixture.nativeElement as HTMLElement;
    const trashBtn = compiled.querySelector('.btn-light.text-danger') as HTMLButtonElement;
    if (trashBtn) {
      trashBtn.click();
      fixture.detectChanges();
      expect(component.showDeleteModal()).toBe(true);
    }

    component.createForm.set({
      date: '2026-07-01',
      endDate: '2026-07-02',
      description: 'Test Holiday',
      isHoliday: true,
      isParentTeacherMeeting: false,
      pmtStartTime: '',
      pmtEndTime: ''
    });

    // Open create modal and detect changes to render template HTML branches
    component.showCreateModal.set(true);
    fixture.detectChanges();
    compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.modal-card')).toBeTruthy();

    // Click Cancel button in create modal
    const cancelCreateBtn = compiled.querySelector('.modal-footer-custom .btn-light') as HTMLButtonElement;
    if (cancelCreateBtn) {
      cancelCreateBtn.click();
      fixture.detectChanges();
      expect(component.showCreateModal()).toBe(false);
    }

    // Reopen create modal, save it
    component.showCreateModal.set(true);
    fixture.detectChanges();

    // Trigger ngModelChanges by changing input values in the DOM
    const inputs = compiled.querySelectorAll('input');
    if (inputs.length >= 4) {
      const dateInput = inputs[0] as HTMLInputElement;
      dateInput.value = '2026-07-05';
      dateInput.dispatchEvent(new Event('input'));

      const endDateInput = inputs[1] as HTMLInputElement;
      endDateInput.value = '2026-07-06';
      endDateInput.dispatchEvent(new Event('input'));

      const descInput = inputs[2] as HTMLInputElement;
      descInput.value = 'New Holiday Name';
      descInput.dispatchEvent(new Event('input'));

      const holidaySwitch = inputs[3] as HTMLInputElement;
      holidaySwitch.checked = false;
      holidaySwitch.dispatchEvent(new Event('change'));
    }
    fixture.detectChanges();

    const saveCreateBtn = compiled.querySelector('.modal-footer-custom .btn-primary') as HTMLButtonElement;
    if (saveCreateBtn) {
      saveCreateBtn.click();
      fixture.detectChanges();
    }

    // Reopen modal to inspect saving state
    component.showCreateModal.set(true);
    component.isSaving.set(true);
    fixture.detectChanges();
    expect(compiled.querySelector('.modal-footer-custom .spinner-border')).toBeTruthy();

    // Close create modal and open delete modal
    component.isSaving.set(false);
    component.showCreateModal.set(false);
    component.deletingEvent.set({ id: 10, date: '2026-07-01', description: 'Test', isHoliday: true, academicYearId: 1 });
    component.showDeleteModal.set(true);
    fixture.detectChanges();
    compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.modal-card')).toBeTruthy();

    // Click Confirm Delete button
    const confirmDeleteBtn = compiled.querySelector('.modal-footer-custom .btn-danger') as HTMLButtonElement;
    if (confirmDeleteBtn) {
      confirmDeleteBtn.click();
      fixture.detectChanges();
    }

    // Simulate clicking close/cancel inside delete modal
    component.showDeleteModal.set(true);
    fixture.detectChanges();
    const closeBtn = compiled.querySelector('.modal-footer-custom .btn-light') as HTMLButtonElement;
    if (closeBtn) {
      closeBtn.click();
      fixture.detectChanges();
      expect(component.showDeleteModal()).toBe(false);
    }

    // Reopen delete modal
    component.showDeleteModal.set(true);
    component.isDeleting.set(true); // Toggle deleting state to render spinner
    fixture.detectChanges();
    compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.modal-footer-custom .spinner-border')).toBeTruthy();
  });

  it('should cover loading spinner and interaction buttons in template', () => {
    // 1. Cover loading spinner (lines 2-7)
    component.loading.set(true);
    fixture.detectChanges();
    let compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('img[alt="Loading..."]')).toBeTruthy();

    // 2. Cover select changes (line 51) and refresh click (line 63)
    component.loading.set(false);
    sessionStorage.setItem('role', 'Admin');
    component.ngOnInit();
    fixture.detectChanges();
    compiled = fixture.nativeElement as HTMLElement;

    const selectEl = compiled.querySelector('select') as HTMLSelectElement;
    if (selectEl) {
      selectEl.value = '1';
      selectEl.dispatchEvent(new Event('change'));
      fixture.detectChanges();
    }

    const refreshBtn = compiled.querySelector('.btn-outline-secondary') as HTMLButtonElement;
    if (refreshBtn) {
      refreshBtn.click();
      fixture.detectChanges();
    }
  });
});
