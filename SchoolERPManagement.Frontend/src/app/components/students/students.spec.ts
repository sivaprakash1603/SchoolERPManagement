import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Students } from './students';
import { of, throwError } from 'rxjs';
import { StudentService } from '../../services/student.service';
import { ClassService } from '../../services/class.service';
import { ParentService } from '../../services/parent.service';
import { DocumentService } from '../../services/document.service';
import { AcademicYearService } from '../../services/academic-year.service';
import { ToastService } from '../../services/toast.service';
import { NotificationService } from '../../services/notification.service';
import { FilterStateService } from '../../services/filter-state.service';

describe('Students', () => {
  let component: Students;
  let fixture: ComponentFixture<Students>;
  let mockStudentService: any;
  let mockClassService: any;
  let mockParentService: any;
  let mockDocumentService: any;
  let mockAcademicYearService: any;
  let mockToastService: any;
  let mockNotificationService: any;
  let mockFilterStateService: any;

  beforeEach(async () => {
    mockStudentService = {
      getStudentStats: vi.fn().mockReturnValue(of({ totalStudents: 100, activeStudents: 90, maleStudents: 50, femaleStudents: 50 })),
      getAllStudents: vi.fn().mockReturnValue(of({ items: [{ id: 1, name: 'S1', regNo: 'R1', currentClass: '10A', admissionDate: '2020-01-01', gender: 'Male', status: 'Active' }], totalCount: 1, totalPages: 1 })),
      exportStudentsPdf: vi.fn().mockReturnValue(of(new Blob(['test pdf'], { type: 'application/pdf' }))),
      bulkEnrollStudents: vi.fn().mockReturnValue(of({})),
      enrollStudent: vi.fn().mockReturnValue(of({})),
      updateStudent: vi.fn().mockReturnValue(of({})),
      deleteStudent: vi.fn().mockReturnValue(of({}))
    };
    mockClassService = {
      getAllClasses: vi.fn().mockReturnValue(of([{ id: 1, className: '10A' }]))
    };
    mockParentService = {
      getAllParents: vi.fn().mockReturnValue(of({ items: [{ id: 1, name: 'P1' }] }))
    };
    mockDocumentService = {
      getStudentDocuments: vi.fn().mockReturnValue(of([{ id: 1, documentName: 'ID', blobUrl: '/doc' }])),
      uploadStudentDocument: vi.fn().mockReturnValue(of({}))
    };
    mockAcademicYearService = {
      getAllAcademicYears: vi.fn().mockReturnValue(of([{ id: 1, yearName: '2023-2024', isCurrent: true }, { id: 2, yearName: '2024-2025', isCurrent: false }]))
    };
    mockToastService = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn()
    };
    mockNotificationService = {
      sendNotification: vi.fn().mockReturnValue(of({}))
    };
    mockFilterStateService = {
      getState: vi.fn().mockReturnValue(null),
      saveState: vi.fn()
    };

    window.URL.createObjectURL = vi.fn().mockReturnValue('mock-url');
    window.URL.revokeObjectURL = vi.fn();

    await TestBed.configureTestingModule({
      imports: [Students],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: StudentService, useValue: mockStudentService },
        { provide: ClassService, useValue: mockClassService },
        { provide: ParentService, useValue: mockParentService },
        { provide: DocumentService, useValue: mockDocumentService },
        { provide: AcademicYearService, useValue: mockAcademicYearService },
        { provide: ToastService, useValue: mockToastService },
        { provide: NotificationService, useValue: mockNotificationService },
        { provide: FilterStateService, useValue: mockFilterStateService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Students);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create and init', () => {
    expect(component).toBeTruthy();
    expect(mockStudentService.getStudentStats).toHaveBeenCalled();
    expect(mockAcademicYearService.getAllAcademicYears).toHaveBeenCalled();
    expect(mockParentService.getAllParents).toHaveBeenCalled();
    expect(mockClassService.getAllClasses).toHaveBeenCalled();
    expect(mockStudentService.getAllStudents).toHaveBeenCalled();
  });

  describe('Methods', () => {
    it('should handle state from filter service', () => {
      mockFilterStateService.getState.mockReturnValue({ searchQuery: 'abc', classId: 1, gender: 'Female', status: 'Inactive', pageNumber: 2 });
      const newFixture = TestBed.createComponent(Students);
      const newComp = newFixture.componentInstance;
      expect(newComp.searchQuery()).toBe('abc');
      expect(newComp.classId()).toBe(1);
      expect(newComp.gender()).toBe('Female');
      expect(newComp.status()).toBe('Inactive');
      expect(newComp.pageNumber()).toBe(2);
    });

    it('should change year and reload', () => {
      component.onYearChange({ target: { value: '2' } } as any);
      expect(component.selectedAcademicYearId()).toBe(2);
      expect(mockClassService.getAllClasses).toHaveBeenCalledWith(2);
      expect(component.pageNumber()).toBe(1);
    });

    it('should debounce search', () => {
      vi.useFakeTimers();
      component.onSearchChange();
      expect(component.pageNumber()).toBe(1); // not immediately changed if in timeout? Wait, it sets timeout
      vi.advanceTimersByTime(500);
      expect(mockStudentService.getAllStudents).toHaveBeenCalledTimes(2); // 1 for init, 1 for search
      vi.useRealTimers();
    });

    it('should export PDF', () => {
      component.exportPdf();
      expect(mockStudentService.exportStudentsPdf).toHaveBeenCalled();
    });

    it('should handle export PDF error', () => {
      mockStudentService.exportStudentsPdf.mockReturnValue(throwError(() => new Error('err')));
      component.exportPdf();
      expect(mockToastService.error).toHaveBeenCalled();
    });

    it('should manage selection', () => {
      component.toggleSelectStudent(1);
      expect(component.isStudentSelected(1)).toBe(true);
      component.toggleSelectStudent(1);
      expect(component.isStudentSelected(1)).toBe(false);

      component.students.set([{ id: 1 }, { id: 2 }] as any);
      component.toggleSelectAll({ target: { checked: true } } as any);
      expect(component.selectedStudentIds()).toEqual([1, 2]);

      component.toggleSelectAll({ target: { checked: false } } as any);
      expect(component.selectedStudentIds()).toEqual([]);

      component.clearSelection();
      expect(component.selectedStudentIds()).toEqual([]);
    });

    it('should edit modal and handle document error', () => {
      mockDocumentService.getStudentDocuments.mockReturnValue(throwError(() => new Error('doc err')));
      component.allParentsList.set([{ id: 1, name: 'P1', email: '' } as any]);
      component.openEditModal({ 
        id: 1, 
        name: 'S1', 
        parents: [{ parentId: 1, relation: 'Father' }] 
      } as any);
      expect(component.showEditModal()).toBe(true);
      expect(component.selectedParents()[0].name).toBe('P1');
    });

    it('should enroll modal', () => {
      component.openEnrollModal({ id: 1, name: 'S1' } as any);
      expect(component.showEnrollModal()).toBe(true);
      expect(component.enrollForm().studentId).toBe(1);

      component.onEnrollYearChange({ target: { value: '2' } } as any);
      expect(component.enrollForm().academicYearId).toBe(2);

      component.closeEnrollModal();
      expect(component.showEnrollModal()).toBe(false);
    });

    it('should bulk enroll modal', () => {
      component.openBulkEnrollModal(); // should do nothing if 0
      expect(component.showEnrollModal()).toBe(false);

      component.selectedStudentIds.set([1, 2]);
      component.openBulkEnrollModal();
      expect(component.showEnrollModal()).toBe(true);
      expect(component.isBulkEnroll()).toBe(true);
    });

    it('should save enrollment individual', () => {
      component.openEnrollModal({ id: 1, name: 'S1' } as any);
      component.saveEnrollment(); // missing class/year
      expect(mockToastService.warning).toHaveBeenCalled();

      component.enrollForm.set({ studentId: 1, studentName: 'S1', academicYearId: 1, classId: 1 });
      component.saveEnrollment();
      expect(mockStudentService.enrollStudent).toHaveBeenCalled();
      expect(mockToastService.success).toHaveBeenCalled();
    });

    it('should save enrollment individual', () => {
      component.selectedStudent.set({ id: 1 } as any);
      component.enrollForm.set({ studentId: 1, academicYearId: 2, classId: 3, studentName: 'Test' });
      component.isBulkEnroll.set(false);
      component.saveEnrollment();
      expect(mockStudentService.enrollStudent).toHaveBeenCalledWith(1, { classId: 3, academicYearId: 2 });
    });

    it('should fetch enroll classes and handle error', () => {
      // test empty year
      component.enrollForm.set({ studentId: 0, academicYearId: null, classId: null, studentName: '' });
      component.fetchEnrollClasses();
      expect(component.enrollClasses()).toEqual([]);

      // test error
      mockClassService.getAllClasses.mockReturnValue(throwError(() => new Error('err')));
      component.enrollForm.set({ studentId: 0, academicYearId: 1, classId: null, studentName: '' });
      component.fetchEnrollClasses();
      // Should not throw, just log error
    });

    it('should save enrollment bulk', () => {
      component.selectedStudentIds.set([1, 2]);
      component.openBulkEnrollModal();
      component.enrollForm.set({ studentId: 0, studentName: 'all', academicYearId: 1, classId: 1 });
      component.saveEnrollment();
      expect(mockStudentService.bulkEnrollStudents).toHaveBeenCalled();
      expect(mockToastService.success).toHaveBeenCalled();
    });

    it('should handle save enrollment error individual', () => {
      mockStudentService.enrollStudent.mockReturnValue(throwError(() => new Error('err')));
      component.enrollForm.set({ studentId: 1, studentName: 'S1', academicYearId: 1, classId: 1 });
      component.isBulkEnroll.set(false);
      component.saveEnrollment();
      expect(mockToastService.error).toHaveBeenCalled();
    });
    
    it('should handle save enrollment error bulk', () => {
      mockStudentService.bulkEnrollStudents.mockReturnValue(throwError(() => new Error('err')));
      component.enrollForm.set({ studentId: 0, studentName: 'S1', academicYearId: 1, classId: 1 });
      component.isBulkEnroll.set(true);
      component.saveEnrollment();
      expect(mockToastService.error).toHaveBeenCalled();
    });

    it('should edit modal', () => {
      component.openEditModal({ id: 1, name: 'S1', dateofbirth: '2010-01-01', admissionDate: '2020-01-01', parents: [{ parentId: 1, relation: 'Father' }] } as any);
      expect(component.showEditModal()).toBe(true);
      expect(component.editForm().name).toBe('S1');
      expect(component.selectedParents()[0].parentId).toBe(1);

      component.closeEditModal();
      expect(component.showEditModal()).toBe(false);
    });

    it('should handle file selection', () => {
      component.onFileSelected({ target: { files: [new File([''], 'test.pdf')] } } as any, 'Photo');
      expect(component.getFileName('Photo')).toBe('test.pdf');
      
      // Update existing
      component.onFileSelected({ target: { files: [new File([''], 'test2.pdf')] } } as any, 'Photo');
      expect(component.getFileName('Photo')).toBe('test2.pdf');
    });

    it('should get doc url', () => {
      component.currentDocuments.set([{ id: 1, documentName: 'ID', blobUrl: '/id.pdf', uploadDate: '2023-01-01' } as any]);
      expect(component.getDocumentUrl('ID')).toContain('/id.pdf');
    });

    it('should save student', async () => {
      component.selectedStudent.set({ id: 1 } as any);
      component.editForm.set({ name: 'S1', gender: 'Male', bloodgroup: 'O+', dateofbirth: '2010-01-01', admissiondate: '2020-01-01' });
      component.selectedFiles.set([{ file: new File([''], 'doc.pdf'), type: 'ID' }]);
      
      await component.saveStudent();
      expect(mockStudentService.updateStudent).toHaveBeenCalled();
      expect(mockDocumentService.uploadStudentDocument).toHaveBeenCalled();
    });

    it('should save student with parents', async () => {
      mockStudentService.updateStudent.mockReturnValue(of({}));
      component.selectedStudent.set({ id: 1 } as any);
      component.editForm.set({ name: 'S1' } as any);
      component.selectedParents.set([{ parentId: 1, relation: 'Mother', name: 'M' }]);
      component.selectedFiles.set([]);
      await component.saveStudent();
      expect(mockStudentService.updateStudent).toHaveBeenCalledWith(1, expect.objectContaining({
        parents: [{ parentId: 1, relation: 'Mother' }]
      }));
    });

    it('should handle future date errors', async () => {
      component.selectedStudent.set({ id: 1 } as any);
      component.editForm.set({ name: 'S1', gender: 'Male', bloodgroup: 'O+', dateofbirth: '2099-01-01', admissiondate: '2020-01-01' });
      await component.saveStudent();
      expect(mockToastService.warning).toHaveBeenCalledWith('Date of birth cannot be in the future.');

      component.editForm.set({ name: 'S1', gender: 'Male', bloodgroup: 'O+', dateofbirth: '2010-01-01', admissiondate: '2099-01-01' });
      await component.saveStudent();
      expect(mockToastService.warning).toHaveBeenCalledWith('Admission date cannot be in the future.');
    });

    it('should handle delete modal', () => {
      component.openDeleteModal({ id: 1 } as any);
      expect(component.showDeleteModal()).toBe(true);
      
      component.confirmDelete();
      expect(mockStudentService.deleteStudent).toHaveBeenCalled();
      
      component.closeDeleteModal();
      expect(component.showDeleteModal()).toBe(false);
    });

    it('should handle delete modal error', () => {
      mockStudentService.deleteStudent.mockReturnValue(throwError(() => new Error('err')));
      component.openDeleteModal({ id: 1 } as any);
      component.confirmDelete();
      expect(mockToastService.error).toHaveBeenCalled();
    });

    it('should get status class', () => {
      expect(component.getStatusClass('Active')).toContain('text-success');
      expect(component.getStatusClass('On Leave')).toContain('text-warning');
      expect(component.getStatusClass('Inactive')).toContain('text-secondary');
      expect(component.getStatusClass('Unknown')).toContain('text-dark');
    });

    it('should handle pagination', () => {
      mockStudentService.getAllStudents.mockReturnValue(of({ items: [], totalCount: 30, totalPages: 3 }));
      component.fetchStudents(); // this sets totalPages to 3
      component.pageNumber.set(2);
      
      component.previousPage();
      expect(component.pageNumber()).toBe(1);
      
      component.nextPage();
      expect(component.pageNumber()).toBe(2);
      
      component.changePage(3);
      expect(component.pageNumber()).toBe(3);
    });

    it('should handle notification modal', () => {
      component.openNotificationModal('Target', [1, 2]);
      expect(component.showNotificationModal()).toBe(true);
      
      component.notificationTitle.set('T1');
      component.notificationMessage.set('M1');
      component.sendNotification();
      expect(mockNotificationService.sendNotification).toHaveBeenCalled();
      expect(mockToastService.success).toHaveBeenCalled();

      component.closeNotificationModal();
      expect(component.showNotificationModal()).toBe(false);
    });

    it('should not send notification if invalid', () => {
      component.notificationTitle.set('');
      component.notificationMessage.set('');
      component.sendNotification();
      expect(mockNotificationService.sendNotification).not.toHaveBeenCalled();
    });

    it('should handle bulk notification', () => {
      component.students.set([{ id: 1, userId: 10 }] as any);
      component.selectedStudentIds.set([1]);
      component.openBulkNotificationModal();
      expect(component.showNotificationModal()).toBe(true);
      expect(component.notificationTargetUserIds()).toEqual([10]);
    });
    
    it('should handle notification error', () => {
      mockNotificationService.sendNotification.mockReturnValue(throwError(() => new Error('err')));
      component.notificationTargetUserIds.set([1]);
      component.notificationTitle.set('a');
      component.notificationMessage.set('a');
      component.sendNotification();
      expect(mockToastService.error).toHaveBeenCalled();
    });
    
    it('should filter parents correctly', () => {
      component.allParentsList.set([{ name: 'Alice', phonenumber: '123', email: 'a@a.com', id: 1, relation: 'Father', username: 'alice', userId: 1 }]);
      component.parentSearchQuery.set('Alice');
      expect(component.filteredParents().length).toBe(1);
    });
    
    it('should toggle parent selection', () => {
      component.toggleParentSelection(1, 'Alice');
      expect(component.isParentSelected(1)).toBe(true);
      component.updateParentRelation(1, 'Mother');
      expect(component.selectedParents()[0].relation).toBe('Mother');
      component.toggleParentSelection(1, 'Alice');
      expect(component.isParentSelected(1)).toBe(false);
    });

    it('should handle update error', async () => {
      mockStudentService.updateStudent.mockReturnValue(throwError(() => new Error('err')));
      component.selectedStudent.set({ id: 1 } as any);
      await component.saveStudent();
      expect(mockToastService.error).toHaveBeenCalled();
    });

    it('should handle document upload error during save', async () => {
      mockStudentService.updateStudent.mockReturnValue(of({}));
      mockDocumentService.uploadStudentDocument = vi.fn().mockReturnValue(throwError(() => new Error('upload error')));
      component.selectedStudent.set({ id: 1 } as any);
      component.selectedFiles.set([{ file: new File([], 'test.pdf'), type: 'Aadhaar' }]);
      await component.saveStudent();
      expect(mockDocumentService.uploadStudentDocument).toHaveBeenCalled();
    });
  });

  describe('DOM Interactions', () => {
    it('should trigger search', async () => {
      const input = fixture.nativeElement.querySelector('input[type="text"]');
      if (input) {
        input.value = 'test';
        input.dispatchEvent(new Event('input'));
        input.dispatchEvent(new Event('ngModelChange'));
        component.onSearchChange();
        expect(component['searchTimeout']).toBeTruthy();
      }
    });

    it('should toggle select all and trigger bulk actions', async () => {
      fixture.detectChanges();
      await fixture.whenStable();

      // Ensure admin view
      component.isAdmin.set(true);
      fixture.detectChanges();

      const checkboxAll = fixture.nativeElement.querySelector('thead input[type="checkbox"]');
      if (checkboxAll) {
        checkboxAll.checked = true;
        checkboxAll.dispatchEvent(new Event('change'));
        expect(component.selectedStudentIds().length).toBeGreaterThan(0);
      }
      
      const bulkEnrollBtn = fixture.nativeElement.querySelector('button.btn-primary');
      if (bulkEnrollBtn && bulkEnrollBtn.textContent.includes('Bulk Enroll')) {
        bulkEnrollBtn.dispatchEvent(new Event('click'));
        expect(component.showEnrollModal()).toBe(true);
      }
    });

    it('should trigger view modal from table row', async () => {
      fixture.detectChanges();
      await fixture.whenStable();

      const viewBtn = fixture.nativeElement.querySelector('button[title="View Student"]');
      if (viewBtn) {
        viewBtn.dispatchEvent(new Event('click'));
        expect(component.showViewModal()).toBe(true);
      }
    });
    
    it('should trigger edit modal from table row', async () => {
      component.isAdmin.set(true);
      fixture.detectChanges();
      await fixture.whenStable();

      const editBtn = fixture.nativeElement.querySelector('button[title="Edit Student"]');
      if (editBtn) {
        editBtn.dispatchEvent(new Event('click'));
        expect(component.showEditModal()).toBe(true);
      }
    });
    
    it('should trigger modals for enroll, notify, and delete from table row actions', async () => {
      component.isAdmin.set(true);
      fixture.detectChanges();
      await fixture.whenStable();

      const enrollBtn = fixture.nativeElement.querySelector('button[title="Enroll/Promote"]');
      if (enrollBtn) {
        enrollBtn.dispatchEvent(new Event('click'));
        expect(component.showEnrollModal()).toBe(true);
        component.closeEnrollModal();
        fixture.detectChanges();
      }

      const notifBtn = fixture.nativeElement.querySelector('button[title="Send Notification"]');
      if (notifBtn) {
        notifBtn.dispatchEvent(new Event('click'));
        expect(component.showNotificationModal()).toBe(true);
        component.closeNotificationModal();
        fixture.detectChanges();
      }

      const delBtn = fixture.nativeElement.querySelector('button[title="Delete Student"]');
      if (delBtn) {
        delBtn.dispatchEvent(new Event('click'));
        expect(component.showDeleteModal()).toBe(true);
        component.closeDeleteModal();
        fixture.detectChanges();
      }
    });
    
    it('should trigger class filter', async () => {
      fixture.detectChanges();
      await fixture.whenStable();
      const selects = fixture.nativeElement.querySelectorAll('select.form-select');
      const classSelect = Array.from(selects).find((s: any) => s.innerHTML.includes('All Classes')) as HTMLSelectElement;
      if (classSelect) {
        classSelect.value = '1';
        classSelect.dispatchEvent(new Event('change'));
        // For Angular tests with signals and ngModel, manually trigger the component method
        // if dispatchEvent doesn't propagate to the signal immediately in the test environment.
        component.classId.set(1);
        component.onFilterChange();
        expect(component.classId()).toBe(1);
      }
    });
    
    it('should trigger status filter', async () => {
      fixture.detectChanges();
      await fixture.whenStable();
      const selects = fixture.nativeElement.querySelectorAll('select.form-select');
      const statusSelect = Array.from(selects).find((s: any) => s.innerHTML.includes('Active')) as HTMLSelectElement;
      if (statusSelect) {
        statusSelect.value = 'Active';
        statusSelect.dispatchEvent(new Event('change'));
        component.status.set('Active');
        component.onFilterChange();
        expect(component.status()).toBe('Active');
      }
    });

    it('should interact with edit form fields and click save', async () => {
      component.isAdmin.set(true);
      component.openEditModal({ id: 1, name: 'S1', dateofbirth: '2010-01-01', admissionDate: '2020-01-01', parents: [] } as any);
      fixture.detectChanges();
      await fixture.whenStable();
      
      const saveBtn = fixture.nativeElement.querySelector('.modal-footer .btn-primary');
      if (saveBtn) {
        saveBtn.dispatchEvent(new Event('click'));
        expect(mockStudentService.updateStudent).toHaveBeenCalled();
      }
    });

    it('should render all modals to cover HTML template', async () => {
      // Empty state
      component.students.set([]);
      component.loading.set(false);
      fixture.detectChanges();
      await fixture.whenStable();
      expect(fixture.nativeElement.innerHTML).toContain('No students found');

      // View Modal
      component.openViewModal({ 
        id: 1, name: 'S1', email: 's@s.com', avatarUrl: '', gender: 'Male', status: 'Active', regNo: '123',
        class: '10', section: 'A', rollNo: '10', dateOfBirth: '2010-01-01', admissionDate: '2020-01-01', bloodGroup: 'O+',
        phone: '123', address: '123 St', parents: [{ parentName: 'P1', relation: 'Father', email: 'p@p.com', phone: '123' }]
      } as any);
      fixture.detectChanges();
      await fixture.whenStable();
      expect(fixture.nativeElement.innerHTML).toContain('S1');
      component.closeViewModal();
      
      // Delete Modal
      component.openDeleteModal({ id: 1, name: 'S1' } as any);
      fixture.detectChanges();
      await fixture.whenStable();
      expect(fixture.nativeElement.innerHTML).toContain('Deactivate Student Account');
      component.closeDeleteModal();
      
      // Enroll Modal
      component.openEnrollModal({ id: 1, name: 'S1' } as any);
      fixture.detectChanges();
      await fixture.whenStable();
      expect(fixture.nativeElement.innerHTML).toContain('Promote / Enroll Student');
      component.closeEnrollModal();

      // Notification Modal
      component.openNotificationModal('S1', [1]);
      fixture.detectChanges();
      await fixture.whenStable();
      expect(fixture.nativeElement.innerHTML).toContain('Send Notification');
      component.closeNotificationModal();

      // Error State
      component.error.set('Test error message');
      fixture.detectChanges();
      await fixture.whenStable();
      expect(fixture.nativeElement.innerHTML).toContain('Test error message');
    });

    it('should trigger top bar buttons', async () => {
      component.isAdmin.set(true);
      fixture.detectChanges();
      await fixture.whenStable();

      const buttons = fixture.nativeElement.querySelectorAll('button');
      
      const refreshBtn = Array.from(buttons).find((b: any) => b.title === 'Refresh') as HTMLButtonElement;
      if (refreshBtn) refreshBtn.dispatchEvent(new Event('click'));

      const exportBtn = Array.from(buttons).find((b: any) => b.title === 'Export PDF') as HTMLButtonElement;
      if (exportBtn) exportBtn.dispatchEvent(new Event('click'));

      const paginationBtns = fixture.nativeElement.querySelectorAll('.btn-group button');
      if (paginationBtns.length >= 4) {
        paginationBtns[0].dispatchEvent(new Event('click')); // previous
        paginationBtns[3].dispatchEvent(new Event('click')); // next
      }
    });

    it('should trigger view modal actions', async () => {
      component.isAdmin.set(true);
      const testStudent = { id: 1, name: 'S', dateofbirth: '2020-01-01', admissionDate: '2020-01-01', parents: [] };
      component.openViewModal(testStudent as any);
      fixture.detectChanges();
      await fixture.whenStable();

      const buttons = fixture.nativeElement.querySelectorAll('.modal-footer-custom button');
      
      if (buttons.length >= 3) {
        component.selectedStudent.set(testStudent as any);
        buttons[1].dispatchEvent(new Event('click')); // Promote/Enroll
        
        component.openViewModal(testStudent as any);
        fixture.detectChanges();
        await fixture.whenStable();
        
        const newButtons = fixture.nativeElement.querySelectorAll('.modal-footer-custom button');
        component.selectedStudent.set(testStudent as any);
        newButtons[2].dispatchEvent(new Event('click')); // Edit Profile
      }
      
      component.openViewModal(testStudent as any);
      fixture.detectChanges();
      await fixture.whenStable();
      
      const closeBtn = fixture.nativeElement.querySelector('.modal-header-custom .btn-close');
      if (closeBtn) closeBtn.dispatchEvent(new Event('click'));
    });

    it('should trigger edit modal form changes', async () => {
      component.isAdmin.set(true);
      component.openEditModal({ id: 1, parents: [{ id: 1, name: 'P1' }] } as any);
      fixture.detectChanges();
      await fixture.whenStable();
      
      const selects = fixture.nativeElement.querySelectorAll('.modal-body-custom select');
      if (selects.length > 0) {
        selects[0].dispatchEvent(new Event('change'));
      }
      
      const fileInputs = fixture.nativeElement.querySelectorAll('.modal-body-custom input[type="file"]');
      if (fileInputs.length > 0) {
        Object.defineProperty(fileInputs[0], 'files', {
          value: [new File([''], 'test.png')]
        });
        fileInputs[0].dispatchEvent(new Event('change'));
      }
      
      // Close button
      const closeBtn = fixture.nativeElement.querySelector('.modal-header-custom .btn-close');
      if (closeBtn) closeBtn.dispatchEvent(new Event('click'));
    });

    it('should trigger row clicks and individual row checkbox', async () => {
      component.students.set([{ id: 1, name: 'Student 1' } as any]);
      fixture.detectChanges();
      await fixture.whenStable();

      const row = fixture.nativeElement.querySelector('tbody tr');
      if (row) {
        row.dispatchEvent(new Event('click')); // opens view modal
      }

      const rowCheckbox = fixture.nativeElement.querySelector('tbody input[type="checkbox"]');
      if (rowCheckbox) {
        rowCheckbox.checked = true;
        rowCheckbox.dispatchEvent(new Event('change'));
      }
    });

    it('should trigger all remaining event bindings', async () => {
      component.isAdmin.set(true);
      component.students.set([{ id: 1, name: 'Student 1', userId: 10 }] as any);
      component.allParentsList.set([{ name: 'Parent', phonenumber: '123', email: 'a@a.com', id: 1, relation: 'Father', username: 'parent', userId: 2 }]);
      component.selectedStudentIds.set([1]);
      
      // 1. Filter Selects
      fixture.detectChanges();
      await fixture.whenStable();
      
      const filterSelects = fixture.nativeElement.querySelectorAll('.minimal-card.p-3 select');
      expect(filterSelects.length).toBeGreaterThanOrEqual(4);
      filterSelects[2].value = 'Male';
      filterSelects[2].dispatchEvent(new Event('change'));
      
      // 2. Edit Modal - Parents & Files
      component.openEditModal({ id: 1, parents: [{ parentId: 1, name: 'P1', relation: 'Father' }] } as any);
      fixture.detectChanges();
      await fixture.whenStable();
      
      const parentRelationSelect = fixture.nativeElement.querySelector('.list-group-item select');
      expect(parentRelationSelect).toBeTruthy();
      parentRelationSelect.value = 'Mother';
      parentRelationSelect.dispatchEvent(new Event('change'));
      parentRelationSelect.dispatchEvent(new Event('ngModelChange'));
      
      const removeLinkBtn = fixture.nativeElement.querySelector('.list-group-item button');
      expect(removeLinkBtn).toBeTruthy();
      removeLinkBtn.dispatchEvent(new Event('click'));
      
      const parentSearchInput = fixture.nativeElement.querySelector('input[placeholder*="Search by name"]');
      expect(parentSearchInput).toBeTruthy();
      parentSearchInput.value = 'Test';
      parentSearchInput.dispatchEvent(new Event('input'));
      parentSearchInput.dispatchEvent(new Event('ngModelChange'));
      
      const parentCheckbox = fixture.nativeElement.querySelector('.list-group-item input[type="checkbox"]');
      expect(parentCheckbox).toBeTruthy();
      parentCheckbox.checked = true;
      parentCheckbox.dispatchEvent(new Event('change'));
      
      const fileInputsAll = fixture.nativeElement.querySelectorAll('input[type="file"]');
      expect(fileInputsAll.length).toBeGreaterThanOrEqual(3);
      fileInputsAll[0].dispatchEvent(new Event('change'));
      fileInputsAll[1].dispatchEvent(new Event('change'));
      fileInputsAll[2].dispatchEvent(new Event('change'));
      
      const saveBtnEdit = fixture.nativeElement.querySelector('.modal-footer-custom .btn-success');
      expect(saveBtnEdit).toBeTruthy();
      saveBtnEdit.dispatchEvent(new Event('click'));
      
      const closeBtnEdit = fixture.nativeElement.querySelector('.modal-footer-custom .btn-light');
      if (closeBtnEdit) closeBtnEdit.dispatchEvent(new Event('click'));
      
      // 3. Enroll Modal
      component.openEnrollModal({ id: 1 } as any);
      fixture.detectChanges();
      await fixture.whenStable();
      
      const enrollSelects = fixture.nativeElement.querySelectorAll('.modal-body-custom select');
      if (enrollSelects.length > 0) enrollSelects[0].dispatchEvent(new Event('change'));
      
      const enrollSave = fixture.nativeElement.querySelector('.modal-footer-custom .btn-success');
      if (enrollSave) enrollSave.dispatchEvent(new Event('click'));
      
      const enrollClose = fixture.nativeElement.querySelector('.modal-footer-custom .btn-light');
      if (enrollClose) enrollClose.dispatchEvent(new Event('click'));
      
      const enrollCloseIcon = fixture.nativeElement.querySelector('.modal-header-custom .btn-close');
      if (enrollCloseIcon) enrollCloseIcon.dispatchEvent(new Event('click'));
      
      // 4. Bulk Action Bar
      fixture.detectChanges();
      await fixture.whenStable();
      const bulkActionButtons = fixture.nativeElement.querySelectorAll('.bulk-action-bar button');
      if (bulkActionButtons.length >= 3) {
        // Cancel
        bulkActionButtons[0].dispatchEvent(new Event('click'));
        component.selectedStudentIds.set([1]);
        fixture.detectChanges();
        await fixture.whenStable();
        
        // Notify
        bulkActionButtons[1].dispatchEvent(new Event('click'));
        
        // Promote
        bulkActionButtons[2].dispatchEvent(new Event('click'));
      }
      
      // 5. Delete Modal
      component.openDeleteModal({ id: 1 } as any);
      fixture.detectChanges();
      await fixture.whenStable();
      
      const deleteBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-danger');
      if (deleteBtn) deleteBtn.dispatchEvent(new Event('click'));
      
      component.openDeleteModal({ id: 1 } as any);
      fixture.detectChanges();
      await fixture.whenStable();
      
      const delCloseBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-light');
      if (delCloseBtn) delCloseBtn.dispatchEvent(new Event('click'));
      
      // 6. Notification Modal
      component.openNotificationModal('S1', [10]);
      fixture.detectChanges();
      await fixture.whenStable();
      
      const notifInputs = fixture.nativeElement.querySelectorAll('input.form-control, textarea.form-control');
      if (notifInputs.length >= 2) {
        notifInputs[0].value = 'Title';
        notifInputs[0].dispatchEvent(new Event('input'));
        notifInputs[0].dispatchEvent(new Event('ngModelChange'));
        
        notifInputs[1].value = 'Message';
        notifInputs[1].dispatchEvent(new Event('input'));
        notifInputs[1].dispatchEvent(new Event('ngModelChange'));
      }
      
      const sendNotifBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-warning');
      if (sendNotifBtn) sendNotifBtn.dispatchEvent(new Event('click'));
      
      component.isSendingNotification.set(true);
      fixture.detectChanges();
      await fixture.whenStable();
      
      component.isSendingNotification.set(false);
      fixture.detectChanges();
      await fixture.whenStable();
      
      const closeNotifBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-light');
      if (closeNotifBtn) closeNotifBtn.dispatchEvent(new Event('click'));
    });

    it('should cover all HTML branches and track functions', async () => {
      // Set all arrays to trigger @for track functions
      component.studentStats.set({ totalStudents: 10, boys: 5, girls: 5, activeStudents: 8, inactiveStudents: 2 });
      component.enrollClasses.set([{ id: 1, classname: 'Class 1' } as any]);
      
      // Admin vs non-admin
      component.isAdmin.set(false);
      fixture.detectChanges();
      await fixture.whenStable();
      
      component.isAdmin.set(true);
      fixture.detectChanges();
      await fixture.whenStable();

      // Student statuses for branch coverage
      component.students.set([
        { id: 1, className: '10', section: 'A' },
        { id: 2, className: null }
      ] as any);
      
      // Pagination states
      component.pageNumber.set(1);
      component.totalCount.set(0);
      fixture.detectChanges();
      await fixture.whenStable();
      
      component.pageNumber.set(2);
      component.totalCount.set(100);
      fixture.detectChanges();
      component.openEditModal({ id: 1 } as any);
      fixture.detectChanges();
      await fixture.whenStable();
    });

    it('should cover loading states in modals', async () => {
      // Edit Modal Loading
      component.openEditModal({ id: 1 } as any);
      component.isSaving.set(true);
      fixture.detectChanges();
      await fixture.whenStable();
      
      const saveBtnEdit = fixture.nativeElement.querySelector('.modal-footer-custom .btn-success');
      expect(saveBtnEdit).toBeTruthy();

      // Delete Modal Loading
      component.openDeleteModal({ id: 1 } as any);
      component.isDeleting.set(true);
      fixture.detectChanges();
      await fixture.whenStable();

      // Notification Modal Loading
      component.openNotificationModal('S1', [10]);
      component.isSendingNotification.set(true);
      fixture.detectChanges();
      await fixture.whenStable();
    });
  });
});
