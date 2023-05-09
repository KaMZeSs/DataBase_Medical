using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Npgsql;

namespace DataBase_Medical.Windows
{
    /// <summary>
    /// Логика взаимодействия для Window_Doctor.xaml
    /// </summary>
    public partial class Window_Doctor : Window
    {
        public Window_Doctor()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = "Доктор: " + await ConnectionCarrier.Carrier.GetCurrentFIO();

            Patient_MenuItem_Refresh_Click(sender, e);
            await ConnectionCarrier.Carrier.WaitForConnectionAsync();
            RaiseFirstSelection(Patient_DataGrid);
        }

        /// <summary>
        /// Заполняет DataTable значениями из NpgsqlDataReader.
        /// Key - название колонки в DataTable и Value - название в reader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columns">Словарь значений, где Key - название колонки в DataTable и Value - название в reader</param>
        private DataTable NpgsqlDataReader_To_DataTable(NpgsqlDataReader reader, Dictionary<String, String> columns)
        {
            DataTable dt = new DataTable();
            foreach (var column in columns)
            {
                dt.Columns.Add(column.Key);
            }
            while (reader.Read())
            {
                DataRow dataRow = dt.NewRow();

                // Fill the columns
                foreach (var column in columns)
                {
                    dataRow[column.Key] = reader[column.Value];
                }

                dt.Rows.Add(dataRow);
            }

            return dt;
        }

        private Dictionary<String, String> NpgsqlDataReader_To_Dictionary(NpgsqlDataReader reader, Dictionary<String, String> columns)
        {
            DataTable dt = new DataTable();
            foreach (var column in columns)
            {
                dt.Columns.Add(column.Key);
            }
            reader.Read();

            DataRow dataRow = dt.NewRow();

            // Fill the columns
            foreach (var column in columns)
            {
                dataRow[column.Key] = reader[column.Value];
            }

            Dictionary<String, String> result = new();

            //reader.Read();
            foreach (var column in columns)
            {
                result.Add(column.Key, dataRow[column.Key].ToString());
            }

            return result;
        }

        private Dictionary<String, String> NpgsqlDataReader_To_DictionaryList(NpgsqlDataReader reader, String column_key, String column_value)
        {
            Dictionary<String, String> result = new();
            DataTable dt = new DataTable();
            dt.Columns.Add(column_key);
            dt.Columns.Add(column_value);
            while (reader.Read())
            {
                DataRow dataRow = dt.NewRow();
                dataRow[column_key] = reader[column_key];
                dataRow[column_value] = reader[column_value];

                result.Add(dataRow[column_key].ToString(), dataRow[column_value].ToString());
            }

            return result;
        }

        private void RaiseFirstSelection(DataGrid dataGrid)
        {
            dataGrid.SelectedIndex = 0;
        }

        string? Patient_Selected_Id = string.Empty;

        #region Patient info
        private async void Patient_Load_Data(String id)
        {
            Dictionary<String, String> data;

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"Patient_Full\" Where \"Patient_Id\" = {id}";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                data = this.NpgsqlDataReader_To_Dictionary(reader, new Dictionary<string, string>()
                {
                    { "id", "Patient_Id" },
                    { "Имя", "Patient_Name" },
                    { "Фамилия", "Patient_Surname" },
                    { "Отчество", "Patient_Patronymic" },
                    { "Дата рождения", "Patient_BirthDay" },
                    { "Социальное положение", "SocialStatus_Name" }
                });

                reader.Close();

                Patient_Label_Surname.Content = data["Фамилия"];
                Patient_Label_Name.Content = data["Имя"];
                Patient_Label_Patronymic.Content = data["Отчество"];
                Patient_Label_Birthday.Content = data["Дата рождения"].Split(' ')[0];
                Patient_Label_SocialStatus.Content = data["Социальное положение"];

                sql = $"Select get_patient_department({id})";
                reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();
                data = this.NpgsqlDataReader_To_Dictionary(reader, new()
                    {
                        { "dep", "get_patient_department" }
                    });
                Patient_Label_Department.Content = data["dep"] == "" ? "Нет" : data["dep"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }

            Patient_Load_DoctorAppointments();
        }

        private async void Patient_MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Patient_Label_Surname.Content = "";
            Patient_Label_Surname.Visibility = Visibility.Visible;

            Patient_Label_Name.Content = "";
            Patient_Label_Name.Visibility = Visibility.Visible;

            Patient_Label_Patronymic.Content = "";
            Patient_Label_Patronymic.Visibility = Visibility.Visible;

            Patient_Label_SocialStatus.Content = "";
            Patient_Label_SocialStatus.Visibility = Visibility.Visible;

