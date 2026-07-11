import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { enviroment } from '../../../enviroments/enviroment';
import { CancelarReservaPayload, CrearReservaPayload, Reserva } from '../models/reserva.models';

@Injectable({ providedIn: 'root' })
export class ReservasService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${enviroment.apiUrl}/reservas`;
  crearReserva(payload: CrearReservaPayload): Observable<Reserva> { return this.http.post<Reserva>(this.apiUrl, payload); }
  cancelarReserva(idReserva: number, payload: CancelarReservaPayload): Observable<Reserva> { return this.http.post<Reserva>(`${this.apiUrl}/${idReserva}/cancelar`, payload); }
  getReservasUsuario(idUsuario: number): Observable<Reserva[]> { return this.http.get<Reserva[]>(`${this.apiUrl}/usuario/${idUsuario}`); }
  getReservasActivas(fechaReserva: string): Observable<Reserva[]> { return this.http.get<Reserva[]>(`${this.apiUrl}/activas`, { params: { fechaReserva } }); }
}
