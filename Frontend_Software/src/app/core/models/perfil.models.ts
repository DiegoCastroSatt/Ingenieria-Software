import { RutinaResumen } from './rutina.models';
export type ActualizarPerfilImcPayload = {
  fechaNacimiento?: string | null; sexo?: string | null; alturaCm: number; pesoKg: number;
  objetivo?: string | null; nivelActividad?: string | null;
};
export type ActualizarInformacionPublicaPayload = {
  alias?: string | null; avatarUrl?: string | null; telefonoTrabajo?: string | null; emailTrabajo?: string | null;
  sitioPersonal?: string | null; twitter?: string | null;
};
export type AvatarUploadResponse = { avatarUrl: string };
export type PerfilUsuario = {
  idPerfil: number; idUsuario: number; fechaNacimiento?: string | null; sexo?: string | null; alturaCm?: number | null;
  pesoKg?: number | null; objetivo?: string | null; nivelActividad?: string | null; alias?: string | null;
  avatarUrl?: string | null; telefonoTrabajo?: string | null; emailTrabajo?: string | null; sitioPersonal?: string | null;
  twitter?: string | null; fechaActualizacion: string;
};
export type HistorialImc = {
  idImc: number; idUsuario: number; alturaCm: number; pesoKg: number; imc: number; categoriaImc: string; fechaRegistro: string;
};
export type ImcRecommendationResponse = {
  idUsuario: number; imc: number; categoriaImc: string; perfil: PerfilUsuario; rutinasRecomendadas: RutinaResumen[];
};
