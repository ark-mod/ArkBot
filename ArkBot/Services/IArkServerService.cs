using System;
using System.Threading.Tasks;
using Discord;

namespace ArkBot.Helpers
{
    public interface IArkServerService
    {
        Task<bool> RestartServer(string serverKey, Func<string, Task<IUserMessage>> sendMessageDirected);
        Task<bool> ShutdownServer(string serverKey, Func<string, Task<IUserMessage>> sendMessageDirected, bool warnIfServerIsNotStarted = true, bool forcedShutdown = false);
        Task<bool> StartServer(string serverKey, Func<string, Task<IUserMessage>> sendMessageDirected, bool warnIfServerIsAlreadyStarted = true);
        Task<bool> UpdateServer(string serverKey, Func<string, Task<IUserMessage>> sendMessageDirected, Func<string, string> getMessageDirected, int timeoutSeconds);
        Task<bool> SaveWorld(string serverKey, Func<string, Task<IUserMessage>> sendMessageDirected, int timeoutSeconds, bool noUpdateForThisCall = false);
        DateTime? GetServerStartTime(string serverKey);
    }
}