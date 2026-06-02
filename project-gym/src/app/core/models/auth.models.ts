export type RegisterPayload = {
  nombre: string;
  rut: string;
  correo: string;
  password: string;
  rol?: string;
};

export type AuthUser = {
  id: number;
  nombre: string;
  correo: string;
  rol: string;
};

export type AuthResponse = {
  message: string;
  user: AuthUser;
  token: string;
};

export type HealthResponse = {
  status: string;
  message: string;
};

export type Maquina = {
  idMaquina: number;
  idTipoMaquina: number;
  tipoMaquina: string;
  nombre: string;
  descripcion: string;
  musculosObjetivo: string;
  imagenUrl: string;
  ubicacion: string;
  estado: string;
  cantidad: number;
};

export type Ejercicio = {
  idEjercicio: number;
  nombre: string;
  descripcion: string;
  grupoMuscular: string;
  dificultad: string;
  idMaquina?: number | null;
  nombreMaquina?: string | null;
};

export type RutinaResumen = {
  idRutina: number;
  idUsuario?: number | null;
  nombre: string;
  descripcion: string;
  tipoRutina: string;
  objetivo: string;
  categoriaImc?: string | null;
  dificultad: string;
  esPublica: boolean;
  idRutinaOrigen?: number | null;
};

export type RutinaEjercicio = {
  idRutinaEjercicio: number;
  idRutina: number;
  idEjercicio: number;
  dia: string;
  orden: number;
  series?: number | null;
  repeticiones?: number | null;
  duracionMinutos?: number | null;
  descansoSegundos?: number | null;
  notas?: string | null;
  nombreEjercicio: string;
  descripcionEjercicio: string;
  grupoMuscular: string;
};

export type RutinaDetalle = RutinaResumen & {
  ejercicios: RutinaEjercicio[];
};

export type ActualizarPerfilImcPayload = {
  fechaNacimiento?: string | null;
  sexo?: string | null;
  alturaCm: number;
  pesoKg: number;
  objetivo?: string | null;
  nivelActividad?: string | null;
};

export type PerfilUsuario = {
  idPerfil: number;
  idUsuario: number;
  fechaNacimiento?: string | null;
  sexo?: string | null;
  alturaCm?: number | null;
  pesoKg?: number | null;
  objetivo?: string | null;
  nivelActividad?: string | null;
  fechaActualizacion: string;
};

export type ImcRecommendationResponse = {
  idUsuario: number;
  imc: number;
  categoriaImc: string;
  perfil: PerfilUsuario;
  rutinasRecomendadas: RutinaResumen[];
};

export type RutinaEjercicioRequest = {
  idEjercicio: number;
  dia: string;
  orden: number;
  series?: number | null;
  repeticiones?: number | null;
  duracionMinutos?: number | null;
  descansoSegundos?: number | null;
  notas?: string | null;
};

export type CrearRutinaPayload = {
  idUsuario: number;
  nombre: string;
  descripcion: string;
  objetivo: string;
  dificultad: string;
  esPublica: boolean;
  ejercicios: RutinaEjercicioRequest[];
};

export type EditarRutinaPayload = {
  idUsuario: number;
  nombre?: string;
  descripcion?: string;
  objetivo?: string;
  dificultad?: string;
  esPublica?: boolean;
  ejercicios: RutinaEjercicioRequest[];
};

export type CopiarRutinaPayload = {
  idUsuario: number;
  activarRutina: boolean;
};

export type CrearReservaPayload = {
  idUsuario: number;
  idMaquina: number;
  fechaReserva: string;
  horaInicio: string;
  horaFin: string;
};

export type CancelarReservaPayload = {
  idUsuario: number;
};

export type Reserva = {
  idReserva: number;
  idUsuario: number;
  idMaquina: number;
  nombreMaquina: string;
  fechaReserva: string;
  horaInicio: string;
  horaFin: string;
  estado: string;
  fechaCreacion: string;
  fechaActualizacion: string;
};

export type IniciarSesionPayload = {
  idUsuario: number;
  idRutina?: number | null;
  notas?: string | null;
};

export type SesionEntrenamiento = {
  idSesion: number;
  idUsuario: number;
  idRutina?: number | null;
  fechaInicio: string;
  fechaFin?: string | null;
  estado: string;
  notas?: string | null;
};

export type AgregarDetalleSesionPayload = {
  idEjercicio: number;
  idMaquina?: number | null;
  idReserva?: number | null;
  seriesRealizadas?: number | null;
  repeticionesRealizadas?: number | null;
  pesoUsadoKg?: number | null;
  duracionMinutos?: number | null;
  esfuerzoPercibido?: number | null;
  notas?: string | null;
};

export type DetalleSesionEntrenamiento = {
  idDetalle: number;
  idSesion: number;
  idEjercicio: number;
  nombreEjercicio: string;
  idMaquina?: number | null;
  nombreMaquina?: string | null;
  idReserva?: number | null;
  seriesRealizadas?: number | null;
  repeticionesRealizadas?: number | null;
  pesoUsadoKg?: number | null;
  duracionMinutos?: number | null;
  esfuerzoPercibido?: number | null;
  notas?: string | null;
  fechaCreacion: string;
};

export type CompletarSesionPayload = {
  porcentajeCompletado: number;
  caloriasEstimadas?: number | null;
  tiempoTotalMinutos?: number | null;
  observacion?: string | null;
  notas?: string | null;
};

export type Progreso = {
  idProgreso: number;
  idUsuario: number;
  idRutina?: number | null;
  fecha: string;
  porcentajeCompletado: number;
  caloriasEstimadas?: number | null;
  tiempoTotalMinutos?: number | null;
  observacion?: string | null;
};

export type SesionHistorial = {
  sesion: SesionEntrenamiento;
  rutina?: RutinaResumen | null;
  progreso?: Progreso | null;
  detalles: DetalleSesionEntrenamiento[];
};
