using NuciCLI;

namespace MoreCulturalNamesBuilder.Configuration
{
    public sealed class InputSettings
    {
        static string[] LanguageStorePathOptions = { "--lang", "--languages" };
        static string[] LocationsStorePathOptions = { "--loc", "--locations" };
        static string[] TitleStorePathOptions = { "-t", "--titles" };
        static string[] LandedTitlesFilePathOptions = { "--landed-titles", "--landed-titles-in", "--landed-titles-input" };

        public string LanguageStorePath { get; set; }

        public string LocationStorePath { get; set; }

        public string TitleStorePath { get; set; }

        public string LandedTitlesFilePath { get; set; }

        public InputSettings(string[] args)
        {
            LanguageStorePath = CliArgumentsReader.GetOptionValue(args, LanguageStorePathOptions);
            LocationStorePath = CliArgumentsReader.GetOptionValue(args, LocationsStorePathOptions);
            TitleStorePath = CliArgumentsReader.GetOptionValue(args, TitleStorePathOptions);
            LandedTitlesFilePath = CliArgumentsReader.TryGetOptionValue(args, LandedTitlesFilePathOptions);
        }
    }
}
