using System;
using System.Threading.Tasks;
using Discord;

namespace ArkBot.Helpers
{
    public interface IArkServerService
    {
        Task<bool> RestartServer(string serverKey, Func<string, Task<Message>> sendMessageDirected);
        Task<bool> ShutdownServer(string serverKey, Func<string, Task<Message>> sendMessageDirected, bool warnIfServerIsNotStarted = true, bool skipSavingBeforeShutdown = false);
        Task<bool> StartServer(string serverKey, Func<string, Task<Message>> sendMessageDirected, bool warnIfServerIsAlreadyStarted = true);
        Task<bool> UpdateServer(string serverKey, Func<string, Task<Message>> sendMessageDirected, Func<string, string> getMessageDirected, int timeoutSeconds);
        Task<bool> SaveWorld(string serverKey, Func<string, Task<Message>> sendMessageDirected, int timeoutSeconds, bool noUpdateForThisCall = false);
    }
}