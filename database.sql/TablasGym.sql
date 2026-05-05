CREATE DATABASE IF NOT EXISTS gym_db;
USE gym_db;

SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS detalle_sesion_entrenamiento;
DROP TABLE IF EXISTS sesiones_entrenamiento;
DROP TABLE IF EXISTS progreso;
DROP TABLE IF EXISTS usuario_rutina;
DROP TABLE IF EXISTS rutina_ejercicio;
DROP TABLE IF EXISTS historial_imc;
DROP TABLE IF EXISTS perfil_usuario;
DROP TABLE IF EXISTS reservas;
DROP TABLE IF EXISTS mantenimientos;
DROP TABLE IF EXISTS horarios;
DROP TABLE IF EXISTS ejercicios;
DROP TABLE IF EXISTS rutinas;
DROP TABLE IF EXISTS usuarios;
DROP TABLE IF EXISTS maquinas;
DROP TABLE IF EXISTS tipos_maquina;

SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE tipos_maquina (
    id_tipo_maquina INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE maquinas (
    id_maquina INT AUTO_INCREMENT PRIMARY KEY,
    id_tipo_maquina INT NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255) NOT NULL,
    ubicacion VARCHAR(100) NOT NULL,
    estado ENUM('disponible', 'ocupada', 'mantencion', 'fuera_servicio') NOT NULL DEFAULT 'disponible',
    cantidad INT NOT NULL DEFAULT 1,
    FOREIGN KEY (id_tipo_maquina) REFERENCES tipos_maquina(id_tipo_maquina)
);

