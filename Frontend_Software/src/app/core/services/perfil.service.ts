import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { enviroment } from '../../../enviroments/enviroment';
import { ActualizarInformacionPublicaPayload, ActualizarPerfilImcPayload, AvatarUploadResponse, HistorialImc, ImcRecommendationResponse, PerfilUsuario } from '../models/perfil.models';
import { RutinaResumen } from '../models/rutina.models';

@Injectable({ providedIn: 'root' })
export class PerfilService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${enviroment.apiUrl}/perfiles`;
  getPerfil(idUsuario: number): Observable<PerfilUsuario> { return this.http.get<PerfilUsuario>(`${this.apiUrl}/${idUsuario}`); }
  getHistorialImc(idUsuario: number): Observable<HistorialImc[]> { return this.http.get<HistorialImc[]>(`${this.apiUrl}/${idUsuario}/historial-imc`); }
  calcularImc(idUsuario: number, payload: ActualizarPerfilImcPayload): Observable<ImcRecommendationResponse> { return this.http.post<ImcRecommendationResponse>(`${this.apiUrl}/${idUsuario}/imc`, payload); }
  getRecomendaciones(idUsuario: number): Observable<RutinaResumen[]> { return this.http.get<RutinaResumen[]>(`${this.apiUrl}/${idUsuario}/recomendaciones`); }
  actualizarInformacionPublica(idUsuario: number, payload: ActualizarInformacionPublicaPayload): Observable<PerfilUsuario> { return this.http.put<PerfilUsuario>(`${this.apiUrl}/${idUsuario}/informacion-publica`, payload); }
  subirAvatar(idUsuario: number, file: File): Observable<AvatarUploadResponse> {
    const formData = new FormData();
    formData.append('avatar', file);
    return this.http.post<AvatarUploadResponse>(`${this.apiUrl}/${idUsuario}/avatar`, formData);
  }
}
