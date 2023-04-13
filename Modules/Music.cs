using CliWrap;
using Discord.Commands;
using Discord.Interactions;
using Discord;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using System.Diagnostics;
using YoutubeExplode.Videos.Streams;
using Discord.Audio;
using Discord.WebSocket;

namespace ACTECH.Modules
{
    internal class Music: ModuleBase
    {
		string[] q_url = new string[100];
		int max_len = 100;
		int q_start = 0;
		int q_end = 0;
		int q_count = 0;
		bool is_playing = false;
		AudioOutStream pcm;
		public IUser user;
		public IUser bot_user;
		public IAudioClient audio_client;
		public ISocketMessageChannel msg_channel;

		int SizeCheckQ()
		{
			if (q_end == -1)
				return 0;

			q_count = q_end - q_start;
			if (q_end < q_start)
				q_count = q_url.Length - q_start + q_end;

			return q_count;
		}

		async Task Enqueue(string val)
		{
			q_count = SizeCheckQ();

			if (q_count == max_len)
			{
				await msg_channel.SendMessageAsync("Queue full.");
				return;
			}

			q_url[q_end] = val;
			q_end++;

			if (q_end == max_len)
				q_end = 0;

			q_count = SizeCheckQ();
			await msg_channel.SendMessageAsync("Added to queue.");
		}

		async Task<string> Dequeue()
		{
			q_count = SizeCheckQ();

			if (q_count == 0)
			{
				await msg_channel.SendMessageAsync("Queue empty.");
				return "-1";
			}

			var url = q_url[q_start];
			q_start++;

			if (q_start == max_len)
				q_start = 0;

			q_count = SizeCheckQ();
			return url;
		}

        public async Task VideoHandlerAsync(string query)
		{
			if (q_count == max_len)
			{
				await msg_channel.SendMessageAsync("Queue full.");
				return;
			}

			var url = await YtSearchAsync(query);

			if (url == "")
			{
				await msg_channel.SendMessageAsync("No results found.");
				return;
			}

			await msg_channel.SendMessageAsync(url);
			
			if (is_playing)
			{
				await Enqueue(url);
				return;
			}

			var task = Task.Run( async () =>
			{
				await Enqueue(url);
			});
			task.Wait();

			await PlayAsync();
		}

		private async Task<string> YtSearchAsync(string query)
		{
			var search = new YouTubeService(new BaseClientService.Initializer()
			{
				ApiKey = "API_KEY"
			});
			var searchRequest = search.Search.List("snippet");
			searchRequest.Q = query;
			searchRequest.Type = "video";
			searchRequest.MaxResults = 1;

			var searchResponse = await searchRequest.ExecuteAsync();
			if (searchResponse.Items.Count == 0)
			{
				return "";
			}
			var url = "https://www.youtube.com/watch?v=" + searchResponse.Items.Single().Id.VideoId.ToString();
			return url;
		}

		public async Task PlayAsync()
		{
			while (q_count > 0)
			{
				var video = await Dequeue();
				if (video == "-1")
					return;

				is_playing = true;
				var ytClient = new YoutubeClient();
				var streamManifest = await ytClient.Videos.Streams.GetManifestAsync(video);
				var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
				var stream = await ytClient.Videos.Streams.GetAsync(streamInfo);

				var memoryStream = new MemoryStream();
				await Cli.Wrap("ffmpeg")
					.WithArguments(" -hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1")
					.WithStandardInputPipe(PipeSource.FromStream(stream))
					.WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
					.ExecuteAsync();

				using (pcm = audio_client.CreatePCMStream(AudioApplication.Mixed))
				{
					try { await pcm.WriteAsync(memoryStream.ToArray().AsMemory(0, (int)memoryStream.Length)); }
					finally 
					{
						var task = Task.Run( async() => 
						{
							await pcm.FlushAsync();
						});
						task.Wait();
						is_playing = false;
					}
				}
			}
		}

		public async Task SkipAsync()
		{
			pcm.Dispose();
			is_playing = false;
			await PlayAsync();
		}

		public async Task LeaveVCAsync()
		{
			try { var check_channel = (bot_user as IVoiceState).VoiceChannel; }
			catch (Exception ex) { Console.WriteLine(ex.ToString()); return; }

			var channel = (bot_user as IVoiceState).VoiceChannel;

			if (channel == null)
			{
				await msg_channel.SendMessageAsync("Not in a voice channel.");
				return;
			}

			await ClearQueueAsync();
			await channel.DisconnectAsync();
			is_playing = false;
			await msg_channel.SendMessageAsync("Bye!");
		}

		public async Task ClearQueueAsync()
		{
			q_url = new string[100];
			q_start = 0;
			q_end = 0;
			q_count = 0;
		}
    }
}
