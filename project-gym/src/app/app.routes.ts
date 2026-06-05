import { Routes } from '@angular/router';
import { App } from './app';
import { Perfil } from './pages/perfil/perfil';

export const routes: Routes = [
  { path: 'perfil', component: Perfil },
  { path: '', component: App }  // Ruta por defecto para mostrar el componente principal
];