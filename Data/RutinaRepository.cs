using MySqlConnector;
using Softawer.Models;

namespace Softawer.Data;

public class RutinaRepository(MySqlDataSource dataSource)
{
    public async Task<IReadOnlyList<RutinaResumenResponse>> ListRutinasPredefinidasAsync()
    {
        return await ListRutinasAsync("""
            SELECT id_rutina, id_usuario, nombre, descripcion, tipo_rutina, objetivo, categoria_imc, dificultad, es_publica, id_rutina_origen
            FROM rutinas
            WHERE tipo_rutina = 'predefinida' AND es_publica = TRUE
            ORDER BY nombre;
            """);
    }

    public async Task<IReadOnlyList<RutinaResumenResponse>> ListRutinasRecomendadasAsync(string categoriaImc)
    {
        return await ListRutinasAsync("""
            SELECT id_rutina, id_usuario, nombre, descripcion, tipo_rutina, objetivo, categoria_imc, dificultad, es_publica, id_rutina_origen
            FROM rutinas
            WHERE tipo_rutina = 'predefinida'
              AND es_publica = TRUE
              AND categoria_imc = @categoriaImc
            ORDER BY nombre;
            """, command => command.Parameters.AddWithValue("@categoriaImc", categoriaImc));
    }

    public async Task<IReadOnlyList<RutinaResumenResponse>> ListRutinasUsuarioAsync(int idUsuario)
    {
        return await ListRutinasAsync("""
            SELECT id_rutina, id_usuario, nombre, descripcion, tipo_rutina, objetivo, categoria_imc, dificultad, es_publica, id_rutina_origen
            FROM rutinas
            WHERE id_usuario = @idUsuario OR (id_usuario IS NULL AND tipo_rutina = 'predefinida' AND es_publica = TRUE)
            ORDER BY nombre;
            """, command => command.Parameters.AddWithValue("@idUsuario", idUsuario));
    }

