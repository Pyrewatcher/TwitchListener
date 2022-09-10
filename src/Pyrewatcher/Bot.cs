using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Handlers;
using System.Threading;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;

namespace Pyrewatcher
{
  public class Bot
  {
    private readonly TwitchClient _client;
    private readonly IConfiguration _config;
    private readonly ILogger<Bot> _logger;

    private readonly IBroadcastersRepository _broadcastersRepository;
    private readonly ILocalizationRepository _localizationRepository;

    private readonly ActionHandler _actionHandler;
    private readonly CommandHandler _commandHandler;
    private readonly CyclicTasksHandler _cyclicTasksHandler;

    public Bot(TwitchClient client, IConfiguration config, ILogger<Bot> logger, IBroadcastersRepository broadcastersRepository,
               ILocalizationRepository localizationRepository, ActionHandler actionHandler, CommandHandler commandHandler,
               CyclicTasksHandler cyclicTasksHandler)
    {
      _client = client;
      _config = config;
      _logger = logger;
      _broadcastersRepository = broadcastersRepository;
      _localizationRepository = localizationRepository;
      _actionHandler = actionHandler;
      _commandHandler = commandHandler;
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

      var channels = (await _broadcastersRepository.GetConnectedAsync()).Select(x => x.Name).ToList();
      Globals.Locale = await _localizationRepository.GetLocalizationByCodeAsync(Globals.LocaleCode);

      var credentials = new ConnectionCredentials(_config.GetSection("Twitch")["Username"], _config.GetSection("Twitch")["IrcToken"],
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
      Thread.Sleep(5000);
      Environment.Exit(1);
    }

    private void OnConnectionError(object sender, OnConnectionErrorArgs e)
    {
      _logger.LogError("OnConnectionError - An error occurred: {Message}", e.Error.Message);
      Thread.Sleep(5000);
      Environment.Exit(1);
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
      Thread.Sleep(5000);
      Environment.Exit(1);
    }

    private void OnError(object sender, OnErrorEventArgs e)
    {
      _logger.LogError(e.Exception, "OnError - An error occurred: {Message}", e.Exception.Message);
      Thread.Sleep(5000);
      Environment.Exit(1);
    }

    private void OnCommunitySubscription(object sender, OnCommunitySubscriptionArgs e)
    {
      Console.WriteLine();
    }

    private async void OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
    {
      try
      {
        await _commandHandler.HandleCommand(e.Command);
      }
      catch (Exception exception)
      {
        _logger.LogError(exception, "An error occurred while executing the command");
      }
    }
  }
}
