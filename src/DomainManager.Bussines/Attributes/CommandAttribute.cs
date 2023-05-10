namespace DomainManager.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class CommandAttribute : Attribute {
    public CommandAttribute(string text) {
        Text = text;
    }

    public string Text { get; }
    public string? Description { get; init; }

    public string? Help { get; init; }
    public bool RegisterCommand { get; init; } = true;

    public bool InlineCommand { get; init; }
}