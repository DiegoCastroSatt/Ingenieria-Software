import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { enviroment } from '../../../enviroments/enviroment';
import { CrearMetricaPayload, Metrica } from '../models/metrica.models';

@Injectable({ providedIn: 'root' })
export class MetricasService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${enviroment.apiUrl}/metricas`;
  getPorUsuario(idUsuario: number): Observable<Metrica[]> { return this.http.get<Metrica[]>(`${this.apiUrl}/usuario/${idUsuario}`); }
  crear(payload: CrearMetricaPayload): Observable<Metrica> { return this.http.post<Metrica>(this.apiUrl, payload); }
  eliminar(idMetrica: number, idUsuario: number): Observable<void> { return this.http.delete<void>(`${this.apiUrl}/${idMetrica}`, { params: { idUsuario } }); }
}
