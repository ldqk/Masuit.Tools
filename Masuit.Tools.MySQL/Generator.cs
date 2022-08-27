using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masuit.Tools.MySQL;

public partial class Generator
{
    public static string BuildCaller(string klass)
    {
        var code = new StringBuilder();
        code.AppendLine(@$"using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

async Task Main()
{{
    var conn_str = ""Server=127.0.0.1;Port=3306;Database=Demo;User Id=root;Password=123456;AllowLoadLocalInfile=true"";
    // docker run -p 3306:3306 -e MYSQL_ROOT_PASSWORD=123456 -e TZ=Asia/Shanghai --restart=always --name mysql-demo -d mysql --character-set-server=utf8mb4 --collation-server=utf8mb4_unicode_ci --local-infile=ON
    var services = new ServiceCollection();
    var provider = services.AddLogging()
        .AddDbContextPool<{klass}>(optionsBuilder =>
        {{
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new EFLoggerProvider());
            optionsBuilder
                .UseLoggerFactory(loggerFactory)
                .LogTo(Console.WriteLine, new EventId[] {{ CoreEventId.CoreBaseId }})");
        code.Append("                ");
        code.AppendLine(".UseMySql(conn_str, ServerVersion.AutoDetect(conn_str))");
        code.Append("                ");
        code.AppendLine(@$".UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)  // 设置不跟踪所有查询            
                .EnableSensitiveDataLogging();  // 启用敏感数据日志记录
        }})
        .BuildServiceProvider();
    var ctx = provider.GetService<{klass}>();
}}

// You can define other methods, fields, classes and namespaces here
");
        code.AppendLine(@"public class EFLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new EFLogger(categoryName);
    public void Dispose() { }
}

public class EFLogger : ILogger
{
    private readonly string categoryName;
    public EFLogger(string categoryName) => this.categoryName = categoryName;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (categoryName == ""Microsoft.EntityFrameworkCore.Database.Command"" && logLevel == LogLevel.Information)  // EFCore 日志种类和级别
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(formatter(state, exception));
            Console.ResetColor();
        }
    }
    public IDisposable BeginScope<TState>(TState state) => null;
}");
        return code.ToString();
    }

    public static string BuildDbContext(string klass, INFORMATION_SCHEMA_TABLE[] tables)
    {
        var code = new StringBuilder();
        code.AppendLine($"public partial class {klass} : DbContext");
        code.AppendLine("{");
        code.AppendLine($@"    public {klass}(DbContextOptions<{klass}> options) : base(options) {{ }}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {{
        base.OnModelCreating(modelBuilder);");
        foreach (var table in tables)
        {
            code.AppendLine($"        modelBuilder.ApplyConfiguration(new {table.TABLE_NAME}Configuration());");
        }
        code.AppendLine("    }");
        foreach (var table in tables)
        {
            if (!string.IsNullOrEmpty(table.TABLE_COMMENT))
            {
                code.AppendLine($"    /// <summary> {table.TABLE_COMMENT} </summary>");
            }
            code.AppendLine($"    public virtual DbSet<{table.TABLE_NAME}> {table.TABLE_NAME} {{ get; set; }}");
        }
        code.AppendLine("}");
        return code.ToString();
    }

    public static string BuildTableClass(string table, INFORMATION_SCHEMA_COLUMN[] columns)
    {
        var code = new StringBuilder();
        code.AppendLine($"public class {table}");
        code.AppendLine("{");
        foreach (var column in columns)
        {
            var find = DataTypeMapper.FirstOrDefault(o => column.DATA_TYPE?.Equals(o.dbType, StringComparison.OrdinalIgnoreCase) == true);
            find = find.dbType != null ? find : DataTypeMapper.FirstOrDefault(o => column.DATA_TYPE?.Contains(o.dbType, StringComparison.OrdinalIgnoreCase) == true);
            var data_type = TypeToKeywords(find.type ?? typeof(object));
            var nullable = find.type?.IsValueType == true && column.IS_NULLABLE != "NO" ? "?" : "";
            if (!string.IsNullOrEmpty(column.COLUMN_COMMENT))
            {
                code.AppendLine($"    /// <summary> {column.COLUMN_COMMENT} </summary>");
            }
            code.Append($"    public {data_type}{nullable} {column.COLUMN_NAME} {{ get; set; }}");
            if (column.COLUMN_DEFAULT != null && !column.COLUMN_DEFAULT.Equals(""))
            {
                code.Append(" = ").Append(column.COLUMN_DEFAULT).Append(";");
            }
            code.AppendLine();
        }
        code.AppendLine("}");

        code.AppendLine();
        code.AppendLine($"public class {table}Configuration : IEntityTypeConfiguration<{table}>");
        code.AppendLine("{");
        code.AppendLine($"    public void Configure(EntityTypeBuilder<{table}> builder)");
        code.AppendLine("    {");
        code.AppendLine($"        builder.ToTable(nameof({table}));");
        if (columns.Any(o => o.COLUMN_KEY == "PRI"))
        {
            code.Append("        builder.HasKey(x => new { ");
            foreach (var o in columns)
            {
                if (o.COLUMN_KEY == "PRI")
                {
                    code.Append("x.").Append(o.COLUMN_NAME).Append(", ");
                }
            }
            code.AppendLine("});");
        }
        foreach (var column in columns)
        {
            code.Append($"        builder.Property(x => x.{column.COLUMN_NAME})");
            if (!string.IsNullOrEmpty(column.COLUMN_TYPE))
            {
                code.Append($".HasColumnType(\"{column.COLUMN_TYPE}\")");
            }
            if (!string.IsNullOrEmpty(column.COLUMN_COMMENT))
            {
                code.Append($".HasComment(\"{column.COLUMN_COMMENT}\")");
            }
            code.AppendLine(";");
        }
        code.AppendLine("    }");
        code.AppendLine("}");
        return code.ToString();
    }
}


