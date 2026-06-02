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
DROP TABLE IF EXISTS reserva_cancelaciones;
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
    musculos_objetivo VARCHAR(255) NOT NULL DEFAULT '',
    imagen_url VARCHAR(255) NOT NULL DEFAULT '',
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

CREATE TABLE reserva_cancelaciones (
    id_cancelacion INT AUTO_INCREMENT PRIMARY KEY,
    id_reserva INT NOT NULL,
    id_usuario INT NOT NULL,
    estado_anterior ENUM('activa', 'cancelada', 'completada', 'no_asistio') NOT NULL,
    fecha_cancelacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_reserva) REFERENCES reservas(id_reserva),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
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

-- tipos de maquina
INSERT INTO tipos_maquina (nombre) VALUES
('Piernas'),
('Gluteos'),
('Hombro'),
('Espalda'),
('Pecho'),
('Cardio'),
('Funcional'),
('Brazos');

-- maquinas
INSERT INTO maquinas (id_tipo_maquina, nombre, descripcion, musculos_objetivo, imagen_url, ubicacion, estado, cantidad) VALUES
(1, '45 degrees Leg Press', 'Prensa inclinada para empuje de tren inferior con soporte lumbar.', 'Quadriceps, Glutes, Hamstrings', 'machines/45-leg-press.jpg', 'Sala Piernas', 'disponible', 2),
(1, 'Hack Squat', 'Sentadilla guiada para trabajar piernas con trayectoria estable.', 'Quadriceps, Glutes', 'machines/hack-squat.jpg', 'Sala Piernas', 'disponible', 1),
(2, 'Hip Thrust Machine', 'Maquina para extension de cadera enfocada en gluteos.', 'Gluteus Maximus', 'machines/hip-thrust-machine.jpg', 'Sala Piernas', 'disponible', 1),
(1, 'Belt Squat', 'Sentadilla con cinturon que reduce carga sobre la columna.', 'Quadriceps, Glutes, Hamstrings (Spinal Decompression)', 'machines/belt-squat.png', 'Sala Piernas', 'disponible', 1),
(1, 'Pendulum Squat', 'Sentadilla pendular para enfatizar cuadriceps y gluteos.', 'Quadriceps, Glutes', 'machines/pendulum-squat.jpg', 'Sala Piernas', 'disponible', 1),
(2, 'Glute Drive', 'Equipo de empuje de cadera para fuerza de gluteos.', 'Glutes', 'machines/glute-drive.png', 'Sala Piernas', 'disponible', 1),
(2, 'Hip Abductor Machine', 'Aislamiento de abductores de cadera con recorrido controlado.', 'Gluteus Medius, Hip Abductors', 'machines/hip-abductor-machine.png', 'Sala Piernas', 'disponible', 1),
(5, 'Iso-Lateral Chest Press', 'Press de pecho unilateral con palancas independientes.', 'Pectorals, Triceps, Anterior Deltoids', 'machines/iso-lateral-chest-press.png', 'Sala Fuerza', 'disponible', 2),
(3, 'Iso-Lateral Shoulder Press', 'Press vertical para hombros con movimiento unilateral.', 'Deltoids, Triceps', 'machines/iso-lateral-shoulder-press.png', 'Sala Fuerza', 'disponible', 1),
(4, 'Iso-Lateral Low Row', 'Remo bajo unilateral para espalda media y dorsal.', 'Latissimus Dorsi, Rhomboids, Traps, Biceps', 'machines/iso-lateral-low-row.png', 'Sala Fuerza', 'disponible', 1),
(4, 'Lat Pulldown Machine', 'Jalon al pecho guiado para dorsales y biceps.', 'Latissimus Dorsi, Biceps', 'machines/lat-pulldown-machine.png', 'Sala Fuerza', 'disponible', 2),
(7, 'Assisted Dip & Chin-up', 'Asistencia regulable para fondos y dominadas.', 'Chest, Triceps, Lats, Shoulders', 'machines/assisted-dip-chin-up.png', 'Sala Funcional', 'disponible', 1),
(5, 'Pec Deck / Rear Delt Fly', 'Equipo dual para pecho y deltoides posterior.', 'Pectorals / Posterior Deltoids', 'machines/pec-deck-rear-delt-fly.png', 'Sala Fuerza', 'disponible', 1),
(3, 'Lateral Raise Machine', 'Aislamiento de deltoides lateral.', 'Lateral Deltoids', 'machines/lateral-raise-machine.png', 'Sala Fuerza', 'disponible', 1),
(7, 'Power Rack', 'Rack multiproposito para ejercicios compuestos con barra.', 'Full Body (Compound Exercises)', 'machines/power-rack.jpg', 'Sala Peso Libre', 'disponible', 2),
(7, 'Olympic Barbell & Plates', 'Barras olimpicas y discos para trabajo de fuerza general.', 'Full Body (Compound Exercises)', 'machines/olympic-barbell-plates.jpg', 'Sala Peso Libre', 'disponible', 6),
(7, 'Urethane Dumbbells', 'Mancuernas de uretano para ejercicios de aislamiento y compuestos.', 'Full Body (Isolation & Compound)', 'machines/urethane-dumbbells.png', 'Sala Peso Libre', 'disponible', 20),
(6, 'Suspension Treadmill', 'Trotadora con sistema de suspension para cardio controlado.', 'Cardiovascular System, Legs', 'machines/suspension-treadmill.jpg', 'Sala Cardio', 'disponible', 4),
(6, 'StairMaster / Step Mill', 'Escaladora para gluteos, cuadriceps y cardio intenso.', 'Glutes, Quadriceps, Cardiovascular System', 'machines/stairmaster-step-mill.jpg', 'Sala Cardio', 'disponible', 2),
(6, 'Curved Treadmill', 'Trotadora curva sin motor para cadena posterior y cardio.', 'Posterior Chain, Cardiovascular System', 'machines/curved-treadmill.png', 'Sala Cardio', 'disponible', 2),
(6, 'Assault / Air Bike', 'Bicicleta de aire para acondicionamiento de cuerpo completo.', 'Full Body, Cardiovascular System', 'machines/assault-air-bike.jpg', 'Sala Cardio', 'disponible', 2),
(6, 'Concept2 Rower', 'Remo indoor para espalda, piernas, core y cardio.', 'Full Body (Back, Legs, Core, Cardio)', 'machines/concept2-rower.jpg', 'Sala Cardio', 'disponible', 2),
(7, 'Dual Adjustable Pulley', 'Polea doble ajustable para entrenamiento funcional.', 'Full Body (Functional Training)', 'machines/dual-adjustable-pulley.png', 'Sala Funcional', 'disponible', 2),
(8, 'Multi-Station Tower', 'Torre multiestacion para espalda, triceps y biceps.', 'Back, Triceps, Biceps', 'machines/multi-station-tower.jpg', 'Sala Fuerza', 'disponible', 1),
(5, 'Cable Crossover', 'Cruce de poleas para pectorales y brazos.', 'Pectorals, Arms', 'machines/cable-crossover.png', 'Sala Fuerza', 'disponible', 1);

