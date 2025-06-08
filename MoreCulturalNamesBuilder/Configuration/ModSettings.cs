using NuciCLI;

namespace MoreCulturalNamesBuilder.Configuration
{
    public sealed class ModSettings(string[] args)
    {
        static readonly string[] IdOptions = ["--id"];

        static readonly string[] NameOptions = ["-n", "--name"];

        static readonly string[] VersionOptions = ["-v", "--ver", "--version"];

        static readonly string[] DependencyOptions = ["--dep", "--dependency"];

        static readonly string[] GameOptions = ["-g", "--game"];

        static readonly string[] GameVersionOptions = ["--game-ver", "--game-version"];

        public string Id { get; } = CliArgumentsReader.GetOptionValue(args, IdOptions);

        public string Name { get; } = CliArgumentsReader.GetOptionValue(args, NameOptions);

        public string Version { get; set; } = CliArgumentsReader.GetOptionValue(args, VersionOptions);

        public string Dependency { get; set; } = CliArgumentsReader.TryGetOptionValue(args, DependencyOptions);

        public string Game { get; } = CliArgumentsReader.GetOptionValue(args, GameOptions);

        public string GameVersion { get; } = CliArgumentsReader.GetOptionValue(args, GameVersionOptions);
    }
}
