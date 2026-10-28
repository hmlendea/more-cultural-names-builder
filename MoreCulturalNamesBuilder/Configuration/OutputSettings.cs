using System;

using NuciCLI.Arguments;

namespace MoreCulturalNamesBuilder.Configuration
{
    public sealed class OutputSettings(ArgumentsCollection args)
    {
        public string ModOutputDirectory { get; set; } = ResolveOutputDirectory(args);

        public bool AreVerboseCommentsEnabled { get; set; } = string.Equals(args.Get<string>("verbose"), "true");

        public string? LandedTitlesFileName { get; set; } = args["landed-titles-name"] as string;

        private static string ResolveOutputDirectory(ArgumentsCollection args)
        {
            string? outputDirectory = args["output"] as string;

            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                return outputDirectory;
            }

            string? outputDirectoryAlias = args["out"] as string;

            if (!string.IsNullOrWhiteSpace(outputDirectoryAlias))
            {
                return outputDirectoryAlias;
            }

            throw new ArgumentException("Missing required argument: --output (alias: --out).");
        }
    }
}
