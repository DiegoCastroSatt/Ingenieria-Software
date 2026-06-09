using MySqlConnector;
using Softawer.Models;

namespace Softawer.Data;

public class PerfilUsuarioRepository(MySqlDataSource dataSource)
{
    public async Task<PerfilUsuario?> GetPerfilAsync(int idUsuario)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id_perfil, id_usuario, fecha_nacimiento, sexo, altura_cm, peso_kg, objetivo, nivel_actividad,
                   alias, avatar_url, telefono_trabajo, email_trabajo, sitio_personal, twitter, fecha_actualizacion
            FROM perfil_usuario
            WHERE id_usuario = @idUsuario
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@idUsuario", idUsuario);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapPerfil(reader) : null;
    }

    public async Task<PerfilUsuario> UpsertPerfilAsync(int idUsuario, ActualizarPerfilImcRequest request)
    {
        var existente = await GetPerfilAsync(idUsuario);

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();

        if (existente is null)
        {
            command.CommandText = """
                INSERT INTO perfil_usuario (id_usuario, fecha_nacimiento, sexo, altura_cm, peso_kg, objetivo, nivel_actividad)
                VALUES (@idUsuario, @fechaNacimiento, @sexo, @alturaCm, @pesoKg, @objetivo, @nivelActividad);
                """;
        }
        else
        {
            command.CommandText = """
                UPDATE perfil_usuario
                SET fecha_nacimiento = @fechaNacimiento,
                    sexo = @sexo,
                    altura_cm = @alturaCm,
                    peso_kg = @pesoKg,
                    objetivo = @objetivo,
                    nivel_actividad = @nivelActividad
                WHERE id_usuario = @idUsuario;
                """;
        }

        command.Parameters.AddWithValue("@idUsuario", idUsuario);
        command.Parameters.AddWithValue("@fechaNacimiento", request.FechaNacimiento.HasValue ? request.FechaNacimiento.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value);
        command.Parameters.AddWithValue("@sexo", ToDbValue(request.Sexo));
        command.Parameters.AddWithValue("@alturaCm", request.AlturaCm);
        command.Parameters.AddWithValue("@pesoKg", request.PesoKg);
        command.Parameters.AddWithValue("@objetivo", ToDbValue(request.Objetivo));
        command.Parameters.AddWithValue("@nivelActividad", ToDbValue(request.NivelActividad));
        await command.ExecuteNonQueryAsync();

        return (await GetPerfilAsync(idUsuario))!;
    }

    public async Task<HistorialImc> AddHistorialImcAsync(int idUsuario, decimal alturaCm, decimal pesoKg, decimal imc, string categoriaImc)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO historial_imc (id_usuario, altura_cm, peso_kg, imc, categoria_imc)
            VALUES (@idUsuario, @alturaCm, @pesoKg, @imc, @categoriaImc);
            """;
        command.Parameters.AddWithValue("@idUsuario", idUsuario);
        command.Parameters.AddWithValue("@alturaCm", alturaCm);
        command.Parameters.AddWithValue("@pesoKg", pesoKg);
        command.Parameters.AddWithValue("@imc", imc);
        command.Parameters.AddWithValue("@categoriaImc", categoriaImc);
        await command.ExecuteNonQueryAsync();

        var idImc = Convert.ToInt32(command.LastInsertedId);
        return await GetHistorialImcAsync(idImc) ?? throw new InvalidOperationException("No se pudo leer el historial IMC insertado.");
    }

    public async Task<HistorialImc?> GetHistorialImcAsync(int idImc)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id_imc, id_usuario, altura_cm, peso_kg, imc, categoria_imc, fecha_registro
            FROM historial_imc
            WHERE id_imc = @idImc
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@idImc", idImc);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapImc(reader) : null;
    }

    public async Task<IReadOnlyList<HistorialImc>> ListHistorialImcUsuarioAsync(int idUsuario)
    {
        var historial = new List<HistorialImc>();
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id_imc, id_usuario, altura_cm, peso_kg, imc, categoria_imc, fecha_registro
            FROM historial_imc
            WHERE id_usuario = @idUsuario
            ORDER BY fecha_registro DESC, id_imc DESC;
            """;
        command.Parameters.AddWithValue("@idUsuario", idUsuario);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            historial.Add(MapImc(reader));
        }

        return historial;
    }

    public async Task<string?> GetCategoriaImcActualAsync(int idUsuario)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT categoria_imc
            FROM historial_imc
            WHERE id_usuario = @idUsuario
            ORDER BY fecha_registro DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@idUsuario", idUsuario);
        return (string?)await command.ExecuteScalarAsync();
    }

    public async Task<PerfilUsuario?> UpsertInformacionPublicaAsync(int idUsuario, ActualizarInformacionPublicaRequest request)
    {
        var existente = await GetPerfilAsync(idUsuario);

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();

        if (existente is null)
        {
            command.CommandText = """
                INSERT INTO perfil_usuario (id_usuario, alias, avatar_url, telefono_trabajo, email_trabajo, sitio_personal, twitter)
                VALUES (@idUsuario, @alias, @avatarUrl, @telefonoTrabajo, @emailTrabajo, @sitioPersonal, @twitter);
                """;
        }
        else
        {
            command.CommandText = """
                UPDATE perfil_usuario
                SET alias = @alias,
                    avatar_url = @avatarUrl,
                    telefono_trabajo = @telefonoTrabajo,
                    email_trabajo = @emailTrabajo,
                    sitio_personal = @sitioPersonal,
                    twitter = @twitter
                WHERE id_usuario = @idUsuario;
                """;
        }

        command.Parameters.AddWithValue("@idUsuario", idUsuario);
        command.Parameters.AddWithValue("@alias", ToDbValue(request.Alias));
        command.Parameters.AddWithValue("@avatarUrl", ToDbValue(request.AvatarUrl));
        command.Parameters.AddWithValue("@telefonoTrabajo", ToDbValue(request.TelefonoTrabajo));
        command.Parameters.AddWithValue("@emailTrabajo", ToDbValue(request.EmailTrabajo));
        command.Parameters.AddWithValue("@sitioPersonal", ToDbValue(request.SitioPersonal));
        command.Parameters.AddWithValue("@twitter", ToDbValue(request.Twitter));
        await command.ExecuteNonQueryAsync();

        return (await GetPerfilAsync(idUsuario));
    }

    private static PerfilUsuario MapPerfil(MySqlDataReader reader)
    {
        return new PerfilUsuario
        {
            IdPerfil = reader.GetInt32("id_perfil"),
            IdUsuario = reader.GetInt32("id_usuario"),
            FechaNacimiento = reader.IsDBNull("fecha_nacimiento")
                ? null
                : DateOnly.FromDateTime(reader.GetDateTime("fecha_nacimiento")),
            Sexo = reader.IsDBNull("sexo") ? null : reader.GetString("sexo"),
            AlturaCm = reader.IsDBNull("altura_cm") ? null : reader.GetDecimal("altura_cm"),
            PesoKg = reader.IsDBNull("peso_kg") ? null : reader.GetDecimal("peso_kg"),
            Objetivo = reader.IsDBNull("objetivo") ? null : reader.GetString("objetivo"),
            NivelActividad = reader.IsDBNull("nivel_actividad") ? null : reader.GetString("nivel_actividad"),
            Alias = reader.IsDBNull("alias") ? null : reader.GetString("alias"),
            AvatarUrl = reader.IsDBNull("avatar_url") ? null : reader.GetString("avatar_url"),
            TelefonoTrabajo = reader.IsDBNull("telefono_trabajo") ? null : reader.GetString("telefono_trabajo"),
            EmailTrabajo = reader.IsDBNull("email_trabajo") ? null : reader.GetString("email_trabajo"),
            SitioPersonal = reader.IsDBNull("sitio_personal") ? null : reader.GetString("sitio_personal"),
            Twitter = reader.IsDBNull("twitter") ? null : reader.GetString("twitter"),
            FechaActualizacion = reader.GetDateTime("fecha_actualizacion")
        };
    }

    private static HistorialImc MapImc(MySqlDataReader reader)
    {
        return new HistorialImc
        {
            IdImc = reader.GetInt32("id_imc"),
            IdUsuario = reader.GetInt32("id_usuario"),
            AlturaCm = reader.GetDecimal("altura_cm"),
            PesoKg = reader.GetDecimal("peso_kg"),
            Imc = reader.GetDecimal("imc"),
            CategoriaImc = reader.GetString("categoria_imc"),
            FechaRegistro = reader.GetDateTime("fecha_registro")
        };
    }

    private static object ToDbValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
    }
}