CREATE TABLE usuarios (
    id_usuario INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    rut VARCHAR(20) NOT NULL UNIQUE,
    correo VARCHAR(100) NOT NULL UNIQUE,
    contrasena_hash VARCHAR(255) NOT NULL,
    rol ENUM('usuario', 'admin', 'entrenador') NOT NULL DEFAULT 'usuario',
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE perfil_usuario (
    id_perfil INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT NOT NULL UNIQUE,
    fecha_nacimiento DATE NULL,
    sexo VARCHAR(20) NULL,
    altura_cm DECIMAL(5,2) NULL,
    peso_kg DECIMAL(5,2) NULL,
    objetivo VARCHAR(100) NULL,
    nivel_actividad VARCHAR(50) NULL,
    fecha_actualizacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
);

CREATE TABLE historial_imc (
    id_imc INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT NOT NULL,
    altura_cm DECIMAL(5,2) NOT NULL,
    peso_kg DECIMAL(5,2) NOT NULL,
    imc DECIMAL(5,2) NOT NULL,
    categoria_imc ENUM('bajo_peso', 'normal', 'sobrepeso', 'obesidad') NOT NULL,
    fecha_registro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
);

CREATE TABLE rutinas (
    id_rutina INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT NULL,
    nombre VARCHAR(100) NOT NULL,
    descripcion TEXT NOT NULL,
    tipo_rutina ENUM('predefinida', 'personalizada', 'copiada') NOT NULL,
    objetivo VARCHAR(100) NOT NULL,
    categoria_imc ENUM('bajo_peso', 'normal', 'sobrepeso', 'obesidad') NULL,
    dificultad VARCHAR(50) NOT NULL,
    es_publica BOOLEAN NOT NULL DEFAULT TRUE,
    id_rutina_origen INT NULL,
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    FOREIGN KEY (id_rutina_origen) REFERENCES rutinas(id_rutina)
);

CREATE TABLE ejercicios (
    id_ejercicio INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(255) NOT NULL,
    grupo_muscular VARCHAR(50) NOT NULL,
    dificultad VARCHAR(50) NOT NULL,
    id_maquina INT NULL,
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (id_maquina) REFERENCES maquinas(id_maquina)
);

CREATE TABLE rutina_ejercicio (
    id_rutina_ejercicio INT AUTO_INCREMENT PRIMARY KEY,
    id_rutina INT NOT NULL,
    id_ejercicio INT NOT NULL,
    dia VARCHAR(20) NOT NULL,
    orden INT NOT NULL,
    series INT NULL,
    repeticiones INT NULL,
    duracion_minutos INT NULL,
    descanso_segundos INT NULL,
    notas VARCHAR(255) NULL,
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (id_rutina) REFERENCES rutinas(id_rutina),
    FOREIGN KEY (id_ejercicio) REFERENCES ejercicios(id_ejercicio)
);

CREATE TABLE usuario_rutina (
    id_usuario_rutina INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_rutina INT NOT NULL,
    estado ENUM('activa', 'pausada', 'finalizada') NOT NULL DEFAULT 'activa',
    fecha_asignacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    FOREIGN KEY (id_rutina) REFERENCES rutinas(id_rutina)
);

CREATE TABLE horarios (
    id_horario INT AUTO_INCREMENT PRIMARY KEY,
    id_maquina INT NOT NULL,
    tipo VARCHAR(50) NOT NULL,
    fecha DATE NOT NULL,
    hora_inicio TIME NOT NULL,
    hora_fin TIME NOT NULL,
    FOREIGN KEY (id_maquina) REFERENCES maquinas(id_maquina)
);

CREATE TABLE mantenimientos (
    id_mantenimiento INT AUTO_INCREMENT PRIMARY KEY,
    id_maquina INT NOT NULL,
    descripcion VARCHAR(255) NOT NULL,
    tipo_mantenimiento VARCHAR(100) NOT NULL,
    fecha_inicio DATETIME NOT NULL,
    fecha_fin DATETIME NULL,
    estado ENUM('programado', 'en_proceso', 'completado', 'cancelado') NOT NULL DEFAULT 'programado',
    FOREIGN KEY (id_maquina) REFERENCES maquinas(id_maquina)
);

CREATE TABLE reservas (
    id_reserva INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_maquina INT NOT NULL,
    fecha_reserva DATE NOT NULL,
    hora_inicio TIME NOT NULL,
    hora_fin TIME NOT NULL,
    estado ENUM('activa', 'cancelada', 'completada', 'no_asistio') NOT NULL DEFAULT 'activa',
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    FOREIGN KEY (id_maquina) REFERENCES maquinas(id_maquina)
);

CREATE TABLE sesiones_entrenamiento (
    id_sesion INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_rutina INT NULL,
    fecha_inicio DATETIME NOT NULL,
    fecha_fin DATETIME NULL,
    estado ENUM('en_progreso', 'completada', 'cancelada') NOT NULL DEFAULT 'en_progreso',
    notas VARCHAR(255) NULL,
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    FOREIGN KEY (id_rutina) REFERENCES rutinas(id_rutina)
);

CREATE TABLE detalle_sesion_entrenamiento (
    id_detalle INT AUTO_INCREMENT PRIMARY KEY,
    id_sesion INT NOT NULL,
    id_ejercicio INT NOT NULL,
    id_maquina INT NULL,
    id_reserva INT NULL,
    series_realizadas INT NULL,
    repeticiones_realizadas INT NULL,
    peso_usado_kg DECIMAL(6,2) NULL,
    duracion_minutos INT NULL,
    esfuerzo_percibido INT NULL,
    notas VARCHAR(255) NULL,
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_sesion) REFERENCES sesiones_entrenamiento(id_sesion),
    FOREIGN KEY (id_ejercicio) REFERENCES ejercicios(id_ejercicio),
    FOREIGN KEY (id_maquina) REFERENCES maquinas(id_maquina),
    FOREIGN KEY (id_reserva) REFERENCES reservas(id_reserva)
);

CREATE TABLE progreso (
    id_progreso INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_rutina INT NULL,
    fecha DATE NOT NULL,
    porcentaje_completado DECIMAL(5,2) NOT NULL DEFAULT 0,
    calorias_estimadas DECIMAL(8,2) NULL,
    tiempo_total_minutos INT NULL,
    observacion VARCHAR(255) NULL,
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    FOREIGN KEY (id_rutina) REFERENCES rutinas(id_rutina)
);

INSERT INTO tipos_maquina (nombre) VALUES
('Piernas'),
('Biceps'),
('Triceps'),
('Hombro'),
('Espalda'),
('Pecho'),
('Cardio'),
('Core');

