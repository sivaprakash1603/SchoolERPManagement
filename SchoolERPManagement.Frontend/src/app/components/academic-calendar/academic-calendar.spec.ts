import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { AcademicCalendar } from './academic-calendar';

describe('AcademicCalendar', () => {
  let component: AcademicCalendar;
  let fixture: ComponentFixture<AcademicCalendar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AcademicCalendar],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AcademicCalendar);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
