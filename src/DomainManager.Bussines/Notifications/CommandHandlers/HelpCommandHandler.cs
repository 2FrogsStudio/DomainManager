using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DomainManager.Notifications.CommandHandlers;

public class HelpCommandHandler : CommandHandlerBase {
    public HelpCommandHandler(ITelegramBotClient botClient) : base(Command.Help, botClient) { }

    protected override Task<string> Consume(string[] args, Message message, CancellationToken cancellationToken) {
        var sb = new StringBuilder("Usage:\n");
        foreach (var (_, commandDescription) in
                 CommandHelpers.CommandAttributeByCommand.Where(c => c.Value is not null))
            sb.Append($"{commandDescription!.Text}\t- {commandDescription.Description}\n");
        var text = sb.ToString().TrimEnd('\n');

        return Task.FromResult(text.Replace("_", @"\_"));
    }
}