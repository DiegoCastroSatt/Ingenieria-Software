import { isPlatformBrowser } from '@angular/common';
import { Component, OnInit, PLATFORM_ID, effect, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthUser } from '../../core/models/auth.models';
import { Maquina } from '../../core/models/catalogo.models';
import { Reserva } from '../../core/models/reserva.models';
import { ReservasService } from '../../core/services/reservas.service';
import { formatApiError } from '../../core/utils/api-error';

@Component({
  selector: 'app-reservations-section',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './reservations-section.html'
})
export class ReservationsSection implements OnInit {
  private readonly reservasService = inject(ReservasService);
  private readonly platformId = inject(PLATFORM_ID);

  readonly maquinas = input.required<Maquina[]>();
  readonly usuario = input<AuthUser | null>(null);
  readonly reservasCambiadas = output<Reserva[]>();
  readonly mensaje = output<string>();

  protected readonly reservas = signal<Reserva[]>([]);
  protected readonly reservasActivas = signal<Reserva[]>([]);
  protected readonly hoyIso = this.obtenerHoyIso();
  protected formulario = {
    idMaquina: 0,
    fechaReserva: this.hoyIso,
    horaInicio: '09:00',
    horaFin: '10:00'
  };

  constructor() {
    effect(() => {
      const maquinas = this.maquinas();
      if (!this.formulario.idMaquina && maquinas.length > 0) {
        this.formulario.idMaquina = maquinas[0].idMaquina;
      }
    });

    effect(() => {
      const usuario = this.usuario();
      if (usuario) {
        this.cargarReservas(usuario.id);
      } else {
        this.actualizarReservas([]);
      }
    });
  }

  ngOnInit(): void {
    this.cargarReservasActivas();
  }

  protected maquinaSeleccionadaDisponible(): boolean {
    return this.fechaPermitida() && this.horarioValido() && this.estadoSeleccionado() === 'disponible';
  }

  protected estadoMaquina(maquina: Maquina): string {
    if (maquina.estado === 'mantencion' || maquina.estado === 'fuera_servicio') return 'fuera_servicio';
    if (maquina.estado === 'ocupada' || this.tieneTraslape(maquina.idMaquina)) return 'reservado';
    return 'disponible';
  }

  protected etiquetaEstado(maquina: Maquina): string {
    const estado = this.estadoMaquina(maquina);
    if (estado === 'reservado') return 'Reservado';
    if (estado === 'fuera_servicio') return 'Fuera de Servicio';
    return 'Disponible';
  }

  protected textoBoton(): string {
    const estado = this.estadoSeleccionado();
    if (estado === 'reservado') return 'Reservado';
    if (estado === 'fuera_servicio') return 'Fuera de Servicio';
    if (estado === 'sin_seleccion') return 'Selecciona una maquina';
    if (estado === 'fecha_pasada') return 'Fecha no valida';
    if (!this.horarioValido()) return 'Horario no valido';
    return 'Reservar maquina';
  }

  protected mensajeEstado(): string {
    const estado = this.estadoSeleccionado();
    if (estado === 'reservado') return 'Reservado: esta maquina ya tiene una reserva activa que cruza con este horario.';
    if (estado === 'fuera_servicio') return 'Fuera de Servicio: esta maquina esta en mantencion o no esta operativa.';
    if (estado === 'fecha_pasada') return 'No se pueden crear reservas en fechas anteriores a hoy.';
    if (!this.horarioValido()) return 'El horario debe tener una hora de inicio menor que la hora de termino.';
    return 'Disponible para el horario seleccionado.';
  }

  protected cambiarFecha(): void {
    if (!this.fechaPermitida()) {
      this.reservasActivas.set([]);
      return;
    }
    this.cargarReservasActivas();
  }

