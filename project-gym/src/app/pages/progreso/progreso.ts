import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Metrica, SesionHistorial } from '../../core/models/auth.models';
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

type LiftMetric = {
  id: string;
  sourceId: number | null;
  exercise: string;
  valueKg: number;
  date: string;
  notes: string;
};

type LiftMetricSummary = {
  exercise: string;
  firstKg: number;
  lastKg: number;
  bestKg: number;
  improvementKg: number;
  count: number;
  latestDate: string;
};

@Component({
  selector: 'app-progreso',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './progreso.html',
  styleUrl: './progreso.css',
})
export class Progreso implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly gymService = inject(GymService);
  private readonly router = inject(Router);

  protected readonly currentUser = this.authService.currentUser;
  protected readonly history = signal<SesionHistorial[]>([]);
  protected readonly liftMetrics = signal<LiftMetric[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal('');
  protected readonly totalGoal = computed(() => Math.max(20, this.history().length));
  protected readonly metricError = signal('');
  protected readonly metricSuccess = signal('');
  protected metricForm: {
    exercise: string;
    valueKg: number | null;
    date: string;
    notes: string;
  } = {
    exercise: '',
    valueKg: null,
    date: this.getTodayIso(),
    notes: '',
  };

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

    return this.history().length > 0 ? 'Iniciando' : 'Sin sesiones';
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

  protected readonly metricSummaries = computed<LiftMetricSummary[]>(() => {
    const groups = new Map<string, LiftMetric[]>();

    for (const metric of this.visibleMetrics()) {
      const key = this.normalizeMetricName(metric.exercise);
      groups.set(key, [...(groups.get(key) ?? []), metric]);
    }

    return Array.from(groups.values())
      .map((metrics) => {
        const ordered = [...metrics].sort((a, b) => this.toTime(a.date) - this.toTime(b.date));
        const first = ordered[0];
        const last = ordered[ordered.length - 1];
        const bestKg = Math.max(...ordered.map((metric) => metric.valueKg));

        return {
          exercise: last.exercise,
          firstKg: first.valueKg,
          lastKg: last.valueKg,
          bestKg,
          improvementKg: last.valueKg - first.valueKg,
          count: ordered.length,
          latestDate: last.date,
        };
      })
      .sort((a, b) => this.toTime(b.latestDate) - this.toTime(a.latestDate));
  });

  protected readonly metricTimeline = computed(() =>
    [...this.visibleMetrics()]
      .sort((a, b) => this.toTime(b.date) - this.toTime(a.date))
      .slice(0, 8)
  );

  ngOnInit(): void {
    const user = this.currentUser();

    if (!user) {
      this.loading.set(false);
      this.errorMessage.set('Debes iniciar sesion para ver tu progreso.');
      return;
    }

    this.loadMetricHistory(user.id);
    this.loadProgress(user.id);
  }

  protected volverAlInicio(): void {
    this.router.navigate(['/']);
  }

  protected addMetric(): void {
    const user = this.currentUser();
    const exercise = this.metricForm.exercise.trim();
    const valueKg = Number(this.metricForm.valueKg);

    this.metricError.set('');
    this.metricSuccess.set('');

    if (!user) {
      this.metricError.set('Debes iniciar sesion para guardar metricas.');
      return;
    }

    if (!exercise) {
      this.metricError.set('Ingresa el nombre del ejercicio.');
      return;
    }

    if (!Number.isFinite(valueKg) || valueKg <= 0) {
      this.metricError.set('Ingresa un peso valido en kg.');
      return;
    }

    this.gymService.crearMetrica({
      idUsuario: user.id,
      ejercicio: exercise,
      pesoKg: Math.round(valueKg * 10) / 10,
      fecha: this.metricForm.date || this.getTodayIso(),
      notas: this.metricForm.notes.trim() || null,
    }).subscribe({
      next: (metric) => {
        this.liftMetrics.set([this.mapApiMetric(metric), ...this.liftMetrics()]);
        this.metricForm = {
          exercise: '',
          valueKg: null,
          date: this.getTodayIso(),
          notes: '',
        };
        this.metricSuccess.set('Metrica agregada al historial.');
      },
      error: () => {
        this.metricError.set('No se pudo guardar la metrica.');
      },
    });
  }

  protected removeMetric(metricId: string): void {
    const user = this.currentUser();

    if (!user) {
      return;
    }

    const metric = this.liftMetrics().find((item) => item.id === metricId);

    if (!metric?.sourceId) {
      return;
    }

    this.gymService.eliminarMetrica(metric.sourceId, user.id).subscribe({
      next: () => {
        this.liftMetrics.set(this.liftMetrics().filter((item) => item.id !== metricId));
      },
      error: () => {
        this.metricError.set('No se pudo eliminar la metrica.');
      },
    });
  }

  protected formatKg(value: number): string {
    return `${Number.isInteger(value) ? value : value.toFixed(1)} kg`;
  }

  protected formatDelta(value: number): string {
    if (value > 0) {
      return `+${this.formatKg(value)}`;
    }

    return this.formatKg(value);
  }

  private loadProgress(idUsuario: number): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.gymService.getHistorial(idUsuario).subscribe({
      next: (history) => {
        this.history.set(this.uniqueHistoryBySession(history));
        this.loading.set(false);
      },
      error: () => {
        this.history.set([]);
        this.loading.set(false);
        this.errorMessage.set('No se pudo cargar el progreso del usuario.');
      },
    });
  }

  private uniqueHistoryBySession(history: SesionHistorial[]): SesionHistorial[] {
    const unique = new Map<number, SesionHistorial>();

    for (const entry of history) {
      if (!unique.has(entry.sesion.idSesion)) {
        unique.set(entry.sesion.idSesion, entry);
      }
    }

    return Array.from(unique.values());
  }

  private loadMetricHistory(idUsuario: number): void {
    this.gymService.getMetricas(idUsuario).subscribe({
      next: (metrics) => {
        this.liftMetrics.set(metrics.map((metric) => this.mapApiMetric(metric)));
      },
      error: () => {
        this.liftMetrics.set([]);
        this.metricError.set('No se pudieron cargar las metricas de fuerza.');
      },
    });
  }

  private visibleMetrics(): LiftMetric[] {
    return this.liftMetrics();
  }

  private mapApiMetric(metric: Metrica): LiftMetric {
    return {
      id: String(metric.idMetrica),
      sourceId: metric.idMetrica,
      exercise: metric.ejercicio,
      valueKg: Math.round(metric.pesoKg * 10) / 10,
      date: metric.fecha,
      notes: metric.notas ?? '',
    };
  }

  private normalizeMetricName(value: string): string {
    return value.trim().toLocaleLowerCase('es-CL');
  }

  private getTodayIso(): string {
    return new Date().toISOString().slice(0, 10);
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
