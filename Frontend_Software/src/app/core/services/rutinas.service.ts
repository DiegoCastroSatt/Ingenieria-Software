import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { enviroment } from '../../../enviroments/enviroment';
import { CopiarRutinaPayload, CrearRutinaPayload, EditarRutinaPayload, RutinaDetalle, RutinaResumen } from '../models/rutina.models';

@Injectable({ providedIn: 'root' })
export class RutinasService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${enviroment.apiUrl}/rutinas`;
  getPredefinidas(): Observable<RutinaResumen[]> { return this.http.get<RutinaResumen[]>(`${this.apiUrl}/predefinidas`); }
  getPorUsuario(idUsuario: number): Observable<RutinaResumen[]> { return this.http.get<RutinaResumen[]>(`${this.apiUrl}/usuario/${idUsuario}`); }
  getDetalle(idRutina: number): Observable<RutinaDetalle> { return this.http.get<RutinaDetalle>(`${this.apiUrl}/${idRutina}`); }
  crear(payload: CrearRutinaPayload): Observable<RutinaDetalle> { return this.http.post<RutinaDetalle>(`${this.apiUrl}/personalizadas`, payload); }
  editar(idRutina: number, payload: EditarRutinaPayload): Observable<RutinaDetalle> { return this.http.put<RutinaDetalle>(`${this.apiUrl}/${idRutina}`, payload); }
  copiar(idRutina: number, payload: CopiarRutinaPayload): Observable<RutinaDetalle> { return this.http.post<RutinaDetalle>(`${this.apiUrl}/${idRutina}/copiar`, payload); }
  eliminar(idRutina: number, idUsuario: number): Observable<void> { return this.http.delete<void>(`${this.apiUrl}/${idRutina}`, { params: { idUsuario } }); }
}
