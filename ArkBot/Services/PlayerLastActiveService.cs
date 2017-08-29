using ArkBot.Ark;
using ArkBot.Services.Data;
using ArkBot.Threading;
using Autofac;
using Discord;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ArkBot.Helpers
{
    /// <summary>
    /// Service for keeping track of when players were last active
    /// </summary>
    public class PlayerLastActiveService : IPlayerLastActiveService
    {
        private ISavedState _savedState;
        private ArkContextManager _contextManager;
        private ActionBlock<IArkUpdateableContext> _queue;

        public PlayerLastActiveService(ISavedState savedState, ArkContextManager contextManager)
        {
            _savedState = savedState;
            _contextManager = contextManager;

            _queue = new ActionBlock<IArkUpdateableContext>(
                updateableContext =>
                {
                    try
                    {
                        if (updateableContext is ArkServerContext)
                        {
                            var serverContext = updateableContext as ArkServerContext;
                            foreach (var player in serverContext.Players)
                            {
                                var state = _savedState.PlayerLastActive.FirstOrDefault(x => 
                                    x.ServerKey != null 
                                    && serverContext.Config.Key.Equals(x.ServerKey) 
                                    && x.SteamId != null 
                                    && x.SteamId.Equals(player.SteamId, StringComparison.OrdinalIgnoreCase));
                                if (state == null)
                                {
                                    state = new PlayerLastActiveSavedState
                                    {
                                        ServerKey = serverContext.Config.Key,
                                        SteamId = player.SteamId,
                                        LastActiveTime = player.LastActiveTime
                                    };

                                    _savedState.PlayerLastActive.Add(state);
                                }
                                else if (player.LastActiveTime > state.LastActiveTime) state.LastActiveTime = player.LastActiveTime;

                                state.Id = player.Id;
                                state.TribeId = player.TribeId;
                                state.Name = player.Name;
                                state.CharacterName = player.CharacterName;
                            }

                            _savedState.Save();
                        }
                    }
                    catch(Exception ex)
                    {

                    }
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 }
            );

            contextManager.GameDataUpdated += ContextManager_GameDataUpdated;
        }

        private void ContextManager_GameDataUpdated(IArkUpdateableContext sender)
        {
            _queue.Post(sender);
        }
    }
}
