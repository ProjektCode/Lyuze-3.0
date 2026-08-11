using Discord;
using Discord.Interactions;
using Lyuze.Core.Abstractions.Interfaces;
using Lyuze.Core.Shared.Components;
using Lyuze.Core.Shared.Images;
using System.Linq;

namespace Lyuze.Core.Features.Utility.Services;
public class HelpService(InteractionService interactionService, IServiceProvider serviceProvider, ColorUtils colorUtils, ILoggingService loggingService) {
    private readonly InteractionService _interactionService = interactionService;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ColorUtils _colorUtils = colorUtils;
    private readonly ILoggingService _loggingService = loggingService;

    private async Task<IReadOnlyList<SlashCommandInfo>> GetVisibleCommandsAsync(IInteractionContext ctx) {

        var commands = new List<SlashCommandInfo>();

        foreach (var command in _interactionService.SlashCommands) {
            if (await CanSeeAsync(ctx, command)) commands.Add(command);
        }

        return commands;
    }


    private async Task<bool> CanSeeAsync(IInteractionContext ctx, SlashCommandInfo command) {
        
        var condition = await command.CheckPreconditionsAsync(ctx, _serviceProvider);

        return condition.IsSuccess;
    }


    private static CommandCategory GetCategory(SlashCommandInfo command) => ResolveCategory(command);

    private static IEnumerable<string> GetGroupNames(ModuleInfo? module) {

        while (module != null) {

           if(module.IsSlashGroup && !string.IsNullOrWhiteSpace(module.SlashGroupName)) yield return module.SlashGroupName;

            module = module.Parent;
        }
    }

    private static CommandCategory ResolveCategory(SlashCommandInfo command) {

        var module = command.Module;

        while (module != null) {
            if (module.IsSlashGroup) return new CommandCategory(Slugify(module.SlashGroupName), module.SlashGroupName, module.Description ?? $"{module.SlashGroupName}'s commands.");

            module = module.Parent;
        }

        var moduleName = command.Module?.Name ?? "Uncategorized";

        string displayName;

        if (moduleName.EndsWith("Module")) {
            displayName = moduleName.Substring(0, moduleName.Length - "Module".Length);
        } else {
            displayName = moduleName;
        }

        return new CommandCategory(Slugify(displayName), displayName, command.Module?.Description ?? $"{displayName}'s commands.");
    }

    private static string GetCommandPath(SlashCommandInfo command) {

        var groups = GetGroupNames(command.Module).Reverse();

        return string.Join(" ", groups.Append(command.Name));
    }


    private static string Slugify(string input) {
        var sb = new System.Text.StringBuilder(input.Length);

        foreach (var ch in input.Trim().ToLowerInvariant()) {
            if (char.IsLetterOrDigit(ch)) {
                sb.Append(ch);
            } else if (ch is ' ' or '-' or '_') {
                sb.Append('-');
            }
        }

        return sb.ToString().Trim('-');
    }

