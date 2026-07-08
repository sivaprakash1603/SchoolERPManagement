import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Exams } from './exams';

describe('Exams', () => {
  let component: Exams;
  let fixture: ComponentFixture<Exams>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Exams],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Exams);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should return correct page title and description based on role', () => {
    component.isAdminOrTeacher.set(true);
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

  it('should toggle modals correctly', () => {
    expect(component.showCreateExamModal()).toBe(false);
    expect(component.showScheduleModal()).toBe(false);

    component.showCreateExamModal.set(true);
    expect(component.showCreateExamModal()).toBe(true);

    component.showCreateExamModal.set(false);
    expect(component.showCreateExamModal()).toBe(false);
  });
});
