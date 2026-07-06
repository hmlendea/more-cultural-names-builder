using NuciCLI.Arguments;

namespace MoreCulturalNamesBuilder.Configuration
{
    public sealed class OutputSettings(ArgumentsCollection args)
    {
        public string ModOutputDirectory { get; set; } = args.Get<string>("output");

        public bool AreVerboseCommentsEnabled { get; set; } = args.Get<string>("verbose") == "true";

        public string LandedTitlesFileName { get; set; } = (string)args["landed-titles-name"];
    }
}
