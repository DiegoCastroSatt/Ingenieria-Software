import { CommonModule, isPlatformBrowser } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, HostListener, OnInit, PLATFORM_ID, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ActualizarPerfilImcPayload,
  AgregarDetalleSesionPayload,
  AuthResponse,
  AuthUser,
  CompletarSesionPayload,
  Ejercicio,
  ImcRecommendationResponse,
  Maquina,
  Reserva,
  RutinaDetalle,
  RutinaResumen,
  SesionEntrenamiento,
  SesionHistorial
} from './core/models/auth.models';
import { GymService } from './core/services/gym-data.service';

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

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private readonly gymService = inject(GymService);
  private readonly platformId = inject(PLATFORM_ID);

  protected readonly title = signal('project-gym');
  protected readonly menuOpen = signal(false);
  protected readonly loginModalOpen = signal(false);
  protected readonly registerModalOpen = signal(false);
  protected readonly loginError = signal('');
  protected readonly registerError = signal('');
  protected readonly registerSuccess = signal('');
  protected readonly loginLoading = signal(false);
  protected readonly registerLoading = signal(false);
  protected readonly currentUser = signal<AuthUser | null>(null);
  protected readonly apiMessage = signal('');
  protected readonly healthStatus = signal('Comprobando backend...');
  protected readonly bmiResult = signal<ImcRecommendationResponse | null>(null);
  protected readonly recommendedRoutines = signal<RutinaResumen[]>([]);
  protected readonly predefinedRoutines = signal<RutinaResumen[]>([]);
  protected readonly userRoutines = signal<RutinaResumen[]>([]);
  protected readonly routineDetails = signal<Record<number, RutinaDetalle>>({});
  protected readonly expandedRoutines = signal<Record<number, boolean>>({});
  protected readonly machines = signal<Maquina[]>([]);
  protected readonly exercises = signal<Ejercicio[]>([]);
  protected readonly reservations = signal<Reserva[]>([]);
  protected readonly currentSession = signal<SesionEntrenamiento | null>(null);
  protected readonly workoutHistory = signal<SesionHistorial[]>([]);

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
      category: 'Fuerza e Hipertrofia',
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
      category: 'Fuerza e Hipertrofia',
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
      category: 'Hipertrofia y Flexibilidad',
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
    this.reservationForm.fechaReserva = new Date().toISOString().slice(0, 10);

    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    this.checkHealth();
    this.loadCatalogs();
    this.loadPredefinedRoutines();
  }

  @HostListener('document:click', ['$event'])
  protected handleDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement | null;

    if (!target?.closest('.menu-wrapper')) {
      this.menuOpen.set(false);
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

    document.getElementById('rutinas')?.scrollIntoView({ behavior: 'smooth' });
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
  }

  protected isRoutineOpen(index: number): boolean {
    return !!this.expandedRoutines()[index];
  }

  protected getRoutineDetail(routineId: number): RutinaDetalle | null {
    return this.routineDetails()[routineId] ?? null;
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
    this.bmiResult.set(null);
    this.recommendedRoutines.set([]);
    this.userRoutines.set([]);
    this.reservations.set([]);
    this.currentSession.set(null);
    this.workoutHistory.set([]);
    this.apiMessage.set('Sesion cerrada.');
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

    this.gymService.login(nombre, password).subscribe({
      next: (response: AuthResponse) => {
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

    this.gymService.registro(payload).subscribe({
      next: (response: AuthResponse) => {
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
    const user = this.currentUser();
    if (!user) {
      this.apiMessage.set('Debes iniciar sesion para copiar una rutina.');
      return;
    }

    this.gymService.copiarRutina(idRutina, { idUsuario: user.id, activarRutina: true }).subscribe({
      next: (routine) => {
        this.apiMessage.set(`Rutina copiada: ${routine.nombre}.`);
        this.loadUserData(user.id);
      },
      error: (error) => {
        this.apiMessage.set(this.extractError(error, 'No se pudo copiar la rutina.'));
      }
    });
  }

  protected crearReserva(): void {
    const user = this.currentUser();
    if (!user) {
      this.apiMessage.set('Debes iniciar sesion para reservar una maquina.');
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
        this.loadReservations(user.id);
      },
      error: (error) => {
        this.apiMessage.set(this.extractError(error, 'No se pudo crear la reserva.'));
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

    this.gymService.agregarDetalleSesion(session.idSesion, payload).subscribe({
      next: (detail) => {
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

  private loadCatalogs(): void {
    this.gymService.getMaquinas().subscribe({
      next: (machines) => {
        this.machines.set(machines);
        if (!this.reservationForm.idMaquina && machines.length > 0) {
          this.reservationForm.idMaquina = machines[0].idMaquina;
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

  private loadUserData(idUsuario: number): void {
    this.loadUserRoutines(idUsuario);
    this.loadReservations(idUsuario);
    this.loadHistory(idUsuario);
    this.gymService.getRecomendaciones(idUsuario).subscribe({
      next: (routines) => this.recommendedRoutines.set(routines),
      error: () => this.recommendedRoutines.set([])
    });
  }

  private loadUserRoutines(idUsuario: number): void {
    this.gymService.getRutinasUsuario(idUsuario).subscribe({
      next: (routines) => {
        this.userRoutines.set(routines.filter((routine) => routine.idUsuario === idUsuario));
        if (!this.sessionForm.idRutina && routines.length > 0) {
          this.sessionForm.idRutina = routines[0].idRutina;
        }
      }
    });
  }

  private loadReservations(idUsuario: number): void {
    this.gymService.getReservasUsuario(idUsuario).subscribe({
      next: (reservations) => {
        this.reservations.set(reservations);
      }
    });
  }

  private loadHistory(idUsuario: number): void {
    this.gymService.getHistorial(idUsuario).subscribe({
      next: (history) => {
        this.workoutHistory.set(history);
      }
    });
  }

  private checkHealth(): void {
    this.gymService.health().subscribe({
      next: (response) => this.healthStatus.set(response.message),
      error: () => this.healthStatus.set('Backend no disponible.')
    });
  }

  private extractError(error: HttpErrorResponse, fallback: string): string {
    if (typeof error.error === 'string') {
      return error.error;
    }

    if (error.error?.title) {
      return error.error.title;
    }

    if (error.error?.detail) {
      return error.error.detail;
    }

    return fallback;
  }
}
