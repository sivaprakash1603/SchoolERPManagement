import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError, Subject } from 'rxjs';
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
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

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
      getAllExams: vi.fn().mockReturnValue(of([{ id: 1, examname: 'Term 1', academicyearId: 2 }])),
      getExamSchedules: vi.fn().mockReturnValue(of([
        { id: 10, examId: 1, classId: 100, subjectId: 200, examDate: '2026-07-01T10:00:00Z', durationMinutes: 120, session: 'Morning', className: 'Class 3-A', subjectName: 'Math' }
      ])),
      createExam: vi.fn().mockReturnValue(of({})),
      createExamSchedule: vi.fn().mockReturnValue(of({})),
      updateExamSchedule: vi.fn().mockReturnValue(of({})),
      deleteExamSchedule: vi.fn().mockReturnValue(of({})),
      getExamResultsByClass: vi.fn().mockReturnValue(of([{ studentId: 50, marks: 85 }])),
      publishResult: vi.fn().mockReturnValue(of({})),
      getStudentResults: vi.fn().mockReturnValue(of([{ examId: 1, subjectId: 200, marks: 85 }]))
    };
    mockAcademicYearService = {
      getAllAcademicYears: vi.fn().mockReturnValue(of([{ id: 2, yearName: '2026-2027', isCurrent: true }]))
    };
    mockClassService = {
      getAllClasses: vi.fn().mockReturnValue(of([{ id: 100, classname: 'Class 3-A', section: 'A' }]))
    };
    mockSubjectService = {
      getAllSubjects: vi.fn().mockReturnValue(of([{ id: 200, subjectName: 'Math', description: '' }])),
      getSubjectsByClass: vi.fn().mockReturnValue(of([{ id: 200, subjectName: 'Math', description: '' }]))
    };
    mockStudentService = {
      getStudentByUserId: vi.fn().mockReturnValue(of({ id: 50, classId: 100, name: 'Student One', regNo: 'S001' })),
      getStudentsByClassId: vi.fn().mockReturnValue(of([{ id: 50, name: 'Student One', regNo: 'S001', classId: 100, parentId: 0 }]))
    };
    mockTeacherService = {
      getTeacherByUsername: vi.fn().mockReturnValue(of({ id: 10, name: 'Teacher Ten' }))
    };
    mockTimetableService = {
      getTeacherTimetable: vi.fn().mockReturnValue(of([{ classId: 100, subjectId: 200 }]))
    };
    mockParentService = {
      getParentByUserId: vi.fn().mockReturnValue(of({ id: 30 })),
      getParentChildren: vi.fn().mockReturnValue(of([{ studentId: 50, classId: 100, name: 'Child 1', className: 'Class 3-A' }])),
      selectedChildId: null
    };
    mockToastService = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn()
    };
    mockFilterStateService = {
      getState: vi.fn().mockReturnValue(undefined),
      saveState: vi.fn()
    };

    sessionStorage.clear();
    sessionStorage.setItem('role', 'Admin');
    sessionStorage.setItem('userId', '1');

    await TestBed.configureTestingModule({
      imports: [Exams, CommonModule, FormsModule],
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
  });

  afterEach(() => {
    sessionStorage.clear();
    vi.clearAllMocks();
  });

  describe('Initialization and Role Handling', () => {
    it('should restore state from FilterStateService on construct', () => {
      mockFilterStateService.getState.mockReturnValue({
        selectedAcademicYearId: 99,
        selectedExam: { id: 88, examname: 'Mock Exam' },
        selectedSchedule: { id: 77, subjectName: 'Mock Subject' }
      });
      const newFixture = TestBed.createComponent(Exams);
      const newComponent = newFixture.componentInstance;
      
      expect(newComponent.selectedAcademicYearId()).toBe(99);
      expect(newComponent.selectedExam()?.id).toBe(88);
      expect(newComponent.selectedSchedule()?.id).toBe(77);
      
      // trigger effect
      newFixture.detectChanges();
      expect(mockFilterStateService.saveState).toHaveBeenCalled();
    });

    it('should initialize Admin role correctly', () => {
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      
      expect(component.userRole()).toBe('Admin');
      expect(component.isAdminOrTeacher()).toBe(true);
      expect(component.isAdmin()).toBe(true);
      expect(mockAcademicYearService.getAllAcademicYears).toHaveBeenCalled();
      expect(mockSubjectService.getAllSubjects).toHaveBeenCalled();
    });

    it('should initialize Student role correctly and load results/exams', () => {
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      
      expect(component.studentId()).toBe(50);
      expect(component.studentClassId()).toBe(100);
      expect(mockStudentService.getStudentByUserId).toHaveBeenCalledWith(1);
      expect(mockExamService.getStudentResults).toHaveBeenCalledWith(50);
      expect(mockExamService.getAllExams).toHaveBeenCalledWith(100);
    });

    it('should handle error when fetching Student profile', () => {
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '1');
      mockStudentService.getStudentByUserId.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      fixture.detectChanges();
      
      expect(consoleSpy).toHaveBeenCalledWith('Failed to resolve student profile', expect.any(Error));
      consoleSpy.mockRestore();
    });

    it('should initialize Parent role and load children', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      
      expect(mockParentService.getParentByUserId).toHaveBeenCalledWith(1);
      expect(mockParentService.getParentChildren).toHaveBeenCalledWith(30);
      expect(component.parentChildren().length).toBe(1);
      expect(component.selectedChildId()).toBe(50);
      expect(component.studentId()).toBe(50);
      expect(component.studentClassId()).toBe(100);
    });

    it('should handle Parent role with previously selected child', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      mockParentService.selectedChildId = 50;
      fixture.detectChanges();
      expect(component.selectedChildId()).toBe(50);
    });

    it('should handle error when fetching Parent profile', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      mockParentService.getParentByUserId.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      fixture.detectChanges();
      expect(consoleSpy).toHaveBeenCalledWith('Failed to resolve parent profile', expect.any(Error));
      consoleSpy.mockRestore();
    });

    it('should handle error when fetching Parent children', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      mockParentService.getParentChildren.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      fixture.detectChanges();
      expect(consoleSpy).toHaveBeenCalledWith('Failed to load parent children', expect.any(Error));
      consoleSpy.mockRestore();
    });

    it('should initialize Teacher role and map timetable correctly', () => {
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 'T1');
      fixture.detectChanges();
      
      expect(mockTeacherService.getTeacherByUsername).toHaveBeenCalledWith('T1');
      expect(mockTimetableService.getTeacherTimetable).toHaveBeenCalledWith(10);
      expect(component.teacherClassIds()).toContain(100);
      expect(component.teacherSubjectClassMap()[100]).toContain(200);
    });

    it('should handle Teacher timetable error gracefully', () => {
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 'T1');
      mockTimetableService.getTeacherTimetable.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      fixture.detectChanges();
      expect(consoleSpy).toHaveBeenCalledWith('Failed to fetch teacher timetable', expect.any(Error));
      consoleSpy.mockRestore();
    });

    it('should handle Teacher profile error gracefully', () => {
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 'T1');
      mockTeacherService.getTeacherByUsername.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      fixture.detectChanges();
      expect(consoleSpy).toHaveBeenCalledWith('Failed to resolve teacher profile', expect.any(Error));
      consoleSpy.mockRestore();
    });
  });

  describe('Data Loading (Academic Years, Classes, Subjects, Exams, Schedules)', () => {
    beforeEach(() => {
      fixture.detectChanges(); // defaults to Admin
    });

    it('should fetch academic years and handle error', () => {
      mockAcademicYearService.getAllAcademicYears.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      component.fetchAcademicYears();
      expect(consoleSpy).toHaveBeenCalledWith('Failed to load academic years', expect.any(Error));
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to load academic sessions.');
      consoleSpy.mockRestore();
    });

    it('should fetch classes and handle error', () => {
      mockClassService.getAllClasses.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      component.fetchClasses(1);
      expect(consoleSpy).toHaveBeenCalledWith('Failed to load classes', expect.any(Error));
      consoleSpy.mockRestore();
    });

    it('should filter classes for Teacher', () => {
      component.userRole.set('Teacher');
      component.teacherClassIds.set([100]);
      mockClassService.getAllClasses.mockReturnValue(of([{ id: 100 }, { id: 101 }]));
      component.fetchClasses(1);
      expect(component.classes().length).toBe(1);
      expect(component.classes()[0].id).toBe(100);
    });

    it('should fetch subjects and handle error', () => {
      mockSubjectService.getAllSubjects.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      component.fetchSubjects();
      expect(consoleSpy).toHaveBeenCalledWith('Failed to load subjects', expect.any(Error));
      consoleSpy.mockRestore();
    });

    it('should handle exams fetch error', () => {
      mockExamService.getAllExams.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      component.fetchExams();
      expect(consoleSpy).toHaveBeenCalledWith('Failed to load exams', expect.any(Error));
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to load exams.');
      consoleSpy.mockRestore();
    });

    it('should auto select first exam or clear selection on empty exams', () => {
      mockExamService.getAllExams.mockReturnValue(of([]));
      component.selectedAcademicYearId.set(2);
      component.fetchExams();
      expect(component.selectedExam()).toBeNull();
      expect(component.schedules()).toEqual([]);
      expect(component.students()).toEqual([]);
    });

    it('should change year and reload classes and exams', () => {
      component.onYearSelect('3');
      expect(component.selectedAcademicYearId()).toBe(3);
      expect(mockClassService.getAllClasses).toHaveBeenCalledWith(3);
      expect(mockExamService.getAllExams).toHaveBeenCalled();
    });

    it('should select an exam and fetch schedules', () => {
      const exam = { id: 5, examname: 'Test', academicyearId: 2 };
      component.selectExam(exam as any);
      expect(component.selectedExam()?.id).toBe(5);
      expect(component.students()).toEqual([]);
      expect(mockExamService.getExamSchedules).toHaveBeenCalledWith(5);
    });

    it('should filter schedules for Student/Parent by classId', () => {
      component.userRole.set('Student');
      component.studentClassId.set(100);
      mockExamService.getExamSchedules.mockReturnValue(of([{ classId: 100 }, { classId: 101 }]));
      component.fetchSchedules(1);
      expect(component.schedules().length).toBe(1);
      expect(component.schedules()[0].classId).toBe(100);
    });

    it('should filter schedules for Teacher by classId and subjectId map', () => {
      component.userRole.set('Teacher');
      component.teacherSubjectClassMap.set({ 100: [200] });
      mockExamService.getExamSchedules.mockReturnValue(of([
        { classId: 100, subjectId: 200 }, // allowed
        { classId: 100, subjectId: 201 }, // not allowed subject
        { classId: 101, subjectId: 200 }  // not allowed class
      ]));
      component.fetchSchedules(1);
      expect(component.schedules().length).toBe(1);
      expect(component.schedules()[0].subjectId).toBe(200);
    });

    it('should handle schedule fetch error', () => {
      mockExamService.getExamSchedules.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      component.fetchSchedules(1);
      expect(consoleSpy).toHaveBeenCalledWith('Failed to load schedules', expect.any(Error));
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to load exam schedules.');
      consoleSpy.mockRestore();
    });

    it('should handle child change for Parent', () => {
      component.userRole.set('Parent');
      component.parentChildren.set([{ studentId: 99, classId: 101 }]);
      component.onChildChange('99');
      expect(component.selectedChildId()).toBe(99);
      expect(component.studentId()).toBe(99);
      expect(component.studentClassId()).toBe(101);
      expect(mockExamService.getStudentResults).toHaveBeenCalledWith(99);
    });

    it('should clear selection when child change yields no valid child/class', () => {
      component.parentChildren.set([{ studentId: 88 }]); // no classId
      component.onChildChange('88');
      expect(component.studentClassId()).toBeNull();
      expect(component.studentResults()).toEqual([]);
      expect(component.exams()).toEqual([]);
    });
  });

  describe('Schedule Modal and Forms', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should toggle create exam modal', () => {
      component.openCreateExamModal();
      expect(component.showCreateExamModal()).toBe(true);
      expect(component.examForm().examname).toBe('');
      
      component.closeCreateExamModal();
      expect(component.showCreateExamModal()).toBe(false);
    });

    it('should save a new exam', () => {
      component.selectedAcademicYearId.set(2);
      component.examForm.set({ examname: 'Test Exam' });
      component.saveExam();
      expect(mockExamService.createExam).toHaveBeenCalledWith({ examname: 'Test Exam', academicyearId: 2 });
      expect(mockToastService.success).toHaveBeenCalledWith('Exam created successfully.');
      expect(component.showCreateExamModal()).toBe(false);
    });

    it('should abort saving exam if name or year is missing', () => {
      component.selectedAcademicYearId.set(2);
      component.examForm.set({ examname: '   ' }); // empty
      component.saveExam();
      expect(mockExamService.createExam).not.toHaveBeenCalled();
    });

    it('should handle save exam error', () => {
      component.selectedAcademicYearId.set(2);
      component.examForm.set({ examname: 'Test' });
      mockExamService.createExam.mockReturnValue(throwError(() => ({ error: { message: 'err' } })));
      component.saveExam();
      expect(mockToastService.error).toHaveBeenCalledWith('err');
    });

    it('should handle validation errors correctly via getErrorMessage', () => {
      component.selectedAcademicYearId.set(2);
      component.examForm.set({ examname: 'Test' });
      mockExamService.createExam.mockReturnValue(throwError(() => ({ error: { errors: { field1: ['Error 1'], field2: ['Error 2'] } } })));
      component.saveExam();
      expect(mockToastService.error).toHaveBeenCalledWith('Error 1 Error 2');
    });

    it('should open schedule modal and prefill defaults', () => {
      component.classes.set([{ id: 100 } as any]);
      component.openScheduleModal();
      expect(component.showScheduleModal()).toBe(true);
      expect(component.editingScheduleId()).toBeNull();
      expect(component.scheduleForm().classId).toBe(100);
      expect(mockSubjectService.getSubjectsByClass).toHaveBeenCalledWith(100);
    });

    it('should open schedule modal and set empty subject with no class defaults', () => {
      component.classes.set([]);
      component.openScheduleModal();
      expect(component.showScheduleModal()).toBe(true);
      expect(component.editingScheduleId()).toBeNull();
      expect(component.scheduleForm().classId).toBeNull();
      expect(mockSubjectService.getSubjectsByClass).not.toHaveBeenCalled();
    });

    it('should handle modal class change and load subjects', () => {
      mockSubjectService.getSubjectsByClass.mockReturnValue(of([{ id: 200 }, { id: 201 }]));
      component.onModalClassChange(100);
      expect(component.modalSubjects().length).toBe(2);
      expect(component.scheduleForm().subjectId).toBe(200);
    });

    it('should preserve subjectId on modal class change if valid', () => {
      mockSubjectService.getSubjectsByClass.mockReturnValue(of([{ id: 200 }, { id: 201 }]));
      component.onModalClassChange(100, 201);
      expect(component.scheduleForm().subjectId).toBe(201);
    });

    it('should filter modal subjects for Teacher', () => {
      component.userRole.set('Teacher');
      component.teacherSubjectClassMap.set({ 100: [201] });
      mockSubjectService.getSubjectsByClass.mockReturnValue(of([{ id: 200 }, { id: 201 }]));
      component.onModalClassChange(100);
      expect(component.modalSubjects().length).toBe(1);
      expect(component.modalSubjects()[0].id).toBe(201);
    });

    it('should handle modal class change with no classId', () => {
      component.onModalClassChange(null);
      expect(component.modalSubjects()).toEqual([]);
      expect(component.scheduleForm().subjectId).toBeNull();
    });

    it('should handle modal class change error', () => {
      mockSubjectService.getSubjectsByClass.mockReturnValue(throwError(() => new Error('err')));
      component.onModalClassChange(100);
      expect(component.modalSubjects()).toEqual([]);
      expect(component.scheduleForm().subjectId).toBeNull();
    });

    it('should open schedule modal in edit mode', () => {
      component.editSchedule({ id: 99, classId: 100, subjectId: 200, examDate: '2026-07-01T10:00:00Z', durationMinutes: 60, session: 'Afternoon' } as any);
      expect(component.editingScheduleId()).toBe(99);
      expect(component.scheduleForm().classId).toBe(100);
      expect(component.scheduleForm().subjectId).toBe(200);
      expect(component.scheduleForm().examDate).toBe('2026-07-01');
      expect(component.scheduleForm().session).toBe('Afternoon');
      expect(component.showScheduleModal()).toBe(true);
    });

    it('should save schedule (create)', () => {
      component.selectedExam.set({ id: 1 } as any);
      component.scheduleForm.set({ classId: 100, subjectId: 200, examDate: '2026-07-01', durationMinutes: 120, session: 'Morning' });
      component.saveSchedule();
      expect(mockExamService.createExamSchedule).toHaveBeenCalled();
      expect(mockToastService.success).toHaveBeenCalledWith('Subject scheduled successfully.');
      expect(component.showScheduleModal()).toBe(false);
    });

    it('should handle schedule create error', () => {
      component.selectedExam.set({ id: 1 } as any);
      component.scheduleForm.set({ classId: 100, subjectId: 200, examDate: '2026-07-01', durationMinutes: 120, session: 'Morning' });
      mockExamService.createExamSchedule.mockReturnValue(throwError(() => ({ error: { errors: { field: ['invalid'] } } })));
      component.saveSchedule();
      expect(mockToastService.error).toHaveBeenCalledWith('invalid');
    });

    it('should save schedule (update)', () => {
      component.selectedExam.set({ id: 1 } as any);
      component.editingScheduleId.set(99);
      component.scheduleForm.set({ classId: 100, subjectId: 200, examDate: '2026-07-01', durationMinutes: 120, session: 'Morning' });
      component.saveSchedule();
      expect(mockExamService.updateExamSchedule).toHaveBeenCalledWith(99, expect.anything());
      expect(mockToastService.success).toHaveBeenCalledWith('Schedule updated successfully.');
    });

    it('should handle schedule update error', () => {
      component.selectedExam.set({ id: 1 } as any);
      component.editingScheduleId.set(99);
      component.scheduleForm.set({ classId: 100, subjectId: 200, examDate: '2026-07-01', durationMinutes: 120, session: 'Morning' });
      mockExamService.updateExamSchedule.mockReturnValue(throwError(() => new Error('err')));
      component.saveSchedule();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to update schedule.');
    });

    it('should not save schedule if required fields missing', () => {
      component.selectedExam.set({ id: 1 } as any);
      component.scheduleForm.set({ classId: 100, subjectId: null, examDate: '', durationMinutes: 120, session: 'Morning' });
      component.saveSchedule();
      expect(mockExamService.createExamSchedule).not.toHaveBeenCalled();
    });
  });

  describe('Students and Grading', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should select schedule and fetch students', () => {
      const schedule = { id: 10, examId: 1, classId: 100, subjectId: 200 } as any;
      component.selectSchedule(schedule);
      expect(component.selectedSchedule()?.id).toBe(10);
      expect(mockStudentService.getStudentsByClassId).toHaveBeenCalledWith(100);
      expect(mockExamService.getExamResultsByClass).toHaveBeenCalledWith(1, 100, 200);
      
      const uiStudents = component.students();
      expect(uiStudents.length).toBe(1);
      expect(uiStudents[0].marks).toBe(85);
      expect(uiStudents[0].isPublished).toBe(true);
    });

    it('should handle getExamResultsByClass error', () => {
      const schedule = { id: 10, examId: 1, classId: 100, subjectId: 200 } as any;
      mockExamService.getExamResultsByClass.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      component.selectSchedule(schedule);
      
      const uiStudents = component.students();
      expect(uiStudents.length).toBe(1);
      expect(uiStudents[0].marks).toBeNull();
      expect(uiStudents[0].isPublished).toBe(false);
      consoleSpy.mockRestore();
    });

    it('should handle getStudentsByClassId error', () => {
      const schedule = { id: 10, examId: 1, classId: 100, subjectId: 200 } as any;
      mockStudentService.getStudentsByClassId.mockReturnValue(throwError(() => new Error('err')));
      component.selectSchedule(schedule);
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to load students for this class.');
    });

    it('should publish marks for a student', () => {
      component.selectedExam.set({ id: 1 } as any);
      component.selectedSchedule.set({ subjectId: 200 } as any);
      const student = { id: 50, marks: 95, isPublishing: false, isPublished: false, name: 'S1' } as any;
      component.publishMarks(student);
      expect(mockExamService.publishResult).toHaveBeenCalledWith({ examId: 1, subjectId: 200, studentId: 50, marks: 95 });
      expect(student.isPublished).toBe(true);
      expect(student.isPublishing).toBe(false);
      expect(mockToastService.success).toHaveBeenCalledWith('Published marks for S1');
    });

    it('should not publish marks if student marks are null', () => {
      component.selectedExam.set({ id: 1 } as any);
      component.selectedSchedule.set({ subjectId: 200 } as any);
      const student = { id: 50, marks: null, isPublishing: false, isPublished: false } as any;
      component.publishMarks(student);
      expect(mockExamService.publishResult).not.toHaveBeenCalled();
    });

    it('should handle publish marks error', () => {
      component.selectedExam.set({ id: 1 } as any);
      component.selectedSchedule.set({ subjectId: 200 } as any);
      const student = { id: 50, marks: 95, isPublishing: false, isPublished: false } as any;
      mockExamService.publishResult.mockReturnValue(throwError(() => new Error('err')));
      
      component.publishMarks(student);
      expect(mockToastService.error).toHaveBeenCalled();
      expect(student.isPublishing).toBe(false);
    });

    it('should fetch student results and handle error', () => {
      mockExamService.getStudentResults.mockReturnValue(throwError(() => new Error('err')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      component.fetchStudentResults(50);
      expect(consoleSpy).toHaveBeenCalledWith('Failed to load student results', expect.any(Error));
      consoleSpy.mockRestore();
    });
  });

  describe('View UI Helpers and Summary', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should get correct page title and description based on role', () => {
      component.isAdminOrTeacher.set(true);
      component.userRole.set('Admin');
      expect(component.pageTitle).toBe('Exam & Results Management');
      expect(component.pageDescription).toBe('Schedule exams, publish results, and grade student submissions.');

      component.isAdminOrTeacher.set(false);
      component.userRole.set('Parent');
      expect(component.pageTitle).toBe('Child Exams & Performance');
      expect(component.pageDescription).toBe("View your child's exam schedules, subject results, and term report cards.");

      component.userRole.set('Student');
      expect(component.pageTitle).toBe('Exams & Performance');
      expect(component.pageDescription).toBe('View your exam schedules, subject results, and term report cards.');
    });

    it('should get student mark for schedule correctly', () => {
      component.studentResults.set([{ examId: 1, subjectId: 200, marks: 85 }]);
      expect(component.getStudentMarkForSchedule({ examId: 1, subjectId: 200 } as any)).toBe('85');
      
      // Found but marks null
      component.studentResults.set([{ examId: 1, subjectId: 200, marks: null }]);
      expect(component.getStudentMarkForSchedule({ examId: 1, subjectId: 200 } as any)).toBe('N/A');
      
      // Not found
      expect(component.getStudentMarkForSchedule({ examId: 2, subjectId: 200 } as any)).toBe('Pending');
    });

    it('should calculate exam performance summary accurately', () => {
      component.schedules.set([
        { examId: 1, subjectId: 100 } as any, // 95
        { examId: 1, subjectId: 101 } as any, // 35 (fail)
        { examId: 1, subjectId: 102 } as any  // no results
      ]);
      component.studentResults.set([
        { examId: 1, subjectId: 100, marks: 95 },
        { examId: 1, subjectId: 101, marks: 35 }
      ]);
      
      const summary = component.getExamPerformanceSummary();
      expect(summary).not.toBeNull();
      expect(summary?.totalObtained).toBe(130); // 95 + 35
      expect(summary?.totalPossible).toBe(200); // 2 graded subjects
      expect(summary?.percentage).toBe(65);
      expect(summary?.grade).toBe('C');
      expect(summary?.passCount).toBe(1);
      expect(summary?.failCount).toBe(1);
    });

    it('should return null for summary if no schedules or no grades', () => {
      component.schedules.set([]);
      expect(component.getExamPerformanceSummary()).toBeNull();

      component.schedules.set([{ examId: 1, subjectId: 100 } as any]);
      component.studentResults.set([]);
      expect(component.getExamPerformanceSummary()).toBeNull();
    });
    
    it('should assign correct grades based on percentage', () => {
      component.schedules.set([{ examId: 1, subjectId: 100 } as any]);
      
      const testGrade = (marks: number) => {
        component.studentResults.set([{ examId: 1, subjectId: 100, marks }]);
        return component.getExamPerformanceSummary()?.grade;
      };

      expect(testGrade(95)).toBe('A+');
      expect(testGrade(85)).toBe('A');
      expect(testGrade(75)).toBe('B');
      expect(testGrade(65)).toBe('C');
      expect(testGrade(55)).toBe('D');
      expect(testGrade(45)).toBe('E');
      expect(testGrade(35)).toBe('F');
    });
  });

  describe('DOM Rendering', () => {
    it('should render loading state initially', () => {
      // Prevent automatic data loading from completing synchronously
      mockExamService.getAllExams.mockReturnValue(new Subject());
      fixture.detectChanges(); // Triggers ngOnInit, loadingExams stays true
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Preparing exam rosters...');
    });

    it('should render Admin UI with Create Exam button', () => {
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      component.loadingExams.set(false);
      component.loadingSchedules.set(false);
      component.loadingStudents.set(false);
      component.selectedAcademicYearId.set(2);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Add New Exam / Term');
      expect(compiled.textContent).toContain('Term 1'); // Exam name in the list
    });

    it('should render left panel message when no exam selected', () => {
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      component.loadingExams.set(false);
      component.selectedAcademicYearId.set(2);
      component.selectedExam.set(null);
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Please select an exam from the left panel to manage schedules and grades.');
    });

    it('should render Student UI with Summary Card and Grades', () => {
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      
      component.loadingExams.set(false);
      component.loadingSchedules.set(false);
      component.loadingStudents.set(false);
      component.selectedAcademicYearId.set(2);
      component.selectedExam.set({ id: 1, examname: 'Term 1' } as any);
      component.schedules.set([{ id: 10, examId: 1, classId: 100, subjectId: 200, subjectName: 'Math', examDate: '2026-07-01T10:00:00Z', durationMinutes: 120, session: 'Morning' } as any]);
      component.studentResults.set([{ examId: 1, subjectId: 200, marks: 85 }]);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('OVERALL AVERAGE');
      expect(compiled.textContent).toContain('85%');
      expect(compiled.textContent).toContain('Math');
      expect(compiled.textContent).toContain('85'); // Marks in the row
    });

    it('should render empty exam list', () => {
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      component.loadingExams.set(false);
      component.selectedAcademicYearId.set(2);
      component.exams.set([]);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('No exams registered for this session yet.');
    });

    it('should render empty schedules list', () => {
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      component.loadingExams.set(false);
      component.loadingSchedules.set(false);
      component.selectedAcademicYearId.set(2);
      component.selectedExam.set({ id: 1, examname: 'Term 1' } as any);
      component.schedules.set([]);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('No subjects scheduled for this exam yet.');
    });

    it('should render grade sheet in Admin/Teacher mode', () => {
      sessionStorage.setItem('role', 'Teacher');
      fixture.detectChanges();
      component.loadingExams.set(false);
      component.loadingSchedules.set(false);
      component.loadingStudents.set(false);
      component.selectedAcademicYearId.set(2);
      component.selectedExam.set({ id: 1, examname: 'Term 1' } as any);
      component.selectedSchedule.set({ id: 10, className: 'Class 3-A', subjectName: 'Math' } as any);
      component.students.set([
        { id: 50, name: 'Student One', regNo: 'S001', marks: 95, isPublished: true, isPublishing: false } as any,
        { id: 51, name: 'Student Two', regNo: 'S002', marks: null, isPublished: false, isPublishing: true } as any
      ]);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toMatch(/Grade Sheet:\s*Class 3-A\s*-\s*Math/);
      expect(compiled.textContent).toContain('Student One');
      expect(compiled.textContent).toContain('Update Marks');
      expect(compiled.textContent).toContain('Student Two'); // The one with spinner
    });

    it('should render empty students list in grade sheet', () => {
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      component.loadingExams.set(false);
      component.loadingSchedules.set(false);
      component.loadingStudents.set(false);
      component.selectedAcademicYearId.set(2);
      component.selectedExam.set({ id: 1, examname: 'Term 1' } as any);
      component.selectedSchedule.set({ id: 10, className: 'Class 3-A', subjectName: 'Math' } as any);
      component.students.set([]);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('No students enrolled in this class.');
    });

    it('should render Create Exam Modal', () => {
      component.showCreateExamModal.set(true);
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Create Assessment Term');
    });

    it('should render Schedule Modal and saving state', () => {
      component.showScheduleModal.set(true);
      component.isSavingSchedule.set(true);
      component.editingScheduleId.set(99);
      component.classes.set([{ id: 100, classname: 'Class 1' } as any]);
      component.modalSubjects.set([{ id: 200, subjectName: 'Math' } as any]);
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Edit Exam Schedule');
      expect(compiled.innerHTML).toContain('spinner-border');
    });
    
    it('should render parent child select', () => {
      sessionStorage.setItem('role', 'Parent');
      fixture.detectChanges();
      component.parentChildren.set([{ studentId: 50, name: 'Child 1', className: 'Class 1' }]);
      fixture.detectChanges();
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Select Child:');
      expect(compiled.textContent).toContain('Child 1 (Class 1)');
    });
  });
});