  protected crearReserva(): void {
    const usuario = this.usuario();
    if (!usuario) return this.mensaje.emit('Debes iniciar sesion para reservar una maquina.');
    if (!this.fechaPermitida()) return this.mensaje.emit('No se pueden crear reservas en fechas anteriores a hoy.');
    if (!this.horarioValido()) return this.mensaje.emit('La hora de inicio debe ser menor que la hora de termino.');
    if (!this.maquinaSeleccionadaDisponible()) return this.mensaje.emit(this.mensajeEstado());

    this.reservasService.crearReserva({
      idUsuario: usuario.id,
      idMaquina: this.formulario.idMaquina,
      fechaReserva: this.formulario.fechaReserva,
      horaInicio: `${this.formulario.horaInicio}:00`,
      horaFin: `${this.formulario.horaFin}:00`
    }).subscribe({
      next: () => {
        this.mensaje.emit('Reserva creada correctamente.');
        this.cargarReservasActivas();
        this.cargarReservas(usuario.id);
      },
      error: (error) => this.mensaje.emit(formatApiError(error, 'No se pudo crear la reserva.'))
    });
  }

  protected puedeCancelar(reserva: Reserva): boolean {
    return reserva.estado === 'activa' && reserva.fechaReserva >= this.hoyIso;
  }

  protected cancelarReserva(reserva: Reserva): void {
    const usuario = this.usuario();
    if (!usuario) return this.mensaje.emit('Debes iniciar sesion para cancelar una reserva.');
    if (!this.puedeCancelar(reserva)) return this.mensaje.emit('Solo se pueden cancelar reservas activas desde hoy en adelante.');

    if (isPlatformBrowser(this.platformId) &&
        !window.confirm(`Estas seguro de que quieres cancelar la reserva de ${reserva.nombreMaquina}?`)) return;

    this.reservasService.cancelarReserva(reserva.idReserva, { idUsuario: usuario.id }).subscribe({
      next: (cancelada) => {
        this.actualizarReservas(this.reservas().map((item) => item.idReserva === cancelada.idReserva ? cancelada : item));
        this.mensaje.emit('Reserva cancelada.');
        this.cargarReservasActivas();
        this.cargarReservas(usuario.id);
      },
      error: (error) => this.mensaje.emit(formatApiError(error, 'No se pudo cancelar la reserva.'))
    });
  }

  private estadoSeleccionado(): string {
    if (!this.fechaPermitida()) return 'fecha_pasada';
    const maquina = this.maquinas().find((item) => item.idMaquina === this.formulario.idMaquina);
    return maquina ? this.estadoMaquina(maquina) : 'sin_seleccion';
  }

  private tieneTraslape(idMaquina: number): boolean {
    if (!this.horarioValido()) return false;
    const inicio = this.horaEnMinutos(this.formulario.horaInicio);
    const fin = this.horaEnMinutos(this.formulario.horaFin);
    return this.reservasActivas().some((reserva) =>
      reserva.idMaquina === idMaquina && reserva.estado === 'activa' &&
      reserva.fechaReserva === this.formulario.fechaReserva &&
      this.horaEnMinutos(reserva.horaInicio) < fin && this.horaEnMinutos(reserva.horaFin) > inicio
    );
  }

  private horarioValido(): boolean {
    return this.horaEnMinutos(this.formulario.horaInicio) < this.horaEnMinutos(this.formulario.horaFin);
  }

  private fechaPermitida(): boolean {
    return !this.formulario.fechaReserva || this.formulario.fechaReserva >= this.hoyIso;
  }

  private horaEnMinutos(value: string): number {
    const [horas, minutos] = value.split(':').map(Number);
    return Number.isFinite(horas) && Number.isFinite(minutos) ? horas * 60 + minutos : 0;
  }

  private obtenerHoyIso(): string {
    const hoy = new Date();
    return `${hoy.getFullYear()}-${String(hoy.getMonth() + 1).padStart(2, '0')}-${String(hoy.getDate()).padStart(2, '0')}`;
  }

  private cargarReservas(idUsuario: number): void {
    this.reservasService.getReservasUsuario(idUsuario).subscribe({
      next: (reservas) => this.actualizarReservas(reservas),
      error: () => this.actualizarReservas([])
    });
  }

  private cargarReservasActivas(): void {
    if (!this.formulario.fechaReserva || !this.fechaPermitida()) {
      this.reservasActivas.set([]);
      return;
    }
    this.reservasService.getReservasActivas(this.formulario.fechaReserva).subscribe({
      next: (reservas) => this.reservasActivas.set(reservas),
      error: () => this.reservasActivas.set([])
    });
  }

  private actualizarReservas(reservas: Reserva[]): void {
    this.reservas.set(reservas);
    this.reservasCambiadas.emit(reservas);
  }
}
