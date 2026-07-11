import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { Perfil } from './pages/perfil/perfil';
import { Progreso } from './pages/progreso/progreso';

export const routes: Routes = [
  { path: 'perfil', component: Perfil },
  { path: 'progreso', component: Progreso },
  { path: '', component: Home },
  { path: '**', redirectTo: '' }
];
