import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { formatApiError } from '../../core/utils/api-error';

@Component({ selector: 'app-auth-dialogs', standalone: true, imports: [FormsModule], templateUrl: './auth-dialogs.html' })
export class AuthDialogs {
  private readonly authService = inject(AuthService);
  readonly mensaje = output<string>();
  protected readonly loginAbierto = signal(false);
  protected readonly registroAbierto = signal(false);
  protected readonly errorLogin = signal('');
  protected readonly errorRegistro = signal('');
  protected readonly loginCargando = signal(false);
  protected readonly registroCargando = signal(false);
  protected loginForm = { nombre: '', password: '' };
  protected registerForm = { nombre: '', rut: '', correo: '', edad: '', password: '' };

  openLogin(): void {
    this.registroAbierto.set(false);
    this.errorLogin.set('');
    this.loginAbierto.set(true);
  }
  openRegister(): void {
    this.loginAbierto.set(false);
    this.errorRegistro.set('');
    this.registroAbierto.set(true);
  }
  protected cerrarLogin(): void { this.loginAbierto.set(false); this.loginCargando.set(false); }
  protected cerrarRegistro(): void { this.registroAbierto.set(false); this.registroCargando.set(false); }
  protected cerrarDesdeFondo(event: MouseEvent, dialogo: 'login' | 'registro'): void {
    if (event.target !== event.currentTarget) return;
    dialogo === 'login' ? this.cerrarLogin() : this.cerrarRegistro();
  }
  protected login(): void {
    const nombre = this.loginForm.nombre.trim();
    const password = this.loginForm.password.trim();
    if (!nombre || !password) return this.errorLogin.set('Por favor, completa ambos campos.');
    this.loginCargando.set(true);
    this.errorLogin.set('');
    this.authService.login(nombre, password).subscribe({
      next: (response) => { this.loginCargando.set(false); this.cerrarLogin(); this.mensaje.emit(`Sesion iniciada para ${response.user.nombre}.`); },
      error: (error: HttpErrorResponse) => { this.loginCargando.set(false); this.errorLogin.set(formatApiError(error, 'Usuario o contrasena incorrectos.')); }
    });
  }
  protected registrar(): void {
    const payload = {
      nombre: this.registerForm.nombre.trim(), rut: this.registerForm.rut.trim(), correo: this.registerForm.correo.trim(),
      edad: this.registerForm.edad.trim(), password: this.registerForm.password.trim()
    };
    if (!payload.nombre || !payload.rut || !payload.correo || !payload.password) return this.errorRegistro.set('Completa todos los campos para continuar.');
    this.registroCargando.set(true);
    this.errorRegistro.set('');
    this.authService.register(payload).subscribe({
      next: (response) => { this.registroCargando.set(false); this.cerrarRegistro(); this.mensaje.emit(`Usuario ${response.user.nombre} registrado correctamente.`); },
      error: (error: HttpErrorResponse) => { this.registroCargando.set(false); this.errorRegistro.set(formatApiError(error, 'No fue posible registrar al usuario.')); }
    });
  }
}
