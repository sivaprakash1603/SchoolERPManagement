import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { Homework } from './homework';
import { HomeworkService } from '../../services/homework.service';
import { ToastService } from '../../services/toast.service';
import { FilterStateService } from '../../services/filter-state.service';
import { AcademicYearService } from '../../services/academic-year.service';
import { ClassService } from '../../services/class.service';
import { SubjectService } from '../../services/subject.service';
import { TeacherService } from '../../services/teacher.service';

describe('Homework', () => {
  let component: Homework;
  let fixture: ComponentFixture<Homework>;
  let mockHomeworkService: any;
  let mockToastService: any;
  let mockFilterStateService: any;

  beforeEach(async () => {
    mockHomeworkService = {
      getHomeworksByClassAndSubject: () => of([]),
      getTeacherHomeworks: () => of([]),
      addHomework: () => of({}),
      submitHomework: () => of({}),
      evaluateSubmission: () => of({}),
      unsubmitHomework: () => of({}),
      getSubmissionsForHomework: () => of([])
    };

    mockToastService = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn(),
      info: vi.fn()
    };

    mockFilterStateService = {
      getState: vi.fn().mockReturnValue({
        selectedAcademicYearId: 1,
        selectedClassId: 2,
        selectedSubjectId: 3,
        studentFilterTab: 'all'
      }),
      saveState: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [Homework],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: HomeworkService, useValue: mockHomeworkService },
        { provide: ToastService, useValue: mockToastService },
        { provide: FilterStateService, useValue: mockFilterStateService },
        { provide: AcademicYearService, useValue: { getAllAcademicYears: () => of([]) } },
        { provide: ClassService, useValue: { getAllClasses: () => of([]) } },
        { provide: SubjectService, useValue: { getSubjectsByClass: () => of([]) } },
        { provide: TeacherService, useValue: { getTeacherByUsername: () => of({}), getAllTeachers: () => of([]) } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Homework);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create the component and load saved filters', () => {
    expect(component).toBeTruthy();
    expect(mockFilterStateService.getState).toHaveBeenCalledWith('homework');
    expect(component.selectedAcademicYearId()).toBe(1);
    expect(component.selectedClassId()).toBe(2);
    expect(component.selectedSubjectId()).toBe(3);
    expect(component.studentFilterTab()).toBe('all');
  });

  it('should trigger confirmUnsubmit and set showUnsubmitConfirm to true', () => {
    component.confirmUnsubmit(101);
    expect(component.submissionIdToUnsubmit()).toBe(101);
    expect(component.showUnsubmitConfirm()).toBe(true);
  });

  it('should cancel unsubmit and reset variables', () => {
    component.confirmUnsubmit(101);
    component.cancelUnsubmit();
    expect(component.showUnsubmitConfirm()).toBe(false);
    expect(component.submissionIdToUnsubmit()).toBeNull();
  });

  it('should execute unsubmit, call API, and show success toast', () => {
    const unsubmitSpy = vi.spyOn(mockHomeworkService, 'unsubmitHomework');
    
    component.confirmUnsubmit(101);
    component.executeUnsubmit();

    expect(unsubmitSpy).toHaveBeenCalledWith(101);
    expect(mockToastService.success).toHaveBeenCalledWith('Assignment unsubmitted successfully.');
    expect(component.showUnsubmitConfirm()).toBe(false);
  });

  it('should display error toast if unsubmit API fails', () => {
    vi.spyOn(mockHomeworkService, 'unsubmitHomework').mockReturnValue(
      throwError(() => new Error('Server error'))
    );
    
    component.confirmUnsubmit(101);
    component.executeUnsubmit();

    expect(mockToastService.error).toHaveBeenCalledWith('Failed to unsubmit homework.');
  });
});
