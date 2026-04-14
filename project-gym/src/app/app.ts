import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App {
  protected readonly title = signal('project-gym');

  toggleMenu() {
    const btn = document.getElementById('menuBtn');
    const dd = document.getElementById('dropdown');
    btn?.classList.toggle('open');
    dd?.classList.toggle('show');
  }

  toggleRoutine(header: HTMLElement) {
    const body = header.nextElementSibling as HTMLElement;
    const arrow = header.querySelector('.routine-arrow') as HTMLElement;

    body.classList.toggle('open');
    if (arrow) {
      arrow.style.transform = body.classList.contains('open')
        ? 'rotate(90deg)'
        : '';
    }
  }

  toggleEx(el: HTMLElement) {
    el.classList.toggle('done');
    el.textContent = el.classList.contains('done') ? '✓' : '';
  }

  abrirLogin() {
    const modal = document.getElementById('loginModal');
    const err = document.getElementById('loginError');

    modal?.classList.add('show');
    if (err) err.style.display = 'none';
  }

  cerrarLogin() {
    document.getElementById('loginModal')?.classList.remove('show');
  }

  abrirRegistro() {
    document.getElementById('registroModal')?.classList.add('show');
  }

  cerrarRegistro() {
    document.getElementById('registroModal')?.classList.remove('show');
  }

  intentarLogin() {
    const u = (document.getElementById('inputUsuario') as HTMLInputElement)?.value.trim();
    const p = (document.getElementById('inputPass') as HTMLInputElement)?.value.trim();

    if (u && p) {
      this.cerrarLogin();
      alert('¡Bienvenido, ' + u + '!');
    } else {
      const err = document.getElementById('loginError');
      if (err) err.style.display = 'block';
    }
  }
}