import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { AcademicSessions } from './academic-sessions';
import { AcademicYearService } from '../../services/academic-year.service';

describe('AcademicSessions', () => {
  let component: AcademicSessions;
  let fixture: ComponentFixture<AcademicSessions>;
  let mockAcademicYearService: any;

  beforeEach(async () => {
    mockAcademicYearService = {
      getAllAcademicYears: () => of([{ id: 1, yearName: '2026-2027', isCurrent: true, startDate: '2026-06-01', endDate: '2027-05-31' }]),
      createAcademicYear: () => of({}),
      setCurrentAcademicYear: () => of({})
    };

    await TestBed.configureTestingModule({
      imports: [AcademicSessions],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AcademicYearService, useValue: mockAcademicYearService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AcademicSessions);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create and load sessions', () => {
    expect(component).toBeTruthy();
    fixture.detectChanges();
    expect(component.sessions().length).toBe(1);
  });

  it('should open and close create modal', () => {
    expect(component.showCreateModal()).toBe(false);
    component.openCreateModal();
    expect(component.showCreateModal()).toBe(true);

    component.closeCreateModal();
    expect(component.showCreateModal()).toBe(false);
  });

  it('should save session successfully', () => {
    component.createForm.set({ yearName: '2027-2028', startDate: '2027-06-01', endDate: '2028-05-31' });
    component.saveSession();
    expect(component.isSaving()).toBe(false);
  });

  it('should set session as active successfully', () => {
    component.setAsActive(1);
    expect(component.sessions().length).toBe(1);
  });
});
