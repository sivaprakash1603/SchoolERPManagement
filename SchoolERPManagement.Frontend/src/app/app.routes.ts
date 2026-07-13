import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { Landing } from './components/landing/landing';
import { Dashboard } from './components/dashboard/dashboard';
import { Students } from './components/students/students';
import { Teachers } from './components/teachers/teachers';
import { Parents } from './components/parents/parents';
import { Layout } from './components/layout/layout';

import { StudentOnboarding } from './components/student-onboarding/student-onboarding';
import { TeacherOnboarding } from './components/teacher-onboarding/teacher-onboarding';
import { AcademicSessions } from './components/academic-sessions/academic-sessions';
import { Classes } from './components/classes/classes';
import { Subjects } from './components/subjects/subjects';
import { Timetable } from './components/timetable/timetable';
import { Attendance } from './components/attendance/attendance';
import { AcademicCalendar } from './components/academic-calendar/academic-calendar';
import { ParentTeacherMeetings } from './components/parent-teacher-meetings/parent-teacher-meetings';
import { ResetPassword } from './components/reset-password/reset-password';
import { Homework } from './components/homework/homework';
import { Exams } from './components/exams/exams';
import { Fees } from './components/fees/fees';
import { Assets } from './components/assets/assets';
import { Documents } from './components/documents/documents';
import { PaymentResultComponent } from './components/payment-result/payment-result';
import { NotificationsComponent } from './components/notifications/notifications';
import { FAQ } from './components/faq/faq';

import { authGuard } from './guards/auth.guard';
import { unsavedChangesGuard } from './guards/unsaved-changes.guard';

export const routes: Routes = [
    { path: '', component: Landing, pathMatch: 'full' },
    { path: 'login', component: Login },
    { path: 'reset-password', component: ResetPassword },
    { path: 'faq', component: FAQ },
    { 
        path: '', 
        component: Layout,
        canActivate: [authGuard],
        children: [
            { path: 'dashboard', component: Dashboard },
            { path: 'students', component: Students },
            { path: 'students/onboarding', component: StudentOnboarding, canDeactivate: [unsavedChangesGuard] },
            { path: 'teachers', component: Teachers },
            { path: 'teachers/onboarding', component: TeacherOnboarding, canDeactivate: [unsavedChangesGuard] },
            { path: 'parents', component: Parents },
            { path: 'classes', component: Classes },
            { path: 'subjects', component: Subjects },
            { path: 'timetable', component: Timetable },
            { path: 'attendance', component: Attendance },
            { path: 'homework', component: Homework },
            { path: 'exams', component: Exams },
            { path: 'fees', component: Fees },
            { path: 'assets', component: Assets },
            { path: 'documents', component: Documents },
            { path: 'payment-result', component: PaymentResultComponent },
            { path: 'academic-calendar', component: AcademicCalendar },
            { path: 'parent-teacher-meetings', component: ParentTeacherMeetings },
            { path: 'academic-sessions', component: AcademicSessions },
            { path: 'notifications', component: NotificationsComponent }
        ]
    }
];
