USE gym_db;

CREATE TABLE IF NOT EXISTS perfil_usuario (
    id_perfil INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT NOT NULL,
    alias VARCHAR(100) NULL,
    avatar_url VARCHAR(500) NULL,
    telefono_trabajo VARCHAR(20) NULL,
    email_trabajo VARCHAR(100) NULL,
    sitio_personal VARCHAR(200) NULL,
    twitter VARCHAR(50) NULL,
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
);
