// See https://aka.ms/new-console-template for more information


using Masuit.Tools.MySQL;

var host = "192.168.142.129";
var mysql = $"Server={host};Port=3306;Database=Demo;User Id=root;Password=123456;AllowLoadLocalInfile=true";


using var conn = new MySqlConnector.MySqlConnection(mysql);
var klass = conn.Database + "DbContext";
Console.WriteLine(Generator.BuildCaller(klass));
var tables = MySQLQuery.GetTABLES(conn);
var ctx = Generator.BuildDbContext(klass, tables);
Console.WriteLine(ctx);
foreach (var table in tables)
{
    var columns = MySQLQuery.GetCOLUMNS(conn, table.TABLE_NAME);
    Console.WriteLine(Generator.BuildTableClass(table.TABLE_NAME, columns));
}