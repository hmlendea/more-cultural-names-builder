using NuciCLI.Arguments;

namespace MoreCulturalNamesBuilder.Configuration
{
    public sealed class InputSettings(ArgumentsCollection args)
    {
        public string LanguageStorePath { get; set; } = args.Get<string>("lang");

        public string LocationStorePath { get; set; } = args.Get<string>("loc");

        public string LandedTitlesFilePath { get; set; } = (string)args["landed-titles"];
    }
}
