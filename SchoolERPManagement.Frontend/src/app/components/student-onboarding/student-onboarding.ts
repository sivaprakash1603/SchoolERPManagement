import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { StudentService, ParentSelection } from '../../services/student.service';
import { ClassService, ClassResponseDTO } from '../../services/class.service';
import { ParentService, ParentResponseDTO } from '../../services/parent.service';
import { DocumentService } from '../../services/document.service';
import { AcademicYearService, AcademicYearResponseDTO } from '../../services/academic-year.service';
import { ToastService } from '../../services/toast.service';

interface SelectedParent {
  parentId: number;
  firstName: string;
  lastName: string;
  relation: string;
}

interface NewParentForm {
  firstName: string;
  lastName: string;
  email: string;
  phonenumber: string;
  relation: string;
}

@Component({
  selector: 'app-student-onboarding',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './student-onboarding.html'
})
export class StudentOnboarding implements OnInit {
  private router = inject(Router);
  private studentService = inject(StudentService);
  private classService = inject(ClassService);
  private parentService = inject(ParentService);
  private documentService = inject(DocumentService);
  private academicYearService = inject(AcademicYearService);
  private toastService = inject(ToastService);

  // Wizard State
  currentStep = signal<number>(1);
  isSubmitting = signal(false);
  isCancelled = false;
  isSubmitted = false;

  todayDate = new Date().toISOString().split('T')[0];

  // Data Loading
  classes = signal<ClassResponseDTO[]>([]);
  parents = signal<ParentResponseDTO[]>([]);
  academicYears = signal<AcademicYearResponseDTO[]>([]);
  parentSearchQuery = signal('');

  filteredParents() {
    const query = this.parentSearchQuery().toLowerCase();
    return this.parents().filter(p => 
      (p.firstName + ' ' + p.lastName).toLowerCase().includes(query) || 
      p.phonenumber.includes(query) || 
      p.email.toLowerCase().includes(query)
    );
  }
  
  // Form State
  studentForm = signal({
    firstName: '',
    lastName: '',
    email: '',
    classId: null as number | null,
    academicYearId: null as number | null,
    gender: 'Male',
    bloodgroup: '',
    dateofbirth: '',
    admissiondate: new Date().toISOString().split('T')[0]
  });

  // Selected existing parents with their relations
  selectedParents = signal<SelectedParent[]>([]);

  // New parents to create
  newParents = signal<NewParentForm[]>([]);

  // Document State
  selectedDocuments = signal<{ file: File, type: string }[]>([]);

  relationOptions = ['Father', 'Mother', 'Guardian', 'Other'];

  ngOnInit() {
    this.fetchAcademicYears();
    this.fetchParents();
  }

  fetchAcademicYears() {
    this.academicYearService.getAllAcademicYears().subscribe({
      next: (years) => {
        this.academicYears.set(years);
        const current = years.find(y => y.isCurrent);
        if (current) {
          this.studentForm.update(form => ({ ...form, academicYearId: current.id }));
        } else if (years.length > 0) {
          this.studentForm.update(form => ({ ...form, academicYearId: years[0].id }));
        }
        this.fetchClasses();
      },
      error: (err) => console.error('Failed to load academic years', err)
    });
  }

  fetchClasses() {
    const yearId = this.studentForm().academicYearId;
    this.classService.getAllClasses(yearId || undefined).subscribe({
      next: (data) => this.classes.set(data),
      error: (err) => console.error('Failed to fetch classes', err)
    });
  }

  onYearChange() {
    this.studentForm.update(form => ({ ...form, classId: null }));
    this.fetchClasses();
  }

  fetchParents() {
    this.parentService.getAllParents({ pageSize: 1000 }).subscribe({
      next: (data) => this.parents.set(data.items),
      error: (err) => console.error('Failed to fetch parents', err)
    });
  }

  // Navigation
  nextStep() {
    if (this.currentStep() === 1) {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (this.studentForm().email && !emailRegex.test(this.studentForm().email)) {
        this.toastService.warning('Please enter a valid email address for the student.');
        return;
      }

      if (this.studentForm().dateofbirth) {
        const selectedDate = new Date(this.studentForm().dateofbirth);
        const today = new Date();
        selectedDate.setHours(0,0,0,0);
        today.setHours(0,0,0,0);
        if (selectedDate > today) {
          this.toastService.warning('Date of birth cannot be in the future.');
          return;
        }
      }

      if (this.studentForm().admissiondate) {
        const selectedDate = new Date(this.studentForm().admissiondate);
        const today = new Date();
        selectedDate.setHours(0,0,0,0);
        today.setHours(0,0,0,0);
        if (selectedDate > today) {
          this.toastService.warning('Admission date cannot be in the future.');
          return;
        }
      }
    }

    if (this.currentStep() === 2) {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      for (const np of this.newParents()) {
        if (!np.firstName || !np.lastName || !np.email || !np.phonenumber) {
          this.toastService.warning('Please fill in all details for new parents before continuing.');
          return;
        }
        if (!emailRegex.test(np.email)) {
          this.toastService.warning(`Please enter a valid email address for parent: ${np.firstName}`);
          return;
        }
      }
    }

    if (this.currentStep() < 4) {
      this.currentStep.update(s => s + 1);
    }
  }

  prevStep() {
    if (this.currentStep() > 1) {
      this.currentStep.update(s => s - 1);
    }
  }

  // --- Parent Selection Logic ---

  isParentSelected(parentId: number): boolean {
    return this.selectedParents().some(p => p.parentId === parentId);
  }

