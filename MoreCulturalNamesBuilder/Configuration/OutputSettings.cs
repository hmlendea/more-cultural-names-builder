using NuciCLI.Arguments;
using System;

namespace MoreCulturalNamesBuilder.Configuration
{
    public sealed class OutputSettings(ArgumentsCollection args)
    {
        public string ModOutputDirectory { get; set; } = ResolveOutputDirectory(args);

        public bool AreVerboseCommentsEnabled { get; set; } = args.Get<string>("verbose") == "true";

        public string LandedTitlesFileName { get; set; } = (string)args["landed-titles-name"];

        private static string ResolveOutputDirectory(ArgumentsCollection args)
        {
            string outputDirectory = (string)args["output"];

            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                return outputDirectory;
            }

            string outputDirectoryAlias = (string)args["out"];

            if (!string.IsNullOrWhiteSpace(outputDirectoryAlias))
            {
                return outputDirectoryAlias;
            }

            throw new ArgumentException("Missing required argument: --output (alias: --out).");
        }
    }
}
