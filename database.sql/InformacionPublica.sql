ALTER TABLE perfil_usuario 
ADD COLUMN alias VARCHAR(100) NULL AFTER nivel_actividad,
ADD COLUMN avatar_url VARCHAR(500) NULL AFTER alias,
ADD COLUMN telefono_trabajo VARCHAR(20) NULL AFTER avatar_url,
ADD COLUMN email_trabajo VARCHAR(100) NULL AFTER telefono_trabajo,
ADD COLUMN sitio_personal VARCHAR(200) NULL AFTER email_trabajo,
ADD COLUMN twitter VARCHAR(50) NULL AFTER sitio_personal;

