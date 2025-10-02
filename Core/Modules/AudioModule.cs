using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Services;
using Victoria;
using Victoria.Rest;
using Victoria.Rest.Search;

namespace Lyuze.Core.Modules {

    public class AudioModule(LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode, AudioService audioService, ILoggingService logger) : InteractionModuleBase<SocketInteractionContext> {
        //private static readonly IEnumerable<int> Range = Enumerable.Range(1900, 2000);

        [SlashCommand("join", "Join Voice Channel")]
        public async Task JoinAsync() {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null) {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            try {
                await lavaNode.JoinAsync(voiceState.VoiceChannel);
                await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");

                audioService.TextChannels.TryAdd(Context.Guild.Id, Context.Channel.Id);
            } catch (Exception exception) {
                await ReplyAsync(exception.ToString());
            }
        }

        [SlashCommand("leave", "Makes Bot Leave Current Voice Channel")]
        public async Task LeaveAsync() {
            var voiceChannel = ((IVoiceState)Context.User).VoiceChannel;
            if (voiceChannel == null) {
                await ReplyAsync("Not sure which voice channel to disconnect from.");
                return;
            }

            try {
                await lavaNode.LeaveAsync(voiceChannel);
                await ReplyAsync($"I've left {voiceChannel.Name}!");
            } catch (Exception exception) {
                await ReplyAsync(exception.Message);
            }
        }

        [SlashCommand("play", "Play a song or playlist")]
        public async Task PlayAsync([Remainder] string searchQuery) {
            if (string.IsNullOrWhiteSpace(searchQuery)) {
                await RespondAsync("Please provide search terms or a URL.");
                return;
            }

            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (player == null) {
                var voiceState = Context.User as IVoiceState;
                if (voiceState?.VoiceChannel == null) {
                    await RespondAsync("You must be in a voice channel!");
                    return;
                }

                player = await lavaNode.JoinAsync(voiceState.VoiceChannel);
                audioService.TextChannels.TryAdd(Context.Guild.Id, Context.Channel.Id);
            }

            // Search
            var searchResponse = Uri.IsWellFormedUriString(searchQuery, UriKind.Absolute)
                ? await lavaNode.LoadTrackAsync(searchQuery)
                : await lavaNode.LoadTrackAsync($"ytsearch:{searchQuery}");

            if (searchResponse?.Tracks == null || searchResponse.Tracks.Count == 0) {
                await RespondAsync($"I couldn't find anything for `{searchQuery}`.");
                return;
            }

            if (searchResponse.Type == LoadType.Playlist) {
                foreach (var track in searchResponse.Tracks)
                    player.GetQueue().Enqueue(track);

                var firstTrack = player.GetQueue().Dequeue();
                await player.PlayAsync(lavaNode, firstTrack);

                await RespondAsync($"🎶 Now playing playlist **{searchResponse.Playlist.Name}** starting with **{firstTrack.Title}**");
            } else {
                var track = searchResponse.Tracks.First();
                if (player.Track == null)
                    await player.PlayAsync(lavaNode, track);
                else
                    player.GetQueue().Enqueue(track);

                await RespondAsync($"🎶 Added **{track.Title}** to the queue.");
            }
        }



        [SlashCommand("pause", "Pauses current Song"), RequirePlayer]
        public async Task PauseAsync() {
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (player == null || player.IsPaused && player.Track != null) {
                await ReplyAsync("I cannot pause when I'm not playing anything or even connected!");
                return;
            }

            try {
                await player.PauseAsync(lavaNode);
                await ReplyAsync($"Paused: {player.Track.Title}");
            } catch (Exception exception) {
                await ReplyAsync(exception.Message);
            }
        }

        [SlashCommand("resume", "Resumes Current Track"), RequirePlayer]
        public async Task ResumeAsync() {
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (!player.IsPaused && player.Track != null) {
                await ReplyAsync("I cannot resume when I'm not playing anything or even connected!");
                return;
            }

            try {
                await player.ResumeAsync(lavaNode, player.Track);
                await ReplyAsync($"Resumed: {player.Track.Title}");
            } catch (Exception exception) {
                await ReplyAsync(exception.Message);
            }
        }

        [SlashCommand("stop", "Stops Music Player"), RequirePlayer]
        public async Task StopAsync() {
            var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
            if (player == null || !player.State.IsConnected || player.Track == null) {
                await ReplyAsync("Woah, can't stop won't stop.");
                return;
            }

            try {
                await player.StopAsync(lavaNode, player.Track);
                await ReplyAsync("No longer playing anything.");
            } catch (Exception exception) {
                await ReplyAsync(exception.Message);
            }
        }

        [SlashCommand("skip", "Skips Current Song"), RequirePlayer]
        public async Task SkipAsync() {

            try {

                var player = await lavaNode.TryGetPlayerAsync(Context.Guild.Id);
                if (player == null || !player.State.IsConnected || player.Track == null) {
                    await ReplyAsync("Woaaah there, I can't skip when nothing is playing.");
                    return;
                }

                // User must be in a VC
                var userVc = (Context.User as IVoiceState)?.VoiceChannel as SocketVoiceChannel;
                if (userVc == null) {
                    await ReplyAsync("You must be in a voice channel to skip.");
                    return;
                }

                // Bot must be in a VC
                var botVc = Context.Guild.CurrentUser.VoiceChannel as SocketVoiceChannel;
                if (botVc == null) {
                    await ReplyAsync("I'm not connected to a voice channel.");
                    return;
                }

                // Must be same VC
                if (userVc.Id != botVc.Id) {
                    await ReplyAsync("You must be in the same voice channel as me to skip.");
                    return;
                }

                // ✅ Only *connected* users
                var connectedHumans = botVc.ConnectedUsers
                    .Where(u => !u.IsBot)
                    .ToArray();

                // Optional: debug who we see
                await logger.LogInformationAsync("skip-debug",
                    $"Connected humans: {string.Join(", ", connectedHumans.Select(u => $"{u.Username}#{u.Discriminator}"))}");

                // Auto-skip if you're the only human
                if (connectedHumans.Length <= 1) {
                    var (skipped, currentTrack) = await player.SkipAsync(lavaNode);
                    await ReplyAsync($"Skipped: {skipped.Title}\nNow Playing: {currentTrack?.Title ?? "Nothing"}");
                    return;
                }

                // Vote skip
                if (!audioService.VoteQueue.Add(Context.User.Id)) {
                    await ReplyAsync("You can't vote again.");
                    return;
                }

                var percentage = (audioService.VoteQueue.Count / (double)connectedHumans.Length) * 100;
                if (percentage < 85) {
                    await ReplyAsync($"You need more than 85% votes to skip this song. Current: {percentage:F2}%");
                    return;
                }

                var (sk, now) = await player.SkipAsync(lavaNode);
                await ReplyAsync($"Skipped: {sk.Title}\nNow Playing: {now?.Title ?? "Nothing"}");

            } catch (Exception ex) {
                await ReplyAsync($"Error while trying to skip: {ex.Message}");
            }


            }





    }

}

