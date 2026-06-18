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
Historias de Usuario y sus Criterios de Aceptación

## Codigos HTTP de la API

La API usa codigos HTTP para diferenciar caminos exitosos, errores de validacion, permisos y conflictos de negocio. El frontend muestra el codigo entre parentesis junto a un mensaje entendible para el usuario, sin exponer errores internos de base de datos ni stack traces.

### Codigos usados
- `200 OK`: consulta o accion exitosa que devuelve datos. Ejemplos: listar maquinas, iniciar sesion, completar sesion.
- `201 Created`: recurso creado correctamente. Ejemplos: crear reserva, crear rutina personalizada.
- `204 No Content`: accion exitosa sin cuerpo de respuesta. Ejemplos: eliminar rutina o metrica.
- `400 Bad Request`: datos invalidos o incompletos. Ejemplos: avatar invalido, peso de metrica menor o igual a cero, usuario obligatorio.
- `401 Unauthorized`: credenciales invalidas. Ejemplo: login con usuario o contrasena incorrectos.
- `403 Forbidden`: el usuario existe, pero no tiene permiso para la accion. Ejemplos: editar o eliminar una rutina que no le pertenece.
- `404 Not Found`: recurso inexistente. Ejemplos: usuario, maquina, reserva, rutina, sesion, ejercicio o metrica no encontrada.
- `409 Conflict`: una regla de negocio bloquea la operacion. Ejemplos: reserva duplicada, reserva no cancelable, sesion que no esta en progreso.
- `503 Service Unavailable`: un servicio requerido no esta disponible. Ejemplo: health check con base de datos no disponible.

### Ejemplos de mensajes al usuario
- Reserva con horario ocupado: `No se pudo crear la reserva. (409) Ya existe una reserva en ese horario.`
- Rutina ajena: `No se pudo eliminar la rutina. (403) Solo puedes eliminar rutinas propias.`
- Usuario inexistente: `No se pudo calcular el IMC. (404) Usuario no encontrado.`
- Avatar invalido: `No se pudo subir el avatar. (400) Formato no permitido. Usa JPG, PNG, GIF o WEBP.`
- Servicio no disponible: `No se pudo conectar con el servidor. (503) Intenta nuevamente mas tarde.`

Los errores internos se sanitizan antes de mostrarse. No se deben mostrar nombres de tablas, mensajes SQL, rutas internas del servidor ni trazas de ejecucion al usuario final.

## Historias de Usuario y Criterios de Aceptacion
1. Como usuario, quiero poder acceder a mi progreso a lo largo del tiempo.

a) Dado que el usuario ha iniciado sesión pero no posee registros, cuando el usuario accede a la sección de progreso, entonces el sistema debe informar que aún no existen datos disponibles.

b) Dado que el usuario ha iniciado sesión y posee registros, cuando el usuario accede a la sección de progreso, entonces el sistema debe informar que existen datos disponibles.

2. Como usuario, quiero poder acceder a mis métricas de desempeño.

a) Dado que el usuario ha iniciado sesión y posee métricas registradas, cuando el usuario accede a la sección de métricas, entonces el sistema debe mostrar sus métricas de desempeño actualizadas.

b) Dado que el usuario ha iniciado sesión y no posee métricas registradas, cuando el usuario accede a la sección de métricas, entonces el sistema debe mostrar un mensaje indicando que no hay métricas registradas.

3. Como usuario, quiero poder tener control de mis datos personales y de contacto.

a) Dado que el usuario ha iniciado sesión, cuando modifica su información personal y guarda los cambios, entonces el sistema debe actualizar correctamente los datos ingresados.

4. Como usuario, quiero añadir máquinas a favoritos para ahorrar tiempo al realizar reservas.

a) Dado que el usuario ha iniciado sesión y visualiza una máquina disponible, cuando selecciona la opción “Añadir a favoritos”, entonces el sistema debe agregarla a su lista de favoritos.

