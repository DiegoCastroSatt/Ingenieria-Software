import { Component, PLATFORM_ID, inject, signal } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { GymService } from '../../core/services/gym-data.service';
import { ActualizarPerfilImcPayload, ActualizarInformacionPublicaPayload, HistorialImc, ImcRecommendationResponse, PerfilUsuario, RutinaResumen } from '../../core/models/auth.models';

@Component({
  selector: 'app-perfil',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './perfil.html',
  styleUrl: './perfil.css'
})
export class Perfil {
  ngOnInit(): void {
    this.loadPerfil();
  }
  private readonly gymService = inject(GymService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser = isPlatformBrowser(this.platformId);
  private selectedAvatarFile: File | null = null;
  private readonly publicInfoCachePrefix = 'perfil_publico_';

  protected readonly currentUser = this.authService.currentUser;
  protected readonly apiMessage = signal('');
  protected readonly bmiResult = signal<ImcRecommendationResponse | null>(null);
  protected readonly bmiHistory = signal<HistorialImc[]>([]);
  protected readonly recommendedRoutines = signal<RutinaResumen[]>([]);
  protected readonly uploadingAvatar = signal(false);
  protected readonly defaultAvatarUrl = '/default-avatar.svg';

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
    avatarOption: 'predeterminado',
    avatarUrl: this.defaultAvatarUrl,
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

    if (this.selectedAvatarFile) {
      this.uploadingAvatar.set(true);
      this.gymService.subirAvatar(user.id, this.selectedAvatarFile).subscribe({
        next: (response) => {
          this.selectedAvatarFile = null;
          this.guardarInformacionPublicaConAvatar(response.avatarUrl);
        },
        error: (error) => {
          this.uploadingAvatar.set(false);
          this.savePublicInfoCache(this.buildCachedPerfil(user.id, this.publicInfoForm.avatarUrl));
          this.apiMessage.set(`${this.extractError(error, 'No se pudo subir el avatar.')} Se mantuvo guardado en este navegador.`);
        }
      });
      return;
    }

    this.guardarInformacionPublicaConAvatar(this.getAvatarUrl());
  }

  private getAvatarUrl(): string | null {
    return this.publicInfoForm.avatarOption === 'subido'
      ? this.publicInfoForm.avatarUrl || this.defaultAvatarUrl
      : this.defaultAvatarUrl;
  }

  protected usarAvatarPredeterminado(): void {
    this.selectedAvatarFile = null;
    this.publicInfoForm.avatarOption = 'predeterminado';
    this.publicInfoForm.avatarUrl = this.defaultAvatarUrl;
  }

  protected subirAvatarDesdeEscritorio(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) {
      input.value = '';
      return;
    }

    if (!file.type.startsWith('image/')) {
      this.apiMessage.set('Selecciona una imagen válida para tu avatar.');
      input.value = '';
      return;
    }

    if (file.size > 2 * 1024 * 1024) {
      this.apiMessage.set('La imagen debe pesar menos de 2 MB.');
      input.value = '';
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const previewUrl = typeof reader.result === 'string' ? reader.result : '';
      if (previewUrl) {
        this.selectedAvatarFile = file;
        this.publicInfoForm.avatarOption = 'subido';
        this.publicInfoForm.avatarUrl = previewUrl;
        this.apiMessage.set('Vista previa lista. Presiona Guardar Información Pública para guardar el avatar.');
      }
      input.value = '';
    };
    reader.onerror = () => {
      this.apiMessage.set('No se pudo leer la imagen seleccionada.');
      input.value = '';
    };
    reader.readAsDataURL(file);
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
        this.patchFormsFromPerfil(response.perfil);
        this.bmiResult.set(response);
        this.recommendedRoutines.set(response.rutinasRecomendadas);
        this.loadHistorialImc();
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

  private guardarInformacionPublicaConAvatar(avatarUrl: string | null): void {
    const user = this.currentUser();
    if (!user) {
      this.uploadingAvatar.set(false);
      this.apiMessage.set('Debes iniciar sesión primero.');
      return;
    }

    const payload: ActualizarInformacionPublicaPayload = {
      alias: this.publicInfoForm.alias || null,
      avatarUrl,
      telefonoTrabajo: this.publicInfoForm.telefonoTrabajo || null,
      emailTrabajo: this.publicInfoForm.emailTrabajo || null,
      sitioPersonal: this.publicInfoForm.sitioPersonal || null,
      twitter: this.publicInfoForm.twitter || null
    };

    this.gymService.actualizarInformacionPublica(user.id, payload).subscribe({
      next: (perfil) => {
        this.uploadingAvatar.set(false);
        this.patchFormsFromPerfil(perfil);
        this.savePublicInfoCache(perfil);
        this.apiMessage.set('Información pública actualizada correctamente.');
      },
      error: (error) => {
        this.uploadingAvatar.set(false);
        this.savePublicInfoCache(this.buildCachedPerfil(user.id, avatarUrl));
        this.apiMessage.set(`${this.extractError(error, 'No se pudo actualizar la información pública.')} Se mantuvo guardado en este navegador.`);
      }
    });
  }

