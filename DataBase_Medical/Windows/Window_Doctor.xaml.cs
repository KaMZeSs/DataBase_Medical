using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;


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
        string? CurrentAppointment_Selected_Id = string.Empty;
        Dictionary<String, string> Procedures = new();
        Dictionary<String, string> Procedures_Cost = new();

        private async void Patient_Load_DoctorAppointments()
        {
            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"{(isAppointmentHistory ? "DoctorAppointment_Full_Info" : "Current_Appointment_Full")}\" " +
                    $"Where \"DoctorAppointment_Patient_Id\" = {Patient_Selected_Id} " +
                    $"ORDER BY \"DoctorAppointment_Start_Date\"";
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
                        { "Дата последней процедуры", "end_date" },
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
                        { "Ожидаемая дата окончания", "end_date" },
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
            if (isAppointmentHistory)
                return;

            var vs = Patient_CurrAppointment_DataGrid.SelectedIndex;

            if (vs is -1)
                return;

            var row = Patient_CurrAppointment_DataGrid.Items[vs] as DataRowView;
            CurrentAppointment_Selected_Id = row?["id"].ToString();
        }
        private async void Patient_CurrAppointment_Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            if (isAppointmentHistory)
                return;
            if (CurrentAppointment_Selected_Id == string.Empty)
                return;

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"CALL stop_appointment({CurrentAppointment_Selected_Id})";
                await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();
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
        private async void Patient_CurrAppointment_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            if (isAppointmentHistory)
                return;

            Appointment_Date_DatePicker.DisplayDateStart = DateTime.Now;
            Appointment_Date_DatePicker.SelectedDate = DateTime.Now;
            Appointment_Time_TextBox.Text = $"{DateTime.Now.Hour}:{(DateTime.Now.Minute < 10 ? "0" + DateTime.Now.Minute.ToString() : DateTime.Now.Minute)}";

            NewAppointment_Grid.Visibility = NewAppointment_Grid.Visibility is Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            var button = sender as Button;
            button.Content = button.Content.ToString() == "Добавить" ? "Скрыть" : "Добавить";

            if (button.Content == "Добавить")
            {
                return;
            }

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = "Select * From \"Procedure\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                Procedures.Clear();
                Procedures = this.NpgsqlDataReader_To_DictionaryList(reader, "Procedure_Id", "Procedure_Name");
                reader.Close();

                reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();
                Procedures_Cost.Clear();
                Procedures_Cost = this.NpgsqlDataReader_To_DictionaryList(reader, "Procedure_Id", "Procedure_Cost");
                reader.Close();
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }
            
            Appointment_Procedure_ComboBox.ItemsSource = Procedures.Select(x => x.Value + $" ({Procedures_Cost[x.Key]} руб.)").OrderBy(x => x).ToArray();
        }
        private async void NewAppointment_Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            if (isAppointmentHistory)
                return;
            
            var date = DateOnly.FromDateTime((DateTime)Appointment_Date_DatePicker.SelectedDate);
            if (!TimeOnly.TryParse(Appointment_Time_TextBox.Text, out TimeOnly time))
            {
                MessageBox.Show("Невозможно считать время", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dateTime = date.ToDateTime(time);

            var day = Int32.Parse(Appointment_Days_TextBox.Text.Length is 0 ? "0" : Appointment_Days_TextBox.Text);
            var hours = Int32.Parse(Appointment_Hours_TextBox.Text.Length is 0 ? "0" : Appointment_Hours_TextBox.Text);
            var minutes = Int32.Parse(Appointment_Minutes_TextBox.Text.Length is 0 ? "0" : Appointment_Minutes_TextBox.Text);

            var interval = TimeSpan.FromDays(day) + TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes);

            var count = Int32.Parse(Appointment_Count_TextBox.Text.Length is 0 ? "0" : Appointment_Count_TextBox.Text);

            var end_date = DateTime.Now + interval * count;

            var name = Appointment_Procedure_ComboBox.Text.Substring(0, Appointment_Procedure_ComboBox.Text.LastIndexOf('(') - 1);
            var procedure_id = Procedures.Where(x => x.Value == name).First().Key;

            var sql = $"INSERT INTO \"DoctorAppointment\" " +
                $"(\"DoctorAppointment_Procedure_Id\", \"DoctorAppointment_Patient_Id\", " +
                $"\"DoctorAppointment_Start_Date\", \"DoctorAppointment_Interval\", \"DoctorAppointment_Count\") " +
                $"VALUES ({procedure_id}, {Patient_Selected_Id}, '{dateTime}', '{interval}', {count})";

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();
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
        private void Patient_CurrAppointment_History_Button_Click(object sender, RoutedEventArgs e)
        {
            isAppointmentHistory = !isAppointmentHistory;

            Appointment_Title_Label.Content = isAppointmentHistory ? "История назначений" : "Текущие назначения";

            var button = sender as Button;
            button.Content = button.Content.ToString() == "История" ? "Текущие" : "История";

            Patient_Load_DoctorAppointments();
        }

        #region TextBox Check

        private void TextBox_PreviewTextInput_Int(object sender, TextCompositionEventArgs e)
        {
            int num;
            e.Handled = !IsNumeric(e.Text, out num);

            if (e.Handled)
                return;

            if (num < 0)
                e.Handled = true;


            var rtb = sender as RibbonTextBox;

            if (!IsNumeric(rtb.Text + e.Text, out num))
            {
                e.Handled = true;
                return;
            }

            if (rtb == Appointment_Minutes_TextBox)
            {
                if (num > 60)
                    e.Handled = true;
            }
            else if (rtb == Appointment_Hours_TextBox)
            {
                if (num > 24)
                    e.Handled = true;
            }
        }

        private bool IsNumeric(string input, out int output)
        {
            return int.TryParse(input, out output);
        }

        #endregion

        #endregion

        #region Diseases

        bool isDiseaseHistory = false;
        string? CurrentDisease_Selected_Id = string.Empty;
        Dictionary<String, string> Diseases = new();

        //TO CHECK
        private async void Patient_Load_Diseases()
        {
            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"{(isDiseaseHistory ? "All_PatientDiseases_Full" : "Current_PatientDiseases_Full")}\" " +
                    $"Where \"PatientDiseases_Patient_Id\" = {Patient_Selected_Id}";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                Dictionary<String, String> dict = null;

                if (isAppointmentHistory)
                {
                    dict = new Dictionary<string, string>()
                    {
                        { "id", "PatientDiseases_Id" },
                        { "Заболевание", "Disease_Name" },
                        { "Дата заболевания", "PatientDiseases_Start_Date" },
                        { "Дата выздоровления", "PatientDiseases_End_Date" }
                    };
                }
                else
                {
                    dict = new Dictionary<string, string>()
                    {
                        { "id", "PatientDiseases_Id" },
                        { "Заболевание", "Disease_Name" },
                        { "Дата заболевания", "PatientDiseases_Start_Date" }
                    };
                }

                var dt = this.NpgsqlDataReader_To_DataTable(reader, dict);

                reader.Close();

                this.Patient_Diseases_DataGrid.ItemsSource = new DataView(dt);
                this.Patient_Diseases_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
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

        //TO CHECK
        private void Patient_Diseases_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (isDiseaseHistory)
                return;

            var vs = Patient_Diseases_DataGrid.SelectedIndex;

            if (vs is -1)
                return;

            var row = Patient_Diseases_DataGrid.Items[vs] as DataRowView;
            CurrentDisease_Selected_Id = row?["id"].ToString();
        }

        //TO CHECK
        private async void Patient_Diseases_Button_Add_Click(object sender, RoutedEventArgs e)
        {
            if (isDiseaseHistory)
                return;

            Patient_Diseases_Grid_Add.Visibility = Patient_Diseases_Grid_Add.Visibility is Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            var button = sender as Button;
            button.Content = button.Content.ToString() == "Добавить" ? "Скрыть" : "Добавить";

            if (button.Content == "Добавить")
            {
                return;
            }

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = "Select * From \"Disease\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                Diseases.Clear();
                Diseases = this.NpgsqlDataReader_To_DictionaryList(reader, "Disease_Id", "Disease_Name");
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }

            Patient_Diseases_ComboBox.ItemsSource = Diseases.Values.OrderBy(x => x).ToArray();
        }

        //TO CHECK
        private async void Patient_Diseases_Button_Add_Ok_Click(object sender, RoutedEventArgs e)
        {
            var dicease_id = Diseases.Where(x => x.Value == Patient_Diseases_ComboBox.Text).First().Key;

            var sql = $"INSERT INTO \"PatientDiseases\" " +
                $"(\"PatientDiseases_Patient_Id\", \"PatientDiseases_Disease_Id\") VALUES " +
                $"({Patient_Selected_Id}, {dicease_id})";

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }

            Patient_Load_Diseases();
        }

        //TO CHECK
        private async void Patient_Diseases_Button_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (isDiseaseHistory)
                return;
            if (CurrentDisease_Selected_Id == string.Empty)
                return;

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"CALL stop_patient_disease({CurrentDisease_Selected_Id})";
                await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }

            Patient_Load_Diseases();
        }

        //TO CHECK
        private async void Patient_Diseases_Button_History_Click(object sender, RoutedEventArgs e)
        {
            isDiseaseHistory = !isDiseaseHistory;

            Patient_Diseases_Title_Label.Content = isDiseaseHistory ? "История болезней" : "Список текущих болезней";

            var button = sender as Button;
            button.Content = isDiseaseHistory ? "История" : "Текущие";

            Patient_Load_Diseases();
        }

        //TO CHECK
        private async void Patient_Diseases_Button_Filter_Click(object sender, RoutedEventArgs e)
        {
            Patient_Diseases_Grid_Filter.Visibility = Patient_Diseases_Grid_Filter.Visibility is Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            this.Patient_Diseases_DatePicker_End.IsEnabled = !isDiseaseHistory;

            this.Patient_Diseases_DatePicker_Start.SelectedDate = this.Patient_Diseases_DatePicker_End.SelectedDate = DateTime.Now;
        }

        private async void Patient_Diseases_Button_Filter_Ok_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion


    }
}
