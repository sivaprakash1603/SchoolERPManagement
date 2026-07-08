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
      getAcademicCalendarSummary: () => of({ totalHolidays: 2, totalEvents: 1, events: [] }),
      createCalendarEvent: () => of({}),
      deleteCalendarEvent: () => of({})
    };
    mockAcademicYearService = {
      getAllAcademicYears: () => of([{ id: 1, yearName: '2026-2027', isCurrent: true, startDate: '2026-06-01', endDate: '2027-05-31' }])
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
    expect(component).toBeTruthy();
    fixture.detectChanges();
    expect(component.academicYears().length).toBe(1);
    expect(component.selectedAcademicYearId()).toBe(1);
  });

  it('should open and close create modal', () => {
    expect(component.showCreateModal()).toBe(false);
    component.openCreateModal();
    expect(component.showCreateModal()).toBe(true);

    component.closeCreateModal();
    expect(component.showCreateModal()).toBe(false);
  });

  it('should validate form and save calendar event successfully', () => {
    component.selectedAcademicYearId.set(1);
    component.createForm.set({
      date: '2026-07-01',
      endDate: '',
      description: 'Test Holiday',
      isHoliday: true
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
      isHoliday: true
    });

    component.saveCalendarEvent();
    expect(mockToastService.warning).toHaveBeenCalledWith('Please fill in both Date and Description.');
  });

  it('should handle event deletion successfully', () => {
    component.deletingEvent.set({ id: 10, date: '2026-07-01', description: 'Test', isHoliday: true, academicYearId: 1 });
    component.confirmDelete();
    expect(mockToastService.success).toHaveBeenCalledWith('Calendar entry deleted successfully!');
  });
});
