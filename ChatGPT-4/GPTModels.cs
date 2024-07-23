namespace ChatGPT_4;

public class GPTModels
{
    public GPTModels(string displayName, string value)
    {
        DisplayName = displayName;
        Value = value;
    }

    public string DisplayName { get; set; }
    public string Value { get; set; }
}