using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Input;
using ACTECH.Modules;
using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordInteractivity.Core;
using DiscordInteractivity.Enums;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ACTECH.Services
{
	public class Handler: DiscordClientService
	{
		private readonly IHost _host;
		private readonly IServiceProvider _provider;
		private readonly DiscordSocketClient _client;
		private readonly CommandService _service;
		private readonly InteractionService _intr;
		private readonly YouTubeService _yt;
		private readonly IConfiguration _config;

		
		public Handler(IHost host, DiscordSocketClient client, ILogger<DiscordClientService> logger, IServiceProvider provider, CommandService service, InteractionService intr, YouTubeService yt, IConfiguration config) : base(client, logger)
		{
			_host = host;
			_provider = provider;
			_client = client;
			_service = service;
			_intr = intr;
			_yt = yt;
			_config = config;
		}

		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			using IServiceScope serviceScope = _host.Services.CreateScope();
			IServiceProvider provider = serviceScope.ServiceProvider;

			var _client = provider.GetRequiredService<DiscordSocketClient>();
			var sCommands = provider.GetRequiredService<InteractionService>();

			_intr.InteractionExecuted += OnInteractionExecuted;
			_client.MessageReceived += OnMessageRecieved;
			//_client.SlashCommandExecuted += SlashCommandHandler;
			_service.CommandExecuted += OnCommandExecuted;
			await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);

			await _client.SetGameAsync("t!help");

			_client.Ready += async () =>
			{
				ulong guildId = 1067468516405551104;
				await sCommands.RegisterCommandsToGuildAsync(guildId);
				Console.WriteLine("Ready!");
			};
		}

		private async Task OnInteractionExecuted(ICommandInfo info, IInteractionContext context, Discord.Interactions.IResult result)
		{
			if (result.IsSuccess) return;
			await context.Channel.SendMessageAsync(result.ErrorReason);
		}

		//private async Task SlashCommandHandler(SocketSlashCommand command)
		//{
		//	General general = new General();

		//	var teamID = (string)command.Data.Options.FirstOrDefault(x => x.Name == "team_ID").Value;
		//	Console.WriteLine(teamID);
		//	var _task = Task.Run(async () =>
		//	{
		//		await general.TeamAsync(teamID);
		//	});
		//	Task.WaitAll(_task);
		//	var testguild = _client.GetGuild(1067918410194878535);
		//	var guild = _client.GetGuild(1067468516405551104);
		//	// actech server
		//	//var guild = _client.GetGuild(1064831455454314537);
		//	//var testrole = testguild.Roles.FirstOrDefault(x => x.Name == ("TECH-" + teamID));
		//	//var testUsers = command.Data.Options.TakeWhile(x => x.Name != "team_ID");
		//	//foreach (var member in testUsers)
		//	//{
		//	//	var user = (SocketGuildUser)member.Value;
		//	//	user.AddRoleAsync(testrole);
		//	//}
		//	var role = guild.Roles.FirstOrDefault(x => x.Name == ("TECH-" + teamID));
		//	var guildUsers = command.Data.Options.TakeWhile(x => x.Name != "team_ID");
		//	foreach (var member in guildUsers)
		//	{
		//		var user = (SocketGuildUser)member.Value;
		//		user.AddRoleAsync(role);
		//	}

		//	await command.RespondAsync("Done!");
		//}

		
		private async Task OnCommandExecuted(Optional<CommandInfo> info, ICommandContext context, Discord.Commands.IResult result)
		{
			if (result.IsSuccess) return;
			await context.Channel.SendMessageAsync(result.ErrorReason);
		}

		private async Task OnMessageRecieved(SocketMessage socketMessage)
		{
			if (!(socketMessage is SocketUserMessage message)) return;
			if (message.Author.IsBot) return;

			int argPos = 0;
			if (!message.HasStringPrefix(_config["Prefix"], ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

			var context = new SocketCommandContext(_client, message);
			await _service.ExecuteAsync(context, argPos, _provider);
		}
	}
}
