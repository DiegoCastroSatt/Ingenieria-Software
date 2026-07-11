USE gym_db;

CREATE TABLE IF NOT EXISTS usuario_maquina_favorita (
    id_usuario INT NOT NULL,
    id_maquina INT NOT NULL,
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id_usuario, id_maquina),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    FOREIGN KEY (id_maquina) REFERENCES maquinas(id_maquina)
);
