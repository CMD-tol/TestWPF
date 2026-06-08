using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.Sql;
using Microsoft.Data.Sqlite;
namespace TestWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        // окно авторизации 

       
        private void ButtonCatalog_Click(object sender, RoutedEventArgs e)
        {
            GuestWindow GuestCatalog = new GuestWindow();
            GuestCatalog.Show();
        }
        private void ButtonAuth_Click(object sender, RoutedEventArgs e)
        {
            string Connect = @"Data Source=E:\Подготовка к демоэгзамену\TestWPF\Resurce\DB\DBApp.db";
            using (var Conn = new SqliteConnection(Connect))
            {
                string Login = LoginBox.Text;
                string Password = PasswordBox.Password;
                Conn.Open();
                string query = @"
            SELECT 
                u.user_id,
                u.login,
                u.password,
                u.last_name,
                u.first_name,
                u.middle_name,
                u.role_id,
                r.role_name
            FROM Users u
            JOIN Roles r ON r.role_id = u.role_id
            WHERE u.login = @login AND u.password = @password";
                ;

                using (var command = new SqliteCommand(query, Conn))
                {
                    
                    command.Parameters.AddWithValue("@login", Login);
                    command.Parameters.AddWithValue("@password", Password);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {

                           
                            string UserName =  reader.GetString(reader.GetOrdinal("last_name"));
                            string Role = reader.GetString(reader.GetOrdinal("role_name"));
                            if(Role == "Администратор" || Role == "administrator")
                            {
                                CataloWindow cataloWindow = new CataloWindow();
                                cataloWindow.SetUserName(UserName);
                                cataloWindow.Show();
                                this.Close();
                            }
                            if (Role == "guest" || Role == "Гость")
                            {
                                GuestWindow GuestWindow = new GuestWindow();
                               
                                GuestWindow.Show();
                                this.Close();
                            }

                            if (Role == "Менеджер" || Role == "manager")
                            {
                                ManagerWindow cataloWindow = new ManagerWindow();
                                cataloWindow.SetUserName(UserName);
                                cataloWindow.Show();
                                this.Close();
                            }

                        }
                        else
                        {
                            MessageBox.Show("Проверте правльность написание пароля или логина");
                        }
                    }
                }
            }
        }
    }
}