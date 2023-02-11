using Discord;
using Discord.Commands;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using ACTECH.Services;
using Discord.Addons.Interactive;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DiscordInteractivity.Core;
using Discord.Interactions;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using System.Runtime.CompilerServices;

namespace ACTECH
{ 
	public class Program
	{
		static async Task Main()
		{
			var builder = new HostBuilder()
				.ConfigureAppConfiguration(x =>
				{
					 var configuration = new ConfigurationBuilder()
						.SetBasePath(Directory.GetCurrentDirectory())
						.AddJsonFile("appsettings.json", false, true)
						.Build();

					 x.AddConfiguration(configuration);
				})
				.ConfigureLogging(x =>
				{
					x.AddConsole();
					x.SetMinimumLevel(LogLevel.Debug);
				})
				.ConfigureDiscordHost((context, config) =>
				{
					config.SocketConfig = new DiscordSocketConfig
					{
						LogLevel = LogSeverity.Debug,
						AlwaysDownloadUsers = true,
						MessageCacheSize = 200,
						GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent | GatewayIntents.GuildMembers
					};

					config.Token = context.Configuration["Token"];
				})
				.UseCommandService((context, config) =>
				{
					config.CaseSensitiveCommands = false;
					config.LogLevel = LogSeverity.Debug;
					config.DefaultRunMode = Discord.Commands.RunMode.Async;
				})
				.UseInteractionService((context, config) =>
				{
					config.LogLevel = LogSeverity.Debug;
					config.DefaultRunMode = Discord.Interactions.RunMode.Async;
				})
				.ConfigureServices((context, services) =>
				{
					services
						.AddHostedService<Handler>()
						.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
						.AddSingleton<InteractivityService>()
						.AddSingleton(new YouTubeService(new BaseClientService.Initializer()
						{
							ApiKey = "YT_API_KEY",
							ApplicationName = "APPLICATION_NAME"
						}));
				})
				.UseConsoleLifetime();

			var host = builder.Build();
			using (host)
			{
				await host.RunAsync();
			}
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
	}
}
