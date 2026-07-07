import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { AcademicSessions } from './academic-sessions';

describe('AcademicSessions', () => {
  let component: AcademicSessions;
  let fixture: ComponentFixture<AcademicSessions>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AcademicSessions],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AcademicSessions);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
