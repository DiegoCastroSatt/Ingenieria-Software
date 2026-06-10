import { Routes } from '@angular/router';
import { App } from './app';
import { Perfil } from './pages/perfil/perfil';
import { Progreso } from './pages/progreso/progreso';
import { authGuard } from './guards/auth-guard';

export const routes: Routes = [
  { path: 'perfil', component: Perfil },
  {
    path: 'progreso',
    component: Progreso,
    canActivate: [authGuard]
  },
  { path: '', component: App },
  { path: '**', redirectTo: '' }
];
