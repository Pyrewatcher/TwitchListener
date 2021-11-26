using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
    [UsedImplicitly]
    public class NeonCommand : ICommand
    {
        private readonly TwitchClient _client;

        public NeonCommand(TwitchClient client)
        {
            _client = client;
        }

        public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
        {
            var days = (DateTime.Now - new DateTime(2021, 10, 16)).Days;

            _client.SendMessage(message.Channel, string.Format(Globals.Locale["neon_response"], days));

            return await Task.FromResult(true);
        }
    }
}
