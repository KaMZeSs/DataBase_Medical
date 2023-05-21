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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Npgsql;

namespace DataBase_Medical.Windows
{
    /// <summary>
    /// Логика взаимодействия для Window_Chief.xaml
    /// </summary>
    public partial class Window_Chief : Window
    {
        public Window_Chief()
        {
            InitializeComponent();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = "Главный врач: " + await ConnectionCarrier.Carrier.GetCurrentFIO();
            this.Grid_Department.Visibility = this.Grid_Staff.Visibility = this.Grid_Patient.Visibility = Visibility.Collapsed;
            this.Grid_Patient.Visibility = Visibility.Visible;
            Patient_MenuItem_Refresh_Click(sender, e);
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
                    Staff_MenuItem_Refresh_Click(sender, e);
                    await ConnectionCarrier.Carrier.WaitForConnectionAsync();
                    RaiseFirstSelection(Staff_DataGrid);
                    break;
                }
                case "Отделения":
                {
                    this.Grid_Department.Visibility = Visibility.Visible;
                    Department_MenuItem_Refresh_Click(sender, e);
                    await ConnectionCarrier.Carrier.WaitForConnectionAsync();
                    RaiseFirstSelection(Department_DataGrid);
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
            var bool_columns_list = new String[]
            {
                "Госпитализирован"
            };
            var dates_columns_list = new String[]
            {
                "Выписка",
                "Госпитализация",
                "Выздоровление",
                "Дата заболевания",
                "Заболевание"
            };

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
                    if (dates_columns_list.Contains(column.Key))
                    {
                        dataRow[column.Key] = reader[column.Value].ToString().Split(' ')[0];
                    }
                    else if (bool_columns_list.Contains(column.Key))
                    {
                        var vs = reader[column.Value].ToString();
                        dataRow[column.Key] = vs == "True" ? "Да" : "Нет";
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

            Patient_Load_DoctorAppointments();
            Patient_Load_Diseases();
            Patient_Load_Stays();
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
                        { "Болезнь", "disease_name" },
                        { "Заболевание", "patientdiseases_start_date" },
                        { "Выздоровление", "patientdiseases_end_date" }
                    };
                }
                else
                {
                    dict = new Dictionary<string, string>()
                    {
                        { "id", "patientdiseases_id" },
                        { "Болезнь", "disease_name" },
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

        private async void Patient_Load_Stays()
        {
            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"HospitalStay_Full_Info\" Where \"HospitalStay_Patient_Id\" = {Patient_Selected_Id} ORDER BY \"HospitalStay_Start_Date\" DESC";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                Dictionary<String, String> dict = new Dictionary<string, string>()
                    {
                        { "id", "HospitalStay_Id" },
                        { "Госпитализация", "HospitalStay_Start_Date" },
                        { "Выписка", "HospitalStay_End_Date" },
                        { "Отделение", "Department_Name" },
                        { "Стоимость", "HospitalStay_Cost" },
                        { "Полная сумма", "fullstay_cost" },
                    };

                var dt = this.NpgsqlDataReader_To_DataTable(reader, dict);

                reader.Close();

                this.Patient_Stays_DataGrid.ItemsSource = new DataView(dt);
                this.Patient_Stays_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
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

        #region Staff

        string? Staff_Selected_Id = string.Empty;
        Dictionary<String, string> JobTitles = new()
        {
            { "chief", "Главный врач" },
            { "department_chief", "Заведующий отделением" },
            { "doctor" , "Врач" },
            { "admin" , "Администратор" }
        };


        private async void Staff_Load_Data(String id)
        {
            Dictionary<String, String> data;

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"Staff_Full\" Where \"Staff_Id\" = {id} And role = 'doctor'";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                data = this.NpgsqlDataReader_To_Dictionary(reader, new Dictionary<string, string>()
                {
                    { "id", "Staff_Id" },
                    { "Имя", "Staff_Name" },
                    { "Фамилия", "Staff_Surname" },
                    { "Отчество", "Staff_Patronymic" },
                    { "Название отделения", "Department_Name" },
                    { "Название категории", "Category_Name" },
                    { "Должность", "role" }
                });

                Staff_Label_Name.Content = data["Имя"];
                Staff_Label_Surname.Content = data["Фамилия"];
                Staff_Label_Patronymic.Content = data["Отчество"];
                Staff_Label_Department.Content = data["Название отделения"];
                Staff_Label_Category.Content = data["Название категории"];
                Staff_Label_JobTitle.Content = JobTitles.Where(x => x.Key == data["Должность"]).FirstOrDefault().Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }

            Staff_Load_Patiens();
        }

        private async void Staff_MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = "Select * FROM \"Doctors_SurnameNP\"";
                //String sql = "Select * FROM \"Staff_SurnameNP\" WHERE role = 'doctor' Order By \"Staff_Id\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "Staff_Id" },
                    { "ФИО", "Staff_SurnameNP" },
                    { "Название отделения", "Department_Name" }
                });


