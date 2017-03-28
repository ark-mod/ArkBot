using System;
using System.Threading.Tasks;
using Discord;

namespace ArkBot.Helpers
{
    public interface IArkServerService : IDisposable
    {
        Task<bool> RestartServer(Func<string, Task<Message>> sendMessageDirected);
        Task<bool> ShutdownServer(Func<string, Task<Message>> sendMessageDirected, bool warnIfServerIsNotStarted = true);
        Task<bool> StartServer(Func<string, Task<Message>> sendMessageDirected, bool warnIfServerIsAlreadyStarted = true);
        Task<bool> UpdateServer(Func<string, Task<Message>> sendMessageDirected, Func<string, string> getMessageDirected);
        Task<bool> SaveWorld(Func<string, Task<Message>> sendMessageDirected, int timeoutSeconds, bool noUpdateForThisCall = false);
    }
}