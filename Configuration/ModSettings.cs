using NuciCLI;

namespace MoreCulturalNamesModBuilder.Configuration
{
    public sealed class ModSettings
    {
        static string[] IdOptions = { "--id" };

        static string[] NameOptions = { "-n", "--name" };

        static string[] VersionOptions = { "-v", "--ver", "--version" };

        static string[] DependencyOptions = { "--dep", "--dependency" };

        static string[] GameOptions = { "-g", "--game" };

        static string[] GameVersionOptions = { "--game-ver", "--game-version" };

        public ModSettings(string[] args)
        {
            Id = CliArgumentsReader.GetOptionValue(args, IdOptions);
            Name = CliArgumentsReader.GetOptionValue(args, NameOptions);
            Version = CliArgumentsReader.GetOptionValue(args, VersionOptions);
            Dependency = CliArgumentsReader.TryGetOptionValue(args, DependencyOptions);
            Game = CliArgumentsReader.GetOptionValue(args, GameOptions);
            GameVersion = CliArgumentsReader.GetOptionValue(args, GameVersionOptions);
        }

        public string Id { get; }

        public string Name { get; }

        public string Version { get; set; }

        public string Dependency { get; set; }

        public string Game { get; }

        public string GameVersion { get; }
    }
}
