import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { StudentOnboarding } from './student-onboarding';

describe('StudentOnboarding', () => {
  let component: StudentOnboarding;
  let fixture: ComponentFixture<StudentOnboarding>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StudentOnboarding],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(StudentOnboarding);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should handle wizard step navigation', () => {
    component.currentStep.set(1);
    
    component.nextStep();
    expect(component.currentStep()).toBe(2);

    component.prevStep();
    expect(component.currentStep()).toBe(1);
  });

  it('should filter parent query correctly', () => {
    component.parents.set([
      { id: 1, userId: 10, name: 'Alice Smith', email: 'alice@test.com', phonenumber: '1234567890', relation: 'Father', username: 'alice' },
      { id: 2, userId: 11, name: 'Bob Johnson', email: 'bob@test.com', phonenumber: '9876543210', relation: 'Father', username: 'bob' }
    ]);
    component.parentSearchQuery.set('Alice');
    const filtered = component.filteredParents();
    expect(filtered.length).toBe(1);
    expect(filtered[0].name).toBe('Alice Smith');
  });

  it('should toggle parent selection and get relation', () => {
    component.selectedParents.set([]);
    component.toggleParentSelection(5, 'Chris Evans');
    
    expect(component.isParentSelected(5)).toBe(true);
    expect(component.getSelectedParentRelation(5)).toBe('Father');

    component.updateExistingParentRelation(5, 'Mother');
    expect(component.getSelectedParentRelation(5)).toBe('Mother');

    component.toggleParentSelection(5, 'Chris Evans');
    expect(component.isParentSelected(5)).toBe(false);
  });

  it('should add and remove new parent forms', () => {
    component.newParents.set([]);
    
    component.addNewParentForm();
    expect(component.newParents().length).toBe(1);
    expect(component.newParents()[0].relation).toBe('Father');

    component.removeNewParentForm(0);
    expect(component.newParents().length).toBe(0);
  });
});
