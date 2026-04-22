import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, HostListener, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GymService } from './gym-data.service';
import { HttpClient } from '@angular/common/http';
import { Router } from'@angular/router'

type RoutineExercise = {
  name: string;
  detail: string;
  image?: string;
};

type Routine = {
  icon: string;
  name: string;
  description: string;
  accent: string;
  exercises: RoutineExercise[];
};

type DailyExercise = {
  name: string;
  detail: string;
  sets: string;
  done: boolean;
};

type TooltipState = {
  visible: boolean;
  name: string;
  image: string;
  left: number;
  top: number;
};

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  /*user: any = null;
  form = {
    email: '',
    password: ''
  };
  constructor(private http: HttpClient, private router: Router) {}
  login() {
    this.loginLoading.set(true);
    this.loginError.set('');

    this.http.post<any>('https://localhost:5001/api/auth/login', this.form)
      .subscribe({
        next: (res) => {
          localStorage.setItem('user', JSON.stringify(res));
          this.user = res;

          this.loginLoading.set(false);
          this.cerrarLogin();
          this.router.navigate(['/home']);
        },
        error: (error) => {
          this.loginLoading.set(false);
          this.loginError.set(error?.error || 'Usuario o contraseña incorrectos.');
        }
      });
  } */
  private readonly gymService = inject(GymService);

  protected readonly title = signal('project-gym');
  protected readonly menuOpen = signal(false);
  protected readonly loginModalOpen = signal(false);
  protected readonly registerModalOpen = signal(false);
  protected readonly loginError = signal('');
  protected readonly registerError = signal('');
  protected readonly registerSuccess = signal('');
  protected readonly loginLoading = signal(false);
  protected readonly registerLoading = signal(false);
  protected readonly tooltip = signal<TooltipState>({
    visible: false,
    name: '',
    image: '',
    left: 0,
    top: 0
  });

  protected readonly todayExercises = signal<DailyExercise[]>([
    { name: 'Press de pecho', detail: 'Maquina', sets: '4 x 12', done: true },
    { name: 'Fondos asistidos', detail: 'Peso corporal', sets: '3 x 10', done: false },
    { name: 'Press inclinado', detail: 'Mancuernas', sets: '3 x 15', done: false }
  ]);

  protected readonly routines = signal<Routine[]>([
    {
      icon: 'FI',
      name: 'Fuerza',
      description: 'Entrenamiento de fuerza',
      accent: 'rgba(230,57,70,0.15)',
      exercises: [
        {
          name: 'Maquina Smith',
          detail: 'Trabajo guiado para fuerza',
          image:
            'https://image.made-in-china.com/202f0j00zSghrtckZsoO/Multi-Function-Sport-Commercial-Life-Fitness-Equipment-Exercise-Machine-Smith-Machine-Gym-Machine-for-Indoor-Home-Gym-Strength-Training.jpg'
        },
        {
          name: 'Prensa de piernas',
          detail: 'Empuje para tren inferior',
          image:
            'https://mundoentrenamiento.com/wp-content/uploads/2022/11/mujer-haciendo-prensa-para-piernas.jpg'
        },
        {
        name: 'Peso muerto',
        detail: 'Fuerza global del cuerpo'
      },
      {
        name: 'Sentadilla con barra',
        detail: 'Ejercicio base de fuerza'
      },
      ]
    },
    {
      icon: 'HI',
      name: 'Hipertrofia',
      description: 'Aumento de masa muscular',
      accent: 'rgba(56,189,248,0.12)',
      exercises: [
        {
          name: 'Maquina de poleas',
          detail: 'Trabajo de control y aislamiento',
          image:
            'https://www.maquinassanmartino.com/uploads/l_34_6002-a-fotos-poleas-800x600px2.jpg'
        },
        {
          name: 'Curl de biceps',
          detail: 'Brazos con mancuernas',
          image: 'https://sdmed.cl/wp-content/uploads/2022/10/2-9-600x600.jpg'
        },
        {
          name: 'Press de pecho con mancuernas',
          detail: 'Desarrollo del pectoral'
        },
        {
          name: 'Elevaciones laterales',
          detail: 'Aislamiento de hombros'
        }
      ]
    },
    {
      icon: 'CA',
      name: 'Cardio',
      description: 'Resistencia cardiovascular',
      accent: 'rgba(34,197,94,0.12)',
      exercises: [
      {
        name: 'Paralelas',
        detail: 'Trabajo funcional y resistencia'
      },
      {
        name: 'Circuito HIIT',
        detail: 'Intervalos de alta intensidad'
      },
      {
        name: 'Trote en cinta',
        detail: 'Cardio continuo moderado'
      },
      {
        name: 'Bicicleta estática',
        detail: 'Resistencia de bajo impacto'
      }
    ]
    },
    {
      icon: 'MA',
      name: 'Motor Angular',
      description: 'Desarrollo de tren inferior',
      accent: 'rgba(168,85,247,0.12)',
      exercises: [
        {
          name: 'Prensa de piernas',
          detail: 'Trabajo de cuádriceps y glúteos'
        },
        {
          name: 'Extensión de cuádriceps',
          detail: 'Aislamiento de cuádriceps'
        },
        {
          name: 'Curl femoral',
          detail: 'Trabajo de isquiotibiales'
        },
        {
          name: 'Gemelos en máquina',
          detail: 'Desarrollo de pantorrillas'
        }
      ]
    },
    {
    icon: 'FU',
    name: 'Funcional',
    description: 'Entrenamiento de movimientos completos',
    accent: 'rgba(251,146,60,0.15)',
    exercises: [
      {
        name: 'Burpees',
        detail: 'Ejercicio completo de cuerpo'
      },
      {
        name: 'Kettlebell swing',
        detail: 'Potencia de cadera y entrena glúteos y isquiotibiales.'
      },
      {
        name: 'Saltos al cajón',
        detail: 'Desarrollan potencia explosiva, fuerza en el tren inferior.'
      },
      {
        name: 'Plancha dinámica',
        detail: 'Trabajo abdominal, oblicuos y espalda baja'
      }
    ]
  },
  {
    icon: 'CO',
    name: 'Core',
    description: 'Fortalecimiento abdominal y lumbar',
    accent: 'rgba(99,102,241,0.15)',
    exercises: [
      {
        name: 'Crunch abdominal',
        detail: 'Trabajo básico de abdomen'
      },
      {
        name: 'Elevaciones de piernas',
        detail: 'Abdomen inferior'
      },
      {
        name: 'Plancha frontal',
        detail: 'Estabilidad central'
      },
      {
        name: 'Russian twist',
        detail: 'Trabajo de oblicuos'
      }
    ]
  }
  ]);

  protected readonly expandedRoutines = signal<Record<number, boolean>>({});

  protected loginForm = {
    nombre: '',
    password: ''
  };

  protected registerForm = {
    nombre: '',
    rut: '',
    correo: '',
    password: ''
  };

  @HostListener('document:click', ['$event'])
  protected handleDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement | null;

    if (!target?.closest('.menu-wrapper')) {
      this.menuOpen.set(false);
    }

    if (target?.classList.contains('modal-overlay')) {
      if (target.classList.contains('login-overlay')) {
        this.cerrarLogin();
      }

      if (target.classList.contains('register-overlay')) {
        this.cerrarRegistro();
      }
    }
  }

  protected scrollToRutinas(): void {
    document.getElementById('rutinas')?.scrollIntoView({ behavior: 'smooth' });
  }

  protected toggleMenu(): void {
    this.menuOpen.update((value) => !value);
  }

  protected toggleRoutine(index: number): void {
    this.expandedRoutines.update((current) => ({
      ...current,
      [index]: !current[index]
    }));
  }

  protected isRoutineOpen(index: number): boolean {
    return !!this.expandedRoutines()[index];
  }

  protected toggleExercise(index: number): void {
    this.todayExercises.update((items) =>
      items.map((item, itemIndex) =>
        itemIndex === index ? { ...item, done: !item.done } : item
      )
    );
  }

  protected abrirLogin(): void {
    this.menuOpen.set(false);
    this.registerModalOpen.set(false);
    this.loginError.set('');
    this.loginModalOpen.set(true);
  }

  protected cerrarLogin(): void {
    this.loginModalOpen.set(false);
    this.loginLoading.set(false);
  }

  protected abrirRegistro(): void {
    this.menuOpen.set(false);
    this.loginModalOpen.set(false);
    this.registerError.set('');
    this.registerSuccess.set('');
    this.registerModalOpen.set(true);
  }

  protected cerrarRegistro(): void {
    this.registerModalOpen.set(false);
    this.registerLoading.set(false);
  }

  protected intentarLogin(): void {
    const nombre = this.loginForm.nombre.trim();
    const password = this.loginForm.password.trim();

    if (!nombre || !password) {
      this.loginError.set('Por favor, completa ambos campos.');
      return;
    }
  
    this.loginLoading.set(true);
    this.loginError.set('');

    this.gymService.login(nombre, password).subscribe({
      next: (response) => {
        this.loginLoading.set(false);
        this.cerrarLogin();
        window.alert(`Conectado. El servidor responde: ${response}`);
      },
      error: (error: HttpErrorResponse) => {
        this.loginLoading.set(false);
        this.loginError.set(error?.error || 'Usuario o contrasena incorrectos.');
      }
    });
  }

  protected registrarUsuario(): void {
    const payload = {
      nombre: this.registerForm.nombre.trim(),
      rut: this.registerForm.rut.trim(),
      correo: this.registerForm.correo.trim(),
      password: this.registerForm.password.trim()
    };

    if (!payload.nombre || !payload.rut || !payload.correo || !payload.password) {
      this.registerError.set('Completa todos los campos para continuar.');
      this.registerSuccess.set('');
      return;
    }

    this.registerLoading.set(true);
    this.registerError.set('');
    this.registerSuccess.set('');

    this.gymService.registro(payload).subscribe({
      next: (response) => {
        this.registerLoading.set(false);
        this.registerSuccess.set(response);
        this.registerForm = {
          nombre: '',
          rut: '',
          correo: '',
          password: ''
        };
      },
      error: (error: HttpErrorResponse) => {
        this.registerLoading.set(false);
        this.registerError.set(error?.error || 'No fue posible registrar al usuario.');
      }
    });
  }

  protected showTooltip(event: MouseEvent, exercise: RoutineExercise): void {
    if (!exercise.image) {
      return;
    }

    this.tooltip.set({
      visible: true,
      name: exercise.name,
      image: exercise.image,
      ...this.calculateTooltipPosition(event)
    });
  }

  protected moveTooltip(event: MouseEvent): void {
    const current = this.tooltip();

    if (!current.visible) {
      return;
    }

    this.tooltip.set({
      ...current,
      ...this.calculateTooltipPosition(event)
    });
  }

  protected hideTooltip(): void {
    this.tooltip.update((current) => ({ ...current, visible: false }));
  }

  private calculateTooltipPosition(event: MouseEvent): { left: number; top: number } {
    const tooltipWidth = 220;
    const tooltipHeight = 190;

    let left = event.clientX + 16;
    let top = event.clientY + 16;

    if (left + tooltipWidth > window.innerWidth - 8) {
      left = event.clientX - tooltipWidth - 12;
    }

    if (top + tooltipHeight > window.innerHeight - 8) {
      top = event.clientY - tooltipHeight - 12;
    }

    return { left, top };
  }
}