                this.Staff_DataGrid.ItemsSource = new DataView(dt);
                this.Staff_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;

                List<string> values = Staff_DataGrid.Items.Cast<DataRowView>()
                    .Select(row => row["Название отделения"].ToString())
                    .Distinct()
                    .ToList();
                if (!values.Contains(""))
                    values.Insert(0, "");
                Staff_ComboBox_SearchData.ItemsSource = values;
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }
            if (Staff_Selected_Id != string.Empty)
            {
                Staff_Load_Data(Staff_Selected_Id);
            }
            else
            {
                RaiseFirstSelection(Staff_DataGrid);
            }
        }

        private void Staff_MenuItem_Search_Click(object sender, RoutedEventArgs e)
        {
            this.Staff_Grid_Search.Visibility =
               this.Staff_Grid_Search.Visibility is Visibility.Visible ?
               Visibility.Collapsed : Visibility.Visible;
        }

        private void Staff_DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (Staff_DataGrid.SelectedIndex is -1)
                return;

            var vs = Staff_DataGrid.SelectedIndex;
            var row = Staff_DataGrid.Items[vs] as DataRowView;
            Staff_Selected_Id = row?["id"].ToString();

            Staff_Load_Data(Staff_Selected_Id);
        }

        private async void Staff_Button_SearchData_Click(object sender, RoutedEventArgs e)
        {
            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * FROM \"Doctors_SurnameNP\"";

                if (Staff_TextBox_SearchData.Text.Length is not 0)
                {
                    sql += $" Where lower(\"Staff_SurnameNP\") Like '%{Staff_TextBox_SearchData.Text.ToLower()}%'";
                }
                if (Staff_ComboBox_SearchData.Text.Length is not 0)
                {
                    if (sql.Contains("Where"))
                    {
                        sql += $" And lower(\"Department_Name\") Like '%{Staff_ComboBox_SearchData.Text.ToLower()}%'";
                    }
                    else
                    {
                        sql += $" Where lower(\"Department_Name\") Like '%{Staff_ComboBox_SearchData.Text.ToLower()}%'";
                    }
                }

                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "Staff_Id" },
                    { "ФИО", "Staff_SurnameNP" },
                    { "Название отделения", "Department_Name" }
                });

                this.Staff_DataGrid.ItemsSource = new DataView(dt);
                this.Staff_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        private async void Staff_Load_Patiens()
        {
            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"Patient_SurnameNP_isHospitalized\" Where \"Patient_CurrentDoctor_Id\" = {Staff_Selected_Id}";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "Patient_Id" },
                    { "ФИО", "Patient_SurnameNP" },
                    { "Госпитализирован", "isHospitalized" }
                });

                this.Staff_Patient_DataGrid.ItemsSource = new DataView(dt);
                this.Staff_Patient_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
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

        private void Staff_Patient_DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Staff_Patient_DataGrid.SelectedIndex is -1)
                return;

            var vs = Staff_Patient_DataGrid.SelectedIndex;
            var row = Staff_Patient_DataGrid.Items[vs] as DataRowView;
            var selected_patient_id = row?["id"].ToString();



            Patient_MenuItem_Refresh_Click(sender, e);

            var patients = Patient_DataGrid.Items.Cast<DataRowView>();
            var sel = patients.Where(x => x?["id"].ToString() == selected_patient_id);

            if (!sel.Any())
            {
                return;
            }
            var row_view = sel.First();
            this.Grid_Patient.Visibility = Visibility.Visible;
            this.Grid_Staff.Visibility = Visibility.Collapsed;

            var index = Patient_DataGrid.Items.IndexOf(row_view);

            Patient_DataGrid.SelectedIndex = index;
        }

        #endregion

        #region Department

        string? Department_Selected_Id = string.Empty;

        private async void Department_Load_Data(String id)
        {
            Dictionary<String, String> data;

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"Department\" Where \"Department_Exists\" = true AND \"Department_Id\" = {id}";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                data = this.NpgsqlDataReader_To_Dictionary(reader, new Dictionary<string, string>()
                {
                    { "id", "Department_Id" },
                    { "Название отделения", "Department_Name" },
                    { "Телефон отделения", "Department_Phone" },
                    { "Койки отделения", "Department_Beds" }
                });

                Department_Label_Name.Content = data["Название отделения"];
                Department_Label_Phone.Content = data["Телефон отделения"];
                Department_Label_Beds.Content = data["Койки отделения"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }
            Department_Load_Patiens();
        }

        private async void Department_MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Department_Label_Name.Content = "";

            Department_Label_Phone.Content = "";

            Department_Label_Beds.Content = "";

            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = "Select * From \"Department\" Where \"Department_Exists\" = true Order By \"Department_Id\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "Department_Id" },
                    { "Название отделения", "Department_Name" }
                });

                this.Department_DataGrid.ItemsSource = new DataView(dt);
                this.Department_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }
            if (Department_Selected_Id != string.Empty)
            {
                Department_Load_Data(Department_Selected_Id);
            }
            else
            {
                RaiseFirstSelection(Department_DataGrid);
            }
        }

        private void Department_MenuItem_Search_Click(object sender, RoutedEventArgs e)
        {
            this.Department_Grid_Search.Visibility =
               this.Department_Grid_Search.Visibility is Visibility.Visible ?
               Visibility.Collapsed : Visibility.Visible;
        }

        private void Department_DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (Department_DataGrid.SelectedIndex is -1)
                return;

            var vs = Department_DataGrid.SelectedIndex;
            var row = Department_DataGrid.Items[vs] as DataRowView;
            Department_Selected_Id = row?["id"].ToString();

            Department_Load_Data(Department_Selected_Id);
        }

        private async void Department_Button_SearchData_Click(object sender, RoutedEventArgs e)
        {
            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"Department\" Where lower(\"Department_Name\") Like '%{Department_TextBox_SearchData.Text.ToLower()}%' Order By \"Department_Id\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "Department_Id" },
                    { "Название отделения", "Department_Name" }
                });

                this.Department_DataGrid.ItemsSource = new DataView(dt);
                this.Department_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        private async void Department_Load_Patiens()
        {
            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"Patient_SurnameNP_Department_Id\" Where \"Department_Id\" = {Department_Selected_Id}";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "Patient_Id" },
                    { "ФИО", "Patient_SurnameNP" }
                });

                this.Department_Patient_DataGrid.ItemsSource = new DataView(dt);
                this.Department_Patient_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
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

        private void Department_Patient_DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Department_Patient_DataGrid.SelectedIndex is -1)
                return;

            var vs = Department_Patient_DataGrid.SelectedIndex;
            var row = Department_Patient_DataGrid.Items[vs] as DataRowView;
            var selected_patient_id = row?["id"].ToString();



            Patient_MenuItem_Refresh_Click(sender, e);

            var patients = Patient_DataGrid.Items.Cast<DataRowView>();
            var sel = patients.Where(x => x?["id"].ToString() == selected_patient_id);

            if (!sel.Any())
            {
                return;
            }
            var row_view = sel.First();
            this.Grid_Patient.Visibility = Visibility.Visible;
            this.Grid_Department.Visibility = Visibility.Collapsed;

            var index = Patient_DataGrid.Items.IndexOf(row_view);

            Patient_DataGrid.SelectedIndex = index;
        }

        #endregion

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Statistic_View_Statistic_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Statistic_Export_Statistic_Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