INSERT INTO maquinas (id_tipo_maquina, nombre, descripcion, ubicacion, estado, cantidad) VALUES
(1, 'Prensa 45', 'Prensa inclinada para tren inferior', 'Sala A', 'disponible', 2),
(1, 'Extension de cuadriceps', 'Maquina de aislamiento para cuadriceps', 'Sala A', 'disponible', 1),
(2, 'Curl de biceps', 'Maquina para flexion de codo', 'Sala B', 'disponible', 2),
(3, 'Polea de triceps', 'Polea alta para trabajo de triceps', 'Sala B', 'disponible', 2),
(4, 'Press de hombro', 'Press guiado para hombros', 'Sala B', 'disponible', 1),
(5, 'Remo sentado', 'Trabajo de espalda media', 'Sala C', 'disponible', 2),
(6, 'Press banca guiado', 'Pecho y triceps con soporte', 'Sala C', 'disponible', 2),
(7, 'Trotadora', 'Cardio de bajo y medio impacto', 'Sala D', 'disponible', 5),
(7, 'Bicicleta estatica', 'Cardio de bajo impacto', 'Sala D', 'disponible', 4),
(8, 'Banco abdominal', 'Trabajo de core y estabilidad', 'Sala E', 'disponible', 2);

-- Las contrasenas ya estan hasheadas con PBKDF2 para este seed.
-- En produccion no deben almacenarse contrasenas en texto plano.
INSERT INTO usuarios (nombre, rut, correo, contrasena_hash, rol) VALUES
('Juan Perez', '19092112-1', 'juan@mail.com', 'PBKDF2$100000$2fyU5pHeuafkdfRUZwMr9Q==$Nt7xFu1uRnururxutJoWiaBCtBnPlQ1K393AWLEj3Y0=', 'usuario'),
('Maria Lopez', '22333444-5', 'maria@mail.com', 'PBKDF2$100000$2fyU5pHeuafkdfRUZwMr9Q==$Nt7xFu1uRnururxutJoWiaBCtBnPlQ1K393AWLEj3Y0=', 'usuario'),
('Carlos Soto', '90109078-6', 'carlos@mail.com', 'PBKDF2$100000$2fyU5pHeuafkdfRUZwMr9Q==$Nt7xFu1uRnururxutJoWiaBCtBnPlQ1K393AWLEj3Y0=', 'entrenador'),
('Admin Gym', '11111111-1', 'admin@gym.com', 'PBKDF2$100000$2fyU5pHeuafkdfRUZwMr9Q==$Nt7xFu1uRnururxutJoWiaBCtBnPlQ1K393AWLEj3Y0=', 'admin');

INSERT INTO perfil_usuario (id_usuario, fecha_nacimiento, sexo, altura_cm, peso_kg, objetivo, nivel_actividad) VALUES
(1, '1998-06-10', 'masculino', 175.00, 78.00, 'ganar_fuerza', 'medio'),
(2, '2000-02-21', 'femenino', 162.00, 69.50, 'bajar_grasa', 'medio'),
(3, '1997-11-30', 'masculino', 180.00, 72.00, 'mantener', 'alto');

INSERT INTO historial_imc (id_usuario, altura_cm, peso_kg, imc, categoria_imc, fecha_registro) VALUES
(1, 175.00, 78.00, 25.47, 'sobrepeso', '2026-04-10 09:00:00'),
(1, 175.00, 76.50, 24.98, 'normal', '2026-04-24 09:00:00'),
(2, 162.00, 69.50, 26.48, 'sobrepeso', '2026-04-10 09:00:00'),
(3, 180.00, 72.00, 22.22, 'normal', '2026-04-10 09:00:00');

INSERT INTO rutinas (id_usuario, nombre, descripcion, tipo_rutina, objetivo, categoria_imc, dificultad, es_publica, id_rutina_origen) VALUES
(NULL, 'Fuerza principiante', 'Rutina inicial enfocada en movimientos basicos de fuerza.', 'predefinida', 'ganar_fuerza', 'normal', 'principiante', TRUE, NULL),
(NULL, 'Hipertrofia principiante', 'Rutina inicial enfocada en volumen y tecnica.', 'predefinida', 'ganar_musculo', 'normal', 'principiante', TRUE, NULL),
(NULL, 'Cardio inicial', 'Rutina base para mejorar resistencia cardiovascular.', 'predefinida', 'mejorar_resistencia', 'normal', 'principiante', TRUE, NULL),
(NULL, 'Full Body principiante', 'Rutina global para todo el cuerpo en nivel inicial.', 'predefinida', 'acondicionamiento', 'normal', 'principiante', TRUE, NULL),
(NULL, 'Piernas principiante', 'Rutina de tren inferior con enfoque tecnico.', 'predefinida', 'fortalecer_piernas', 'normal', 'principiante', TRUE, NULL),
(NULL, 'Rutina bajo impacto', 'Rutina suave enfocada en movilidad y cardio controlado.', 'predefinida', 'bajar_grasa', 'sobrepeso', 'principiante', TRUE, NULL),
(NULL, 'Rutina fuerza basica', 'Rutina de fuerza general para usuarios con IMC normal.', 'predefinida', 'ganar_fuerza', 'normal', 'principiante', TRUE, NULL),
(NULL, 'Rutina acondicionamiento inicial', 'Rutina de adaptacion progresiva para usuarios con bajo peso o normal.', 'predefinida', 'acondicionamiento', 'bajo_peso', 'principiante', TRUE, NULL),
(1, 'Mi rutina de Juan', 'Rutina personalizada creada por el usuario.', 'personalizada', 'ganar_fuerza', NULL, 'intermedio', FALSE, NULL);

