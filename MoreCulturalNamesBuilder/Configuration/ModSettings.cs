using NuciCLI.Arguments;

namespace MoreCulturalNamesBuilder.Configuration
{
    public sealed class ModSettings(ArgumentsCollection args)
    {
        public string Id { get; } = args.Get<string>("id");

        public string Name { get; } = args.Get<string>("name");

        public string Version { get; set; } = args.Get<string>("version");

        public string Dependency { get; set; } = (string)args["dependency"];

        public string Game { get; } = args.Get<string>("game");

        public string GameVersion { get; } = args.Get<string>("game-version");
    }
}
