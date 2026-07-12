using MySqlConnector;
using Softawer.Models;

namespace Softawer.Data;

public class ReporteProblemaRepository(MySqlDataSource dataSource)
{
    public async Task<ReporteProblema> CreateReporteAsync(CrearReporteProblemaRequest request)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO reportes_problemas (id_usuario, id_maquina, descripcion, estado)
            VALUES (@idUsuario, @idMaquina, @descripcion, 'pendiente');
            """;
        command.Parameters.AddWithValue("@idUsuario", request.IdUsuario);
        command.Parameters.AddWithValue("@idMaquina", (object?)request.IdMaquina ?? DBNull.Value);
        command.Parameters.AddWithValue("@descripcion", request.Descripcion.Trim());

        await command.ExecuteNonQueryAsync();
        return await GetReporteAsync(Convert.ToInt32(command.LastInsertedId))
            ?? throw new InvalidOperationException("No se pudo leer el reporte insertado.");
    }

    public async Task<ReporteProblema?> GetReporteAsync(int idReporte)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT rp.id_reporte, rp.id_usuario, rp.id_maquina, m.nombre AS nombre_maquina,
                   rp.descripcion, rp.fecha_creacion, rp.estado, rp.fecha_actualizacion
            FROM reportes_problemas rp
            LEFT JOIN maquinas m ON m.id_maquina = rp.id_maquina
            WHERE rp.id_reporte = @idReporte
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@idReporte", idReporte);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapReporte(reader) : null;
    }

    public async Task<IReadOnlyList<ReporteProblema>> ListReportesUsuarioAsync(int idUsuario)
    {
        var reportes = new List<ReporteProblema>();
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT rp.id_reporte, rp.id_usuario, rp.id_maquina, m.nombre AS nombre_maquina,
                   rp.descripcion, rp.fecha_creacion, rp.estado, rp.fecha_actualizacion
            FROM reportes_problemas rp
            LEFT JOIN maquinas m ON m.id_maquina = rp.id_maquina
            WHERE rp.id_usuario = @idUsuario
            ORDER BY rp.fecha_creacion DESC, rp.id_reporte DESC;
            """;
        command.Parameters.AddWithValue("@idUsuario", idUsuario);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            reportes.Add(MapReporte(reader));
        }

        return reportes;
    }

    private static ReporteProblema MapReporte(MySqlDataReader reader)
    {
        return new ReporteProblema
        {
            IdReporte = reader.GetInt32("id_reporte"),
            IdUsuario = reader.GetInt32("id_usuario"),
            IdMaquina = reader.IsDBNull("id_maquina") ? null : reader.GetInt32("id_maquina"),
            NombreMaquina = reader.IsDBNull("nombre_maquina") ? null : reader.GetString("nombre_maquina"),
            Descripcion = reader.GetString("descripcion"),
            FechaCreacion = reader.GetDateTime("fecha_creacion"),
            Estado = reader.GetString("estado"),
            FechaActualizacion = reader.GetDateTime("fecha_actualizacion")
        };
    }
}
