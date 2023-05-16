using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Npgsql;

namespace DataBase_Medical.Windows
{
    /// <summary>
    /// Логика взаимодействия для Window_HeadOfTheDepartment.xaml
    /// </summary>
    public partial class Window_HeadOfTheDepartment : Window
    {
        public Window_HeadOfTheDepartment()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = "Select \"Department_Name\" From \"Staff_Full\" Where \"Staff_Login\" = session_user";
                var reader = await new NpgsqlCommand(sql, ConnectionCarrier.Carrier.Connection).ExecuteReaderAsync();
                await reader.ReadAsync();
                var str = reader.GetString(0);
                reader.Close();

                this.Title = $"Заведующий отделением \"{str}\"";
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("hospitalized"))
                {
                    MessageBox.Show(ex.Message, $"Невозможно сменить лечащего врача{Environment.NewLine}" +
                        $"Пациент госпитализирован не в вашем отделении", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }
            finally
            {
                await ConnectionCarrier.Carrier.Connection.CloseAsync();
            }

            this.Title += " : " + await ConnectionCarrier.Carrier.GetCurrentFIO();
        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Grid_Department.Visibility = this.Grid_Staff.Visibility = this.Grid_Patient.Visibility = Visibility.Collapsed;

            var obj = sender as MenuItem;
            switch (obj.Header)
            {
                case "Больные":
                {
                    this.Grid_Patient.Visibility = Visibility.Visible;
                    Patient_MenuItem_Refresh_Click(sender, e);
                    await ConnectionCarrier.Carrier.WaitForConnectionAsync();
                    RaiseFirstSelection(Patient_DataGrid);
                    break;
                }
                case "Персонал":
                {
                    this.Grid_Staff.Visibility = Visibility.Visible;
                    //Staff_MenuItem_Refresh_Click(sender, e);
                    //await ConnectionCarrier.Carrier.WaitForConnectionAsync();
                    //RaiseFirstSelection(Staff_DataGrid);
                    break;
                }
                case "Отделения":
                {
                    this.Grid_Department.Visibility = Visibility.Visible;
                    //Department_MenuItem_Refresh_Click(sender, e);
                    //await ConnectionCarrier.Carrier.WaitForConnectionAsync();
                    //RaiseFirstSelection(Department_DataGrid);
                    break;
                }
            }
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
                    if (column.Key.Contains("Дата"))
                    {
                        dataRow[column.Key] = reader[column.Value].ToString().Split(' ')[0];
                    }
                    else
                    {
                        dataRow[column.Key] = reader[column.Value];
                    }
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

        #region Patient

        string? Patient_Selected_Id = string.Empty;
        (String Key, string Value)[] Staff_Id_Name;

        #region Loaders 

        private async void Patient_Load_Data(String id)
        {
            Dictionary<String, String> data;

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"Patient_Full_With_Staff\" Where \"Patient_Id\" = {id}";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                data = this.NpgsqlDataReader_To_Dictionary(reader, new Dictionary<string, string>()
                {
                    { "id", "Patient_Id" },
                    { "Имя", "Patient_Name" },
                    { "Фамилия", "Patient_Surname" },
                    { "Отчество", "Patient_Patronymic" },
                    { "Дата рождения", "Patient_BirthDay" },
                    { "Социальное положение", "SocialStatus_Name" },
                    { "Лечащий врач", "Staff_SurnameNP" }
                });

                reader.Close();

                Patient_Label_Surname.Content = data["Фамилия"];
                Patient_Label_Name.Content = data["Имя"];
                Patient_Label_Patronymic.Content = data["Отчество"];
                Patient_Label_Birthday.Content = data["Дата рождения"].Split(' ')[0];
                Patient_Label_SocialStatus.Content = data["Социальное положение"];
                Patient_Label_Doctor.Content = data["Лечащий врач"];

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
                return;
            }
            finally
            {
                await conn.CloseAsync();
            }

            Patient_Label_Doctor.Visibility = Visibility.Visible;
            Patient_ComboBox_Doctor.Visibility = Visibility.Collapsed;

            Patient_Load_DoctorAppointments();
            Patient_Load_Diseases();
        }

        bool isAppointmentHistory = false;
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