-- usuarios
INSERT INTO usuarios (nombre, rut, correo, contrasena_hash, rol) VALUES
('Juan Perez', '19092112-1', 'juan@mail.com', 'PBKDF2$100000$2fyU5pHeuafkdfRUZwMr9Q==$Nt7xFu1uRnururxutJoWiaBCtBnPlQ1K393AWLEj3Y0=', 'usuario'),
('Maria Lopez', '22333444-5', 'maria@mail.com', 'PBKDF2$100000$2fyU5pHeuafkdfRUZwMr9Q==$Nt7xFu1uRnururxutJoWiaBCtBnPlQ1K393AWLEj3Y0=', 'usuario'),
('Carlos Soto', '90109078-6', 'carlos@mail.com', 'PBKDF2$100000$2fyU5pHeuafkdfRUZwMr9Q==$Nt7xFu1uRnururxutJoWiaBCtBnPlQ1K393AWLEj3Y0=', 'entrenador'),
('Admin Gym', '11111111-1', 'admin@gym.com', 'PBKDF2$100000$2fyU5pHeuafkdfRUZwMr9Q==$Nt7xFu1uRnururxutJoWiaBCtBnPlQ1K393AWLEj3Y0=', 'admin');

-- perfil usuario
INSERT INTO perfil_usuario (id_usuario, fecha_nacimiento, sexo, altura_cm, peso_kg, objetivo, nivel_actividad) VALUES
(1, '1998-06-10', 'masculino', 175.00, 78.00, 'ganar_fuerza', 'medio'),
(2, '2000-02-21', 'femenino', 162.00, 69.50, 'bajar_grasa', 'medio'),
(3, '1997-11-30', 'masculino', 180.00, 72.00, 'mantener', 'alto');

-- historial imc
INSERT INTO historial_imc (id_usuario, altura_cm, peso_kg, imc, categoria_imc, fecha_registro) VALUES
(1, 175.00, 78.00, 25.47, 'sobrepeso', '2026-04-10 09:00:00'),
(1, 175.00, 76.50, 24.98, 'normal', '2026-04-24 09:00:00'),
(2, 162.00, 69.50, 26.48, 'sobrepeso', '2026-04-10 09:00:00'),
(3, 180.00, 72.00, 22.22, 'normal', '2026-04-10 09:00:00');

