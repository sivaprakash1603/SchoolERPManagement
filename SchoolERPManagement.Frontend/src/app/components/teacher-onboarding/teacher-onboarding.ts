import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TeacherService } from '../../services/teacher.service';
import { ClassService, ClassResponseDTO } from '../../services/class.service';
import { DocumentService } from '../../services/document.service';
import { ToastService } from '../../services/toast.service';

interface SubjectAssignment {
  classId: number;
  subjectId: number;
  className: string;
  subjectName: string;
}

@Component({
  selector: 'app-teacher-onboarding',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './teacher-onboarding.html',
  styleUrls: ['./teacher-onboarding.css']
})
export class TeacherOnboarding implements OnInit {
  private router = inject(Router);
  private teacherService = inject(TeacherService);
  private classService = inject(ClassService);
  private documentService = inject(DocumentService);
  private toastService = inject(ToastService);

  // Wizard State
  currentStep = signal<number>(1);
  isSubmitting = signal(false);
  isCancelled = false;
  isSubmitted = false;

  // Loaded Options
  classes = signal<ClassResponseDTO[]>([]);
  subjects = signal<any[]>([]);

  // Form State
  teacherForm = signal({
    name: '',
    email: '',
    phonenumber: '',
    qualifications: '',
    subjectSpecialtyId: null as number | null
  });

  // Assignments State
  selectedAssignments = signal<SubjectAssignment[]>([]);
  currentAssignment = signal({
    classId: null as number | null,
    subjectId: null as number | null
  });

  // Document State
  selectedDocuments = signal<{ file: File, type: string }[]>([]);

  ngOnInit() {
    this.fetchClasses();
    this.fetchSubjects();
  }

  fetchClasses() {
    this.classService.getAllClasses().subscribe({
      next: (data) => this.classes.set(data),
      error: (err) => console.error('Failed to fetch classes', err)
    });
  }

  fetchSubjects() {
    this.teacherService.getAllSubjects().subscribe({
      next: (data) => this.subjects.set(data),
      error: (err) => console.error('Failed to fetch subjects', err)
    });
  }

  // Wizard Navigation
  nextStep() {
    if (this.currentStep() < 4) {
      this.currentStep.update(s => s + 1);
    }
  }

  prevStep() {
    if (this.currentStep() > 1) {
      this.currentStep.update(s => s - 1);
    }
  }

  // --- Assignment Logic ---
  addAssignment() {
    const classId = this.currentAssignment().classId;
    const subjectId = this.currentAssignment().subjectId;

    if (!classId || !subjectId) {
      this.toastService.warning('Please select both a class and a subject.');
      return;
    }

    // Check duplicate
    const isDuplicate = this.selectedAssignments().some(
      a => a.classId === classId && a.subjectId === subjectId
    );

    if (isDuplicate) {
      this.toastService.warning('This assignment has already been added.');
      return;
    }

    const cls = this.classes().find(c => c.id === classId);
    const sub = this.subjects().find(s => s.id === subjectId);

    if (cls && sub) {
      if (!cls.subjects || !cls.subjects.some((s: any) => s.id === subjectId)) {
        this.toastService.warning(`Subject '${sub.subjectName || sub.subjectname}' is not assigned to Class '${cls.classname}'. Please map it in the Classes page first.`);
        return;
      }

      const assignment: SubjectAssignment = {
        classId,
        subjectId,
        className: `${cls.classname} - ${cls.section}`,
        subjectName: sub.subjectName || sub.subjectname
      };

      this.selectedAssignments.update(list => [...list, assignment]);
      
      // Reset assignment select boxes
      this.currentAssignment.set({ classId: null, subjectId: null });
    }
  }

  removeAssignment(index: number) {
    this.selectedAssignments.update(list => list.filter((_, i) => i !== index));
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

  // --- Complete Onboarding ---
  async completeOnboarding() {
    this.isSubmitting.set(true);

    try {
      // Step 1: Create Teacher
      const form = this.teacherForm();
      const payload = {
        name: form.name,
        email: form.email,
        phonenumber: form.phonenumber,
        qualifications: form.qualifications,
        subjectSpecialtyId: form.subjectSpecialtyId ?? undefined
      };
      const newTeacher = await new Promise<any>((resolve, reject) => {
        this.teacherService.addTeacher(payload).subscribe({
          next: (res) => resolve(res),
          error: (err) => reject(err)
        });
      });

      // Step 2: Assign Subjects sequentially
      for (const assignment of this.selectedAssignments()) {
        await new Promise<void>((resolve) => {
          const dto = {
            teacherId: newTeacher.id,
            subjectId: assignment.subjectId,
            classId: assignment.classId
          };
          this.teacherService.assignSubject(dto).subscribe({
            next: () => resolve(),
            error: (err) => {
              console.error('Failed to assign subject', err);
              this.toastService.error(err.error?.Message || err.error?.message || 'Failed to assign subject');
              resolve(); // Continue onboarding other elements
            }
          });
        });
      }

      // Step 3: Upload Documents sequentially
      for (const doc of this.selectedDocuments()) {
        await new Promise<void>((resolve) => {
          this.documentService.uploadTeacherDocument(newTeacher.id, doc.file, doc.type).subscribe({
            next: () => resolve(),
            error: (err) => {
              console.error(`Failed to upload ${doc.type}`, err);
              resolve(); // Continue onboarding
            }
          });
        });
      }

      this.isSubmitted = true;
      this.toastService.success('Teacher and document records created successfully!');
      this.router.navigate(['/teachers']);
    } catch (error: any) {
      this.toastService.error(error?.error?.message || 'Failed to complete teacher onboarding.');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  cancel() {
    this.isCancelled = true;
    this.router.navigate(['/teachers']);
  }

  canDeactivate(): boolean {
    if (this.isCancelled || this.isSubmitted) return true;
    this.toastService.warning('Please cancel or submit the form before navigating to another page.');
    return false;
  }
}
