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
      getAllClasses: () => of([{ id: 1, classname: 'Class 1', section: 'A', classteacherId: 10, academicyearId: 2, subjects: [] }]),
      createClass: () => of({}),
      updateClass: () => of({}),
      deleteClass: () => of({})
    };
    mockTeacherService = {
      getAllTeachers: () => of({ items: [{ id: 10, name: 'Teacher Ten', username: 't10', joiningdate: new Date(), phonenumber: '123', userId: 100 }] })
    };
    mockAcademicYearService = {
      getAllAcademicYears: () => of([{ id: 2, yearName: '2026-2027', isCurrent: true, startDate: '', endDate: '' }])
    };
    mockSubjectService = {
      getAllSubjects: () => of([{ id: 5, subjectName: 'Math', description: '' }])
    };
    mockToastService = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn()
    };

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

  it('should create and load initial data', () => {
    expect(component).toBeTruthy();
    fixture.detectChanges();
    expect(component.classes().length).toBe(1);
    expect(component.teachers().length).toBe(1);
    expect(component.subjects().length).toBe(1);
  });

  it('should retrieve teacher name helper', () => {
    fixture.detectChanges();
    expect(component.getTeacherName(10)).toBe('Teacher Ten');
    expect(component.getTeacherName(99)).toBe('Unknown');
    expect(component.getTeacherName(undefined)).toBe('Not Assigned');
  });

  it('should toggle modal flags', () => {
    expect(component.showCreateModal()).toBe(false);
    component.openCreateModal();
    expect(component.showCreateModal()).toBe(true);

    component.closeCreateModal();
    expect(component.showCreateModal()).toBe(false);
  });

  it('should validate form and save class successfully', () => {
    fixture.detectChanges();
    component.createForm.set({
      classname: 'Class 3',
      section: 'A',
      classteacherId: 10,
      academicyearId: 2,
      subjectIds: [5]
    });

    component.saveClass();
    expect(mockToastService.success).toHaveBeenCalledWith('Class created successfully!');
  });

  it('should open edit modal and update class successfully', () => {
    fixture.detectChanges();
    const classItem = { id: 1, classname: 'Class 1', section: 'A', classteacherId: 10, academicyearId: 2, subjects: [] };
    
    component.openEditModal(classItem);
    expect(component.showEditModal()).toBe(true);
    expect(component.editingClass()).toEqual(classItem);

    component.saveEdit();
    expect(mockToastService.success).toHaveBeenCalledWith('Class updated successfully!');
  });

  it('should open delete modal and confirm deletion successfully', () => {
    fixture.detectChanges();
    const classItem = { id: 1, classname: 'Class 1', section: 'A', classteacherId: 10, academicyearId: 2, subjects: [] };
    
    component.openDeleteModal(classItem);
    expect(component.showDeleteModal()).toBe(true);
    expect(component.deletingClass()).toEqual(classItem);

    component.confirmDelete();
    expect(mockToastService.success).toHaveBeenCalledWith('Class deleted successfully!');
  });
});
