import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

type RegisterPayload = {
  nombre: string;
  rut: string;
  correo: string;
  password: string;
};

@Injectable({
  providedIn: 'root'
})
export class GymService {
  private readonly apiUrl = 'http://localhost:5000';

  constructor(private readonly http: HttpClient) {}

  login(nombre: string, password: string): Observable<string> {
    return this.http.post(`${this.apiUrl}/login`, { nombre, password }, { responseType: 'text' });
  }

  registro(usuario: RegisterPayload): Observable<string> {
    return this.http.post(`${this.apiUrl}/register`, usuario, { responseType: 'text' });
  }
}
