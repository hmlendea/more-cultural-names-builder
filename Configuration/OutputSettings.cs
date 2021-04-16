using NuciCLI;

namespace MoreCulturalNamesModBuilder.Configuration
{
    public sealed class OutputSettings
    {
        static string[] OutputDirectoryPathOptions = { "-o", "--out", "--output" };

        static string[] VerboseCommentsOptions = { "--verbose" };

        public string ModOutputDirectory { get; set; }

        public bool AreVerboseCommentsEnabled { get; set; }

        public OutputSettings(string[] args)
        {
            ModOutputDirectory = CliArgumentsReader.GetOptionValue(args, OutputDirectoryPathOptions);
            AreVerboseCommentsEnabled = CliArgumentsReader.HasOption(args, VerboseCommentsOptions);
        }
    }
}
