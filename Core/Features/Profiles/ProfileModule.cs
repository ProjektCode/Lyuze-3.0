using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lyuze.Core.Extensions;
using Lyuze.Core.Shared.Embeds;

namespace Lyuze.Core.Features.Profiles {

    public class ProfileModule(ProfileService profileService) : InteractionModuleBase<SocketInteractionContext> {
        private readonly ProfileService _profileService = profileService;

        [SlashCommand("profile", "View a user's profile")]
        public async Task ProfileCmd([Summary("user", "The user to view the profile of")] SocketGuildUser? user = null) {

            await DeferAsync();
            user ??= Context.User as SocketGuildUser;
            if (user == null) {
                await FollowupAsync("Could not find the specified user.", ephemeral: true);
                return;
            }
            var embed = await _profileService.GetProfileAsync(user, Context);
            await FollowupAsync(embed: embed);
            await Context.DelayDeleteOriginalAsync(30);

        }

        [SlashCommand("setprofilebg", "Set your profile background image URL")]
        public async Task SetProfileBgCmd([Summary("url", "The URL of the background image")] string url, SocketGuildUser? user = null) {

            await DeferAsync(ephemeral: true);
            user ??= Context.User as SocketGuildUser;
            if (user == null) {
                await FollowupAsync("Could not find the specified user.", ephemeral: true);
                return;
            }

            var embed = await _profileService.UpdateProfileBackgroundAsync(user, Context, url);
            await FollowupAsync(embed: embed);
            await Context.DelayDeleteOriginalAsync(15);

        }

        [SlashCommand("setprofileaboutme", "Set your profile 'About Me' section")]
        public async Task SetProfileAboutMeCmd([Summary("aboutme", "The text for your 'About Me' section")] string aboutMe, SocketGuildUser? user = null) {

            await DeferAsync(ephemeral: true);
            user ??= Context.User as SocketGuildUser;
            if (user == null) {
                await FollowupAsync("Could not find the specified user.", ephemeral: true);
                return;
            }

            var embed = await _profileService.UpdateProfileAboutMeAsync(user, Context, aboutMe);
            await FollowupAsync(embed: embed);
            await Context.DelayDeleteOriginalAsync(15);

        }

        [SlashCommand("setprofilestatus", "Set whether your profile is public or private")]
        public async Task SetProfileStatusCmd([Summary("ispublic", "Set to true to make your profile public, false for private")] bool isPublic, SocketGuildUser? user = null) {

            await DeferAsync(ephemeral: true);
            user ??= Context.User as SocketGuildUser;
            if (user == null) {
                await FollowupAsync("Could not find the specified user.", ephemeral: true);
                return;
            }
            var embed = await _profileService.UpdateProfilePublicAsync(user, Context, isPublic);
            await FollowupAsync(embed: embed);
            await Context.DelayDeleteOriginalAsync(15);

        }

        [SlashCommand("setplvlnotifications", "Set whether you get notified when you level up")]
        public async Task SetLevelNotifications([Summary("isNotify", "Set to true to make your level notification public, false for no notification")] bool isNotify, SocketGuildUser? user = null) {

            await DeferAsync(ephemeral: true);
            user ??= Context.User as SocketGuildUser;
            if (user == null) {
                await FollowupAsync("Could not find the specified user.", ephemeral: true);
                return;
            }
            var embed = await _profileService.UpdateProfilePublicAsync(user, Context, isNotify);
            await FollowupAsync(embed: embed);
            await Context.DelayDeleteOriginalAsync(15);

        }

    }
}
