import { Component, input, output } from '@angular/core';
import { Maquina } from '../../core/models/catalogo.models';

@Component({
  selector: 'app-machine-card',
  standalone: true,
  templateUrl: './machine-card.html'
})
export class MachineCard {
  readonly maquina = input.required<Maquina>();
  readonly favorita = input(false);
  readonly favoritoCambiado = output<Maquina>();
}
