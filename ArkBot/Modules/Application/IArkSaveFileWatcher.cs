using System;

namespace ArkBot.Modules.Application
{
    public interface IArkSaveFileWatcher : IDisposable
    {
        event ArkSaveFileChangedEventHandler Changed;
    }

    public delegate void ArkSaveFileChangedEventHandler(ArkServerContext context, ArkSaveFileChangedEventArgs e);
}