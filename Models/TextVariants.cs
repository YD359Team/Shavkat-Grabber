namespace Shavkat_grabber.Models;

public class TextVariants
{
    public string PlainText { get; }
    public string Markdown { get; }

    public TextVariants(string plainText, string markdown)
    {
        PlainText = plainText;
        Markdown = markdown;
    }
}
