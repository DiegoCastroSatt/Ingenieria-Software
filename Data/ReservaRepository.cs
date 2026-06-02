using MySqlConnector;
using Softawer.Models;

namespace Softawer.Data;

public class ReservaRepository(MySqlDataSource dataSource)
{
    public async Task<IReadOnlyList<Reserva>> GetReservasActivasMaquinaAsync(int idMaquina, DateOnly fechaReserva)
    {
        var reservas = new List<Reserva>();
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT r.id_reserva, r.id_usuario, r.id_maquina, m.nombre AS nombre_maquina, r.fecha_reserva, r.hora_inicio, r.hora_fin,
                   r.estado, r.fecha_creacion, r.fecha_actualizacion
            FROM reservas r
            INNER JOIN maquinas m ON m.id_maquina = r.id_maquina
            WHERE r.id_maquina = @idMaquina
              AND r.fecha_reserva = @fechaReserva
              AND r.estado = 'activa';
            """;
        command.Parameters.AddWithValue("@idMaquina", idMaquina);
        command.Parameters.AddWithValue("@fechaReserva", fechaReserva.ToDateTime(TimeOnly.MinValue));

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            reservas.Add(MapReserva(reader));
        }

        return reservas;
    }

    public async Task<IReadOnlyList<Reserva>> ListReservasActivasPorFechaAsync(DateOnly fechaReserva)
    {
        var reservas = new List<Reserva>();
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT r.id_reserva, r.id_usuario, r.id_maquina, m.nombre AS nombre_maquina, r.fecha_reserva, r.hora_inicio, r.hora_fin,
                   r.estado, r.fecha_creacion, r.fecha_actualizacion
            FROM reservas r
            INNER JOIN maquinas m ON m.id_maquina = r.id_maquina
            WHERE r.fecha_reserva = @fechaReserva
              AND r.estado = 'activa'
            ORDER BY r.hora_inicio ASC;
            """;
        command.Parameters.AddWithValue("@fechaReserva", fechaReserva.ToDateTime(TimeOnly.MinValue));

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            reservas.Add(MapReserva(reader));
        }

        return reservas;
    }

    public async Task<Reserva> CreateReservaAsync(CrearReservaRequest request)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO reservas (id_usuario, id_maquina, fecha_reserva, hora_inicio, hora_fin, estado)
            VALUES (@idUsuario, @idMaquina, @fechaReserva, @horaInicio, @horaFin, 'activa');
            """;
        command.Parameters.AddWithValue("@idUsuario", request.IdUsuario);
        command.Parameters.AddWithValue("@idMaquina", request.IdMaquina);
        command.Parameters.AddWithValue("@fechaReserva", request.FechaReserva.ToDateTime(TimeOnly.MinValue));
        command.Parameters.AddWithValue("@horaInicio", request.HoraInicio.ToTimeSpan());
        command.Parameters.AddWithValue("@horaFin", request.HoraFin.ToTimeSpan());
        await command.ExecuteNonQueryAsync();
        var idReserva = Convert.ToInt32(command.LastInsertedId);
        return await GetReservaAsync(idReserva) ?? throw new InvalidOperationException("No se pudo leer la reserva creada.");
    }

    public async Task<Reserva?> GetReservaAsync(int idReserva)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT r.id_reserva, r.id_usuario, r.id_maquina, m.nombre AS nombre_maquina, r.fecha_reserva, r.hora_inicio, r.hora_fin,
                   r.estado, r.fecha_creacion, r.fecha_actualizacion
            FROM reservas r
            INNER JOIN maquinas m ON m.id_maquina = r.id_maquina
            WHERE r.id_reserva = @idReserva
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@idReserva", idReserva);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapReserva(reader) : null;
    }

    public async Task<Reserva> CancelReservaAsync(Reserva reserva)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await using (var updateCommand = connection.CreateCommand())
        {
            updateCommand.Transaction = transaction;
            updateCommand.CommandText = """
                UPDATE reservas
                SET estado = 'cancelada'
                WHERE id_reserva = @idReserva
                  AND estado = 'activa';
                """;
            updateCommand.Parameters.AddWithValue("@idReserva", reserva.IdReserva);
            var updatedRows = await updateCommand.ExecuteNonQueryAsync();
            if (updatedRows != 1)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("No se pudo cancelar la reserva.");
            }
        }

        await using (var historyCommand = connection.CreateCommand())
        {
            historyCommand.Transaction = transaction;
            historyCommand.CommandText = """
                INSERT INTO reserva_cancelaciones (id_reserva, id_usuario, estado_anterior)
                VALUES (@idReserva, @idUsuario, @estadoAnterior);
                """;
            historyCommand.Parameters.AddWithValue("@idReserva", reserva.IdReserva);
            historyCommand.Parameters.AddWithValue("@idUsuario", reserva.IdUsuario);
            historyCommand.Parameters.AddWithValue("@estadoAnterior", reserva.Estado);
            await historyCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
        return await GetReservaAsync(reserva.IdReserva)
            ?? throw new InvalidOperationException("No se pudo leer la reserva cancelada.");
    }

    public async Task<IReadOnlyList<Reserva>> ListReservasUsuarioAsync(int idUsuario)
    {
        var reservas = new List<Reserva>();
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT r.id_reserva, r.id_usuario, r.id_maquina, m.nombre AS nombre_maquina, r.fecha_reserva, r.hora_inicio, r.hora_fin,
                   r.estado, r.fecha_creacion, r.fecha_actualizacion
            FROM reservas r
            INNER JOIN maquinas m ON m.id_maquina = r.id_maquina
            WHERE r.id_usuario = @idUsuario
            ORDER BY r.fecha_reserva DESC, r.hora_inicio DESC;
            """;
        command.Parameters.AddWithValue("@idUsuario", idUsuario);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            reservas.Add(MapReserva(reader));
        }

        return reservas;
    }

    private static Reserva MapReserva(MySqlDataReader reader)
    {
        return new Reserva
        {
            IdReserva = reader.GetInt32("id_reserva"),
            IdUsuario = reader.GetInt32("id_usuario"),
            IdMaquina = reader.GetInt32("id_maquina"),
            NombreMaquina = reader.GetString("nombre_maquina"),
            FechaReserva = DateOnly.FromDateTime(reader.GetDateTime("fecha_reserva")),
            HoraInicio = TimeOnly.FromTimeSpan(reader.GetTimeSpan("hora_inicio")),
            HoraFin = TimeOnly.FromTimeSpan(reader.GetTimeSpan("hora_fin")),
            Estado = reader.GetString("estado"),
            FechaCreacion = reader.GetDateTime("fecha_creacion"),
            FechaActualizacion = reader.GetDateTime("fecha_actualizacion")
        };
    }
}
