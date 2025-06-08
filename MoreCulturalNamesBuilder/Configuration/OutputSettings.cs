using NuciCLI;

namespace MoreCulturalNamesBuilder.Configuration
{
    public sealed class OutputSettings(string[] args)
    {
        static readonly string[] OutputDirectoryPathOptions = ["-o", "--out", "--output"];

        static readonly string[] VerboseCommentsOptions = ["--verbose"];

        static readonly string[] LandedTitlesFileNameOptions = ["--landed-titles-name"];

        public string ModOutputDirectory { get; set; } = CliArgumentsReader.GetOptionValue(args, OutputDirectoryPathOptions);

        public bool AreVerboseCommentsEnabled { get; set; } = CliArgumentsReader.HasOption(args, VerboseCommentsOptions);

        public string LandedTitlesFileName { get; set; } = CliArgumentsReader.TryGetOptionValue(args, LandedTitlesFileNameOptions);
    }
}
