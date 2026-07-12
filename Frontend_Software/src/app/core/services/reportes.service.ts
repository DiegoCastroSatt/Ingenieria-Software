import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { enviroment } from '../../../enviroments/enviroment';
import { CrearReporteProblemaPayload, ReporteProblema } from '../models/reporte.models';

@Injectable({ providedIn: 'root' })
export class ReportesService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${enviroment.apiUrl}/reportes-problemas`;

  crear(payload: CrearReporteProblemaPayload): Observable<ReporteProblema> {
    return this.http.post<ReporteProblema>(this.apiUrl, payload);
  }

  getPorUsuario(idUsuario: number): Observable<ReporteProblema[]> {
    return this.http.get<ReporteProblema[]>(`${this.apiUrl}/usuario/${idUsuario}`);
  }

  getPorId(idReporte: number): Observable<ReporteProblema> {
    return this.http.get<ReporteProblema>(`${this.apiUrl}/${idReporte}`);
  }
}
