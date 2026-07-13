import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { Classes } from './classes';
import { ClassService } from '../../services/class.service';
import { TeacherService } from '../../services/teacher.service';
import { AcademicYearService } from '../../services/academic-year.service';
import { SubjectService } from '../../services/subject.service';
import { ToastService } from '../../services/toast.service';

const makeClass = (overrides: any = {}) => ({
  id: 1,
  classname: 'Class 1',
  section: 'A',
  classteacherId: 10,
  academicyearId: 2,
  studentCount: 25,
  subjects: [{ id: 5, subjectName: 'Math' }],
  ...overrides
});

describe('Classes', () => {
  let component: Classes;
  let fixture: ComponentFixture<Classes>;
  let mockClassService: any;
  let mockTeacherService: any;
  let mockAcademicYearService: any;
  let mockSubjectService: any;
  let mockToastService: any;

  beforeEach(async () => {
    mockClassService = {
      getAllClasses: () => of([makeClass()]),
      createClass: (dto: any) => of({}),
      updateClass: (id: number, dto: any) => of({}),
      deleteClass: (id: number) => of({})
    };
    mockTeacherService = {
      getAllTeachers: () => of({
        items: [
          { id: 10, name: 'Teacher Ten', username: 't10', joiningdate: new Date(), phonenumber: '123', userId: 100 },
          { id: 11, name: 'Teacher Eleven', username: 't11', joiningdate: new Date(), phonenumber: '456', userId: 101 }
        ]
      })
    };
    mockAcademicYearService = {
      getAllAcademicYears: () => of([
        { id: 2, yearName: '2026-2027', isCurrent: true, startDate: '2026-06-01', endDate: '2027-05-31' },
        { id: 3, yearName: '2027-2028', isCurrent: false, startDate: '2027-06-01', endDate: '2028-05-31' }
      ])
    };
    mockSubjectService = {
      getAllSubjects: () => of([
        { id: 5, subjectName: 'Math', description: '' },
        { id: 6, subjectName: 'Science', description: '' }
      ])
    };
    mockToastService = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn()
    };

    sessionStorage.clear();
    sessionStorage.setItem('role', 'Admin');

    await TestBed.configureTestingModule({
      imports: [Classes],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ClassService, useValue: mockClassService },
        { provide: TeacherService, useValue: mockTeacherService },
        { provide: AcademicYearService, useValue: mockAcademicYearService },
        { provide: SubjectService, useValue: mockSubjectService },
        { provide: ToastService, useValue: mockToastService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Classes);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  afterEach(() => {
    sessionStorage.clear();
  });

  // ─── Initialization ──────────────────────────────────────────────────────────

  it('should create and load initial data for Admin', () => {
    expect(component).toBeTruthy();
    fixture.detectChanges();
    expect(component.isAdmin()).toBe(true);
    expect(component.userRole()).toBe('Admin');
    expect(component.classes().length).toBe(1);
    expect(component.teachers().length).toBe(2);
    expect(component.subjects().length).toBe(2);
    expect(component.academicYears().length).toBe(2);
    expect(component.selectedAcademicYearId()).toBe(2);
  });

  it('should init as non-admin for Student role', () => {
    sessionStorage.setItem('role', 'Student');
    component.ngOnInit();
    expect(component.isAdmin()).toBe(false);
    expect(component.userRole()).toBe('Student');
  });

  it('should select first year when no current year is marked', () => {
    vi.spyOn(mockAcademicYearService, 'getAllAcademicYears').mockReturnValue(of([
      { id: 3, yearName: '2027-2028', isCurrent: false, startDate: '2027-06-01', endDate: '2028-05-31' }
    ]));
    component.selectedAcademicYearId.set(null as any);
    component.fetchAcademicYears();
    expect(component.selectedAcademicYearId()).toBe(3);
  });

  it('should sync createForm academicyearId when a yearId is already saved', () => {
    component.selectedAcademicYearId.set(3);
    component.fetchAcademicYears();
    expect(component.createForm().academicyearId).toBe(3);
  });

  // ─── Filter State ─────────────────────────────────────────────────────────────

  it('should save and restore filter state from sessionStorage', () => {
    component.selectedAcademicYearId.set(3);
    component.saveFilterState();
    const stored = JSON.parse(sessionStorage.getItem('classes_filter_state')!);
    expect(stored.selectedAcademicYearId).toBe(3);

    // Now restore
    component.selectedAcademicYearId.set(null as any);
    component.loadFilterState();
    expect(component.selectedAcademicYearId()).toBe(3);
  });

  it('should handle corrupt filter state JSON gracefully', () => {
    sessionStorage.setItem('classes_filter_state', 'NOT_JSON');
    expect(() => component.loadFilterState()).not.toThrow();
  });

  it('should not restore filter state when selectedAcademicYearId is undefined in saved state', () => {
    sessionStorage.setItem('classes_filter_state', JSON.stringify({ selectedAcademicYearId: null }));
    component.selectedAcademicYearId.set(2);
    component.loadFilterState();
    // should not change since saved value is null/undefined
    expect(component.selectedAcademicYearId()).toBe(2);
  });

  // ─── fetchAcademicYears ───────────────────────────────────────────────────────

  it('should handle fetchAcademicYears error', () => {
    vi.spyOn(mockAcademicYearService, 'getAllAcademicYears').mockReturnValue(throwError(() => new Error('Year fail')));
    component.fetchAcademicYears();
    expect(component.error()).toBe('Failed to load academic sessions.');
  });

  // ─── fetchClasses ─────────────────────────────────────────────────────────────

  it('should early-return fetchClasses when no yearId', () => {
    component.selectedAcademicYearId.set(null as any);
    const spy = vi.spyOn(mockClassService, 'getAllClasses');
    component.fetchClasses();
    expect(spy).not.toHaveBeenCalled();
    expect(component.loading()).toBe(false);
  });

  it('should handle fetchClasses error', () => {
    component.selectedAcademicYearId.set(2);
    vi.spyOn(mockClassService, 'getAllClasses').mockReturnValue(throwError(() => new Error('Class fail')));
    component.fetchClasses();
    expect(component.error()).toBe('Failed to load classes.');
  });

  // ─── fetchTeachers / fetchSubjects ───────────────────────────────────────────

  it('should handle fetchTeachers error silently', () => {
    vi.spyOn(mockTeacherService, 'getAllTeachers').mockReturnValue(throwError(() => new Error('Teacher fail')));
    expect(() => component.fetchTeachers()).not.toThrow();
  });

  it('should handle fetchSubjects error silently', () => {
    vi.spyOn(mockSubjectService, 'getAllSubjects').mockReturnValue(throwError(() => new Error('Subject fail')));
    expect(() => component.fetchSubjects()).not.toThrow();
  });

  // ─── Helper Methods ───────────────────────────────────────────────────────────

  it('should get teacher name correctly', () => {
    fixture.detectChanges();
    expect(component.getTeacherName(10)).toBe('Teacher Ten');
    expect(component.getTeacherName(99)).toBe('Unknown');
    expect(component.getTeacherName(undefined)).toBe('Not Assigned');
    expect(component.getTeacherName(0)).toBe('Not Assigned');
  });

  it('should get year name correctly', () => {
    fixture.detectChanges();
    expect(component.getYearName(2)).toBe('2026-2027');
    expect(component.getYearName(99)).toBe('N/A');
    expect(component.getYearName(undefined)).toBe('N/A');
  });

  it('should get available teachers for create excluding assigned ones', () => {
    fixture.detectChanges();
    component.classes.set([makeClass({ classteacherId: 10 })]);
    const available = component.getAvailableTeachersForCreate();
    expect(available.map(t => t.id)).not.toContain(10);
    expect(available.map(t => t.id)).toContain(11);
  });

  it('should get available teachers for edit excluding others but including current', () => {
    fixture.detectChanges();
    component.classes.set([makeClass({ classteacherId: 10 }), makeClass({ id: 2, classteacherId: 11 })]);
    // editing class with teacher 10 — teacher 11 is assigned elsewhere, teacher 10 should be available
    const available = component.getAvailableTeachersForEdit(10);
    expect(available.map(t => t.id)).toContain(10);
    expect(available.map(t => t.id)).not.toContain(11);
  });

  // ─── onYearChange ─────────────────────────────────────────────────────────────

  it('should update year filter on dropdown change', () => {
    fixture.detectChanges();
    const select = { target: { value: '3' } } as any;
    component.onYearChange(select);
    expect(component.selectedAcademicYearId()).toBe(3);
  });

  // ─── Create Modal ─────────────────────────────────────────────────────────────

  it('should open and close create modal', () => {
    expect(component.showCreateModal()).toBe(false);
    component.selectedAcademicYearId.set(2);
    component.openCreateModal();
    expect(component.showCreateModal()).toBe(true);
    expect(component.createForm().classname).toBe('');
    expect(component.createForm().academicyearId).toBe(2);

    component.closeCreateModal();
    expect(component.showCreateModal()).toBe(false);
  });

  it('should warn when creating class without name or section', () => {
    component.createForm.set({ classname: '', section: '', classteacherId: null, academicyearId: 2, subjectIds: [] });
    component.saveClass();
    expect(mockToastService.warning).toHaveBeenCalledWith('Please fill in both Class Name and Section.');

    component.createForm.set({ classname: 'Class 3', section: '', classteacherId: null, academicyearId: 2, subjectIds: [] });
    component.saveClass();
    expect(mockToastService.warning).toHaveBeenCalledTimes(2);
  });

  it('should create class successfully', () => {
    fixture.detectChanges();
    component.createForm.set({ classname: 'Class 3', section: 'B', classteacherId: 11, academicyearId: 2, subjectIds: [5, 6] });
    component.saveClass();
    expect(mockToastService.success).toHaveBeenCalledWith('Class created successfully!');
    expect(component.showCreateModal()).toBe(false);
  });

  it('should handle saveClass with null classteacherId/academicyearId (DTO coercion)', () => {
    fixture.detectChanges();
    component.createForm.set({ classname: 'Class 4', section: 'C', classteacherId: null, academicyearId: null, subjectIds: [] });
    component.saveClass();
    expect(mockToastService.success).toHaveBeenCalledWith('Class created successfully!');
  });

  it('should handle saveClass API error', () => {
    vi.spyOn(mockClassService, 'createClass').mockReturnValue(throwError(() => ({ error: { message: 'Teacher already assigned' } })));
    component.createForm.set({ classname: 'Class 3', section: 'A', classteacherId: 10, academicyearId: 2, subjectIds: [] });
    component.saveClass();
    expect(mockToastService.error).toHaveBeenCalledWith('Teacher already assigned');
  });

  it('should handle saveClass API error with fallback message', () => {
    vi.spyOn(mockClassService, 'createClass').mockReturnValue(throwError(() => ({ error: {} })));
    component.createForm.set({ classname: 'Class 3', section: 'A', classteacherId: null, academicyearId: 2, subjectIds: [] });
    component.saveClass();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to save class. A teacher might already be assigned to another class.');
  });

  // ─── Edit Modal ───────────────────────────────────────────────────────────────

  it('should open and close edit modal', () => {
    const cls = makeClass();
    component.openEditModal(cls as any);
    expect(component.showEditModal()).toBe(true);
    expect(component.editingClass()).toEqual(cls);
    expect(component.editForm().classname).toBe('Class 1');
    expect(component.editForm().subjectIds).toEqual([5]);

    component.closeEditModal();
    expect(component.showEditModal()).toBe(false);
    expect(component.editingClass()).toBeNull();
  });

  it('should open edit modal for class with no subjects', () => {
    const cls = makeClass({ subjects: null });
    component.openEditModal(cls as any);
    expect(component.editForm().subjectIds).toEqual([]);
  });

  it('should warn when saving edit without name or section', () => {
    component.editingClass.set(makeClass() as any);
    component.editForm.set({ classname: '', section: 'A', classteacherId: null, academicyearId: 2, subjectIds: [] });
    component.saveEdit();
    expect(mockToastService.warning).toHaveBeenCalledWith('Please fill in both Class Name and Section.');
  });

  it('should early-return saveEdit when no editingClass', () => {
    component.editingClass.set(null);
    const spy = vi.spyOn(mockClassService, 'updateClass');
    component.saveEdit();
    expect(spy).not.toHaveBeenCalled();
  });

  it('should save edit successfully', () => {
    fixture.detectChanges();
    component.openEditModal(makeClass() as any);
    component.saveEdit();
    expect(mockToastService.success).toHaveBeenCalledWith('Class updated successfully!');
    expect(component.showEditModal()).toBe(false);
  });

  it('should handle saveEdit with null teacher/year (DTO coercion)', () => {
    fixture.detectChanges();
    component.editingClass.set(makeClass() as any);
    component.editForm.set({ classname: 'Class 1', section: 'A', classteacherId: null, academicyearId: null, subjectIds: [] });
    component.saveEdit();
    expect(mockToastService.success).toHaveBeenCalledWith('Class updated successfully!');
  });

  it('should handle saveEdit API error with message', () => {
    vi.spyOn(mockClassService, 'updateClass').mockReturnValue(throwError(() => ({ error: { message: 'Update failed' } })));
    component.editingClass.set(makeClass() as any);
    component.editForm.set({ classname: 'Class 1', section: 'A', classteacherId: null, academicyearId: 2, subjectIds: [] });
    component.saveEdit();
    expect(mockToastService.error).toHaveBeenCalledWith('Update failed');
  });

  it('should handle saveEdit API error with fallback message', () => {
    vi.spyOn(mockClassService, 'updateClass').mockReturnValue(throwError(() => ({ error: {} })));
    component.editingClass.set(makeClass() as any);
    component.editForm.set({ classname: 'Class 1', section: 'A', classteacherId: null, academicyearId: 2, subjectIds: [] });
    component.saveEdit();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to update class.');
  });

  // ─── Subject Toggle ───────────────────────────────────────────────────────────

  it('should toggle subject selection in create form', () => {
    component.createForm.set({ classname: 'X', section: 'Y', classteacherId: null, academicyearId: 2, subjectIds: [] });

    // Add subject
    component.toggleSubjectSelection(5, 'create');
    expect(component.createForm().subjectIds).toContain(5);

    // Remove subject
    component.toggleSubjectSelection(5, 'create');
    expect(component.createForm().subjectIds).not.toContain(5);
  });

  it('should toggle subject selection in edit form', () => {
    component.openEditModal(makeClass({ subjects: [] }) as any);

    // Add subject
    component.toggleSubjectSelection(6, 'edit');
    expect(component.editForm().subjectIds).toContain(6);

    // Remove subject
    component.toggleSubjectSelection(6, 'edit');
    expect(component.editForm().subjectIds).not.toContain(6);
  });

  // ─── Delete Modal ─────────────────────────────────────────────────────────────

  it('should open and close delete modal', () => {
    const cls = makeClass();
    component.openDeleteModal(cls as any);
    expect(component.showDeleteModal()).toBe(true);
    expect(component.deletingClass()).toEqual(cls);

    component.closeDeleteModal();
    expect(component.showDeleteModal()).toBe(false);
    expect(component.deletingClass()).toBeNull();
  });

  it('should early-return confirmDelete when no deletingClass', () => {
    component.deletingClass.set(null);
    const spy = vi.spyOn(mockClassService, 'deleteClass');
    component.confirmDelete();
    expect(spy).not.toHaveBeenCalled();
  });

  it('should delete class successfully', () => {
    fixture.detectChanges();
    component.openDeleteModal(makeClass() as any);
    component.confirmDelete();
    expect(mockToastService.success).toHaveBeenCalledWith('Class deleted successfully!');
    expect(component.showDeleteModal()).toBe(false);
  });

  it('should handle confirmDelete API error with message', () => {
    vi.spyOn(mockClassService, 'deleteClass').mockReturnValue(throwError(() => ({ error: { message: 'Cannot delete class with students' } })));
    component.deletingClass.set(makeClass() as any);
    component.confirmDelete();
    expect(mockToastService.error).toHaveBeenCalledWith('Cannot delete class with students');
  });

  it('should handle confirmDelete API error with fallback message', () => {
    vi.spyOn(mockClassService, 'deleteClass').mockReturnValue(throwError(() => ({ error: {} })));
    component.deletingClass.set(makeClass() as any);
    component.confirmDelete();
    expect(mockToastService.error).toHaveBeenCalledWith('Failed to delete class.');
  });

  // ─── DOM Tests ────────────────────────────────────────────────────────────────

  it('should render class table rows in the DOM for Admin', () => {
    component.isAdmin.set(true);
    component.loading.set(false);
    component.classes.set([makeClass()]);
    component.teachers.set([{ id: 10, name: 'Teacher Ten' } as any]);
    component.academicYears.set([{ id: 2, yearName: '2026-2027', isCurrent: true } as any]);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.innerHTML).toContain('Class Directory');
    expect(compiled.innerHTML).toContain('Class 1');
    expect(compiled.innerHTML).toContain('Teacher Ten');
    expect(compiled.innerHTML).toContain('2026-2027');
    expect(compiled.innerHTML).toContain('25 Students');

    // New Class button visible for admin
    expect(compiled.innerHTML).toContain('New Class');
  });

  it('should render empty state when no classes', () => {
    component.loading.set(false);
    component.error.set(null);
    component.classes.set([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.innerHTML).toContain('No Classes Found');
  });

  it('should render error state with try-again button', () => {
    component.loading.set(false);
    component.error.set('Failed to load classes.');
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.innerHTML).toContain('Failed to load classes.');
    const tryAgainBtn = compiled.querySelector('button.btn-outline-primary') as HTMLButtonElement;
    expect(tryAgainBtn).toBeTruthy();
    tryAgainBtn.click();
    fixture.detectChanges();
  });

  it('should render loading spinner', () => {
    component.loading.set(true);
    fixture.detectChanges();
    expect(fixture.nativeElement.innerHTML).toContain('Organizing class sections...');
  });

  it('should render Create Modal and interact with inputs', () => {
    component.isAdmin.set(true);
    component.loading.set(false);
    component.classes.set([makeClass()]);
    component.subjects.set([{ id: 5, subjectName: 'Math' } as any, { id: 6, subjectName: 'Science' } as any]);
    component.teachers.set([{ id: 11, name: 'Teacher Eleven' } as any]);
    component.academicYears.set([{ id: 2, yearName: '2026-2027', isCurrent: true } as any]);
    component.showCreateModal.set(true);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.innerHTML).toContain('Create Class');

    // isSaving spinner path
    component.isSaving.set(true);
    fixture.detectChanges();
    expect(compiled.innerHTML).toContain('Creating...');

    component.isSaving.set(false);
    fixture.detectChanges();
    expect(compiled.innerHTML).toContain('Create Class');

    // Subject toggle button
    const subjectBtns = compiled.querySelectorAll('.modal-body-custom button');
    if (subjectBtns.length > 0) {
      (subjectBtns[0] as HTMLButtonElement).click();
      fixture.detectChanges();
      // toggle again
      (subjectBtns[0] as HTMLButtonElement).click();
      fixture.detectChanges();
    }

    // Close via overlay click
    const overlay = compiled.querySelector('.modal-overlay') as HTMLElement;
    if (overlay) {
      overlay.click();
      fixture.detectChanges();
    }
  });

  it('should render Edit Modal with spinner and subject toggles', () => {
    component.isAdmin.set(true);
    component.loading.set(false);
    component.classes.set([makeClass()]);
    component.subjects.set([{ id: 5, subjectName: 'Math' } as any]);
    component.teachers.set([{ id: 10, name: 'Teacher Ten' } as any]);
    component.academicYears.set([{ id: 2, yearName: '2026-2027', isCurrent: true } as any]);
    component.openEditModal(makeClass() as any);
    component.showEditModal.set(true);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.innerHTML).toContain('Edit Class');

    // isUpdating spinner
    component.isUpdating.set(true);
    fixture.detectChanges();
    expect(compiled.innerHTML).toContain('Saving...');

    component.isUpdating.set(false);
    fixture.detectChanges();
    expect(compiled.innerHTML).toContain('Save Changes');

    // Edit table row buttons: Edit and Delete
    component.showEditModal.set(false);
    fixture.detectChanges();

    component.isAdmin.set(true);
    component.classes.set([makeClass()]);
    fixture.detectChanges();
    const editBtn = compiled.querySelector('button[title="Edit"]') as HTMLButtonElement;
    if (editBtn) {
      editBtn.click();
      fixture.detectChanges();
    }
    const deleteBtn = compiled.querySelector('button[title="Delete"]') as HTMLButtonElement;
    if (deleteBtn) {
      deleteBtn.click();
      fixture.detectChanges();
    }
  });

  it('should render Delete Modal with spinner states', () => {
    component.isAdmin.set(true);
    component.loading.set(false);
    component.showDeleteModal.set(true);
    component.deletingClass.set(makeClass() as any);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.innerHTML).toContain('Delete Class');
    expect(compiled.innerHTML).toContain('Class 1 - A');

    // isDeleting spinner
    component.isDeleting.set(true);
    fixture.detectChanges();
    expect(compiled.innerHTML).toContain('Deleting...');

    component.isDeleting.set(false);
    fixture.detectChanges();
    expect(compiled.innerHTML).toContain('Delete Class');

    // Close via overlay click
    const overlay = compiled.querySelector('.modal-overlay') as HTMLElement;
    if (overlay) {
      overlay.click();
      fixture.detectChanges();
    }
  });

  it('should render year dropdown and trigger year change', () => {
    component.isAdmin.set(false);
    component.loading.set(false);
    component.classes.set([makeClass()]);
    component.academicYears.set([
      { id: 2, yearName: '2026-2027', isCurrent: true } as any,
      { id: 3, yearName: '2027-2028', isCurrent: false } as any
    ]);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const select = compiled.querySelector('select') as HTMLSelectElement;
    expect(select).toBeTruthy();

    // simulate year change
    Object.defineProperty(select, 'value', { writable: true, value: '3' });
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();
    expect(component.selectedAcademicYearId()).toBe(3);
  });

  it('should not show New Class button for non-admin', () => {
    component.isAdmin.set(false);
    component.loading.set(false);
    component.classes.set([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.innerHTML).not.toContain('New Class');
  });

  it('should click Save Changes and Cancel buttons in Edit Modal footer', () => {
    component.isAdmin.set(true);
    component.loading.set(false);
    component.classes.set([makeClass()]);
    component.subjects.set([{ id: 5, subjectName: 'Math' } as any]);
    component.teachers.set([{ id: 10, name: 'Teacher Ten' } as any]);
    component.academicYears.set([{ id: 2, yearName: '2026-2027', isCurrent: true } as any]);
    component.openEditModal(makeClass() as any);
    component.showEditModal.set(true);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    // Click Save Changes button (line 367)
    const saveBtn = compiled.querySelector('.modal-footer-custom button.btn-primary') as HTMLButtonElement;
    if (saveBtn) {
      saveBtn.click();
      fixture.detectChanges();
    }

    // Reopen and click Cancel button (line 360)
    component.openEditModal(makeClass() as any);
    component.showEditModal.set(true);
    fixture.detectChanges();
    const cancelBtn = compiled.querySelector('.modal-footer-custom button.btn-light') as HTMLButtonElement;
    if (cancelBtn) {
      cancelBtn.click();
      fixture.detectChanges();
    }
  });

  it('should click Confirm Delete and Cancel buttons in Delete Modal footer', () => {
    component.isAdmin.set(true);
    component.loading.set(false);
    component.showDeleteModal.set(true);
    component.deletingClass.set(makeClass() as any);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    // Click Confirm Delete button (line 423)
    const confirmBtn = compiled.querySelector('.modal-footer-custom button.btn-danger') as HTMLButtonElement;
    if (confirmBtn) {
      confirmBtn.click();
      fixture.detectChanges();
    }

    // Reopen and click Cancel (line 416)
    component.showDeleteModal.set(true);
    component.deletingClass.set(makeClass() as any);
    fixture.detectChanges();
    const cancelBtn = compiled.querySelector('.modal-footer-custom button.btn-light') as HTMLButtonElement;
    if (cancelBtn) {
      cancelBtn.click();
      fixture.detectChanges();
    }
  });

  it('should click Edit and Delete row buttons in the table', () => {
    component.isAdmin.set(true);
    component.loading.set(false);
    component.error.set(null);
    component.classes.set([makeClass()]);
    component.teachers.set([{ id: 10, name: 'Teacher Ten' } as any]);
    component.academicYears.set([{ id: 2, yearName: '2026-2027', isCurrent: true } as any]);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    // Edit button (line 141-148)
    const editBtn = compiled.querySelector('button[title="Edit"]') as HTMLButtonElement;
    if (editBtn) {
      editBtn.click();
      fixture.detectChanges();
      expect(component.showEditModal()).toBe(true);
      component.showEditModal.set(false);
      fixture.detectChanges();
    }

    // Delete button (line 150-158)
    const deleteBtn = compiled.querySelector('button[title="Delete"]') as HTMLButtonElement;
    if (deleteBtn) {
      deleteBtn.click();
      fixture.detectChanges();
      expect(component.showDeleteModal()).toBe(true);
    }
  });
});

