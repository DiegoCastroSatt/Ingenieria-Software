using MySqlConnector;
using Softawer.Models;

namespace Softawer.Data;

public class SesionEntrenamientoRepository(MySqlDataSource dataSource)
{
    public async Task<SesionEntrenamiento> CreateSesionAsync(SesionEntrenamiento sesion)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO sesiones_entrenamiento (id_usuario, id_rutina, fecha_inicio, estado, notas)
            VALUES (@idUsuario, @idRutina, @fechaInicio, @estado, @notas);
            """;
        command.Parameters.AddWithValue("@idUsuario", sesion.IdUsuario);
        command.Parameters.AddWithValue("@idRutina", sesion.IdRutina);
        command.Parameters.AddWithValue("@fechaInicio", sesion.FechaInicio);
        command.Parameters.AddWithValue("@estado", sesion.Estado);
        command.Parameters.AddWithValue("@notas", sesion.Notas);
        await command.ExecuteNonQueryAsync();
        sesion.IdSesion = Convert.ToInt32(command.LastInsertedId);
        return await GetSesionAsync(sesion.IdSesion) ?? sesion;
    }

    public async Task<SesionEntrenamiento?> GetSesionAsync(int idSesion)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id_sesion, id_usuario, id_rutina, fecha_inicio, fecha_fin, estado, notas
            FROM sesiones_entrenamiento
            WHERE id_sesion = @idSesion
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@idSesion", idSesion);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapSesion(reader) : null;
    }

    public async Task<DetalleSesionEntrenamiento> AddDetalleAsync(DetalleSesionEntrenamiento detalle)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO detalle_sesion_entrenamiento
                (id_sesion, id_ejercicio, id_maquina, id_reserva, series_realizadas, repeticiones_realizadas, peso_usado_kg, duracion_minutos, esfuerzo_percibido, notas, fecha_creacion)
            VALUES
                (@idSesion, @idEjercicio, @idMaquina, @idReserva, @seriesRealizadas, @repeticionesRealizadas, @pesoUsadoKg, @duracionMinutos, @esfuerzoPercibido, @notas, @fechaCreacion);
            """;
        command.Parameters.AddWithValue("@idSesion", detalle.IdSesion);
        command.Parameters.AddWithValue("@idEjercicio", detalle.IdEjercicio);
        command.Parameters.AddWithValue("@idMaquina", detalle.IdMaquina);
        command.Parameters.AddWithValue("@idReserva", detalle.IdReserva);
        command.Parameters.AddWithValue("@seriesRealizadas", detalle.SeriesRealizadas);
        command.Parameters.AddWithValue("@repeticionesRealizadas", detalle.RepeticionesRealizadas);
        command.Parameters.AddWithValue("@pesoUsadoKg", detalle.PesoUsadoKg);
        command.Parameters.AddWithValue("@duracionMinutos", detalle.DuracionMinutos);
        command.Parameters.AddWithValue("@esfuerzoPercibido", detalle.EsfuerzoPercibido);
        command.Parameters.AddWithValue("@notas", detalle.Notas);
        command.Parameters.AddWithValue("@fechaCreacion", detalle.FechaCreacion);
        await command.ExecuteNonQueryAsync();
        detalle.IdDetalle = Convert.ToInt32(command.LastInsertedId);
        return await GetDetalleAsync(detalle.IdDetalle) ?? detalle;
    }

    public async Task CompleteSesionAsync(SesionEntrenamiento sesion, CompletarSesionRequest request)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            await using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = """
                    UPDATE sesiones_entrenamiento
                    SET fecha_fin = @fechaFin,
                        estado = @estado,
                        notas = @notas
                    WHERE id_sesion = @idSesion;
                    """;
                command.Parameters.AddWithValue("@fechaFin", sesion.FechaFin);
                command.Parameters.AddWithValue("@estado", sesion.Estado);
                command.Parameters.AddWithValue("@notas", sesion.Notas);
                command.Parameters.AddWithValue("@idSesion", sesion.IdSesion);
                await command.ExecuteNonQueryAsync();
            }

            await using (var progresoCommand = connection.CreateCommand())
            {
                progresoCommand.Transaction = transaction;
                progresoCommand.CommandText = """
                    INSERT INTO progreso (id_usuario, id_rutina, fecha, porcentaje_completado, calorias_estimadas, tiempo_total_minutos, observacion)
                    VALUES (@idUsuario, @idRutina, @fecha, @porcentajeCompletado, @caloriasEstimadas, @tiempoTotalMinutos, @observacion);
                    """;
                progresoCommand.Parameters.AddWithValue("@idUsuario", sesion.IdUsuario);
                progresoCommand.Parameters.AddWithValue("@idRutina", sesion.IdRutina);
                progresoCommand.Parameters.AddWithValue("@fecha", DateOnly.FromDateTime((sesion.FechaFin ?? DateTime.UtcNow).Date).ToDateTime(TimeOnly.MinValue));
                progresoCommand.Parameters.AddWithValue("@porcentajeCompletado", request.PorcentajeCompletado);
                progresoCommand.Parameters.AddWithValue("@caloriasEstimadas", request.CaloriasEstimadas);
                progresoCommand.Parameters.AddWithValue("@tiempoTotalMinutos", request.TiempoTotalMinutos);
                progresoCommand.Parameters.AddWithValue("@observacion", request.Observacion);
                await progresoCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IReadOnlyList<SesionHistorialResponse>> GetHistorialUsuarioAsync(int idUsuario)
    {
        var sesiones = new List<SesionHistorialResponse>();

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT s.id_sesion, s.id_usuario, s.id_rutina, s.fecha_inicio, s.fecha_fin, s.estado, s.notas,
                   r.nombre AS rutina_nombre, r.descripcion AS rutina_descripcion, r.tipo_rutina, r.objetivo, r.categoria_imc, r.dificultad, r.es_publica, r.id_rutina_origen,
                   p.id_progreso, p.fecha AS progreso_fecha, p.porcentaje_completado, p.calorias_estimadas, p.tiempo_total_minutos, p.observacion
            FROM sesiones_entrenamiento s
            LEFT JOIN rutinas r ON r.id_rutina = s.id_rutina
            LEFT JOIN progreso p ON p.id_usuario = s.id_usuario AND p.id_rutina <=> s.id_rutina AND p.fecha = DATE(s.fecha_inicio)
            WHERE s.id_usuario = @idUsuario
            ORDER BY s.fecha_inicio DESC;
            """;
        command.Parameters.AddWithValue("@idUsuario", idUsuario);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            sesiones.Add(new SesionHistorialResponse
            {
                Sesion = MapSesion(reader),
                Rutina = reader.IsDBNull("rutina_nombre")
                    ? null
                    : new RutinaResumenResponse
                    {
                        IdRutina = reader.GetInt32("id_rutina"),
                        IdUsuario = null,
                        Nombre = reader.GetString("rutina_nombre"),
                        Descripcion = reader.GetString("rutina_descripcion"),
                        TipoRutina = reader.GetString("tipo_rutina"),
                        Objetivo = reader.GetString("objetivo"),
                        CategoriaImc = reader.IsDBNull("categoria_imc") ? null : reader.GetString("categoria_imc"),
                        Dificultad = reader.GetString("dificultad"),
                        EsPublica = reader.GetBoolean("es_publica"),
                        IdRutinaOrigen = reader.IsDBNull("id_rutina_origen") ? null : reader.GetInt32("id_rutina_origen")
                    },
                Progreso = reader.IsDBNull("id_progreso")
                    ? null
                    : new Progreso
                    {
                        IdProgreso = reader.GetInt32("id_progreso"),
                        IdUsuario = idUsuario,
                        IdRutina = reader.IsDBNull("id_rutina") ? null : reader.GetInt32("id_rutina"),
                        Fecha = DateOnly.FromDateTime(reader.GetDateTime("progreso_fecha")),
                        PorcentajeCompletado = reader.GetDecimal("porcentaje_completado"),
                        CaloriasEstimadas = reader.IsDBNull("calorias_estimadas") ? null : reader.GetDecimal("calorias_estimadas"),
                        TiempoTotalMinutos = reader.IsDBNull("tiempo_total_minutos") ? null : reader.GetInt32("tiempo_total_minutos"),
                        Observacion = reader.IsDBNull("observacion") ? null : reader.GetString("observacion")
                    }
            });
        }

        await reader.CloseAsync();

        foreach (var sesion in sesiones)
        {
            sesion.Detalles = (await GetDetallesSesionAsync(sesion.Sesion.IdSesion)).ToList();
        }

        return sesiones;
    }

    private async Task<IReadOnlyList<DetalleSesionEntrenamiento>> GetDetallesSesionAsync(int idSesion)
    {
        var detalles = new List<DetalleSesionEntrenamiento>();
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT d.id_detalle, d.id_sesion, d.id_ejercicio, e.nombre AS nombre_ejercicio, d.id_maquina, m.nombre AS nombre_maquina,
                   d.id_reserva, d.series_realizadas, d.repeticiones_realizadas, d.peso_usado_kg, d.duracion_minutos, d.esfuerzo_percibido, d.notas, d.fecha_creacion
            FROM detalle_sesion_entrenamiento d
            INNER JOIN ejercicios e ON e.id_ejercicio = d.id_ejercicio
            LEFT JOIN maquinas m ON m.id_maquina = d.id_maquina
            WHERE d.id_sesion = @idSesion
            ORDER BY d.id_detalle;
            """;
        command.Parameters.AddWithValue("@idSesion", idSesion);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            detalles.Add(new DetalleSesionEntrenamiento
            {
                IdDetalle = reader.GetInt32("id_detalle"),
                IdSesion = reader.GetInt32("id_sesion"),
                IdEjercicio = reader.GetInt32("id_ejercicio"),
                NombreEjercicio = reader.GetString("nombre_ejercicio"),
                IdMaquina = reader.IsDBNull("id_maquina") ? null : reader.GetInt32("id_maquina"),
                NombreMaquina = reader.IsDBNull("nombre_maquina") ? null : reader.GetString("nombre_maquina"),
                IdReserva = reader.IsDBNull("id_reserva") ? null : reader.GetInt32("id_reserva"),
                SeriesRealizadas = reader.IsDBNull("series_realizadas") ? null : reader.GetInt32("series_realizadas"),
                RepeticionesRealizadas = reader.IsDBNull("repeticiones_realizadas") ? null : reader.GetInt32("repeticiones_realizadas"),
                PesoUsadoKg = reader.IsDBNull("peso_usado_kg") ? null : reader.GetDecimal("peso_usado_kg"),
                DuracionMinutos = reader.IsDBNull("duracion_minutos") ? null : reader.GetInt32("duracion_minutos"),
                EsfuerzoPercibido = reader.IsDBNull("esfuerzo_percibido") ? null : reader.GetInt32("esfuerzo_percibido"),
                Notas = reader.IsDBNull("notas") ? null : reader.GetString("notas"),
                FechaCreacion = reader.GetDateTime("fecha_creacion")
            });
        }

        return detalles;
    }

    private async Task<DetalleSesionEntrenamiento?> GetDetalleAsync(int idDetalle)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT d.id_detalle, d.id_sesion, d.id_ejercicio, e.nombre AS nombre_ejercicio, d.id_maquina, m.nombre AS nombre_maquina,
                   d.id_reserva, d.series_realizadas, d.repeticiones_realizadas, d.peso_usado_kg, d.duracion_minutos, d.esfuerzo_percibido, d.notas, d.fecha_creacion
            FROM detalle_sesion_entrenamiento d
            INNER JOIN ejercicios e ON e.id_ejercicio = d.id_ejercicio
            LEFT JOIN maquinas m ON m.id_maquina = d.id_maquina
            WHERE d.id_detalle = @idDetalle
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@idDetalle", idDetalle);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new DetalleSesionEntrenamiento
        {
            IdDetalle = reader.GetInt32("id_detalle"),
            IdSesion = reader.GetInt32("id_sesion"),
            IdEjercicio = reader.GetInt32("id_ejercicio"),
            NombreEjercicio = reader.GetString("nombre_ejercicio"),
            IdMaquina = reader.IsDBNull("id_maquina") ? null : reader.GetInt32("id_maquina"),
            NombreMaquina = reader.IsDBNull("nombre_maquina") ? null : reader.GetString("nombre_maquina"),
            IdReserva = reader.IsDBNull("id_reserva") ? null : reader.GetInt32("id_reserva"),
            SeriesRealizadas = reader.IsDBNull("series_realizadas") ? null : reader.GetInt32("series_realizadas"),
            RepeticionesRealizadas = reader.IsDBNull("repeticiones_realizadas") ? null : reader.GetInt32("repeticiones_realizadas"),
            PesoUsadoKg = reader.IsDBNull("peso_usado_kg") ? null : reader.GetDecimal("peso_usado_kg"),
            DuracionMinutos = reader.IsDBNull("duracion_minutos") ? null : reader.GetInt32("duracion_minutos"),
            EsfuerzoPercibido = reader.IsDBNull("esfuerzo_percibido") ? null : reader.GetInt32("esfuerzo_percibido"),
            Notas = reader.IsDBNull("notas") ? null : reader.GetString("notas"),
            FechaCreacion = reader.GetDateTime("fecha_creacion")
        };
    }

    private static SesionEntrenamiento MapSesion(MySqlDataReader reader)
    {
        return new SesionEntrenamiento
        {
            IdSesion = reader.GetInt32("id_sesion"),
            IdUsuario = reader.GetInt32("id_usuario"),
            IdRutina = reader.IsDBNull("id_rutina") ? null : reader.GetInt32("id_rutina"),
            FechaInicio = reader.GetDateTime("fecha_inicio"),
            FechaFin = reader.IsDBNull("fecha_fin") ? null : reader.GetDateTime("fecha_fin"),
            Estado = reader.GetString("estado"),
            Notas = reader.IsDBNull("notas") ? null : reader.GetString("notas")
        };
    }
}
