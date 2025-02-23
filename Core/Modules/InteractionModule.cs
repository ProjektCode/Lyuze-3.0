using Discord;
using Discord.Interactions;


namespace Lyuze.Core.Modules {
    public class InteractionModule: InteractionModuleBase<SocketInteractionContext> {

        [SlashCommand("ping", "Slash Command Example as a ping")]
        public async Task HandlePingCommand() {
            await RespondAsync("pong");
        }

        [SlashCommand("components", "Buttons and selectable menus demo")]
        public async Task ComponentCommand() {
            var btn = new ButtonBuilder() {
                Label = "Button Label",
                CustomId = "btn_components",
                Style = ButtonStyle.Primary
            };

            var menu = new SelectMenuBuilder() {
                CustomId = "menu_components",
                Placeholder = "Sample Menu Placeholder"
            };

            menu.AddOption("First Option", "First");
            menu.AddOption("Second Option", "Second");

            var comp = new ComponentBuilder();
            comp.WithButton(btn);
            comp.WithSelectMenu(menu);
            await RespondAsync("Testing", components: comp.Build());

        }

        [ComponentInteraction("btn_components")]
        public async Task HandleButtonInput() {
            await RespondWithModalAsync<DemoModal>("demo_modal");
        }

        [ComponentInteraction("menu_components")]
        public async Task HandleMenuSelectAsync(string[] inputs) {
            await RespondAsync(inputs[0]);
        }

        [ModalInteraction("demo_modal")]
        public async Task HandleModalInput(DemoModal modal) {
            string input = modal.Greeting;
            await RespondAsync(input);
        }

        [MessageCommand("Message Command Test")]
        public async Task MessageCommandAsync(IMessage msg) {
            await RespondAsync($"Message author is: {msg.Author.Mention}.");

        }

    }

    public class DemoModal: IModal {
        public string Title => "Demo Modal";

        [InputLabel("Send a greeting!")]
        [ModalTextInput("Greeting Input", TextInputStyle.Short, placeholder: "Be nice...", maxLength: 100)]
        public required string Greeting { get; set; }

    }
}
