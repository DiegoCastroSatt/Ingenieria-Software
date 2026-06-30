using MySqlConnector;
using Softawer.Models;

namespace Softawer.Data;

public class UsuarioRepository(MySqlDataSource dataSource)
{
    public async Task<Usuario?> GetUsuarioAsync(int idUsuario)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id_usuario, nombre, rut, correo, contrasena_hash, rol, fecha_creacion, fecha_actualizacion
            FROM usuarios
            WHERE id_usuario = @idUsuario
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@idUsuario", idUsuario);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapUsuario(reader) : null;
    }

    public async Task<Usuario?> GetByCorreoAsync(string correo)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id_usuario, nombre, rut, correo, contrasena_hash, rol, fecha_creacion, fecha_actualizacion
            FROM usuarios
            WHERE correo = @correo
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@correo", correo);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapUsuario(reader) : null;
    }

    public async Task<Usuario?> GetByLoginIdentifierAsync(string identificador)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id_usuario, nombre, rut, correo, edad, contrasena_hash, rol, fecha_creacion, fecha_actualizacion
            FROM usuarios
            WHERE nombre = @identificador OR correo = @identificador
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@identificador", identificador);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapUsuario(reader) : null;
    }

    public async Task<bool> ExistsByCorreoOrRutAsync(string correo, string rut)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM usuarios
            WHERE correo = @correo OR rut = @rut;
            """;
        command.Parameters.AddWithValue("@correo", correo);
        command.Parameters.AddWithValue("@rut", rut);
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task<Usuario> CreateUsuarioAsync(Usuario usuario)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO usuarios (nombre, rut, correo, edad, contrasena_hash, rol)
            VALUES (@nombre, @rut, @correo, @contrasenaHash, @rol);
            """;
        command.Parameters.AddWithValue("@nombre", usuario.Nombre);
        command.Parameters.AddWithValue("@rut", usuario.Rut);
        command.Parameters.AddWithValue("@correo", usuario.Correo);
        command.Parameters.AddWithValue("@edad", usuario.Edad);
        command.Parameters.AddWithValue("@contrasenaHash", usuario.ContrasenaHash);
        command.Parameters.AddWithValue("@rol", usuario.Rol);
        await command.ExecuteNonQueryAsync();

        usuario.IdUsuario = Convert.ToInt32(command.LastInsertedId);
        return (await GetUsuarioAsync(usuario.IdUsuario))!;
    }

    private static Usuario MapUsuario(MySqlDataReader reader)
    {
        return new Usuario
        {
            IdUsuario = reader.GetInt32("id_usuario"),
            Nombre = reader.GetString("nombre"),
            Rut = reader.GetString("rut"),
            Correo = reader.GetString("correo"),
            Edad = reader.GetString("edad"),
            ContrasenaHash = reader.GetString("contrasena_hash"),
            Rol = reader.GetString("rol"),
            FechaCreacion = reader.GetDateTime("fecha_creacion"),
            FechaActualizacion = reader.GetDateTime("fecha_actualizacion")
        };
    }
}
