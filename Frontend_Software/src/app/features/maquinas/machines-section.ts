import { Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthUser } from '../../core/models/auth.models';
import { Maquina } from '../../core/models/catalogo.models';
import { CatalogoService } from '../../core/services/catalogo.service';
import { formatApiError } from '../../core/utils/api-error';
import { MachineCard } from './machine-card';

@Component({
  selector: 'app-machines-section',
  standalone: true,
  imports: [FormsModule, MachineCard],
  templateUrl: './machines-section.html'
})
export class MachinesSection {
  private readonly catalogoService = inject(CatalogoService);

  readonly maquinas = input.required<Maquina[]>();
  readonly usuario = input<AuthUser | null>(null);
  readonly mensaje = output<string>();

  protected readonly favoritas = signal<Maquina[]>([]);
  protected readonly favoritasAbiertas = signal(true);
  protected readonly busqueda = signal('');
  protected readonly categoria = signal('');
  protected readonly ubicacion = signal('');
  protected readonly musculo = signal('');
  protected readonly estado = signal('');

  protected readonly categorias = computed(() => this.unicos(this.maquinas().map((maquina) => maquina.tipoMaquina)));
  protected readonly ubicaciones = computed(() => this.unicos(this.maquinas().map((maquina) => maquina.ubicacion)));
  protected readonly gruposMusculares = computed(() =>
    this.unicos(this.maquinas().flatMap((maquina) => this.separarMusculos(maquina.musculosObjetivo)))
  );
  protected readonly hayFiltros = computed(() =>
    !!this.busqueda().trim() || !!this.categoria() || !!this.ubicacion() || !!this.musculo() || !!this.estado()
  );
  protected readonly maquinasFiltradas = computed(() => {
    const busqueda = this.normalizar(this.busqueda());
    const musculo = this.normalizar(this.musculo());

    return this.maquinas().filter((maquina) => {
      if (this.categoria() && maquina.tipoMaquina !== this.categoria()) return false;
      if (this.ubicacion() && maquina.ubicacion !== this.ubicacion()) return false;
      if (this.estado() && maquina.estado !== this.estado()) return false;
      if (musculo && !this.normalizar(maquina.musculosObjetivo).includes(musculo)) return false;
      if (!busqueda) return true;

      return this.normalizar([
        maquina.nombre,
        maquina.descripcion,
        maquina.tipoMaquina,
        maquina.ubicacion,
        maquina.musculosObjetivo,
        maquina.estado
      ].join(' ')).includes(busqueda);
    });
  });

  constructor() {
    effect(() => {
      const usuario = this.usuario();
      if (!usuario) {
        this.favoritas.set([]);
        return;
      }
      this.cargarFavoritas(usuario.id);
    });
  }

  protected esFavorita(idMaquina: number): boolean {
    return this.favoritas().some((maquina) => maquina.idMaquina === idMaquina);
  }

  protected alternarFavoritas(): void {
    this.favoritasAbiertas.update((abiertas) => !abiertas);
  }

  protected limpiarFiltros(): void {
    this.busqueda.set('');
    this.categoria.set('');
    this.ubicacion.set('');
    this.musculo.set('');
    this.estado.set('');
  }

  protected cambiarFavorita(maquina: Maquina): void {
    const usuario = this.usuario();
    if (!usuario) {
      this.mensaje.emit('Debes iniciar sesion para guardar maquinas favoritas.');
      return;
    }

    const eraFavorita = this.esFavorita(maquina.idMaquina);
    const request = eraFavorita
      ? this.catalogoService.removeMaquinaFavorita(maquina.idMaquina, usuario.id)
      : this.catalogoService.addMaquinaFavorita(maquina.idMaquina, { idUsuario: usuario.id });

    request.subscribe({
      next: (favoritas) => {
        this.favoritas.set(favoritas);
        this.mensaje.emit(`${maquina.nombre} ${eraFavorita ? 'quitada de favoritas' : 'agregada a favoritas'}.`);
      },
      error: (error) => this.mensaje.emit(formatApiError(error, 'No se pudo actualizar la maquina favorita.'))
    });
  }

  private cargarFavoritas(idUsuario: number): void {
    this.catalogoService.getMaquinasFavoritas(idUsuario).subscribe({
      next: (favoritas) => this.favoritas.set(favoritas),
      error: () => this.favoritas.set([])
    });
  }

  private separarMusculos(value: string): string[] {
    return value.split(/[(),/]/).map((item) => item.trim()).filter(Boolean);
  }

  private unicos(values: string[]): string[] {
    return [...new Set(values.map((value) => value.trim()).filter(Boolean))]
      .sort((a, b) => a.localeCompare(b));
  }

  private normalizar(value: string): string {
    return value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().trim();
  }
}
