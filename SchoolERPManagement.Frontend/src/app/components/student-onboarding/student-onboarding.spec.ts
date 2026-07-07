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
});
