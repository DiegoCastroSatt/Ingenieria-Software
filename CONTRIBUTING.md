## DoR
### Progreso de usuario
Como usuario, quiero poder acceder a mi progreso a lo largo del tiempo.
Dado que el usuario ha iniciado sesión pero no posee registros, cuando el usuario accede a la sección de progreso, entonces el sistema debe informar que aún no existen datos disponibles.
Dado que el usuario ha iniciado sesión y posee registros, cuando el usuario accede a la sección de progreso, entonces el sistema debe informar que existen datos disponibles.
Estimación de esfuerzo: 8
Dependencias: 

### Metricas de desempeño
Como usuario, quiero poder acceder a mis métricas de desempeño.

a) Dado que el usuario ha iniciado sesión y posee métricas registradas, cuando el usuario accede a la sección de métricas, entonces el sistema debe mostrar sus métricas de desempeño actualizadas.

b) Dado que el usuario ha iniciado sesión y no posee métricas registradas, cuando el usuario accede a la sección de métricas, entonces el sistema debe mostrar un mensaje indicando que no hay métricas registradas.

### Datos Personales y de contacto
Como usuario, quiero poder tener control de mis datos personales y de contacto.

a) Dado que el usuario ha iniciado sesión, cuando modifica su información personal y guarda los cambios, entonces el sistema debe actualizar correctamente los datos ingresados.

## Maquinas a Favoritos
Como usuario, quiero añadir máquinas a favoritos para ahorrar tiempo al realizar reservas.

a) Dado que el usuario ha iniciado sesión y visualiza una máquina disponible, cuando selecciona la opción “Añadir a favoritos”, entonces el sistema debe agregarla a su lista de favoritos.
## Progreso de usuario

### Definition of Ready

* La historia de usuario está claramente definida.
* Se especifica qué información constituye el progreso del usuario.
* Existen criterios de aceptación definidos y aprobados por el equipo.
* Se conoce la fuente de datos desde donde se obtendrá el historial de progreso.
* Las dependencias con la base de datos y servicios asociados han sido identificadas.
* La historia ha sido estimada por el equipo de desarrollo.

---

## Métricas de desempeño


* La historia de usuario está claramente descrita.
* Se encuentran definidas las métricas que serán mostradas al usuario.
* Los criterios de aceptación han sido acordados por el equipo.
* Se dispone de acceso a los datos necesarios para calcular o recuperar las métricas.
* Se identificaron dependencias técnicas y funcionales.
* La historia ha sido estimada por el equipo de desarrollo.

### Datos personales y de contacto

* La historia de usuario está correctamente documentada.
* Se encuentran definidos los campos que el usuario podrá modificar.
* Existen reglas de validación para cada campo editable.
* Los criterios de aceptación han sido revisados y aprobados.
* Se identificaron las dependencias con autenticación y almacenamiento de usuarios.
* La historia ha sido estimada por el equipo de desarrollo.

### Máquinas a favoritos

* La historia de usuario está claramente definida.
* Se encuentra especificado el comportamiento esperado para agregar y eliminar favoritos.
* Los criterios de aceptación han sido acordados por el equipo.
* Existe una definición clara de la estructura de datos que almacenará los favoritos.
* Se identificaron las dependencias con el catálogo de máquinas y el sistema de usuarios.
* La historia ha sido estimada por el equipo de desarrollo.

## DoD