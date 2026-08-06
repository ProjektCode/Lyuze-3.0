using Discord;
using Discord.Interactions;

namespace Lyuze.Core.Features.Utility.Services {

    public class HelpService(InteractionService interactionService, IServiceProvider serviceProvider){
        private readonly InteractionService _interactionService = interactionService;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task<CommandResult> GetCommandInfoAsync(string commandName, IInteractionContext ctx) {

            
            var command = _interactionService.SlashCommands.FirstOrDefault(c => string.Equals(c.Name, commandName, StringComparison.OrdinalIgnoreCase));

            if (command == null) return CommandResult.Failure($"Command '{commandName}' not found.");

            var condition = await command.CheckPreconditionsAsync(ctx, _serviceProvider);
            if (!condition.IsSuccess) return CommandResult.Failure($"You do not have permission to view this command.");

            return CommandResult.Success(command);
        }
    }

    public readonly record struct CommandResult {
        public bool IsSuccess { get; init; }
        public string? ErrorMessage { get; init; }
        public SlashCommandInfo? Info { get; init; }

        public static CommandResult Success(SlashCommandInfo slashCommandInfo) =>
            new() { IsSuccess = true, Info = slashCommandInfo};

        public static CommandResult Failure(string errorMessage) =>
            new() { IsSuccess = false, ErrorMessage = errorMessage };
    }

}
