import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AcademicYearService } from '../../services/academic-year.service';
import { ClassService } from '../../services/class.service';
import { SubjectService } from '../../services/subject.service';
import { ToastService } from '../../services/toast.service';
import { SystemSetupService } from '../../services/system-setup.service';

@Component({
  selector: 'app-setup-wizard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './setup-wizard.html',
  styleUrls: ['./setup-wizard.css']
})
export class SetupWizardComponent implements OnInit {
  private router = inject(Router);
  private academicYearService = inject(AcademicYearService);
  private classService = inject(ClassService);
  private subjectService = inject(SubjectService);
  toastService = inject(ToastService);
  private systemSetupService = inject(SystemSetupService);

  currentStep = signal<number>(1);
  isSubmitting = signal(false);

  // Step 2 Data
  academicYearForm = signal({
    yearname: '2025-2026',
    startdate: '',
    enddate: ''
  });

  // Step 3 Data
  classesToAdd = signal<{classname: string, section: string}[]>([
    { classname: 'Grade 1', section: 'A' },
    { classname: 'Grade 2', section: 'A' }
  ]);
  
  subjectsToAdd = signal<{subjectName: string}[]>([
    { subjectName: 'Mathematics' },
    { subjectName: 'Science' }
  ]);

  ngOnInit() {
    // Check if they really need to be here
    this.systemSetupService.getSetupStatus().subscribe(res => {
      if (res.isComplete) {
        this.router.navigate(['/dashboard']);
      }
    });
  }

  nextStep() {
    if (this.currentStep() === 2) {
      this.saveAcademicYear();
    } else {
      this.currentStep.update(s => s + 1);
    }
  }

  prevStep() {
    if (this.currentStep() > 1) {
      this.currentStep.update(s => s - 1);
    }
  }

  saveAcademicYear() {
    const form = this.academicYearForm();
    if (!form.yearname || !form.startdate || !form.enddate) {
      this.toastService.warning('Please fill in all mandatory fields.');
      return;
    }
    if (new Date(form.startdate) >= new Date(form.enddate)) {
      this.toastService.warning('End date must be after the start date.');
      return;
    }
    
    this.isSubmitting.set(true);
    
    // The AcademicYearService expects { yearName, startDate, endDate }
    this.academicYearService.createAcademicYear({
      yearName: form.yearname,
      startDate: form.startdate,
      endDate: form.enddate
    }).subscribe({
      next: () => {
        this.systemSetupService.markSetupComplete();
        this.isSubmitting.set(false);
        this.currentStep.update(s => s + 1);
      },
      error: (err) => {
        this.toastService.error(err.error?.message || 'Failed to create academic session.');
        this.isSubmitting.set(false);
      }
    });
  }
  
  addEmptyClass() {
    this.classesToAdd.update(arr => [...arr, {classname: '', section: ''}]);
  }
  
  removeEmptyClass(index: number) {
    this.classesToAdd.update(arr => arr.filter((_, i) => i !== index));
  }
  
  addEmptySubject() {
    this.subjectsToAdd.update(arr => [...arr, {subjectName: ''}]);
  }
  
  removeEmptySubject(index: number) {
    this.subjectsToAdd.update(arr => arr.filter((_, i) => i !== index));
  }

  async saveClassesAndSubjects() {
    this.isSubmitting.set(true);
    let errorCount = 0;
    
    // Save classes sequentially
    for (const c of this.classesToAdd()) {
      if (!c.classname || !c.section) continue;
      
      try {
        await new Promise<void>((resolve, reject) => {
          this.classService.createClass({ classname: c.classname, section: c.section }).subscribe({
            next: () => resolve(),
            error: () => reject()
          });
        });
      } catch {
        errorCount++;
      }
    }

    // Save subjects sequentially
    for (const s of this.subjectsToAdd()) {
      if (!s.subjectName) continue;
      
      try {
        await new Promise<void>((resolve, reject) => {
          this.subjectService.createSubject({ subjectName: s.subjectName }).subscribe({
            next: () => resolve(),
            error: () => reject()
          });
        });
      } catch {
        errorCount++;
      }
    }

    this.isSubmitting.set(false);
    
    if (errorCount > 0) {
      this.toastService.warning(`Some classes or subjects could not be saved. You can fix this later.`);
    } else {
      this.toastService.success('Classes and subjects configured.');
    }
    
    this.currentStep.update(s => s + 1);
  }
  
  skipClassesAndSubjects() {
    this.currentStep.update(s => s + 1);
  }

  finishSetup() {
    this.router.navigate(['/dashboard']);
  }
}
