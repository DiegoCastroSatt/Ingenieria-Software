import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { enviroment } from '../enviroments/enviroment';

export type RegisterPayload = {
  nombre: string;
  rut: string;
  correo: string;
  password: string;
};

export type AuthUser = {
  id: number;
  nombre: string;
  correo: string;
};

export type AuthResponse = {
  message: string;
  user: AuthUser;
};

export type HealthResponse = {
  status: string;
  message: string;
};

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
