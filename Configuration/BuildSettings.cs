using NuciCLI;

namespace MoreCulturalNamesModBuilder.Configuration
{
    public sealed class BuildSettings
    {
        static string[] GameOptions = { "-g", "--game" };

        public BuildSettings(string[] args)
        {
            Game = CliArgumentsReader.GetOptionValue(args, GameOptions);
        }

        public string Game { get; set; }
    }
}
