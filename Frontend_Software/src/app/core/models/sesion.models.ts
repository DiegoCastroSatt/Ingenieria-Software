import { RutinaResumen } from './rutina.models';
export type IniciarSesionPayload = { idUsuario: number; idRutina?: number | null; notas?: string | null };
export type SesionEntrenamiento = {
  idSesion: number; idUsuario: number; idRutina?: number | null; fechaInicio: string; fechaFin?: string | null;
  estado: string; notas?: string | null;
};
export type AgregarDetalleSesionPayload = {
  idEjercicio: number; idMaquina?: number | null; idReserva?: number | null; seriesRealizadas?: number | null;
  repeticionesRealizadas?: number | null; pesoUsadoKg?: number | null; duracionMinutos?: number | null;
  esfuerzoPercibido?: number | null; notas?: string | null;
};
export type DetalleSesionEntrenamiento = {
  idDetalle: number; idSesion: number; idEjercicio: number; nombreEjercicio: string; idMaquina?: number | null;
  nombreMaquina?: string | null; idReserva?: number | null; seriesRealizadas?: number | null;
  repeticionesRealizadas?: number | null; pesoUsadoKg?: number | null; duracionMinutos?: number | null;
  esfuerzoPercibido?: number | null; notas?: string | null; fechaCreacion: string;
};
export type CompletarSesionPayload = {
  porcentajeCompletado: number; caloriasEstimadas?: number | null; tiempoTotalMinutos?: number | null;
  observacion?: string | null; notas?: string | null;
};
export type Progreso = {
  idProgreso: number; idUsuario: number; idRutina?: number | null; fecha: string; porcentajeCompletado: number;
  caloriasEstimadas?: number | null; tiempoTotalMinutos?: number | null; observacion?: string | null;
};
export type SesionHistorial = {
  sesion: SesionEntrenamiento; rutina?: RutinaResumen | null; progreso?: Progreso | null; detalles: DetalleSesionEntrenamiento[];
};
