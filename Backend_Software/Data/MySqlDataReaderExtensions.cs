using MySqlConnector;

namespace Softawer.Data;

public static class MySqlDataReaderExtensions
{
    public static bool IsDBNull(this MySqlDataReader reader, string columnName)
    {
        return reader.IsDBNull(reader.GetOrdinal(columnName));
    }
}
