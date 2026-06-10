import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { SesionHistorial } from '../../core/models/auth.models';
import { AuthService } from '../../core/services/auth.service';
import { GymService } from '../../core/services/gym-data.service';

type ProgressBar = {
  name: string;
  value: number;
  percent: number;
  color: string;
};

type SessionDot = {
  index: number;
  className: string;
  label: string;
};

@Component({
  selector: 'app-progreso',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './progreso.html',
  styleUrl: './progreso.css',
})
export class Progreso implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly gymService = inject(GymService);
  private readonly router = inject(Router);

  protected readonly currentUser = this.authService.currentUser;
  protected readonly history = signal<SesionHistorial[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal('');
  protected readonly totalGoal = computed(() => Math.max(20, this.history().length));

  protected readonly completedSessions = computed(() =>
    this.history().filter((entry) => entry.sesion.estado === 'completada' || entry.progreso).length
  );

  protected readonly sessionsWithProgress = computed(() =>
    this.history().filter((entry) => !!entry.progreso).length
  );

  protected readonly activeSessions = computed(() =>
    this.history().filter((entry) => entry.sesion.estado !== 'completada' && !entry.progreso).length
  );

  protected readonly remainingSessions = computed(() =>
    Math.max(0, this.totalGoal() - this.completedSessions())
  );

  protected readonly completionPct = computed(() =>
    this.asPercent(this.completedSessions(), this.totalGoal())
  );

  protected readonly averageProgress = computed(() => {
    const progressEntries = this.history()
      .map((entry) => entry.progreso?.porcentajeCompletado)
      .filter((value): value is number => typeof value === 'number');

    if (progressEntries.length === 0) {
      return 0;
    }

    const total = progressEntries.reduce((sum, value) => sum + value, 0);
    return Math.round(total / progressEntries.length);
  });

  protected readonly totalMinutes = computed(() =>
    this.history().reduce((sum, entry) => sum + (entry.progreso?.tiempoTotalMinutos ?? 0), 0)
  );

  protected readonly totalCalories = computed(() =>
    this.history().reduce((sum, entry) => sum + (entry.progreso?.caloriasEstimadas ?? 0), 0)
  );

  protected readonly statusLabel = computed(() => {
    if (this.completionPct() >= 100) {
      return 'Completado';
    }

    if (this.completionPct() > 50) {
      return 'En progreso';
    }

    return this.completedSessions() > 0 ? 'Iniciando' : 'Sin sesiones';
  });

  protected readonly bars = computed<ProgressBar[]>(() => [
    {
      name: 'Completadas',
      value: this.completedSessions(),
      percent: this.asPercent(this.completedSessions(), this.totalGoal()),
      color: '#00C9A7',
    },
    {
      name: 'Con progreso',
      value: this.sessionsWithProgress(),
      percent: this.asPercent(this.sessionsWithProgress(), this.totalGoal()),
      color: '#4A90E2',
    },
    {
      name: 'En curso',
      value: this.activeSessions(),
      percent: this.asPercent(this.activeSessions(), this.totalGoal()),
      color: '#F59E0B',
    },
    {
      name: 'Restantes',
      value: this.remainingSessions(),
      percent: this.asPercent(this.remainingSessions(), this.totalGoal()),
      color: '#FF6B6B',
    },
  ]);

  protected readonly sessionDots = computed<SessionDot[]>(() => {
    const entries = this.history();

    return Array.from({ length: this.totalGoal() }, (_, index) => {
      const entry = entries[index];
      const number = index + 1;

      if (!entry) {
        return {
          index: number,
          className: 's-pending',
          label: `Sesion ${number}: pendiente`,
        };
      }

      if (entry.sesion.estado === 'completada' || entry.progreso) {
        return {
          index: number,
          className: 's-done',
          label: `Sesion ${number}: completada`,
        };
      }

      return {
        index: number,
        className: 's-active',
        label: `Sesion ${number}: ${entry.sesion.estado}`,
      };
    });
  });

  protected readonly recentHistory = computed(() =>
    [...this.history()]
      .sort((a, b) => this.toTime(b.sesion.fechaInicio) - this.toTime(a.sesion.fechaInicio))
      .slice(0, 6)
  );

  ngOnInit(): void {
    const user = this.currentUser();

    if (!user) {
      this.loading.set(false);
      this.errorMessage.set('Debes iniciar sesion para ver tu progreso.');
      this.router.navigate(['/']);
      return;
    }

    this.loadProgress(user.id);
  }

  protected volverAlInicio(): void {
    this.router.navigate(['/']);
  }

  private loadProgress(idUsuario: number): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.gymService.getHistorial(idUsuario).subscribe({
      next: (history) => {
        this.history.set(history);
        this.loading.set(false);
      },
      error: () => {
        this.history.set([]);
        this.loading.set(false);
        this.errorMessage.set('No se pudo cargar el progreso del usuario.');
      },
    });
  }

  private asPercent(value: number, total: number): number {
    if (total <= 0) {
      return 0;
    }

    return Math.min(100, Math.round((value / total) * 100));
  }

  private toTime(value: string): number {
    const time = new Date(value).getTime();
    return Number.isFinite(time) ? time : 0;
  }
}
