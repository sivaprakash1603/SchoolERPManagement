import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { Timetable } from './timetable';
import { TimetableService } from '../../services/timetable.service';
import { ClassService } from '../../services/class.service';
import { SubjectService } from '../../services/subject.service';
import { TeacherService } from '../../services/teacher.service';
import { AcademicYearService } from '../../services/academic-year.service';
import { StudentService } from '../../services/student.service';
import { ParentService } from '../../services/parent.service';
import { ToastService } from '../../services/toast.service';
import { FilterStateService } from '../../services/filter-state.service';

describe('Timetable', () => {
  let component: Timetable;
  let fixture: ComponentFixture<Timetable>;

  let timetableServiceMock: any;
  let classServiceMock: any;
  let subjectServiceMock: any;
  let teacherServiceMock: any;
  let academicYearServiceMock: any;
  let studentServiceMock: any;
  let parentServiceMock: any;
  let toastServiceMock: any;
  let filterStateServiceMock: any;

  const mockClasses = [{ id: 1, classname: 'Grade 1', section: 'A' }];
  const mockSubjects = [{ id: 1, subjectName: 'Math' }];
  const mockTeachers = [{ id: 1, name: 'Mr. Smith' }];
  const mockYears = [{ id: 1, name: '2024-2025', isCurrent: true }];
  const mockSlots = [{ id: 1, classId: 1, subjectId: 1, teacherId: 1, dayOfWeek: 'Monday', startTime: '09:00', endTime: '10:00', roomNo: '101' }];

  beforeEach(async () => {
    timetableServiceMock = {
      getClassTimetable: vi.fn().mockReturnValue(of(mockSlots)),
      getTeacherTimetable: vi.fn().mockReturnValue(of(mockSlots)),
      createTimetable: vi.fn().mockReturnValue(of({})),
      updateTimetableSlot: vi.fn().mockReturnValue(of({})),
      generateTimetable: vi.fn().mockReturnValue(of(mockSlots)),
      saveGeneratedTimetable: vi.fn().mockReturnValue(of({}))
    };

    classServiceMock = { getAllClasses: vi.fn().mockReturnValue(of(mockClasses)) };
    subjectServiceMock = { getAllSubjects: vi.fn().mockReturnValue(of(mockSubjects)) };
    teacherServiceMock = { 
      getAllTeachers: vi.fn().mockReturnValue(of({ items: mockTeachers })),
      getTeacherByUsername: vi.fn().mockReturnValue(of({ id: 1, name: 'Mr. Smith' }))
    };
    academicYearServiceMock = { getAllAcademicYears: vi.fn().mockReturnValue(of(mockYears)) };
    studentServiceMock = { getStudentByUserId: vi.fn().mockReturnValue(of({ id: 1, classId: 1 })) };
    parentServiceMock = {
      getParentByUserId: vi.fn().mockReturnValue(of({ id: 1 })),
      getParentChildren: vi.fn().mockReturnValue(of([{ studentId: 101, classId: 1 }])),
      selectedChildId: null
    };
    toastServiceMock = { success: vi.fn(), warning: vi.fn(), error: vi.fn() };
    filterStateServiceMock = {
      getState: vi.fn().mockReturnValue(null),
      saveState: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [Timetable, FormsModule],
      providers: [
        provideRouter([]),
        { provide: TimetableService, useValue: timetableServiceMock },
        { provide: ClassService, useValue: classServiceMock },
        { provide: SubjectService, useValue: subjectServiceMock },
        { provide: TeacherService, useValue: teacherServiceMock },
        { provide: AcademicYearService, useValue: academicYearServiceMock },
        { provide: StudentService, useValue: studentServiceMock },
        { provide: ParentService, useValue: parentServiceMock },
        { provide: ToastService, useValue: toastServiceMock },
        { provide: FilterStateService, useValue: filterStateServiceMock }
      ]
    }).compileComponents();
  });

  afterEach(() => {
    sessionStorage.clear();
    vi.restoreAllMocks();
  });

  function createComponent() {
    fixture = TestBed.createComponent(Timetable);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  describe('Initialization by Role', () => {
    it('should initialize for Admin', () => {
      sessionStorage.setItem('role', 'Admin');
      createComponent();
      expect(component.userRole()).toBe('Admin');
      expect(component.isAdmin()).toBe(true);
      expect(component.viewMode()).toBe('class');
      expect(academicYearServiceMock.getAllAcademicYears).toHaveBeenCalled();
    });

    it('should initialize for Teacher', () => {
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 'smith');
      createComponent();
      expect(component.userRole()).toBe('Teacher');
      expect(component.viewMode()).toBe('personal');
      expect(teacherServiceMock.getTeacherByUsername).toHaveBeenCalledWith('smith');
      expect(timetableServiceMock.getTeacherTimetable).toHaveBeenCalledWith(1);
    });

    it('should initialize for Student', () => {
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '10');
      createComponent();
      expect(component.userRole()).toBe('Student');
      expect(studentServiceMock.getStudentByUserId).toHaveBeenCalledWith(10);
      expect(timetableServiceMock.getClassTimetable).toHaveBeenCalledWith(1);
    });

    it('should initialize for Parent', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '20');
      createComponent();
      expect(component.userRole()).toBe('Parent');
      expect(parentServiceMock.getParentByUserId).toHaveBeenCalledWith(20);
      expect(parentServiceMock.getParentChildren).toHaveBeenCalledWith(1);
      expect(timetableServiceMock.getClassTimetable).toHaveBeenCalledWith(1);
    });
  });

  describe('Component Logic & Helpers', () => {
    beforeEach(() => {
      sessionStorage.setItem('role', 'Admin');
      createComponent();
    });

    it('should handle filter state restore', () => {
      filterStateServiceMock.getState.mockReturnValueOnce({ selectedAcademicYearId: 2, selectedClassId: 2, viewMode: 'personal' });
      // Need to re-create to trigger constructor
      fixture = TestBed.createComponent(Timetable);
      component = fixture.componentInstance;
      expect(component.selectedAcademicYearId()).toBe(2);
      expect(component.selectedClassId()).toBe(2);
      expect(component.viewMode()).toBe('personal');
    });

    it('should get page title and descriptions', () => {
      component.viewMode.set('class');
      expect(component.pageTitle).toBe('Class Timetable');
      expect(component.pageDescription).toBe('View and configure weekly schedules for institutional classes.');

      component.viewMode.set('personal');
      component.userRole.set('Teacher');
      expect(component.pageTitle).toBe('My Schedule');
      expect(component.pageDescription).toBe('View your personalized weekly teaching schedule.');

      component.userRole.set('Parent');
      expect(component.pageTitle).toBe('Child Timetable');
      expect(component.pageDescription).toBe("View your child's classroom schedule.");
      
      component.userRole.set('Student');
      expect(component.pageTitle).toBe('My Timetable');
      expect(component.pageDescription).toBe('View your classroom schedule.');
    });

    it('should initialize and update period timings', () => {
      component.initializeTimings(2);
      expect(component.periodTimings().length).toBe(2);
      expect(component.periodTimings()[0].startTime).toBe('09:00');

      component.updateTiming(0, 'startTime', '08:00');
      expect(component.periodTimings()[0].startTime).toBe('08:00');
      
      // Update via change
      component.onPeriodsPerDayChange(3);
      expect(component.periodTimings().length).toBe(3);
    });

    it('should resolve names', () => {
      expect(component.getClassNameForSlot(1)).toBe('Grade 1 - A');
      expect(component.getClassNameForSlot(99)).toBe('Class #99');
      expect(component.getSubjectName(1)).toBe('Math');
      expect(component.getSubjectName(99)).toBe('Unknown');
      expect(component.getTeacherName(1)).toBe('Mr. Smith');
      expect(component.getTeacherName(99)).toBe('Unknown');
    });

    it('should format times', () => {
      expect(component.formatTime('14:30:00')).toBe('02:30 PM');
      expect(component.formatTime('09:15:00')).toBe('09:15 AM');
      expect(component.formatTime('')).toBe('');
    });
    
    it('should handle view mode switch', () => {
      component.teacherId.set(1);
      component.selectedClassId.set(1);
      
      component.switchViewMode('personal');
      expect(timetableServiceMock.getTeacherTimetable).toHaveBeenCalled();
      
      component.switchViewMode('class');
      expect(timetableServiceMock.getClassTimetable).toHaveBeenCalled();
    });

    it('should handle initialization errors', () => {
      // Classes error
      classServiceMock.getAllClasses.mockReturnValueOnce(throwError(() => new Error('error')));
      component.fetchClasses();
      expect(toastServiceMock.error).toHaveBeenCalledWith('Failed to load classes.');
      
      // Timetable errors
      timetableServiceMock.getClassTimetable.mockReturnValueOnce(throwError(() => new Error('error')));
      component.fetchTimetable();
      expect(component.error()).toBe('Failed to load class timetable.');

      timetableServiceMock.getTeacherTimetable.mockReturnValueOnce(throwError(() => new Error('error')));
      component.fetchTeacherTimetable(1);
      expect(component.error()).toBe('Failed to load teaching timetable.');
    });
  });

  describe('CRUD Operations', () => {
    beforeEach(() => {
      sessionStorage.setItem('role', 'Admin');
      createComponent();
    });

    it('should open and close create modal', () => {
      component.openCreateModal();
      expect(component.showCreateModal()).toBe(true);
      expect(component.createForm().subjectId).toBe(1);

      component.closeCreateModal();
      expect(component.showCreateModal()).toBe(false);
    });

    it('should save timetable entry successfully', () => {
      component.selectedClassId.set(1);
      component.openCreateModal();
      component.saveTimetableEntry();
      expect(timetableServiceMock.createTimetable).toHaveBeenCalled();
      expect(toastServiceMock.success).toHaveBeenCalled();
    });

    it('should block save if fields are missing', () => {
      component.selectedClassId.set(1);
      component.openCreateModal();
      component.createForm.set({ ...component.createForm(), subjectId: null });
      component.saveTimetableEntry();
      expect(toastServiceMock.warning).toHaveBeenCalled();
      expect(timetableServiceMock.createTimetable).not.toHaveBeenCalled();
    });

    it('should open and close edit modal', () => {
      component.isAdmin.set(true);
      component.openEditModal(mockSlots[0] as any);
      expect(component.showEditModal()).toBe(true);
      expect(component.editForm().subjectId).toBe(1);

      component.closeEditModal();
      expect(component.showEditModal()).toBe(false);
    });

    it('should save edited slot', () => {
      component.isAdmin.set(true);
      component.openEditModal(mockSlots[0] as any);
      component.saveEditSlot();
      expect(timetableServiceMock.updateTimetableSlot).toHaveBeenCalledWith(1, expect.any(Object));
      expect(toastServiceMock.success).toHaveBeenCalled();
    });

    it('should handle errors on save and edit', () => {
      component.selectedClassId.set(1);
      component.openCreateModal();
      timetableServiceMock.createTimetable.mockReturnValueOnce(throwError(() => ({ error: { message: 'create err' } })));
      component.saveTimetableEntry();
      expect(toastServiceMock.error).toHaveBeenCalledWith('create err');

      component.isAdmin.set(true);
      component.openEditModal(mockSlots[0] as any);
      timetableServiceMock.updateTimetableSlot.mockReturnValueOnce(throwError(() => ({ error: { message: 'edit err' } })));
      component.saveEditSlot();
      expect(toastServiceMock.error).toHaveBeenCalledWith('edit err');
    });

    it('should block edit save if fields missing', () => {
      component.isAdmin.set(true);
      component.openEditModal(mockSlots[0] as any);
      component.editForm.set({ ...component.editForm(), subjectId: null });
      component.saveEditSlot();
      expect(toastServiceMock.warning).toHaveBeenCalledWith('Subject and Teacher are required.');
    });
  });

  describe('Generator', () => {
    beforeEach(() => {
      sessionStorage.setItem('role', 'Admin');
      createComponent();
    });

    it('should open and close generator modal', () => {
      component.openGeneratorModal();
      expect(component.showGeneratorModal()).toBe(true);
      
      component.closeGeneratorModal();
      expect(component.showGeneratorModal()).toBe(false);
    });

    it('should generate timetable and save it', () => {
      component.openGeneratorModal();
      
      // Test loading state HTML
      component.isGenerating.set(true);
      fixture.detectChanges();
      expect(fixture.nativeElement.innerHTML).toContain('Generating timetable using constraints...');
      component.isGenerating.set(false);

      component.generateTimetable();
      expect(timetableServiceMock.generateTimetable).toHaveBeenCalled();
      expect(component.generatedPreview().length).toBe(1);
      
      component.saveGeneratedTimetable();
      expect(timetableServiceMock.saveGeneratedTimetable).toHaveBeenCalled();
      expect(toastServiceMock.success).toHaveBeenCalled();
    });

    it('should generate timetable when classId is null', () => {
      component.selectedClassId.set(null);
      component.openGeneratorModal();
      component.generateTimetable();
      expect(component.slots()).toEqual([]);
    });

    it('should handle generator errors', () => {
      component.openGeneratorModal();
      
      timetableServiceMock.generateTimetable.mockReturnValueOnce(throwError(() => ({ error: { message: 'gen err' } })));
      component.generateTimetable();
      expect(toastServiceMock.error).toHaveBeenCalledWith('gen err');
      
      timetableServiceMock.saveGeneratedTimetable.mockReturnValueOnce(throwError(() => new Error('err')));
      component.saveGeneratedTimetable();
      expect(toastServiceMock.error).toHaveBeenCalledWith('Failed to save timetable.');
    });
  });

  describe('HTML Coverages', () => {
    beforeEach(() => {
      sessionStorage.setItem('role', 'Admin');
      createComponent();
    });

    it('should cover structural UI elements like empty states and errors', () => {
      component.loading.set(true);
      fixture.detectChanges();
      expect(fixture.nativeElement.innerHTML).toContain('Scheduling class slots...');
      
      component.loading.set(false);
      component.error.set('Failed to load');
      fixture.detectChanges();
      expect(fixture.nativeElement.innerHTML).toContain('Failed to load');
      
      const retryBtn = fixture.nativeElement.querySelector('.text-center button.btn-outline-primary');
      if (retryBtn) retryBtn.dispatchEvent(new Event('click'));
      
      component.error.set(null);
      component.slots.set([]);
      fixture.detectChanges();
      expect(fixture.nativeElement.innerHTML).toContain('No classes scheduled');
    });

    it('should cover admin buttons, view switchers, and dropdowns', () => {
      // Admin specific generate/create buttons
      const createBtn = fixture.nativeElement.querySelector('button.btn-primary');
      if (createBtn) createBtn.dispatchEvent(new Event('click'));
      expect(component.showCreateModal()).toBe(true);
      
      // Select dropdowns
      const selects = fixture.nativeElement.querySelectorAll('select.form-select');
      if (selects.length > 0) {
        selects[0].dispatchEvent(new Event('change'));
      }
      
      // View swapper
      const toggleClassBtn = fixture.nativeElement.querySelector('.btn-group button.btn-outline-primary:nth-child(1)');
      if (toggleClassBtn) toggleClassBtn.dispatchEvent(new Event('click'));
      expect(component.viewMode()).toBe('class');
    });

    it('should cover modal clicks and loading states', () => {
      component.openCreateModal();
      component.isSaving.set(true);
      fixture.detectChanges();
      expect(fixture.nativeElement.innerHTML).toContain('Saving...');
      
      const closeX = fixture.nativeElement.querySelector('.modal-header-custom .btn-close');
      if (closeX) closeX.dispatchEvent(new Event('click'));
      expect(component.showCreateModal()).toBe(false);

      component.openEditModal(mockSlots[0] as any);
      component.isSaving.set(true);
      fixture.detectChanges();
      
      const cancelBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-light');
      if (cancelBtn) cancelBtn.dispatchEvent(new Event('click'));
      expect(component.showEditModal()).toBe(false);
    });

    it('should cover generator modal form inputs and buttons', () => {
      component.openGeneratorModal();
      fixture.detectChanges();
      
      // Simulate input changes
      const numInputs = fixture.nativeElement.querySelectorAll('input[type="number"]');
      if (numInputs.length >= 2) {
        numInputs[0].value = '9';
        numInputs[0].dispatchEvent(new Event('input')); // (ngModelChange)
        
        numInputs[1].value = '1';
        numInputs[1].dispatchEvent(new Event('input')); // (ngModelChange)
      }
      expect(component.generatorForm().periodsPerDay).toBe(9);
      
      // Generate btn
      const genBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-primary');
      if (genBtn) genBtn.dispatchEvent(new Event('click'));
      expect(timetableServiceMock.generateTimetable).toHaveBeenCalled();
      
      // Save gen btn
      component.isGenerating.set(false);
      component.isSavingGenerated.set(true);
      fixture.detectChanges();
      
      const saveGenBtn = fixture.nativeElement.querySelector('.btn-success');
      expect(saveGenBtn).toBeTruthy();
      if (saveGenBtn) saveGenBtn.dispatchEvent(new Event('click'));
    });
    
    it('should cover parent child dropdown', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '20');
      // trigger reinit for parent
      fixture = TestBed.createComponent(Timetable);
      component = fixture.componentInstance;
      fixture.detectChanges();
      
      expect(component.userRole()).toBe('Parent');
      expect(component.parentChildren().length).toBeGreaterThan(0);
      
      const childSelect = fixture.nativeElement.querySelector('.minimal-card select');
      if (childSelect) {
        childSelect.value = '101';
        childSelect.dispatchEvent(new Event('change'));
      }
      expect(component.selectedChildId()).toBe(101);
    });
  });
});
