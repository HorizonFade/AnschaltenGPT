using ChatGPT_4.Views;
using System.Windows;
using System.Windows.Input;
using OpenAI;
using OpenAI.Chat;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using OpenAI.Threads;
using MySql.Data.MySqlClient;

[assembly: ArmDot.Client.VirtualizeCode]

namespace ChatGPT_4;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool isDragging;
    private Point startPoint;
    public int userID;
    public string key;
    public string connectionString;

    public MainWindow()
    {
        InitializeComponent();
        MainFrame.Content = new Login(this);
        //key = "";
        key = Properties.Resources.apiKey;
        //connectionString = "";
        connectionString = Properties.Resources.connectionString;
    }
    private void DashboardButton_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Dashboard());
        NaviLabel.Content = "Dashboard";
    }

    private void ChatButton_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Chat(this));
        NaviLabel.Content = "Chat";
    }

    private void ImageButton_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Image(this));
        NaviLabel.Content = "Image Generation";
    }
    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new Settings(this));
        NaviLabel.Content = "Settings";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized; 
    }

    private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            isDragging = true;
            startPoint = e.GetPosition(this);
        }
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (isDragging)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - startPoint.X;
            var deltaY = currentPoint.Y - startPoint.Y;

            Left += deltaX;
            Top += deltaY;
        }
    }

    private void Window_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) isDragging = false;
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            isDragging = true;
            startPoint = e.GetPosition(this);
        }
    }
    public void EnableButtons()
    {
        Dashboard.IsEnabled = true;
        Chat.IsEnabled = true;
        Image.IsEnabled = true;
        Settings.IsEnabled = true;
        MainFrame.Navigate(new Dashboard());
        NaviLabel.Content = "Dashboard";
    }

    public void UserControlNav()
    {
        MainFrame.Navigate(new UserControl(this));
        NaviLabel.Content = "User Control";
    }
    public void AddToDatabase(int modelid, int request, int answer)
    {
        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string updateStatsQuery = @"
            UPDATE request
            SET 
                requests = requests + @requests, 
                answers = answers + @answers
            WHERE userid = @userid AND modelid = @modelid;";

                using (MySqlCommand cmd = new MySqlCommand(updateStatsQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@requests", request);
                    cmd.Parameters.AddWithValue("@answers", answer);
                    cmd.Parameters.AddWithValue("@userid", userID);
                    cmd.Parameters.AddWithValue("@modelid", modelid);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows == 0)
                    {
                        string insertQuery = @"
                        INSERT INTO request (userid, modelid, requests, answers)
                        VALUES (@userid, @modelid, @requests, @answers);";

                        cmd.CommandText = insertQuery;

                        cmd.ExecuteNonQuery();
                        Console.WriteLine("New user stats row inserted.");
                    }
                    else
                    {
                        Console.WriteLine("User stats successfully updated.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while updating user stats: {ex.Message}");
        }
    }
}