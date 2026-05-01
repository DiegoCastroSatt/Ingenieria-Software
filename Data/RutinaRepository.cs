using MySqlConnector;
using Softawer.Models;

namespace Softawer.Data;

public class RutinaRepository(MySqlDataSource dataSource)
{
    public async Task<IReadOnlyList<Rutina>> ListRutinasAsync()
    {
        var rutinas = new List<Rutina>();

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT id, nombre, descripcion
            FROM rutinas
            ORDER BY nombre;
            """;

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rutinas.Add(MapRutina(reader));
        }

        return rutinas;
    }

    public async Task<IReadOnlyList<UsuarioRutina>> ListRutinasUsuarioAsync(int usuarioId)
    {
        var usuarioRutinas = new List<UsuarioRutina>();

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT ur.id, ur.id_usuario, ur.id_rutina, ur.fecha_inicio,
                   r.nombre AS rutina_nombre,
                   r.descripcion AS rutina_descripcion
            FROM usuario_rutina ur
            JOIN rutinas r ON ur.id_rutina = r.id
            WHERE ur.id_usuario = @usuarioId
            ORDER BY ur.fecha_inicio DESC;
            """;
        command.Parameters.AddWithValue("@usuarioId", usuarioId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            usuarioRutinas.Add(MapUsuarioRutina(reader));
        }

        return usuarioRutinas;
    }

    public async Task<UsuarioRutina> AssignRutinaAsync(UsuarioRutina usuarioRutina)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO usuario_rutina (id_usuario, id_rutina, fecha_inicio)
            VALUES (@idUsuario, @idRutina, @fechaInicio);
            """;
        command.Parameters.AddWithValue("@idUsuario", usuarioRutina.IdUsuario);
        command.Parameters.AddWithValue("@idRutina", usuarioRutina.IdRutina);
        command.Parameters.AddWithValue("@fechaInicio", usuarioRutina.FechaInicio.Date);

        await command.ExecuteNonQueryAsync();
        usuarioRutina.Id = Convert.ToInt32(command.LastInsertedId);
        return usuarioRutina;
    }

    public async Task<IReadOnlyList<Progreso>> ListProgresoUsuarioAsync(int usuarioId)
    {
        var progreso = new List<Progreso>();

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT p.id, p.id_usuario, p.id_rutina, p.peso, p.repeticiones, p.fecha,
                   r.nombre AS rutina_nombre
            FROM progreso p
            JOIN rutinas r ON p.id_rutina = r.id
            WHERE p.id_usuario = @usuarioId
            ORDER BY p.fecha DESC;
            """;
        command.Parameters.AddWithValue("@usuarioId", usuarioId);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            progreso.Add(MapProgreso(reader));
        }

        return progreso;
    }

    public async Task<Progreso> AddProgresoAsync(Progreso progreso)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO progreso (id_usuario, id_rutina, peso, repeticiones, fecha)
            VALUES (@idUsuario, @idRutina, @peso, @repeticiones, @fecha);
            """;
        command.Parameters.AddWithValue("@idUsuario", progreso.IdUsuario);
        command.Parameters.AddWithValue("@idRutina", progreso.IdRutina);
        command.Parameters.AddWithValue("@peso", progreso.Peso);
        command.Parameters.AddWithValue("@repeticiones", progreso.Repeticiones);
        command.Parameters.AddWithValue("@fecha", progreso.Fecha.Date);

        await command.ExecuteNonQueryAsync();
        progreso.Id = Convert.ToInt32(command.LastInsertedId);
        return progreso;
    }

    private static Rutina MapRutina(MySqlDataReader reader)
    {
        return new Rutina
        {
            Id = reader.GetInt32("id"),
            Nombre = reader.GetString("nombre"),
            Descripcion = reader.GetString("descripcion")
        };
    }

    private static UsuarioRutina MapUsuarioRutina(MySqlDataReader reader)
    {
        return new UsuarioRutina
        {
            Id = reader.GetInt32("id"),
            IdUsuario = reader.GetInt32("id_usuario"),
            IdRutina = reader.GetInt32("id_rutina"),
            FechaInicio = reader.GetDateTime("fecha_inicio"),
            RutinaNombre = reader.GetString("rutina_nombre"),
            RutinaDescripcion = reader.GetString("rutina_descripcion")
        };
    }

    private static Progreso MapProgreso(MySqlDataReader reader)
    {
        return new Progreso
        {
            Id = reader.GetInt32("id"),
            IdUsuario = reader.GetInt32("id_usuario"),
            IdRutina = reader.GetInt32("id_rutina"),
            Peso = reader.GetDecimal("peso"),
            Repeticiones = reader.GetInt32("repeticiones"),
            Fecha = reader.GetDateTime("fecha"),
            RutinaNombre = reader.GetString("rutina_nombre")
        };
    }
}