  protected volverAlInicio(): void {
    this.router.navigate(['/']);
  }

  private loadPerfil(): void {
    const user = this.currentUser();
    if (!user) {
      return;
    }

    this.loadHistorialImc();
    this.restorePublicInfoCache(user.id);
    this.gymService.getPerfil(user.id).subscribe({
      next: (perfil) => {
        const cachedPerfil = this.getCachedPublicInfo(user.id);
        const perfilParaMostrar = cachedPerfil && this.hasPublicInfo(cachedPerfil) && !this.hasPublicInfo(perfil)
          ? cachedPerfil
          : perfil;

        this.patchFormsFromPerfil(perfilParaMostrar);
        this.savePublicInfoCache(perfilParaMostrar);
      },
      error: () => {
        this.apiMessage.set('');
      }
    });
  }

  private loadHistorialImc(): void {
    const user = this.currentUser();
    if (!user) {
      this.bmiHistory.set([]);
      return;
    }

    this.gymService.getHistorialImc(user.id).subscribe({
      next: (history) => {
        this.bmiHistory.set(history);
      },
      error: () => {
        this.bmiHistory.set([]);
      }
    });
  }

  private patchFormsFromPerfil(perfil: PerfilUsuario): void {
    this.selectedAvatarFile = null;
    this.bmiForm = {
      fechaNacimiento: perfil.fechaNacimiento ?? '',
      sexo: perfil.sexo ?? 'masculino',
      alturaCm: perfil.alturaCm ?? 175,
      pesoKg: perfil.pesoKg ?? 75,
      objetivo: perfil.objetivo ?? 'ganar_fuerza',
      nivelActividad: perfil.nivelActividad ?? 'medio'
    };

    this.publicInfoForm = {
      alias: perfil.alias ?? '',
      avatarOption: perfil.avatarUrl && perfil.avatarUrl !== this.defaultAvatarUrl ? 'subido' : 'predeterminado',
      avatarUrl: perfil.avatarUrl ?? this.defaultAvatarUrl,
      telefonoTrabajo: perfil.telefonoTrabajo ?? '',
      emailTrabajo: perfil.emailTrabajo ?? '',
      sitioPersonal: perfil.sitioPersonal ?? '',
      twitter: perfil.twitter ?? ''
    };
  }

  private getPublicInfoCacheKey(idUsuario: number): string {
    return `${this.publicInfoCachePrefix}${idUsuario}`;
  }

  private restorePublicInfoCache(idUsuario: number): void {
    const cachedPerfil = this.getCachedPublicInfo(idUsuario);
    if (cachedPerfil) {
      this.patchFormsFromPerfil(cachedPerfil);
    }
  }

  private savePublicInfoCache(perfil: PerfilUsuario): void {
    if (!this.isBrowser) {
      return;
    }

    try {
      localStorage.setItem(this.getPublicInfoCacheKey(perfil.idUsuario), JSON.stringify(perfil));
    } catch {
      this.apiMessage.set('La imagen es muy pesada para quedar guardada en este navegador.');
    }
  }

  private buildCachedPerfil(idUsuario: number, avatarUrl: string | null): PerfilUsuario {
    return {
      idPerfil: 0,
      idUsuario,
      alias: this.publicInfoForm.alias || null,
      avatarUrl,
      telefonoTrabajo: this.publicInfoForm.telefonoTrabajo || null,
      emailTrabajo: this.publicInfoForm.emailTrabajo || null,
      sitioPersonal: this.publicInfoForm.sitioPersonal || null,
      twitter: this.publicInfoForm.twitter || null,
      fechaActualizacion: new Date().toISOString()
    };
  }

  private getCachedPublicInfo(idUsuario: number): PerfilUsuario | null {
    if (!this.isBrowser) {
      return null;
    }

    const cachedPerfil = localStorage.getItem(this.getPublicInfoCacheKey(idUsuario));
    if (!cachedPerfil) {
      return null;
    }

    try {
      return JSON.parse(cachedPerfil) as PerfilUsuario;
    } catch {
      localStorage.removeItem(this.getPublicInfoCacheKey(idUsuario));
      return null;
    }
  }

  private hasPublicInfo(perfil: PerfilUsuario): boolean {
    return !!(
      perfil.alias?.trim() ||
      (perfil.avatarUrl?.trim() && perfil.avatarUrl !== this.defaultAvatarUrl) ||
      perfil.telefonoTrabajo?.trim() ||
      perfil.emailTrabajo?.trim() ||
      perfil.sitioPersonal?.trim() ||
      perfil.twitter?.trim()
    );
  }
}
