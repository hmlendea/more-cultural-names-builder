using NuciCLI;

namespace MoreCulturalNamesBuilder.Configuration
{
    public sealed class InputSettings(string[] args)
    {
        static readonly string[] LanguageStorePathOptions = ["--lang", "--languages"];
        static readonly string[] LocationsStorePathOptions = ["--loc", "--locations"];
        static readonly string[] LandedTitlesFilePathOptions = ["--landed-titles", "--landed-titles-in", "--landed-titles-input"];

        public string LanguageStorePath { get; set; } = CliArgumentsReader.GetOptionValue(args, LanguageStorePathOptions);

        public string LocationStorePath { get; set; } = CliArgumentsReader.GetOptionValue(args, LocationsStorePathOptions);

        public string LandedTitlesFilePath { get; set; } = CliArgumentsReader.TryGetOptionValue(args, LandedTitlesFilePathOptions);
    }
}
