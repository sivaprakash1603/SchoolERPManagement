import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { Exams } from './exams';
import { ExamService } from '../../services/exam.service';
import { AcademicYearService } from '../../services/academic-year.service';
import { ClassService } from '../../services/class.service';
import { SubjectService } from '../../services/subject.service';
import { StudentService } from '../../services/student.service';
import { ToastService } from '../../services/toast.service';
import { TeacherService } from '../../services/teacher.service';
import { TimetableService } from '../../services/timetable.service';
import { ParentService } from '../../services/parent.service';
import { FilterStateService } from '../../services/filter-state.service';

describe('Exams', () => {
  let component: Exams;
  let fixture: ComponentFixture<Exams>;
  let mockExamService: any;
  let mockAcademicYearService: any;
  let mockClassService: any;
  let mockSubjectService: any;
  let mockStudentService: any;
  let mockToastService: any;
  let mockTeacherService: any;
  let mockTimetableService: any;
  let mockParentService: any;
  let mockFilterStateService: any;

  beforeEach(async () => {
    mockExamService = {
      getAllExams: () => of([{ id: 1, examname: 'Term 1', academicyearId: 2 }]),
      getExamSchedules: () => of([{ id: 10, examId: 1, classId: 100, subjectId: 200, examDate: '2026-07-01', durationMinutes: 120, session: 'Morning' }]),
      createExam: () => of({}),
      createExamSchedule: () => of({}),
      deleteExamSchedule: () => of({})
    };
    mockAcademicYearService = {
      getAllAcademicYears: () => of([{ id: 2, yearName: '2026-2027', isCurrent: true }])
    };
    mockClassService = {
      getAllClasses: () => of([{ id: 100, classname: 'Class 3-A', section: 'A' }]),
      getClassesByAcademicYearId: () => of([{ id: 100, classname: 'Class 3-A', section: 'A' }])
    };
    mockSubjectService = {
      getAllSubjects: () => of([{ id: 200, subjectName: 'Math', description: '' }]),
      getSubjectsByClass: () => of([{ id: 200, subjectName: 'Math', description: '' }])
    };
    mockStudentService = {
      getStudentByUserId: () => of({ id: 50, classId: 100, name: 'Student One' }),
      getStudentsByClassId: () => of([{ id: 50, name: 'Student One', admissionnumber: 'ST001', rollnumber: 1, classId: 100, parentId: 0 }])
    };
    mockTeacherService = {
      getTeacherByUsername: () => of({ id: 10, name: 'Teacher Ten' })
    };
    mockTimetableService = {
      getTeacherTimetable: () => of([{ classId: 100, subjectId: 200 }])
    };
    mockParentService = {
      getParentByUserId: () => of({ id: 30 }),
      getParentChildren: () => of([{ studentId: 50, classId: 100 }])
    };
    mockToastService = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn()
    };
    mockFilterStateService = {
      getState: () => ({}),
      saveState: vi.fn()
    };

    sessionStorage.setItem('role', 'Admin');
    sessionStorage.setItem('userId', '1');

    await TestBed.configureTestingModule({
      imports: [Exams],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ExamService, useValue: mockExamService },
        { provide: AcademicYearService, useValue: mockAcademicYearService },
        { provide: ClassService, useValue: mockClassService },
        { provide: SubjectService, useValue: mockSubjectService },
        { provide: StudentService, useValue: mockStudentService },
        { provide: TeacherService, useValue: mockTeacherService },
        { provide: TimetableService, useValue: mockTimetableService },
        { provide: ParentService, useValue: mockParentService },
        { provide: ToastService, useValue: mockToastService },
        { provide: FilterStateService, useValue: mockFilterStateService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Exams);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  afterEach(() => {
    sessionStorage.clear();
  });

  it('should create and load dashboard', () => {
    expect(component).toBeTruthy();
    fixture.detectChanges();
    expect(component.exams().length).toBe(1);
    expect(component.academicYears().length).toBe(1);
  });

  it('should toggle schedule modals', () => {
    expect(component.showScheduleModal()).toBe(false);
    component.openScheduleModal();
    expect(component.showScheduleModal()).toBe(true);

    component.closeScheduleModal();
    expect(component.showScheduleModal()).toBe(false);
  });

  it('should create new exam successfully', () => {
    component.examForm.set({ examname: 'Final Exams' });
    component.saveExam();
    expect(mockToastService.success).toHaveBeenCalledWith('Exam created successfully.');
  });

  it('should schedule exam successfully', () => {
    component.selectedExam.set({ id: 1, examname: 'Term 1', academicyearId: 2 });
    component.scheduleForm.set({
      classId: 100,
      subjectId: 200,
      examDate: '2026-07-01',
      durationMinutes: 120,
      session: 'Morning'
    });

    component.saveSchedule();
    expect(mockToastService.success).toHaveBeenCalledWith('Subject scheduled successfully.');
  });
});