-- rutinas 
-- rutinas 
INSERT INTO rutinas (id_usuario, nombre, descripcion, tipo_rutina, objetivo, categoria_imc, dificultad, es_publica, id_rutina_origen) VALUES
(NULL, 'Fuerza principiante', 'Rutina inicial de fuerza con ejercicios basicos y descansos amplios para construir tecnica.', 'predefinida', 'ganar_fuerza', 'normal', 'principiante', TRUE, NULL),
(NULL, 'Hipertrofia principiante', 'Rutina de volumen inicial para generar tension mecanica y crecimiento muscular.', 'predefinida', 'ganar_musculo', 'bajo_peso', 'principiante', TRUE, NULL),
(NULL, 'Cardio inicial', 'Rutina cardiovascular de bajo riesgo para mejorar resistencia general.', 'predefinida', 'mejorar_resistencia', 'normal', 'principiante', TRUE, NULL),
(NULL, 'Full Body principiante', 'Rutina completa para trabajar fuerza, movilidad y acondicionamiento en una sesion.', 'predefinida', 'acondicionamiento', 'normal', 'principiante', TRUE, NULL),
(NULL, 'Piernas principiante', 'Rutina de tren inferior para fortalecer cuadriceps, gluteos e isquiotibiales.', 'predefinida', 'fortalecer_piernas', 'normal', 'principiante', TRUE, NULL),
(NULL, 'Rutina bajo impacto', 'Rutina suave para sobrepeso u obesidad con cardio controlado y maquinas estables.', 'predefinida', 'bajar_grasa', 'sobrepeso', 'principiante', TRUE, NULL),
(NULL, 'Rutina fuerza basica', 'Rutina general de fuerza para usuarios con IMC normal.', 'predefinida', 'ganar_fuerza', 'normal', 'principiante', TRUE, NULL),
(NULL, 'Rutina acondicionamiento inicial', 'Adaptacion inicial para bajo peso o condicion fisica baja.', 'predefinida', 'acondicionamiento', 'bajo_peso', 'principiante', TRUE, NULL),
(NULL, 'Fuerza avanzada', 'Rutina de fuerza intermedia con ejercicios compuestos y progresion de carga.', 'predefinida', 'ganar_fuerza', 'normal', 'intermedio', TRUE, NULL),
(NULL, 'Hipertrofia avanzada', 'Rutina de hipertrofia intermedia para detalle muscular y volumen.', 'predefinida', 'ganar_musculo', 'normal', 'intermedio', TRUE, NULL),
(1, 'Mi rutina de Juan', 'Rutina personalizada', 'personalizada', 'ganar_fuerza', NULL, 'intermedio', FALSE, NULL);

-- ejercicios
INSERT INTO ejercicios (nombre, descripcion, grupo_muscular, dificultad, id_maquina) VALUES
('Sentadilla con barra', 'Ejercicio compuesto para fuerza de piernas y cadera dentro del rack.', 'piernas', 'intermedio', 15),
('Press de banca', 'Empuje horizontal para pectorales, triceps y deltoides anteriores.', 'pecho', 'principiante', 8),
('Remo con barra', 'Traccion horizontal para espalda media y dorsal.', 'espalda', 'intermedio', 16),
('Press militar', 'Empuje vertical para hombros y triceps.', 'hombros', 'intermedio', 16),
('Prensa de piernas', 'Empuje controlado para cuadriceps y gluteos.', 'piernas', 'principiante', 1),
('Jalon al pecho', 'Traccion vertical para dorsales y biceps.', 'espalda', 'principiante', 11),
('Press de pecho en maquina', 'Empuje guiado para pectoral y triceps.', 'pecho', 'principiante', 8),
('Caminata en trotadora', 'Cardio LISS de bajo a medio impacto.', 'cardio', 'principiante', 18),
('Zancadas con mancuernas', 'Trabajo unilateral de piernas y estabilidad.', 'piernas', 'principiante', 17),
('Dominadas asistidas', 'Traccion asistida para espalda, brazos y hombros.', 'espalda', 'principiante', 12),
('Fondos asistidos', 'Empuje asistido para pecho y triceps.', 'pecho', 'principiante', 12),
('Plancha abdominal', 'Trabajo de core y estabilidad sin maquina.', 'core', 'principiante', NULL),
('Extension de piernas', 'Aislamiento de cuadriceps con recorrido controlado.', 'piernas', 'principiante', 2),
('Remo en polea baja', 'Remo estable para espalda y biceps.', 'espalda', 'principiante', 10),
('Bicicleta estatica', 'Cardio de bajo impacto con intensidad regulable.', 'cardio', 'principiante', 21),
('Peso muerto', 'Levantamiento compuesto para cadena posterior y fuerza maxima.', 'full_body', 'avanzado', 15),
('Press inclinado con mancuernas', 'Empuje inclinado para pectoral superior y hombros.', 'pecho', 'intermedio', 17),
('Elevaciones laterales', 'Aislamiento de deltoides lateral.', 'hombros', 'principiante', 14),
('Curl de biceps', 'Flexion de codo para trabajo de biceps.', 'brazos', 'principiante', 17),
('Press frances', 'Extension de codo para triceps.', 'brazos', 'intermedio', 17);

