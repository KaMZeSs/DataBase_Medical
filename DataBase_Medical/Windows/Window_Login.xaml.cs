using System;
using System.Collections.Generic;
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

namespace DataBase_Medical.Windows
{
    /// <summary>
    /// Логика взаимодействия для Window_Login.xaml
    /// </summary>
    public partial class Window_Login : Window
    {
        public Window_Login()
        {
            InitializeComponent();
            this.Loggin_box.Text = "chief";
            this.Password_box.Password = "admin";
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var loggin = this.Loggin_box.Text;
            var pswd = this.Password_box.Password;
            string vs;
            Task<String> task = null;
            try
            {
                ConnectionCarrier.CreateConnection(loggin, pswd);
                task = ConnectionCarrier.Carrier.GetCurrentJob();
                
                vs = await task;
            }
            catch
            {
                if (task.Exception.Message.StartsWith("28P01"))
                {
                    MessageBox.Show("Вы ввели неверный логин, или пароль.\nЕсли вы забыли логин, или пароль - обратитесь к администратору",
                            "Ошибка входа");
                }
                return;
            }

            if (loggin is "postgres")
            {
                MessageBox.Show("Вы ввели неверный логин, или пароль.\nЕсли вы забыли логин, или пароль - обратитесь к администратору",
                                "Ошибка входа");
            }
            

            switch (vs)
            {
                case "Главный врач":
                {
                    var window = new Window_Chief();
                    window.Show();
                    break;
                }
                case "Заведующий отделением":
                {
                    var window = new Window_HeadOfTheDepartment();
                    window.Show();
                    break;
                }
                case "Врач":
                {
                    var window = new Window_Doctor();
                    window.Show();
                    break;
                }
                case "Администратор":
                {
                    var window = new Window_Administrator();
                    window.Show();
                    break;
                }
                default:
                {
                    MessageBox.Show("Вашей должности не существует.\nОбратитесь к администратору",
                        "Ошибка должности сотрудника");
                    return;
                }
            }
            this.Close();
        }
    }
}
