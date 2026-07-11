export type CrearReservaPayload = { idUsuario: number; idMaquina: number; fechaReserva: string; horaInicio: string; horaFin: string };
export type CancelarReservaPayload = { idUsuario: number };
export type Reserva = {
  idReserva: number; idUsuario: number; idMaquina: number; nombreMaquina: string; fechaReserva: string;
  horaInicio: string; horaFin: string; estado: string; fechaCreacion: string; fechaActualizacion: string;
};
