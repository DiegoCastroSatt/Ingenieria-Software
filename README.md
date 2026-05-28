## Vision de Producto

Hola, nosotros en Software somos un equipo enfocado en una aplicación web llamada Gym_App para personas que quieren mantener un estilo de vida más saludable y necesitan dar seguimiento a sus rutinas.
Es muy común que los usuarios sigan su rutina, pero se encuentren con equipos ocupados o fuera de servicio, afectando su experiencia.
Por eso, estamos desarrollando Gym_App, una aplicación web que permite a los usuarios reservar máquinas, gestionar sus horarios y organizar sus rutinas de forma anticipada.
Con esto, logramos optimizar el uso del gimnasio, mejorar la experiencia del cliente y fomentar una rutina más ordenada y eficiente.

## Epics
1. Seguimiento de progreso
2. Reserva de maquinas
3. Gestión de usuarios
4. Gestión de rutinas

## Funcionalidades que tiene o que tendrá la aplicación:

### Autenticación y Gestión de Usuarios
- Registro e inicio de sesión seguro de usuarios
- Hash de contraseñas con encriptación segura
- Gestión de perfiles de usuario
- Control de datos personales y contacto

### Perfil y Salud
- Visualización y actualización del perfil del usuario
- Cálculo automático del Índice de Masa Corporal (IMC)
- Seguimiento de progreso personal
- Registro de histórico de sesiones de entrenamiento

### Catálogo de Máquinas
- Visualización del catálogo de equipos disponibles en el gimnasio
- Información detallada de cada máquina (nombre, descripción, área)

### Reserva de Máquinas
- Reserva de máquinas con disponibilidad de horarios
- Visualización de máquinas disponibles
- Cancelación de reservas
- Políticas automáticas de reserva

### Gestión de Rutinas
- Crear rutinas personalizadas
- Copiar rutinas existentes para reutilización
- Asignación de máquinas a ejercicios en las rutinas
- Seguimiento de rutinas activas

### Sesiones de Entrenamiento
- Registro de sesiones de entrenamiento completadas
- Seguimiento de ejercicios realizados por sesión
- Histórico de sesiones

### Seguimiento de Progreso
- Visualización del progreso del usuario a lo largo del tiempo
- Métricas de desempeño en rutinas
- Historial de sesiones completadas

### Health Check
- Verificación del estado de la aplicación y servicios
- Monitoreo de conectividad con la base de datos

## Historias de Usuario y suz Criterios de Aceptación
1) Como usuario, quiero poder acceder a mi progreso a lo largo del tiempo.

a) Dado que el usuario ha iniciado sesión pero no posee registros, cuando el usuario accede a la sección de progreso, entonces el sistema debe informar que aún no existen datos disponibles.
b) Dado que el usuario ha iniciado sesión y posee registros, cuando el usuario accede a la sección de progreso, entonces el sistema informa que existen datos disponibles.

2) Como usuario, quiero poder acceder a mis metricas de desempeño.

a) Dado que el usuario ha iniciado sesión y posee metricas registradas, cuando el usuario accede a la sección de métricas, el sistema debe mostrar sus métricas de desempeño actualizadas.
b) Dado que el usuario ha iniciado sesión y no posee metricas registradas, cuando el usuario accede a la sección de métricas, el sistema debe mostrar un mensaje de "no hay métricas registras".

3) Como usuario, quiero poder tener control de mis datos personales y contacto.

a) Dado que el usuario ha iniciado sesión, cuando modifica su información personal y guarda los cambios, entoncces el sistema debe actualizar correctamente los datos ingresados.

4) Como usuario, quiero añadir maquinas a "favoritos" para ahorrar tiempo al realizar reservas.

a) Dado que el usuario ha iniciado sesión y visualiza una máquina disponible, cuando selecciona la opción “Añadir a favoritos”, entonces el sistema la agrega a su lista de favoritos.
Como usuario, quiero poder acceder a mis metricas de desempeño.

Como usuario, quiero poder tener control de mis datos personales y contacto.

Como usuario, quiero añadir maquinas a "favoritos" para ahorrar tiempo al realizar reservas.
