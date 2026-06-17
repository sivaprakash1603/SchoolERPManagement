import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { Dashboard } from './components/dashboard/dashboard';
import { Layout } from './components/layout/layout';

export const routes: Routes = [
    { path: '', redirectTo: '/login', pathMatch: 'full' },
    { path: 'login', component: Login },
    { 
        path: '', 
        component: Layout,
        children: [
            { path: 'dashboard', component: Dashboard },
            // Other inner pages can be mapped here later
        ]
    }
];
