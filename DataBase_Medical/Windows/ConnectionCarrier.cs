using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
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

        public NpgsqlConnection Connection
        {
            get
            {
                return _connection;
            }
        }

        public async Task OpenConnectionAsyncSave()
        {
            while (_connection.State is not ConnectionState.Closed)
                await Task.Run(() => Thread.Sleep(20));
            await _connection.OpenAsync();
        }

        public async Task WaitForConnectionAsync()
        {
            while (_connection.State is not ConnectionState.Closed)
                await Task.Run(() => Thread.Sleep(20));
        }

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
                String sql = "Select * From \"CurrentStaff_Role\"";
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

            switch (str)
            {
                case "admin":
                {
                    return "Администратор";
                }
                case "chief":
                {
                    return "Главный врач";
                }
                case "department_chief":
                {
                    return "Заведующий отделением";
                }
                case "doctor":
                {
                    return "Врач";
                }
                default:
                {
                    throw new NotImplementedException();
                }
            }
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

        public async Task<long> GetCurrentSequenceId(String column)
        {
            var sequence_name = String.Empty;
            switch (column)
            {
                case "Category":
                {
                    sequence_name = "Category_Category_Id_seq";
                    break;
                }
                case "Disease":
                {
                    sequence_name = "Disease_Disease_Id_seq";
                    break;
                }
                case "Procedure":
                {
                    sequence_name = "Procedure_procedure_id_seq";
                    break;
                }
                case "SocialStatus":
                {
                    sequence_name = "SocialStatus_SocialStatus_id_seq";
                    break;
                }
                case "Department":
                {
                    sequence_name = "Department_Department_id_seq";
                    break;
                }
                case "Staff":
                {
                    sequence_name = "Staff_Staff_id_seq";
                    break;
                }
                case "Patient":
                {
                    sequence_name = "Patient_Patient_Id_seq";
                    break;
                }
                case "PatientDiseases":
                {
                    sequence_name = "PatientDeceases_PatientDeceases_Id_seq";
                    break;
                }
                case "HospitalStay":
                {
                    sequence_name = "HospitalStay_HospitalStay_Id_seq";
                    break;
                }
                case "DoctorAppointment":
                {
                    sequence_name = "DoctorAppointment_DoctorAppointment_Id_seq";
                    break;
                }
                default:
                {
                    throw new Exception();
                }
            }

            object data = null;
            try
            {
                await _connection.OpenAsync();
                var sql = $"Select nextval ('\"{sequence_name}\"')";
                await new NpgsqlCommand(sql, _connection).ExecuteNonQueryAsync();
                sql = $"Select currval ('\"{sequence_name}\"')";
                data = await new NpgsqlCommand(sql, _connection).ExecuteScalarAsync();
                sql = $"Select setval ('\"{sequence_name}\"', {(long)data - 1})";
                await new NpgsqlCommand(sql, _connection).ExecuteNonQueryAsync();
            }
            catch
            {
                throw;
            }
            finally
            {
                await _connection.CloseAsync();
            }

            return (long)data - 1;
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