INSERT INTO rutina_ejercicio (id_rutina, id_ejercicio, dia, orden, series, repeticiones, duracion_minutos, descanso_segundos, notas) VALUES
(1, 1, 'Lunes', 1, 3, 10, NULL, 150, 'Descanso 2 a 3 minutos'),
(1, 2, 'Lunes', 2, 3, 10, NULL, 150, 'Controlar tecnica'),
(1, 3, 'Miercoles', 1, 3, 10, NULL, 150, 'Espalda neutra'),
(1, 4, 'Viernes', 1, 3, 10, NULL, 150, 'Hombros estables'),
(2, 1, 'Lunes', 1, 3, 10, NULL, 150, 'Foco en tension mecanica'),
(2, 2, 'Lunes', 2, 3, 10, NULL, 150, 'Carga progresiva'),
(2, 3, 'Miercoles', 1, 3, 10, NULL, 150, 'Rango completo'),
(2, 4, 'Viernes', 1, 3, 10, NULL, 150, 'Sin balanceo'),
(3, 8, 'Lunes', 1, NULL, NULL, 20, 0, 'Caminata a paso ligero'),
(3, 15, 'Miercoles', 1, NULL, NULL, 20, 0, 'Cardio bajo impacto'),
(3, 12, 'Viernes', 1, 3, 45, NULL, 30, 'Respiracion controlada'),
(4, 9, 'Lunes', 1, 3, 12, NULL, 90, 'Por pierna'),
(4, 10, 'Miercoles', 1, 3, NULL, NULL, 90, 'Fallo tecnico controlado'),
(4, 11, 'Viernes', 1, 3, 12, NULL, 90, 'Control escapular'),
(4, 12, 'Viernes', 2, 3, 45, NULL, 45, 'Mantener abdomen activo'),
(5, 5, 'Martes', 1, 4, 12, NULL, 75, 'Profundidad controlada'),
(5, 13, 'Martes', 2, 3, 15, NULL, 60, 'No bloquear rodillas'),
(5, 9, 'Sabado', 1, 3, 12, NULL, 90, 'Trabajo unilateral'),
(6, 13, 'Lunes', 1, 3, 15, NULL, 120, 'Bajo impacto'),
(6, 14, 'Lunes', 2, 3, 15, NULL, 120, 'Postura controlada'),
(6, 7, 'Miercoles', 1, 3, 15, NULL, 120, 'Carga liviana'),
(6, 15, 'Viernes', 1, NULL, NULL, 20, 0, 'Sin impacto en rodillas'),
(7, 1, 'Lunes', 1, 3, 10, NULL, 120, 'Fuerza basica'),
(7, 2, 'Miercoles', 1, 3, 10, NULL, 120, 'Empuje horizontal'),
(7, 6, 'Viernes', 1, 3, 12, NULL, 90, 'Dorsal controlado'),
(8, 15, 'Lunes', 1, NULL, NULL, 15, 0, 'Adaptacion cardiovascular'),
(8, 12, 'Miercoles', 1, 3, 20, NULL, 30, 'Core basico'),
(8, 13, 'Viernes', 1, 3, 15, NULL, 90, 'Activacion de piernas'),
(9, 16, 'Lunes', 1, 4, 5, NULL, 240, 'Fuerza maxima'),
(9, 17, 'Miercoles', 1, 4, 10, NULL, 120, 'Pectoral superior'),
(9, 18, 'Viernes', 1, 4, 15, NULL, 60, 'Aislamiento'),
(10, 16, 'Lunes', 1, 4, 5, NULL, 240, 'Ejercicio pesado'),
(10, 17, 'Miercoles', 1, 4, 10, NULL, 120, 'Hipertrofia'),
(10, 18, 'Jueves', 1, 4, 15, NULL, 60, 'Detalle de hombros'),
(10, 19, 'Viernes', 1, 3, 12, NULL, 60, 'Superserie con triceps'),
(10, 20, 'Viernes', 2, 3, 12, NULL, 60, 'Superserie con biceps'),
(11, 2, 'Lunes', 1, 4, 12, NULL, 75, 'Rutina personalizada base'),
(11, 6, 'Miercoles', 1, 4, 12, NULL, 75, 'Espalda controlada'),
(11, 5, 'Viernes', 1, 4, 12, NULL, 90, 'Piernas completas');

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
