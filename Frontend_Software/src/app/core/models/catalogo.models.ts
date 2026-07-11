export type Maquina = {
  idMaquina: number; idTipoMaquina: number; tipoMaquina: string; nombre: string; descripcion: string;
  musculosObjetivo: string; imagenUrl: string; ubicacion: string; estado: string; cantidad: number;
};
export type MaquinaFavoritaPayload = { idUsuario: number };
export type Ejercicio = {
  idEjercicio: number; nombre: string; descripcion: string; grupoMuscular: string; dificultad: string;
  idMaquina?: number | null; nombreMaquina?: string | null;
};
