export type CrearReporteProblemaPayload = {
  idUsuario: number;
  idMaquina: number | null;
  descripcion: string;
};

export type ReporteProblema = {
  idReporte: number;
  idUsuario: number;
  idMaquina: number | null;
  nombreMaquina: string | null;
  descripcion: string;
  fechaCreacion: string;
  estado: string;
  fechaActualizacion: string;
};
