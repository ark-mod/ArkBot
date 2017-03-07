using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Commands
{
    public interface IRoleRestrictedCommand : ICommand
    {
        string[] ForRoles { get; }
    }
}
