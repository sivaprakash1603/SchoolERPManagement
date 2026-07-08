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

  it('should open and close create modal', () => {
    expect(component.showCreateModal()).toBe(false);
    component.openCreateModal();
    expect(component.showCreateModal()).toBe(true);
    expect(component.createForm().isHoliday).toBe(true);

    component.closeCreateModal();
    expect(component.showCreateModal()).toBe(false);
  });

  it('should handle year change selection', () => {
    const mockEvent = {
      target: { value: '2' }
    } as unknown as Event;

    component.onYearChange(mockEvent);
    expect(component.selectedAcademicYearId()).toBe(2);
  });

  it('should open and close delete modal', () => {
    expect(component.showDeleteModal()).toBe(false);
    const mockEvent = { id: 1, date: '2024-05-10', description: 'Holiday', isHoliday: true, academicYearId: 1 };
    
    component.openDeleteModal(mockEvent);
    expect(component.showDeleteModal()).toBe(true);
    expect(component.deletingEvent()).toEqual(mockEvent);

    component.closeDeleteModal();
    expect(component.showDeleteModal()).toBe(false);
    expect(component.deletingEvent()).toBeNull();
  });
});