        bool isDiseaseHistory = false;
        private async void Patient_Load_Diseases()
        {
            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From get_patient_diseases({Patient_Selected_Id}) " +
                    $"{(isDiseaseHistory ? "" : " WHERE \"patientdiseases_end_date\" IS NULL")}";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                Dictionary<String, String> dict = null;

                if (isDiseaseHistory)
                {
                    dict = new Dictionary<string, string>()
                    {
                        { "id", "patientdiseases_id" },
                        { "Заболевание", "disease_name" },
                        { "Дата заболевания", "patientdiseases_start_date" },
                        { "Дата выздоровления", "patientdiseases_end_date" }
                    };
                }
                else
                {
                    dict = new Dictionary<string, string>()
                    {
                        { "id", "patientdiseases_id" },
                        { "Заболевание", "disease_name" },
                        { "Дата заболевания", "patientdiseases_start_date" }
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

        #endregion

        #region Search

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
                String sql = "Select * FROM \"PatientSurnameNP_Department\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "Patient_Id" },
                    { "ФИО", "Patient_SurnameNP" },
                    { "Название отделения", "Department_Name" }
                });

                this.Patient_DataGrid.ItemsSource = new DataView(dt);
                this.Patient_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;

                List<string> values = Patient_DataGrid.Items.Cast<DataRowView>()
                    .Select(row => row["Название отделения"].ToString())
                    .Distinct()
                    .ToList();
                values.Sort();
                if (!values.Contains(""))
                    values.Insert(0, "");

                Patient_ComboBox_SearchData.ItemsSource = values;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                String sql = "Select * FROM \"PatientSurnameNP_Department\" " +
                    $"Where lower(\"Patient_SurnameNP\") Like '%{Patient_TextBox_SearchData.Text.ToLower()}%' " +
                    $"And lower(\"Department_Name\") Like '%{Patient_ComboBox_SearchData.Text.ToLower()}%'";
                if (Patient_ComboBox_SearchData.Text.Length is 0)
                    sql += "  OR \"Department_Name\" IS NULL";

                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "Patient_Id" },
                    { "ФИО", "Patient_SurnameNP" },
                    { "Название отделения", "Department_Name" }
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

        #region Buttons

        private async void Patient_Button_Hospitalize_Click(object sender, RoutedEventArgs e)
        {
            Hospitalize_Cost_Grid.Visibility = Hospitalize_Cost_Grid.Visibility is Visibility.Visible ?
                Visibility.Collapsed : Visibility.Visible;
        }
       
        private async void Patient_Button_DeHospitalize_Click(object sender, RoutedEventArgs e)
        {
            if (Patient_Label_Department.Content.ToString() == "Нет")
            {
                MessageBox.Show("Нельзя выписать не госпитализированного пациента", "Ошибка выписки",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var sql = $"CALL discharge_patient({Patient_Selected_Id})";
            
            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await conn.OpenAsync();
                await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not hospitalized in your department"))
                {
                    MessageBox.Show("Пациент госпитализирован не в вашем отделении", "Ошибка выписки",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }

                return;
            }
            finally
            {
                await conn.CloseAsync();
            }

            Patient_Load_Data(Patient_Selected_Id);
        }

        private async void Patient_Button_ChangeDoctor_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            Dictionary<String, String> Staff_Id_Name = null;

            if (Patient_Button_ChangeDoctor.Content.ToString() == "Сменить врача")
            {
                var conn = ConnectionCarrier.Carrier.Connection;
                try
                {
                    await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                    String sql = "Select * From \"Current_DepartmentChief_Doctors\"";
                    var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                    Staff_Id_Name = this.NpgsqlDataReader_To_DictionaryList(reader, "Staff_Id", "Staff_SurnameNP");
                    Staff_Id_Name.Add("NULL", "");

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                finally
                {
                    await conn.CloseAsync();
                }

                var vs = Staff_Id_Name.Select(x => (x.Key, x.Value)).ToList().OrderBy(x => x.Value).ToArray();
                for (int i = 0; i < vs.Length; i++)
                {
                    if (vs[i].Value.Length is 0)
                        continue;
                    vs[i] = (vs[i].Key, $"{i}. {vs[i].Value}");
                }

                this.Staff_Id_Name = vs;

                Patient_ComboBox_Doctor.ItemsSource = vs.Select(x => x.Value);
                Patient_ComboBox_Doctor.SelectedIndex = Patient_ComboBox_Doctor.Items.IndexOf(Patient_Label_Doctor.Content);

                Patient_Button_ChangeDoctorCancel.Visibility = Visibility.Visible;
            }
            else
            {
                var selected_doctor_id = this.Staff_Id_Name.First(x => x.Value == Patient_ComboBox_Doctor.Text).Key;
                var conn = ConnectionCarrier.Carrier.Connection;
                try
                {
                    await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                    String sql = $"Update \"Patient\" Set \"Patient_CurrentDoctor_Id\" = {selected_doctor_id} Where \"Patient_Id\" = {Patient_Selected_Id}";
                    await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();
                    Patient_Load_Data(Patient_Selected_Id);
                    Patient_Button_ChangeDoctorCancel.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("hospitalized"))
                    {
                        MessageBox.Show($"Пациент госпитализирован не в вашем отделении", $"Невозможно сменить лечащего врача", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }
                finally
                {
                    await conn.CloseAsync();
                }
            }

            Patient_Label_Doctor.Visibility = Patient_Label_Doctor.Visibility is Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            Patient_ComboBox_Doctor.Visibility = Patient_ComboBox_Doctor.Visibility is Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;


            if (Patient_Button_ChangeDoctor.Content.ToString() == "Сменить врача")
            {
                Patient_Button_ChangeDoctor.Content = "Подтвердить изменение";
            }
            else
            {
                Patient_Button_ChangeDoctor.Content = "Сменить врача";
            }
        }

        private async void Patient_Button_ChangeDoctorCancel_Click(object sender, RoutedEventArgs e)
        {
            Patient_Button_ChangeDoctorCancel.Visibility = Visibility.Collapsed;
            Patient_Button_ChangeDoctor.Content = "Сменить врача";

            Patient_Label_Doctor.Visibility = Patient_Label_Doctor.Visibility is Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            Patient_ComboBox_Doctor.Visibility = Patient_ComboBox_Doctor.Visibility is Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void Patient_Button_StayAddOk_Click(object sender, RoutedEventArgs e)
        {
            var curr_dep = await ConnectionCarrier.Carrier.GetCurrentStaffDepartment();
            if (curr_dep == "NULL" | curr_dep.Length is 0)
            {
                MessageBox.Show("Вы не закреплены ни за каким отделением", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Int32.TryParse(Stay_TextBox_Cost.Text, out int cost))
            {
                MessageBox.Show("Невозможно считать стоимость госпитализации", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sql = "INSERT INTO \"HospitalStay\" " +
                "(\"HospitalStay_Patient_Id\", \"HospitalStay_Start_Date\", \"HospitalStay_Department_Id\", \"HospitalStay_Cost\") " +
                $"VALUES ({Patient_Selected_Id}, '{DateTime.Now:yyyy-MM-dd}', {curr_dep}, {cost})";

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await conn.OpenAsync();
                await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already hospitalized"))
                {
                    MessageBox.Show("Пациент уже госпитализирован в другом отделении", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }

                return;
            }
            finally
            {
                await conn.CloseAsync();
            }

            Patient_Load_Data(Patient_Selected_Id);
        }

        private async void Patient_Button_DiseaseHistory_Click(object sender, RoutedEventArgs e)
        {
            isDiseaseHistory = !isDiseaseHistory;

            Patient_Diseases_Title_Label.Content = isDiseaseHistory ? "История болезней" : "Текущие болезни";

            var button = sender as Button;
            button.Content = isDiseaseHistory ? "Текущие болезни" : "История болезней";

            Patient_Load_Diseases();
        }

        private async void Patient_Button_AppointmentHistory_Click(object sender, RoutedEventArgs e)
        {
            isAppointmentHistory = !isAppointmentHistory;

            Appointment_Title_Label.Content = isAppointmentHistory ? "История назначений" : "Текущие назначения";

            var button = sender as Button;
            button.Content = isDiseaseHistory ? "Текущие назначения" : "История назначений";

            Patient_Load_DoctorAppointments();
        }

        #endregion

        #endregion
    }
}
