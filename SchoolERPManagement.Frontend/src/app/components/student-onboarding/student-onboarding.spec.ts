import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { StudentOnboarding } from './student-onboarding';
import { StudentService } from '../../services/student.service';
import { ClassService } from '../../services/class.service';
import { ParentService } from '../../services/parent.service';
import { DocumentService } from '../../services/document.service';
import { AcademicYearService } from '../../services/academic-year.service';
import { ToastService } from '../../services/toast.service';
import { of, throwError } from 'rxjs';

describe('StudentOnboarding', () => {
  let component: StudentOnboarding;
  let fixture: ComponentFixture<StudentOnboarding>;

  let mockRouter: any;
  let mockStudentService: any;
  let mockClassService: any;
  let mockParentService: any;
  let mockDocumentService: any;
  let mockAcademicYearService: any;
  let mockToastService: any;

  beforeEach(async () => {
    mockRouter = { navigate: vi.fn() };
    mockStudentService = { addStudent: vi.fn().mockReturnValue(of({ id: 99 })) };
    mockClassService = { getAllClasses: vi.fn().mockReturnValue(of([{ id: 1, name: 'Class 1' }])) };
    mockParentService = { 
      getAllParents: vi.fn().mockReturnValue(of({ items: [{ id: 10, name: 'John Doe', email: 'j@d.com', phonenumber: '123' }] })),
      addParent: vi.fn().mockReturnValue(of({ id: 11 }))
    };
    mockDocumentService = { uploadStudentDocument: vi.fn().mockReturnValue(of({})) };
    mockAcademicYearService = { getAllAcademicYears: vi.fn().mockReturnValue(of([{ id: 1, isCurrent: true }, { id: 2, isCurrent: false }])) };
    mockToastService = { success: vi.fn(), warning: vi.fn(), error: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [StudentOnboarding],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: StudentService, useValue: mockStudentService },
        { provide: ClassService, useValue: mockClassService },
        { provide: ParentService, useValue: mockParentService },
        { provide: DocumentService, useValue: mockDocumentService },
        { provide: AcademicYearService, useValue: mockAcademicYearService },
        { provide: ToastService, useValue: mockToastService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(StudentOnboarding);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  describe('Initialization', () => {
    it('should initialize data correctly and set current year', () => {
      expect(mockAcademicYearService.getAllAcademicYears).toHaveBeenCalled();
      expect(component.academicYears().length).toBe(2);
      expect(component.studentForm().academicYearId).toBe(1); // the current one
      expect(mockClassService.getAllClasses).toHaveBeenCalledWith(1);
      expect(mockParentService.getAllParents).toHaveBeenCalled();
      expect(component.parents().length).toBe(1);
    });

    it('should fallback to first year if no current year exists', () => {
      mockAcademicYearService.getAllAcademicYears.mockReturnValue(of([{ id: 5, isCurrent: false }]));
      fixture = TestBed.createComponent(StudentOnboarding);
      component = fixture.componentInstance;
      fixture.detectChanges();
      expect(component.studentForm().academicYearId).toBe(5);
    });

    it('should handle API errors gracefully on init', () => {
      mockAcademicYearService.getAllAcademicYears.mockReturnValue(throwError(() => new Error('Error')));
      mockClassService.getAllClasses.mockReturnValue(throwError(() => new Error('Error')));
      mockParentService.getAllParents.mockReturnValue(throwError(() => new Error('Error')));
      
      fixture = TestBed.createComponent(StudentOnboarding);
      component = fixture.componentInstance;
      
      const spyConsole = vi.spyOn(console, 'error').mockImplementation(() => {});
      fixture.detectChanges();
      
      expect(spyConsole).toHaveBeenCalled();
      spyConsole.mockRestore();
    });
  });

  describe('Navigation and UI Methods', () => {
    it('should handle wizard step navigation within bounds', () => {
      component.currentStep.set(1);
      component.prevStep(); // Should not go below 1
      expect(component.currentStep()).toBe(1);

      component.nextStep();
      expect(component.currentStep()).toBe(2);
      component.nextStep();
      component.nextStep();
      expect(component.currentStep()).toBe(4);
      component.nextStep(); // Should not go above 4
      expect(component.currentStep()).toBe(4);

      component.prevStep();
      expect(component.currentStep()).toBe(3);
    });

    it('should handle year change and refetch classes', () => {
      component.studentForm.update(f => ({ ...f, classId: 10 }));
      component.onYearChange();
      expect(component.studentForm().classId).toBeNull();
      expect(mockClassService.getAllClasses).toHaveBeenCalled();
    });

    it('should handle cancellation logic', () => {
      expect(component.canDeactivate()).toBe(false);
      expect(mockToastService.warning).toHaveBeenCalled();

      component.cancel();
      expect(component.isCancelled).toBe(true);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/students']);
      expect(component.canDeactivate()).toBe(true);
    });
  });

  describe('Parent Logic', () => {
    it('should search and filter parents', () => {
      component.parents.set([
        { id: 1, userId: 10, name: 'Alice', email: 'alice@test.com', phonenumber: '1234', relation: 'Father', username: 'alice' },
        { id: 2, userId: 11, name: 'Bob', email: 'bob@test.com', phonenumber: '5678', relation: 'Father', username: 'bob' }
      ]);
      component.parentSearchQuery.set('Alice');
      let filtered = component.filteredParents();
      expect(filtered.length).toBe(1);
      expect(filtered[0].name).toBe('Alice');

      component.parentSearchQuery.set('5678');
      filtered = component.filteredParents();
      expect(filtered.length).toBe(1);
      expect(filtered[0].name).toBe('Bob');
    });

    it('should toggle parent selection and update relation', () => {
      component.selectedParents.set([]);
      component.toggleParentSelection(5, 'Chris');
      
      expect(component.isParentSelected(5)).toBe(true);
      expect(component.getSelectedParentRelation(5)).toBe('Father');

      component.updateExistingParentRelation(5, 'Mother');
      expect(component.getSelectedParentRelation(5)).toBe('Mother');

      component.toggleParentSelection(5, 'Chris');
      expect(component.isParentSelected(5)).toBe(false);
    });

    it('should add, update, and remove new parents', () => {
      component.newParents.set([]);
      component.addNewParentForm();
      expect(component.newParents().length).toBe(1);

      component.updateNewParent(0, 'name', 'New Guy');
      expect(component.newParents()[0].name).toBe('New Guy');

      component.removeNewParentForm(0);
      expect(component.newParents().length).toBe(0);
    });
    
    it('should track items by index', () => {
      expect(component.trackByIndex(5)).toBe(5);
    });
  });

  describe('Document Logic', () => {
    it('should add, update, and remove documents', () => {
      const file1 = new File([''], 'doc1.pdf');
      const file2 = new File([''], 'doc2.pdf');

      // Add a document
      component.onFileSelected({ target: { files: [file1] } } as any, 'Identity');
      expect(component.getFileName('Identity')).toBe('doc1.pdf');
      expect(component.getFileName('Unknown')).toBeNull();

      // Update same type
      component.onFileSelected({ target: { files: [file2] } } as any, 'Identity');
      expect(component.getFileName('Identity')).toBe('doc2.pdf');
      expect(component.selectedDocuments().length).toBe(1);

      // Add new type
      component.onFileSelected({ target: { files: [file1] } } as any, 'Photo');
      expect(component.selectedDocuments().length).toBe(2);

      // Remove
      component.removeDocument('Identity');
      expect(component.selectedDocuments().length).toBe(1);
      
      // Invalid event test
      component.onFileSelected({ target: { files: null } } as any, 'Photo');
      expect(component.selectedDocuments().length).toBe(1); // unchanged
    });
  });

  describe('Submission', () => {
    it('should prevent submission if admission date is in future', async () => {
      const future = new Date();
      future.setDate(future.getDate() + 2);
      component.studentForm.update(f => ({ ...f, admissiondate: future.toISOString() }));
      await component.completeOnboarding();
      expect(mockToastService.warning).toHaveBeenCalledWith(expect.stringContaining('Admission date cannot be in the future'));
      expect(component.isSubmitting()).toBe(false);
    });

    it('should prevent submission if DOB is in future', async () => {
      component.studentForm.update(f => ({ ...f, admissiondate: '' })); // clear admission date
      const future = new Date();
      future.setDate(future.getDate() + 2);
      component.studentForm.update(f => ({ ...f, dateofbirth: future.toISOString() }));
      await component.completeOnboarding();
      expect(mockToastService.warning).toHaveBeenCalledWith(expect.stringContaining('Date of birth cannot be in the future'));
    });

    it('should successfully complete onboarding with new parents and docs', async () => {
      // Setup successful state
      component.studentForm.update(f => ({ ...f, name: 'Kid', classId: 1, academicYearId: 1 }));
      component.selectedParents.set([{ parentId: 10, name: 'Old Parent', relation: 'Father' }]);
      component.newParents.set([{ name: 'New Mom', email: 'nm@m.com', phonenumber: '123', relation: 'Mother' }]);
      component.selectedDocuments.set([{ file: new File([''], 'doc.pdf'), type: 'Identity' }]);

      await component.completeOnboarding();

      expect(mockParentService.addParent).toHaveBeenCalled();
      expect(mockStudentService.addStudent).toHaveBeenCalled();
      expect(mockDocumentService.uploadStudentDocument).toHaveBeenCalled();
      
      expect(mockToastService.success).toHaveBeenCalled();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/students']);
      expect(component.isSubmitted).toBe(true);
      expect(component.canDeactivate()).toBe(true);
    });

    it('should handle document upload failure gracefully', async () => {
      mockDocumentService.uploadStudentDocument.mockReturnValue(throwError(() => new Error('Doc upload failed')));
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      
      component.studentForm.update(f => ({ ...f, name: 'Kid', classId: 1, academicYearId: 1 }));
      component.selectedDocuments.set([{ file: new File([''], 'doc.pdf'), type: 'Identity' }]);
      
      await component.completeOnboarding();
      
      expect(consoleSpy).toHaveBeenCalled();
      expect(mockToastService.success).toHaveBeenCalled(); // Should still succeed overall
      consoleSpy.mockRestore();
    });

    it('should handle failure in parent creation', async () => {
      mockParentService.addParent.mockReturnValue(throwError(() => ({ error: { message: 'Parent error' } })));
      component.newParents.set([{ name: 'New Mom', email: 'nm@m.com', phonenumber: '123', relation: 'Mother' }]);
      
      await component.completeOnboarding();
      
      expect(mockToastService.error).toHaveBeenCalledWith('Parent error');
      expect(component.isSubmitted).toBe(false);
    });

    it('should handle failure in student creation', async () => {
      mockStudentService.addStudent.mockReturnValue(throwError(() => new Error('Student error')));
      component.studentForm.update(f => ({ ...f, name: 'Kid', classId: 1, academicYearId: 1 }));
      
      await component.completeOnboarding();
      
      expect(mockToastService.error).toHaveBeenCalled();
    });
  });

  describe('HTML Template Interactions', () => {
    it('should interact with step 1 inputs and next button', () => {
      component.currentStep.set(1);
      // Make form valid so next button is enabled
      component.studentForm.update(f => ({ 
        ...f, 
        name: 'Test Student', 
        email: 'test@test.com',
        classId: 1, 
        academicYearId: 1, 
        dateofbirth: '2015-01-01', 
        admissiondate: '2023-01-01' 
      }));
      fixture.detectChanges();

      const buttons = Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[];
      const nextBtn = buttons.find(b => b.textContent?.includes('Next Step'));
      expect(nextBtn).toBeTruthy();
      expect(nextBtn!.disabled).toBe(false);
      
      nextBtn!.dispatchEvent(new Event('click'));
      expect(component.currentStep()).toBe(2);
    });

    it('should interact with step 2 (parents)', () => {
      component.currentStep.set(2);
      component.parents.set([{ id: 99, userId: 1, name: 'Test Parent', email: 'p@p.com', phonenumber: '1', relation: 'Father', username: 'test' }]);
      fixture.detectChanges();
      
      // Test search input
      const searchInput = fixture.nativeElement.querySelector('input[placeholder="Search by name, email or phone..."]');
      if (searchInput) {
        searchInput.value = 'Test Parent';
        searchInput.dispatchEvent(new Event('input'));
        expect(component.parentSearchQuery()).toBe('Test Parent');
      }

      // Add new parent button
      const buttons = Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[];
      const addParentBtn = buttons.find(b => b.textContent?.includes('Add New Parent'));
      if (addParentBtn) addParentBtn.dispatchEvent(new Event('click'));
      expect(component.newParents().length).toBe(1);
    });

    it('should interact with step 3 (documents)', async () => {
      component.currentStep.set(3);
      component.selectedDocuments.set([{ type: 'Birth Certificate', file: new File([''], 'bc.pdf') }]);
      component.selectedDocuments.set([{ type: 'Photo', file: new File([''], 'photo.png') }]);
      fixture.detectChanges();
      await fixture.whenStable(); // Ensure @for finishes rendering
      
      // File upload event
      const fileInputs = Array.from(fixture.nativeElement.querySelectorAll('input[type="file"]')) as HTMLInputElement[];
      expect(fileInputs.length).toBe(3);
      
      fileInputs[0].dispatchEvent(new Event('change')); // Identity
      fileInputs[1].dispatchEvent(new Event('change')); // Birth Certificate
      fileInputs[2].dispatchEvent(new Event('change')); // Photo
      
      // Since remove button doesn't exist in template, just verify getFileName displays
      const texts = fixture.nativeElement.textContent;
      expect(texts).toContain('photo.png');
    });

    it('should interact with step 4 (review and submit) with empty states', () => {
      component.currentStep.set(4);
      component.selectedParents.set([]);
      component.newParents.set([]);
      fixture.detectChanges();
      
      const texts = fixture.nativeElement.textContent;
      expect(texts).toContain('None selected');
    });

    it('should interact with step 4 (review and submit) with parents and submitting state', () => {
      component.currentStep.set(4);
      component.selectedParents.set([{ parentId: 10, name: 'Old Parent', relation: 'Father' }]);
      component.newParents.set([{ name: 'New Mom', email: 'nm@m.com', phonenumber: '123', relation: 'Mother' }]);
      component.isSubmitting.set(true);
      fixture.detectChanges();
      
      const texts = fixture.nativeElement.textContent;
      expect(texts).toContain('Old Parent');
      expect(texts).toContain('New Mom');
      expect(texts).toContain('Processing...');

      const buttons = Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[];
      const submitBtn = buttons.find(b => b.textContent?.includes('Processing...'));
      if (submitBtn) {
        expect(submitBtn.disabled).toBe(true);
      }
      
      component.isSubmitting.set(false);
      fixture.detectChanges();
      const submitBtnReady = Array.from(fixture.nativeElement.querySelectorAll('button')).find((b: any) => b.textContent?.includes('Complete Onboarding')) as HTMLButtonElement;
      if (submitBtnReady) {
        submitBtnReady.dispatchEvent(new Event('click'));
        expect(mockToastService.warning).not.toHaveBeenCalled();
      }
    });

    it('should trigger all template functions in step 1', async () => {
      component.currentStep.set(1);
      fixture.detectChanges();
      await fixture.whenStable();

      const nameInput = fixture.nativeElement.querySelector('input[placeholder="Enter student name"]');
      nameInput.value = 'NewName';
      nameInput.dispatchEvent(new Event('input'));

      const emailInput = fixture.nativeElement.querySelector('input[placeholder="student@example.com"]');
      emailInput.value = 'new@new.com';
      emailInput.dispatchEvent(new Event('input'));

      const selects = fixture.nativeElement.querySelectorAll('select');
      
      // academicYearId
      selects[0].value = '2';
      selects[0].dispatchEvent(new Event('change'));

      // classId
      selects[1].value = '1';
      selects[1].dispatchEvent(new Event('change'));

      // gender
      selects[2].value = 'Female';
      selects[2].dispatchEvent(new Event('change'));

      // bloodgroup
      selects[3].value = 'B+';
      selects[3].dispatchEvent(new Event('change'));

      const dateInputs = fixture.nativeElement.querySelectorAll('input[type="date"]');
      // dateofbirth
      dateInputs[0].value = '2010-01-01';
      dateInputs[0].dispatchEvent(new Event('input'));

      // admissiondate
      dateInputs[1].value = '2024-01-01';
      dateInputs[1].dispatchEvent(new Event('input'));

      expect(component.studentForm().name).toBe('NewName');
      expect(component.studentForm().email).toBe('new@new.com');
      expect(component.studentForm().gender).toBe('Female');
      expect(component.studentForm().bloodgroup).toBe('B+');
    });

    it('should trigger template functions for existing and new parents in step 2', async () => {
      component.currentStep.set(2);
      component.parents.set([{ id: 99, userId: 1, name: 'Test Parent', email: 'p@p.com', phonenumber: '1', relation: 'Father', username: 'test' }]);
      component.selectedParents.set([{ parentId: 99, name: 'Test Parent', relation: 'Father' }]);
      component.newParents.set([{ name: '', email: '', phonenumber: '', relation: 'Mother' }]);
      fixture.detectChanges();
      await fixture.whenStable();

      // Test existing parent relation change
      const existingRelSelect = fixture.nativeElement.querySelector('select.form-select-sm');
      if (existingRelSelect) {
        existingRelSelect.value = 'Mother';
        existingRelSelect.dispatchEvent(new Event('change'));
        expect(component.getSelectedParentRelation(99)).toBe('Mother');
      }

      // Test new parent fields
      const npName = fixture.nativeElement.querySelector('input[placeholder="Parent\'s Name"]');
      npName.value = 'NP Name';
      npName.dispatchEvent(new Event('input'));

      const npEmail = fixture.nativeElement.querySelector('input[placeholder="Email Address"]');
      npEmail.value = 'np@email.com';
      npEmail.dispatchEvent(new Event('input'));

      const npPhone = fixture.nativeElement.querySelector('input[placeholder="Phone Number"]');
      npPhone.value = '999999';
      npPhone.dispatchEvent(new Event('input'));

      const selects = fixture.nativeElement.querySelectorAll('select.form-select-sm');
      const npRelSelect = selects[selects.length - 1]; // last select is new parent relation
      npRelSelect.value = 'Father';
      npRelSelect.dispatchEvent(new Event('change'));

      expect(component.newParents()[0].name).toBe('NP Name');
      expect(component.newParents()[0].email).toBe('np@email.com');
      expect(component.newParents()[0].phonenumber).toBe('999999');
      expect(component.newParents()[0].relation).toBe('Father');

      const removeBtn = fixture.nativeElement.querySelector('button.btn-outline-danger');
      removeBtn.dispatchEvent(new Event('click'));
      expect(component.newParents().length).toBe(0);
    });

    it('should trigger cancel and prevStep from template', () => {
      component.currentStep.set(2);
      fixture.detectChanges();

      const cancelBtn = Array.from(fixture.nativeElement.querySelectorAll('button')).find((b: any) => b.textContent?.includes('Cancel')) as HTMLButtonElement;
      if (cancelBtn) cancelBtn.dispatchEvent(new Event('click'));
      expect(component.isCancelled).toBe(true);

      const prevBtn = Array.from(fixture.nativeElement.querySelectorAll('button')).find((b: any) => b.textContent?.includes('Back')) as HTMLButtonElement;
      if (prevBtn) prevBtn.dispatchEvent(new Event('click'));
      expect(component.currentStep()).toBe(1);
    });
  });
});
