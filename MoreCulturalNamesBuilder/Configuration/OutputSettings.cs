using NuciCLI;

namespace MoreCulturalNamesBuilder.Configuration
{
    public sealed class OutputSettings
    {
        static string[] OutputDirectoryPathOptions = { "-o", "--out", "--output" };

        static string[] VerboseCommentsOptions = { "--verbose" };

        static string[] LandedTitlesFileNameOptions = { "--landed-titles-name" };

        public string ModOutputDirectory { get; set; }

        public bool AreVerboseCommentsEnabled { get; set; }

        public string LandedTitlesFileName { get; set; }

        public OutputSettings(string[] args)
        {
            ModOutputDirectory = CliArgumentsReader.GetOptionValue(args, OutputDirectoryPathOptions);
            AreVerboseCommentsEnabled = CliArgumentsReader.HasOption(args, VerboseCommentsOptions);
            LandedTitlesFileName = CliArgumentsReader.TryGetOptionValue(args, LandedTitlesFileNameOptions);
        }
    }
}
