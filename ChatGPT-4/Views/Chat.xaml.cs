using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using System.IO;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;

namespace ChatGPT_4.Views;

/// <summary>
///     Interaction logic for Chat.xaml
/// </summary>

public partial class Chat : Page
{
    
    private readonly string _initialMessage = "Hi, wie kann ich dir helfen?";
    private string? _modelDisplayName = "GPT-4-Turbo-Preview";
    private string? _modelValue;
    private string _apiKey;
    private string _conversationPath;
    private MainWindow _mainWindow;

    public Chat(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        _apiKey = _mainWindow.key;
        GetModels();
        InitializeComponent();
        _conversationPath = GetConversationsFilePath();
        AddFileConversations(_conversationPath);
        OutputText.Document.Blocks.Clear();

        OutputText.Document.Blocks.Add(AddParagraph(Role.Assistant,_initialMessage));
        if (_mainWindow.userID == 10)
        {
            Version.IsEnabled = true;
        }
        if (!Conversation.IsEmpty())
        {
            foreach (var message in Conversation.GetList())
            {
                if (message.Role == Role.User)
                {
                    OutputText.Document.Blocks.Add(AddParagraph(Role.User,message.ToString()));
                }
                else
                {
                    OutputText.Document.Blocks.Add(AddParagraph(Role.Assistant,message.ToString()));
                }

            }
            

        }




    }


    private async void Send_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(InputText.Text))
        {
            await SendMessageAsync();
        }
        else
        {

        }

    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        OutputText.Document.Blocks.Clear();
        Conversation.ClearMessages();
        if (File.Exists(_conversationPath))
        {
            File.Delete(_conversationPath);
        }

        OutputText.Document.Blocks.Add(AddParagraph(Role.Assistant,_initialMessage));

    }

    public async Task SendMessageAsync()
    {
        try
        {
            string userInput = InputText.Text;
            OutputText.Document.Blocks.Add(AddParagraph(Role.User, userInput));

            Conversation.AddMessage(new Message(Role.User, userInput));
            int userInputWordCount = userInput.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            InputText.Clear();
            WriteToTextFile();
            
            
            using var api = new OpenAIClient(_apiKey);

            var chatRequest = new ChatRequest(Conversation.GetList(), new Model(_modelValue));


            OutputText.Document.Blocks.Add(AddParagraph(Role.Assistant,""));

            var response = await api.ChatEndpoint.StreamCompletionAsync(chatRequest,
                partialResponse =>
                {
                    Dispatcher.Invoke(() => { OutputText.AppendText($"{partialResponse.FirstChoice.Delta}"); });
                });
            var choice = response.FirstChoice;

            Conversation.AddMessage(new Message(Role.Assistant, choice.Message));
            int choiceWordCount = choice.Message.Content.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            _mainWindow.AddToDatabase(1,userInputWordCount,choiceWordCount);
            WriteToTextFile();
        }
        catch (Exception e)
        {
            OutputText.AppendText("Error: " + e.Message);
            Console.WriteLine(e);
            throw;
        }
    }

    public async void GetModels()
    {
        try
        {
            using var api = new OpenAIClient(_apiKey);
            var models = await api.ModelsEndpoint.GetModelsAsync();

            foreach (string model in models)
                if (model.StartsWith("gpt", StringComparison.OrdinalIgnoreCase))
                {
                    var gpt = model.Replace("g", "G").Replace("p", "P").Replace("t", "T");
                    var chars = gpt.ToCharArray();

                    for (var i = 0; i < chars.Length; i++)
                        if (chars[i] >= 'a' && chars[i] <= 'z' && i > 0 && chars[i - 1] == '-')
                            chars[i] = char.ToUpper(chars[i]);

                    Version.Items.Add(new GPTModels(new string(chars), model));
                }

            foreach (var item in Version.Items)
            {
                var gptModel = (GPTModels)item;
                if (gptModel.Value == "gpt-4-turbo-preview") Version.SelectedItem = item;
            }
        }
        catch (Exception e)
        {
            MessageBox.Show("Please enter a message", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
        
    }

    private void Version_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = (GPTModels)Version.SelectedItem;

        _modelDisplayName = selected.DisplayName;
        _modelValue = selected.Value;

    }

    private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        OutputText.ScrollToEnd();
    }

    public void WriteToTextFile()
    {
        try
        {
            string json = JsonConvert.SerializeObject(Conversation.JsoList,Formatting.Indented);

            File.WriteAllText(_conversationPath, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while writing to the file: {ex.Message}", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
            InputText.Text=ex.Message;
        }
    }

    public void AddFileConversations(string filePath)
    {
        try
        {
            if (File.Exists(filePath)&&new FileInfo(filePath).Length!=0)
            {
                string json = File.ReadAllText(filePath);

                List<JSONList> messages = JsonConvert.DeserializeObject<List<JSONList>>(json);
                
                
                Conversation.ClearMessages();


                foreach (JSONList message in messages)
                {
                    Conversation.AddMessage(new Message(message.Role,message.Content));
                }

            }
            else
            {
                
                using (File.Create(filePath)) { } 
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while reading from the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Console.WriteLine(ex.Message);
            InputText.Text = ex.Message;
        }
    }
    private Paragraph AddParagraph(Role role, string text)
    {
        switch (role)
        {
            case Role.User:
                Paragraph userParagraph = new Paragraph();
                userParagraph.Margin = new Thickness(0);
                Run userBoldRun = new Run("You: ");
                userBoldRun.Foreground = Brushes.LightPink;
                userParagraph.Inlines.Add(new Bold(userBoldRun));
                userParagraph.Inlines.Add(new Run(text));
                return userParagraph;

            case Role.Assistant:
                Paragraph gptParagraph = new Paragraph();
                gptParagraph.Margin = new Thickness(0);
                Run gptBoldRun = new Run($"{_modelDisplayName}: ");
                gptBoldRun.Foreground = Brushes.LightBlue;
                gptParagraph.Inlines.Add(new Bold(gptBoldRun));
                gptParagraph.Inlines.Add(new Run(text));
                return gptParagraph;

            default:
                Paragraph errorParagraph = new Paragraph();
                errorParagraph.Margin = new Thickness(0);
                Run errorBoldRun = new Run("There was an Error!");
                errorBoldRun.Foreground = Brushes.Red;
                errorParagraph.Inlines.Add(new Bold(errorBoldRun));
                return errorParagraph;

        }
    }
    public string GetConversationsFilePath()
    {
        string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        string appDir = Path.Combine(appDataDir, "AnschaltenGPT");
        Directory.CreateDirectory(appDir);

        return Path.Combine(appDir, "conversations.json");
    }

    



}