  toggleParentSelection(parentId: number, parentFirstName: string, parentLastName: string) {
    if (this.isParentSelected(parentId)) {
      this.selectedParents.update(list => list.filter(p => p.parentId !== parentId));
    } else {
      this.selectedParents.update(list => [...list, { parentId, firstName: parentFirstName, lastName: parentLastName, relation: 'Father' }]);
    }
  }

  updateExistingParentRelation(parentId: number, relation: string) {
    this.selectedParents.update(list =>
      list.map(p => p.parentId === parentId ? { ...p, relation } : p)
    );
  }

  getSelectedParentRelation(parentId: number): string {
    return this.selectedParents().find(p => p.parentId === parentId)?.relation || 'Father';
  }

  // --- New Parent Logic ---

  addNewParentForm() {
    this.newParents.update(list => [...list, { firstName: '', lastName: '', email: '', phonenumber: '', relation: 'Father' }]);
  }

  removeNewParentForm(index: number) {
    this.newParents.update(list => list.filter((_, i) => i !== index));
  }

  updateNewParent(index: number, field: keyof NewParentForm, value: string) {
    const list = this.newParents();
    list[index] = { ...list[index], [field]: value };
    // No need to call .set() — we mutate in place to avoid re-render
  }

  trackByIndex(index: number, item?: any): number {
    return index;
  }

  // --- Document Logic ---

  onFileSelected(event: Event, type: string) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      const current = this.selectedDocuments();
      const existingIndex = current.findIndex(d => d.type === type);
      
      if (existingIndex >= 0) {
        current[existingIndex].file = file;
      } else {
        current.push({ file, type });
      }
      this.selectedDocuments.set([...current]);
    }
  }

  removeDocument(type: string) {
    this.selectedDocuments.update(docs => docs.filter(d => d.type !== type));
  }

  getFileName(type: string): string | null {
    const doc = this.selectedDocuments().find(d => d.type === type);
    return doc ? doc.file.name : null;
  }

  // --- Submission ---

  async completeOnboarding() {
    if (this.studentForm().admissiondate) {
      const selectedDate = new Date(this.studentForm().admissiondate);
      const today = new Date();
      selectedDate.setHours(0,0,0,0);
      today.setHours(0,0,0,0);
      if (selectedDate > today) {
        this.toastService.warning('Admission date cannot be in the future.');
        return;
      }
    }
    if (this.studentForm().dateofbirth) {
      const selectedDate = new Date(this.studentForm().dateofbirth);
      const today = new Date();
      selectedDate.setHours(0,0,0,0);
      today.setHours(0,0,0,0);
      if (selectedDate > today) {
        this.toastService.warning('Date of birth cannot be in the future.');
        return;
      }
    }

    // Ensure student email is not the same as any newly created parent email
    const studentEmail = this.studentForm().email?.trim().toLowerCase();
    if (studentEmail) {
      if (this.newParents().some(p => p.email?.trim().toLowerCase() === studentEmail)) {
        this.toastService.warning("The student's email cannot be exactly the same as a new parent's email.");
        return;
      }
      const selectedParentIds = this.selectedParents().map(p => p.parentId);
      const existingParentEmails = this.parents().filter(p => selectedParentIds.includes(p.id)).map(p => p.email.toLowerCase());
      if (existingParentEmails.includes(studentEmail)) {
        this.toastService.warning("The student's email cannot be exactly the same as a selected parent's email.");
        return;
      }
    }

    this.isSubmitting.set(true);

    try {
      // Step 1: Create any new parents first
      const allParents: ParentSelection[] = [
        ...this.selectedParents().map(p => ({ parentId: p.parentId, relation: p.relation }))
      ];

      for (const np of this.newParents()) {
        if (np.firstName && np.lastName && np.email) {
          const created = await new Promise<any>((resolve, reject) => {
            this.parentService.addParent({ firstName: np.firstName, lastName: np.lastName, email: np.email, phonenumber: np.phonenumber }).subscribe({
              next: (res) => resolve(res),
              error: (err) => reject(err)
            });
          });
          allParents.push({ parentId: created.id, relation: np.relation });
        }
      }

      // Step 2: Create Student
      const dto = {
        firstName: this.studentForm().firstName,
        lastName: this.studentForm().lastName,
        email: this.studentForm().email,
        classId: this.studentForm().classId!,
        academicYearId: this.studentForm().academicYearId!,
        parents: allParents,
        gender: this.studentForm().gender,
        bloodgroup: this.studentForm().bloodgroup,
        dateofbirth: this.studentForm().dateofbirth || undefined,
        admissiondate: this.studentForm().admissiondate || undefined
      };

      const newStudent = await new Promise<any>((resolve, reject) => {
        this.studentService.addStudent(dto).subscribe({
          next: (res) => resolve(res),
          error: (err) => reject(err)
        });
      });

      // Step 3: Upload Documents sequentially
      const docs = this.selectedDocuments();
      for (const doc of docs) {
        await new Promise<void>((resolve) => {
          this.documentService.uploadStudentDocument(newStudent.id, doc.file, doc.type).subscribe({
            next: () => resolve(),
            error: (err) => {
              console.error(`Failed to upload ${doc.type}`, err);
              resolve(); // Continue anyway
            }
          });
        });
      }

      this.isSubmitted = true;
      this.toastService.success('Student, Parent, and Document records created successfully!');
      this.router.navigate(['/students']);

    } catch (err: any) {
      console.error('Student onboarding failed', err);
      this.toastService.error(err.error?.message || 'Failed to complete student onboarding.');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  cancel() {
    this.isCancelled = true;
    this.router.navigate(['/students']);
  }

  canDeactivate(): boolean {
    if (this.isCancelled || this.isSubmitted) return true;
    this.toastService.warning('Please cancel or submit the form before navigating to another page.');
    return false;
  }
}
