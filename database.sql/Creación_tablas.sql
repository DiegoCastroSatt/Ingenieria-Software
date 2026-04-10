-- Tipos de máquinas
INSERT INTO tipos_maquina (nombre) VALUES
('Piernas'),
('Biceps'),
('Triceps'),
('Hombro'),
('Espalda'),
('Pecho'),
('Cardio');

-- Máquinas
INSERT INTO maquinas (nombre, id_tipo, cantidad) VALUES
('Prensa', 1, 3),
('Curl Biceps', 2, 2),
('Polea Triceps', 3, 2),
('Press Hombro', 4, 2),
('Remo', 5, 3),
('Press Banca', 6, 2),
('Trotadora', 7, 5);

-- Usuarios
INSERT INTO usuarios (nombre, rut, correo, contrasena) VALUES
('Juan Perez', '19092112-1', 'juan@mail.com', '1234'),
('Maria Lopez', '22333444-5', 'maria@mail.com', '1234'),
('Carlos Soto', '90109078-6', 'carlos@mail.com', '1234');

-- Rutinas
INSERT INTO rutinas (nombre, descripcion) VALUES
('Fuerza', 'Entrenamiento de fuerza'),
('Hipertrofia', 'Aumento de masa muscular'),
('Cardio', 'Resistencia cardiovascular'),
('Full Body', 'Entrenamiento completo'),
('Piernas', 'Entrenamiento de piernas');

-- Usuario ↔ Rutina
INSERT INTO usuario_rutina (id_usuario, id_rutina, fecha_inicio) VALUES
(1, 1, '2026-04-10'),
(2, 2, '2026-04-10'),
(3, 3, '2026-04-10');

-- Horarios
INSERT INTO horarios (id_maquina, tipo, fecha, hora_inicio, hora_fin) VALUES
(1, 'uso', '2026-04-10', '10:00', '11:00'),
(7, 'limpieza', '2026-04-10', '12:00', '13:00');

-- Mantenimiento
INSERT INTO mantenimientos (id_maquina, fecha, descripcion) VALUES
(7, '2026-04-15', 'Revisión general');

-- Reservas
INSERT INTO reservas (id_usuario, id_maquina, fecha, hora_inicio, hora_fin) VALUES
(1, 7, '2026-04-10', '10:00', '11:00'),
(2, 1, '2026-04-10', '11:00', '12:00');

-- Progreso
INSERT INTO progreso (id_usuario, id_rutina, peso, repeticiones, fecha) VALUES
(1, 1, 50.0, 10, '2026-04-10'),
(2, 2, 40.0, 12, '2026-04-10');

ALTER TABLE usuarios ADD UNIQUE (rut);