public partial class Generator
{
    static string TypeToKeywords(Type type)
    {
        return new[]
        {
            (typeof(bool),    "bool"   ),
            (typeof(byte),    "byte"   ),
            (typeof(char),    "char"   ),
            (typeof(decimal), "decimal"),
            (typeof(double),  "double" ),
            (typeof(float),   "float"  ),
            (typeof(int),     "int"    ),
            (typeof(long),    "long"   ),
            (typeof(object),  "object" ),
            (typeof(sbyte),   "sbyte"  ),
            (typeof(short),   "short"  ),
            (typeof(string),  "string" ),
            (typeof(uint),    "uint"   ),
            (typeof(ulong),   "ulong"  ),
            (typeof(ushort),  "ushort" ),
            (typeof(byte[]),  "byte[]" ),
        }.FirstOrDefault(o => o.Item1 == type).Item2 ?? type.Name;
    }

    private static (string dbType, Type type)[] DataTypeMapper = new[]
    {
        (nameof(MySqlDbType.Decimal),     typeof(decimal) ),
        (nameof(MySqlDbType.Byte),        typeof(sbyte)   ),
        (nameof(MySqlDbType.Int16),       typeof(short)   ),
        (nameof(MySqlDbType.Int32),       typeof(int)     ), ("int",       typeof(int)     ),
        (nameof(MySqlDbType.Float),       typeof(float)   ),
        (nameof(MySqlDbType.Double),      typeof(double)  ),
        (nameof(MySqlDbType.Timestamp),   typeof(DateTime)),
        (nameof(MySqlDbType.Int64),       typeof(long)    ),
        (nameof(MySqlDbType.Int24),       typeof(int)     ),
        (nameof(MySqlDbType.Date),        typeof(DateTime)),
        (nameof(MySqlDbType.Time),        typeof(TimeSpan)),
        (nameof(MySqlDbType.DateTime),    typeof(DateTime)),
        (nameof(MySqlDbType.Datetime),    typeof(DateTime)),
        (nameof(MySqlDbType.Year),        typeof(short)   ),
        (nameof(MySqlDbType.Newdate),     typeof(DateTime)),
        (nameof(MySqlDbType.VarString),   typeof(string)  ),
        (nameof(MySqlDbType.Bit),         typeof(bool)    ),
        (nameof(MySqlDbType.JSON),        typeof(object)  ),
        (nameof(MySqlDbType.NewDecimal),  typeof(decimal) ),
        (nameof(MySqlDbType.Enum),        typeof(object)  ),
        (nameof(MySqlDbType.Set),         typeof(object)  ),
        (nameof(MySqlDbType.TinyBlob),    typeof(byte[])  ),
        (nameof(MySqlDbType.MediumBlob),  typeof(byte[])  ),
        (nameof(MySqlDbType.LongBlob),    typeof(byte[])  ),
        (nameof(MySqlDbType.Blob),        typeof(byte[])  ),
        (nameof(MySqlDbType.VarChar),     typeof(string)  ),
        (nameof(MySqlDbType.String),      typeof(string)  ),
        (nameof(MySqlDbType.Geometry),    typeof(object)  ),
        (nameof(MySqlDbType.UByte),       typeof(byte)    ),
        (nameof(MySqlDbType.UInt16),      typeof(ushort)  ),
        (nameof(MySqlDbType.UInt32),      typeof(uint)    ),
        (nameof(MySqlDbType.UInt64),      typeof(ulong)   ),
        (nameof(MySqlDbType.UInt24),      typeof(uint)    ),
        (nameof(MySqlDbType.TinyText),    typeof(string)  ),
        (nameof(MySqlDbType.MediumText),  typeof(string)  ),
        (nameof(MySqlDbType.LongText),    typeof(string)  ),
        (nameof(MySqlDbType.Text),        typeof(string)  ),
        (nameof(MySqlDbType.VarBinary),   typeof(string)  ),
        (nameof(MySqlDbType.Binary),      typeof(string)  ),
        (nameof(MySqlDbType.Guid),        typeof(Guid)    ),
    };
}
