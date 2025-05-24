namespace Shavkat_grabber.Models;

public class Question
{
    public string QuestionText { get; }
    public string[] Answers { get; }

    public Question(string questionText, string[] answers)
    {
        QuestionText = questionText;
        Answers = answers;
    }
}
