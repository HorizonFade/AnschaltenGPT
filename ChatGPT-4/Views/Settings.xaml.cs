using OpenAI;
using System.Windows;
using System.Windows.Controls;

namespace ChatGPT_4.Views;

/// <summary>
///     Interaction logic for Settings.xaml
/// </summary>
public partial class Settings : Page
{
    private MainWindow _mainWindow;
    public Settings(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        if (_mainWindow.userID == 10)
        {
            UserControl.IsEnabled = true;
            UserControl.Visibility = Visibility.Visible;
        }
    }

    private void UserControl_OnClick(object sender, RoutedEventArgs e)
    {
        _mainWindow.UserControlNav();
    }
}