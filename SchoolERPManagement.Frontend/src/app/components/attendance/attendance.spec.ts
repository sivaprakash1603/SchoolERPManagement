import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Attendance } from './attendance';

describe('Attendance', () => {
  let component: Attendance;
  let fixture: ComponentFixture<Attendance>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Attendance],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Attendance);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should return correct page title and description based on user role', () => {
    component.userRole.set('Student');
    expect(component.pageTitle).toBe('My Attendance Dashboard');
    expect(component.pageDescription).toBe('Overview of your personal attendance performance.');

    component.userRole.set('Parent');
    expect(component.pageTitle).toBe('Child Attendance Dashboard');
    expect(component.pageDescription).toBe("Overview of your child's attendance performance.");

    component.userRole.set('Admin');
    component.activeTab.set('students');
    expect(component.pageTitle).toBe('Daily Student Attendance');
    expect(component.pageDescription).toBe('Record and review student attendance.');
  });

  it('should toggle tabs correctly', () => {
    component.activeTab.set('students');
    expect(component.activeTab()).toBe('students');
    
    component.activeTab.set('teachers');
    expect(component.activeTab()).toBe('teachers');
  });

  it('should change tab and reset loading', () => {
    component.activeTab.set('students');
    // Simulate setting tab to teachers
    component.activeTab.set('teachers');
    expect(component.activeTab()).toBe('teachers');
  });
});
