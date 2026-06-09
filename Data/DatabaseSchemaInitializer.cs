using MySqlConnector;

namespace Softawer.Data;

public class DatabaseSchemaInitializer(MySqlDataSource dataSource)
{
    public async Task EnsureUsuarioMaquinaFavoritaAsync()
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS usuario_maquina_favorita (
                id_usuario INT NOT NULL,
                id_maquina INT NOT NULL,
                fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY (id_usuario, id_maquina),
                FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
                FOREIGN KEY (id_maquina) REFERENCES maquinas(id_maquina)
            );
            """;
        await command.ExecuteNonQueryAsync();
    }

    public async Task EnsureReservaCancelacionesAsync()
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS reserva_cancelaciones (
                id_cancelacion INT AUTO_INCREMENT PRIMARY KEY,
                id_reserva INT NOT NULL,
                id_usuario INT NOT NULL,
                estado_anterior ENUM('activa', 'cancelada', 'completada', 'no_asistio') NOT NULL,
                fecha_cancelacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (id_reserva) REFERENCES reservas(id_reserva),
                FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
            );
            """;
        await command.ExecuteNonQueryAsync();
    }
}
