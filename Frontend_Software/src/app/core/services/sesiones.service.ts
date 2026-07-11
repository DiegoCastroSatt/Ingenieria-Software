import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { enviroment } from '../../../enviroments/enviroment';
import { AgregarDetalleSesionPayload, CompletarSesionPayload, DetalleSesionEntrenamiento, IniciarSesionPayload, SesionEntrenamiento, SesionHistorial } from '../models/sesion.models';

@Injectable({ providedIn: 'root' })
export class SesionesService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${enviroment.apiUrl}/sesiones`;
  iniciar(payload: IniciarSesionPayload): Observable<SesionEntrenamiento> { return this.http.post<SesionEntrenamiento>(this.apiUrl, payload); }
  agregarDetalle(idSesion: number, payload: AgregarDetalleSesionPayload): Observable<DetalleSesionEntrenamiento> { return this.http.post<DetalleSesionEntrenamiento>(`${this.apiUrl}/${idSesion}/detalles`, payload); }
  completar(idSesion: number, payload: CompletarSesionPayload): Observable<SesionEntrenamiento> { return this.http.post<SesionEntrenamiento>(`${this.apiUrl}/${idSesion}/completar`, payload); }
  getHistorial(idUsuario: number): Observable<SesionHistorial[]> { return this.http.get<SesionHistorial[]>(`${this.apiUrl}/usuario/${idUsuario}/historial`); }
}
