using ArkBot.Ark;
using System;

namespace ArkBot
{
    public interface IArkSaveFileWatcher : IDisposable
    {
        event ArkSaveFileChangedEventHandler Changed;
    }

    public delegate void ArkSaveFileChangedEventHandler(ArkServerContext context, ArkSaveFileChangedEventArgs e);
}