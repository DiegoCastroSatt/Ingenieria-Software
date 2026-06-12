using MySqlConnector;
using Softawer.Models;

namespace Softawer.Data;

public class MetricasRepository(MySqlDataSource dataSource)
{
    public async Task<IReadOnlyList<MetricaFuerza>> ListMetricasUsuarioAsync(int idUsuario)
    {
        var metricas = new List<MetricaFuerza>();

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id_metrica, id_usuario, ejercicio, peso_kg, fecha, notas, fecha_creacion
            FROM metricas_fuerza
            WHERE id_usuario = @idUsuario
            ORDER BY fecha DESC, id_metrica DESC;
            """;
        command.Parameters.AddWithValue("@idUsuario", idUsuario);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            metricas.Add(MapMetrica(reader));
        }

        return metricas;
    }

    public async Task<MetricaFuerza> CreateMetricaAsync(CrearMetricaFuerzaRequest request)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO metricas_fuerza (id_usuario, ejercicio, peso_kg, fecha, notas)
            VALUES (@idUsuario, @ejercicio, @pesoKg, @fecha, @notas);
            """;
        command.Parameters.AddWithValue("@idUsuario", request.IdUsuario);
        command.Parameters.AddWithValue("@ejercicio", request.Ejercicio.Trim());
        command.Parameters.AddWithValue("@pesoKg", request.PesoKg);
        command.Parameters.AddWithValue("@fecha", request.Fecha.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@notas", string.IsNullOrWhiteSpace(request.Notas) ? DBNull.Value : request.Notas.Trim());

        await command.ExecuteNonQueryAsync();
        return await GetMetricaAsync(Convert.ToInt32(command.LastInsertedId))
            ?? throw new InvalidOperationException("No se pudo leer la metrica insertada.");
    }

    public async Task<bool> DeleteMetricaAsync(int idMetrica, int idUsuario)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM metricas_fuerza
            WHERE id_metrica = @idMetrica AND id_usuario = @idUsuario;
            """;
        command.Parameters.AddWithValue("@idMetrica", idMetrica);
        command.Parameters.AddWithValue("@idUsuario", idUsuario);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    private async Task<MetricaFuerza?> GetMetricaAsync(int idMetrica)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id_metrica, id_usuario, ejercicio, peso_kg, fecha, notas, fecha_creacion
            FROM metricas_fuerza
            WHERE id_metrica = @idMetrica
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@idMetrica", idMetrica);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapMetrica(reader) : null;
    }

    private static MetricaFuerza MapMetrica(MySqlDataReader reader)
    {
        return new MetricaFuerza
        {
            IdMetrica = reader.GetInt32("id_metrica"),
            IdUsuario = reader.GetInt32("id_usuario"),
            Ejercicio = reader.GetString("ejercicio"),
            PesoKg = reader.GetDecimal("peso_kg"),
            Fecha = DateOnly.FromDateTime(reader.GetDateTime("fecha")),
            Notas = reader.IsDBNull("notas") ? null : reader.GetString("notas"),
            FechaCreacion = reader.GetDateTime("fecha_creacion")
        };
    }
}
