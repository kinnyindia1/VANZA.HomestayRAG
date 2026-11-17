
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Text.Json;

namespace VANZA.HomestayAI.API.Services
{
    public class SqlService
    {
        private readonly IConfiguration _configuration;

        public SqlService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Step 1: Read the live SQL Server database schema
        public async Task<string> GetDatabaseSchemaAsync()
        {
            Console.WriteLine();
            Console.WriteLine("================================================");
            Console.WriteLine("[SQL SERVER] READING DATABASE SCHEMA");
            Console.WriteLine("================================================");

            string connectionString =
                _configuration.GetConnectionString("ReadOnlyConnection")
                ?? throw new Exception(
                    "ReadOnlyConnection is missing from appsettings.json.");

            string sql = """
                SELECT
                    TABLE_SCHEMA,
                    TABLE_NAME,
                    COLUMN_NAME,
                    DATA_TYPE
                FROM INFORMATION_SCHEMA.COLUMNS
                ORDER BY TABLE_SCHEMA, TABLE_NAME, ORDINAL_POSITION;
                """;

            var schema = new System.Text.StringBuilder();

            using var connection = new SqlConnection(connectionString);

            Console.WriteLine("[SQL SERVER] Opening read-only connection...");

            await connection.OpenAsync();

            Console.WriteLine("[SQL SERVER] Connection opened successfully.");
            Console.WriteLine("[SQL SERVER] Reading INFORMATION_SCHEMA.COLUMNS...");

            using var command = new SqlCommand(sql, connection);

            using var reader = await command.ExecuteReaderAsync();

            string? currentTable = null;

            while (await reader.ReadAsync())
            {
                string tableSchema = reader["TABLE_SCHEMA"].ToString()!;
                string tableName = reader["TABLE_NAME"].ToString()!;
                string columnName = reader["COLUMN_NAME"].ToString()!;
                string dataType = reader["DATA_TYPE"].ToString()!;

                string fullTableName = $"{tableSchema}.{tableName}";

                if (currentTable != fullTableName)
                {
                    currentTable = fullTableName;

                    schema.AppendLine();
                    schema.AppendLine($"TABLE: {fullTableName}");
                }

                schema.AppendLine(
                    $"  COLUMN: {columnName} | TYPE: {dataType}");
            }

            Console.WriteLine("[SQL SERVER] Schema retrieved successfully.");
            Console.WriteLine($"[SQL SERVER] Schema length: {schema.Length} characters");

            return schema.ToString();
        }

        // Step 2: Execute the generated SELECT query
        public async Task<string> ExecuteSelectAsync(string sql)
        {
            Console.WriteLine();
            Console.WriteLine("================================================");
            Console.WriteLine("[SQL SERVER] EXECUTING GENERATED QUERY");
            Console.WriteLine("================================================");

            Console.WriteLine("[SQL SERVER] SQL to execute:");
            Console.WriteLine(sql);

            string connectionString =
                _configuration.GetConnectionString("ReadOnlyConnection")
                ?? throw new Exception(
                    "ReadOnlyConnection is missing from appsettings.json.");

            using var connection = new SqlConnection(connectionString);

            Console.WriteLine("[SQL SERVER] Opening read-only connection...");

            await connection.OpenAsync();

            Console.WriteLine("[SQL SERVER] Connection opened.");

            using var command = new SqlCommand(sql, connection);

            command.CommandTimeout = 60;

            using var reader = await command.ExecuteReaderAsync();

            var rows = new List<Dictionary<string, object?>>();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    object value = reader.GetValue(i);

                    row[reader.GetName(i)] =
                        value == DBNull.Value ? null : value;
                }

                rows.Add(row);
            }

            string result = JsonSerializer.Serialize(
                rows,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            Console.WriteLine("[SQL SERVER] Query executed successfully.");
            Console.WriteLine($"[SQL SERVER] Rows returned: {rows.Count}");

            Console.WriteLine();
            Console.WriteLine("[SQL SERVER] QUERY RESULT:");
            Console.WriteLine(result);
            Console.WriteLine("================================================");

            return result;
        }

        public bool IsValidSelect(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return false;

            var parser = new TSql160Parser(true);

            using var reader = new StringReader(sql);

            var fragment = parser.Parse(
                reader,
                out IList<ParseError> errors);

            if (errors.Count > 0)
                return false;

            if (fragment is not TSqlScript script)
                return false;

            if (script.Batches.Count != 1)
                return false;

            if (script.Batches[0].Statements.Count != 1)
                return false;

            return script.Batches[0].Statements[0]
                is SelectStatement;
        }
    }
}
