import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Documents } from './documents';
import { DocumentService } from '../../services/document.service';
import { StudentService } from '../../services/student.service';
import { TeacherService } from '../../services/teacher.service';
import { ParentService } from '../../services/parent.service';
import { ClassService } from '../../services/class.service';
import { ToastService } from '../../services/toast.service';
import { TimetableService } from '../../services/timetable.service';
import { FilterStateService } from '../../services/filter-state.service';
import { of, throwError } from 'rxjs';

describe('Documents', () => {
  let component: Documents;
  let fixture: ComponentFixture<Documents>;

  // Mocks
  let mockDocumentService: any;
  let mockStudentService: any;
  let mockTeacherService: any;
  let mockParentService: any;
  let mockClassService: any;
  let mockToastService: any;
  let mockTimetableService: any;
  let mockFilterStateService: any;

  beforeEach(async () => {
    mockDocumentService = {
      getTeacherDocuments: vi.fn().mockReturnValue(of([])),
      getStudentDocuments: vi.fn().mockReturnValue(of([])),
      getPendingDocuments: vi.fn().mockReturnValue(of([])),
      uploadStudentDocument: vi.fn().mockReturnValue(of({})),
      uploadTeacherDocument: vi.fn().mockReturnValue(of({})),
      verifyDocument: vi.fn().mockReturnValue(of({})),
      deleteDocument: vi.fn().mockReturnValue(of({}))
    };

    mockStudentService = {
      getStudentByUserId: vi.fn().mockReturnValue(of({ id: 1, name: 'S1' })),
      getAllStudents: vi.fn().mockReturnValue(of({ items: [{ id: 1, name: 'S1', classId: 1 }] }))
    };

    mockTeacherService = {
      getTeacherByUsername: vi.fn().mockReturnValue(of({ id: 1, name: 'T1' })),
      getAllTeachers: vi.fn().mockReturnValue(of({ items: [{ id: 1, name: 'T1' }] }))
    };

    mockParentService = {
      getParentByUserId: vi.fn().mockReturnValue(of({ id: 1, name: 'P1' })),
      getParentChildren: vi.fn().mockReturnValue(of([{ studentId: 1, name: 'C1' }])),
      selectedChildId: null
    };

    mockClassService = {
      getAllClasses: vi.fn().mockReturnValue(of([{ id: 1, classname: '10', section: 'A' }]))
    };

    mockToastService = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn()
    };

    mockTimetableService = {
      getTeacherTimetable: vi.fn().mockReturnValue(of([{ classId: 1 }]))
    };

    mockFilterStateService = {
      getState: vi.fn().mockReturnValue({ activeDirectoryTab: 'students', selectedClassId: 1 }),
      saveState: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [Documents],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: DocumentService, useValue: mockDocumentService },
        { provide: StudentService, useValue: mockStudentService },
        { provide: TeacherService, useValue: mockTeacherService },
        { provide: ParentService, useValue: mockParentService },
        { provide: ClassService, useValue: mockClassService },
        { provide: ToastService, useValue: mockToastService },
        { provide: TimetableService, useValue: mockTimetableService },
        { provide: FilterStateService, useValue: mockFilterStateService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Documents);
    component = fixture.componentInstance;
  });

  afterEach(() => {
    sessionStorage.clear();
    vi.clearAllMocks();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit Initialization', () => {
    it('should restore state from FilterStateService on construct', () => {
      expect(component.activeDirectoryTab()).toBe('students');
      expect(component.selectedClassId()).toBe(1);
    });

    it('should default role to Student if missing and handle student load', () => {
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      expect(component.userRole()).toBe('Student');
      expect(mockStudentService.getStudentByUserId).toHaveBeenCalledWith(1);
    });

    it('should initialize Admin role and load directory + pending docs', () => {
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      expect(mockClassService.getAllClasses).toHaveBeenCalled();
      expect(mockDocumentService.getPendingDocuments).toHaveBeenCalled();
    });

    it('should initialize Teacher role, fetch profile, timetable, and load directory', () => {
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 'teacher1');
      fixture.detectChanges();
      expect(mockTeacherService.getTeacherByUsername).toHaveBeenCalledWith('teacher1');
      expect(mockTimetableService.getTeacherTimetable).toHaveBeenCalledWith(1);
      expect(component.teacherClassIds()).toEqual([1]);
      expect(mockClassService.getAllClasses).toHaveBeenCalled();
      expect(mockDocumentService.getPendingDocuments).toHaveBeenCalled();
    });

    it('should handle error when loading Teacher timetable', () => {
      mockTimetableService.getTeacherTimetable.mockReturnValue(throwError(() => new Error('err')));
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 'teacher1');
      fixture.detectChanges();
      // Even if it fails, it calls loadDirectoryClasses and loadPendingDocuments
      expect(mockClassService.getAllClasses).toHaveBeenCalled();
      expect(mockDocumentService.getPendingDocuments).toHaveBeenCalled();
    });

    it('should handle error when loading Teacher profile', () => {
      mockTeacherService.getTeacherByUsername.mockReturnValue(throwError(() => new Error('err')));
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 'teacher1');
      fixture.detectChanges();
      // Error is caught, execution stops
      expect(mockTimetableService.getTeacherTimetable).not.toHaveBeenCalled();
    });

    it('should load Parent profile and children', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      expect(mockParentService.getParentByUserId).toHaveBeenCalledWith(1);
      expect(mockParentService.getParentChildren).toHaveBeenCalledWith(1);
      expect(component.selectedChildId()).toBe(1);
      expect(mockDocumentService.getStudentDocuments).toHaveBeenCalledWith(1);
    });

    it('should handle parent with no children', () => {
      mockParentService.getParentChildren.mockReturnValue(of([]));
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      expect(component.isLoading()).toBe(false);
      expect(component.selectedChildId()).toBeNull();
    });

    it('should handle error when fetching Parent children', () => {
      mockParentService.getParentChildren.mockReturnValue(throwError(() => new Error('err')));
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      expect(component.isLoading()).toBe(false);
    });

    it('should handle error when fetching Parent profile', () => {
      mockParentService.getParentByUserId.mockReturnValue(throwError(() => new Error('err')));
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to resolve parent profile.');
      expect(component.isLoading()).toBe(false);
    });

    it('should handle error when fetching Student profile', () => {
      mockStudentService.getStudentByUserId.mockReturnValue(throwError(() => new Error('err')));
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to resolve student profile.');
      expect(component.isLoading()).toBe(false);
    });
  });

  describe('Teacher Self Documents & Views', () => {
    it('should load teacher self documents successfully', () => {
      component.resolvedTeacherId.set(1);
      mockDocumentService.getTeacherDocuments.mockReturnValue(of([{ id: 1, status: 'Verified' }]));
      component.loadTeacherSelfDocuments();
      expect(component.activeDocumentsList().length).toBe(1);
      expect(component.verifiedDocsCount()).toBe(1);
      expect(component.isLoading()).toBe(false);
    });

    it('should handle error in loadTeacherSelfDocuments', () => {
      component.resolvedTeacherId.set(1);
      mockDocumentService.getTeacherDocuments.mockReturnValue(throwError(() => new Error('err')));
      component.loadTeacherSelfDocuments();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to load your documents.');
      expect(component.isLoading()).toBe(false);
    });

    it('should do nothing in loadTeacherSelfDocuments if no teacherId', () => {
      component.resolvedTeacherId.set(null);
      component.loadTeacherSelfDocuments();
      expect(component.isLoading()).toBe(false);
      expect(mockDocumentService.getTeacherDocuments).not.toHaveBeenCalled();
    });

    it('should switch teacher view mode to self', () => {
      component.resolvedTeacherId.set(1);
      component.switchTeacherViewMode('self');
      expect(component.teacherViewMode()).toBe('self');
      expect(component.selectedStudent()).toBeNull();
      expect(component.selectedTeacher()).toBeNull();
      expect(mockDocumentService.getTeacherDocuments).toHaveBeenCalled();
    });

    it('should switch teacher view mode to verify', () => {
      component.switchTeacherViewMode('verify');
      expect(component.teacherViewMode()).toBe('verify');
      expect(component.activeDocumentsList().length).toBe(0);
      expect(mockDocumentService.getPendingDocuments).toHaveBeenCalled();
    });
  });

  describe('Student / Parent Flows', () => {
    it('should update child on onChildChange', () => {
      component.onChildChange('2');
      expect(component.selectedChildId()).toBe(2);
      expect(mockParentService.selectedChildId).toBe(2);
      expect(mockDocumentService.getStudentDocuments).toHaveBeenCalledWith(2);
    });

    it('should handle error in loadDocumentsForStudent', () => {
      mockDocumentService.getStudentDocuments.mockReturnValue(throwError(() => new Error('err')));
      component.loadDocumentsForStudent(1);
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to load documents.');
      expect(component.isLoading()).toBe(false);
    });
  });

  describe('Pending Documents and Directory Loading', () => {
    it('should load pending documents successfully', () => {
      mockDocumentService.getPendingDocuments.mockReturnValue(of([{ id: 1, ownerType: 'Student' }]));
      component.loadPendingDocuments();
      expect(component.pendingDocuments().length).toBe(1);
      expect(component.systemPendingCount()).toBe(1);
      expect(component.isLoading()).toBe(false);
    });

    it('should handle error in loadPendingDocuments', () => {
      mockDocumentService.getPendingDocuments.mockReturnValue(throwError(() => new Error('err')));
      component.loadPendingDocuments();
      expect(component.isLoading()).toBe(false);
    });

    it('loadDirectoryClasses should filter classes for Teacher', () => {
      component.userRole.set('Teacher');
      component.teacherClassIds.set([1]); // Only class 1
      mockClassService.getAllClasses.mockReturnValue(of([
        { id: 1, classname: '10', section: 'A' },
        { id: 2, classname: '11', section: 'B' }
      ]));
      component.loadDirectoryClasses();
      expect(component.classesList().length).toBe(1);
      expect(component.classesList()[0].id).toBe(1);
    });

    it('loadDirectoryClasses should filter classes for Teacher matching currentTeacher classname', () => {
      component.userRole.set('Teacher');
      component.teacherClassIds.set([]); // None directly assigned in timetable
      component.currentTeacher.set({ className: '10', section: 'A' } as any);
      mockClassService.getAllClasses.mockReturnValue(of([
        { id: 1, classname: '10', section: 'A' },
        { id: 2, classname: '11', section: 'B' }
      ]));
      component.loadDirectoryClasses();
      expect(component.classesList().length).toBe(1);
      expect(component.classesList()[0].id).toBe(1);
    });
    
    it('loadDirectoryClasses should not filter for Admin', () => {
      component.userRole.set('Admin');
      mockClassService.getAllClasses.mockReturnValue(of([
        { id: 1, classname: '10', section: 'A' },
        { id: 2, classname: '11', section: 'B' }
      ]));
      component.loadDirectoryClasses();
      expect(component.classesList().length).toBe(2);
    });
  });

  describe('refreshDirectoryList Logic', () => {
    it('should refresh pending students with teacher filter', () => {
      component.onlyShowPending.set(true);
      component.activeDirectoryTab.set('students');
      component.userRole.set('Teacher');
      component.classesList.set([{ id: 1 } as any]); // Teacher has access to class 1
      
      component.pendingDocuments.set([
        { ownerType: 'Student', ownerId: 1, ownerName: 'S1', ownerIdentifier: 'R1', classId: 1 }, // Allowed
        { ownerType: 'Student', ownerId: 2, ownerName: 'S2', ownerIdentifier: 'R2', classId: 2 }  // Filtered out
      ] as any[]);
      
      mockStudentService.getAllStudents.mockReturnValue(of({ items: [
        { id: 1, name: 'S1', classId: 1 },
        { id: 2, name: 'S2', classId: 2 }
      ]}));

      component.refreshDirectoryList();
      expect(component.studentsDirectory().length).toBe(1);
      expect(component.studentsDirectory()[0].id).toBe(1);
    });

    it('should search within pending students', () => {
      component.onlyShowPending.set(true);
      component.activeDirectoryTab.set('students');
      component.userRole.set('Admin');
      component.searchQuery.set('s2');
      
      component.pendingDocuments.set([
        { ownerType: 'Student', ownerId: 1, ownerName: 'S1', ownerIdentifier: 'R1' },
        { ownerType: 'Student', ownerId: 2, ownerName: 'S2', ownerIdentifier: 'R2' }
      ] as any[]);
      
      mockStudentService.getAllStudents.mockReturnValue(of({ items: [
        { id: 1, name: 'S1' },
        { id: 2, name: 'S2' }
      ]}));

      component.refreshDirectoryList();
      expect(component.studentsDirectory().length).toBe(1);
      expect(component.studentsDirectory()[0].name).toBe('S2');
    });

    it('should handle error in getAllStudents during pending fetch', () => {
      component.onlyShowPending.set(true);
      component.activeDirectoryTab.set('students');
      mockStudentService.getAllStudents.mockReturnValue(throwError(() => new Error('err')));
      component.refreshDirectoryList();
      expect(component.studentsDirectory().length).toBe(0);
    });

    it('should refresh pending teachers and search', () => {
      component.onlyShowPending.set(true);
      component.activeDirectoryTab.set('teachers');
      component.searchQuery.set('t2');
      
      component.pendingDocuments.set([
        { ownerType: 'Teacher', ownerId: 1, ownerName: 'T1', ownerIdentifier: 'U1' },
        { ownerType: 'Teacher', ownerId: 2, ownerName: 'T2', ownerIdentifier: 'U2' },
        { ownerType: 'Teacher', ownerId: 2, ownerName: 'T2', ownerIdentifier: 'U2' } // duplicate id
      ] as any[]);
      
      component.refreshDirectoryList();
      expect(component.teachersDirectory().length).toBe(1);
      expect(component.teachersDirectory()[0].name).toBe('T2');
    });

    it('should refresh all students (not only pending) with teacher filter', () => {
      component.onlyShowPending.set(false);
      component.activeDirectoryTab.set('students');
      component.userRole.set('Teacher');
      component.selectedClassId.set(null);
      component.classesList.set([{ id: 1 } as any]); // Teacher class
      
      mockStudentService.getAllStudents.mockReturnValue(of({ items: [
        { id: 1, name: 'S1', classId: 1 },
        { id: 2, name: 'S2', classId: 2 }
      ]}));

      component.refreshDirectoryList();
      expect(component.studentsDirectory().length).toBe(1);
      expect(component.studentsDirectory()[0].id).toBe(1);
    });

    it('should refresh all students (not only pending) without teacher filter if selectedClassId exists', () => {
      component.onlyShowPending.set(false);
      component.activeDirectoryTab.set('students');
      component.userRole.set('Teacher');
      component.selectedClassId.set(2);
      
      mockStudentService.getAllStudents.mockReturnValue(of({ items: [
        { id: 2, name: 'S2', classId: 2 }
      ]}));

      component.refreshDirectoryList();
      expect(component.studentsDirectory().length).toBe(1);
    });

    it('should handle error in getAllStudents for all students', () => {
      component.onlyShowPending.set(false);
      component.activeDirectoryTab.set('students');
      mockStudentService.getAllStudents.mockReturnValue(throwError(() => new Error('err')));
      component.refreshDirectoryList();
      expect(component.studentsDirectory().length).toBe(0); // whatever it was
    });

    it('should refresh all teachers (not only pending)', () => {
      component.onlyShowPending.set(false);
      component.activeDirectoryTab.set('teachers');
      mockTeacherService.getAllTeachers.mockReturnValue(of({ items: [{ id: 1, name: 'T1' }] }));
      component.refreshDirectoryList();
      expect(component.teachersDirectory().length).toBe(1);
    });
    
    it('should handle error in getAllTeachers for all teachers', () => {
      component.onlyShowPending.set(false);
      component.activeDirectoryTab.set('teachers');
      mockTeacherService.getAllTeachers.mockReturnValue(throwError(() => new Error('err')));
      component.refreshDirectoryList();
      expect(component.teachersDirectory().length).toBe(0);
    });
  });

  describe('Directory Tab & Filters', () => {
    it('should clear selection on tab change', () => {
      component.selectedStudent.set({ id: 1 } as any);
      component.onTabChange('teachers');
      expect(component.activeDirectoryTab()).toBe('teachers');
      expect(component.selectedStudent()).toBeNull();
      expect(component.activeDocumentsList().length).toBe(0);
    });

    it('should update class filter and refresh list', () => {
      const spy = vi.spyOn(component, 'refreshDirectoryList');
      component.onClassChangeFilter(2);
      expect(component.selectedClassId()).toBe(2);
      expect(spy).toHaveBeenCalled();
    });
  });

  describe('Directory Selection', () => {
    it('should select student and load their documents', () => {
      mockDocumentService.getStudentDocuments.mockReturnValue(of([{ id: 1 }]));
      component.selectStudentFromDirectory({ id: 1 } as any);
      expect(component.selectedStudent()?.id).toBe(1);
      expect(component.selectedTeacher()).toBeNull();
      expect(component.activeDocumentsList().length).toBe(1);
    });

    it('should handle error when selecting student', () => {
      mockDocumentService.getStudentDocuments.mockReturnValue(throwError(() => new Error('err')));
      component.selectStudentFromDirectory({ id: 1 } as any);
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to load student documents.');
      expect(component.isLoading()).toBe(false);
    });

    it('should select teacher and load their documents', () => {
      mockDocumentService.getTeacherDocuments.mockReturnValue(of([{ id: 1 }]));
      component.selectTeacherFromDirectory({ id: 1 } as any);
      expect(component.selectedTeacher()?.id).toBe(1);
      expect(component.selectedStudent()).toBeNull();
      expect(component.activeDocumentsList().length).toBe(1);
    });

    it('should handle error when selecting teacher', () => {
      mockDocumentService.getTeacherDocuments.mockReturnValue(throwError(() => new Error('err')));
      component.selectTeacherFromDirectory({ id: 1 } as any);
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to load teacher documents.');
      expect(component.isLoading()).toBe(false);
    });
  });

  describe('Common Utilities', () => {
    it('getRequiredDocuments should return student docs if student is selected or role is student', () => {
      component.userRole.set('Student');
      const docs = component.getRequiredDocuments();
      expect(docs.map(d => d.key)).toContain('photo');
      expect(docs.map(d => d.key)).toContain('tc');
    });

    it('getRequiredDocuments should return teacher docs if teacher is selected', () => {
      component.userRole.set('Admin');
      component.selectedTeacher.set({ id: 1 } as any);
      const docs = component.getRequiredDocuments();
      expect(docs.map(d => d.key)).toContain('degree');
      expect(docs.map(d => d.key)).toContain('exp');
    });

    it('getRequiredDocuments should return empty array if no match', () => {
      component.userRole.set('Admin');
      component.selectedTeacher.set(null);
      component.selectedStudent.set(null);
      const docs = component.getRequiredDocuments();
      expect(docs.length).toBe(0);
    });

    it('getUploadedDocument should find matching document by name', () => {
      component.activeDocumentsList.set([
        { id: 1, documentName: 'Photo', status: 'Verified' } as any,
        { id: 2, documentName: 'Photo', status: 'Pending' } as any // Latest ID wins
      ]);
      const match = component.getUploadedDocument('Profile Photo');
      expect(match?.id).toBe(2);
    });

    it('getUploadedDocument should return undefined if not found', () => {
      component.activeDocumentsList.set([{ id: 1, documentName: 'Other' } as any]);
      const match = component.getUploadedDocument('Photo');
      expect(match).toBeUndefined();
    });

    it('getAdditionalDocuments should return non-required documents', () => {
      component.activeDocumentsList.set([
        { id: 1, documentName: 'Photo' } as any, // standard
        { id: 2, documentName: 'Custom Report' } as any // additional
      ]);
      const addDocs = component.getAdditionalDocuments();
      expect(addDocs.length).toBe(1);
      expect(addDocs[0].documentName).toBe('Custom Report');
    });

    it('hasPhoto & getPhotoUrl should work for student and teacher', () => {
      // Student
      component.selectedStudent.set({ profilePhotoUrl: '/photo.png' } as any);
      expect(component.hasPhoto()).toBe(true);
      expect(component.getPhotoUrl()).toContain('/photo.png');
      
      // Student null photo
      component.selectedStudent.set({ profilePhotoUrl: 'null' } as any);
      expect(component.hasPhoto()).toBe(false);

      // Teacher
      component.selectedStudent.set(null);
      component.selectedTeacher.set({ profilePhotoUrl: '/teacher.png' } as any);
      expect(component.hasPhoto()).toBe(true);
      expect(component.getPhotoUrl()).toContain('/teacher.png');
      
      // Empty
      component.selectedTeacher.set(null);
      expect(component.hasPhoto()).toBe(false);
      expect(component.getPhotoUrl()).toBe('');
    });

    it('hasDirectoryPhoto & getDirectoryPhotoUrl should work', () => {
      expect(component.hasDirectoryPhoto({ profilePhotoUrl: '/img.jpg' })).toBe(true);
      expect(component.hasDirectoryPhoto({ profilePhotoUrl: 'null' })).toBe(false);
      expect(component.getDirectoryPhotoUrl({ profilePhotoUrl: '/img.jpg' })).toContain('/img.jpg');
    });
    
    it('getFileUrl should prepend base url if it starts with slash', () => {
      expect(component.getFileUrl('/docs/doc1.pdf')).toContain('/docs/doc1.pdf');
      expect(component.getFileUrl('http://docs.com/doc')).toBe('http://docs.com/doc');
    });
  });

  describe('Upload Flow', () => {
    it('should open and close upload modal', () => {
      component.openUploadModal('TestDoc');
      expect(component.uploadForm().documentName).toBe('TestDoc');
      expect(component.showUploadModal()).toBe(true);
      
      component.closeUploadModal();
      expect(component.showUploadModal()).toBe(false);
    });

    it('should set file on selection', () => {
      const mockFile = new File([''], 'test.txt');
      component.onFileSelected({ target: { files: [mockFile] } });
      expect(component.uploadForm().selectedFile).toBe(mockFile);
    });

    it('should require file and document name', () => {
      component.uploadForm.set({ documentName: '', selectedFile: null });
      component.submitUpload();
      expect(mockToastService.warning).toHaveBeenCalledWith('Please select a file to upload.');
      
      component.uploadForm.set({ documentName: '', selectedFile: new File([''], 't') });
      component.submitUpload();
      expect(mockToastService.warning).toHaveBeenCalledWith('Please select the document type.');
    });

    it('should prevent overwrite of existing verified/pending document', () => {
      component.uploadForm.set({ documentName: 'Photo', selectedFile: new File([''], 't') });
      component.activeDocumentsList.set([{ documentName: 'Photo', status: 'Verified' } as any]);
      component.submitUpload();
      expect(mockToastService.warning).toHaveBeenCalledWith("A document of type 'Photo' has already been uploaded.");
    });

    it('should upload student document for Student role', () => {
      component.userRole.set('Student');
      component.resolvedStudentId.set(1);
      component.uploadForm.set({ documentName: 'Profile Photo', selectedFile: new File([''], 't') });
      
      component.submitUpload();
      
      expect(mockDocumentService.uploadStudentDocument).toHaveBeenCalledWith(1, expect.any(File), 'Photo');
      expect(mockToastService.success).toHaveBeenCalledWith('Document uploaded successfully!');
    });

    it('should handle error in student upload', () => {
      component.userRole.set('Student');
      component.resolvedStudentId.set(1);
      component.uploadForm.set({ documentName: 'NewDoc', selectedFile: new File([''], 't') });
      mockDocumentService.uploadStudentDocument.mockReturnValue(throwError(() => new Error('err')));
      
      component.submitUpload();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to upload document.');
    });

    it('should upload student document for Parent role', () => {
      component.userRole.set('Parent');
      component.selectedChildId.set(2);
      component.uploadForm.set({ documentName: 'Birth Certificate', selectedFile: new File([''], 't') });
      
      component.submitUpload();
      
      expect(mockDocumentService.uploadStudentDocument).toHaveBeenCalledWith(2, expect.any(File), 'Birth Certificate');
    });

    it('should handle error in parent upload', () => {
      component.userRole.set('Parent');
      component.selectedChildId.set(2);
      component.uploadForm.set({ documentName: 'NewDoc', selectedFile: new File([''], 't') });
      mockDocumentService.uploadStudentDocument.mockReturnValue(throwError(() => new Error('err')));
      
      component.submitUpload();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to upload document.');
    });

    it('should upload teacher document for Teacher role', () => {
      component.userRole.set('Teacher');
      component.resolvedTeacherId.set(3);
      component.uploadForm.set({ documentName: 'Identity Proof', selectedFile: new File([''], 't') });
      
      component.submitUpload();
      
      expect(mockDocumentService.uploadTeacherDocument).toHaveBeenCalledWith(3, expect.any(File), 'Identity Proof');
      expect(mockDocumentService.getTeacherDocuments).toHaveBeenCalledWith(3);
    });

    it('should handle error in teacher upload', () => {
      component.userRole.set('Teacher');
      component.resolvedTeacherId.set(3);
      component.uploadForm.set({ documentName: 'NewDoc', selectedFile: new File([''], 't') });
      mockDocumentService.uploadTeacherDocument.mockReturnValue(throwError(() => new Error('err')));
      
      component.submitUpload();
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to upload document.');
    });
  });

  describe('Verification and Deletion', () => {
    it('should update verification status and reload student', () => {
      component.activeDirectoryTab.set('students');
      component.selectedStudent.set({ id: 1 } as any);
      
      component.updateVerificationStatus({ id: 10 } as any, 'Verified');
      
      expect(mockDocumentService.verifyDocument).toHaveBeenCalledWith({
        documentType: 'Student',
        documentId: 10,
        status: 'Verified'
      });
      expect(mockToastService.success).toHaveBeenCalledWith('Document marked as Verified!');
      expect(mockDocumentService.getPendingDocuments).toHaveBeenCalled();
      expect(mockDocumentService.getStudentDocuments).toHaveBeenCalledWith(1);
    });

    it('should update verification status and reload teacher', () => {
      component.activeDirectoryTab.set('teachers');
      component.selectedTeacher.set({ id: 2 } as any);
      
      component.updateVerificationStatus({ id: 20 } as any, 'Rejected');
      
      expect(mockDocumentService.verifyDocument).toHaveBeenCalledWith({
        documentType: 'Teacher',
        documentId: 20,
        status: 'Rejected'
      });
      expect(mockDocumentService.getTeacherDocuments).toHaveBeenCalledWith(2);
    });

    it('should handle verification error', () => {
      mockDocumentService.verifyDocument.mockReturnValue(throwError(() => new Error('err')));
      component.updateVerificationStatus({ id: 10 } as any, 'Verified');
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to verify document.');
    });

    it('should delete document and reload', () => {
      // Mock confirm
      const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
      component.selectedStudent.set({ id: 1 } as any);
      
      component.deleteDocument({ blobUrl: 'url123' } as any);
      
      expect(mockDocumentService.deleteDocument).toHaveBeenCalledWith('url123');
      expect(mockToastService.success).toHaveBeenCalledWith('Document deleted successfully.');
      expect(mockDocumentService.getStudentDocuments).toHaveBeenCalledWith(1);
      
      confirmSpy.mockRestore();
    });

    it('should abort delete if unconfirmed', () => {
      const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(false);
      component.deleteDocument({ blobUrl: 'url123' } as any);
      expect(mockDocumentService.deleteDocument).not.toHaveBeenCalled();
      confirmSpy.mockRestore();
    });

    it('should handle delete error', () => {
      const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
      mockDocumentService.deleteDocument.mockReturnValue(throwError(() => new Error('err')));
      component.deleteDocument({ blobUrl: 'url123' } as any);
      expect(mockToastService.error).toHaveBeenCalledWith('Failed to delete document.');
      confirmSpy.mockRestore();
    });
  });
  
  describe('DOM Rendering', () => {
    it('should render Student View', () => {
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      
      // Simulate loaded
      component.isLoading.set(false);
      component.activeDocumentsList.set([{ id: 1, documentName: 'Photo', status: 'Pending', blobUrl: '/a' } as any]);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Documents & Verification');
      expect(compiled.textContent).toContain('Pending');
    });

    it('should render Parent View with children select', () => {
      sessionStorage.setItem('role', 'Parent');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      
      component.isLoading.set(false);
      component.parentChildren.set([{ studentId: 1, name: 'Child One', regNo: 'C1' }]);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Child One');
    });

    it('should render Admin View with directory', () => {
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      
      component.isLoading.set(false);
      component.studentsDirectory.set([{ id: 1, name: 'Student One', regNo: 'S1', className: '10' } as any]);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Documents & Verification');
      expect(compiled.textContent).toContain('Student One');
      
      // Simulate selecting a student
      component.selectedStudent.set({ id: 1, name: 'Student One', regNo: 'S1' } as any);
      fixture.detectChanges();
      expect(compiled.textContent).toContain('No upload available');
    });
    
    it('should render Teacher View in self mode', () => {
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 't1');
      fixture.detectChanges();
      
      component.isLoading.set(false);
      component.teacherViewMode.set('self');
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('My Documents');
    });
    
    it('should render Upload Modal', () => {
      component.showUploadModal.set(true);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Upload Required Document');
    });

    it('should render Teacher View in verify mode with directory', () => {
      sessionStorage.setItem('role', 'Teacher');
      sessionStorage.setItem('username', 't1');
      fixture.detectChanges();
      
      component.isLoading.set(false);
      component.teacherViewMode.set('verify');
      component.activeDirectoryTab.set('students');
      component.studentsDirectory.set([{ id: 1, name: 'Student One', regNo: 'S1', className: '10' } as any]);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Student One');
    });

    it('should render Teachers Directory tab in Admin View', () => {
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      
      component.isLoading.set(false);
      component.activeDirectoryTab.set('teachers');
      component.teachersDirectory.set([{ id: 1, name: 'Teacher One', username: 'T1' } as any]);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Teacher One');
    });

    it('should render Verified and Rejected document statuses', () => {
      sessionStorage.setItem('role', 'Student');
      sessionStorage.setItem('userId', '1');
      fixture.detectChanges();
      
      component.isLoading.set(false);
      component.activeDocumentsList.set([
        { id: 1, documentName: 'Profile Photo', status: 'Verified', blobUrl: '/a', verifiedAt: '2023-01-01' } as any,
        { id: 2, documentName: 'Identity Proof / Aadhaar Card', status: 'Rejected', blobUrl: '/b', remarks: 'Invalid' } as any,
        { id: 3, documentName: 'Custom Doc', status: 'Pending', blobUrl: '/c' } as any
      ]);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Verified');
      expect(compiled.textContent).toContain('Rejected');
    });

    it('should render action buttons for Pending documents in Admin view', () => {
      sessionStorage.setItem('role', 'Admin');
      fixture.detectChanges();
      
      component.isLoading.set(false);
      component.selectedStudent.set({ id: 1, name: 'Student One', regNo: 'S1' } as any);
      component.activeDocumentsList.set([
        { id: 1, documentName: 'Profile Photo', status: 'Pending', blobUrl: '/a' } as any,
        { id: 2, documentName: 'Birth Certificate', status: 'Verified', blobUrl: '/b' } as any,
        { id: 3, documentName: 'Transfer Certificate', status: 'Rejected', blobUrl: '/c' } as any
      ]);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Open File');
      expect(compiled.textContent).toContain('Verify');
      expect(compiled.textContent).toContain('Reject');
      expect(compiled.textContent).toContain('Delete');
      expect(compiled.textContent).toContain('Verified');
      expect(compiled.textContent).toContain('Rejected');
    });

    it('should render Upload Modal in uploading state', () => {
      component.showUploadModal.set(true);
      component.isUploading.set(true);
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Uploading...');
    });
  });
});
