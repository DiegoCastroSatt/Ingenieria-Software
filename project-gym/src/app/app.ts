import { CommonModule, isPlatformBrowser } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, HostListener, OnInit, PLATFORM_ID, inject, signal, computed } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { filter, finalize } from 'rxjs/operators';
import {
  ActualizarPerfilImcPayload,
  AgregarDetalleSesionPayload,
  AuthUser,
  CompletarSesionPayload,
  CrearRutinaPayload,
  DetalleSesionEntrenamiento,
  EditarRutinaPayload,
  Ejercicio,
  ImcRecommendationResponse,
  Maquina,
  Reserva,
  RutinaDetalle,
  RutinaEjercicioRequest,
  RutinaResumen,
  SesionEntrenamiento,
  SesionHistorial
} from './core/models/auth.models';
import { GymService } from './core/services/gym-data.service';
import { AuthService } from './core/services/auth.service';
import { formatApiError } from './core/utils/api-error';

type DailyExercise = {
  name: string;
  detail: string;
  sets: string;
  done: boolean;
};

type ExerciseWithImage = {
  name: string;
  image: string;
  category: string;
  purpose: string;
  description: string;
};

type TrainingRoutineTab = 'personal' | 'predefinida' | 'libre';

type RoutineExerciseDraft = RutinaEjercicioRequest & {
  key: number;
};

