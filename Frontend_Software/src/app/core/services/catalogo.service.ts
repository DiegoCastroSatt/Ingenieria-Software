import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { enviroment } from '../../../enviroments/enviroment';
import { Ejercicio, Maquina, MaquinaFavoritaPayload } from '../models/catalogo.models';

@Injectable({ providedIn: 'root' })
export class CatalogoService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${enviroment.apiUrl}/catalogo`;

  getMaquinas(): Observable<Maquina[]> { return this.http.get<Maquina[]>(`${this.apiUrl}/maquinas`); }
  getEjercicios(): Observable<Ejercicio[]> { return this.http.get<Ejercicio[]>(`${this.apiUrl}/ejercicios`); }
  getMaquinasFavoritas(idUsuario: number): Observable<Maquina[]> { return this.http.get<Maquina[]>(`${this.apiUrl}/maquinas/favoritas/${idUsuario}`); }
  addMaquinaFavorita(idMaquina: number, payload: MaquinaFavoritaPayload): Observable<Maquina[]> {
    return this.http.post<Maquina[]>(`${this.apiUrl}/maquinas/${idMaquina}/favorita`, payload);
  }
  removeMaquinaFavorita(idMaquina: number, idUsuario: number): Observable<Maquina[]> {
    return this.http.delete<Maquina[]>(`${this.apiUrl}/maquinas/${idMaquina}/favorita`, { params: { idUsuario } });
  }
}
