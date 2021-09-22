using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Handlers;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;

namespace Pyrewatcher
{
  public class Bot
  {
    private readonly ActionHandler _actionHandler;
    private readonly BroadcasterRepository _broadcasters;
    private readonly ILocalizationRepository _localization;
    private readonly TwitchClient _client;
    private readonly CommandHandler _commandHandler;
    private readonly IConfiguration _configuration;
    private readonly CyclicTasksHandler _cyclicTasksHandler;
    private readonly ILogger<Bot> _logger;

    public Bot(TwitchClient client, IConfiguration configuration, ILocalizationRepository localization, ILogger<Bot> logger,
               BroadcasterRepository broadcasters, CommandHandler commandHandler, ActionHandler actionHandler, CyclicTasksHandler cyclicTasksHandler)
    {
      _client = client;
      _configuration = configuration;
      _localization = localization;
      _logger = logger;
      _broadcasters = broadcasters;
      _commandHandler = commandHandler;
      _actionHandler = actionHandler;
      _cyclicTasksHandler = cyclicTasksHandler;
    }

    public async Task Setup()
    {
      _client.OnMessageReceived += OnMessageReceived;
      _client.OnJoinedChannel += OnJoinedChannel;
      _client.OnConnected += OnConnected;
      _client.OnFailureToReceiveJoinConfirmation += OnFailureToReceiveJoinConfirmation;
      _client.OnChatCommandReceived += OnChatCommandReceived;
      _client.OnCommunitySubscription += OnCommunitySubscription;
      _client.OnConnectionError += OnConnectionError;
      _client.OnDisconnected += OnDisconnected;
      _client.OnError += OnError;
      _client.OnGiftedSubscription += OnGiftedSubscription;
      _client.OnLeftChannel += OnLeftChannel;
      _client.OnNewSubscriber += OnNewSubscriber;
      _client.OnReSubscriber += OnReSubscriber;
      _client.OnUnaccountedFor += OnUnaccountedFor;

      var channels = (await _broadcasters.FindWithNameAllConnectedAsync()).Select(x => x.Name).ToList();
      Globals.Locale = await _localization.GetLocalizationByCode(Globals.LocaleCode);

      var credentials = new ConnectionCredentials(_configuration.GetSection("Twitch")["Username"], _configuration.GetSection("Twitch")["IrcToken"],
                                                  capabilities: new Capabilities(false));

      _client.Initialize(credentials, channels);
      _client.AddChatCommandIdentifier('\\');
    }

    public void Connect()
    {
      _client.Connect();

      _cyclicTasksHandler.RunTasks();
    }

    private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
    {
      var message = e.ChatMessage;

      if (message.Username is "scytlee_" or "viskul" && message.Message.StartsWith("!!"))
      {
        _client.SendMessage(message.Channel, $" {message.Message[2..]}");
      }

      if (message.Username == "scytlee_" && message.Message.StartsWith("nervSub"))
      {
        _client.SendMessage(message.Channel, message.Message);
      }
    }

    private void OnUnaccountedFor(object sender, OnUnaccountedForArgs e)
    {
      _logger.LogInformation("Unhandled IRC message: {message}", e.RawIRC);
    }

    private async void OnNewSubscriber(object sender, OnNewSubscriberArgs e)
    {
      var action = new Dictionary<string, string>
      {
        {"msg-id", e.Subscriber.MsgId},
        {"broadcaster", e.Channel},
        {"user-id", e.Subscriber.UserId},
        {"display-name", e.Subscriber.DisplayName},
        {"msg-param-sub-plan", e.Subscriber.SubscriptionPlanName}
      };

      await _actionHandler.HandleActionAsync(action);
    }

    private async void OnReSubscriber(object sender, OnReSubscriberArgs e)
    {
      var action = new Dictionary<string, string>
      {
        {"msg-id", e.ReSubscriber.MsgId},
        {"broadcaster", e.Channel},
        {"user-id", e.ReSubscriber.UserId},
        {"display-name", e.ReSubscriber.DisplayName},
        {"msg-param-sub-plan", e.ReSubscriber.SubscriptionPlanName}
      };

      await _actionHandler.HandleActionAsync(action);
    }

    private async void OnGiftedSubscription(object sender, OnGiftedSubscriptionArgs e)
    {
      var action = new Dictionary<string, string>
      {
        {"msg-id", e.GiftedSubscription.MsgId},
        {"broadcaster", e.Channel},
        {"user-id", e.GiftedSubscription.UserId},
        {"display-name", e.GiftedSubscription.DisplayName},
        {"msg-param-sub-plan", e.GiftedSubscription.MsgParamSubPlanName},
        {"msg-param-recipient-id", e.GiftedSubscription.MsgParamRecipientId},
        {"msg-param-recipient-display-name", e.GiftedSubscription.MsgParamRecipientDisplayName}
      };

      await _actionHandler.HandleActionAsync(action);
    }

    private void OnConnected(object sender, OnConnectedArgs e)
    {
      _logger.LogInformation("Connected");
    }

    private void OnDisconnected(object sender, OnDisconnectedEventArgs e)
    {
      _logger.LogInformation("Disconnected");
    }

    private void OnConnectionError(object sender, OnConnectionErrorArgs e)
    {
      _logger.LogError("OnConnectionError - An error occurred: {Message}", e.Error.Message);
    }

    private void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
    {
      _logger.LogInformation("Pyrewatcher connected to channel {channel}", e.Channel);
    }

    private void OnLeftChannel(object sender, OnLeftChannelArgs e)
    {
      _logger.LogInformation("Pyrewatcher disconnected from channel {channel}", e.Channel);
    }

    private void OnFailureToReceiveJoinConfirmation(object sender, OnFailureToReceiveJoinConfirmationArgs e)
    {
      _logger.LogInformation("Failed to connect to channel {channel}: {error}", e.Exception.Channel, e.Exception.Details);
      _client.JoinChannel(e.Exception.Channel);
    }

    private void OnError(object sender, OnErrorEventArgs e)
    {
      _logger.LogError(e.Exception, "OnError - An error occurred: {Message}", e.Exception.Message);
    }

    private void OnCommunitySubscription(object sender, OnCommunitySubscriptionArgs e)
    {
      Console.WriteLine();
    }

    private async void OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
    {
      await _commandHandler.HandleCommand(e.Command);
    }
  }
}