type RoutineFormDraft = {
  nombre: string;
  descripcion: string;
  objetivo: string;
  dificultad: string;
  esPublica: boolean;
  ejercicios: RoutineExerciseDraft[];
};

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private readonly gymService = inject(GymService);
  private readonly authService = inject(AuthService);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly router = inject(Router);

  protected readonly title = signal('project-gym');
  protected readonly isPerfilRoute = signal(false);

  constructor() {
    this.updateStandaloneRoute(this.router.url);

    // Escuchar cambios de ruta para mostrar paginas standalone.
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.updateStandaloneRoute(event.urlAfterRedirects);
    });
  }

  protected readonly menuOpen = signal(false);
  protected readonly userMenuOpen = signal(false);
  protected readonly loginModalOpen = signal(false);
  protected readonly registerModalOpen = signal(false);
  protected readonly loginError = signal('');
  protected readonly registerError = signal('');
  protected readonly registerSuccess = signal('');
  protected readonly loginLoading = signal(false);
  protected readonly registerLoading = signal(false);
  protected readonly detailLoading = signal(false);
  protected readonly currentUser = signal<AuthUser | null>(null);
  protected readonly apiMessage = signal('');
  protected readonly healthStatus = signal('Comprobando backend...');
  protected readonly bmiResult = signal<ImcRecommendationResponse | null>(null);
  protected readonly recommendedRoutines = signal<RutinaResumen[]>([]);
  protected readonly predefinedRoutines = signal<RutinaResumen[]>([]);
  protected readonly userRoutines = signal<RutinaResumen[]>([]);
  protected readonly routineDetails = signal<Record<number, RutinaDetalle>>({});
  protected readonly expandedRoutines = signal<Record<number, boolean>>({});
  protected readonly trainingRoutineTab = signal<TrainingRoutineTab>('personal');
  protected readonly selectedTrainingRoutineId = signal(0);
  protected readonly routineEditorOpen = signal(false);
  protected readonly editingRoutineId = signal<number | null>(null);
  protected readonly sessionDetails = signal<DetalleSesionEntrenamiento[]>([]);
  private routineExerciseDraftKey = 1;
  protected readonly machines = signal<Maquina[]>([]);
  protected readonly favoriteMachines = signal<Maquina[]>([]);
  protected readonly favoriteMachinesOpen = signal(true);
  protected readonly machineSearchTerm = signal('');
  protected readonly machineCategoryFilter = signal('');
  protected readonly machineLocationFilter = signal('');
  protected readonly machineMuscleFilter = signal('');
  protected readonly machineStatusFilter = signal('');
  protected readonly machineCategories = computed(() => this.uniqueSorted(this.machines().map((machine) => machine.tipoMaquina)));
  protected readonly machineLocations = computed(() => this.uniqueSorted(this.machines().map((machine) => machine.ubicacion)));
  protected readonly machineMuscleGroups = computed(() =>
    this.uniqueSorted(this.machines().flatMap((machine) => this.splitMachineMuscles(machine.musculosObjetivo)))
  );
  protected readonly hasMachineFilters = computed(() =>
    !!this.machineSearchTerm().trim() ||
    !!this.machineCategoryFilter() ||
    !!this.machineLocationFilter() ||
    !!this.machineMuscleFilter() ||
    !!this.machineStatusFilter()
  );
  protected readonly filteredMachines = computed(() => {
    const searchTerm = this.normalizeMachineText(this.machineSearchTerm());
    const category = this.machineCategoryFilter();
    const location = this.machineLocationFilter();
    const muscle = this.normalizeMachineText(this.machineMuscleFilter());
    const status = this.machineStatusFilter();

    return this.machines().filter((machine) => {
      if (category && machine.tipoMaquina !== category) {
        return false;
      }

      if (location && machine.ubicacion !== location) {
        return false;
      }

      if (status && machine.estado !== status) {
        return false;
      }

      if (muscle && !this.normalizeMachineText(machine.musculosObjetivo).includes(muscle)) {
        return false;
      }

      if (!searchTerm) {
        return true;
      }

      const searchableText = this.normalizeMachineText([
        machine.nombre,
        machine.descripcion,
        machine.tipoMaquina,
        machine.ubicacion,
        machine.musculosObjetivo,
        machine.estado
      ].join(' '));
      return searchableText.includes(searchTerm);
    });
  });
  protected readonly exercises = signal<Ejercicio[]>([]);
  protected readonly reservations = signal<Reserva[]>([]);
  protected readonly activeReservations = signal<Reserva[]>([]);
  protected readonly currentSession = signal<SesionEntrenamiento | null>(null);
  protected readonly workoutHistory = signal<SesionHistorial[]>([]);
  protected readonly todayIso = this.getTodayIso();

  private updateStandaloneRoute(url: string): void {
    const path = url.split('?')[0].split('#')[0];
    this.isPerfilRoute.set(path === '/perfil' || path === '/progreso');
  }

  protected readonly catalogCardio = signal<ExerciseWithImage[]>([
    { 
      name: 'Trote en cinta', 
      image: 'caminadora.png',
      category: 'Cardio',
      purpose: 'Resistencia cardiovascular',
      description: 'Mejora la capacidad aeróbica y la salud del corazón'
    },
    { 
      name: 'Bicicleta estática', 
      image: 'Bici.png',
      category: 'Cardio',
      purpose: 'Resistencia cardiovascular',
      description: 'Cardio de bajo impacto, ideal para articulaciones'
    },
    {
      name: 'Elíptica',
      image: 'eliptica.png',
      category: 'Cardio',
      purpose: 'Resistencia cardiovascular',
      description: 'Ejercicio de bajo impacto que trabaja todo el cuerpo'
    }
  ]);
  protected maquinaSeleccionadaDisponible(): boolean {
    return this.fechaReservaPermitida() && this.horarioReservaValido() && this.estadoReservaSeleccionada() === 'disponible';
  }

  protected estadoReservaSeleccionada(): string {
    if (!this.fechaReservaPermitida()) {
      return 'fecha_pasada';
    }

    const selectedMachine = this.machines().find(
      (machine) => machine.idMaquina === this.reservationForm.idMaquina
    );

    return selectedMachine ? this.estadoReservaMaquina(selectedMachine) : 'sin_seleccion';
  }

  protected estadoReservaMaquina(machine: Maquina): string {
    if (machine.estado === 'mantencion' || machine.estado === 'fuera_servicio') {
      return 'fuera_servicio';
    }

    if (machine.estado === 'ocupada' || this.tieneReservaTraslapada(machine.idMaquina)) {
      return 'reservado';
    }

    return 'disponible';
  }

  protected etiquetaEstadoReserva(machine: Maquina): string {
    const estado = this.estadoReservaMaquina(machine);

    if (estado === 'reservado') {
      return 'Reservado';
    }

    if (estado === 'fuera_servicio') {
      return 'Fuera de Servicio';
    }

    return 'Disponible';
  }

  protected textoBotonReserva(): string {
    const estado = this.estadoReservaSeleccionada();

    if (estado === 'reservado') {
      return 'Reservado';
    }

    if (estado === 'fuera_servicio') {
      return 'Fuera de Servicio';
    }

    if (estado === 'sin_seleccion') {
      return 'Selecciona una maquina';
    }

    if (estado === 'fecha_pasada') {
      return 'Fecha no valida';
    }

    if (!this.horarioReservaValido()) {
      return 'Horario no valido';
    }

    return 'Reservar maquina';
  }

  protected mensajeReservaSeleccionada(): string {
    const estado = this.estadoReservaSeleccionada();

    if (estado === 'reservado') {
      return 'Reservado: esta maquina ya tiene una reserva activa que cruza con este horario.';
    }

    if (estado === 'fuera_servicio') {
      return 'Fuera de Servicio: esta maquina esta en mantencion o no esta operativa.';
    }

    if (estado === 'fecha_pasada') {
      return 'No se pueden crear reservas en fechas anteriores a hoy.';
    }

    if (!this.horarioReservaValido()) {
      return 'El horario debe tener una hora de inicio menor que la hora de termino.';
    }

    return 'Disponible para el horario seleccionado.';
  }

  protected onFechaReservaChange(): void {
    if (!this.fechaReservaPermitida()) {
      this.activeReservations.set([]);
      return;
    }

    this.loadActiveReservations();
  }

  protected readonly catalogFuerza = signal<ExerciseWithImage[]>([
    { 
      name: 'Press de hombro', 
      image: 'press hombro con palanca.jpeg',
      category: 'Fuerza, e Hipertrofia',
      purpose: 'Fuerza de hombros',
      description: 'Trabaja deltoides y parte superior del cuerpo'
    },
    { 
      name: 'Curl de bíceps', 
      image: 'curl biceps.jpeg',
      category: 'Fuerza',
      purpose: 'Fuerza de brazos',
      description: 'Fortalece bíceps y flexores de codo'
    },
    { 
      name: 'Prensa de piernas', 
      image: 'prensa piernas.jpeg',
      category: 'Fuerza/Hipertrofia',
      purpose: 'Fuerza de piernas',
      description: 'Trabaja cuadríceps, glúteos y aductores'
    },
    { 
      name: 'Press de pecho', 
      image: 'press pecho.png',
      category: 'Fuerza/Hipertrofia',
      purpose: 'Fuerza de pecho',
      description: 'Fortalece pectorales, hombros y tríceps'
    },
    { 
      name: 'Remo sentado', 
      image: 'remo sentado.jpeg',
      category: 'Fuerza/Hipertrofia',
      purpose: 'Fuerza de espalda',
      description: 'Trabaja dorsales, romboides y parte media de la espalda'
    }
  ]);

  protected readonly catalogHipertrofia = signal<ExerciseWithImage[]>([
    { 
      name: 'Press de hombro', 
      image: 'press hombro con palanca.jpeg',
      category: 'Hipertrofia',
      purpose: 'Ganancia muscular de hombros',
      description: 'Genera micro-ruptura en fibras musculares del hombro'
    },
    { 
      name: 'Curl de bíceps', 
      image: 'curl biceps.jpeg',
      category: 'Hipertrofia',
      purpose: 'Ganancia muscular de brazos',
      description: 'Aislamiento efectivo para el crecimiento del bíceps'
    },
    { 
      name: 'Sentadilla', 
      image: 'sentadilla.jpeg',
      category: 'Hipertrofia',
      purpose: 'Ganancia muscular de piernas',
      description: 'Ejercicio compuesto para crecimiento de cuadríceps y glúteos'
    }
  ]);

  protected readonly catalogFlexibilidad = signal<ExerciseWithImage[]>([
    { 
      name: 'Plancha frontal', 
      image: 'plancha.png',
      category: 'Flexibilidad/Core',
      purpose: 'Fortalecimiento abdominal',
      description: 'Trabaja core, espalda baja y estabilización del cuerpo'
    },
    { 
      name: 'Cuadríceps', 
      image: 'cuadriceps.png',
      category: 'Hipertrofia/Flexibilidad',
      purpose: 'Flexibilidad de piernas',
      description: 'Estira y mejora la movilidad del cuadríceps'
    }
  ]);

  protected readonly allExercises = signal<ExerciseWithImage[]>([
    // Cardio
    { 
      name: 'Trote en cinta', 
      image: 'cardio.png',
      category: 'Cardio',
      purpose: 'Resistencia cardiovascular',
      description: 'Mejora la capacidad aeróbica y la salud del corazón'
    },
    { 
      name: 'Bicicleta estática', 
      image: 'cardio.png',
      category: 'Cardio',
      purpose: 'Resistencia cardiovascular',
      description: 'Cardio de bajo impacto, ideal para articulaciones'
    },
    // Fuerza
    { 
      name: 'Press de hombro', 
      image: 'press hombro con palanca.jpeg',
      category: 'Fuerza',
      purpose: 'Fuerza de hombros',
      description: 'Trabaja deltoides y parte superior del cuerpo'
    },
    { 
      name: 'Curl de bíceps', 
      image: 'curl biceps.jpeg',
      category: 'Fuerza',
      purpose: 'Fuerza de brazos',
      description: 'Fortalece bíceps y flexores de codo'
    },
    { 
      name: 'Prensa de piernas', 
      image: 'prensa piernas.jpeg',
      category: 'Fuerza',
      purpose: 'Fuerza de piernas',
      description: 'Trabaja cuadríceps, glúteos y aductores'
    },
    { 
      name: 'Press de pecho', 
      image: 'press pecho.png',
      category: 'Fuerza',
      purpose: 'Fuerza de pecho',
      description: 'Fortalece pectorales, hombros y tríceps'
    },
    { 
      name: 'Remo sentado', 
      image: 'remo sentado.jpeg',
      category: 'Fuerza',
      purpose: 'Fuerza de espalda',
      description: 'Trabaja dorsales, romboides y parte media de la espalda'
    },
    // Hipertrofia
    { 
      name: 'Sentadilla', 
      image: 'sentadilla.jpeg',
      category: 'Hipertrofia',
      purpose: 'Ganancia muscular de piernas',
      description: 'Ejercicio compuesto para crecimiento de cuadríceps y glúteos'
    },
    // Flexibilidad/Core
    { 
      name: 'Plancha frontal', 
      image: 'plancha.jpeg',
      category: 'Flexibilidad/Core',
      purpose: 'Fortalecimiento abdominal',
      description: 'Trabaja core, espalda baja y estabilización del cuerpo'
    },
    { 
      name: 'Cuadríceps (estiramiento)', 
      image: 'cuadriceps.jpeg',
      category: 'Flexibilidad',
      purpose: 'Flexibilidad de piernas',
      description: 'Estira y mejora la movilidad del cuadríceps'
    }
  ]);

  protected readonly selectedExerciseImage = signal<string | null>(null);

  protected readonly todayExercises = signal<DailyExercise[]>([
    { name: 'Press de pecho', detail: 'Maquina', sets: '4 x 12', done: true },
    { name: 'Fondos asistidos', detail: 'Peso corporal', sets: '3 x 10', done: false },
    { name: 'Press inclinado', detail: 'Mancuernas', sets: '3 x 15', done: false }
  ]);

  protected loginForm = {
    nombre: '',
    password: ''
  };

  protected registerForm = {
    nombre: '',
    rut: '',
    correo: '',
    edad: '',
    password: ''
  };

  protected bmiForm = {
    fechaNacimiento: '',
    sexo: 'masculino',
    alturaCm: 175,
    pesoKg: 75,
    objetivo: 'ganar_fuerza',
    nivelActividad: 'medio'
  };

  protected reservationForm = {
    idMaquina: 0,
    fechaReserva: '',
    horaInicio: '10:00',
    horaFin: '11:00'
  };

  protected sessionForm = {
    idRutina: 0,
    notas: ''
  };

  protected routineForm: RoutineFormDraft = {
    nombre: '',
    descripcion: '',
    objetivo: '',
    dificultad: 'principiante',
    esPublica: false,
    ejercicios: [] as RoutineExerciseDraft[]
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

  ngOnInit(): void {
    this.reservationForm.fechaReserva = this.todayIso;

    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    this.restoreUserSession();
    this.checkHealth();
    this.loadCatalogs();
    this.loadActiveReservations();
    this.loadPredefinedRoutines();
  }

  private restoreUserSession(): void {
    if (!isPlatformBrowser(this.platformId)) {
      console.log('Not in browser, skipping session restore');
      return;
    }

    try {
      const user = this.authService.currentUser();
      if (user) {
        console.log('Restoring user from AuthService:', user);
        this.currentUser.set(user);
        this.loadUserData(user.id);
      } else {
        console.log('No active user session');
      }
    } catch (error) {
      console.error('Error restoring user session:', error);
      this.authService.logout();
    }
  }

  @HostListener('document:click', ['$event'])
  protected handleDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement | null;

    if (!target?.closest('.menu-wrapper')) {
      this.menuOpen.set(false);
    }

    if (!target?.closest('.user-menu-wrapper')) {
      this.userMenuOpen.set(false);
    }

    if (target?.classList.contains('modal-overlay')) {
      if (target.classList.contains('login-overlay')) {
        this.cerrarLogin();
      }

      if (target.classList.contains('register-overlay')) {
        this.cerrarRegistro();
      }
    }
  }

  protected scrollToRutinas(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    document.getElementById('entrenamiento')?.scrollIntoView({ behavior: 'smooth' });
  }

  protected toggleMenu(): void {
    this.menuOpen.update((value) => !value);
  }

  protected async toggleRoutine(index: number, routineId: number): Promise<void> {
    const isOpen = this.expandedRoutines()[index];
    this.expandedRoutines.update((current) => ({
      ...current,
      [index]: !current[index]
    }));

    if (!isOpen && !this.routineDetails()[routineId]) {
      this.loadRoutineDetail(routineId);
    }
  }

  protected isRoutineOpen(index: number): boolean {
    return !!this.expandedRoutines()[index];
  }

  protected getRoutineDetail(routineId: number): RutinaDetalle | null {
    return this.routineDetails()[routineId] ?? null;
  }

  protected selectTrainingTab(tab: TrainingRoutineTab): void {
    this.trainingRoutineTab.set(tab);
    if (tab === 'libre') {
      this.selectRoutineForTraining(0);
    }
  }

  protected selectRoutineForTraining(routineId: number): void {
    this.selectedTrainingRoutineId.set(routineId);
    this.sessionForm.idRutina = routineId;

    if (routineId > 0) {
      this.loadRoutineDetail(routineId);
    }
  }

  protected selectedTrainingRoutine(): RutinaResumen | null {
    const selectedId = this.selectedTrainingRoutineId();
    if (!selectedId) {
      return null;
    }

    return [...this.userRoutines(), ...this.predefinedRoutines()].find((routine) => routine.idRutina === selectedId) ?? null;
  }

  protected selectedTrainingRoutineDetail(): RutinaDetalle | null {
    const selectedId = this.selectedTrainingRoutineId();
    return selectedId ? this.getRoutineDetail(selectedId) : null;
  }

  protected openCreateRoutineEditor(): void {
    if (!this.currentUser()) {
      this.apiMessage.set('Debes iniciar sesion para crear una rutina.');
      return;
    }

    this.editingRoutineId.set(null);
    this.routineForm = this.getEmptyRoutineForm();
    this.routineEditorOpen.set(true);
  }

  protected openEditRoutineEditor(routine: RutinaResumen): void {
    if (!this.canDeleteRoutine(routine)) {
      this.apiMessage.set('Solo puedes editar tus rutinas personales.');
      return;
    }

    this.editingRoutineId.set(routine.idRutina);
    this.routineEditorOpen.set(true);
    this.loadRoutineForEditor(routine.idRutina);
  }

  protected closeRoutineEditor(): void {
    this.routineEditorOpen.set(false);
    this.editingRoutineId.set(null);
  }

  protected addRoutineExerciseDraft(): void {
    this.routineForm.ejercicios = [
      ...this.routineForm.ejercicios,
      this.createRoutineExerciseDraft(this.routineForm.ejercicios.length + 1)
    ];
  }

  protected removeRoutineExerciseDraft(key: number): void {
    this.routineForm.ejercicios = this.routineForm.ejercicios
      .filter((exercise) => exercise.key !== key)
      .map((exercise, index) => ({ ...exercise, orden: index + 1 }));
  }

  protected saveRoutineForm(): void {
    const user = this.currentUser();
    if (!user) {
      this.apiMessage.set('Debes iniciar sesion para guardar una rutina.');
      return;
    }

    const nombre = this.routineForm.nombre.trim();
    if (!nombre || this.routineForm.ejercicios.length === 0) {
      this.apiMessage.set('La rutina necesita nombre y al menos un ejercicio.');
      return;
    }

    const ejercicios = this.routineForm.ejercicios.map(({ key, ...exercise }, index) => ({
      ...exercise,
      orden: index + 1,
      dia: exercise.dia.trim() || 'General',
      notas: exercise.notas?.trim() || null
    }));

    const basePayload = {
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
      ? this.gymService.editarRutina(editingId, basePayload as EditarRutinaPayload)
      : this.gymService.crearRutina(basePayload as CrearRutinaPayload);

    request.subscribe({
      next: (routine) => {
        this.routineDetails.update((current) => ({
          ...current,
          [routine.idRutina]: routine
        }));
        this.selectRoutineForTraining(routine.idRutina);
        this.loadUserRoutines(user.id);
        this.closeRoutineEditor();
        this.apiMessage.set(editingId ? 'Rutina actualizada.' : 'Rutina creada.');
      },
      error: (error) => {
        this.apiMessage.set(this.extractError(error, 'No se pudo guardar la rutina.'));
      }
    });
  }

  protected copyRoutineAndPrepare(idRutina: number): void {
    const user = this.currentUser();
    if (!user) {
      this.apiMessage.set('Debes iniciar sesion para copiar una rutina.');
      return;
    }

    this.gymService.copiarRutina(idRutina, { idUsuario: user.id, activarRutina: true }).subscribe({
      next: (routine) => {
        this.routineDetails.update((current) => ({
          ...current,
          [routine.idRutina]: routine
        }));
        this.trainingRoutineTab.set('personal');
        this.selectRoutineForTraining(routine.idRutina);
        this.loadUserRoutines(user.id);
        this.apiMessage.set(`Rutina copiada y lista para entrenar: ${routine.nombre}.`);
      },
      error: (error) => {
        this.apiMessage.set(this.extractError(error, 'No se pudo copiar la rutina.'));
      }
    });
  }

  protected toggleExercise(index: number): void {
    this.todayExercises.update((items) =>
      items.map((item, itemIndex) =>
        itemIndex === index ? { ...item, done: !item.done } : item
      )
    );
  }

  protected showExerciseImage(imagePath: string): void {
    this.selectedExerciseImage.set(imagePath);
  }

  protected hideExerciseImage(): void {
    this.selectedExerciseImage.set(null);
  }

  protected getSelectedExerciseInfo(): ExerciseWithImage | null {
    if (!this.selectedExerciseImage()) return null;
    
    const currentImage = this.selectedExerciseImage();
    return this.allExercises().find(ex => ex.image === currentImage) ?? null;
  }

  protected abrirLogin(): void {
    this.menuOpen.set(false);
    this.registerModalOpen.set(false);
    this.loginError.set('');
    this.loginModalOpen.set(true);
  }

  protected cerrarLogin(): void {
    this.loginModalOpen.set(false);
    this.loginLoading.set(false);
  }

  protected abrirRegistro(): void {
    this.menuOpen.set(false);
    this.loginModalOpen.set(false);
    this.registerError.set('');
    this.registerSuccess.set('');
    this.registerModalOpen.set(true);
  }

  protected cerrarRegistro(): void {
    this.registerModalOpen.set(false);
    this.registerLoading.set(false);
  }

  protected cerrarSesion(): void {
    this.currentUser.set(null);
    this.authService.logout();
    this.bmiResult.set(null);
    this.recommendedRoutines.set([]);
    this.userRoutines.set([]);
    this.reservations.set([]);
    this.favoriteMachines.set([]);
    this.currentSession.set(null);
    this.workoutHistory.set([]);
    this.userMenuOpen.set(false);
    this.apiMessage.set('Sesion cerrada.');
  }

  protected toggleUserMenu(): void {
    this.userMenuOpen.update((value) => !value);
  }

  protected cerrarMenuUsuario(): void {
    this.userMenuOpen.set(false);
  }

  protected navegarAPerfil(): void {
    this.userMenuOpen.set(false);
    this.router.navigate(['/perfil']);
  }

  protected navegarAProgreso(): void {
    this.userMenuOpen.set(false);
    this.router.navigate(['/progreso']);
  }

  protected intentarLogin(): void {
    const nombre = this.loginForm.nombre.trim();
    const password = this.loginForm.password.trim();

    if (!nombre || !password) {
      this.loginError.set('Por favor, completa ambos campos.');
      return;
    }

    this.loginLoading.set(true);
    this.loginError.set('');

    this.authService.login(nombre, password).subscribe({
      next: (response) => {
        this.loginLoading.set(false);
        this.currentUser.set(response.user);
        this.cerrarLogin();
        this.apiMessage.set(`Sesion iniciada para ${response.user.nombre}.`);
        this.loadUserData(response.user.id);
      },
      error: (error: HttpErrorResponse) => {
        this.loginLoading.set(false);
        this.loginError.set(this.extractError(error, 'Usuario o contrasena incorrectos.'));
      }
    });
  }

  protected registrarUsuario(): void {
    const payload = {
      nombre: this.registerForm.nombre.trim(),
      rut: this.registerForm.rut.trim(),
      correo: this.registerForm.correo.trim(),
      password: this.registerForm.password.trim()
    };

    if (!payload.nombre || !payload.rut || !payload.correo || !payload.password) {
      this.registerError.set('Completa todos los campos para continuar.');
      return;
    }

    this.registerLoading.set(true);
    this.registerError.set('');
    this.registerSuccess.set('');

    this.authService.register(payload).subscribe({
      next: (response) => {
        this.registerLoading.set(false);
        this.registerSuccess.set(response.message);
        this.currentUser.set(response.user);
        this.cerrarRegistro();
        this.apiMessage.set(`Usuario ${response.user.nombre} registrado correctamente.`);
        this.loadUserData(response.user.id);
      },
      error: (error: HttpErrorResponse) => {
        this.registerLoading.set(false);
        this.registerError.set(this.extractError(error, 'No fue posible registrar al usuario.'));
      }
    });
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
    this.copyRoutineAndPrepare(idRutina);
  }

  protected eliminarRutina(routine: RutinaResumen): void {
    const user = this.currentUser();
    if (!user) {
      this.apiMessage.set('Debes iniciar sesion para eliminar una rutina.');
      return;
    }

    this.gymService.eliminarRutina(routine.idRutina, user.id).subscribe({
      next: () => {
        this.apiMessage.set(`Rutina eliminada: ${routine.nombre}.`);
        this.userRoutines.update((routines) => routines.filter((item) => item.idRutina !== routine.idRutina));

        if (this.sessionForm.idRutina === routine.idRutina) {
          this.sessionForm.idRutina = 0;
        }
      },
      error: (error) => {
        this.apiMessage.set(this.extractError(error, 'No se pudo eliminar la rutina.'));
      }
    });
  }

  protected esMaquinaFavorita(idMaquina: number): boolean {
    return this.favoriteMachines().some((machine) => machine.idMaquina === idMaquina);
  }

  protected toggleFavoriteMachinesSection(): void {
    this.favoriteMachinesOpen.update((isOpen) => !isOpen);
  }

  protected clearMachineFilters(): void {
    this.machineSearchTerm.set('');
    this.machineCategoryFilter.set('');
    this.machineLocationFilter.set('');
    this.machineMuscleFilter.set('');
    this.machineStatusFilter.set('');
  }

  protected toggleMaquinaFavorita(machine: Maquina): void {
    const user = this.currentUser();
    if (!user) {
      this.apiMessage.set('Debes iniciar sesion para guardar maquinas favoritas.');
      return;
    }

    const request = this.esMaquinaFavorita(machine.idMaquina)
      ? this.gymService.removeMaquinaFavorita(machine.idMaquina, user.id)
      : this.gymService.addMaquinaFavorita(machine.idMaquina, { idUsuario: user.id });

    request.subscribe({
      next: (favorites) => {
        this.favoriteMachines.set(favorites);
        const action = this.esMaquinaFavorita(machine.idMaquina) ? 'agregada a favoritas' : 'quitada de favoritas';
        this.apiMessage.set(`${machine.nombre} ${action}.`);
      },
      error: (error) => {
        this.apiMessage.set(this.extractError(error, 'No se pudo actualizar la maquina favorita.'));
      }
    });
  }

  protected canDeleteRoutine(routine: RutinaResumen): boolean {
    const user = this.currentUser();
    return !!user && routine.idUsuario === user.id && routine.tipoRutina !== 'predefinida';
  }

  protected crearReserva(): void {
    const user = this.currentUser();
    if (!user) {
      this.apiMessage.set('Debes iniciar sesion para reservar una maquina.');
      return;
    }
    if (!this.fechaReservaPermitida()) {
      this.apiMessage.set('No se pueden crear reservas en fechas anteriores a hoy.');
      return;
    }
    if (!this.horarioReservaValido()) {
      this.apiMessage.set('La hora de inicio debe ser menor que la hora de termino.');
      return;
    }
    if (!this.maquinaSeleccionadaDisponible()) {
      this.apiMessage.set(this.mensajeReservaSeleccionada());
      return;
    }

    this.gymService.crearReserva({
      idUsuario: user.id,
      idMaquina: this.reservationForm.idMaquina,
      fechaReserva: this.reservationForm.fechaReserva,
      horaInicio: `${this.reservationForm.horaInicio}:00`,
      horaFin: `${this.reservationForm.horaFin}:00`
    }).subscribe({
      next: () => {
        this.apiMessage.set('Reserva creada correctamente.');
        this.loadActiveReservations();
        this.loadReservations(user.id);
      },
      error: (error) => {
        this.apiMessage.set(this.extractError(error, 'No se pudo crear la reserva.'));
      }
    });
  }

  protected puedeCancelarReserva(reservation: Reserva): boolean {
    return reservation.estado === 'activa' && reservation.fechaReserva >= this.todayIso;
  }

  protected cancelarReserva(reservation: Reserva): void {
    const user = this.currentUser();
    if (!user) {
      this.apiMessage.set('Debes iniciar sesion para cancelar una reserva.');
      return;
    }

    if (!this.puedeCancelarReserva(reservation)) {
      this.apiMessage.set('Solo se pueden cancelar reservas activas desde hoy en adelante.');
      return;
    }

    if (isPlatformBrowser(this.platformId) &&
      !window.confirm(`Estas seguro de que quieres cancelar la reserva de ${reservation.nombreMaquina}?`)) {
      return;
    }

    this.gymService.cancelarReserva(reservation.idReserva, { idUsuario: user.id }).subscribe({
      next: (reservaCancelada) => {
        this.reservations.update((reservations) =>
          reservations.map((item) =>
            item.idReserva === reservaCancelada.idReserva ? reservaCancelada : item
          )
        );
        this.apiMessage.set('Reserva cancelada.');
        this.loadActiveReservations();
        this.loadReservations(user.id);
      },
      error: (error) => {
        this.apiMessage.set(this.extractError(error, 'No se pudo cancelar la reserva.'));
      }
    });
  }

  protected iniciarSesionEntrenamiento(): void {
    const user = this.currentUser();
    if (!user) {
      this.apiMessage.set('Debes iniciar sesion para iniciar un entrenamiento.');
      return;
    }

    this.gymService.iniciarSesion({
      idUsuario: user.id,
      idRutina: this.sessionForm.idRutina || null,
      notas: this.sessionForm.notas
    }).subscribe({
      next: (session) => {
        this.currentSession.set(session);
        this.sessionDetails.set([]);
        this.apiMessage.set(`Sesion ${session.idSesion} iniciada.`);
      },
      error: (error) => {
        this.apiMessage.set(this.extractError(error, 'No se pudo iniciar la sesion.'));
      }
    });
  }

  protected agregarDetalleSesion(): void {
    const session = this.currentSession();
    if (!session) {
      this.apiMessage.set('No hay una sesion activa.');
      return;
    }
    if (this.detailLoading()) {
      return;
    }

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
    this.gymService.agregarDetalleSesion(session.idSesion, payload).pipe(
      finalize(() => this.detailLoading.set(false))
    ).subscribe({
      next: (detail) => {
        this.sessionDetails.update((details) => [...details, detail]);
        this.apiMessage.set(`Detalle agregado: ${detail.nombreEjercicio}.`);
      },
      error: (error) => {
        this.apiMessage.set(this.extractError(error, 'No se pudo agregar el detalle de la sesion.'));
      }
    });
  }

  protected completarSesionActual(): void {
    const session = this.currentSession();
    const user = this.currentUser();
    if (!session || !user) {
      this.apiMessage.set('No hay sesion para completar.');
      return;
    }

    const payload: CompletarSesionPayload = {
      porcentajeCompletado: this.completeForm.porcentajeCompletado,
      caloriasEstimadas: this.completeForm.caloriasEstimadas,
      tiempoTotalMinutos: this.completeForm.tiempoTotalMinutos,
      observacion: this.completeForm.observacion,
      notas: this.completeForm.notas
    };

    this.gymService.completarSesion(session.idSesion, payload).subscribe({
      next: () => {
        this.currentSession.set(null);
        this.sessionDetails.set([]);
        this.apiMessage.set('Sesion completada correctamente.');
        this.loadHistory(user.id);
      },
      error: (error) => {
        this.apiMessage.set(this.extractError(error, 'No se pudo completar la sesion.'));
      }
    });
  }

  protected cargarHistorial(): void {
    const user = this.currentUser();
    if (!user) {
      this.apiMessage.set('Debes iniciar sesion para ver el historial.');
      return;
    }

    this.loadHistory(user.id);
  }

  protected reservasParaEntrenamiento(): Reserva[] {
    return this.reservations().filter((reservation) =>
      reservation.estado === 'activa' || reservation.estado === 'confirmada'
    );
  }

  private tieneReservaTraslapada(idMaquina: number): boolean {
    if (!this.horarioReservaValido()) {
      return false;
    }

    const inicioNuevo = this.horaEnMinutos(this.reservationForm.horaInicio);
    const finNuevo = this.horaEnMinutos(this.reservationForm.horaFin);

    return this.activeReservations().some((reservation) =>
      reservation.idMaquina === idMaquina &&
      reservation.estado === 'activa' &&
      reservation.fechaReserva === this.reservationForm.fechaReserva &&
      this.horaEnMinutos(reservation.horaInicio) < finNuevo &&
      this.horaEnMinutos(reservation.horaFin) > inicioNuevo
    );
  }

  private horarioReservaValido(): boolean {
    return this.horaEnMinutos(this.reservationForm.horaInicio) < this.horaEnMinutos(this.reservationForm.horaFin);
  }

  private fechaReservaPermitida(): boolean {
    return !this.reservationForm.fechaReserva || this.reservationForm.fechaReserva >= this.todayIso;
  }

  private getTodayIso(): string {
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private horaEnMinutos(value: string): number {
    const [hours, minutes] = value.split(':').map(Number);

    if (!Number.isFinite(hours) || !Number.isFinite(minutes)) {
      return 0;
    }

    return hours * 60 + minutes;
  }

  private loadCatalogs(): void {
    this.gymService.getMaquinas().subscribe({
      next: (machines) => {
        this.machines.set(machines);
        if (!this.reservationForm.idMaquina && machines.length > 0) {
          this.reservationForm.idMaquina = machines[0].idMaquina;
        }
        if (!this.detailForm.idMaquina && machines.length > 0) {
          this.detailForm.idMaquina = machines[0].idMaquina;
        }
      }
    });

    this.gymService.getEjercicios().subscribe({
      next: (exercises) => {
        this.exercises.set(exercises);
        if (!this.detailForm.idEjercicio && exercises.length > 0) {
          this.detailForm.idEjercicio = exercises[0].idEjercicio;
        }
      }
    });
  }

  private loadPredefinedRoutines(): void {
    this.gymService.getRutinasPredefinidas().subscribe({
      next: (routines) => {
        this.predefinedRoutines.set(routines);
      }
    });
  }

  private loadRoutineDetail(routineId: number): void {
    this.gymService.getRutina(routineId).subscribe({
      next: (routine) => {
        this.routineDetails.update((current) => ({
          ...current,
          [routineId]: routine
        }));
      },
      error: () => {
        this.apiMessage.set('No se pudo cargar el detalle de la rutina.');
      }
    });
  }

  private loadRoutineForEditor(routineId: number): void {
    const cachedRoutine = this.getRoutineDetail(routineId);
    if (cachedRoutine) {
      this.populateRoutineEditor(cachedRoutine);
      return;
    }

    this.gymService.getRutina(routineId).subscribe({
      next: (routine) => {
        this.routineDetails.update((current) => ({
          ...current,
          [routineId]: routine
        }));
        this.populateRoutineEditor(routine);
      },
      error: (error) => {
        this.apiMessage.set(this.extractError(error, 'No se pudo cargar la rutina para editar.'));
      }
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
        key: this.routineExerciseDraftKey++,
        idEjercicio: exercise.idEjercicio,
        dia: exercise.dia || 'General',
        orden: index + 1,
        series: exercise.series ?? null,
        repeticiones: exercise.repeticiones ?? null,
        duracionMinutos: exercise.duracionMinutos ?? null,
        descansoSegundos: exercise.descansoSegundos ?? null,
        notas: exercise.notas ?? ''
      }))
    };
  }

  private getEmptyRoutineForm(): RoutineFormDraft {
    return {
      nombre: '',
      descripcion: '',
      objetivo: '',
      dificultad: 'principiante',
      esPublica: false,
      ejercicios: [this.createRoutineExerciseDraft(1)]
    };
  }

  private createRoutineExerciseDraft(order: number): RoutineExerciseDraft {
    return {
      key: this.routineExerciseDraftKey++,
      idEjercicio: this.exercises()[0]?.idEjercicio ?? 0,
      dia: 'General',
      orden: order,
      series: 3,
      repeticiones: 10,
      duracionMinutos: null,
      descansoSegundos: 60,
      notas: ''
    };
  }

  private loadUserData(idUsuario: number): void {
    console.log('Loading user data for ID:', idUsuario);
    this.loadUserRoutines(idUsuario);
    this.loadReservations(idUsuario);
    this.loadFavoriteMachines(idUsuario);
    this.loadHistory(idUsuario);
    this.gymService.getRecomendaciones(idUsuario).subscribe({
      next: (routines) => {
        console.log('Loaded recommendations:', routines);
        this.recommendedRoutines.set(routines);
      },
      error: (err) => {
        console.error('Error loading recommendations:', err);
        this.recommendedRoutines.set([]);
      }
    });
  }

  private loadUserRoutines(idUsuario: number): void {
    this.gymService.getRutinasUsuario(idUsuario).subscribe({
      next: (routines) => {
        console.log('Loaded user routines:', routines);
        this.userRoutines.set(routines);
        if (!this.sessionForm.idRutina && routines.length > 0) {
          this.sessionForm.idRutina = routines[0].idRutina;
          this.selectedTrainingRoutineId.set(routines[0].idRutina);
        }
      },
      error: (err) => {
        console.error('Error loading user routines:', err);
        this.userRoutines.set([]);
      }
    });
  }

  private loadReservations(idUsuario: number): void {
    this.gymService.getReservasUsuario(idUsuario).subscribe({
      next: (reservations) => {
        console.log('Loaded user reservations:', reservations);
        this.reservations.set(reservations);
      },
      error: (err) => {
        console.error('Error loading reservations:', err);
        this.reservations.set([]);
      }
    });
  }

  private splitMachineMuscles(value: string): string[] {
    return value
      .split(/[(),/]/)
      .map((item) => item.trim())
      .filter(Boolean);
  }

  private uniqueSorted(values: string[]): string[] {
    return [...new Set(values.map((value) => value.trim()).filter(Boolean))]
      .sort((first, second) => first.localeCompare(second));
  }

  private normalizeMachineText(value: string): string {
    return value
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase()
      .trim();
  }

  private loadFavoriteMachines(idUsuario: number): void {
    this.gymService.getMaquinasFavoritas(idUsuario).subscribe({
      next: (machines) => this.favoriteMachines.set(machines),
      error: (err) => {
        console.error('Error loading favorite machines:', err);
        this.favoriteMachines.set([]);
      }
    });
  }

  private loadActiveReservations(): void {
    if (!this.reservationForm.fechaReserva || !this.fechaReservaPermitida()) {
      this.activeReservations.set([]);
      return;
    }

    this.gymService.getReservasActivas(this.reservationForm.fechaReserva).subscribe({
      next: (reservations) => this.activeReservations.set(reservations),
      error: () => this.activeReservations.set([])
    });
  }

  private loadHistory(idUsuario: number): void {
    this.gymService.getHistorial(idUsuario).subscribe({
      next: (history) => {
        console.log('Loaded workout history:', history);
        this.workoutHistory.set(this.uniqueHistoryBySession(history));
      },
      error: (err) => {
        console.error('Error loading history:', err);
        this.workoutHistory.set([]);
      }
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

  private checkHealth(): void {
    this.gymService.health().subscribe({
      next: (response) => this.healthStatus.set(response.message),
      error: () => this.healthStatus.set('Backend no disponible.')
    });
  }

  private extractError(error: HttpErrorResponse, fallback: string): string {
    return formatApiError(error, fallback);
  }
}
