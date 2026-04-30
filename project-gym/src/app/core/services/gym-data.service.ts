import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { enviroment } from '../../../enviroments/enviroment';
import { AuthResponse, HealthResponse, RegisterPayload } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class GymService {
  private readonly apiUrl = enviroment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  login(nombre: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/login`, { nombre, password });
  }

  registro(usuario: RegisterPayload): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/register`, usuario);
  }

  health(): Observable<HealthResponse> {
    return this.http.get<HealthResponse>(`${this.apiUrl}/health`);
  }
}
