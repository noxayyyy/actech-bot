using AngleSharp.Dom;
using AngleSharp.Text;
using CliWrap;
using Discord;
using Discord.Audio;
using Discord.Audio.Streams;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using VideoLibrary;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using YouTubeSearch;

namespace ACTECH.Modules
{
	public class General: ModuleBase<SocketCommandContext>
	{
		static readonly string TEAM_PREFIX = "TECH-";
		static readonly ulong UNVERIFIED_ID = 1067293985485357096;
		static readonly ulong VERIFIED_ID = 1067473165036560395;
		static ulong ROLE_ID;
		static ulong CATEGORY_ID;
		static ulong EVERYONE_ID;
		static readonly ulong ACTECH_ID = 1064831455454314537;
		static LinkedList<string> URL_QUEUE = new LinkedList<string>();
		static Music music = new Music();

		[Command("ping")]
		public async Task PingAsync()
		{
			//await ReplyAsync("Pong!");
			await Context.Channel.TriggerTypingAsync();
			await ReplyAsync("Pong!");
		}
		
		[Command("help")]
		public async Task HelpAsync()
		{
			await ReplyAsync("Check my description.");
		}

		[Command("info")]
		[EnabledInDm(false)]
		public async Task InfoAsync(SocketGuildUser user = null)
		{
			if (user == null)
				user = Context.User as SocketGuildUser;

			await ReplyAsync($"ID: {user.Id}\n" +
				$"Name: {user.Username}#{user.Discriminator}\n" +
				$"Created at: {user.CreatedAt}");
		}

		[Command("spam")]
		[EnabledInDm(false)]
		[DefaultMemberPermissions(GuildPermission.Administrator)]
		public async Task SpamPing(SocketGuildUser user, int count)
		{
			if (user == null || count == 0)
			{
				await ReplyAsync("Invalid use. Try again.");
				return;
			}

			for (int i = 0; i < count; i++)
			{
				await ReplyAsync(user.Mention + " Get pinged asshole");
			}
		}
		[Command("maketeam")]
		[EnabledInDm(false)]
		[DefaultMemberPermissions(GuildPermission.Administrator)]
		public async Task RolesAsync(string teamID, params SocketGuildUser[] members)
		{
			bool repeatingUsers = false;
			bool invalidID = false;
			bool teamExists = false;
			int teamCount = members.Length;

			for (int i = 0; i < teamCount; i++)
			{
				for (int j = 0; j < teamCount; j++)
				{
					if (i == j)
						continue;
					if (members[i] == members[j])
					{
						repeatingUsers = true;
						break;
					}
				}
				if (repeatingUsers) { break; }
			}
			try
			{
				var teamRole = Context.Guild.Roles.First(x => x.Name == (TEAM_PREFIX + teamID));
				teamExists = true;
			}
			catch(Exception ex) { Console.WriteLine(ex.ToString()); }

			foreach(char i in teamID)
			{
				if (i > '9' || i < '0')
				{
					invalidID = true;
					break;
				}
			}

			var typingState = Context.Channel.EnterTypingState();
			if (teamID.Length != 4 || invalidID)
			{
				await ReplyAsync("Invalid Team ID, try again.");
				typingState.Dispose();
				return;
			}
			else if (teamExists)
			{
				await ReplyAsync("Team already exists, try again.");
				typingState.Dispose();
				return;
			}
			else if (teamCount < 3)
			{
				await ReplyAsync("Choose at least `3` members, try again.");
				typingState.Dispose();
				return;
			}
			else if (members.Length > 7)
			{
				await ReplyAsync("A team can have a max number of `7` members.");
				typingState.Dispose();
				return;
			}
			else if (repeatingUsers)
			{
				await ReplyAsync("Entered same user more than once, try again.");
				typingState.Dispose();
				return;
			}

			var msg = await ReplyAsync("Working on it...");

			var _task = Task.Run(async () =>
			{
				await msg.ModifyAsync(x => x.Content += "\n Making roles...");
				await MakeRoleAsync(teamID);
				await MakeTextAsync(teamID);
				await msg.ModifyAsync(x => x.Content += "\n Making channels...");
			});
			var _awaiter = _task.GetAwaiter();
			
			_awaiter.OnCompleted(async () =>
			{
				await Context.Channel.TriggerTypingAsync();
				var _subtask = Task.Run(async () =>
				{
					await TextPermsAsync(teamID);
					await MakeVCAsync(teamID);
				});
				var _subawaiter = _subtask.GetAwaiter();

				_subawaiter.OnCompleted(async () =>
				{
					typingState.Dispose();
					await msg.ModifyAsync(x => x.Content = "Done!");
				});

			});
			_task.Wait();

			var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == (TEAM_PREFIX + teamID));
			foreach (var member in members)
			{
				await member.AddRoleAsync(role);
			}
		}

