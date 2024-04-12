namespace Masuit.Tools.Files;

public class BoolWord
{
    public string Word { get; set; }

    public bool Value { get; set; }

    public BoolWord(string word, bool value)
    {
        Word = word;
        Value = value;
    }
}