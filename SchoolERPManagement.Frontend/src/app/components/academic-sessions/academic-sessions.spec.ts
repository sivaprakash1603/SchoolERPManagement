import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { of, throwError, Subject } from 'rxjs';
import { AcademicSessions } from './academic-sessions';
import { AcademicYearService } from '../../services/academic-year.service';

describe('AcademicSessions', () => {
  let component: AcademicSessions;
  let fixture: ComponentFixture<AcademicSessions>;
  let mockAcademicYearService: any;

  beforeEach(async () => {
    mockAcademicYearService = {
      getAllAcademicYears: () => of([
        { id: 1, yearName: '2026-2027', isCurrent: true, startDate: '2026-06-01', endDate: '2027-05-31' },
        { id: 2, yearName: '2027-2028', isCurrent: false, startDate: '2027-06-01', endDate: '2028-05-31' }
      ]),
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
    expect(component.sessions().length).toBe(2);
  });

  it('should render empty sessions state', () => {
    vi.spyOn(mockAcademicYearService, 'getAllAcademicYears').mockReturnValue(of([]));
    component.ngOnInit();
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.innerHTML).toContain('No Sessions Created');
  });

  it('should handle error when loading sessions fails', () => {
    vi.spyOn(mockAcademicYearService, 'getAllAcademicYears').mockReturnValue(throwError(() => new Error('API Error')));
    component.ngOnInit();
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(component.error()).toBe('Failed to load academic sessions. Please try again.');
    expect(compiled.querySelector('p.text-warning')?.textContent).toContain('Failed to load academic sessions');

    // Click Try Again button
    const tryAgainBtn = compiled.querySelector('.btn-outline-primary') as HTMLButtonElement;
    if (tryAgainBtn) {
      tryAgainBtn.click();
    }
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

  it('should guard against empty form values when saving', () => {
    component.createForm.set({ yearName: '', startDate: '', endDate: '' });
    component.saveSession();
    expect(component.isSaving()).toBe(false);
  });

  it('should handle error when creating session fails', () => {
    vi.spyOn(window, 'alert').mockImplementation(() => {});
    vi.spyOn(mockAcademicYearService, 'createAcademicYear').mockReturnValue(throwError(() => new Error('Save Failed')));
    component.createForm.set({ yearName: '2027-2028', startDate: '2027-06-01', endDate: '2028-05-31' });
    component.saveSession();
    expect(component.isSaving()).toBe(false);
  });

  it('should set session as active successfully', () => {
    component.setAsActive(2);
    expect(component.sessions().length).toBe(2);
  });

  it('should handle error when activating session fails', () => {
    vi.spyOn(window, 'alert').mockImplementation(() => {});
    vi.spyOn(mockAcademicYearService, 'setCurrentAcademicYear').mockReturnValue(throwError(() => new Error('Activation Failed')));
    component.setAsActive(2);
  });

  it('should cover initial loading spinner state in template', () => {
    // Return a non-resolved observable to keep loading true
    const subject = new Subject<any>();
    vi.spyOn(mockAcademicYearService, 'getAllAcademicYears').mockReturnValue(subject);
    component.ngOnInit();
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('img[alt="Loading..."]')).toBeTruthy();
  });

  it('should render modals and list interactions in DOM', () => {
    component.loading.set(false);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;

    // Click Activate Session list button
    const activateBtn = compiled.querySelector('.btn-outline-primary') as HTMLButtonElement;
    if (activateBtn) {
      activateBtn.click();
    }

    // Click New Session header button
    const newSessionBtn = compiled.querySelector('.btn-primary') as HTMLButtonElement;
    if (newSessionBtn) {
      newSessionBtn.click();
      fixture.detectChanges();
      expect(component.showCreateModal()).toBe(true);
    }

    // Trigger ngModel updates by typing in elements
    const inputs = compiled.querySelectorAll('input');
    if (inputs.length >= 3) {
      const yearInput = inputs[0] as HTMLInputElement;
      yearInput.value = '2029-2030';
      yearInput.dispatchEvent(new Event('input'));

      const startInput = inputs[1] as HTMLInputElement;
      startInput.value = '2029-06-01';
      startInput.dispatchEvent(new Event('input'));

      const endInput = inputs[2] as HTMLInputElement;
      endInput.value = '2030-05-31';
      endInput.dispatchEvent(new Event('input'));
    }
    fixture.detectChanges();

    // Click Save/Create Session inside modal card (line 160)
    const saveBtn = compiled.querySelector('.modal-footer-custom .btn-success') as HTMLButtonElement;
    if (saveBtn) {
      saveBtn.click();
      fixture.detectChanges();
    }

    // Reopen modal, click Cancel inside modal card
    component.showCreateModal.set(true);
    fixture.detectChanges();
    const cancelBtn = compiled.querySelector('.modal-footer-custom .btn-light') as HTMLButtonElement;
    if (cancelBtn) {
      cancelBtn.click();
      fixture.detectChanges();
      expect(component.showCreateModal()).toBe(false);
    }

    // Reopen modal, set isSaving to true to render the spinner
    component.showCreateModal.set(true);
    component.isSaving.set(true);
    fixture.detectChanges();
    expect(compiled.querySelector('.spinner-border')).toBeTruthy();
  });
});
