namespace SimpleVote.Shared;

public class InputPrompter
{
    private readonly string? info;
    private readonly string prompt;
    private readonly string[]? acceptableInputs;
    private readonly Func<string, bool>? inputEvaluator;

    public InputPrompter(string prompt, string? info = null,
        string[]? acceptableInputs = null)
    {
        this.info = info;
        this.prompt = prompt;
        this.acceptableInputs = acceptableInputs;
    }

    public InputPrompter(string prompt, string? info = null,
        Func<string, bool>? inputEvaluator = null)
    {
        this.info = info;
        this.prompt = prompt;
        this.inputEvaluator = inputEvaluator;
    }

    public string Prompt()
    {
        if (!string.IsNullOrEmpty(info))
        {
            Console.WriteLine(info);
        }

        string? input = string.Empty;
        do
        {
            Console.Write(prompt);
            input = Console.ReadLine();
        }
        while ((acceptableInputs != null && !acceptableInputs.Contains(input))
            || (inputEvaluator != null && !inputEvaluator(input!)));

        return input!;
    }
}