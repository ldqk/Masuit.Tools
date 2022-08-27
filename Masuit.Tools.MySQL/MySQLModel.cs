using Dapper;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masuit.Tools.MySQL;

public class INFORMATION_SCHEMA_TABLE
{
    public string TABLE_CATALOG { get; set; }
    public string TABLE_SCHEMA { get; set; }
    public string TABLE_NAME { get; set; }
    public string TABLE_TYPE { get; set; }
    public string ENGINE { get; set; }
    public string VERSION { get; set; }
    public string ROW_FORMAT { get; set; }
    public string TABLE_ROWS { get; set; }
    public string AVG_ROW_LENGTH { get; set; }
    public string DATA_LENGTH { get; set; }
    public string MAX_DATA_LENGTH { get; set; }
    public string INDEX_LENGTH { get; set; }
    public string DATA_FREE { get; set; }
    public string AUTO_INCREMENT { get; set; }
    public string CREATE_TIME { get; set; }
    public string UPDATE_TIME { get; set; }
    public string CHECK_TIME { get; set; }
    public string TABLE_COLLATION { get; set; }
    public string CHECKSUM { get; set; }
    public string CREATE_OPTIONS { get; set; }
    public string TABLE_COMMENT { get; set; }
}

public class INFORMATION_SCHEMA_COLUMN
{
    public string TABLE_CATALOG { get; set; }
    public string TABLE_SCHEMA { get; set; }
    public string TABLE_NAME { get; set; }
    public string COLUMN_NAME { get; set; }
    public string ORDINAL_POSITION { get; set; }
    public object COLUMN_DEFAULT { get; set; }
    public string IS_NULLABLE { get; set; }
    public string DATA_TYPE { get; set; }
    public string CHARACTER_MAXIMUM_LENGTH { get; set; }
    public string CHARACTER_OCTET_LENGTH { get; set; }
    public string NUMERIC_PRECISION { get; set; }
    public string NUMERIC_SCALE { get; set; }
    public string DATETIME_PRECISION { get; set; }
    public string CHARACTER_SET_NAME { get; set; }
    public string COLLATION_NAME { get; set; }
    public string COLUMN_TYPE { get; set; }
    public string COLUMN_KEY { get; set; }
    public string EXTRA { get; set; }
    public string PRIVILEGES { get; set; }
    public string COLUMN_COMMENT { get; set; }
    public string GENERATION_EXPRESSION { get; set; }
    public string SRS_ID { get; set; }
}
