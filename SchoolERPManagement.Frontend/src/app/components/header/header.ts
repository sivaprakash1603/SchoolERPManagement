import { Component, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ThemeService } from '../../services/theme';
import { LayoutService } from '../../services/layout.service';
import { StudentService } from '../../services/student.service';
import { TeacherService } from '../../services/teacher.service';
import { ParentService } from '../../services/parent.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header implements OnInit {
  themeService = inject(ThemeService);
  layoutService = inject(LayoutService);
  private studentService = inject(StudentService);
  private teacherService = inject(TeacherService);
  private parentService = inject(ParentService);

  displayName = signal<string>('Loading...');
  displayPhotoUrl = signal<string | null>(null);
  
  constructor(private router: Router) {}

  ngOnInit() {
    const role = sessionStorage.getItem('role') || 'Admin';
    const userIdStr = sessionStorage.getItem('userId');
    const userId = userIdStr ? parseInt(userIdStr, 10) : null;
    const username = sessionStorage.getItem('username') || '';

    if (role === 'Admin') {
      this.displayName.set('Administrator');
      this.displayPhotoUrl.set(null);
    } else if (role === 'Student' && userId) {
      this.studentService.getStudentByUserId(userId).subscribe({
        next: (res) => {
          this.displayName.set(res.name);
          this.displayPhotoUrl.set(res.profilePhotoUrl ? 'http://localhost:5203' + res.profilePhotoUrl : null);
        },
        error: () => this.displayName.set(username || 'Student')
      });
    } else if (role === 'Teacher' && username) {
      this.teacherService.getTeacherByUsername(username).subscribe({
        next: (res) => {
          this.displayName.set(res.name);
          this.displayPhotoUrl.set(res.profilePhotoUrl ? 'http://localhost:5203' + res.profilePhotoUrl : null);
        },
        error: () => this.displayName.set(username || 'Teacher')
      });
    } else if (role === 'Parent' && userId) {
      this.parentService.getParentByUserId(userId).subscribe({
        next: (res) => {
          this.displayName.set(res.name);
          this.displayPhotoUrl.set(null);
        },
        error: () => this.displayName.set(username || 'Parent')
      });
    } else {
      this.displayName.set(username || 'User');
    }
  }

  logout() {
    sessionStorage.clear();
    this.router.navigate(['/login']);
  }
}
