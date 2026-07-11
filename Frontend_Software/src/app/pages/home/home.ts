import { isPlatformBrowser } from '@angular/common';
import { Component, OnInit, PLATFORM_ID, ViewEncapsulation, inject, signal, viewChild } from '@angular/core';
import { Ejercicio, Maquina } from '../../core/models/catalogo.models';
import { Reserva } from '../../core/models/reserva.models';
import { RutinaResumen } from '../../core/models/rutina.models';
import { SesionHistorial } from '../../core/models/sesion.models';
import { CatalogoService } from '../../core/services/catalogo.service';
import { HealthService } from '../../core/services/health.service';
import { AuthService } from '../../core/services/auth.service';
import { MachinesSection } from '../../features/maquinas/machines-section';
import { ReservationsSection } from '../../features/reservas/reservations-section';
import { TrainingSection } from '../../features/entrenamiento/training-section';
import { HistorySection } from '../../features/historial/history-section';
import { AuthDialogs } from '../../shared/auth-dialogs/auth-dialogs';
import { AppHeader } from '../../shared/header/app-header';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [MachinesSection, ReservationsSection, TrainingSection, HistorySection, AuthDialogs, AppHeader],
  templateUrl: './home.html',
  styleUrl: './home.css',
  encapsulation: ViewEncapsulation.None
})
export class Home implements OnInit {
  private readonly catalogoService = inject(CatalogoService);
  private readonly healthService = inject(HealthService);
  private readonly authService = inject(AuthService);
  private readonly platformId = inject(PLATFORM_ID);
  protected readonly authDialogs = viewChild.required(AuthDialogs);
  protected readonly currentUser = this.authService.currentUser;
  protected readonly apiMessage = signal('');
  protected readonly healthStatus = signal('Comprobando backend...');
  protected readonly predefinedRoutines = signal<RutinaResumen[]>([]);
  protected readonly machines = signal<Maquina[]>([]);
  protected readonly exercises = signal<Ejercicio[]>([]);
  protected readonly reservations = signal<Reserva[]>([]);
  protected readonly workoutHistory = signal<SesionHistorial[]>([]);
  protected readonly historyRefresh = signal(0);

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    this.checkHealth();
    this.loadCatalogs();
  }

  protected scrollToRutinas(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    document.getElementById('entrenamiento')?.scrollIntoView({ behavior: 'smooth' });
  }

  private loadCatalogs(): void {
    this.catalogoService.getMaquinas().subscribe({
      next: (machines) => {
        this.machines.set(machines);
      }
    });

    this.catalogoService.getEjercicios().subscribe({
      next: (exercises) => {
        this.exercises.set(exercises);
      }
    });
  }

  private checkHealth(): void {
    this.healthService.health().subscribe({
      next: (response) => this.healthStatus.set(response.message),
      error: () => this.healthStatus.set('Backend no disponible.')
    });
  }

}
