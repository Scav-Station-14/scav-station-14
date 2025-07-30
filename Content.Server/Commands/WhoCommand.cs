using System.Text;
using Robust.Server.Player;
using Robust.Shared.Console;
using Content.Server.Chat.Managers;
using Content.Shared.Chat;

namespace Content.Server.Commands;

public sealed class Who : LocalizedCommands
{
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    public override string Command => "who";
    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        //copied from listplayers command but without the IPs and avalible to all players
        var sb = new StringBuilder();

        var players = _players.Sessions;
        sb.AppendLine($"{"Player Name",20} {"Status",12} {"Playing Time",14} {"Ping",9}");
        sb.AppendLine("--------------------------------------------------------------");

        foreach (var p in players)
        {
            sb.AppendLine(string.Format("{3,20} {0,12} {1,14:hh\\:mm\\:ss} {2,9}",
                p.Status.ToString(),
                DateTime.UtcNow - p.ConnectedTime,
                p.Channel.Ping + "ms",
                p.Name));
        }

        shell.WriteLine(sb.ToString());
        if (shell.Player!=null)
        {
            _chat.DispatchServerMessage(shell.Player, sb.ToString());
        }

    }

}

