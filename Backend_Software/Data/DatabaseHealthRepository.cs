using MySqlConnector;

namespace Softawer.Data;

public class DatabaseHealthRepository(MySqlDataSource dataSource)
{
    public async Task PingAsync()
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1";
        await command.ExecuteScalarAsync();
    }
}