    public async Task<RutinaDetalleResponse?> GetRutinaDetalleAsync(int idRutina)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id_rutina, id_usuario, nombre, descripcion, tipo_rutina, objetivo, categoria_imc, dificultad, es_publica, id_rutina_origen
            FROM rutinas
            WHERE id_rutina = @idRutina
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@idRutina", idRutina);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        var rutina = MapRutina(reader);
        await reader.CloseAsync();
        rutina.Ejercicios = (await ListEjerciciosRutinaAsync(idRutina)).ToList();
        return rutina;
    }

    public async Task<int> CreateRutinaAsync(Rutina rutina, IReadOnlyList<RutinaEjercicioRequest> ejercicios)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var idRutina = await InsertRutinaAsync(connection, transaction, rutina);
            await ReplaceRutinaExercisesAsync(connection, transaction, idRutina, ejercicios);
            await transaction.CommitAsync();
            return idRutina;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<RutinaDetalleResponse> CopyRutinaAsync(RutinaDetalleResponse copia, bool activarRutina)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var idRutina = await InsertRutinaAsync(connection, transaction, new Rutina
            {
                IdUsuario = copia.IdUsuario,
                Nombre = copia.Nombre,
                Descripcion = copia.Descripcion,
                TipoRutina = copia.TipoRutina,
                Objetivo = copia.Objetivo,
                CategoriaImc = copia.CategoriaImc,
                Dificultad = copia.Dificultad,
                EsPublica = copia.EsPublica,
                IdRutinaOrigen = copia.IdRutinaOrigen
            });

            var ejercicios = copia.Ejercicios.Select(ejercicio => new RutinaEjercicioRequest
            {
                IdEjercicio = ejercicio.IdEjercicio,
                Dia = ejercicio.Dia,
                Orden = ejercicio.Orden,
                Series = ejercicio.Series,
                Repeticiones = ejercicio.Repeticiones,
                DuracionMinutos = ejercicio.DuracionMinutos,
                DescansoSegundos = ejercicio.DescansoSegundos,
                Notas = ejercicio.Notas
            }).ToList();

            await ReplaceRutinaExercisesAsync(connection, transaction, idRutina, ejercicios);

            if (activarRutina && copia.IdUsuario.HasValue)
            {
                await CreateUsuarioRutinaAsync(connection, transaction, copia.IdUsuario.Value, idRutina, "activa");
            }

            await transaction.CommitAsync();
            return (await GetRutinaDetalleAsync(idRutina))!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<RutinaDetalleResponse?> UpdateRutinaAsync(int idRutina, ActualizarRutinaRequest request)
    {
        var existente = await GetRutinaDetalleAsync(idRutina);
        if (existente is null)
        {
            return null;
        }

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = """
                    UPDATE rutinas
                    SET nombre = @nombre,
                        descripcion = @descripcion,
                        objetivo = @objetivo,
                        dificultad = @dificultad,
                        es_publica = @esPublica
                    WHERE id_rutina = @idRutina;
                    """;
                command.Parameters.AddWithValue("@idRutina", idRutina);
                command.Parameters.AddWithValue("@nombre", request.Nombre ?? existente.Nombre);
                command.Parameters.AddWithValue("@descripcion", request.Descripcion ?? existente.Descripcion);
                command.Parameters.AddWithValue("@objetivo", request.Objetivo ?? existente.Objetivo);
                command.Parameters.AddWithValue("@dificultad", request.Dificultad ?? existente.Dificultad);
                command.Parameters.AddWithValue("@esPublica", request.EsPublica ?? existente.EsPublica);
                await command.ExecuteNonQueryAsync();
            }

            await ReplaceRutinaExercisesAsync(connection, transaction, idRutina, request.Ejercicios);
            await transaction.CommitAsync();
            return await GetRutinaDetalleAsync(idRutina);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteRutinaAsync(int idRutina, int idUsuario)
    {
        if (!await UsuarioPuedeEditarRutinaAsync(idRutina, idUsuario))
        {
            return false;
        }

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await using (var usuarioRutinaCommand = connection.CreateCommand())
            {
                usuarioRutinaCommand.Transaction = transaction;
                usuarioRutinaCommand.CommandText = "DELETE FROM usuario_rutina WHERE id_rutina = @idRutina AND id_usuario = @idUsuario;";
                usuarioRutinaCommand.Parameters.AddWithValue("@idRutina", idRutina);
                usuarioRutinaCommand.Parameters.AddWithValue("@idUsuario", idUsuario);
                await usuarioRutinaCommand.ExecuteNonQueryAsync();
            }

            await using (var ejerciciosCommand = connection.CreateCommand())
            {
                ejerciciosCommand.Transaction = transaction;
                ejerciciosCommand.CommandText = "DELETE FROM rutina_ejercicio WHERE id_rutina = @idRutina;";
                ejerciciosCommand.Parameters.AddWithValue("@idRutina", idRutina);
                await ejerciciosCommand.ExecuteNonQueryAsync();
            }

            await using (var sesionesCommand = connection.CreateCommand())
            {
                sesionesCommand.Transaction = transaction;
                sesionesCommand.CommandText = "UPDATE sesiones_entrenamiento SET id_rutina = NULL WHERE id_rutina = @idRutina AND id_usuario = @idUsuario;";
                sesionesCommand.Parameters.AddWithValue("@idRutina", idRutina);
                sesionesCommand.Parameters.AddWithValue("@idUsuario", idUsuario);
                await sesionesCommand.ExecuteNonQueryAsync();
            }

            await using (var progresoCommand = connection.CreateCommand())
            {
                progresoCommand.Transaction = transaction;
                progresoCommand.CommandText = "UPDATE progreso SET id_rutina = NULL WHERE id_rutina = @idRutina AND id_usuario = @idUsuario;";
                progresoCommand.Parameters.AddWithValue("@idRutina", idRutina);
                progresoCommand.Parameters.AddWithValue("@idUsuario", idUsuario);
                await progresoCommand.ExecuteNonQueryAsync();
            }

            await using (var rutinaCommand = connection.CreateCommand())
            {
                rutinaCommand.Transaction = transaction;
                rutinaCommand.CommandText = """
                    DELETE FROM rutinas
                    WHERE id_rutina = @idRutina
                      AND id_usuario = @idUsuario
                      AND tipo_rutina IN ('personalizada', 'copiada');
                    """;
                rutinaCommand.Parameters.AddWithValue("@idRutina", idRutina);
                rutinaCommand.Parameters.AddWithValue("@idUsuario", idUsuario);
                var affectedRows = await rutinaCommand.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
                return affectedRows > 0;
            }
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UsuarioPuedeEditarRutinaAsync(int idRutina, int idUsuario)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM rutinas
            WHERE id_rutina = @idRutina
              AND id_usuario = @idUsuario
              AND tipo_rutina IN ('personalizada', 'copiada');
            """;
        command.Parameters.AddWithValue("@idRutina", idRutina);
        command.Parameters.AddWithValue("@idUsuario", idUsuario);
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    private async Task<IReadOnlyList<RutinaResumenResponse>> ListRutinasAsync(string sql, Action<MySqlCommand>? configure = null)
    {
        var rutinas = new List<RutinaResumenResponse>();
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        configure?.Invoke(command);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rutinas.Add(new RutinaResumenResponse
            {
                IdRutina = reader.GetInt32("id_rutina"),
                IdUsuario = reader.IsDBNull("id_usuario") ? null : reader.GetInt32("id_usuario"),
                Nombre = reader.GetString("nombre"),
                Descripcion = reader.GetString("descripcion"),
                TipoRutina = reader.GetString("tipo_rutina"),
                Objetivo = reader.GetString("objetivo"),
                CategoriaImc = reader.IsDBNull("categoria_imc") ? null : reader.GetString("categoria_imc"),
                Dificultad = reader.GetString("dificultad"),
                EsPublica = reader.GetBoolean("es_publica"),
                IdRutinaOrigen = reader.IsDBNull("id_rutina_origen") ? null : reader.GetInt32("id_rutina_origen")
            });
        }

        return rutinas;
    }

    private async Task<IReadOnlyList<RutinaEjercicio>> ListEjerciciosRutinaAsync(int idRutina)
    {
        var ejercicios = new List<RutinaEjercicio>();
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT re.id_rutina_ejercicio, re.id_rutina, re.id_ejercicio, re.dia, re.orden, re.series, re.repeticiones,
                   re.duracion_minutos, re.descanso_segundos, re.notas,
                   e.nombre AS nombre_ejercicio, e.descripcion AS descripcion_ejercicio, e.grupo_muscular
            FROM rutina_ejercicio re
            INNER JOIN ejercicios e ON e.id_ejercicio = re.id_ejercicio
            WHERE re.id_rutina = @idRutina
            ORDER BY re.dia, re.orden;
            """;
        command.Parameters.AddWithValue("@idRutina", idRutina);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ejercicios.Add(new RutinaEjercicio
            {
                IdRutinaEjercicio = reader.GetInt32("id_rutina_ejercicio"),
                IdRutina = reader.GetInt32("id_rutina"),
                IdEjercicio = reader.GetInt32("id_ejercicio"),
                Dia = reader.GetString("dia"),
                Orden = reader.GetInt32("orden"),
                Series = reader.IsDBNull("series") ? null : reader.GetInt32("series"),
                Repeticiones = reader.IsDBNull("repeticiones") ? null : reader.GetInt32("repeticiones"),
                DuracionMinutos = reader.IsDBNull("duracion_minutos") ? null : reader.GetInt32("duracion_minutos"),
                DescansoSegundos = reader.IsDBNull("descanso_segundos") ? null : reader.GetInt32("descanso_segundos"),
                Notas = reader.IsDBNull("notas") ? null : reader.GetString("notas"),
                NombreEjercicio = reader.GetString("nombre_ejercicio"),
                DescripcionEjercicio = reader.GetString("descripcion_ejercicio"),
                GrupoMuscular = reader.GetString("grupo_muscular")
            });
        }

        return ejercicios;
    }

    private static RutinaDetalleResponse MapRutina(MySqlDataReader reader)
    {
        return new RutinaDetalleResponse
        {
            IdRutina = reader.GetInt32("id_rutina"),
            IdUsuario = reader.IsDBNull("id_usuario") ? null : reader.GetInt32("id_usuario"),
            Nombre = reader.GetString("nombre"),
            Descripcion = reader.GetString("descripcion"),
            TipoRutina = reader.GetString("tipo_rutina"),
            Objetivo = reader.GetString("objetivo"),
            CategoriaImc = reader.IsDBNull("categoria_imc") ? null : reader.GetString("categoria_imc"),
            Dificultad = reader.GetString("dificultad"),
            EsPublica = reader.GetBoolean("es_publica"),
            IdRutinaOrigen = reader.IsDBNull("id_rutina_origen") ? null : reader.GetInt32("id_rutina_origen")
        };
    }

    private static async Task<int> InsertRutinaAsync(MySqlConnection connection, MySqlTransaction transaction, Rutina rutina)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO rutinas (id_usuario, nombre, descripcion, tipo_rutina, objetivo, categoria_imc, dificultad, es_publica, id_rutina_origen)
            VALUES (@idUsuario, @nombre, @descripcion, @tipoRutina, @objetivo, @categoriaImc, @dificultad, @esPublica, @idRutinaOrigen);
            """;
        command.Parameters.AddWithValue("@idUsuario", rutina.IdUsuario);
        command.Parameters.AddWithValue("@nombre", rutina.Nombre);
        command.Parameters.AddWithValue("@descripcion", rutina.Descripcion);
        command.Parameters.AddWithValue("@tipoRutina", rutina.TipoRutina);
        command.Parameters.AddWithValue("@objetivo", rutina.Objetivo);
        command.Parameters.AddWithValue("@categoriaImc", rutina.CategoriaImc);
        command.Parameters.AddWithValue("@dificultad", rutina.Dificultad);
        command.Parameters.AddWithValue("@esPublica", rutina.EsPublica);
        command.Parameters.AddWithValue("@idRutinaOrigen", rutina.IdRutinaOrigen);
        await command.ExecuteNonQueryAsync();
        return Convert.ToInt32(command.LastInsertedId);
    }

    private static async Task ReplaceRutinaExercisesAsync(MySqlConnection connection, MySqlTransaction transaction, int idRutina, IReadOnlyList<RutinaEjercicioRequest> ejercicios)
    {
        await using (var deleteCommand = connection.CreateCommand())
        {
            deleteCommand.Transaction = transaction;
            deleteCommand.CommandText = "DELETE FROM rutina_ejercicio WHERE id_rutina = @idRutina;";
            deleteCommand.Parameters.AddWithValue("@idRutina", idRutina);
            await deleteCommand.ExecuteNonQueryAsync();
        }

        foreach (var ejercicio in ejercicios)
        {
            await using var insertCommand = connection.CreateCommand();
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = """
                INSERT INTO rutina_ejercicio (id_rutina, id_ejercicio, dia, orden, series, repeticiones, duracion_minutos, descanso_segundos, notas)
                VALUES (@idRutina, @idEjercicio, @dia, @orden, @series, @repeticiones, @duracionMinutos, @descansoSegundos, @notas);
                """;
            insertCommand.Parameters.AddWithValue("@idRutina", idRutina);
            insertCommand.Parameters.AddWithValue("@idEjercicio", ejercicio.IdEjercicio);
            insertCommand.Parameters.AddWithValue("@dia", ejercicio.Dia);
            insertCommand.Parameters.AddWithValue("@orden", ejercicio.Orden);
            insertCommand.Parameters.AddWithValue("@series", ejercicio.Series);
            insertCommand.Parameters.AddWithValue("@repeticiones", ejercicio.Repeticiones);
            insertCommand.Parameters.AddWithValue("@duracionMinutos", ejercicio.DuracionMinutos);
            insertCommand.Parameters.AddWithValue("@descansoSegundos", ejercicio.DescansoSegundos);
            insertCommand.Parameters.AddWithValue("@notas", ejercicio.Notas);
            await insertCommand.ExecuteNonQueryAsync();
        }
    }

    private static async Task CreateUsuarioRutinaAsync(MySqlConnection connection, MySqlTransaction transaction, int idUsuario, int idRutina, string estado)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO usuario_rutina (id_usuario, id_rutina, estado)
            VALUES (@idUsuario, @idRutina, @estado);
            """;
        command.Parameters.AddWithValue("@idUsuario", idUsuario);
        command.Parameters.AddWithValue("@idRutina", idRutina);
        command.Parameters.AddWithValue("@estado", estado);
        await command.ExecuteNonQueryAsync();
    }
}
