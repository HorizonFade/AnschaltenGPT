using System.Reflection;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Text;

namespace ChatGPT_4.Views;

/// <summary>
///     Interaction logic for Login.xaml
/// </summary>
public partial class Login : Page
{
    private MainWindow _mainWindow; 
    public Login(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        string connectionString = _mainWindow.connectionString;

        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT userid, password FROM user WHERE username=@username LIMIT 1;";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", txtUsername.Text);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) 
                        {
                            string storedPasswordHash = reader.GetString("password");
                            string inputPasswordHash = HashPassword(txtPassword.Password);

                            if (storedPasswordHash == inputPasswordHash)
                            {
                                int userId = reader.GetInt32("userid");

                                MessageBox.Show("Login Successful", "Login", MessageBoxButton.OK, MessageBoxImage.Information);
                                _mainWindow.EnableButtons();
                                _mainWindow.userID = userId;
                            }
                            else
                            {
                                MessageBox.Show("Login Failed", "Login", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Login Failed", "Login", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}");
        }
    
}

    public static string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