INSERT INTO ejercicios (nombre, descripcion, grupo_muscular, dificultad, id_maquina) VALUES
('Prensa de piernas', 'Empuje controlado para cuadriceps y gluteos.', 'piernas', 'principiante', 1),
('Extension de cuadriceps', 'Aislamiento de cuadriceps.', 'piernas', 'principiante', 2),
('Curl de biceps en maquina', 'Trabajo guiado de biceps.', 'biceps', 'principiante', 3),
('Polea de triceps', 'Extension de codo en polea.', 'triceps', 'principiante', 4),
('Press de hombro guiado', 'Empuje vertical con soporte.', 'hombros', 'principiante', 5),
('Remo sentado', 'Trabajo de espalda media y dorsal.', 'espalda', 'principiante', 6),
('Press de pecho guiado', 'Empuje horizontal para pectoral.', 'pecho', 'principiante', 7),
('Trote en cinta', 'Cardio continuo de intensidad moderada.', 'cardio', 'principiante', 8),
('Bicicleta estatica', 'Cardio de bajo impacto para acondicionamiento.', 'cardio', 'principiante', 9),
('Plancha frontal', 'Estabilidad de core sin maquina.', 'core', 'principiante', NULL),
('Sentadilla con peso corporal', 'Trabajo funcional de piernas sin maquina.', 'piernas', 'principiante', NULL),
('Burpees', 'Ejercicio funcional de cuerpo completo.', 'full_body', 'intermedio', NULL);

INSERT INTO rutina_ejercicio (id_rutina, id_ejercicio, dia, orden, series, repeticiones, duracion_minutos, descanso_segundos, notas) VALUES
(1, 7, 'Lunes', 1, 4, 8, NULL, 90, 'Controlar tecnica'),
(1, 6, 'Lunes', 2, 4, 8, NULL, 90, 'Mantener espalda neutra'),
(1, 1, 'Miercoles', 1, 4, 10, NULL, 90, 'Movimiento completo'),
(1, 5, 'Viernes', 1, 3, 10, NULL, 75, 'Sin dolor articular'),
(2, 7, 'Lunes', 1, 4, 12, NULL, 60, 'Ritmo constante'),
(2, 3, 'Lunes', 2, 3, 12, NULL, 45, 'Evitar balanceo'),
(2, 4, 'Miercoles', 1, 3, 15, NULL, 45, 'Contraccion completa'),
(2, 5, 'Viernes', 1, 3, 12, NULL, 60, 'Subida controlada'),
(3, 8, 'Lunes', 1, NULL, NULL, 20, 0, 'Ritmo suave'),
(3, 9, 'Miercoles', 1, NULL, NULL, 25, 0, 'Cadencia estable'),
(3, 10, 'Viernes', 1, 3, 30, NULL, 30, 'Respiracion controlada'),
(4, 11, 'Lunes', 1, 3, 15, NULL, 45, 'Activacion general'),
(4, 7, 'Lunes', 2, 3, 10, NULL, 60, 'Carga ligera'),
(4, 6, 'Jueves', 1, 3, 10, NULL, 60, 'Priorizar tecnica'),
(4, 10, 'Jueves', 2, 3, 40, NULL, 30, 'Mantener abdomen activo'),
(5, 1, 'Martes', 1, 4, 12, NULL, 75, 'Profundidad controlada'),
(5, 2, 'Martes', 2, 3, 15, NULL, 45, 'No bloquear rodillas'),
(5, 11, 'Sabado', 1, 3, 20, NULL, 45, 'Aumentar movilidad'),
(6, 9, 'Lunes', 1, NULL, NULL, 20, 0, 'Intensidad baja'),
(6, 10, 'Lunes', 2, 3, 30, NULL, 30, 'Sin dolor lumbar'),
(6, 11, 'Miercoles', 1, 3, 15, NULL, 45, 'Control postural'),
(7, 7, 'Lunes', 1, 3, 8, NULL, 90, 'Carga moderada'),
(7, 1, 'Miercoles', 1, 3, 10, NULL, 90, 'Pausa de un segundo'),
(8, 9, 'Lunes', 1, NULL, NULL, 15, 0, 'Cardio adaptativo'),
(8, 10, 'Lunes', 2, 3, 20, NULL, 30, 'Core basico'),
(9, 1, 'Martes', 1, 4, 8, NULL, 90, 'Carga progresiva'),
(9, 5, 'Jueves', 1, 3, 10, NULL, 75, 'Estabilidad escapular');

