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
});
