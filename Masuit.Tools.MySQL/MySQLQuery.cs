using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Masuit.Tools.MySQL;

public class MySQLQuery
{
    public static INFORMATION_SCHEMA_TABLE[] GetTABLES(MySqlConnection connection)
    {
        var sql = @"SELECT * FROM information_schema.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = @TABLE_SCHEMA;";
        var value = connection.Query<INFORMATION_SCHEMA_TABLE>(sql, new { TABLE_SCHEMA = connection.Database });
        return value.ToArray();
    }

    public static INFORMATION_SCHEMA_COLUMN[] GetCOLUMNS(MySqlConnection connection, string table)
    {
        var sql = $"SELECT * FROM information_schema.COLUMNS WHERE TABLE_NAME = @TABLE_NAME AND TABLE_SCHEMA = @TABLE_SCHEMA;";
        var value = connection.Query<INFORMATION_SCHEMA_COLUMN>(sql, new { TABLE_NAME = table, TABLE_SCHEMA = connection.Database });
        return value.ToArray();
    }
}
