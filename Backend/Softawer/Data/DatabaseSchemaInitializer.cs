using MySqlConnector;

namespace Softawer.Data;

public class DatabaseSchemaInitializer(MySqlDataSource dataSource)
{
    private static readonly IReadOnlyList<(string Name, string Definition)> PerfilUsuarioPublicColumns =
    [
        ("alias", "VARCHAR(100) NULL"),
        ("avatar_url", "VARCHAR(500) NULL"),
        ("telefono_trabajo", "VARCHAR(20) NULL"),
        ("email_trabajo", "VARCHAR(100) NULL"),
        ("sitio_personal", "VARCHAR(200) NULL"),
        ("twitter", "VARCHAR(50) NULL")
    ];

    public async Task EnsurePerfilUsuarioPublicColumnsAsync()
    {
        await using var connection = await dataSource.OpenConnectionAsync();

        foreach (var column in PerfilUsuarioPublicColumns)
        {
            if (await PerfilUsuarioColumnExistsAsync(connection, column.Name))
            {
                continue;
            }

            await using var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = $"ALTER TABLE perfil_usuario ADD COLUMN {column.Name} {column.Definition};";
            await alterCommand.ExecuteNonQueryAsync();
        }
    }

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

    public async Task EnsureMetricasAsync()
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS metricas_fuerza (
                id_metrica INT AUTO_INCREMENT PRIMARY KEY,
                id_usuario INT NOT NULL,
                ejercicio VARCHAR(120) NOT NULL,
                peso_kg DECIMAL(6,2) NOT NULL,
                fecha DATE NOT NULL,
                notas VARCHAR(255) NULL,
                fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                INDEX idx_metricas_fuerza_usuario_fecha (id_usuario, fecha),
                FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
            );
            """;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<bool> PerfilUsuarioColumnExistsAsync(MySqlConnection connection, string columnName)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'perfil_usuario'
              AND COLUMN_NAME = @columnName;
            """;
        command.Parameters.AddWithValue("@columnName", columnName);
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }
}
