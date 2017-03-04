using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Commands
{
    public interface ICommand
    {
        string Name { get; }
        string[] Aliases { get; }
        string Description { get; }
        string SyntaxHelp { get; }
        string[] UsageExamples { get; }
        bool DebugOnly { get; }
        bool HideFromCommandList { get; }

        Task Run(CommandEventArgs e);
        void Register(CommandBuilder command);
        void Init(DiscordClient client);
    }
}
