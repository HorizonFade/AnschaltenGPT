using Microsoft.Win32;
using OpenAI;
using OpenAI.Images;
using OpenAI.Models;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ChatGPT_4.Views;

/// <summary>
///     Interaction logic for Chat.xaml
/// </summary>
public partial class Image : Page
{
    private string _imageURL;
    private MainWindow _mainWindow;

    public Image(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
    }

    private async void Send_Click(object sender, RoutedEventArgs e)
    {
        if (InputText.Text != "")
            await GenerateImageAsync();
        else
            MessageBox.Show("Bitte eine Nachricht eingeben!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        Clear();
    }

    public async Task GenerateImageAsync()
    {
        if (!string.IsNullOrWhiteSpace(InputText.Text))
            try
            {
                AIImage.Source = null;
                var prompt = InputText.Text;
                using var api = new OpenAIClient(_mainWindow.key);
                var request =
                    new ImageGenerationRequest(prompt,
                        Model.DallE_3, size: "1024x1024");
                InputText.Clear();
                ImageText.Text = "Warte auf Bildgenerierung...";
                AIImage.Visibility = Visibility.Visible;

                var imageResult = await api.ImagesEndPoint.GenerateImageAsync(request);


                foreach (var image in imageResult)
                {
                    _imageURL = image.Url;
                    AIImage.Source = new BitmapImage(new Uri(image.Url));
                }

                ImageText.Visibility = Visibility.Hidden;

                SaveButton.Visibility = Visibility.Visible;
                _mainWindow.AddToDatabase(2,1,1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        else
            MessageBox.Show("Bitte eine Nachricht eingeben!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_imageURL))
            try
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg|All Files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(_imageURL, saveFileDialog.FileName);
                    }

                    MessageBox.Show($"Bild gespeichert unter: {saveFileDialog.FileName}", "Saved", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        else
            MessageBox.Show("Kein Bild generiert oder konnte nicht geladen werden", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
    }

    private void Clear()
    {
        AIImage.Source = null;
        _imageURL = "";
        ImageText.Text = "Schreibe unten in die Textbox, um ein Bild zu generieren";
        AIImage.Visibility = Visibility.Hidden;
        ImageText.Visibility = Visibility.Visible;
        SaveButton.Visibility = Visibility.Hidden;
    }
}