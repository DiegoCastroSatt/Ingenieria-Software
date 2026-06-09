import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { enviroment } from '../../../enviroments/enviroment';
import {
  ActualizarPerfilImcPayload,
  ActualizarInformacionPublicaPayload,
  AgregarDetalleSesionPayload,
  CancelarReservaPayload,
  CompletarSesionPayload,
  CopiarRutinaPayload,
  CrearReservaPayload,
  CrearRutinaPayload,
  DetalleSesionEntrenamiento,
  EditarRutinaPayload,
  Ejercicio,
  HealthResponse,
  HistorialImc,
  ImcRecommendationResponse,
  IniciarSesionPayload,
  Maquina,
  MaquinaFavoritaPayload,
  PerfilUsuario,
  Reserva,
  RutinaDetalle,
  RutinaResumen,
  SesionEntrenamiento,
  SesionHistorial
} from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class GymService {
  private readonly apiUrl = enviroment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  health(): Observable<HealthResponse> {
    return this.http.get<HealthResponse>(`${this.apiUrl}/health`);
  }

  getMaquinas(): Observable<Maquina[]> {
    return this.http.get<Maquina[]>(`${this.apiUrl}/catalogo/maquinas`);
  }

  getMaquinasFavoritas(idUsuario: number): Observable<Maquina[]> {
    return this.http.get<Maquina[]>(`${this.apiUrl}/catalogo/maquinas/favoritas/${idUsuario}`);
  }

  addMaquinaFavorita(idMaquina: number, payload: MaquinaFavoritaPayload): Observable<Maquina[]> {
    return this.http.post<Maquina[]>(`${this.apiUrl}/catalogo/maquinas/${idMaquina}/favorita`, payload);
  }

  removeMaquinaFavorita(idMaquina: number, idUsuario: number): Observable<Maquina[]> {
    return this.http.delete<Maquina[]>(`${this.apiUrl}/catalogo/maquinas/${idMaquina}/favorita`, {
      params: { idUsuario }
    });
  }

  getEjercicios(): Observable<Ejercicio[]> {
    return this.http.get<Ejercicio[]>(`${this.apiUrl}/catalogo/ejercicios`);
  }

  calcularImc(idUsuario: number, payload: ActualizarPerfilImcPayload): Observable<ImcRecommendationResponse> {
    return this.http.post<ImcRecommendationResponse>(`${this.apiUrl}/perfiles/${idUsuario}/imc`, payload);
  }

  getRecomendaciones(idUsuario: number): Observable<RutinaResumen[]> {
    return this.http.get<RutinaResumen[]>(`${this.apiUrl}/perfiles/${idUsuario}/recomendaciones`);
  }

  getPerfil(idUsuario: number): Observable<PerfilUsuario> {
    return this.http.get<PerfilUsuario>(`${this.apiUrl}/perfiles/${idUsuario}`);
  }

  getHistorialImc(idUsuario: number): Observable<HistorialImc[]> {
    return this.http.get<HistorialImc[]>(`${this.apiUrl}/perfiles/${idUsuario}/historial-imc`);
  }

  getRutinasPredefinidas(): Observable<RutinaResumen[]> {
    return this.http.get<RutinaResumen[]>(`${this.apiUrl}/rutinas/predefinidas`);
  }

  getRutinasUsuario(idUsuario: number): Observable<RutinaResumen[]> {
    return this.http.get<RutinaResumen[]>(`${this.apiUrl}/rutinas/usuario/${idUsuario}`);
  }

  getRutina(idRutina: number): Observable<RutinaDetalle> {
    return this.http.get<RutinaDetalle>(`${this.apiUrl}/rutinas/${idRutina}`);
  }

  crearRutina(payload: CrearRutinaPayload): Observable<RutinaDetalle> {
    return this.http.post<RutinaDetalle>(`${this.apiUrl}/rutinas/personalizadas`, payload);
  }

  editarRutina(idRutina: number, payload: EditarRutinaPayload): Observable<RutinaDetalle> {
    return this.http.put<RutinaDetalle>(`${this.apiUrl}/rutinas/${idRutina}`, payload);
  }

  copiarRutina(idRutina: number, payload: CopiarRutinaPayload): Observable<RutinaDetalle> {
    return this.http.post<RutinaDetalle>(`${this.apiUrl}/rutinas/${idRutina}/copiar`, payload);
  }

  eliminarRutina(idRutina: number, idUsuario: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/rutinas/${idRutina}`, {
      params: { idUsuario }
    });
  }

  crearReserva(payload: CrearReservaPayload): Observable<Reserva> {
    return this.http.post<Reserva>(`${this.apiUrl}/reservas`, payload);
  }

  cancelarReserva(idReserva: number, payload: CancelarReservaPayload): Observable<Reserva> {
    return this.http.post<Reserva>(`${this.apiUrl}/reservas/${idReserva}/cancelar`, payload);
  }

  getReservasUsuario(idUsuario: number): Observable<Reserva[]> {
    return this.http.get<Reserva[]>(`${this.apiUrl}/reservas/usuario/${idUsuario}`);
  }

  getReservasActivas(fechaReserva: string): Observable<Reserva[]> {
    return this.http.get<Reserva[]>(`${this.apiUrl}/reservas/activas`, {
      params: { fechaReserva }
    });
  }

  iniciarSesion(payload: IniciarSesionPayload): Observable<SesionEntrenamiento> {
    return this.http.post<SesionEntrenamiento>(`${this.apiUrl}/sesiones`, payload);
  }

  agregarDetalleSesion(idSesion: number, payload: AgregarDetalleSesionPayload): Observable<DetalleSesionEntrenamiento> {
    return this.http.post<DetalleSesionEntrenamiento>(`${this.apiUrl}/sesiones/${idSesion}/detalles`, payload);
  }

  completarSesion(idSesion: number, payload: CompletarSesionPayload): Observable<SesionEntrenamiento> {
    return this.http.post<SesionEntrenamiento>(`${this.apiUrl}/sesiones/${idSesion}/completar`, payload);
  }

  getHistorial(idUsuario: number): Observable<SesionHistorial[]> {
    return this.http.get<SesionHistorial[]>(`${this.apiUrl}/sesiones/usuario/${idUsuario}/historial`);
  }

  actualizarInformacionPublica(idUsuario: number, payload: ActualizarInformacionPublicaPayload): Observable<PerfilUsuario> {
    return this.http.put<PerfilUsuario>(`${this.apiUrl}/perfiles/${idUsuario}/informacion-publica`, payload);
  }
}
