using System;

namespace ArkBot
{
    public interface IArkSaveFileWatcher : IDisposable
    {
        event ArkSaveFileChangedEventHandler Changed;
    }

    public delegate void ArkSaveFileChangedEventHandler(object sender, ArkSaveFileChangedEventArgs e);
}