    public async Task<(Embed embed, MessageComponent? component)> RenderCategoryPickerAsync(IInteractionContext ctx, ulong invokerUserId) {
        try {

            var visibleCommands = await GetVisibleCommandsAsync(ctx);

            if(visibleCommands.Count == 0) {
                var errorEmbed = new EmbedBuilder {
                    Title = "Help Menu",
                    Description = "No commands available.",
                    Color = new Color(await _colorUtils.RandomColorFromUrlAsync(ctx.User.GetAvatarUrl() ?? ctx.User.GetDefaultAvatarUrl())),
                    Timestamp = DateTimeOffset.UtcNow,
                    ThumbnailUrl = ctx.User.GetAvatarUrl(ImageFormat.Auto, 256) ?? ctx.User.GetDefaultAvatarUrl() ?? ctx.User.GetDefaultAvatarUrl(),
                    Footer = new EmbedFooterBuilder {
                        Text = "Help Menu",
                        IconUrl = ctx.Guild?.IconUrl ?? ctx.User.GetDefaultAvatarUrl()
                    }
                };
                return (errorEmbed.Build(), null);
            }

            var visibleCategories = visibleCommands.Select(c => GetCategory(c)).DistinctBy(c => c.Key).ToList();

            if(visibleCategories.Count == 0) {
                var errorEmbed = new EmbedBuilder {
                    Title = "Help Menu",
                    Description = "No categories available.",
                    Color = new Color(await _colorUtils.RandomColorFromUrlAsync(ctx.User.GetAvatarUrl() ?? ctx.User.GetDefaultAvatarUrl())),
                    Timestamp = DateTimeOffset.UtcNow,
                    ThumbnailUrl = ctx.User.GetAvatarUrl(ImageFormat.Auto, 256) ?? ctx.User.GetDefaultAvatarUrl() ?? ctx.User.GetDefaultAvatarUrl(),
                    Footer = new EmbedFooterBuilder {
                        Text = "Help Menu",
                        IconUrl = ctx.Guild?.IconUrl ?? ctx.User.GetDefaultAvatarUrl()
                    }
                };
                return (errorEmbed.Build(), null);
            }

            var embed = new EmbedBuilder {
                Title = "Help Menu",
                Description = "Select a category to view available commands.",
                Color = new Color(await _colorUtils.RandomColorFromUrlAsync(ctx.User.GetAvatarUrl() ?? ctx.User.GetDefaultAvatarUrl())),
                Timestamp = DateTimeOffset.UtcNow,
                ThumbnailUrl = ctx.User.GetAvatarUrl(ImageFormat.Auto, 256) ?? ctx.User.GetDefaultAvatarUrl() ?? ctx.User.GetDefaultAvatarUrl(),
                Footer = new EmbedFooterBuilder {
                    Text = "Help Menu",
                    IconUrl = ctx.Guild?.IconUrl ?? ctx.User.GetDefaultAvatarUrl()
                }
            };

            var menu = new SelectMenuBuilder()
                .WithCustomId(CustomIds.HelpCategory(invokerUserId))
                .WithPlaceholder("Select a category")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (var category in visibleCategories.Take(25)) {
                menu.AddOption(category.Name, category.Key, category.Description);
            }

            var component = new ComponentBuilder().WithSelectMenu(menu);

            return (embed.Build(), component.Build());

        } catch (Exception ex) {
            await _loggingService.LogErrorAsync("HelpService", $"Error rendering category picker for {ctx.User.Username}: ", ex);
            throw;

        }
    }

    public async Task<(Embed embed, MessageComponent component)> RenderCommandListAsync(IInteractionContext ctx, IReadOnlyList<SlashCommandInfo> commands) {
        var category = commands.Select(c => GetCategory(c)).FirstOrDefault(new CommandCategory("unknown", "Unknown", "Unknown category"));

        var embed = new EmbedBuilder {
            Title = $"Commands in {category.Name}",
            Description = category.Description,
            Color = new Color(await _colorUtils.RandomColorFromUrlAsync(ctx.User.GetAvatarUrl() ?? ctx.User.GetDefaultAvatarUrl())),
            Timestamp = DateTimeOffset.UtcNow,
            ThumbnailUrl = ctx.User.GetAvatarUrl(ImageFormat.Auto, 256) ?? ctx.User.GetDefaultAvatarUrl() ?? ctx.User.GetDefaultAvatarUrl(),
            Footer = new EmbedFooterBuilder {
                Text = "Help Menu",
                IconUrl = ctx.Guild?.IconUrl ?? ctx.User.GetDefaultAvatarUrl()
            }
        };
        foreach (var command in commands.Take(25)) {
            embed.AddField(GetCommandPath(command), command.Description ?? "No description provided.", true);
        }

        var menu = new SelectMenuBuilder()
            .WithCustomId(CustomIds.HelpCategory(ctx.User.Id))
            .WithPlaceholder("Select a category")
            .WithMinValues(1)
            .WithMaxValues(1);

        var visibleCategories = (await GetVisibleCommandsAsync(ctx)).Select(c => GetCategory(c)).DistinctBy(c => c.Key).ToList();
        foreach (var cat in visibleCategories.Take(25)) {
            menu.AddOption(cat.Name, cat.Key, cat.Description);
        }
        var component = new ComponentBuilder().WithSelectMenu(menu);
        return (embed.Build(), component.Build());
    }

    public async Task<IReadOnlyList<SlashCommandInfo>> GetVisibleCommandsForCategoryAsync(IInteractionContext ctx, string categoryKey) {
        var normalizedKey = Slugify(categoryKey);
        var visibleCommands = await GetVisibleCommandsAsync(ctx);

        if(string.IsNullOrWhiteSpace(normalizedKey)) return [];

        return visibleCommands.Where(c => GetCategory(c).Key == normalizedKey).ToList();
    }

}


public readonly record struct CommandCategory(
    string Key,
    string Name,
    string? Description);