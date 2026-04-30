using MySqlConnector;
using Softawer.Models;

namespace Softawer.Data;

public class UsuarioRepository(MySqlDataSource dataSource)
{
    public async Task<Usuario?> GetUsuarioAsync(int id)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, nombre, rut, correo, contrasena
            FROM usuarios
            WHERE id = @id
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapUsuario(reader) : null;
    }

    public async Task<IReadOnlyList<Usuario>> ListUsuariosAsync()
    {
        var usuarios = new List<Usuario>();

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, nombre, rut, correo, contrasena
            FROM usuarios
            ORDER BY id;
            """;

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            usuarios.Add(MapUsuario(reader));
        }

        return usuarios;
    }

    public async Task<Usuario?> AuthenticateAsync(string nombre, string password)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, nombre, rut, correo, contrasena
            FROM usuarios
            WHERE nombre = @nombre AND contrasena = @password
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@nombre", nombre);
        command.Parameters.AddWithValue("@password", password);

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
            INSERT INTO usuarios (nombre, rut, correo, contrasena)
            VALUES (@nombre, @rut, @correo, @contrasena);
            """;
        BindCommonParameters(command, usuario);

        await command.ExecuteNonQueryAsync();
        usuario.Id = Convert.ToInt32(command.LastInsertedId);
        return usuario;
    }

    public async Task<bool> UpdateUsuarioAsync(Usuario usuario)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE usuarios
            SET nombre = @nombre,
                rut = @rut,
                correo = @correo,
                contrasena = @contrasena
            WHERE id = @id;
            """;
        BindCommonParameters(command, usuario);
        command.Parameters.AddWithValue("@id", usuario.Id);

        var affectedRows = await command.ExecuteNonQueryAsync();
        return affectedRows > 0;
    }

    private static Usuario MapUsuario(MySqlDataReader reader)
    {
        return new Usuario
        {
            Id = reader.GetInt32("id"),
            Nombre = reader.GetString("nombre"),
            Rut = reader.GetString("rut"),
            Correo = reader.GetString("correo"),
            Contrasena = reader.GetString("contrasena")
        };
    }

    private static void BindCommonParameters(MySqlCommand command, Usuario usuario)
    {
        command.Parameters.AddWithValue("@nombre", usuario.Nombre);
        command.Parameters.AddWithValue("@rut", usuario.Rut);
        command.Parameters.AddWithValue("@correo", usuario.Correo);
        command.Parameters.AddWithValue("@contrasena", usuario.Contrasena);
    }
}