            Patient_Label_Birthday.Content = "";
            Patient_Label_Birthday.Visibility = Visibility.Visible;

            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = "Select * FROM \"Patient_SurnameNP\" Order By \"Patient_Id\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "Patient_Id" },
                    { "ФИО", "Patient_SurnameNP" },
                });

                this.Patient_DataGrid.ItemsSource = new DataView(dt);
                this.Patient_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }
            if (Patient_Selected_Id != string.Empty)
            {
                Patient_Load_Data(Patient_Selected_Id);
            }
            else
            {
                RaiseFirstSelection(Patient_DataGrid);
            }
        }

        private void Patient_MenuItem_Search_Click(object sender, RoutedEventArgs e)
        {
            this.Patient_Grid_Search.Visibility =
               this.Patient_Grid_Search.Visibility is Visibility.Visible ?
               Visibility.Collapsed : Visibility.Visible;
        }

        private void Patient_DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (Patient_DataGrid.SelectedIndex is -1)
                return;

            Patient_Label_Surname.Content = "";
            Patient_Label_Surname.Visibility = Visibility.Visible;

            Patient_Label_Name.Content = "";
            Patient_Label_Name.Visibility = Visibility.Visible;

            Patient_Label_Patronymic.Content = "";
            Patient_Label_Patronymic.Visibility = Visibility.Visible;

            Patient_Label_SocialStatus.Content = "";
            Patient_Label_SocialStatus.Visibility = Visibility.Visible;

            Patient_Label_Birthday.Content = "";
            Patient_Label_Birthday.Visibility = Visibility.Visible;

            var vs = Patient_DataGrid.SelectedIndex;
            var row = Patient_DataGrid.Items[vs] as DataRowView;
            Patient_Selected_Id = row?["id"].ToString();

            Patient_Load_Data(Patient_Selected_Id);
        }

        private async void Patient_Button_SearchData_Click(object sender, RoutedEventArgs e)
        {
            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();

                String sql = $"Select * FROM \"Patient_SurnameNP\"";

                if (Patient_TextBox_SearchData.Text.Length is not 0)
                {
                    sql += $" Where lower(\"Patient_SurnameNP\") Like '%{Patient_TextBox_SearchData.Text.ToLower()}%' Order By \"Patient_Id\"";
                }

                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "Patient_Id" },
                    { "ФИО", "Patient_SurnameNP" }
                });

                this.Patient_DataGrid.ItemsSource = new DataView(dt);
                this.Patient_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        #endregion

        #region Doctor appointment

        bool isAppointmentHistory = false;

        private async void Patient_Load_DoctorAppointments()
        {
            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"{(isAppointmentHistory ? "DoctorAppointment_Full_Info" : "Current_Appointment_Full")}\" Where \"DoctorAppointment_Patient_Id\" = {Patient_Selected_Id}";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                Dictionary<String, String> dict = null;

                if (isAppointmentHistory)
                {
                    dict = new Dictionary<string, string>()
                    {
                        { "id", "DoctorAppointment_Id" },
                        { "Процедура", "Procedure_Name" },
                        { "Назначил", "Staff_SurnameNP" },
                        { "Дата начала", "DoctorAppointment_Start_Date" },
                        { "Дата окончания", "end_date" },
                        { "Интервал", "DoctorAppointment_Interval" },
                        { "Всего", "DoctorAppointment_Count" }
                    };
                }
                else
                {
                    dict = new Dictionary<string, string>()
                    {
                        { "id", "DoctorAppointment_Id" },
                        { "Процедура", "Procedure_Name" },
                        { "Назначил", "Staff_SurnameNP" },
                        { "Дата начала", "DoctorAppointment_Start_Date" },
                        { "Дата окончания", "end_date" },
                        { "Интервал", "DoctorAppointment_Interval" },
                        { "Выполнено", "completed_count" },
                        { "Осталось", "count_left" }
                    };
                }

                var dt = this.NpgsqlDataReader_To_DataTable(reader, dict);

                reader.Close();

                this.Patient_CurrAppointment_DataGrid.ItemsSource = new DataView(dt);
                this.Patient_CurrAppointment_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        private void Patient_CurrAppointment_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (!isAppointmentHistory)
                return;
        }

        private async void Patient_CurrAppointment_Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!isAppointmentHistory)
                return;
        }

        private async void Patient_CurrAppointment_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!isAppointmentHistory)
                return;
        }

        private async void Patient_CurrAppointment_History_Button_Click(object sender, RoutedEventArgs e)
        {
            isAppointmentHistory = !isAppointmentHistory;

            Appointment_Title_Label.Content = isAppointmentHistory ? "История назначений" : "Текущие назначения";

            Patient_Load_DoctorAppointments();

        }

        #endregion

        #region Diseases

        private async void Patient_Load_Diseases()
        {
            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"Current_Appointment_Full\" Where \"DoctorAppointment_Patient_Id\" = {Patient_Selected_Id}";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "DoctorAppointment_Id" },
                    { "Процедура", "Procedure_Name" },
                    { "Назначил", "Staff_SurnameNP" },
                    { "Дата начала", "DoctorAppointment_Start_Date" },
                    { "Интервал", "DoctorAppointment_Interval" },
                    { "Осталось повторений", "count_left" }
                });

                reader.Close();

                this.Patient_CurrAppointment_DataGrid.ItemsSource = new DataView(dt);
                this.Patient_CurrAppointment_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        private void Patient_Diseases_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {

        }

        private async void Patient_Diseases_Button_Add_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void Patient_Diseases_Button_Remove_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void Patient_Diseases_Button_History_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion
    }
}
