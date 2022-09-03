using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pyrewatcher;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.DataAccess.Repositories;
using Pyrewatcher.Handlers;
using Pyrewatcher.Helpers;
using Pyrewatcher.Riot.Interfaces;
using Pyrewatcher.Riot.LeagueOfLegends.Interfaces;
using Pyrewatcher.Riot.LeagueOfLegends.Services;
using Pyrewatcher.Riot.Models;
using Pyrewatcher.Riot.Services;
using Pyrewatcher.Riot.TeamfightTactics.Interfaces;
using Pyrewatcher.Riot.TeamfightTactics.Services;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using TwitchLib.Client;

var host = Host.CreateDefaultBuilder(args)
                     .ConfigureServices((_, services) =>
                     {
                       services.AddSingleton<CommandHandler>();
                       services.AddSingleton<TemplateCommandHandler>();
                       services.AddSingleton<ActionHandler>();
                       services.AddSingleton<CyclicTasksHandler>();

                       services.AddSingleton<CommandHelpers>();
                       services.AddSingleton<TwitchApiHelper>();

                       services.AddSingleton<RiotRateLimiter>();
                       services.AddSingleton<IRiotClient, RiotClient>();
                       services.AddSingleton<ILeagueV4Client, LeagueV4Client>();
                       services.AddSingleton<IMatchV5Client, MatchV5Client>();
                       services.AddSingleton<ISpectatorV4Client, SpectatorV4Client>();
                       services.AddSingleton<ISummonerV4Client, SummonerV4Client>();
                       services.AddSingleton<ITftLeagueV1Client, TftLeagueV1Client>();
                       services.AddSingleton<ITftMatchV1Client, TftMatchV1Client>();
                       services.AddSingleton<ITftSummonerV1Client, TftSummonerV1Client>();

                       services.AddSingleton<TwitchClient>();

                       services.AddSingleton<ILoggerFactory>(x =>
                       {
                         var providerCollection = x.GetService<LoggerProviderCollection>();
                         var factory = new SerilogLoggerFactory(null, true, providerCollection);

                         foreach (var provider in x.GetServices<ILoggerProvider>())
                         {
                           factory.AddProvider(provider);
                         }

                         return factory;
                       });

                       foreach (var commandType in Globals.CommandTypes)
                       {
                         services.AddSingleton(commandType);
                       }

                       foreach (var actionType in Globals.ActionTypes)
                       {
                         services.AddSingleton(actionType);
                       }

                       services.AddTransient<IAdoZRepository, AdoZRepository>();
                       services.AddTransient<IAliasesRepository, AliasesRepository>();
                       services.AddTransient<IBansRepository, BansRepository>();
                       services.AddTransient<IBroadcastersRepository, BroadcastersRepository>();
                       services.AddTransient<ICommandsRepository, CommandsRepository>();
                       services.AddTransient<ICommandVariablesRepository, CommandVariablesRepository>();
                       services.AddTransient<ILocalizationRepository, LocalizationRepository>();
                       services.AddTransient<ILatestCommandExecutionsRepository, LatestCommandExecutionsRepository>();
                       services.AddTransient<ILolChampionsRepository, LolChampionsRepository>();
                       services.AddTransient<ILolMatchesRepository, LolMatchesRepository>();
                       services.AddTransient<ILolRunesRepository, LolRunesRepository>();
                       services.AddTransient<IRiotAccountsRepository, RiotAccountsRepository>();
                       services.AddTransient<ISubscriptionsRepository, SubscriptionsRepository>();
                       services.AddTransient<ITftMatchesRepository, TftMatchesRepository>();
                       services.AddTransient<IUsersRepository, UsersRepository>();

                       services.AddSingleton<Bot>();
                     })
                     .Build();

Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                                      .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                                      .WriteTo.Console()
                                      .CreateLogger();

try
{
  var bot = host.Services.GetService<Bot>();
  await bot.Setup();
  bot.Connect();

  await Task.Delay(-1);
}
catch (Exception ex)
{
  Log.Fatal(ex, "There was a major problem that crashed the application.");
}
finally
{
  Log.CloseAndFlush();
}