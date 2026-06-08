import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { GymService } from '../../core/services/gym-data.service';
import { ActualizarPerfilImcPayload, ActualizarInformacionPublicaPayload, ImcRecommendationResponse, RutinaResumen } from '../../core/models/auth.models';

@Component({
  selector: 'app-perfil',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './perfil.html',
  styleUrl: './perfil.css'
})
export class Perfil {
  constructor() {
    console.log('Perfil component loaded');
  }

  ngOnInit(): void {
    console.log('Perfil component initialized');
  }
  private readonly gymService = inject(GymService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly currentUser = this.authService.currentUser;
  protected readonly apiMessage = signal('');
  protected readonly bmiResult = signal<ImcRecommendationResponse | null>(null);
  protected readonly recommendedRoutines = signal<RutinaResumen[]>([]);

  protected bmiForm = {
    fechaNacimiento: '',
    sexo: 'masculino',
    alturaCm: 175,
    pesoKg: 75,
    objetivo: 'ganar_fuerza',
    nivelActividad: 'medio'
  };

  protected publicInfoForm = {
    alias: '',
    avatarOption: 'ninguno', // 'ninguno', 'foto_oficial', 'actual', 'personalizado'
    customAvatarUrl: '',
    telefonoTrabajo: '',
    emailTrabajo: '',
    sitioPersonal: '',
    twitter: ''
  };

  protected guardarInformacionPublica(): void {
    const user = this.currentUser();
    if (!user) {
      this.apiMessage.set('Debes iniciar sesión primero.');
      return;
    }

    const payload: ActualizarInformacionPublicaPayload = {
      alias: this.publicInfoForm.alias || null,
      avatarUrl: this.getAvatarUrl(),
      telefonoTrabajo: this.publicInfoForm.telefonoTrabajo || null,
      emailTrabajo: this.publicInfoForm.emailTrabajo || null,
      sitioPersonal: this.publicInfoForm.sitioPersonal || null,
      twitter: this.publicInfoForm.twitter || null
    };

    this.gymService.actualizarInformacionPublica(user.id, payload).subscribe({
      next: () => {
        this.apiMessage.set('Información pública actualizada correctamente.');
      },
      error: (error) => {
        this.apiMessage.set(this.extractError(error, 'No se pudo actualizar la información pública.'));
      }
    });
  }

  private getAvatarUrl(): string | null {
    const option = this.publicInfoForm.avatarOption;
    switch (option) {
      case 'ninguno':
        return null;
      case 'foto_oficial':
        // Aquí se podría usar la foto oficial del usuario si estuviera disponible
        return null;
      case 'actual':
        // Aquí se podría usar el avatar actual
        return null;
      case 'personalizado':
        return this.publicInfoForm.customAvatarUrl || null;
      default:
        return null;
    }
  }

  protected guardarPerfilYCalcularImc(): void {
    const user = this.currentUser();
    if (!user) {
      this.apiMessage.set('Debes iniciar sesion primero.');
      return;
    }

    const payload: ActualizarPerfilImcPayload = {
      fechaNacimiento: this.bmiForm.fechaNacimiento || null,
      sexo: this.bmiForm.sexo,
      alturaCm: this.bmiForm.alturaCm,
      pesoKg: this.bmiForm.pesoKg,
      objetivo: this.bmiForm.objetivo,
      nivelActividad: this.bmiForm.nivelActividad
    };

    this.gymService.calcularImc(user.id, payload).subscribe({
      next: (response) => {
        this.bmiResult.set(response);
        this.recommendedRoutines.set(response.rutinasRecomendadas);
        this.apiMessage.set(`IMC calculado: ${response.imc} (${response.categoriaImc}).`);
      },
      error: (error) => {
        this.apiMessage.set(this.extractError(error, 'No se pudo calcular el IMC.'));
      }
    });
  }

  protected copiarRutina(idRutina: number): void {
    const user = this.currentUser();
    if (!user) {
      this.apiMessage.set('Debes iniciar sesion para copiar una rutina.');
      return;
    }

    this.gymService.copiarRutina(idRutina, { idUsuario: user.id, activarRutina: true }).subscribe({
      next: (routine) => {
        this.apiMessage.set(`Rutina copiada: ${routine.nombre}.`);
      },
      error: (error) => {
        this.apiMessage.set(this.extractError(error, 'No se pudo copiar la rutina.'));
      }
    });
  }

  private extractError(error: any, fallback: string): string {
    if (typeof error === 'string') return error;
    if (error?.error?.title) return error.error.title;
    if (error?.error?.detail) return error.error.detail;
    return fallback;
  }

  protected volverAlInicio(): void {
    this.router.navigate(['/']);
  }
}