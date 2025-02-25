using Discord;
using Discord.Interactions;
using Lyuze.Core.Handlers;
using System.Threading.Tasks;

namespace Lyuze.Core.Modules {
    public class JikanModule(AnimeScheduleHandler animeScheduleHandler) : InteractionModuleBase<SocketInteractionContext> {
        private readonly AnimeScheduleHandler _animeScheduleHandler = animeScheduleHandler;

        [SlashCommand("track_anime", "Track a new anime by MAL ID.")]
        public async Task TrackAnime([Summary("Uses the MyAnimeList Anime ID to track, it is the numbers you see in the url. ex) 58437")]int malId) {
            try {

                await _animeScheduleHandler.AddAnimeToTrackAsync(malId);
                await RespondAsync("Anime has been added to the tracking list.", ephemeral: true);

            } catch (Exception ex) { 
                Console.WriteLine(ex.ToString());
                await RespondAsync("An error has occured.", ephemeral: true);
            }

        }
    }
}

