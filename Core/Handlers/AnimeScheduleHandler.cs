using Discord;
using Discord.WebSocket;
using JikanDotNet;
using Lyuze.Core.Models.Jikan;
using Newtonsoft.Json;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Lyuze.Core.Handlers {
    public class AnimeScheduleHandler {

        private readonly Jikan _jikan;
        private readonly Timer _timer;
        private readonly string _filePath = "tracked_anime.json";
        private readonly DiscordSocketClient _client;

        public AnimeScheduleHandler(DiscordSocketClient client) { 
            _client = client;
            _jikan = new Jikan();

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var jikanDirectory = Path.Combine(basePath, "Resources", "Jikan");
            Directory.CreateDirectory(jikanDirectory); // Ensure the directory exists
            _filePath = Path.Combine(jikanDirectory, "tracked_anime.json");

            _timer = new Timer(3600000);
            _timer.Elapsed += CheckAnimeScheduleAsync;
            _timer.Start();
        }

        public async Task AddAnimeToTrackAsync(int malID) {

            var anime = await _jikan.GetAnimeAsync(malID);

            var trackedAnime = new TrackedAnime {
                MalId = malID,
                Title = anime.Data.Titles.FirstOrDefault().Title,
                ImageUrl = anime.Data.Images.JPG.LargeImageUrl,
                LastEpisode = 0
            };

            List<TrackedAnime> trackedAnimes = LoadTrackedAnimes();
            trackedAnimes.Add(trackedAnime);
            SaveTrackedAnimes(trackedAnimes);

        }

        private List<TrackedAnime> LoadTrackedAnimes() {
            if (!File.Exists(_filePath)) {
                return new List<TrackedAnime>();
            }

            var json = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<List<TrackedAnime>>(json);
        }

        private void SaveTrackedAnimes(List<TrackedAnime> trackedAnimes) {
            var json = JsonConvert.SerializeObject(trackedAnimes, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

    private async void CheckAnimeScheduleAsync(object sender, ElapsedEventArgs e) {
            var trackedAnimes = LoadTrackedAnimes();

            foreach (var anime in trackedAnimes) {
                var episodes = await _jikan.GetAnimeEpisodesAsync(anime.MalId);
                var latestEpisode = episodes.Data.OrderByDescending(ep => ep.MalId).FirstOrDefault();

                if (latestEpisode != null && latestEpisode.MalId > anime.LastEpisode) {
                    // Update the last episode number
                    anime.LastEpisode = latestEpisode.MalId;
                    SaveTrackedAnimes(trackedAnimes);

                    // Send an embed notification
                    await SendNotificationAsync(anime, latestEpisode);
                }
            }
        }

        private async Task SendNotificationAsync(TrackedAnime anime, AnimeEpisode episode) {

            var guild = _client.GetGuild(SettingsHandler.Instance.Discord.GuildId);

            var embed = new EmbedBuilder()
                .WithTitle($"{anime.Title} - Episode {episode.MalId}")
                .WithImageUrl(anime.ImageUrl)
                .WithDescription($"Episode {episode.MalId} has been released!")
                .WithColor(Color.Blue)
                .Build();

            var channel = await _client.GetChannelAsync(1343835035584954368) as IMessageChannel;// Retrieve your specific Discord channel here
            await channel.SendMessageAsync(embed: embed);
        }

    }
}
