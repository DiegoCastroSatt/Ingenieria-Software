import { Component, HostListener, inject, input, output, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthUser } from '../../core/models/auth.models';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  templateUrl: './app-header.html'
})
export class AppHeader {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly usuario = input<AuthUser | null>(null);
  readonly loginSolicitado = output<void>();
  readonly mensaje = output<string>();
  protected readonly menuAbierto = signal(false);
  protected readonly menuUsuarioAbierto = signal(false);

  @HostListener('document:click', ['$event'])
  protected cerrarMenusExternos(event: MouseEvent): void {
    const target = event.target as HTMLElement | null;
    if (!target?.closest('.menu-wrapper')) this.menuAbierto.set(false);
    if (!target?.closest('.user-menu-wrapper')) this.menuUsuarioAbierto.set(false);
  }

  protected alternarMenu(): void { this.menuAbierto.update((abierto) => !abierto); }
  protected alternarMenuUsuario(): void { this.menuUsuarioAbierto.update((abierto) => !abierto); }
  protected irAPerfil(): void { this.menuUsuarioAbierto.set(false); void this.router.navigate(['/perfil']); }
  protected irAProgreso(): void { this.menuUsuarioAbierto.set(false); void this.router.navigate(['/progreso']); }
  protected cerrarSesion(): void {
    this.authService.logout();
    this.menuUsuarioAbierto.set(false);
    this.mensaje.emit('Sesion cerrada.');
  }
}
