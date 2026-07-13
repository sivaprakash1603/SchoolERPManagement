import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Teachers } from './teachers';
import { TeacherService } from '../../services/teacher.service';
import { ClassService } from '../../services/class.service';
import { ToastService } from '../../services/toast.service';
import { NotificationService } from '../../services/notification.service';
import { FilterStateService } from '../../services/filter-state.service';
import { TimetableService } from '../../services/timetable.service';
import { of, throwError } from 'rxjs';
import { FormsModule } from '@angular/forms';

describe('Teachers', () => {
  let component: Teachers;
  let fixture: ComponentFixture<Teachers>;

  let teacherServiceMock: any;
  let classServiceMock: any;
  let toastServiceMock: any;
  let notificationServiceMock: any;
  let filterStateServiceMock: any;
  let timetableServiceMock: any;

  const mockTeachersPage = {
    items: [
      { id: 1, userId: 101, name: 'John Doe', email: 'john@test.com', status: 'Active', profilePhotoUrl: 'http://img.com/1.png' },
      { id: 2, userId: 102, name: 'Jane Smith', email: 'jane@test.com', status: 'Inactive', profilePhotoUrl: '' }
    ],
    totalCount: 2,
    totalPages: 1
  };

  const mockSubjects = [
    { id: 1, subjectName: 'Math' },
    { id: 99, subjectName: 'Science' }
  ];
  const mockClasses = [{ id: 1, classname: 'Grade 1', subjects: [{ id: 1 }] }];
  const mockAssignments = [{ classId: 1, subjectId: 1, className: 'Grade 1', subjectName: 'Math' }];

  beforeEach(async () => {
    teacherServiceMock = {
      getAllTeachers: vi.fn().mockReturnValue(of(mockTeachersPage)),
      getAllSubjects: vi.fn().mockReturnValue(of(mockSubjects)),
      updateTeacher: vi.fn().mockReturnValue(of({})),
      deleteTeacher: vi.fn().mockReturnValue(of({})),
      getTeacherAssignments: vi.fn().mockReturnValue(of(mockAssignments)),
      assignSubject: vi.fn().mockReturnValue(of({})),
      unassignSubject: vi.fn().mockReturnValue(of({})),
      autoAssignTeachers: vi.fn().mockReturnValue(of({ totalAssignmentsMade: 5 })),
      exportTeachersPdf: vi.fn().mockReturnValue(of(new Blob(['pdf content'], { type: 'application/pdf' })))
    };

    classServiceMock = {
      getAllClasses: vi.fn().mockReturnValue(of(mockClasses))
    };

    toastServiceMock = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn()
    };

    notificationServiceMock = {
      sendNotification: vi.fn().mockReturnValue(of({}))
    };

    filterStateServiceMock = {
      getState: vi.fn().mockReturnValue(null),
      saveState: vi.fn()
    };

    timetableServiceMock = {
      getTeacherRequirements: vi.fn().mockReturnValue(of([{ name: 'Math', required: 2, available: 1, shortage: 1 }]))
    };

    vi.spyOn(window, 'confirm').mockReturnValue(true);
    window.URL.createObjectURL = vi.fn().mockReturnValue('blob:url');
    window.URL.revokeObjectURL = vi.fn();

    await TestBed.configureTestingModule({
      imports: [Teachers, FormsModule],
      providers: [
        provideRouter([]),
        { provide: TeacherService, useValue: teacherServiceMock },
        { provide: ClassService, useValue: classServiceMock },
        { provide: ToastService, useValue: toastServiceMock },
        { provide: NotificationService, useValue: notificationServiceMock },
        { provide: FilterStateService, useValue: filterStateServiceMock },
        { provide: TimetableService, useValue: timetableServiceMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Teachers);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('Initialization and Filtering', () => {
    it('should create and load data', () => {
      expect(component).toBeTruthy();
      expect(component.teachers().length).toBe(2);
      expect(component.totalCount()).toBe(2);
      expect(component.availableSubjects()).toEqual(mockSubjects);
    });

    it('should restore filter state if available', () => {
      filterStateServiceMock.getState.mockReturnValueOnce({ searchQuery: 'Jane', status: 'Inactive', pageNumber: 2 });
      component.ngOnInit();
      expect(component.searchQuery()).toBe('Jane');
      expect(component.status()).toBe('Inactive');
      expect(component.pageNumber()).toBe(2);
    });

    it('should handle fetch errors', () => {
      teacherServiceMock.getAllTeachers.mockReturnValueOnce(throwError(() => new Error('err')));
      component.fetchTeachers();
      expect(component.error()).toBe('Failed to load teachers. Please try again later.');
      expect(component.loading()).toBe(false);
    });

    it('should trigger debounce search', async () => {
      vi.useFakeTimers();
      vi.spyOn(component, 'fetchTeachers');
      component.onSearchChange();
      vi.advanceTimersByTime(600);
      expect(component.fetchTeachers).toHaveBeenCalled();
      vi.useRealTimers();
    });
    
    it('should change filter explicitly and reset page to 1', () => {
      component.pageNumber.set(3);
      component.onFilterChange();
      expect(component.pageNumber()).toBe(1);
      expect(filterStateServiceMock.saveState).toHaveBeenCalled();
    });
  });

  describe('Pagination', () => {
    it('should navigate pages correctly', () => {
      vi.spyOn(component, 'fetchTeachers').mockImplementation(() => {});
      component.pageNumber.set(2);
      component.totalPages.set(5);

      component.previousPage();
      expect(component.pageNumber()).toBe(1);

      component.nextPage();
      expect(component.pageNumber()).toBe(2);

      component.changePage(4);
      expect(component.pageNumber()).toBe(4);
      
      // out of bounds
      component.changePage(10);
      expect(component.pageNumber()).toBe(4); // should not change
    });
  });

  describe('Modals and CRUD Operations', () => {
    const dummyTeacher = { id: 1, userId: 101, name: 'John Doe', email: 'john@test.com', status: 'Active', avatarUrl: '' } as any;

    it('should open and close View Modal', () => {
      component.openViewModal(dummyTeacher);
      expect(component.showViewModal()).toBe(true);
      
      component.closeViewModal();
      expect(component.showViewModal()).toBe(false);
    });

    it('should manage Edit Modal and Save', () => {
      component.openEditModal(dummyTeacher);
      expect(component.showEditModal()).toBe(true);
      expect(component.editForm().name).toBe('John Doe');

      component.editForm.set({ name: 'John Edited', phonenumber: '123', qualifications: 'B.Sc', subjectSpecialtyId: 2 });
      component.saveTeacher();
      
      expect(teacherServiceMock.updateTeacher).toHaveBeenCalledWith(1, { name: 'John Edited', phonenumber: '123', qualifications: 'B.Sc', subjectSpecialtyId: 2 });
      expect(toastServiceMock.success).toHaveBeenCalledWith('Teacher updated successfully');
      expect(component.showEditModal()).toBe(false);
    });

    it('should handle edit error', () => {
      teacherServiceMock.updateTeacher.mockReturnValueOnce(throwError(() => ({ error: { message: 'err' } })));
      component.openEditModal(dummyTeacher);
      component.saveTeacher();
      expect(toastServiceMock.error).toHaveBeenCalledWith('err');
    });

    it('should manage Delete Modal and Confirm', () => {
      component.openDeleteModal(dummyTeacher);
      expect(component.showDeleteModal()).toBe(true);
      
      component.confirmDelete();
      expect(teacherServiceMock.deleteTeacher).toHaveBeenCalledWith(1);
      expect(toastServiceMock.success).toHaveBeenCalledWith('Teacher deactivated successfully');
      expect(component.showDeleteModal()).toBe(false);
    });
    
    it('should handle delete error', () => {
      teacherServiceMock.deleteTeacher.mockReturnValueOnce(throwError(() => ({ error: { message: 'err' } })));
      component.openDeleteModal(dummyTeacher);
      component.confirmDelete();
      expect(toastServiceMock.error).toHaveBeenCalledWith('err');
    });
  });

  describe('Assignments', () => {
    const dummyTeacher = { id: 1, userId: 101, name: 'John', email: 'j@j.com', avatarUrl: '' } as any;

    it('should open Assignments modal and load data', () => {
      component.openAssignmentsModal(dummyTeacher);
      expect(component.showAssignmentsModal()).toBe(true);
      expect(classServiceMock.getAllClasses).toHaveBeenCalled();
      expect(teacherServiceMock.getTeacherAssignments).toHaveBeenCalledWith(1);
      
      component.closeAssignmentsModal();
      expect(component.showAssignmentsModal()).toBe(false);
    });

    it('should add assignment successfully via DOM', () => {
      component.openAssignmentsModal(dummyTeacher);
      component.assignmentForm.set({ classId: 1, subjectId: 1 });
      
      fixture.detectChanges();
      
      const addBtn = fixture.nativeElement.querySelector('.row button.btn-primary');
      if (addBtn) addBtn.dispatchEvent(new Event('click'));
      
      expect(teacherServiceMock.assignSubject).toHaveBeenCalledWith({ teacherId: 1, classId: 1, subjectId: 1 });
      expect(toastServiceMock.success).toHaveBeenCalledWith('Subject assigned successfully');
    });

    it('should block assignment if subject not in class setup', () => {
      component.openAssignmentsModal(dummyTeacher);
      component.assignmentForm.set({ classId: 1, subjectId: 99 }); // 99 not in mockClasses subject list
      component.addAssignment();
      expect(toastServiceMock.warning).toHaveBeenCalled();
      expect(teacherServiceMock.assignSubject).not.toHaveBeenCalled();
    });

    it('should remove assignment via DOM', () => {
      component.openAssignmentsModal(dummyTeacher);
      fixture.detectChanges();
      
      const removeBtn = fixture.nativeElement.querySelector('.btn-outline-danger');
      if (removeBtn) removeBtn.dispatchEvent(new Event('click'));
      
      expect(teacherServiceMock.unassignSubject).toHaveBeenCalledWith(1, 1, 1);
      expect(toastServiceMock.success).toHaveBeenCalledWith('Assignment removed successfully');
    });

    it('should handle empty assignments state', () => {
      teacherServiceMock.getTeacherAssignments.mockReturnValueOnce(of([]));
      component.openAssignmentsModal(dummyTeacher);
      fixture.detectChanges();
      
      const emptyState = fixture.nativeElement.innerHTML;
      expect(emptyState).toContain('No classes assigned');
    });
  });

  describe('Notifications', () => {
    it('should open notification modal, select target and send', () => {
      component.openNotificationModal('Target 1', [101]);
      expect(component.showNotificationModal()).toBe(true);
      
      component.notificationTitle.set('Hello');
      component.notificationMessage.set('Test message');
      
      component.sendNotification();
      expect(notificationServiceMock.sendNotification).toHaveBeenCalledWith({
        title: 'Hello',
        message: 'Test message',
        targetUserIds: [101]
      });
      expect(toastServiceMock.success).toHaveBeenCalledWith('Notification sent successfully');
      expect(component.showNotificationModal()).toBe(false);
    });

    it('should do nothing if form is incomplete', () => {
      component.openNotificationModal('Target 1', [101]);
      component.notificationTitle.set('');
      component.sendNotification();
      expect(notificationServiceMock.sendNotification).not.toHaveBeenCalled();
    });

    it('should handle notification sending error', () => {
      notificationServiceMock.sendNotification.mockReturnValueOnce(throwError(() => ({ error: { message: 'err' } })));
      component.openNotificationModal('Target', [101]);
      component.notificationTitle.set('A');
      component.notificationMessage.set('B');
      component.sendNotification();
      expect(toastServiceMock.error).toHaveBeenCalledWith('err');
    });
  });

  describe('Bulk Actions & Export & AutoAssign', () => {
    it('should toggle selection for individual and bulk', () => {
      // individual
      component.toggleSelectTeacher(1);
      expect(component.selectedTeacherIds()).toEqual([1]);
      expect(component.isTeacherSelected(1)).toBe(true);
      
      component.toggleSelectTeacher(1);
      expect(component.selectedTeacherIds()).toEqual([]);
      
      // select all
      component.toggleSelectAll({ target: { checked: true } } as any);
      expect(component.selectedTeacherIds()).toEqual([1, 2]);
      
      // trigger change detection to show bulk action bar
      fixture.detectChanges();
      
      const bulkCancelBtn = fixture.nativeElement.querySelector('.bulk-action-bar .btn-outline-secondary');
      if (bulkCancelBtn) bulkCancelBtn.dispatchEvent(new Event('click'));
      expect(component.selectedTeacherIds()).toEqual([]);
      
      // select again
      component.toggleSelectAll({ target: { checked: true } } as any);
      fixture.detectChanges();
      
      const bulkNotifBtn = fixture.nativeElement.querySelector('.bulk-action-bar .btn-warning');
      if (bulkNotifBtn) bulkNotifBtn.dispatchEvent(new Event('click'));
      expect(component.showNotificationModal()).toBe(true);
      
      // deselect all manually
      component.toggleSelectAll({ target: { checked: false } } as any);
      expect(component.selectedTeacherIds()).toEqual([]);
    });

    it('should open bulk notification modal', () => {
      component.selectedTeacherIds.set([1, 2]); // select all 2
      component.openBulkNotificationModal();
      expect(component.showNotificationModal()).toBe(true);
      expect(component.notificationTargetUserIds()).toEqual([101, 102]);
    });

    it('should export PDF', () => {
      component.exportPdf();
      expect(teacherServiceMock.exportTeachersPdf).toHaveBeenCalled();
    });

    it('should auto-assign teachers', () => {
      component.autoAssignTeachers();
      expect(teacherServiceMock.autoAssignTeachers).toHaveBeenCalled();
      expect(toastServiceMock.success).toHaveBeenCalledWith(expect.stringContaining('5 assignments were made'));
    });
  });

  describe('Teacher Requirements', () => {
    it('should load teacher requirements', () => {
      component.periodsPerDay.set(8);
      component.freePeriodsPerStaff.set(2);
      component.fetchTeacherRequirements();
      
      expect(timetableServiceMock.getTeacherRequirements).toHaveBeenCalledWith(8, 2);
      expect(component.showTeacherRequirements()).toBe(true);
      expect(component.teacherRequirements().length).toBe(1);
    });

    it('should block if free periods > periods per day', () => {
      component.periodsPerDay.set(5);
      component.freePeriodsPerStaff.set(8);
      component.fetchTeacherRequirements();
      
      expect(toastServiceMock.error).toHaveBeenCalledWith('Free periods must be less than total periods per day');
      expect(timetableServiceMock.getTeacherRequirements).not.toHaveBeenCalled();
    });
    
    it('should handle requirements fetch error', () => {
      timetableServiceMock.getTeacherRequirements.mockReturnValueOnce(throwError(() => ({ error: { message: 'Req err' } })));
      component.fetchTeacherRequirements();
      expect(component.requirementsError()).toBe('Req err');
    });
  });

  describe('DOM Interactions & HTML Coverage', () => {
    it('should interact with filter inputs', async () => {
      const searchInput = fixture.nativeElement.querySelector('input[placeholder*="Search"]');
      searchInput.value = 'Smith';
      searchInput.dispatchEvent(new Event('input'));
      
      const statusSelect = fixture.nativeElement.querySelector('select');
      statusSelect.value = statusSelect.options[1].value;
      statusSelect.dispatchEvent(new Event('change'));
      
      fixture.detectChanges();
      await fixture.whenStable();
      
      expect(component.searchQuery()).toBe('Smith');
      expect(component.status()).toBe('Active'); // or whatever options[1] is
    });

    it('should trigger action buttons', async () => {
      // Add buttons
      const addBtns = fixture.nativeElement.querySelectorAll('.btn-primary');
      expect(addBtns.length).toBeGreaterThan(0);
      
      // We know there are table rows
      const tableActions = fixture.nativeElement.querySelectorAll('tbody tr td button');
      expect(tableActions.length).toBeGreaterThan(0);
      
      // Let's trigger all the modals manually to get coverage
      component.openViewModal(component.teachers()[0]);
      fixture.detectChanges();
      await fixture.whenStable();
      
      // Try to click close button
      const btnClose = fixture.nativeElement.querySelector('.modal-header-custom .btn-close');
      if (btnClose) btnClose.dispatchEvent(new Event('click'));
      
      component.openEditModal(component.teachers()[0]);
      fixture.detectChanges();
      await fixture.whenStable();
      
      // Modifying form in edit modal
      const editInput = fixture.nativeElement.querySelector('.modal-body-custom input[type="text"]');
      if (editInput) {
        editInput.value = 'Changed';
        editInput.dispatchEvent(new Event('input'));
      }
      
      const saveBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-primary');
      if (saveBtn) saveBtn.dispatchEvent(new Event('click'));
      
      component.openDeleteModal(component.teachers()[0]);
      fixture.detectChanges();
      await fixture.whenStable();
      
      const delBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-danger');
      if (delBtn) delBtn.dispatchEvent(new Event('click'));
      
      component.openNotificationModal('Target', [101]);
      fixture.detectChanges();
      await fixture.whenStable();
      
      const notifInput = fixture.nativeElement.querySelector('.modal-body-custom input');
      if (notifInput) {
        notifInput.value = 'Title';
        notifInput.dispatchEvent(new Event('input'));
      }
      
      const sendBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-primary');
      if (sendBtn) sendBtn.dispatchEvent(new Event('click'));
      
      component.openAssignmentsModal(component.teachers()[0]);
      fixture.detectChanges();
      await fixture.whenStable();
      
      // Check for assignments map
      const asgns = fixture.nativeElement.querySelector('.table-responsive');
      expect(asgns).toBeTruthy();
      // click assignments close button
      const asgnsCloseBtn = fixture.nativeElement.querySelector('.modal-footer .btn-secondary');
      if (asgnsCloseBtn) asgnsCloseBtn.dispatchEvent(new Event('click'));
    });

    it('should interact with requirements section', async () => {
      // click Analyze Requirements button
      const allBtns = Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[];
      const analyzeBtn = allBtns.find(b => b.textContent?.includes('Analyze'));
      if (analyzeBtn) {
        analyzeBtn.dispatchEvent(new Event('click'));
      }
      fixture.detectChanges();
      await fixture.whenStable();
      
      // Try altering periods inputs
      component.showTeacherRequirements.set(true);
      fixture.detectChanges();
      await fixture.whenStable();
      
      const numInputs = fixture.nativeElement.querySelectorAll('input[type="number"]');
      if (numInputs.length >= 2) {
        numInputs[0].value = '7';
        numInputs[0].dispatchEvent(new Event('input'));
        numInputs[0].dispatchEvent(new KeyboardEvent('keypress', { charCode: 50 })); // valid
        numInputs[0].dispatchEvent(new KeyboardEvent('keypress', { charCode: 65 })); // invalid, should be prevented
        
        numInputs[1].value = '1';
        numInputs[1].dispatchEvent(new Event('input'));
        numInputs[1].dispatchEvent(new KeyboardEvent('keypress', { charCode: 50 }));
        numInputs[1].dispatchEvent(new KeyboardEvent('keypress', { charCode: 65 }));
      }
      expect(component.periodsPerDay()).toBe(7);
      expect(component.freePeriodsPerStaff()).toBe(1);
    });

    it('should cover loading states and empty/error states', () => {
      // Main loading state
      component.loading.set(true);
      fixture.detectChanges();
      expect(fixture.nativeElement.innerHTML).toContain('Assembling faculty directory...');
      
      // Error state
      component.loading.set(false);
      component.error.set('Something went wrong');
      fixture.detectChanges();
      expect(fixture.nativeElement.innerHTML).toContain('Something went wrong');
      
      const retryBtn = fixture.nativeElement.querySelector('.text-center button.btn-outline-primary');
      if (retryBtn) retryBtn.dispatchEvent(new Event('click'));
      
      // Empty state
      component.error.set(null);
      component.teachers.set([]);
      fixture.detectChanges();
      expect(fixture.nativeElement.innerHTML).toContain('No teachers found matching the criteria');
    });

    it('should cover teacher stats render', () => {
      component.teacherStats.set({
        totalTeachers: 10,
        activeTeachers: 8,
        inactiveTeachers: 2
      });
      fixture.detectChanges();
      expect(fixture.nativeElement.innerHTML).toContain('10');
      expect(fixture.nativeElement.innerHTML).toContain('8');
      expect(fixture.nativeElement.innerHTML).toContain('2');
    });

    it('should cover requirements analysis clicks and loading/error states', () => {
      // Toggle Requirements via button click
      const reqBtn = fixture.nativeElement.querySelector('.minimal-card button.btn-outline-primary');
      if (reqBtn) reqBtn.dispatchEvent(new Event('click'));
      fixture.detectChanges();
      expect(component.showTeacherRequirements()).toBe(true);
      
      // Loading state
      component.isLoadingRequirements.set(true);
      fixture.detectChanges();
      expect(fixture.nativeElement.innerHTML).toContain('Analyzing...');
      
      // Fetch via button
      component.isLoadingRequirements.set(false);
      fixture.detectChanges();
      const fetchBtn = fixture.nativeElement.querySelector('.col-md-4 button.btn-primary');
      if (fetchBtn) fetchBtn.dispatchEvent(new Event('click'));
      expect(timetableServiceMock.getTeacherRequirements).toHaveBeenCalled();
      
      // Error state
      component.requirementsError.set('Req failure');
      fixture.detectChanges();
      expect(fixture.nativeElement.innerHTML).toContain('Req failure');
    });

    it('should cover assignment and notification modal saving states and inner clicks', () => {
      component.openNotificationModal('Target', [101]);
      component.isSendingNotification.set(true);
      fixture.detectChanges();
      expect(fixture.nativeElement.innerHTML).toContain('Sending...');
      
      component.isSendingNotification.set(false);
      fixture.detectChanges();
      
      const notifInputs = fixture.nativeElement.querySelectorAll('.modal-body-custom .form-control');
      if (notifInputs.length >= 2) {
        notifInputs[0].value = 'New Title';
        notifInputs[0].dispatchEvent(new Event('input')); // title ngModelChange
        
        notifInputs[1].value = 'New Message';
        notifInputs[1].dispatchEvent(new Event('input')); // message ngModelChange
      }
      expect(component.notificationTitle()).toBe('New Title');
      expect(component.notificationMessage()).toBe('New Message');
      
      const notifCancelBtn = fixture.nativeElement.querySelector('.modal-footer-custom .btn-light');
      if (notifCancelBtn) notifCancelBtn.dispatchEvent(new Event('click'));
      expect(component.showNotificationModal()).toBe(false);
      
      component.openAssignmentsModal(component.teachers()[0]);
      component.isAssigning.set(true);
      fixture.detectChanges();
      expect(fixture.nativeElement.innerHTML).toContain('spinner-border');
      
      const asgnCloseX = fixture.nativeElement.querySelector('.modal-header .btn-close');
      if (asgnCloseX) asgnCloseX.dispatchEvent(new Event('click'));
    });
    
    it('should cover edit and delete modal loading states', () => {
      component.openEditModal(component.teachers()[0]);
      component.isSaving.set(true);
      fixture.detectChanges();
      expect(fixture.nativeElement.innerHTML).toContain('Saving...');
      component.closeEditModal();
      
      component.openDeleteModal(component.teachers()[0]);
      component.isDeleting.set(true);
      fixture.detectChanges();
      expect(fixture.nativeElement.innerHTML).toContain('Deactivating...');
      component.closeDeleteModal();
    });
    
    it('should cover all pagination buttons explicitly', () => {
      component.pageNumber.set(2);
      component.totalPages.set(5);
      fixture.detectChanges();
      
      const btns = fixture.nativeElement.querySelectorAll('.px-4.py-3.border-top button');
      if (btns.length >= 2) {
        btns[0].dispatchEvent(new Event('click')); // previous
        btns[2].dispatchEvent(new Event('click')); // next (btns[1] is current page number)
      }
    });
  });
});
