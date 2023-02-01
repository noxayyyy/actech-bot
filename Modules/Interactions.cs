using Discord.WebSocket;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using Discord.Interactions;
using System.Security.Cryptography.X509Certificates;

namespace ACTECH.Modules
{
    public class Interactions: InteractionModuleBase<SocketInteractionContext>
    {
		//[SlashCommand("team-assign", "Assign team roles and create channels.")]
		//public async Task teamAssignAsync(string teamID, params SocketGuildUser[] members)
		//{
		//	General general = new General();
		//	var _task = Task.Run(async () =>
		//	{
		//		await general.TeamAsync(teamID);
		//	});
		//	Task.WaitAll(_task);

		//	var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == ("TECH-" + teamID));
		//	foreach(SocketGuildUser member in members)
		//	{
		//		await member.AddRoleAsync(role);
		//	}
		//	await RespondAsync("Done!");
		//}
		//public async Task ModalAsync(string teamID)
		//{
		//	//var menuOptions = new List<SelectMenuOptionBuilder>();
		//	//await foreach(var member in members)
		//	//{
		//	//	var mem = member.FirstOrDefault().DisplayName;
		//	//	Console.WriteLine(mem);
		//	//	var option = new SelectMenuOptionBuilder()
		//	//		.WithLabel(mem)
		//	//		.WithDescription(null)
		//	//		.WithEmote(null)
		//	//		.WithValue(mem);
		//	//	menuOptions.Add(option);
		//	//}

		//	var task1 = Task.Run(() =>
		//	{
		//		var textBuild = new TextInputBuilder()
		//			.WithLabel("Team Member")
		//			.WithStyle(TextInputStyle.Short)
		//			.WithPlaceholder("Enter Discord Usernames.");
		//		return textBuild;
		//	});
		//	Task.WaitAll(task1);
		//	var awaiter1 = task1.GetAwaiter();
		//	var result1 = awaiter1.GetResult();
			
		//	var task2 = Task.Run(() =>
		//	{
		//		var modalBuild = new ModalBuilder()
		//			.WithTitle("Team Assignment")
		//			.WithCustomId("modal_build")
		//			.AddTextInput(result1.WithCustomId("text_build_1"))
		//			.AddTextInput(result1.WithCustomId("text_build_2"))
		//			.AddTextInput(result1.WithCustomId("text_build_3"))
		//			.AddTextInput(result1.WithCustomId("text_build_4"));

		//		return modalBuild;
		//	});
		//	Task.WaitAll(task2);
		//	var awaiter2 = task2.GetAwaiter();
		//	var result2 = awaiter2.GetResult();

		//	//var menu = new SelectMenuBuilder()
		//	//	.WithPlaceholder("Select Members to Assign Role")
		//	//	.WithCustomId("menu")
		//	//	.WithMaxValues(7)
		//	//	.WithOptions(menuOptions);

		//	//var taskMenu = Task.Run(async () =>
		//	//{
		//	//	var builder = new ComponentBuilder()
		//	//		.WithSelectMenu(menu);
		//	//	await ReplyAsync("Select Team Members", components: builder.Build());
		//	//	Context.Client.SelectMenuExecuted += MenuHandler;
		//	//	await ReplyAsync("Say confirm.");
		//	//	var timeOut = new TimeSpan(60);
		//	//	await WaitForMessageAsync(Context.User, true, timeOut);
		//	//});
		//	//Task.WaitAll(taskMenu);
			
		//	await Context.Interaction.RespondWithModalAsync(result2.Build());

		//	Context.Client.ModalSubmitted += async modal =>
		//	{
		//		int i = -1;
		//		List<SocketMessageComponentData> components = modal.Data.Components.ToList();
		//		foreach(SocketMessageComponentData component in components)
		//		{
		//			if(component.Value == null)
		//				break;
		//			i++;
		//			General.MEMBERS[i] = component.Value;
		//		}
		//	};
		//}
    }
}
