using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatGPT_4.Views
{
    /// <summary>
    /// Interaction logic for UserControl.xaml
    /// </summary>
    public partial class UserControl : Page
    {
        private MainWindow _mainWindow;
        public UserControl(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeComponent();
            GetUsersFromDb();
        }

        private void GetUsersFromDb()
        {
            try
            {
                List<User> users = new List<User>();

                using (MySqlConnection conn = new MySqlConnection(_mainWindow.connectionString))
                {
                    conn.Open();

                    string query = "SELECT userid, username, password FROM user;";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new User
                                {
                                    UserId = reader.GetInt32("userid"),
                                    Username = reader.GetString("username"),
                                    Password = reader.GetString("password")
                                    
                                });
                            }
                        }
                    }
                }

                userDataGrid.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = HashPassword(txtPassword.Password); 

            string connectionString = _mainWindow.connectionString; 

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO user (username, password) VALUES (@username, @password) ON DUPLICATE KEY UPDATE password = @password;";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("User Registered/Updated Successfully");
                        }
                        else
                        {
                            MessageBox.Show("No changes were made.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            GetUsersFromDb();
            txtPassword.Clear();
            txtUsername.Clear();
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            User user = (User)userDataGrid.SelectedItem;
            string connectionString = _mainWindow.connectionString; 

            if (userDataGrid.SelectedItem != null)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = @"DELETE FROM user WHERE userid = @userid;";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@userid", user.UserId);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("User Deleted");
                            }
                            else
                            {
                                MessageBox.Show("No changes were made.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
                GetUsersFromDb();
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

        private void UserDataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (userDataGrid.SelectedItem != null)
            {
                User user = (User)userDataGrid.SelectedItem;
                txtUsername.Text = user.Username;
            }
        }
    }
}
