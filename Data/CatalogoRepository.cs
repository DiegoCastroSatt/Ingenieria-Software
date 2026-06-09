using MySqlConnector;
using Softawer.Models;

namespace Softawer.Data;

public class CatalogoRepository(MySqlDataSource dataSource)
{
    public async Task<IReadOnlyList<Maquina>> ListMaquinasAsync()
    {
        var maquinas = new List<Maquina>();
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT m.id_maquina, m.id_tipo_maquina, tm.nombre AS tipo_maquina, m.nombre, m.descripcion,
                   m.musculos_objetivo, m.imagen_url, m.ubicacion, m.estado, m.cantidad
            FROM maquinas m
            INNER JOIN tipos_maquina tm ON tm.id_tipo_maquina = m.id_tipo_maquina
            ORDER BY tm.nombre, m.nombre;
            """;
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            maquinas.Add(new Maquina
            {
                IdMaquina = reader.GetInt32("id_maquina"),
                IdTipoMaquina = reader.GetInt32("id_tipo_maquina"),
                TipoMaquina = reader.GetString("tipo_maquina"),
                Nombre = reader.GetString("nombre"),
                Descripcion = reader.GetString("descripcion"),
                MusculosObjetivo = reader.GetString("musculos_objetivo"),
                ImagenUrl = reader.GetString("imagen_url"),
                Ubicacion = reader.GetString("ubicacion"),
                Estado = reader.GetString("estado"),
                Cantidad = reader.GetInt32("cantidad")
            });
        }

        return maquinas;
    }

    public async Task<Maquina?> GetMaquinaAsync(int idMaquina)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT m.id_maquina, m.id_tipo_maquina, tm.nombre AS tipo_maquina, m.nombre, m.descripcion,
                   m.musculos_objetivo, m.imagen_url, m.ubicacion, m.estado, m.cantidad
            FROM maquinas m
            INNER JOIN tipos_maquina tm ON tm.id_tipo_maquina = m.id_tipo_maquina
            WHERE m.id_maquina = @idMaquina
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("@idMaquina", idMaquina);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Maquina
        {
            IdMaquina = reader.GetInt32("id_maquina"),
            IdTipoMaquina = reader.GetInt32("id_tipo_maquina"),
            TipoMaquina = reader.GetString("tipo_maquina"),
            Nombre = reader.GetString("nombre"),
            Descripcion = reader.GetString("descripcion"),
            MusculosObjetivo = reader.GetString("musculos_objetivo"),
            ImagenUrl = reader.GetString("imagen_url"),
            Ubicacion = reader.GetString("ubicacion"),
            Estado = reader.GetString("estado"),
            Cantidad = reader.GetInt32("cantidad")
        };
    }

    public async Task<IReadOnlyList<Maquina>> ListMaquinasFavoritasAsync(int idUsuario)
    {
        var maquinas = new List<Maquina>();
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT m.id_maquina, m.id_tipo_maquina, tm.nombre AS tipo_maquina, m.nombre, m.descripcion,
                   m.musculos_objetivo, m.imagen_url, m.ubicacion, m.estado, m.cantidad
            FROM usuario_maquina_favorita umf
            INNER JOIN maquinas m ON m.id_maquina = umf.id_maquina
            INNER JOIN tipos_maquina tm ON tm.id_tipo_maquina = m.id_tipo_maquina
            WHERE umf.id_usuario = @idUsuario
            ORDER BY umf.fecha_creacion DESC, tm.nombre, m.nombre;
            """;
        command.Parameters.AddWithValue("@idUsuario", idUsuario);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            maquinas.Add(ReadMaquina(reader));
        }

        return maquinas;
    }

    public async Task<bool> AddMaquinaFavoritaAsync(int idUsuario, int idMaquina)
    {
        if (!await ExistsUsuarioAsync(idUsuario) || await GetMaquinaAsync(idMaquina) is null)
        {
            return false;
        }

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT IGNORE INTO usuario_maquina_favorita (id_usuario, id_maquina)
            VALUES (@idUsuario, @idMaquina);
            """;
        command.Parameters.AddWithValue("@idUsuario", idUsuario);
        command.Parameters.AddWithValue("@idMaquina", idMaquina);
        await command.ExecuteNonQueryAsync();
        return true;
    }

    public async Task<bool> RemoveMaquinaFavoritaAsync(int idUsuario, int idMaquina)
    {
        if (!await ExistsUsuarioAsync(idUsuario) || await GetMaquinaAsync(idMaquina) is null)
        {
            return false;
        }

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM usuario_maquina_favorita
            WHERE id_usuario = @idUsuario AND id_maquina = @idMaquina;
            """;
        command.Parameters.AddWithValue("@idUsuario", idUsuario);
        command.Parameters.AddWithValue("@idMaquina", idMaquina);
        await command.ExecuteNonQueryAsync();
        return true;
    }

    public async Task<IReadOnlyList<Ejercicio>> ListEjerciciosAsync()
    {
        var ejercicios = new List<Ejercicio>();
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT e.id_ejercicio, e.nombre, e.descripcion, e.grupo_muscular, e.dificultad, e.id_maquina, m.nombre AS nombre_maquina
            FROM ejercicios e
            LEFT JOIN maquinas m ON m.id_maquina = e.id_maquina
            ORDER BY e.nombre;
            """;
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ejercicios.Add(new Ejercicio
            {
                IdEjercicio = reader.GetInt32("id_ejercicio"),
                Nombre = reader.GetString("nombre"),
                Descripcion = reader.GetString("descripcion"),
                GrupoMuscular = reader.GetString("grupo_muscular"),
                Dificultad = reader.GetString("dificultad"),
                IdMaquina = reader.IsDBNull("id_maquina") ? null : reader.GetInt32("id_maquina"),
                NombreMaquina = reader.IsDBNull("nombre_maquina") ? null : reader.GetString("nombre_maquina")
            });
        }

        return ejercicios;
    }

    private async Task<bool> ExistsUsuarioAsync(int idUsuario)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT EXISTS(SELECT 1 FROM usuarios WHERE id_usuario = @idUsuario);";
        command.Parameters.AddWithValue("@idUsuario", idUsuario);
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) == 1;
    }

    private static Maquina ReadMaquina(MySqlDataReader reader)
    {
        return new Maquina
        {
            IdMaquina = reader.GetInt32("id_maquina"),
            IdTipoMaquina = reader.GetInt32("id_tipo_maquina"),
            TipoMaquina = reader.GetString("tipo_maquina"),
            Nombre = reader.GetString("nombre"),
            Descripcion = reader.GetString("descripcion"),
            MusculosObjetivo = reader.GetString("musculos_objetivo"),
            ImagenUrl = reader.GetString("imagen_url"),
            Ubicacion = reader.GetString("ubicacion"),
            Estado = reader.GetString("estado"),
            Cantidad = reader.GetInt32("cantidad")
        };
    }
}