INSERT INTO usuario_rutina (id_usuario, id_rutina, estado, fecha_asignacion) VALUES
(1, 1, 'activa', '2026-04-10 08:00:00'),
(2, 6, 'activa', '2026-04-10 08:00:00'),
(3, 4, 'activa', '2026-04-10 08:00:00'),
(1, 9, 'pausada', '2026-04-18 09:00:00');

INSERT INTO horarios (id_maquina, tipo, fecha, hora_inicio, hora_fin) VALUES
(1, 'uso', '2026-05-10', '10:00:00', '11:00:00'),
(8, 'limpieza', '2026-05-10', '12:00:00', '13:00:00');

INSERT INTO mantenimientos (id_maquina, descripcion, tipo_mantenimiento, fecha_inicio, fecha_fin, estado) VALUES
(8, 'Revision general de motor y banda', 'preventivo', '2026-05-10 12:00:00', '2026-05-10 13:00:00', 'programado'),
(6, 'Ajuste de poleas y cableado', 'correctivo', '2026-05-02 09:00:00', '2026-05-02 11:00:00', 'completado');

INSERT INTO reservas (id_usuario, id_maquina, fecha_reserva, hora_inicio, hora_fin, estado) VALUES
(1, 1, '2026-05-10', '10:00:00', '11:00:00', 'activa'),
(2, 9, '2026-05-10', '09:00:00', '09:45:00', 'completada'),
(1, 7, '2026-05-11', '18:00:00', '18:30:00', 'cancelada');

INSERT INTO sesiones_entrenamiento (id_usuario, id_rutina, fecha_inicio, fecha_fin, estado, notas) VALUES
(1, 1, '2026-04-24 18:00:00', '2026-04-24 18:58:00', 'completada', 'Sesion enfocada en fuerza de tren superior'),
(2, 6, '2026-04-25 09:30:00', '2026-04-25 10:05:00', 'completada', 'Sesion de cardio de bajo impacto'),
(1, 9, '2026-05-01 19:00:00', NULL, 'en_progreso', 'Pendiente de cierre');

INSERT INTO detalle_sesion_entrenamiento (id_sesion, id_ejercicio, id_maquina, id_reserva, series_realizadas, repeticiones_realizadas, peso_usado_kg, duracion_minutos, esfuerzo_percibido, notas) VALUES
(1, 7, 7, NULL, 4, 8, 45.00, NULL, 7, 'Buen control de tecnica'),
(1, 6, 6, NULL, 4, 8, 40.00, NULL, 8, 'Fatiga moderada'),
(2, 9, 9, 2, NULL, NULL, NULL, 25, 5, 'Cardio estable'),
(2, 10, NULL, NULL, 3, 30, NULL, NULL, 6, 'Core sin molestias');

INSERT INTO progreso (id_usuario, id_rutina, fecha, porcentaje_completado, calorias_estimadas, tiempo_total_minutos, observacion) VALUES
(1, 1, '2026-04-24', 100.00, 420.00, 58, 'Sesion completada con buena tecnica'),
(2, 6, '2026-04-25', 100.00, 290.00, 35, 'Rutina de bajo impacto completada'),
(1, 9, '2026-05-01', 50.00, 180.00, 25, 'Sesion interrumpida por tiempo');
