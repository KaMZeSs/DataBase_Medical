using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Npgsql;

using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace DataBase_Medical.Windows
{
    /// <summary>
    /// Логика взаимодействия для Window_Administrator.xaml
    /// </summary>
    public partial class Window_Administrator : Window
    {
        public Window_Administrator()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = "Администратор: " + await ConnectionCarrier.Carrier.GetCurrentFIO();
        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Grid_Patient.Visibility = this.Grid_Staff.Visibility = this.Grid_Department.Visibility =
                this.Grid_Disease.Visibility = this.Grid_Procedure.Visibility =
                this.Grid_SocialStatus.Visibility = this.Grid_Category.Visibility = Visibility.Collapsed;

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
                case "Заболевания":
                {
                    this.Grid_Disease.Visibility = Visibility.Visible;
                    Disease_MenuItem_Refresh_Click(sender, e);
                    await ConnectionCarrier.Carrier.WaitForConnectionAsync();
                    RaiseFirstSelection(Disease_DataGrid);
                    break;
                }
                case "Процедуры":
                {
                    this.Grid_Procedure.Visibility = Visibility.Visible;
                    Procedure_MenuItem_Refresh_Click(sender, e);
                    await ConnectionCarrier.Carrier.WaitForConnectionAsync();
                    RaiseFirstSelection(Procedure_DataGrid);
                    break;
                }
                case "Социальное положение":
                {
                    this.Grid_SocialStatus.Visibility = Visibility.Visible;
                    SocialStatus_MenuItem_Refresh_Click(sender, e);
                    await ConnectionCarrier.Carrier.WaitForConnectionAsync();
                    RaiseFirstSelection(SocialStatus_DataGrid);
                    break;
                }
                case "Категории":
                {
                    this.Grid_Category.Visibility = Visibility.Visible;
                    Category_MenuItem_Refresh_Click(sender, e);
                    await ConnectionCarrier.Carrier.WaitForConnectionAsync();
                    RaiseFirstSelection(Category_DataGrid);
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

        bool isReducting = false;

        #region Category

        string? Category_Selected_Id = string.Empty;

        private async void Category_Load_Data(String id)
        {
            Dictionary<String, String> data;

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"Category\" Where \"Category_Id\" = {id}";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                data = this.NpgsqlDataReader_To_Dictionary(reader, new Dictionary<string, string>()
                {
                    { "id", "Category_Id" },
                    { "Название категории", "Category_Name" },
                });

                Category_Label_Name.Content = data["Название категории"];
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

        private async void Category_MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Category_Label_Name.Content = "";
            Category_Label_Name.Visibility = Visibility.Visible;
            Category_TextBox_Name.Visibility = Visibility.Collapsed;

            Category_Button_Reduct.IsEnabled = true;
            Category_Button_Ok.IsEnabled = false;
            Category_Button_Cancel.IsEnabled = false;

            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = "Select * From \"Category\" Order By \"Category_Id\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "Category_Id" },
                    { "Название категории", "Category_Name" }
                });

                this.Category_DataGrid.ItemsSource = new DataView(dt);
                this.Category_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }
            if (Category_Selected_Id != string.Empty)
            {
                Category_Load_Data(Category_Selected_Id);
            }
            else
            {
                RaiseFirstSelection(Category_DataGrid);
            }
        }

        private void Category_MenuItem_Search_Click(object sender, RoutedEventArgs e)
        {
            this.Category_Grid_Search.Visibility =
               this.Category_Grid_Search.Visibility is Visibility.Visible ?
               Visibility.Collapsed : Visibility.Visible;
        }

        private void Category_MenuItem_Add_Click(object sender, RoutedEventArgs e)
        {
            isReducting = false;
            Category_Label_Name.Visibility = Visibility.Collapsed;
            Category_TextBox_Name.Visibility = Visibility.Visible;

            Category_Button_Reduct.IsEnabled = false;
            Category_Button_Ok.IsEnabled = true;
            Category_Button_Cancel.IsEnabled = true;

            Category_TextBox_Name.Text = "";
        }

        private void Category_DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (Category_DataGrid.SelectedIndex is -1)
                return;
            Category_Label_Name.Visibility = Visibility.Visible;
            Category_TextBox_Name.Visibility = Visibility.Collapsed;

            Category_Button_Reduct.IsEnabled = true;
            Category_Button_Ok.IsEnabled = false;
            Category_Button_Cancel.IsEnabled = false;

            var vs = Category_DataGrid.SelectedIndex;
            var row = Category_DataGrid.Items[vs] as DataRowView;
            Category_Selected_Id = row?["id"].ToString();

            Category_Load_Data(Category_Selected_Id);
        }

        private void Category_Button_Reduct_Click(object sender, RoutedEventArgs e)
        {
            isReducting = true;
            if (Category_Selected_Id == String.Empty)
                return;

            Category_Label_Name.Visibility = Visibility.Collapsed;
            Category_TextBox_Name.Visibility = Visibility.Visible;

            Category_Button_Reduct.IsEnabled = false;
            Category_Button_Ok.IsEnabled = true;
            Category_Button_Cancel.IsEnabled = true;

            Category_TextBox_Name.Text = Category_Label_Name.Content.ToString();
        }

        private void Category_Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Category_Label_Name.Visibility = Visibility.Visible;
            Category_TextBox_Name.Visibility = Visibility.Collapsed;

            Category_Button_Reduct.IsEnabled = true;
            Category_Button_Ok.IsEnabled = false;
            Category_Button_Cancel.IsEnabled = false;
        }

        private async void Category_Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            if (Category_TextBox_Name.Text.Length is 0)
            {
                MessageBox.Show($"Название не может быть пустым",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string sql;
            if (!isReducting)
            {
                Category_Selected_Id = (await ConnectionCarrier.Carrier.GetCurrentSequenceId("Category")).ToString();
                sql = $"Insert Into \"Category\" (\"Category_Name\") Values ('{Category_TextBox_Name.Text}')";
            }
            else
            {
                sql = $"Update \"Category\" Set \"Category_Name\" = '{Category_TextBox_Name.Text}' Where \"Category_Id\" = {Category_Selected_Id}";
            }

            if (!isReducting | (isReducting & Category_Label_Name.Content.ToString() != Category_TextBox_Name.Text))
            {
                var conn = ConnectionCarrier.Carrier.Connection;

                try
                {
                    await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                    await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();

                    Category_Label_Name.Content = Category_TextBox_Name.Text;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("повторяющееся"))
                    {
                        MessageBox.Show($"Невозможно выполнить операцию{Environment.NewLine}Такое название уже есть",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Непредвиденная ошибка{Environment.NewLine}{ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }
                finally
                {
                    await conn.CloseAsync();
                }
            }

            Category_MenuItem_Refresh_Click(sender, e);
        }

        private async void Category_Button_SearchData_Click(object sender, RoutedEventArgs e)
        {
            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"Category\" Where lower(\"Category_Name\") Like '%{Category_TextBox_SearchData.Text.ToLower()}%' Order By \"Category_Id\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "Category_Id" },
                    { "Название категории", "Category_Name" }
                });

                this.Category_DataGrid.ItemsSource = new DataView(dt);
                this.Category_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
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

        #region SocialStatus

        string? SocialStatus_Selected_Id = string.Empty;

        private async void SocialStatus_Load_Data(String id)
        {
            Dictionary<String, String> data;

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"SocialStatus\" Where \"SocialStatus_Id\" = {id}";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                data = this.NpgsqlDataReader_To_Dictionary(reader, new Dictionary<string, string>()
                {
                    { "id", "SocialStatus_Id" },
                    { "Название социального статуса", "SocialStatus_Name" },
                });

                SocialStatus_Label_Name.Content = data["Название социального статуса"];
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

        private async void SocialStatus_MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            SocialStatus_Label_Name.Content = "";
            SocialStatus_Label_Name.Visibility = Visibility.Visible;
            SocialStatus_TextBox_Name.Visibility = Visibility.Collapsed;

            SocialStatus_Button_Reduct.IsEnabled = true;
            SocialStatus_Button_Ok.IsEnabled = false;
            SocialStatus_Button_Cancel.IsEnabled = false;

            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = "Select * From \"SocialStatus\" Order By \"SocialStatus_Id\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "SocialStatus_Id" },
                    { "Название социального статуса", "SocialStatus_Name" }
                });

                this.SocialStatus_DataGrid.ItemsSource = new DataView(dt);
                this.SocialStatus_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }
            if (SocialStatus_Selected_Id != string.Empty)
            {
                SocialStatus_Load_Data(SocialStatus_Selected_Id);
            }
            else
            {
                RaiseFirstSelection(SocialStatus_DataGrid);
            }
        }

        private void SocialStatus_MenuItem_Search_Click(object sender, RoutedEventArgs e)
        {
            this.SocialStatus_Grid_Search.Visibility =
               this.SocialStatus_Grid_Search.Visibility is Visibility.Visible ?
               Visibility.Collapsed : Visibility.Visible;
        }

        private void SocialStatus_MenuItem_Add_Click(object sender, RoutedEventArgs e)
        {
            isReducting = false;
            SocialStatus_Label_Name.Visibility = Visibility.Collapsed;
            SocialStatus_TextBox_Name.Visibility = Visibility.Visible;

            SocialStatus_Button_Reduct.IsEnabled = false;
            SocialStatus_Button_Ok.IsEnabled = true;
            SocialStatus_Button_Cancel.IsEnabled = true;

            SocialStatus_TextBox_Name.Text = "";
        }

        private void SocialStatus_DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (SocialStatus_DataGrid.SelectedIndex is -1)
                return;
            SocialStatus_Label_Name.Visibility = Visibility.Visible;
            SocialStatus_TextBox_Name.Visibility = Visibility.Collapsed;

            SocialStatus_Button_Reduct.IsEnabled = true;
            SocialStatus_Button_Ok.IsEnabled = false;
            SocialStatus_Button_Cancel.IsEnabled = false;

            var vs = SocialStatus_DataGrid.SelectedIndex;
            var row = SocialStatus_DataGrid.Items[vs] as DataRowView;
            SocialStatus_Selected_Id = row?["id"].ToString();

            SocialStatus_Load_Data(SocialStatus_Selected_Id);
        }

        private void SocialStatus_Button_Reduct_Click(object sender, RoutedEventArgs e)
        {
            if (SocialStatus_Selected_Id == String.Empty)
                return;

            isReducting = true;

            SocialStatus_Label_Name.Visibility = Visibility.Collapsed;
            SocialStatus_TextBox_Name.Visibility = Visibility.Visible;

            SocialStatus_Button_Reduct.IsEnabled = false;
            SocialStatus_Button_Ok.IsEnabled = true;
            SocialStatus_Button_Cancel.IsEnabled = true;

            SocialStatus_TextBox_Name.Text = SocialStatus_Label_Name.Content.ToString();
        }

        private void SocialStatus_Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            SocialStatus_Label_Name.Visibility = Visibility.Visible;
            SocialStatus_TextBox_Name.Visibility = Visibility.Collapsed;

            SocialStatus_Button_Reduct.IsEnabled = true;
            SocialStatus_Button_Ok.IsEnabled = false;
            SocialStatus_Button_Cancel.IsEnabled = false;
        }

        private async void SocialStatus_Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            if (SocialStatus_TextBox_Name.Text.Length is 0)
            {
                MessageBox.Show($"Название не может быть пустым",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string sql;
            if (!isReducting)
            {
                SocialStatus_Selected_Id = (await ConnectionCarrier.Carrier.GetCurrentSequenceId("SocialStatus")).ToString();
                sql = $"Insert Into \"SocialStatus\" (\"SocialStatus_Name\") Values ('{SocialStatus_TextBox_Name.Text}')";
            }
            else
            {
                sql = $"Update \"SocialStatus\" Set \"SocialStatus_Name\" = '{SocialStatus_TextBox_Name.Text}' Where \"SocialStatus_Id\" = {SocialStatus_Selected_Id}";
            }

            if (!isReducting | (isReducting & SocialStatus_Label_Name.Content.ToString() != SocialStatus_TextBox_Name.Text))
            {
                var conn = ConnectionCarrier.Carrier.Connection;

                try
                {
                    await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                    await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();

                    SocialStatus_Label_Name.Content = SocialStatus_TextBox_Name.Text;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("повторяющееся"))
                    {
                        MessageBox.Show($"Невозможно выполнить операцию{Environment.NewLine}Такое название уже есть",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Непредвиденная ошибка{Environment.NewLine}{ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }
                finally
                {
                    await conn.CloseAsync();
                }
            }

            SocialStatus_MenuItem_Refresh_Click(sender, e);
        }

        private async void SocialStatus_Button_SearchData_Click(object sender, RoutedEventArgs e)
        {
            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"SocialStatus\" Where lower(\"SocialStatus_Name\") Like '%{SocialStatus_TextBox_SearchData.Text.ToLower()}%' Order By \"SocialStatus_Id\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "SocialStatus_Id" },
                    { "Название социального статуса", "SocialStatus_Name" }
                });

                this.SocialStatus_DataGrid.ItemsSource = new DataView(dt);
                this.SocialStatus_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
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

        #region Procedure

        string? Procedure_Selected_Id = string.Empty;

        private async void Procedure_Load_Data(String id)
        {
            Dictionary<String, String> data;

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"Procedure\" Where \"Procedure_Id\" = {id}";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                data = this.NpgsqlDataReader_To_Dictionary(reader, new Dictionary<string, string>()
                {
                    { "id", "Procedure_Id" },
                    { "Название процедуры", "Procedure_Name" },
                    { "Стоимость процедуры", "Procedure_Cost" }
                });

                Procedure_Label_Name.Content = data["Название процедуры"];
                Procedure_Label_Cost.Content = data["Стоимость процедуры"];
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

        private async void Procedure_MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Procedure_Label_Name.Content = "";
            Procedure_Label_Name.Visibility = Visibility.Visible;
            Procedure_TextBox_Name.Visibility = Visibility.Collapsed;

            Procedure_Label_Cost.Content = "";
            Procedure_Label_Cost.Visibility = Visibility.Visible;
            Procedure_TextBox_Cost.Visibility = Visibility.Collapsed;

            Procedure_Button_Reduct.IsEnabled = true;
            Procedure_Button_Ok.IsEnabled = false;
            Procedure_Button_Cancel.IsEnabled = false;

            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = "Select * From \"Procedure\" Order By \"Procedure_Id\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "Procedure_Id" },
                    { "Название процедуры", "Procedure_Name" }
                });

                this.Procedure_DataGrid.ItemsSource = new DataView(dt);
                this.Procedure_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }
            if (Procedure_Selected_Id != string.Empty)
            {
                Procedure_Load_Data(Procedure_Selected_Id);
            }
            else
            {
                RaiseFirstSelection(Procedure_DataGrid);
            }
        }

        private void Procedure_MenuItem_Search_Click(object sender, RoutedEventArgs e)
        {
            this.Procedure_Grid_Search.Visibility =
               this.Procedure_Grid_Search.Visibility is Visibility.Visible ?
               Visibility.Collapsed : Visibility.Visible;
        }

        private void Procedure_MenuItem_Add_Click(object sender, RoutedEventArgs e)
        {
            isReducting = false;
            Procedure_Label_Name.Visibility = Visibility.Collapsed;
            Procedure_TextBox_Name.Visibility = Visibility.Visible;

            Procedure_Label_Cost.Visibility = Visibility.Collapsed;
            Procedure_TextBox_Cost.Visibility = Visibility.Visible;

            Procedure_Button_Reduct.IsEnabled = false;
            Procedure_Button_Ok.IsEnabled = true;
            Procedure_Button_Cancel.IsEnabled = true;

            Procedure_TextBox_Name.Text = "";
            Procedure_TextBox_Cost.Text = "";
        }

        private void Procedure_DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (Procedure_DataGrid.SelectedIndex is -1)
                return;
            Procedure_Label_Name.Visibility = Visibility.Visible;
            Procedure_TextBox_Name.Visibility = Visibility.Collapsed;

            Procedure_Label_Cost.Visibility = Visibility.Visible;
            Procedure_TextBox_Cost.Visibility = Visibility.Collapsed;

            Procedure_Button_Reduct.IsEnabled = true;
            Procedure_Button_Ok.IsEnabled = false;
            Procedure_Button_Cancel.IsEnabled = false;

            var vs = Procedure_DataGrid.SelectedIndex;
            var row = Procedure_DataGrid.Items[vs] as DataRowView;
            Procedure_Selected_Id = row?["id"].ToString();

            Procedure_Load_Data(Procedure_Selected_Id);
        }

        private void Procedure_Button_Reduct_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure_Selected_Id == String.Empty)
                return;

            isReducting = true;

            Procedure_Label_Name.Visibility = Visibility.Collapsed;
            Procedure_TextBox_Name.Visibility = Visibility.Visible;

            Procedure_Label_Cost.Visibility = Visibility.Collapsed;
            Procedure_TextBox_Cost.Visibility = Visibility.Visible;

            Procedure_Button_Reduct.IsEnabled = false;
            Procedure_Button_Ok.IsEnabled = true;
            Procedure_Button_Cancel.IsEnabled = true;

            Procedure_TextBox_Name.Text = Procedure_Label_Name.Content.ToString();
            Procedure_TextBox_Cost.Text = Procedure_Label_Cost.Content.ToString();
        }

        private void Procedure_Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Procedure_Label_Name.Visibility = Visibility.Visible;
            Procedure_TextBox_Name.Visibility = Visibility.Collapsed;

            Procedure_Label_Cost.Visibility = Visibility.Visible;
            Procedure_TextBox_Cost.Visibility = Visibility.Collapsed;

            Procedure_Button_Reduct.IsEnabled = true;
            Procedure_Button_Ok.IsEnabled = false;
            Procedure_Button_Cancel.IsEnabled = false;
        }

        private async void Procedure_Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure_TextBox_Name.Text.Length is 0)
            {
                MessageBox.Show($"Название не может быть пустым",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                Double.Parse(Procedure_TextBox_Cost.Text);
            }
            catch
            {
                MessageBox.Show($"Невозможно считать стоимость. Повторите ввод",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string sql;
            if (!isReducting)
            {
                Procedure_Selected_Id = (await ConnectionCarrier.Carrier.GetCurrentSequenceId("Procedure")).ToString();
                sql = $"Insert Into \"Procedure\" (\"Procedure_Name\", \"Procedure_Cost\") Values ('{Procedure_TextBox_Name.Text}', '{Procedure_TextBox_Cost.Text}')";
            }
            else
            {
                sql = $"Update \"Procedure\" Set \"Procedure_Name\" = '{Procedure_TextBox_Name.Text}', " +
                    $"\"Procedure_Cost\" = '{Procedure_TextBox_Cost.Text}' Where \"Procedure_Id\" = {Procedure_Selected_Id}";
            }

            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();

                Procedure_Label_Name.Content = Procedure_TextBox_Name.Text;
            }
            catch (Exception ex)
            {

                MessageBox.Show($"Непредвиденная ошибка{Environment.NewLine}{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                await conn.CloseAsync();
            }

            Procedure_MenuItem_Refresh_Click(sender, e);
        }

        private async void Procedure_Button_SearchData_Click(object sender, RoutedEventArgs e)
        {
            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"Procedure\" Where lower(\"Procedure_Name\") Like '%{Procedure_TextBox_SearchData.Text.ToLower()}%' Order By \"Procedure_Id\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "Procedure_Id" },
                    { "Название процедуры", "Procedure_Name" }
                });

                this.Procedure_DataGrid.ItemsSource = new DataView(dt);
                this.Procedure_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
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

        #region Disease

        string? Disease_Selected_Id = string.Empty;

        private async void Disease_Load_Data(String id)
        {
            Dictionary<String, String> data;

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"Disease\" Where \"Disease_Id\" = {id}";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                data = this.NpgsqlDataReader_To_Dictionary(reader, new Dictionary<string, string>()
                {
                    { "id", "Disease_Id" },
                    { "Название заболевания", "Disease_Name" },
                });

                Disease_Label_Name.Content = data["Название заболевания"];
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

        private async void Disease_MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Disease_Label_Name.Content = "";
            Disease_Label_Name.Visibility = Visibility.Visible;
            Disease_TextBox_Name.Visibility = Visibility.Collapsed;

            Disease_Button_Reduct.IsEnabled = true;
            Disease_Button_Ok.IsEnabled = false;
            Disease_Button_Cancel.IsEnabled = false;

            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = "Select * From \"Disease\" Order By \"Disease_Id\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "Disease_Id" },
                    { "Название заболевания", "Disease_Name" }
                });

                this.Disease_DataGrid.ItemsSource = new DataView(dt);
                this.Disease_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }
            if (Disease_Selected_Id != string.Empty)
            {
                Disease_Load_Data(Disease_Selected_Id);
            }
            else
            {
                RaiseFirstSelection(Disease_DataGrid);
            }
        }

        private void Disease_MenuItem_Search_Click(object sender, RoutedEventArgs e)
        {
            this.Disease_Grid_Search.Visibility =
               this.Disease_Grid_Search.Visibility is Visibility.Visible ?
               Visibility.Collapsed : Visibility.Visible;
        }

        private void Disease_MenuItem_Add_Click(object sender, RoutedEventArgs e)
        {
            isReducting = false;
            Disease_Label_Name.Visibility = Visibility.Collapsed;
            Disease_TextBox_Name.Visibility = Visibility.Visible;

            Disease_Button_Reduct.IsEnabled = false;
            Disease_Button_Ok.IsEnabled = true;
            Disease_Button_Cancel.IsEnabled = true;

            Disease_TextBox_Name.Text = "";
        }

        private void Disease_DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (Disease_DataGrid.SelectedIndex is -1)
                return;
            Disease_Label_Name.Visibility = Visibility.Visible;
            Disease_TextBox_Name.Visibility = Visibility.Collapsed;

            Disease_Button_Reduct.IsEnabled = true;
            Disease_Button_Ok.IsEnabled = false;
            Disease_Button_Cancel.IsEnabled = false;

            var vs = Disease_DataGrid.SelectedIndex;
            var row = Disease_DataGrid.Items[vs] as DataRowView;
            Disease_Selected_Id = row?["id"].ToString();

            Disease_Load_Data(Disease_Selected_Id);
        }

        private void Disease_Button_Reduct_Click(object sender, RoutedEventArgs e)
        {
            if (Disease_Selected_Id == String.Empty)
                return;

            isReducting = true;

            Disease_Label_Name.Visibility = Visibility.Collapsed;
            Disease_TextBox_Name.Visibility = Visibility.Visible;

            Disease_Button_Reduct.IsEnabled = false;
            Disease_Button_Ok.IsEnabled = true;
            Disease_Button_Cancel.IsEnabled = true;

            Disease_TextBox_Name.Text = Disease_Label_Name.Content.ToString();
        }

        private void Disease_Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Disease_Label_Name.Visibility = Visibility.Visible;
            Disease_TextBox_Name.Visibility = Visibility.Collapsed;

            Disease_Button_Reduct.IsEnabled = true;
            Disease_Button_Ok.IsEnabled = false;
            Disease_Button_Cancel.IsEnabled = false;
        }

        private async void Disease_Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            if (Disease_TextBox_Name.Text.Length is 0)
            {
                MessageBox.Show($"Название не может быть пустым",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string sql;
            if (!isReducting)
            {
                Disease_Selected_Id = (await ConnectionCarrier.Carrier.GetCurrentSequenceId("Disease")).ToString();
                sql = $"Insert Into \"Disease\" (\"Disease_Name\") Values ('{Disease_TextBox_Name.Text}')";
            }
            else
            {
                sql = $"Update \"Disease\" Set \"Disease_Name\" = '{Disease_TextBox_Name.Text}' Where \"Disease_Id\" = {Disease_Selected_Id}";
            }

            if (!isReducting | (isReducting & Procedure_Label_Name.Content.ToString() != Procedure_TextBox_Name.Text))
            {
                var conn = ConnectionCarrier.Carrier.Connection;

                try
                {
                    await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                    await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();

                    Disease_Label_Name.Content = Disease_TextBox_Name.Text;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("повторяющееся"))
                    {
                        MessageBox.Show($"Невозможно выполнить операцию{Environment.NewLine}Такое название уже есть",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Непредвиденная ошибка{Environment.NewLine}{ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }
                finally
                {
                    await conn.CloseAsync();
                }
            }

            Disease_MenuItem_Refresh_Click(sender, e);
        }

        private async void Disease_Button_SearchData_Click(object sender, RoutedEventArgs e)
        {
            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * From \"Disease\" Where lower(\"Disease_Name\") Like '%{Disease_TextBox_SearchData.Text.ToLower()}%' Order By \"Disease_Id\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "Disease_Id" },
                    { "Название заболевания", "Disease_Name" }
                });

                this.Disease_DataGrid.ItemsSource = new DataView(dt);
                this.Disease_DataGrid.Columns.Where(x => x.Header == "id").First().Visibility = Visibility.Collapsed;
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
        }

        private async void Department_MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Department_Label_Name.Content = "";
            Department_Label_Name.Visibility = Visibility.Visible;
            Department_TextBox_Name.Visibility = Visibility.Collapsed;

            Department_Label_Phone.Content = "";
            Department_Label_Phone.Visibility = Visibility.Visible;
            Department_TextBox_Phone.Visibility = Visibility.Collapsed;

            Department_Label_Beds.Content = "";
            Department_Label_Beds.Visibility = Visibility.Visible;
            Department_TextBox_Beds.Visibility = Visibility.Collapsed;

            Department_Button_Reduct.IsEnabled = true;
            Department_Button_Ok.IsEnabled = false;
            Department_Button_Cancel.IsEnabled = false;

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

        private void Department_MenuItem_Add_Click(object sender, RoutedEventArgs e)
        {
            isReducting = false;

            Department_Label_Name.Visibility = Visibility.Collapsed;
            Department_TextBox_Name.Visibility = Visibility.Visible;

            Department_Label_Phone.Visibility = Visibility.Collapsed;
            Department_TextBox_Phone.Visibility = Visibility.Visible;

            Department_Label_Beds.Visibility = Visibility.Collapsed;
            Department_TextBox_Beds.Visibility = Visibility.Visible;

            Department_Button_Reduct.IsEnabled = false;
            Department_Button_Ok.IsEnabled = true;
            Department_Button_Cancel.IsEnabled = true;

            Department_TextBox_Name.Text = "";
            Department_TextBox_Phone.Text = "";
            Department_TextBox_Beds.Text = "";
        }

        private async void Department_MenuItem_Delete_Click(object sender, RoutedEventArgs e)
        {
            bool isDeleting = true;
            if (Department_Selected_Id == String.Empty)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Вы точно уверены, что хотите удалить это отделение?", 
                "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result is not MessageBoxResult.Yes)
            {
                return;
            }

            String sql = $"UPDATE \"Department\" SET \"Department_Exists\" = false Where \"Department_Id\" = {Department_Selected_Id}";

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                isDeleting = false;
                if (ex.Message.Contains("has staff"))
                {
                    MessageBox.Show($"Невозможно выполнить удаление{Environment.NewLine}В отеделении записаны сотрудники", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                }
                else if (ex.Message.Contains("has patients"))
                {
                    MessageBox.Show($"Невозможно выполнить удаление{Environment.NewLine}В отеделении записаны пациенты",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                await conn.CloseAsync();
            }

            if (isDeleting)
            {
                RaiseFirstSelection(Department_DataGrid);
            }
        }

        private void Department_DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (Department_DataGrid.SelectedIndex is -1)
                return;
            Department_Label_Name.Visibility = Visibility.Visible;
            Department_TextBox_Name.Visibility = Visibility.Collapsed;
            
            Department_Label_Phone.Visibility = Visibility.Visible;
            Department_TextBox_Phone.Visibility = Visibility.Collapsed;

            Department_Label_Beds.Visibility = Visibility.Visible;
            Department_TextBox_Beds.Visibility = Visibility.Collapsed;

            Department_Button_Reduct.IsEnabled = true;
            Department_Button_Ok.IsEnabled = false;
            Department_Button_Cancel.IsEnabled = false;

            var vs = Department_DataGrid.SelectedIndex;
            var row = Department_DataGrid.Items[vs] as DataRowView;
            Department_Selected_Id = row?["id"].ToString();

            Department_Load_Data(Department_Selected_Id);
        }

        private void Department_Button_Reduct_Click(object sender, RoutedEventArgs e)
        {
            if (Department_Selected_Id == String.Empty)
                return;

            isReducting = true;

            Department_Label_Name.Visibility = Visibility.Collapsed;
            Department_TextBox_Name.Visibility = Visibility.Visible;

            Department_Label_Phone.Visibility = Visibility.Collapsed;
            Department_TextBox_Phone.Visibility = Visibility.Visible;

            Department_Label_Beds.Visibility = Visibility.Collapsed;
            Department_TextBox_Beds.Visibility = Visibility.Visible;

            Department_Button_Reduct.IsEnabled = false;
            Department_Button_Ok.IsEnabled = true;
            Department_Button_Cancel.IsEnabled = true;

            Department_TextBox_Name.Text = Department_Label_Name.Content.ToString();
            Department_TextBox_Phone.Text = Department_Label_Phone.Content.ToString();
            Department_TextBox_Beds.Text = Department_Label_Beds.Content.ToString();
        }

        private void Department_Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Department_Label_Name.Visibility = Visibility.Visible;
            Department_TextBox_Name.Visibility = Visibility.Collapsed;

            Department_Label_Phone.Visibility = Visibility.Visible;
            Department_TextBox_Phone.Visibility = Visibility.Collapsed;

            Department_Label_Beds.Visibility = Visibility.Visible;
            Department_TextBox_Beds.Visibility = Visibility.Collapsed;

            Department_Button_Reduct.IsEnabled = true;
            Department_Button_Ok.IsEnabled = false;
            Department_Button_Cancel.IsEnabled = false;
        }

        private async void Department_Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            if (Department_TextBox_Name.Text.Length is 0)
            {
                MessageBox.Show($"Название не может быть пустым",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                Int32.Parse(Department_TextBox_Beds.Text);
            }
            catch
            {
                MessageBox.Show($"Невозможно считать число кроватей. Повторите ввод",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string sql;
            if (!isReducting)
            {
                Department_Selected_Id = (await ConnectionCarrier.Carrier.GetCurrentSequenceId("Department")).ToString();
                sql = $"Insert Into \"Department\" (\"Department_Name\", \"Department_Phone\", \"Department_Beds\") " +
                    $"Values ('{Department_TextBox_Name.Text}', '{Department_TextBox_Phone.Text}', " +
                    $"'{Department_TextBox_Beds.Text}')";
            }
            else
            {
                sql = $"Update \"Department\" Set " +
                    (Department_TextBox_Name.Text == Department_Label_Name.Content.ToString() ? "" : $"\"Department_Name\" = '{Department_TextBox_Name.Text}', ") +
                    $"\"Department_Phone\" = '{Department_TextBox_Phone.Text}', " +
                    $"\"Department_Beds\" = {Department_TextBox_Beds.Text} " +
                    $"Where \"Department_Id\" = {Department_Selected_Id}";
            }


            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();
            }
            catch (Exception ex) 
            {
                if (ex.Message.Contains("negative"))
                {
                    MessageBox.Show($"Невозможно установить отрицательное количество коек",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                }
                else if (ex.Message.Contains("less"))
                {
                    MessageBox.Show($"Невозможно установить количество коек меньше,{Environment.NewLine}" +
                        $"чем используется в данный момент",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (ex.Message.Contains("повторяющееся"))
                {
                    MessageBox.Show($"Невозможно выполнить операцию{Environment.NewLine}Уже есть отдиление с таким же названием",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Непредвиденная ошибка{Environment.NewLine}{ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }
            finally
            {
                await conn.CloseAsync();
            }

            Department_MenuItem_Refresh_Click(sender, e);
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

        #endregion

        #region Staff

        string? Staff_Selected_Id = string.Empty;
        Dictionary<String, string> Categories = new();
        Dictionary<String, string> Departments = new();
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
                String sql = $"Select * From \"Staff_Full\" Where \"Staff_Id\" = {id}";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                data = this.NpgsqlDataReader_To_Dictionary(reader, new Dictionary<string, string>()
                {
                    { "id", "Staff_Id" },
                    { "Имя", "Staff_Name" },
                    { "Фамилия", "Staff_Surname" },
                    { "Отчество", "Staff_Patronymic" },
                    { "Название отделения", "Department_Name" },
                    { "Название категории", "Category_Name" },
                    { "Дата принятия на работу", "Staff_EmploymentDate" },
                    { "Оклад", "Staff_Salary" },
                    { "Логн", "Staff_Login" },
                    { "Должность", "role" }
                });

                Staff_Label_Name.Content = data["Имя"];
                Staff_Label_Surname.Content = data["Фамилия"];
                Staff_Label_Patronymic.Content = data["Отчество"];
                Staff_Label_Department.Content = data["Название отделения"];
                Staff_Label_Category.Content = data["Название категории"];
                Staff_Label_Date.Content = data["Дата принятия на работу"].Split(' ')[0];
                Staff_Label_Salary.Content = data["Оклад"];
                Staff_Label_Login.Content = data["Логн"];
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
        }

        private async void Staff_MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Staff_Label_Surname.Content = "";
            Staff_Label_Surname.Visibility = Visibility.Visible;
            Staff_TextBox_Surname.Visibility = Visibility.Collapsed;

            Staff_Label_Name.Content = "";
            Staff_Label_Name.Visibility = Visibility.Visible;
            Staff_TextBox_Name.Visibility = Visibility.Collapsed;

            Staff_Label_Patronymic.Content = "";
            Staff_Label_Patronymic.Visibility = Visibility.Visible;
            Staff_TextBox_Patronymic.Visibility = Visibility.Collapsed;

            Staff_Label_Department.Content = "";
            Staff_Label_Department.Visibility = Visibility.Visible;
            Staff_ComboBox_Department.Visibility = Visibility.Collapsed;

            Staff_Label_Login.Content = "";
            Staff_Label_Login.Visibility = Visibility.Visible;
            Staff_TextBox_Login.Visibility = Visibility.Collapsed;

            Staff_Label_Category.Content = "";
            Staff_Label_Category.Visibility = Visibility.Visible;
            Staff_ComboBox_Category.Visibility = Visibility.Collapsed;

            Staff_Label_Date.Content = "";
            Staff_Label_Date.Visibility = Visibility.Visible;
            Staff_DatePicker_Date.Visibility = Visibility.Collapsed;

            Staff_Label_Salary.Content = "";
            Staff_Label_Salary.Visibility = Visibility.Visible;
            Staff_TextBox_Salary.Visibility = Visibility.Collapsed;

            Staff_Label_JobTitle.Content = "";
            Staff_Label_JobTitle.Visibility = Visibility.Visible;
            Staff_ComboBox_JobTitle.Visibility = Visibility.Collapsed;

            Staff_Label_Password.Visibility = Visibility.Collapsed;
            Staff_TextBox_Password.Visibility = Visibility.Collapsed;

            Staff_Button_Reduct.IsEnabled = true;
            Staff_Button_Ok.IsEnabled = false;
            Staff_Button_Cancel.IsEnabled = false;

            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = "Select * FROM \"Staff_SurnameNP\" Order By \"Staff_Id\"";
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

        private async void Staff_MenuItem_Add_Click(object sender, RoutedEventArgs e)
        {
            isReducting = false;

            Staff_Label_Surname.Visibility = Visibility.Collapsed;
            Staff_TextBox_Surname.Visibility = Visibility.Visible;

            Staff_Label_Name.Visibility = Visibility.Collapsed;
            Staff_TextBox_Name.Visibility = Visibility.Visible;

            Staff_Label_Patronymic.Visibility = Visibility.Collapsed;
            Staff_TextBox_Patronymic.Visibility = Visibility.Visible;

            Staff_Label_Department.Visibility = Visibility.Collapsed;
            Staff_ComboBox_Department.Visibility = Visibility.Visible;

            Staff_Label_Login.Visibility = Visibility.Collapsed;
            Staff_TextBox_Login.Visibility = Visibility.Visible;

            Staff_Label_Category.Visibility = Visibility.Collapsed;
            Staff_ComboBox_Category.Visibility = Visibility.Visible;

            Staff_Label_Salary.Visibility = Visibility.Collapsed;
            Staff_TextBox_Salary.Visibility = Visibility.Visible;

            Staff_Label_JobTitle.Visibility = Visibility.Collapsed;
            Staff_ComboBox_JobTitle.Visibility = Visibility.Visible;

            Staff_Label_Password.Visibility = Visibility.Visible;
            Staff_TextBox_Password.Visibility = Visibility.Visible;

            Staff_Button_Reduct.IsEnabled = false;
            Staff_Button_Ok.IsEnabled = true;
            Staff_Button_Cancel.IsEnabled = true;

            Staff_DatePicker_Date.Text = DateTime.Now.ToString();
            Staff_Label_Date.Content = Staff_DatePicker_Date.Text;

            Staff_TextBox_Surname.Text = "";
            Staff_TextBox_Name.Text = "";
            Staff_TextBox_Patronymic.Text = "";
            Staff_TextBox_Login.Text = "";
            Staff_TextBox_Salary.Text = "";
            Staff_TextBox_Password.Text = "";

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = "Select * From \"Category\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                Categories.Clear();
                Categories = this.NpgsqlDataReader_To_DictionaryList(reader, "Category_Id", "Category_Name");
                Categories.Add("NULL", "");
                reader.Close();

                sql = "Select * From \"Department\"";
                reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();
                Departments.Clear();
                Departments = this.NpgsqlDataReader_To_DictionaryList(reader, "Department_Id", "Department_Name");
                Departments.Add("NULL", "");
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }
            Staff_ComboBox_Department.ItemsSource = Departments.Values.OrderBy(x => x).ToArray();
            Staff_ComboBox_Department.SelectedIndex = Staff_ComboBox_Department.Items.IndexOf("");

            Staff_ComboBox_Category.ItemsSource = Categories.Values.OrderBy(x => x).ToArray();
            Staff_ComboBox_Category.SelectedIndex = Staff_ComboBox_Category.Items.IndexOf("");

            Staff_ComboBox_JobTitle.ItemsSource = JobTitles.Values.OrderBy(x => x).ToArray();
            Staff_ComboBox_JobTitle.SelectedIndex = Staff_ComboBox_JobTitle.Items.IndexOf("");
        }

        private async void Staff_MenuItem_Delete_Click(object sender, RoutedEventArgs e)
        {
            bool isDeleting = true;
            if (Staff_Selected_Id == String.Empty)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Вы точно уверены, что хотите удалить этого сотрудника?",
                "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result is not MessageBoxResult.Yes)
            {
                return;
            }

            String sql = $"CALL delete_staff({Staff_Selected_Id})";

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                isDeleting = false;
                if (ex.Message.Contains("doctor for"))
                {
                    MessageBox.Show($"Невозможно выполнить удаление{Environment.NewLine}У данного доктора есть пациенты",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                }
                else if (ex.Message.Contains("себя"))
                {
                    MessageBox.Show($"Невозможно выполнить удаление{Environment.NewLine}Пользователь не может удалить сам себя",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                await conn.CloseAsync();
            }

            if (isDeleting)
            {
                RaiseFirstSelection(Department_DataGrid);
            }
        }

        private void Staff_DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (Staff_DataGrid.SelectedIndex is -1)
                return;

            Staff_Label_Surname.Visibility = Visibility.Visible;
            Staff_TextBox_Surname.Visibility = Visibility.Collapsed;

            Staff_Label_Name.Visibility = Visibility.Visible;
            Staff_TextBox_Name.Visibility = Visibility.Collapsed;

            Staff_Label_Patronymic.Visibility = Visibility.Visible;
            Staff_TextBox_Patronymic.Visibility = Visibility.Collapsed;

            Staff_Label_Department.Visibility = Visibility.Visible;
            Staff_ComboBox_Department.Visibility = Visibility.Collapsed;

            Staff_Label_Login.Visibility = Visibility.Visible;
            Staff_TextBox_Login.Visibility = Visibility.Collapsed;

            Staff_Label_Category.Visibility = Visibility.Visible;
            Staff_ComboBox_Category.Visibility = Visibility.Collapsed;

            Staff_Label_Date.Visibility = Visibility.Visible;
            Staff_DatePicker_Date.Visibility = Visibility.Collapsed;

            Staff_Label_Salary.Visibility = Visibility.Visible;
            Staff_TextBox_Salary.Visibility = Visibility.Collapsed;

            Staff_Label_JobTitle.Visibility = Visibility.Visible;
            Staff_ComboBox_JobTitle.Visibility = Visibility.Collapsed;

            Staff_Label_Password.Visibility = Visibility.Collapsed;
            Staff_TextBox_Password.Visibility = Visibility.Collapsed;

            Staff_Button_Reduct.IsEnabled = true;
            Staff_Button_Ok.IsEnabled = false;
            Staff_Button_Cancel.IsEnabled = false;

            var vs = Staff_DataGrid.SelectedIndex;
            var row = Staff_DataGrid.Items[vs] as DataRowView;
            Staff_Selected_Id = row?["id"].ToString();

            Staff_Load_Data(Staff_Selected_Id);
        }

        private async void Staff_Button_Reduct_Click(object sender, RoutedEventArgs e)
        {
            if (Staff_Selected_Id == String.Empty)
                return;

            isReducting = true;

            Staff_Label_Surname.Visibility = Visibility.Collapsed;
            Staff_TextBox_Surname.Visibility = Visibility.Visible;

            Staff_Label_Name.Visibility = Visibility.Collapsed;
            Staff_TextBox_Name.Visibility = Visibility.Visible;

            Staff_Label_Patronymic.Visibility = Visibility.Collapsed;
            Staff_TextBox_Patronymic.Visibility = Visibility.Visible;

            Staff_Label_Department.Visibility = Visibility.Collapsed;
            Staff_ComboBox_Department.Visibility = Visibility.Visible;

            Staff_Label_Login.Visibility = Visibility.Collapsed;
            Staff_TextBox_Login.Visibility = Visibility.Visible;

            Staff_Label_Category.Visibility = Visibility.Collapsed;
            Staff_ComboBox_Category.Visibility = Visibility.Visible;

            Staff_Label_Salary.Visibility = Visibility.Collapsed;
            Staff_TextBox_Salary.Visibility = Visibility.Visible;

            Staff_Label_JobTitle.Visibility = Visibility.Collapsed;
            Staff_ComboBox_JobTitle.Visibility = Visibility.Visible;

            Staff_Label_Password.Visibility = Visibility.Visible;
            Staff_TextBox_Password.Visibility = Visibility.Visible;

            Staff_Button_Reduct.IsEnabled = false;
            Staff_Button_Ok.IsEnabled = true;
            Staff_Button_Cancel.IsEnabled = true;

            Staff_TextBox_Surname.Text = Staff_Label_Surname.Content.ToString();
            Staff_TextBox_Name.Text = Staff_Label_Name.Content.ToString();
            Staff_TextBox_Patronymic.Text = Staff_Label_Patronymic.Content.ToString();
            Staff_TextBox_Login.Text = Staff_Label_Login.Content.ToString();
            Staff_TextBox_Salary.Text = Staff_Label_Salary.Content.ToString();
            Staff_TextBox_Password.Text = "";

            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = "Select * From \"Category\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                Categories.Clear();
                Categories = this.NpgsqlDataReader_To_DictionaryList(reader, "Category_Id", "Category_Name");
                Categories.Add("NULL", "");
                reader.Close();

                sql = "Select * From \"Department\"";
                reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();
                Departments.Clear();
                Departments = this.NpgsqlDataReader_To_DictionaryList(reader, "Department_Id", "Department_Name");
                Departments.Add("NULL", "");
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }
            Staff_ComboBox_Department.ItemsSource = Departments.Values.OrderBy(x => x).ToArray();
            Staff_ComboBox_Department.SelectedIndex = Staff_ComboBox_Department.Items.IndexOf(Staff_Label_Department.Content);

            Staff_ComboBox_Category.ItemsSource = Categories.Values.OrderBy(x => x).ToArray();
            Staff_ComboBox_Category.SelectedIndex = Staff_ComboBox_Category.Items.IndexOf(Staff_Label_Category.Content);

            Staff_ComboBox_JobTitle.ItemsSource = JobTitles.Values.OrderBy(x => x).ToArray();
            Staff_ComboBox_JobTitle.SelectedIndex = Staff_ComboBox_JobTitle.Items.IndexOf(Staff_Label_JobTitle.Content);
        }

        private void Staff_Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Staff_Label_Surname.Visibility = Visibility.Visible;
            Staff_TextBox_Surname.Visibility = Visibility.Collapsed;

            Staff_Label_Name.Visibility = Visibility.Visible;
            Staff_TextBox_Name.Visibility = Visibility.Collapsed;

            Staff_Label_Patronymic.Visibility = Visibility.Visible;
            Staff_TextBox_Patronymic.Visibility = Visibility.Collapsed;

            Staff_Label_Department.Visibility = Visibility.Visible;
            Staff_ComboBox_Department.Visibility = Visibility.Collapsed;

            Staff_Label_Login.Visibility = Visibility.Visible;
            Staff_TextBox_Login.Visibility = Visibility.Collapsed;

            Staff_Label_Category.Visibility = Visibility.Visible;
            Staff_ComboBox_Category.Visibility = Visibility.Collapsed;

            Staff_Label_Date.Visibility = Visibility.Visible;
            Staff_DatePicker_Date.Visibility = Visibility.Collapsed;

            Staff_Label_Salary.Visibility = Visibility.Visible;
            Staff_TextBox_Salary.Visibility = Visibility.Collapsed;

            Staff_Label_JobTitle.Visibility = Visibility.Visible;
            Staff_ComboBox_JobTitle.Visibility = Visibility.Collapsed;

            Staff_Label_Password.Visibility = Visibility.Collapsed;
            Staff_TextBox_Password.Visibility = Visibility.Collapsed;

            Staff_Button_Reduct.IsEnabled = true;
            Staff_Button_Ok.IsEnabled = false;
            Staff_Button_Cancel.IsEnabled = false;
        }

        private async void Staff_Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            // Проверки

            if (Staff_TextBox_Surname.Text.Length is 0)
            {
                MessageBox.Show($"Фамилия пользователя не может быть пустой",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Staff_TextBox_Name.Text.Length is 0)
            {
                MessageBox.Show($"Имя пользователя не может быть пустым",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Staff_TextBox_Patronymic.Text.Length is 0)
            {
                MessageBox.Show($"Отчество пользователя не может быть пустым",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                Int32.Parse(Staff_TextBox_Salary.Text);
            }
            catch
            {
                MessageBox.Show($"Невозможно считать зарплату пользователя",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Staff_TextBox_Login.Text.Length is 0)
            {
                MessageBox.Show($"Логин пользователя не может быть пустым",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!isReducting & Staff_TextBox_Password.Text.Length is 0)
            {
                MessageBox.Show($"Необходимо указать пароль при создании пользователя",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Уточнения

            if (!isReducting & (Staff_ComboBox_JobTitle.Text == Staff_Label_JobTitle.Content.ToString()))
            {
                var vs = MessageBox.Show($"Вы точно уверены, что хотите изменить должность пользователя?",
                        "Ошибка", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (vs is MessageBoxResult.No)
                {
                    return;
                }
            }

            var login_changed = Staff_TextBox_Login.Text != Staff_Label_Login.Content.ToString();
            var password_changed = Staff_TextBox_Password.Text.Length is not 0;
            if (isReducting & (password_changed | login_changed))
            {
                var str = (login_changed ? "логин" : "") + (password_changed ? (login_changed ? " и пароль" : "пароль") : "");
                var vs = MessageBox.Show($"Внимание, вы собираетесь изменить {str} пользователя{Environment.NewLine}Вы точно хотите продолжить?",
                        "Ошибка", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (vs is MessageBoxResult.No)
                {
                    return;
                }
            }
            
            String saved_login = Staff_Label_Login.Content.ToString();

            string sql = "";

            if (!isReducting)
            {
                Staff_Selected_Id = (await ConnectionCarrier.Carrier.GetCurrentSequenceId("Staff")).ToString();

                var dep_id = Departments.Where(x => x.Value == Staff_ComboBox_Department.Text).First().Key;
                var cat_id = Categories.Where(x => x.Value == Staff_ComboBox_Category.Text).First().Key;
                var date = DateTime.Now;
                sql = $"Insert Into \"Staff\" (\"Staff_Name\", \"Staff_Surname\", \"Staff_Patronymic\", " +
                    $"\"Staff_Department_Id\", \"Staff_Category_Id\", \"Staff_EmploymentDate\", " +
                    $"\"Staff_Salary\", \"Staff_Login\") Values " +
                    $"('{Staff_TextBox_Name.Text}', '{Staff_TextBox_Surname.Text}', '{Staff_TextBox_Patronymic.Text}', " +
                    $"'{dep_id}', '{cat_id}', '{date:dd-MM-yyyy}', " +
                    $"'{Staff_TextBox_Salary.Text}', '{Staff_TextBox_Login.Text}');\n" +
                    $"Select change_password('{Staff_TextBox_Login.Text}', '{Staff_TextBox_Password.Text}');\n" +
                    $"Call add_user_to_{JobTitles.Where(x => x.Value == Staff_ComboBox_JobTitle.Text).First().Key}s_group('{Staff_TextBox_Login.Text}')";
            }
            else
            {
                var dep_id = Departments.Where(x => x.Value == Staff_ComboBox_Department.Text).First().Key;
                var cat_id = Categories.Where(x => x.Value == Staff_ComboBox_Category.Text).First().Key;
                sql = $"Update \"Staff\" Set " +
                                    $"\"Staff_Name\" = '{Staff_TextBox_Name.Text}', " +
                                    $"\"Staff_Surname\" = '{Staff_TextBox_Surname.Text}', " +
                                    $"\"Staff_Patronymic\" = '{Staff_TextBox_Patronymic.Text}', " +
                                    $"\"Staff_Department_Id\" = '{dep_id}', " +
                                    $"\"Staff_Category_Id\" = '{cat_id}', " +
                                    $"\"Staff_Salary\" = '{Staff_TextBox_Salary.Text}'" +
                                    (Staff_TextBox_Login.Text == saved_login ? "" : $", \"Staff_Login\" = '{Staff_TextBox_Login.Text}'") +
                                    $" Where \"Staff_Id\" = {Staff_Selected_Id};\n" +
                                    (Staff_TextBox_Password.Text.Length is 0 ? "" : $"Select change_password('{Staff_TextBox_Login.Text}', '{Staff_TextBox_Password.Text}');\n") +
                                    $"Call add_user_to_{JobTitles.Where(x => x.Value == Staff_ComboBox_JobTitle.Text).First().Key}s_group('{Staff_TextBox_Login.Text}')";
                sql = sql.Replace("\'NULL\'", "NULL");
            }

            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("change department"))
                {
                    MessageBox.Show($"Невозможно изменить отделение{Environment.NewLine}У данного доктора есть пациенты в старом отделении",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Непредвиденная ошибка{Environment.NewLine}{ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }
            finally
            {
                await conn.CloseAsync();
            }
            
            Staff_MenuItem_Refresh_Click(sender, e);
        }

        private async void Staff_Button_SearchData_Click(object sender, RoutedEventArgs e)
        {
            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = $"Select * FROM \"Staff_SurnameNP\"";

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
                sql += " Order By \"Staff_Id\"";

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

        #endregion

        #region Patient

        string? Patient_Selected_Id = string.Empty;
        Dictionary<String, string> SocialStatus = new();

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

                Patient_Label_Surname.Content = data["Фамилия"];
                Patient_Label_Name.Content = data["Имя"];
                Patient_Label_Patronymic.Content = data["Отчество"];
                Patient_Label_Birthday.Content = data["Дата рождения"].Split(' ')[0];
                Patient_Label_SocialStatus.Content = data["Социальное положение"];
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

        private async void Patient_MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Patient_Label_Surname.Content = "";
            Patient_Label_Surname.Visibility = Visibility.Visible;
            Patient_TextBox_Surname.Visibility = Visibility.Collapsed;

            Patient_Label_Name.Content = "";
            Patient_Label_Name.Visibility = Visibility.Visible;
            Patient_TextBox_Name.Visibility = Visibility.Collapsed;

            Patient_Label_Patronymic.Content = "";
            Patient_Label_Patronymic.Visibility = Visibility.Visible;
            Patient_TextBox_Patronymic.Visibility = Visibility.Collapsed;

            Patient_Label_SocialStatus.Content = "";
            Patient_Label_SocialStatus.Visibility = Visibility.Visible;
            Patient_ComboBox_SocialStatus.Visibility = Visibility.Collapsed;

            Patient_Label_Birthday.Content = "";
            Patient_Label_Birthday.Visibility = Visibility.Visible;
            Patient_Label_Birthday_l.Visibility = Visibility.Visible;

            Patient_Button_Reduct.IsEnabled = true;
            Patient_Button_Ok.IsEnabled = false;
            Patient_Button_Cancel.IsEnabled = false;

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
            Patient_TextBox_Surname.Visibility = Visibility.Collapsed;

            Patient_Label_Name.Content = "";
            Patient_Label_Name.Visibility = Visibility.Visible;
            Patient_TextBox_Name.Visibility = Visibility.Collapsed;

            Patient_Label_Patronymic.Content = "";
            Patient_Label_Patronymic.Visibility = Visibility.Visible;
            Patient_TextBox_Patronymic.Visibility = Visibility.Collapsed;

            Patient_Label_SocialStatus.Content = "";
            Patient_Label_SocialStatus.Visibility = Visibility.Visible;
            Patient_ComboBox_SocialStatus.Visibility = Visibility.Collapsed;

            Patient_Label_Birthday.Content = "";
            Patient_Label_Birthday.Visibility = Visibility.Visible;
            Patient_Label_Birthday_l.Visibility = Visibility.Visible;

            Patient_Button_Reduct.IsEnabled = true;
            Patient_Button_Ok.IsEnabled = false;
            Patient_Button_Cancel.IsEnabled = false;

            var vs = Patient_DataGrid.SelectedIndex;
            var row = Patient_DataGrid.Items[vs] as DataRowView;
            Patient_Selected_Id = row?["id"].ToString();

            Patient_Load_Data(Patient_Selected_Id);
        }

        private async void Patient_Button_Reduct_Click(object sender, RoutedEventArgs e)
        {
            if (Patient_Selected_Id == String.Empty)
                return;

            Patient_Label_Surname.Visibility = Visibility.Collapsed;
            Patient_TextBox_Surname.Visibility = Visibility.Visible;

            Patient_Label_Name.Visibility = Visibility.Collapsed;
            Patient_TextBox_Name.Visibility = Visibility.Visible;

            Patient_Label_Patronymic.Visibility = Visibility.Collapsed;
            Patient_TextBox_Patronymic.Visibility = Visibility.Visible;

            Patient_Label_SocialStatus.Visibility = Visibility.Collapsed;
            Patient_ComboBox_SocialStatus.Visibility = Visibility.Visible;

            Patient_Label_Birthday.Visibility = Visibility.Collapsed;
            Patient_Label_Birthday_l.Visibility = Visibility.Collapsed;

            Patient_Button_Reduct.IsEnabled = false;
            Patient_Button_Ok.IsEnabled = true;
            Patient_Button_Cancel.IsEnabled = true;

            Patient_TextBox_Surname.Text = Patient_Label_Surname.Content.ToString();
            Patient_TextBox_Name.Text = Patient_Label_Name.Content.ToString();
            Patient_TextBox_Patronymic.Text = Patient_Label_Patronymic.Content.ToString();


            var conn = ConnectionCarrier.Carrier.Connection;
            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                String sql = "Select * From \"SocialStatus\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                SocialStatus.Clear();
                SocialStatus = this.NpgsqlDataReader_To_DictionaryList(reader, "SocialStatus_Id", "SocialStatus_Name");
                SocialStatus.Add("NULL", "");
                reader.Close();
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }
            Patient_ComboBox_SocialStatus.ItemsSource = SocialStatus.Values.OrderBy(x => x).ToArray();
            Patient_ComboBox_SocialStatus.SelectedIndex = Patient_ComboBox_SocialStatus.Items.IndexOf(Patient_Label_SocialStatus.Content);
        }

        private void Patient_Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Patient_Label_Surname.Visibility = Visibility.Visible;
            Patient_TextBox_Surname.Visibility = Visibility.Collapsed;

            Patient_Label_Name.Visibility = Visibility.Visible;
            Patient_TextBox_Name.Visibility = Visibility.Collapsed;

            Patient_Label_Patronymic.Visibility = Visibility.Visible;
            Patient_TextBox_Patronymic.Visibility = Visibility.Collapsed;

            Patient_Label_SocialStatus.Visibility = Visibility.Visible;
            Patient_ComboBox_SocialStatus.Visibility = Visibility.Collapsed;

            Patient_Label_Birthday.Visibility = Visibility.Visible;
            Patient_Label_Birthday_l.Visibility = Visibility.Visible;

            Patient_Button_Reduct.IsEnabled = true;
            Patient_Button_Ok.IsEnabled = false;
            Patient_Button_Cancel.IsEnabled = false;
        }

        private async void Patient_Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            if (Patient_TextBox_Surname.Text.Length is 0)
            {
                MessageBox.Show($"Фамилия пациента не может быть пустой",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Patient_TextBox_Name.Text.Length is 0)
            {
                MessageBox.Show($"Имя пациента не может быть пустым",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Patient_TextBox_Patronymic.Text.Length is 0)
            {
                MessageBox.Show($"Отчество пациента не может быть пустым",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var soc_id = SocialStatus.Where(x => x.Value == Patient_ComboBox_SocialStatus.Text).First().Key;
            var sql = $"Update \"Patient\" Set " +
                                $"\"Patient_Name\" = '{Patient_TextBox_Name.Text}', " +
                                $"\"Patient_Surname\" = '{Patient_TextBox_Surname.Text}', " +
                                $"\"Patient_Patronymic\" = '{Patient_TextBox_Patronymic.Text}', " +
                                $"\"Patient_SocialStatus_Id\" = '{soc_id}' " +
                                $"Where \"Patient_Id\" = {Patient_Selected_Id}";
            sql = sql.Replace("\'NULL\'", "NULL");

            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await ConnectionCarrier.Carrier.OpenConnectionAsyncSave();
                await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();

                Patient_Label_Name.Content = Patient_TextBox_Name.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Непредвиденная ошибка{Environment.NewLine}{ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                await conn.CloseAsync();
            }

            Patient_MenuItem_Refresh_Click(sender, e);
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
    }
}
