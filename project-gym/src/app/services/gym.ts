import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GymService {
  // Según tu Program.cs, las rutas van directo a la raíz del puerto 5000
  private apiUrl = 'http://localhost:5000'; 

  constructor(private http: HttpClient) { }

  // Conectamos con app.MapPost("/login"...)
  login(nombre: string, password: string): Observable<string> {
    const credenciales = { Nombre: nombre, Password: password };
    
    // Ojo aquí: Tu backend devuelve Results.Ok("Login correcto") que es texto plano, no JSON.
    // Por eso le decimos a Angular que espere un { responseType: 'text' }.
    return this.http.post(`${this.apiUrl}/login`, credenciales, { responseType: 'text' });
  }

  // Dejo preparada la función de registro para cuando la necesites
  registro(usuario: any): Observable<string> {
    return this.http.post(`${this.apiUrl}/register`, usuario, { responseType: 'text' });
  }
}