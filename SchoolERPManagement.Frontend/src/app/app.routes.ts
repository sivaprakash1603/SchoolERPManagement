import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { Dashboard } from './components/dashboard/dashboard';
import { Students } from './components/students/students';
import { Teachers } from './components/teachers/teachers';
import { Parents } from './components/parents/parents';
import { Layout } from './components/layout/layout';

import { StudentOnboarding } from './components/student-onboarding/student-onboarding';
import { AcademicSessions } from './components/academic-sessions/academic-sessions';
import { Classes } from './components/classes/classes';

export const routes: Routes = [
    { path: '', redirectTo: '/login', pathMatch: 'full' },
    { path: 'login', component: Login },
    { 
        path: '', 
        component: Layout,
        children: [
            { path: 'dashboard', component: Dashboard },
            { path: 'students', component: Students },
            { path: 'students/onboarding', component: StudentOnboarding },
            { path: 'teachers', component: Teachers },
            { path: 'parents', component: Parents },
            { path: 'classes', component: Classes },
            { path: 'academic-sessions', component: AcademicSessions }
        ]
    }
];
