import { Routes } from '@angular/router';
import { Layout } from './components/layout/layout';
import { authGuard } from './guards/auth.guard';
import { setupGuard } from './guards/setup.guard';
import { unsavedChangesGuard } from './guards/unsaved-changes.guard';

export const routes: Routes = [
    { 
        path: '', 
        loadComponent: () => import('./components/landing/landing').then(m => m.Landing), 
        pathMatch: 'full' 
    },
    { 
        path: 'login', 
        loadComponent: () => import('./components/login/login').then(m => m.Login) 
    },
    { 
        path: 'reset-password', 
        loadComponent: () => import('./components/reset-password/reset-password').then(m => m.ResetPassword) 
    },
    { 
        path: 'faq', 
        loadComponent: () => import('./components/faq/faq').then(m => m.FAQ) 
    },
    { 
        path: 'setup', 
        loadComponent: () => import('./components/setup-wizard/setup-wizard').then(m => m.SetupWizardComponent), 
        canActivate: [authGuard] 
    },
    { 
        path: '', 
        component: Layout,
        canActivate: [authGuard, setupGuard],
        children: [
            { 
                path: 'dashboard', 
                loadComponent: () => import('./components/dashboard/dashboard').then(m => m.Dashboard) 
            },
            { 
                path: 'students', 
                loadComponent: () => import('./components/students/students').then(m => m.Students) 
            },
            { 
                path: 'students/onboarding', 
                loadComponent: () => import('./components/student-onboarding/student-onboarding').then(m => m.StudentOnboarding), 
                canDeactivate: [unsavedChangesGuard] 
            },
            { 
                path: 'teachers', 
                loadComponent: () => import('./components/teachers/teachers').then(m => m.Teachers) 
            },
            { 
                path: 'teachers/onboarding', 
                loadComponent: () => import('./components/teacher-onboarding/teacher-onboarding').then(m => m.TeacherOnboarding), 
                canDeactivate: [unsavedChangesGuard] 
            },
            { 
                path: 'parents', 
                loadComponent: () => import('./components/parents/parents').then(m => m.Parents) 
            },
            { 
                path: 'classes', 
                loadComponent: () => import('./components/classes/classes').then(m => m.Classes) 
            },
            { 
                path: 'subjects', 
                loadComponent: () => import('./components/subjects/subjects').then(m => m.Subjects) 
            },
            { 
                path: 'timetable', 
                loadComponent: () => import('./components/timetable/timetable').then(m => m.Timetable) 
            },
            { 
                path: 'attendance', 
                loadComponent: () => import('./components/attendance/attendance').then(m => m.Attendance) 
            },
            { 
                path: 'homework', 
                loadComponent: () => import('./components/homework/homework').then(m => m.Homework) 
            },
            { 
                path: 'exams', 
                loadComponent: () => import('./components/exams/exams').then(m => m.Exams) 
            },
            { 
                path: 'fees', 
                loadComponent: () => import('./components/fees/fees').then(m => m.Fees) 
            },
            { 
                path: 'assets', 
                loadComponent: () => import('./components/assets/assets').then(m => m.Assets) 
            },
            { 
                path: 'documents', 
                loadComponent: () => import('./components/documents/documents').then(m => m.Documents) 
            },
            { 
                path: 'payment-result', 
                loadComponent: () => import('./components/payment-result/payment-result').then(m => m.PaymentResultComponent) 
            },
            { 
                path: 'academic-calendar', 
                loadComponent: () => import('./components/academic-calendar/academic-calendar').then(m => m.AcademicCalendar) 
            },
            { 
                path: 'parent-teacher-meetings', 
                loadComponent: () => import('./components/parent-teacher-meetings/parent-teacher-meetings').then(m => m.ParentTeacherMeetings) 
            },
            { 
                path: 'academic-sessions', 
                loadComponent: () => import('./components/academic-sessions/academic-sessions').then(m => m.AcademicSessions) 
            },
            { 
                path: 'notifications', 
                loadComponent: () => import('./components/notifications/notifications').then(m => m.NotificationsComponent) 
            },
            { 
                path: 'chat', 
                loadComponent: () => import('./components/chatbot/chatbot').then(m => m.Chatbot) 
            },
            { 
                path: 'ai-insights', 
                loadComponent: () => import('./components/admin-ai-query/admin-ai-query').then(m => m.AdminAiQuery) 
            }
        ]
    }
];
