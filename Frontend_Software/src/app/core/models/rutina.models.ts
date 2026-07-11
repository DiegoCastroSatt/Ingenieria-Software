export type RutinaResumen = {
  idRutina: number; idUsuario?: number | null; nombre: string; descripcion: string; tipoRutina: string;
  objetivo: string; categoriaImc?: string | null; dificultad: string; esPublica: boolean; idRutinaOrigen?: number | null;
};
export type RutinaEjercicio = {
  idRutinaEjercicio: number; idRutina: number; idEjercicio: number; dia: string; orden: number;
  series?: number | null; repeticiones?: number | null; duracionMinutos?: number | null; descansoSegundos?: number | null;
  notas?: string | null; nombreEjercicio: string; descripcionEjercicio: string; grupoMuscular: string;
};
export type RutinaDetalle = RutinaResumen & { ejercicios: RutinaEjercicio[] };
export type RutinaEjercicioRequest = {
  idEjercicio: number; dia: string; orden: number; series?: number | null; repeticiones?: number | null;
  duracionMinutos?: number | null; descansoSegundos?: number | null; notas?: string | null;
};
export type CrearRutinaPayload = {
  idUsuario: number; nombre: string; descripcion: string; objetivo: string; dificultad: string;
  esPublica: boolean; ejercicios: RutinaEjercicioRequest[];
};
export type EditarRutinaPayload = {
  idUsuario: number; nombre?: string; descripcion?: string; objetivo?: string; dificultad?: string;
  esPublica?: boolean; ejercicios: RutinaEjercicioRequest[];
};
export type CopiarRutinaPayload = { idUsuario: number; activarRutina: boolean };
