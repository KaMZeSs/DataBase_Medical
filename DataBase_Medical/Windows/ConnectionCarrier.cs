using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Npgsql;

namespace DataBase_Medical.Windows
{
    class ConnectionCarrier
    {
        #region Singleton

        public static ConnectionCarrier Carrier
        {
            get
            {
                if (carrier is null)
                    throw new Exception();
                return carrier;
            }
        }
        public static void CreateConnection(String login, String password,
            String host = "localhost", int port = 5432, String dataBase = "postgres")
        {
            carrier = new ConnectionCarrier(login, password, host, port, dataBase);
        }
        private static ConnectionCarrier carrier;

        #endregion

        private NpgsqlConnection _connection;

        public NpgsqlConnection Connection { get { return _connection; } }

        public ConnectionCarrier(String login, String password,
            String host= "localhost", int port = 5432, String dataBase = "postgres")
        {
            string connectString = $"Host={host};Port={port};" +
                $"User Id={login};Password={password};" +
                $"Database={dataBase};Timeout=300;CommandTimeout=300;";
            _connection = new NpgsqlConnection(connectString);
        }

        public async Task<String> GetCurrentJob()
        {
            var str = String.Empty;
            try
            {
                await _connection.OpenAsync();
                String sql = "Select * From CurrentStaff_JobName";
                var reader = await new NpgsqlCommand(sql, _connection).ExecuteReaderAsync();
                await reader.ReadAsync();
                str = reader.GetString(0);
            }
            catch
            {

            }
            finally
            {
                await _connection.CloseAsync();
            }

            return str;
        }

        public async Task<String> GetCurrentFIO()
        {
            var str = String.Empty;
            try
            {
                await _connection.OpenAsync();
                String sql = "Select * From \"CurrentStaff_SurnameNP\"";
                var reader = await new NpgsqlCommand(sql, _connection).ExecuteReaderAsync();
                await reader.ReadAsync();
                str = reader.GetString(0);
            }
            catch
            {

            }
            finally
            {
                await _connection.CloseAsync();
            }

            return str;
        }

        public async Task<NpgsqlDataReader> CustomCommand(String sql)
        {
            NpgsqlDataReader reader;
            try
            {
                await _connection.OpenAsync();
                reader = await new NpgsqlCommand(sql, _connection).ExecuteReaderAsync();
            }
            catch
            {
                throw;
            }
            finally
            {
                await _connection.CloseAsync();
            }
            return reader;
        }
    }
}
