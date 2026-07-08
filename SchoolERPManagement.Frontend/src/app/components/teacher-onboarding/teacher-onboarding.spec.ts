import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TeacherOnboarding } from './teacher-onboarding';

describe('TeacherOnboarding', () => {
  let component: TeacherOnboarding;
  let fixture: ComponentFixture<TeacherOnboarding>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TeacherOnboarding],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TeacherOnboarding);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should progress through steps', () => {
    component.currentStep.set(1);

    component.nextStep();
    expect(component.currentStep()).toBe(2);

    component.prevStep();
    expect(component.currentStep()).toBe(1);
  });

  it('should manage assignments', () => {
    component.selectedAssignments.set([
      { classId: 1, subjectId: 2, className: 'Grade 1', subjectName: 'English' }
    ]);
    expect(component.selectedAssignments().length).toBe(1);

    component.removeAssignment(0);
    expect(component.selectedAssignments().length).toBe(0);
  });

  it('should manage uploaded documents', () => {
    const dummyFile = new File([''], 'degree.pdf');
    component.selectedDocuments.set([
      { file: dummyFile, type: 'Degree / Educational Certificate' }
    ]);

    expect(component.getFileName('Degree / Educational Certificate')).toBe('degree.pdf');
    expect(component.getFileName('Identity Proof')).toBeNull();

    component.removeDocument('Degree / Educational Certificate');
    expect(component.selectedDocuments().length).toBe(0);
  });
});
