using System.Collections.Concurrent;
using System.Text.Json;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Victoria;
using Victoria.WebSocket.EventArgs;
using Lyuze.Core.Handlers;

namespace Lyuze.Core.Services {
    public sealed class AudioService {
        private readonly LavaNode<LavaPlayer<LavaTrack>, LavaTrack> _lavaNode;
        private readonly DiscordSocketClient _socketClient;
        private readonly ILoggingService _logger;
        public readonly HashSet<ulong> VoteQueue;
        private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _disconnectTokens;
        public readonly ConcurrentDictionary<ulong, ulong> TextChannels;

        public AudioService(
            LavaNode<LavaPlayer<LavaTrack>, LavaTrack> lavaNode,
            DiscordSocketClient socketClient,
            ILoggingService logger) {
            _lavaNode = lavaNode;
            _socketClient = socketClient;
            _disconnectTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();
            _logger = logger;
            TextChannels = new ConcurrentDictionary<ulong, ulong>();
            VoteQueue = [];
            _lavaNode.OnWebSocketClosed += OnWebSocketClosedAsync;
            _lavaNode.OnPlayerUpdate += OnPlayerUpdateAsync;
            _lavaNode.OnTrackEnd += OnTrackEndAsync;
            _lavaNode.OnTrackStart += OnTrackStartAsync;
        }

        private Task OnTrackStartAsync(TrackStartEventArg arg) {
            return SendAndLogMessageAsync(arg.GuildId, $"Now playing: {arg.Track.Title}");
        }

        private async Task OnTrackEndAsync(TrackEndEventArg arg) {
            if (!TextChannels.TryGetValue(arg.GuildId, out var textChannelId))
                return;

            var player = await _lavaNode.TryGetPlayerAsync(arg.GuildId);
            if (player == null) return;

            if (player.GetQueue().TryDequeue(out var nextTrack)) {
                await player.PlayAsync(_lavaNode, nextTrack);
                await SendAndLogMessageAsync(arg.GuildId, $"Now playing: {nextTrack.Title}");
            } else {
                await SendAndLogMessageAsync(arg.GuildId, "Queue is empty. Leaving channel...");

                var guild = _socketClient.GetGuild(arg.GuildId);
                var botVoiceChannel = guild?.VoiceChannels
                    .FirstOrDefault(vc => vc.ConnectedUsers.Any(u => u.Id == _socketClient.CurrentUser.Id));

                if (botVoiceChannel != null)
                    await _lavaNode.LeaveAsync(botVoiceChannel);
            }
        }



        private async Task<Task> OnPlayerUpdateAsync(PlayerUpdateEventArg arg) {
            await _logger.LogInformationAsync("victoria", $"Guild latency: {arg.Ping}");

            var stats = await _lavaNode.GetLavalinkStatsAsync();
            var frameDeficit = stats.Frames.Deficit;
            var nulledFrames = stats.Frames.Nulled;

            if(frameDeficit > 0 || nulledFrames > 0) {
                await _logger.LogAsync("victoria", LogSeverity.Warning, $"Frame deficit: {frameDeficit}, Nulled frames: {nulledFrames}");
            }

            return Task.CompletedTask;
        }

        private async Task<Task> OnWebSocketClosedAsync(WebSocketClosedEventArg arg) {
            await _logger.LogCriticalAsync("victoria", $"{JsonSerializer.Serialize(arg)}");
            return Task.CompletedTask;
        }

        private async Task<Task> SendAndLogMessageAsync(ulong guildId,
                                            string message) {
            await _logger.LogInformationAsync("victoria", message);
            if (!TextChannels.TryGetValue(guildId, out var textChannelId)) {
                return Task.CompletedTask;
            }

            return (_socketClient
                    .GetGuild(guildId)
                    .GetChannel(textChannelId) as ITextChannel)
                .SendMessageAsync(message);
        }
    }
}