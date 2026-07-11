import { Component, effect, inject, input, output, signal } from '@angular/core';
import { AuthUser } from '../../core/models/auth.models';
import { SesionHistorial } from '../../core/models/sesion.models';
import { SesionesService } from '../../core/services/sesiones.service';

@Component({
  selector: 'app-history-section',
  standalone: true,
  templateUrl: './history-section.html'
})
export class HistorySection {
  private readonly sesionesService = inject(SesionesService);

  readonly usuario = input<AuthUser | null>(null);
  readonly actualizar = input(0);
  readonly historialCambiado = output<SesionHistorial[]>();
  readonly mensaje = output<string>();
  protected readonly historial = signal<SesionHistorial[]>([]);

  constructor() {
    effect(() => {
      this.actualizar();
      const usuario = this.usuario();
      if (usuario) this.cargar(usuario.id);
      else this.establecerHistorial([]);
    });
  }

  protected recargar(): void {
    const usuario = this.usuario();
    if (!usuario) return this.mensaje.emit('Debes iniciar sesion para ver el historial.');
    this.cargar(usuario.id);
  }

  private cargar(idUsuario: number): void {
    this.sesionesService.getHistorial(idUsuario).subscribe({
      next: (historial) => this.establecerHistorial(this.sinSesionesDuplicadas(historial)),
      error: () => this.establecerHistorial([])
    });
  }

  private establecerHistorial(historial: SesionHistorial[]): void {
    this.historial.set(historial);
    this.historialCambiado.emit(historial);
  }

  private sinSesionesDuplicadas(historial: SesionHistorial[]): SesionHistorial[] {
    const sesiones = new Map<number, SesionHistorial>();
    for (const item of historial) {
      if (!sesiones.has(item.sesion.idSesion)) sesiones.set(item.sesion.idSesion, item);
    }
    return [...sesiones.values()];
  }
}
