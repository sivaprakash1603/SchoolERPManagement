import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Classes } from './classes';

describe('Classes', () => {
  let component: Classes;
  let fixture: ComponentFixture<Classes>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Classes],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Classes);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should retrieve teacher name helper', () => {
    component.teachers.set([
      { id: 1, userId: 1, name: 'Teacher One', phonenumber: '123', joiningdate: new Date(), username: 't1' }
    ]);
    expect(component.getTeacherName(1)).toBe('Teacher One');
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

  it('should handle academic year name queries', () => {
    component.academicYears.set([
      { id: 1, yearName: '2024-2025', startDate: '', endDate: '', isCurrent: true }
    ]);
    expect(component.getYearName(1)).toBe('2024-2025');
    expect(component.getYearName(99)).toBe('N/A');
  });

  it('should toggle subject selection inside creation form', () => {
    component.createForm.set({
      classname: 'Class 3',
      section: 'A',
      classteacherId: null,
      academicyearId: 1,
      subjectIds: [10, 20]
    });

    expect(component.createForm().subjectIds.includes(10)).toBe(true);
    expect(component.createForm().subjectIds.includes(30)).toBe(false);

    component.toggleSubjectSelection(30, 'create');
    expect(component.createForm().subjectIds.includes(30)).toBe(true);

    component.toggleSubjectSelection(10, 'create');
    expect(component.createForm().subjectIds.includes(10)).toBe(false);
  });
});
