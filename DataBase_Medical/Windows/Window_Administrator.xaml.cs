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
                    break;
                }
                case "Персонал":
                {
                    this.Grid_Staff.Visibility = Visibility.Visible;
                    break;
                }
                case "Отделения":
                {
                    this.Grid_Department.Visibility = Visibility.Visible;
                    break;
                }
                case "Заболевания":
                {
                    this.Grid_Disease.Visibility = Visibility.Visible;
                    Disease_MenuItem_Refresh_Click(sender, e);
                    await ConnectionCarrier.Carrier.WaitTillNotOpen();
                    RaiseFirstSelection(Disease_DataGrid);
                    break;
                }
                case "Процедуры":
                {
                    this.Grid_Procedure.Visibility = Visibility.Visible;
                    Procedure_MenuItem_Refresh_Click(sender, e);
                    await ConnectionCarrier.Carrier.WaitTillNotOpen();
                    RaiseFirstSelection(Procedure_DataGrid);
                    break;
                }
                case "Социальное положение":
                {
                    this.Grid_SocialStatus.Visibility = Visibility.Visible;
                    SocialStatus_MenuItem_Refresh_Click(sender, e);
                    await ConnectionCarrier.Carrier.WaitTillNotOpen();
                    RaiseFirstSelection(SocialStatus_DataGrid);
                    break;
                }
                case "Категории":
                {
                    this.Grid_Category.Visibility = Visibility.Visible;
                    Category_MenuItem_Refresh_Click(sender, e);
                    await ConnectionCarrier.Carrier.WaitTillNotOpen();
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
        
        private void RaiseFirstSelection(DataGrid dataGrid)
        {
            dataGrid.SelectedIndex = 0;
        }

        #region Category

        string? Category_Selected_Id = string.Empty;
        string? Category_Saved_Id = string.Empty;

        private async void Category_MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (Category_Saved_Id != string.Empty)
            {
                Category_Selected_Id = Category_Saved_Id;
                Category_Saved_Id = string.Empty;
            }
            else
            {
                Category_Selected_Id = string.Empty;
                Category_Saved_Id = string.Empty;
            }

            Category_Label_Name.Content = "";
            Category_Label_Name.Visibility = Visibility.Visible;
            Category_TextBox.Visibility = Visibility.Collapsed;

            Category_Button_Reduct.IsEnabled = true;
            Category_Button_Ok.IsEnabled = false;
            Category_Button_Cancel.IsEnabled = false;

            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await conn.OpenAsync();
                String sql = "Select * From \"Category\"";
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

            RaiseFirstSelection(Category_DataGrid);
        }

        private void Category_MenuItem_Search_Click(object sender, RoutedEventArgs e)
        {
            this.Category_Grid_Search.Visibility =
                this.Category_Grid_Search.Visibility is Visibility.Visible ?
                Visibility.Collapsed : Visibility.Visible;
        }

        private void Category_MenuItem_Add_Click(object sender, RoutedEventArgs e)
        {
            Category_Label_Name.Visibility = Visibility.Collapsed;
            Category_TextBox.Visibility = Visibility.Visible;

            Category_Button_Reduct.IsEnabled = false;
            Category_Button_Ok.IsEnabled = true;
            Category_Button_Cancel.IsEnabled = true;

            Category_TextBox.Text = "";

            Category_Saved_Id = Category_Selected_Id;
            Category_Selected_Id = string.Empty;
        }

        private void Category_DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            Category_Label_Name.Visibility = Visibility.Visible;
            Category_TextBox.Visibility = Visibility.Collapsed;

            Category_Button_Reduct.IsEnabled = true;
            Category_Button_Ok.IsEnabled = false;
            Category_Button_Cancel.IsEnabled = false;

            var vs = Category_DataGrid.SelectedIndex;
            var row = Category_DataGrid.Items[vs] as DataRowView;
            Category_Selected_Id = row?["id"].ToString();

            Category_TextBox.Text = row?["Название категории"].ToString();
            Category_Label_Name.Content = row?["Название категории"].ToString();

            Category_Saved_Id = string.Empty;
        }

        private void Category_Button_Reduct_Click(object sender, RoutedEventArgs e)
        {
            if (Category_Selected_Id == String.Empty)
                return;
            Category_Label_Name.Visibility = Visibility.Collapsed;
            Category_TextBox.Visibility = Visibility.Visible;

            Category_Button_Reduct.IsEnabled = false;
            Category_Button_Ok.IsEnabled = true;
            Category_Button_Cancel.IsEnabled = true;

            Category_TextBox.Text = Category_Label_Name.Content.ToString();

            Category_Saved_Id = string.Empty;
        }

        private void Category_Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Category_Label_Name.Visibility = Visibility.Visible;
            Category_TextBox.Visibility = Visibility.Collapsed;

            Category_Button_Reduct.IsEnabled = true;
            Category_Button_Ok.IsEnabled = false;
            Category_Button_Cancel.IsEnabled = false;

            if (Category_Saved_Id != string.Empty)
            {
                Category_Selected_Id = Category_Saved_Id;
                Category_Saved_Id = "";
            }
        }

        private async void Category_Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            Category_Label_Name.Visibility = Visibility.Visible;
            Category_TextBox.Visibility = Visibility.Collapsed;

            Category_Button_Reduct.IsEnabled = true;
            Category_Button_Ok.IsEnabled = false;
            Category_Button_Cancel.IsEnabled = false;

            string sql;
            if (Category_Selected_Id == "")
            {
                Category_Selected_Id = (await ConnectionCarrier.Carrier.GetCurrentSequenceId("Category")).ToString();
                sql = $"Insert Into \"Category\" (\"Category_Name\") Values ('{Category_TextBox.Text}')";
            }
            else
            {
                sql = $"Update \"Category\" Set \"Category_Name\" = '{Category_TextBox.Text}' Where \"Category_Id\" = {Category_Selected_Id}";
            }
            

            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await conn.OpenAsync();
                await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }

            Category_MenuItem_Refresh_Click(sender, e);
            
            Category_Label_Name.Content = Category_TextBox.Text;
        }

        private async void Category_Button_SearchData_Click(object sender, RoutedEventArgs e)
        {
            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await conn.OpenAsync();
                String sql = $"Select * From \"Category\" Where lower(\"Category_Name\") Like '%{Category_TextBox_SearchData.Text.ToLower()}%'";
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
        string? SocialStatus_Saved_Id = string.Empty;

        private async void SocialStatus_MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (SocialStatus_Saved_Id != string.Empty)
            {
                SocialStatus_Selected_Id = SocialStatus_Saved_Id;
                SocialStatus_Saved_Id = string.Empty;
            }
            else
            {
                SocialStatus_Selected_Id = string.Empty;
                SocialStatus_Saved_Id = string.Empty;
            }

            SocialStatus_Label_Name.Content = "";
            SocialStatus_Label_Name.Visibility = Visibility.Visible;
            SocialStatus_TextBox_Name.Visibility = Visibility.Collapsed;

            SocialStatus_Button_Reduct.IsEnabled = true;
            SocialStatus_Button_Ok.IsEnabled = false;
            SocialStatus_Button_Cancel.IsEnabled = false;

            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await conn.OpenAsync();
                String sql = "Select * From \"SocialStatus\"";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "SocialStatus_Id" },
                    { "Название cоциального положения", "SocialStatus_Name" }
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

            RaiseFirstSelection(Category_DataGrid);
        }

        private void SocialStatus_MenuItem_Search_Click(object sender, RoutedEventArgs e)
        {
            this.SocialStatus_Grid_Search.Visibility =
               this.SocialStatus_Grid_Search.Visibility is Visibility.Visible ?
               Visibility.Collapsed : Visibility.Visible;
        }

        private void SocialStatus_MenuItem_Add_Click(object sender, RoutedEventArgs e)
        {
            SocialStatus_Label_Name.Visibility = Visibility.Collapsed;
            SocialStatus_TextBox_Name.Visibility = Visibility.Visible;

            SocialStatus_Button_Reduct.IsEnabled = false;
            SocialStatus_Button_Ok.IsEnabled = true;
            SocialStatus_Button_Cancel.IsEnabled = true;

            SocialStatus_TextBox_Name.Text = "";

            SocialStatus_Saved_Id = SocialStatus_Selected_Id;
            SocialStatus_Selected_Id = string.Empty;
        }

        private void SocialStatus_DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            SocialStatus_Label_Name.Visibility = Visibility.Visible;
            SocialStatus_TextBox_Name.Visibility = Visibility.Collapsed;

            SocialStatus_Button_Reduct.IsEnabled = true;
            SocialStatus_Button_Ok.IsEnabled = false;
            SocialStatus_Button_Cancel.IsEnabled = false;

            var vs = SocialStatus_DataGrid.SelectedIndex;
            var row = SocialStatus_DataGrid.Items[vs] as DataRowView;
            SocialStatus_Selected_Id = row?["id"].ToString();

            SocialStatus_TextBox_Name.Text = row?["Название cоциального положения"].ToString();
            SocialStatus_Label_Name.Content = row?["Название cоциального положения"].ToString();

            SocialStatus_Saved_Id = string.Empty;
        }

        private void SocialStatus_Button_Reduct_Click(object sender, RoutedEventArgs e)
        {
            if (SocialStatus_Selected_Id == String.Empty)
                return;
            SocialStatus_Label_Name.Visibility = Visibility.Collapsed;
            SocialStatus_TextBox_Name.Visibility = Visibility.Visible;

            SocialStatus_Button_Reduct.IsEnabled = false;
            SocialStatus_Button_Ok.IsEnabled = true;
            SocialStatus_Button_Cancel.IsEnabled = true;

            SocialStatus_TextBox_Name.Text = SocialStatus_Label_Name.Content.ToString();

            SocialStatus_Saved_Id = string.Empty;
        }

        private void SocialStatus_Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            SocialStatus_Label_Name.Visibility = Visibility.Visible;
            SocialStatus_TextBox_Name.Visibility = Visibility.Collapsed;

            SocialStatus_Button_Reduct.IsEnabled = true;
            SocialStatus_Button_Ok.IsEnabled = false;
            SocialStatus_Button_Cancel.IsEnabled = false;

            if (SocialStatus_Saved_Id != String.Empty)
            {
                SocialStatus_Selected_Id = SocialStatus_Saved_Id;
                SocialStatus_Saved_Id = "";
            }
        }

        private async void SocialStatus_Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            SocialStatus_Label_Name.Visibility = Visibility.Visible;
            SocialStatus_TextBox_Name.Visibility = Visibility.Collapsed;

            SocialStatus_Button_Reduct.IsEnabled = true;
            SocialStatus_Button_Ok.IsEnabled = false;
            SocialStatus_Button_Cancel.IsEnabled = false;
            string sql;
            if (SocialStatus_Selected_Id == "")
            {
                SocialStatus_Selected_Id = (await ConnectionCarrier.Carrier.GetCurrentSequenceId("SocialStatus")).ToString();
                sql = $"Insert Into \"SocialStatus\" (\"SocialStatus_Name\") Values ('{SocialStatus_TextBox_Name.Text}')";
            }
            else
            {
                sql = $"Update \"SocialStatus\" Set \"SocialStatus_Name\" = '{SocialStatus_TextBox_Name.Text}' Where \"SocialStatus_Id\" = {SocialStatus_Selected_Id}";
            }


            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await conn.OpenAsync();
                await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }

            SocialStatus_MenuItem_Refresh_Click(sender, e);
            SocialStatus_Label_Name.Content = SocialStatus_TextBox_Name.Text;
        }

        private async void SocialStatus_Button_SearchData_Click(object sender, RoutedEventArgs e)
        {
            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await conn.OpenAsync();
                String sql = $"Select * From \"SocialStatus\" Where lower(\"SocialStatus_Name\") Like '%{SocialStatus_TextBox_SearchData.Text.ToLower()}%'";
                var reader = await new NpgsqlCommand(sql, conn).ExecuteReaderAsync();

                var dt = this.NpgsqlDataReader_To_DataTable(reader, new Dictionary<string, string>()
                {
                    { "id", "SocialStatus_Id" },
                    { "Название cоциального положения", "SocialStatus_Name" }
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
        string? Procedure_Saved_Id = string.Empty;

        private async void Procedure_MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure_Saved_Id != string.Empty)
            {
                Procedure_Selected_Id = Procedure_Saved_Id;
                Procedure_Saved_Id = string.Empty;
            }
            else
            {
                Procedure_Selected_Id = string.Empty;
                Procedure_Saved_Id = string.Empty;
            }

            Procedure_Label_Name.Content = "";
            Procedure_Label_Name.Visibility = Visibility.Visible;
            Procedure_TextBox_Name.Visibility = Visibility.Collapsed;

            Procedure_Button_Reduct.IsEnabled = true;
            Procedure_Button_Ok.IsEnabled = false;
            Procedure_Button_Cancel.IsEnabled = false;

            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await conn.OpenAsync();
                String sql = "Select * From \"Procedure\"";
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

            RaiseFirstSelection(Procedure_DataGrid);
        }

        private void Procedure_MenuItem_Search_Click(object sender, RoutedEventArgs e)
        {
            this.Procedure_Grid_Search.Visibility =
               this.Procedure_Grid_Search.Visibility is Visibility.Visible ?
               Visibility.Collapsed : Visibility.Visible;
        }

        private void Procedure_MenuItem_Add_Click(object sender, RoutedEventArgs e)
        {
            Procedure_Label_Name.Visibility = Visibility.Collapsed;
            Procedure_TextBox_Name.Visibility = Visibility.Visible;

            Procedure_Button_Reduct.IsEnabled = false;
            Procedure_Button_Ok.IsEnabled = true;
            Procedure_Button_Cancel.IsEnabled = true;

            Procedure_TextBox_Name.Text = "";

            Procedure_Saved_Id = Procedure_Selected_Id;
            Procedure_Selected_Id = string.Empty;
        }

        private void Procedure_DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            Procedure_Label_Name.Visibility = Visibility.Visible;
            Procedure_TextBox_Name.Visibility = Visibility.Collapsed;

            Procedure_Button_Reduct.IsEnabled = true;
            Procedure_Button_Ok.IsEnabled = false;
            Procedure_Button_Cancel.IsEnabled = false;

            var vs = Procedure_DataGrid.SelectedIndex;
            var row = Procedure_DataGrid.Items[vs] as DataRowView;
            Procedure_Selected_Id = row?["id"].ToString();

            Procedure_TextBox_Name.Text = row?["Название процедуры"].ToString();
            Procedure_Label_Name.Content = row?["Название процедуры"].ToString();

            Procedure_Saved_Id = string.Empty;
        }

        private void Procedure_Button_Reduct_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure_Selected_Id == String.Empty)
                return;
            Procedure_Label_Name.Visibility = Visibility.Collapsed;
            Procedure_TextBox_Name.Visibility = Visibility.Visible;

            Procedure_Button_Reduct.IsEnabled = false;
            Procedure_Button_Ok.IsEnabled = true;
            Procedure_Button_Cancel.IsEnabled = true;

            Procedure_TextBox_Name.Text = Procedure_Label_Name.Content.ToString();

            Procedure_Saved_Id = string.Empty;
        }

        private void Procedure_Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Procedure_Label_Name.Visibility = Visibility.Visible;
            Procedure_TextBox_Name.Visibility = Visibility.Collapsed;

            Procedure_Button_Reduct.IsEnabled = true;
            Procedure_Button_Ok.IsEnabled = false;
            Procedure_Button_Cancel.IsEnabled = false;

            if (Procedure_Saved_Id != string.Empty)
            {
                Procedure_Selected_Id = Procedure_Saved_Id;
                Procedure_Saved_Id = "";
            }
                
        }

        private async void Procedure_Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            Procedure_Label_Name.Visibility = Visibility.Visible;
            Procedure_TextBox_Name.Visibility = Visibility.Collapsed;

            Procedure_Button_Reduct.IsEnabled = true;
            Procedure_Button_Ok.IsEnabled = false;
            Procedure_Button_Cancel.IsEnabled = false;

            string sql;
            if (Procedure_Selected_Id == "")
            {
                Procedure_Selected_Id = (await ConnectionCarrier.Carrier.GetCurrentSequenceId("Procedure")).ToString();
                sql = $"Insert Into \"Procedure\" (\"Procedure_Name\") Values ('{Procedure_TextBox_Name.Text}')";
            }
            else
            {
                sql = $"Update \"Procedure\" Set \"Procedure_Name\" = '{Procedure_TextBox_Name.Text}' Where \"Procedure_Id\" = {Procedure_Selected_Id}";
            }


            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await conn.OpenAsync();
                await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }

            Procedure_MenuItem_Refresh_Click(sender, e);

            Procedure_Label_Name.Content = Procedure_TextBox_Name.Text;
        }

        private async void Procedure_Button_SearchData_Click(object sender, RoutedEventArgs e)
        {
            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await conn.OpenAsync();
                String sql = $"Select * From \"Procedure\" Where lower(\"Procedure_Name\") Like '%{Procedure_TextBox_SearchData.Text.ToLower()}%'";
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
        string? Disease_Saved_Id = string.Empty;

        private async void Disease_MenuItem_Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (Disease_Saved_Id != string.Empty)
            {
                Disease_Selected_Id = Disease_Saved_Id;
                Disease_Saved_Id = string.Empty;
            }
            else
            {
                Disease_Selected_Id = string.Empty;
                Disease_Saved_Id = string.Empty;
            }

            Disease_Label_Name.Content = "";
            Disease_Label_Name.Visibility = Visibility.Visible;
            Disease_TextBox_Name.Visibility = Visibility.Collapsed;

            Disease_Button_Reduct.IsEnabled = true;
            Disease_Button_Ok.IsEnabled = false;
            Disease_Button_Cancel.IsEnabled = false;

            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await conn.OpenAsync();
                String sql = "Select * From \"Disease\"";
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

            RaiseFirstSelection(Disease_DataGrid);
        }

        private void Disease_MenuItem_Search_Click(object sender, RoutedEventArgs e)
        {
            this.Disease_Grid_Search.Visibility =
               this.Disease_Grid_Search.Visibility is Visibility.Visible ?
               Visibility.Collapsed : Visibility.Visible;
        }

        private void Disease_MenuItem_Add_Click(object sender, RoutedEventArgs e)
        {
            Disease_Label_Name.Visibility = Visibility.Collapsed;
            Disease_TextBox_Name.Visibility = Visibility.Visible;

            Disease_Button_Reduct.IsEnabled = false;
            Disease_Button_Ok.IsEnabled = true;
            Disease_Button_Cancel.IsEnabled = true;

            Disease_TextBox_Name.Text = "";

            Disease_Saved_Id = Disease_Selected_Id;
            Disease_Selected_Id = string.Empty;
        }

        private void Disease_DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            Disease_Label_Name.Visibility = Visibility.Visible;
            Disease_TextBox_Name.Visibility = Visibility.Collapsed;

            Disease_Button_Reduct.IsEnabled = true;
            Disease_Button_Ok.IsEnabled = false;
            Disease_Button_Cancel.IsEnabled = false;

            var vs = Disease_DataGrid.SelectedIndex;
            var row = Disease_DataGrid.Items[vs] as DataRowView;
            Disease_Selected_Id = row?["id"].ToString();

            Disease_TextBox_Name.Text = row?["Название заболевания"].ToString();
            Disease_Label_Name.Content = row?["Название заболевания"].ToString();

            Disease_Saved_Id = string.Empty;
        }

        private void Disease_Button_Reduct_Click(object sender, RoutedEventArgs e)
        {
            if (Disease_Selected_Id == String.Empty)
                return;
            Disease_Label_Name.Visibility = Visibility.Collapsed;
            Disease_TextBox_Name.Visibility = Visibility.Visible;

            Disease_Button_Reduct.IsEnabled = false;
            Disease_Button_Ok.IsEnabled = true;
            Disease_Button_Cancel.IsEnabled = true;

            Disease_TextBox_Name.Text = Disease_Label_Name.Content.ToString();

            Disease_Saved_Id = string.Empty;
        }

        private void Disease_Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Disease_Label_Name.Visibility = Visibility.Visible;
            Disease_TextBox_Name.Visibility = Visibility.Collapsed;

            Disease_Button_Reduct.IsEnabled = true;
            Disease_Button_Ok.IsEnabled = false;
            Disease_Button_Cancel.IsEnabled = false;

            if (Disease_Saved_Id != string.Empty)
            {
                Disease_Selected_Id = Disease_Saved_Id;
                Disease_Saved_Id = "";
            }

        }

        private async void Disease_Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            Disease_Label_Name.Visibility = Visibility.Visible;
            Disease_TextBox_Name.Visibility = Visibility.Collapsed;

            Disease_Button_Reduct.IsEnabled = true;
            Disease_Button_Ok.IsEnabled = false;
            Disease_Button_Cancel.IsEnabled = false;

            string sql;
            if (Disease_Selected_Id == "")
            {
                Disease_Selected_Id = (await ConnectionCarrier.Carrier.GetCurrentSequenceId("Disease")).ToString();
                sql = $"Insert Into \"Disease\" (\"Disease_Name\") Values ('{Disease_TextBox_Name.Text}')";
            }
            else
            {
                sql = $"Update \"Disease\" Set \"Disease_Name\" = '{Disease_TextBox_Name.Text}' Where \"Disease_Id\" = {Disease_Selected_Id}";
            }


            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await conn.OpenAsync();
                await new NpgsqlCommand(sql, conn).ExecuteNonQueryAsync();
            }
            catch
            {

            }
            finally
            {
                await conn.CloseAsync();
            }

            Disease_MenuItem_Refresh_Click(sender, e);

            Disease_Label_Name.Content = Disease_TextBox_Name.Text;
        }

        private async void Disease_Button_SearchData_Click(object sender, RoutedEventArgs e)
        {
            var conn = ConnectionCarrier.Carrier.Connection;

            try
            {
                await conn.OpenAsync();
                String sql = $"Select * From \"Disease\" Where lower(\"Disease_Name\") Like '%{Disease_TextBox_SearchData.Text.ToLower()}%'";
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
    }
}