		private async Task MakeTextAsync(string teamID)
		{
			// create category
			var categoryName = TEAM_PREFIX + teamID;
			var task1 = Task.Run(async () =>
			{
				await Context.Guild.CreateCategoryChannelAsync(categoryName);
			});
			task1.Wait();
			CATEGORY_ID = Context.Guild.CategoryChannels.FirstOrDefault(x => x.Name == categoryName).Id;

			// create text channel within category
			var channelName = "text-" + teamID;
			task1 = Task.Run(async () =>
			{
				await Context.Guild.CreateTextChannelAsync(channelName, c => c.CategoryId = CATEGORY_ID);
			});
			task1.Wait();
		}

		private async Task MakeRoleAsync(string teamID)
		{
			var everyoneperms = Context.Guild.EveryoneRole.Permissions;
			var roleName = TEAM_PREFIX + teamID;
			var task1 = Task.Run(async () =>
			{
				await Context.Guild.CreateRoleAsync(roleName, everyoneperms, Color.Purple);
			});
			task1.Wait();
			ROLE_ID = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName).Id;
			EVERYONE_ID = Context.Guild.Roles.FirstOrDefault(x => x.IsEveryone).Id;
		}

		private async Task TextPermsAsync(string teamID)
		{
			bool actech = false;
			IRole staff = null;
			var channelName = "text-" + teamID;
			var channel = Context.Guild.Channels.FirstOrDefault(x => x.Name == channelName);

			// set viewChannel permission presets
			var denyPerms = new OverwritePermissions(viewChannel: PermValue.Deny);
			var allowPerms = new OverwritePermissions(viewChannel: PermValue.Allow);
			var everyoneRole = Context.Guild.GetRole(EVERYONE_ID);

			// actech
			if (Context.Guild.Id == ACTECH_ID) 
			{
				actech = true;
				staff = Context.Guild.GetRole(1067294831740731452);
			}

			var role = Context.Guild.GetRole(ROLE_ID);
			await channel.AddPermissionOverwriteAsync(everyoneRole, denyPerms);
			await channel.AddPermissionOverwriteAsync(role, allowPerms);
			if (actech) { await channel.AddPermissionOverwriteAsync(staff, allowPerms); }
		}

		private async Task MakeVCAsync(string teamID)
		{
			bool actech = false;
			IRole staff = null;
			string[] vcNames = new string[6];
			vcNames[0] = "Arcadic";
			vcNames[1] = "Cerebral";
			vcNames[2] = "Cauldron";
			vcNames[3] = "Human";
			vcNames[4] = "Mayhem";
			vcNames[5] = "Webbers";

			var role = Context.Guild.GetRole(ROLE_ID);
			var everyoneRole = Context.Guild.GetRole(EVERYONE_ID);
			var denyPerms = new OverwritePermissions(viewChannel: PermValue.Deny);
			var allowPerms = new OverwritePermissions(viewChannel: PermValue.Allow);
			if (Context.Guild.Id == ACTECH_ID)
			{
				actech = true;
				staff = Context.Guild.GetRole(1067294831740731452);
			}

			// create voice channels within category
			foreach(string name in vcNames)
			{
				var channelName = teamID + "-" + name;
				var task1 = Task.Run(async () =>
				{
					await Context.Guild.CreateVoiceChannelAsync(channelName, c => c.CategoryId = CATEGORY_ID);
				});
				task1.Wait();
				var channel = Context.Guild.Channels.FirstOrDefault(x => x.Name == channelName);
				await channel.AddPermissionOverwriteAsync(everyoneRole, denyPerms);
				await channel.AddPermissionOverwriteAsync(role, allowPerms);
				if (actech) 
					await channel.AddPermissionOverwriteAsync(staff, allowPerms);
			}
		}

		//[Command("removeteam")]
		//[EnabledInDm(false)]
		//[DefaultMemberPermissions(GuildPermission.Administrator)]
		//public async Task RemoveTeamAsync(string teamID)
		//{
		//	var checkRole = Context.Guild.Roles.Where(x => x.Name == (TEAM_PREFIX + teamID)).ToList();
		//	if (checkRole.Count == 0)
		//	{
		//		await ReplyAsync("No such team exists.");
		//		return;
		//	}

		//	var msg = await ReplyAsync("Working on it...");
		//	var typingState = Context.Channel.EnterTypingState();

		//	var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == (TEAM_PREFIX + teamID));
		//	var members = role.Members.ToList();

		//	var _task = Task.Run(async () =>
		//	{
		//		foreach(var member in members)
		//			await member.RemoveRoleAsync(role);
		//	});

		//	var channel = Context.Guild.Channels.FirstOrDefault(x => x.Name == ("text-" + teamID));
		//	await channel.DeleteAsync();

		//	await RemoveVCAsync(teamID);

		//	var category = Context.Guild.CategoryChannels.FirstOrDefault(x => x.Name == (TEAM_PREFIX + teamID));
		//	await category.DeleteAsync();

		//	_task.Wait();

		//	typingState.Dispose();
		//	await role.DeleteAsync();
		//	await msg.ModifyAsync(x => x.Content = "Done!");
		//}

		[Command("removeteam")]
		[EnabledInDm(false)]
		[DefaultMemberPermissions(GuildPermission.Administrator)]
		public async Task RemoveTeamAsync(params string[] teamIds)
		{
			await ReplyAsync("Working on it...");
			var typingState = Context.Channel.EnterTypingState();

			foreach(string teamID in teamIds)
			{
				var checkRole = Context.Guild.Roles.Where(x => x.Name == (TEAM_PREFIX + teamID)).ToList();
				if (checkRole.Count == 0)
				{
					await ReplyAsync("`TECH-" + teamID + "`: no such team exists.");
					continue;
				}
				
				var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == (TEAM_PREFIX + teamID));
				var members = role.Members.ToList();
				var _task = Task.Run(async () =>
				{
					foreach(var member in members)
						await member.RemoveRoleAsync(role);
				});
				
				var channel = Context.Guild.Channels.FirstOrDefault(x => x.Name == ("text-" + teamID));
				await channel.DeleteAsync();
				
				await RemoveVCAsync(teamID);

				var category = Context.Guild.CategoryChannels.FirstOrDefault(x => x.Name == (TEAM_PREFIX + teamID));
				await category.DeleteAsync();

				_task.Wait();
				await role.DeleteAsync();
			}
			typingState.Dispose();
			await ReplyAsync("Done bulk removing!");
		}

		private async Task RemoveVCAsync(string teamID)
		{
			string[] vcNames = new string[6];
			vcNames[0] = "Arcadic";
			vcNames[1] = "Cerebral";
			vcNames[2] = "Cauldron";
			vcNames[3] = "Human";
			vcNames[4] = "Mayhem";
			vcNames[5] = "Webbers";
			
			foreach(string name in vcNames)
			{
				var channelName = teamID + "-" + name;
				var voiceChannel = Context.Guild.Channels.FirstOrDefault(x => x.Name == channelName);
				await voiceChannel.DeleteAsync();
			}
		}

		[Command("verify")]
		[EnabledInDm(false)]
		[DefaultMemberPermissions(GuildPermission.ManageRoles)]
		public async Task VerifyAsync(params SocketGuildUser[] users)
		{
			IRole unverifiedRole, verifiedRole;
			try 
			{
				unverifiedRole = Context.Guild.GetRole(UNVERIFIED_ID);
				verifiedRole = Context.Guild.GetRole(VERIFIED_ID);
			}
			catch(Exception ex)
			{
				await ReplyAsync(ex.ToString());
				return;
			}
			foreach(SocketGuildUser user in users)
			{
				if (user.Roles.Contains(unverifiedRole))
					await user.RemoveRoleAsync(unverifiedRole);

				if (!user.Roles.Contains(verifiedRole))
					await user.AddRoleAsync(verifiedRole);
			}
			await ReplyAsync("Verified!");
		}

		[Command("renameteam")]
		[EnabledInDm(false)]
		[DefaultMemberPermissions(GuildPermission.Administrator)]
		public async Task RenameTeamAsync(string teamID, string renameID)
		{
			var checkRole = Context.Guild.Roles.Where(x => x.Name == (TEAM_PREFIX + teamID)).ToList();
			if (checkRole.Count == 0)
			{
				await ReplyAsync("No such team exists.");
				return;
			}
			
			var msg = await ReplyAsync("Working on it...");
			var typingState = Context.Channel.EnterTypingState();

			var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == (TEAM_PREFIX + teamID));
			await role.ModifyAsync(x => x.Name = TEAM_PREFIX + renameID);

			var channel = Context.Guild.Channels.FirstOrDefault(x => x.Name == ("text-" + teamID));
			await channel.ModifyAsync(x => x.Name = "text-" + renameID);

			await RenameVCAsync(teamID, renameID);

			var category = Context.Guild.CategoryChannels.FirstOrDefault(x => x.Name == (TEAM_PREFIX + teamID));
			await category.ModifyAsync(x => x.Name = TEAM_PREFIX + renameID);

			typingState.Dispose();
			await msg.ModifyAsync(x => x.Content = "Done!");
		}

		private async Task RenameVCAsync(string teamID, string renameID)
		{
			string[] vcNames = new string[6];
			vcNames[0] = "Arcadic";
			vcNames[1] = "Cerebral";
			vcNames[2] = "Cauldron";
			vcNames[3] = "Human";
			vcNames[4] = "Mayhem";
			vcNames[5] = "Webbers";
			
			foreach(string name in vcNames)
			{
				var channelName = teamID + "-" + name;
				var voiceChannel = Context.Guild.Channels.FirstOrDefault(x => x.Name == channelName);
				await voiceChannel.ModifyAsync(x => x.Name = renameID + "-" + name);
			}
		}

		[Command("play", RunMode = Discord.Commands.RunMode.Async)]
		[Alias("p")]
		[EnabledInDm(false)]
		public async Task PlayAsync([Remainder] string query)
		{
			var channel = (Context.User as IVoiceState).VoiceChannel;
			if (channel == null)
			{
				await ReplyAsync("User must be in a voice channel first.");
				return;
			}

			if ((Context.Guild.CurrentUser as IVoiceState).VoiceChannel != channel)
				music.audio_client = await channel.ConnectAsync();

			music.user = Context.User;
			music.bot_user = Context.Guild.CurrentUser;
			music.msg_channel = Context.Channel;
			await music.VideoHandlerAsync(query);
		}

		[Command("skip")]
		[Alias("s")]
		[EnabledInDm(false)]
		public async Task SkipAsync()
		{
			await music.SkipAsync();
			await ReplyAsync("Skipped!");
		}

		[Command("leave")]
		[Alias("die", "kys", "quit", "dc", "disconnect")]
		[EnabledInDm(false)]
		public async Task LeaveAsync()
		{
			music.msg_channel = Context.Channel;
			await music.LeaveVCAsync();
		}

		[Command("clear")]
		[EnabledInDm(false)]
		public async Task ClearQueueAsync()
		{
			await music.ClearQueueAsync();
			await ReplyAsync("Queue cleared!");
		}
		
		//[Command("redoteamperms")]
		//[EnabledInDm(false)]
		//[DefaultMemberPermissions(GuildPermission.Administrator)]
		//public async Task RedoTeamPermsAsync(params string[] teamIds)
		//{
		//	var everyoneperms = Context.Guild.EveryoneRole.Permissions;

		//	foreach(string teamID in teamIds) 
		//	{
		//		var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == TEAM_PREFIX + teamID);
		//		await role.ModifyAsync(x => x.Permissions = everyoneperms);
		//	}
		//}

		//[Command("removecategoryperms")]
		//[EnabledInDm(false)]
		//[DefaultMemberPermissions(GuildPermission.Administrator)]
		//public async Task RemoveCategoryPermsAsync(params string[] teamIds)
		//{
		//	string[] vcNames = new string[6];
		//	vcNames[0] = "Arcadic";
		//	vcNames[1] = "Cerebral";
		//	vcNames[2] = "Cauldron";
		//	vcNames[3] = "Human";
		//	vcNames[4] = "Mayhem";
		//	vcNames[5] = "Webbers";

		//	foreach(string teamID in teamIds) 
		//	{
		//		foreach(string name in vcNames)
		//		{
		//			var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == name);
		//			var channelName = teamID + "-" + name;
		//			var channel = Context.Guild.Channels.FirstOrDefault(x => x.Name == channelName);
		//			await channel.RemovePermissionOverwriteAsync(role);
		//		}
		//	}
		//}
		//[Command("deletecommands")]
		//public async Task DeleteAsync()
		//{
		//	await Context.Guild.DeleteApplicationCommandsAsync();
		//	await ReplyAsync("Deleted all application commands in current guild.");
		//}

		//[Command("buildcommands")]
		//private async Task buildSCommandsAsync()
		//{
		//	// test server
		//	var guild = Context.Guild;

		//	// actech server
		//	//var guild = _client.GetGuild(1064831455454314537);

		//	var options = new SlashCommandOptionBuilder[8];
		//	for (int i = 0; i < 8; i++)
		//	{
		//		if (i == 0)
		//		{
		//			options[i] = new SlashCommandOptionBuilder()
		//				.WithName("team_ID")
		//				.WithDescription("Enter Team ID.")
		//				.WithType(ApplicationCommandOptionType.String)
		//				.WithRequired(true);
		//		}
		//		if (i < 5)
		//		{
		//			options[i] = new SlashCommandOptionBuilder()
		//				.WithName("member_" + (i + 1))
		//				.WithDescription("Select Team Member.")
		//				.WithType(ApplicationCommandOptionType.User)
		//				.WithRequired(true);
		//		}
		//		else
		//		{
		//			options[i] = new SlashCommandOptionBuilder()
		//				.WithName("member_" + i)
		//				.WithDescription("Select Team Member.")
		//				.WithType(ApplicationCommandOptionType.User)
		//				.WithRequired(false);
		//		}
		//	}

		//	var guildCommand = new SlashCommandBuilder()
		//		.WithName("team-roles")
		//		.WithDescription("Get your team's roles.")
		//		.AddOptions(options);

		//	try
		//	{
		//		await Context.Client.Rest.CreateGuildCommand(guildCommand.Build(), guild.Id);
		//	}
		//	catch { return; }
		//}

		//[Command()]
		//public async Task RegisterAsync()
		//{

		//	ulong guildId = 1067468516405551104;
		//	await sCommands.RegisterCommandsToGuildAsync(guildId);
		//}

		//public async Task AssignAsync()
		//{
		//	await Context.Guild.CreateApplicationCommandAsync();
		//}

		//[Discord.Commands.Command("team", RunMode = Discord.Commands.RunMode.Async)]
		//[Discord.Commands.RequireBotPermission(GuildPermission.Administrator)]
		//public async Task teamsAsync(string teamID)
		//{
		//	Interactions intr = new Interactions();

		//	var task1 = Task.Run(async () =>
		//	{
		//		await TeamAsync(teamID);
		//	});
		//	Task.WaitAll(task1);

		//	var task2 = Task.Run(async () => 
		//	{
		//		await intr.ModalAsync(teamID);
		//	});
		//	Task.WaitAll(task2);

		//	var task3 = Task.Run(async () =>
		//	{
		//		await MenuHandler(General.MEMBERS);
		//	});
		//	Task.WaitAll(task3);

		//	await ReplyAsync("Done!");
		//}

		//private Process CreateStream(string path)
		//{
		//	return Process.Start(new ProcessStartInfo
		//	{
		//		FileName = "ffmpeg/bin/ffmpeg",
		//		Arguments = $"-hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
		//		UseShellExecute = false,
		//		RedirectStandardOutput = true,
		//	});
		//}

		//public async Task MenuHandler(string[] members)
		//{
		//	foreach(string value in members)
		//	{
		//		if(value == null)
		//			break;
		//		Console.WriteLine(value);
		//		var user = Context.Guild.Users.FirstOrDefault(x => x.DisplayName == value);
		//		var role = Context.Guild.GetRole(ROLE_ID);
		//		await user.AddRoleAsync(role);
		//	}
		//}

		//[Command("prefix")]
		//public async Task PrefixAsync()
		//{
		//    await ReplyAsync(config["Prefix"]);
		//}
	}
}
