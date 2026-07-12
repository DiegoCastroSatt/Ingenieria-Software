import { Component, effect, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthUser } from '../../core/models/auth.models';
import { Maquina } from '../../core/models/catalogo.models';
import { ReporteProblema } from '../../core/models/reporte.models';
import { ReportesService } from '../../core/services/reportes.service';
import { formatApiError } from '../../core/utils/api-error';

@Component({
  selector: 'app-reportes-section',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './reportes-section.html',
  styleUrl: './reportes-section.css'
})
export class ReportesSection {
  private readonly reportesService = inject(ReportesService);

  readonly usuario = input<AuthUser | null>(null);
  readonly maquinas = input.required<Maquina[]>();
  readonly mensaje = output<string>();

  protected readonly reportes = signal<ReporteProblema[]>([]);
  protected readonly cargando = signal(false);
  protected readonly enviando = signal(false);
  protected readonly error = signal('');
  protected readonly exito = signal('');
  protected formulario: { idMaquina: number | null; descripcion: string } = {
    idMaquina: null,
    descripcion: ''
  };

  constructor() {
    effect(() => {
      const usuario = this.usuario();
      if (usuario) {
        this.cargarReportes(usuario.id);
      } else {
        this.reportes.set([]);
        this.reiniciarFormulario();
      }
    });
  }

  protected crearReporte(): void {
    const usuario = this.usuario();
    const descripcion = this.formulario.descripcion.trim();
    this.error.set('');
    this.exito.set('');

    if (!usuario) {
      this.error.set('Debes iniciar sesion para reportar un problema.');
      return;
    }

    if (!descripcion) {
      this.error.set('La descripcion del problema es obligatoria.');
      return;
    }

    if (descripcion.length > 500) {
      this.error.set('La descripcion no puede superar 500 caracteres.');
      return;
    }

    this.enviando.set(true);
    this.reportesService.crear({
      idUsuario: usuario.id,
      idMaquina: this.formulario.idMaquina,
      descripcion
    }).subscribe({
      next: (reporte) => {
        this.enviando.set(false);
        this.exito.set(`Reporte #${reporte.idReporte} enviado correctamente.`);
        this.mensaje.emit(this.exito());
        this.reiniciarFormulario();
        this.cargarReportes(usuario.id);
      },
      error: (responseError) => {
        this.enviando.set(false);
        this.error.set(formatApiError(responseError, 'No se pudo enviar el reporte.'));
      }
    });
  }

  private cargarReportes(idUsuario: number): void {
    this.cargando.set(true);
    this.reportesService.getPorUsuario(idUsuario).subscribe({
      next: (reportes) => {
        this.reportes.set(reportes);
        this.cargando.set(false);
      },
      error: (responseError) => {
        this.reportes.set([]);
        this.cargando.set(false);
        this.error.set(formatApiError(responseError, 'No se pudieron cargar los reportes.'));
      }
    });
  }

  private reiniciarFormulario(): void {
    this.formulario = { idMaquina: null, descripcion: '' };
  }
}
