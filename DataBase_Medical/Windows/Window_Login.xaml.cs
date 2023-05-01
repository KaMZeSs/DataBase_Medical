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
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var loggin = this.Loggin_box.Text;
            var pswd = this.Password_box.Password;
            ConnectionCarrier.CreateConnection(loggin, pswd);
            var vs = await ConnectionCarrier.Carrier.GetCurrentJob();

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
                    break;
                }
            }
            this.Close();
        }
    }
}
