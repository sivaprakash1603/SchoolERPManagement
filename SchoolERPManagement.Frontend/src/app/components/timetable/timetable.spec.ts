import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Timetable } from './timetable';

describe('Timetable', () => {
  let component: Timetable;
  let fixture: ComponentFixture<Timetable>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Timetable],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Timetable);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should return correct page title and description based on viewMode', () => {
    component.viewMode.set('personal');
    component.userRole.set('Teacher');
    expect(component.pageTitle).toBe('My Schedule');
    expect(component.pageDescription).toBe('View your personalized weekly teaching schedule.');

    component.viewMode.set('class');
    expect(component.pageTitle).toBe('Class Timetable');
    expect(component.pageDescription).toBe('View and configure weekly schedules for institutional classes.');
  });

  it('should retrieve class name details', () => {
    component.classes.set([
      { id: 5, classname: 'Grade 3', section: 'A', academicyearId: 1, classteacherId: 1 }
    ]);
    expect(component.getClassNameForSlot(5)).toBe('Grade 3 - A');
    expect(component.getClassNameForSlot(99)).toBe('Class #99');
  });

  it('should initialize timings and update specific period timings', () => {
    component.initializeTimings(3);
    expect(component.periodTimings().length).toBe(3);
    expect(component.periodTimings()[0].periodNumber).toBe(1);

    component.updateTiming(0, 'startTime', '08:30');
    expect(component.periodTimings()[0].startTime).toBe('08:30');
  });

  it('should handle generator modal actions', () => {
    expect(component.showGeneratorModal()).toBe(false);
    component.showGeneratorModal.set(true);
    expect(component.showGeneratorModal()).toBe(true);
  });
});
