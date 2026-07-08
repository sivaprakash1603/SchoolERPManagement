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

  it('should open and close create modal', () => {
    expect(component.showCreateModal()).toBe(false);
    component.openCreateModal();
    expect(component.showCreateModal()).toBe(true);

    component.closeCreateModal();
    expect(component.showCreateModal()).toBe(false);
  });

  it('should validate form before saving session', () => {
    component.createForm.set({ yearName: '', startDate: '', endDate: '' });
    expect(component.isSaving()).toBe(false);
    component.saveSession();
    expect(component.isSaving()).toBe(false); // Should not start saving since validation fails
  });
});
