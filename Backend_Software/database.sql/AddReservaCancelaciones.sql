USE gym_db;

CREATE TABLE IF NOT EXISTS reserva_cancelaciones (
    id_cancelacion INT AUTO_INCREMENT PRIMARY KEY,
    id_reserva INT NOT NULL,
    id_usuario INT NOT NULL,
    estado_anterior ENUM('activa', 'cancelada', 'completada', 'no_asistio') NOT NULL,
    fecha_cancelacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_reserva) REFERENCES reservas(id_reserva),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
);
