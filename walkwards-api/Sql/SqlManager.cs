using System.Data;
using Npgsql;
using walkwards_api.Utilities;

namespace walkwards_api.Sql
{
    public static class SqlManager
    {
        private static string GetConnectionString()
        {
            const string host = "194.150.101.246";
            const string database = "Main";
            const string username = "postgres";
            const string password = "!Malinka@pass#database";

            return $"Host={host};Username={username};Password={password};Database={database}";
        }

        public static async Task<List<Dictionary<string, dynamic>>> Reader(string sql)
        {
            NpgsqlConnection connection = new(GetConnectionString());
            List<Dictionary<string, dynamic>> result = new();

            try
            {
                NpgsqlCommand command = new(sql, connection);

                await connection.OpenAsync();

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    Dictionary<string, dynamic> data = new();

                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        string currentName = reader.GetName(i);
                        dynamic currentValue = reader.GetValue(i);

                        data.Add(currentName, currentValue);
                    }

                    result.Add(data);
                }

                await connection.CloseAsync();

                if (connection.State != ConnectionState.Closed)
                {
                    await LoggerManager.WriteLog("err sql " + connection.State + " " + connection.ConnectionString);
                }

                
            }
            catch (Exception e)
            {
                await LoggerManager.WriteLog("ex sql " + e.Message);
                throw;
            }
            finally
            {
                await connection.DisposeAsync();
            }
            
            return result;
        }
        
        public static async Task ExecuteNonQuery(string sql)
        {
            NpgsqlConnection connection = new (GetConnectionString());

            try
            {
                NpgsqlCommand command = new (sql, connection);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                await connection.CloseAsync();
                

                if (connection.State != ConnectionState.Closed)
                {
                    await LoggerManager.WriteLog("err sql " + connection.State + " " + connection.ConnectionString);

                }
            }
            catch (Exception e)
            {
                await LoggerManager.WriteLog("ex sql " + e.Message);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }
        public static async Task ExecuteFromList(List<string> sql) //lista sqlek
        {
            NpgsqlConnection connection = new (GetConnectionString());
            
            try
            {
                NpgsqlCommand command = new ();
                command.Connection = connection;

                await connection.OpenAsync();

                foreach (var item in sql)
                {
                    command.CommandText = item;
                    await command.ExecuteNonQueryAsync();
                }
            
                await connection.CloseAsync();

                if (connection.State != ConnectionState.Closed)
                {
                    await LoggerManager.WriteLog("err sql " + connection.State + " " + connection.ConnectionString);

                }
            }
            catch (Exception e)
            {
                await LoggerManager.WriteLog("ex sql " + e.Message);
            }
            finally
            {
                await connection.DisposeAsync();
            }
        }
    }
}