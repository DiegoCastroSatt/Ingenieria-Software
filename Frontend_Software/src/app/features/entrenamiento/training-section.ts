import { Component, OnInit, effect, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import {
  AuthUser
} from '../../core/models/auth.models';
import { Ejercicio, Maquina } from '../../core/models/catalogo.models';
import { Reserva } from '../../core/models/reserva.models';
import { CrearRutinaPayload, EditarRutinaPayload, RutinaDetalle, RutinaEjercicioRequest, RutinaResumen } from '../../core/models/rutina.models';
import { AgregarDetalleSesionPayload, CompletarSesionPayload, DetalleSesionEntrenamiento, SesionEntrenamiento } from '../../core/models/sesion.models';
import { RutinasService } from '../../core/services/rutinas.service';
import { SesionesService } from '../../core/services/sesiones.service';
import { formatApiError } from '../../core/utils/api-error';

type TrainingRoutineTab = 'personal' | 'predefinida' | 'libre';
type RoutineExerciseDraft = RutinaEjercicioRequest & { key: number };
type RoutineFormDraft = {
  nombre: string;
  descripcion: string;
  objetivo: string;
  dificultad: string;
  esPublica: boolean;
  ejercicios: RoutineExerciseDraft[];
};

@Component({
  selector: 'app-training-section',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './training-section.html'
})
export class TrainingSection implements OnInit {
  private readonly rutinasService = inject(RutinasService);
  private readonly sesionesService = inject(SesionesService);
  private routineExerciseDraftKey = 1;

  readonly currentUser = input<AuthUser | null>(null);
  readonly machines = input.required<Maquina[]>();
  readonly exercises = input.required<Ejercicio[]>();
  readonly reservations = input.required<Reserva[]>();
  readonly message = output<string>();
  readonly predefinedRoutinesChange = output<RutinaResumen[]>();
  readonly sessionCompleted = output<void>();

  protected readonly predefinedRoutines = signal<RutinaResumen[]>([]);
  protected readonly userRoutines = signal<RutinaResumen[]>([]);
  protected readonly routineDetails = signal<Record<number, RutinaDetalle>>({});
  protected readonly expandedRoutines = signal<Record<number, boolean>>({});
  protected readonly trainingRoutineTab = signal<TrainingRoutineTab>('personal');
  protected readonly selectedTrainingRoutineId = signal(0);
  protected readonly routineEditorOpen = signal(false);
  protected readonly editingRoutineId = signal<number | null>(null);
  protected readonly currentSession = signal<SesionEntrenamiento | null>(null);
  protected readonly sessionDetails = signal<DetalleSesionEntrenamiento[]>([]);
  protected readonly detailLoading = signal(false);

  protected sessionForm = { idRutina: 0, notas: '' };
  protected routineForm: RoutineFormDraft = {
    nombre: '',
    descripcion: '',
    objetivo: '',
    dificultad: 'principiante',
    esPublica: false,
    ejercicios: []
  };
  protected detailForm = {
    idEjercicio: 0,
    idMaquina: 0,
    idReserva: 0,
    seriesRealizadas: 3,
    repeticionesRealizadas: 10,
    pesoUsadoKg: 20,
    duracionMinutos: 10,
    esfuerzoPercibido: 6,
    notas: ''
  };
  protected completeForm = {
    porcentajeCompletado: 100,
    caloriasEstimadas: 250,
    tiempoTotalMinutos: 45,
    observacion: '',
    notas: ''
  };

  constructor() {
    effect(() => {
      const user = this.currentUser();
      if (user) this.loadUserRoutines(user.id);
      else this.userRoutines.set([]);
    });
    effect(() => {
      const exercises = this.exercises();
      if (!this.detailForm.idEjercicio && exercises.length > 0) this.detailForm.idEjercicio = exercises[0].idEjercicio;
    });
    effect(() => {
      const machines = this.machines();
      if (!this.detailForm.idMaquina && machines.length > 0) this.detailForm.idMaquina = machines[0].idMaquina;
    });
  }

  ngOnInit(): void {
    this.routineForm = this.getEmptyRoutineForm();
    this.loadPredefinedRoutines();
  }

  protected toggleRoutine(index: number, routineId: number): void {
    const wasOpen = this.expandedRoutines()[index];
    this.expandedRoutines.update((current) => ({ ...current, [index]: !current[index] }));
    if (!wasOpen && !this.routineDetails()[routineId]) this.loadRoutineDetail(routineId);
  }

  protected isRoutineOpen(index: number): boolean {
    return !!this.expandedRoutines()[index];
  }

  protected getRoutineDetail(routineId: number): RutinaDetalle | null {
    return this.routineDetails()[routineId] ?? null;
  }

  protected selectTrainingTab(tab: TrainingRoutineTab): void {
    this.trainingRoutineTab.set(tab);
    if (tab === 'libre') this.selectRoutineForTraining(0);
  }

  protected selectRoutineForTraining(routineId: number): void {
    this.selectedTrainingRoutineId.set(routineId);
    this.sessionForm.idRutina = routineId;
    if (routineId > 0) this.loadRoutineDetail(routineId);
  }

  protected selectedTrainingRoutine(): RutinaResumen | null {
    const selectedId = this.selectedTrainingRoutineId();
    return selectedId
      ? [...this.userRoutines(), ...this.predefinedRoutines()].find((routine) => routine.idRutina === selectedId) ?? null
      : null;
  }

  protected selectedTrainingRoutineDetail(): RutinaDetalle | null {
    const selectedId = this.selectedTrainingRoutineId();
    return selectedId ? this.getRoutineDetail(selectedId) : null;
  }

  protected openCreateRoutineEditor(): void {
    if (!this.currentUser()) return this.message.emit('Debes iniciar sesion para crear una rutina.');
    this.editingRoutineId.set(null);
    this.routineForm = this.getEmptyRoutineForm();
    this.routineEditorOpen.set(true);
  }

  protected openEditRoutineEditor(routine: RutinaResumen): void {
    if (!this.canDeleteRoutine(routine)) return this.message.emit('Solo puedes editar tus rutinas personales.');
    this.editingRoutineId.set(routine.idRutina);
    this.routineEditorOpen.set(true);
    this.loadRoutineForEditor(routine.idRutina);
  }

  protected closeRoutineEditor(): void {
    this.routineEditorOpen.set(false);
    this.editingRoutineId.set(null);
  }

  protected addRoutineExerciseDraft(): void {
    this.routineForm.ejercicios = [...this.routineForm.ejercicios, this.createRoutineExerciseDraft(this.routineForm.ejercicios.length + 1)];
  }

  protected removeRoutineExerciseDraft(key: number): void {
    this.routineForm.ejercicios = this.routineForm.ejercicios
      .filter((exercise) => exercise.key !== key)
      .map((exercise, index) => ({ ...exercise, orden: index + 1 }));
  }

  protected saveRoutineForm(): void {
    const user = this.currentUser();
    if (!user) return this.message.emit('Debes iniciar sesion para guardar una rutina.');
    const nombre = this.routineForm.nombre.trim();
    if (!nombre || this.routineForm.ejercicios.length === 0) return this.message.emit('La rutina necesita nombre y al menos un ejercicio.');

    const ejercicios = this.routineForm.ejercicios.map(({ key, ...exercise }, index) => ({
      ...exercise,
      orden: index + 1,
      dia: exercise.dia.trim() || 'General',
      notas: exercise.notas?.trim() || null
    }));
    const payload = {
      idUsuario: user.id,
      nombre,
      descripcion: this.routineForm.descripcion.trim(),
      objetivo: this.routineForm.objetivo.trim(),
      dificultad: this.routineForm.dificultad,
      esPublica: this.routineForm.esPublica,
      ejercicios
    };
    const editingId = this.editingRoutineId();
    const request = editingId
      ? this.rutinasService.editar(editingId, payload as EditarRutinaPayload)
      : this.rutinasService.crear(payload as CrearRutinaPayload);

    request.subscribe({
      next: (routine) => {
        this.routineDetails.update((current) => ({ ...current, [routine.idRutina]: routine }));
        this.selectRoutineForTraining(routine.idRutina);
        this.loadUserRoutines(user.id);
        this.closeRoutineEditor();
        this.message.emit(editingId ? 'Rutina actualizada.' : 'Rutina creada.');
      },
      error: (error) => this.message.emit(formatApiError(error, 'No se pudo guardar la rutina.'))
    });
  }

  protected copyRoutineAndPrepare(idRutina: number): void {
    const user = this.currentUser();
    if (!user) return this.message.emit('Debes iniciar sesion para copiar una rutina.');
    this.rutinasService.copiar(idRutina, { idUsuario: user.id, activarRutina: true }).subscribe({
      next: (routine) => {
        this.routineDetails.update((current) => ({ ...current, [routine.idRutina]: routine }));
        this.trainingRoutineTab.set('personal');
        this.selectRoutineForTraining(routine.idRutina);
        this.loadUserRoutines(user.id);
        this.message.emit(`Rutina copiada y lista para entrenar: ${routine.nombre}.`);
      },
      error: (error) => this.message.emit(formatApiError(error, 'No se pudo copiar la rutina.'))
    });
  }

  protected eliminarRutina(routine: RutinaResumen): void {
    const user = this.currentUser();
    if (!user) return this.message.emit('Debes iniciar sesion para eliminar una rutina.');
    this.rutinasService.eliminar(routine.idRutina, user.id).subscribe({
      next: () => {
        this.message.emit(`Rutina eliminada: ${routine.nombre}.`);
        this.userRoutines.update((routines) => routines.filter((item) => item.idRutina !== routine.idRutina));
        if (this.sessionForm.idRutina === routine.idRutina) this.selectRoutineForTraining(0);
      },
      error: (error) => this.message.emit(formatApiError(error, 'No se pudo eliminar la rutina.'))
    });
  }

  protected canDeleteRoutine(routine: RutinaResumen): boolean {
    const user = this.currentUser();
    return !!user && routine.idUsuario === user.id && routine.tipoRutina !== 'predefinida';
  }

  protected iniciarSesionEntrenamiento(): void {
    const user = this.currentUser();
    if (!user) return this.message.emit('Debes iniciar sesion para iniciar un entrenamiento.');
    this.sesionesService.iniciar({ idUsuario: user.id, idRutina: this.sessionForm.idRutina || null, notas: this.sessionForm.notas }).subscribe({
      next: (session) => {
        this.currentSession.set(session);
        this.sessionDetails.set([]);
        this.message.emit(`Sesion ${session.idSesion} iniciada.`);
      },
      error: (error) => this.message.emit(formatApiError(error, 'No se pudo iniciar la sesion.'))
    });
  }

  protected agregarDetalleSesion(): void {
    const session = this.currentSession();
    if (!session) return this.message.emit('No hay una sesion activa.');
    if (this.detailLoading()) return;
    const payload: AgregarDetalleSesionPayload = {
      idEjercicio: this.detailForm.idEjercicio,
      idMaquina: this.detailForm.idMaquina || null,
      idReserva: this.detailForm.idReserva || null,
      seriesRealizadas: this.detailForm.seriesRealizadas,
      repeticionesRealizadas: this.detailForm.repeticionesRealizadas,
      pesoUsadoKg: this.detailForm.pesoUsadoKg,
      duracionMinutos: this.detailForm.duracionMinutos,
      esfuerzoPercibido: this.detailForm.esfuerzoPercibido,
      notas: this.detailForm.notas
    };
    this.detailLoading.set(true);
    this.sesionesService.agregarDetalle(session.idSesion, payload).pipe(finalize(() => this.detailLoading.set(false))).subscribe({
      next: (detail) => {
        this.sessionDetails.update((details) => [...details, detail]);
        this.message.emit(`Detalle agregado: ${detail.nombreEjercicio}.`);
      },
      error: (error) => this.message.emit(formatApiError(error, 'No se pudo agregar el detalle de la sesion.'))
    });
  }

  protected completarSesionActual(): void {
    const session = this.currentSession();
    if (!session || !this.currentUser()) return this.message.emit('No hay sesion para completar.');
    const payload: CompletarSesionPayload = { ...this.completeForm };
    this.sesionesService.completar(session.idSesion, payload).subscribe({
      next: () => {
        this.currentSession.set(null);
        this.sessionDetails.set([]);
        this.message.emit('Sesion completada correctamente.');
        this.sessionCompleted.emit();
      },
      error: (error) => this.message.emit(formatApiError(error, 'No se pudo completar la sesion.'))
    });
  }

  protected reservasParaEntrenamiento(): Reserva[] {
    return this.reservations().filter((reservation) => reservation.estado === 'activa' || reservation.estado === 'confirmada');
  }

  private loadPredefinedRoutines(): void {
    this.rutinasService.getPredefinidas().subscribe({
      next: (routines) => {
        this.predefinedRoutines.set(routines);
        this.predefinedRoutinesChange.emit(routines);
      },
      error: () => {
        this.predefinedRoutines.set([]);
        this.predefinedRoutinesChange.emit([]);
      }
    });
  }

  private loadUserRoutines(idUsuario: number): void {
    this.rutinasService.getPorUsuario(idUsuario).subscribe({
      next: (routines) => {
        this.userRoutines.set(routines);
        if (!this.sessionForm.idRutina && routines.length > 0) this.selectRoutineForTraining(routines[0].idRutina);
      },
      error: () => this.userRoutines.set([])
    });
  }

  private loadRoutineDetail(routineId: number): void {
    this.rutinasService.getDetalle(routineId).subscribe({
      next: (routine) => this.routineDetails.update((current) => ({ ...current, [routineId]: routine })),
      error: () => this.message.emit('No se pudo cargar el detalle de la rutina.')
    });
  }

  private loadRoutineForEditor(routineId: number): void {
    const cached = this.getRoutineDetail(routineId);
    if (cached) return this.populateRoutineEditor(cached);
    this.rutinasService.getDetalle(routineId).subscribe({
      next: (routine) => {
        this.routineDetails.update((current) => ({ ...current, [routineId]: routine }));
        this.populateRoutineEditor(routine);
      },
      error: (error) => this.message.emit(formatApiError(error, 'No se pudo cargar la rutina para editar.'))
    });
  }

  private populateRoutineEditor(routine: RutinaDetalle): void {
    this.routineForm = {
      nombre: routine.nombre,
      descripcion: routine.descripcion ?? '',
      objetivo: routine.objetivo ?? '',
      dificultad: routine.dificultad ?? 'principiante',
      esPublica: routine.esPublica,
      ejercicios: routine.ejercicios.map((exercise, index) => ({
        key: this.routineExerciseDraftKey++, idEjercicio: exercise.idEjercicio, dia: exercise.dia || 'General', orden: index + 1,
        series: exercise.series ?? null, repeticiones: exercise.repeticiones ?? null, duracionMinutos: exercise.duracionMinutos ?? null,
        descansoSegundos: exercise.descansoSegundos ?? null, notas: exercise.notas ?? ''
      }))
    };
  }

  private getEmptyRoutineForm(): RoutineFormDraft {
    return { nombre: '', descripcion: '', objetivo: '', dificultad: 'principiante', esPublica: false, ejercicios: [this.createRoutineExerciseDraft(1)] };
  }

  private createRoutineExerciseDraft(order: number): RoutineExerciseDraft {
    return {
      key: this.routineExerciseDraftKey++, idEjercicio: this.exercises()[0]?.idEjercicio ?? 0, dia: 'General', orden: order,
      series: 3, repeticiones: 10, duracionMinutos: null, descansoSegundos: 60, notas: ''
    };
  }
}
