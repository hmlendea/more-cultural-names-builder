using System;

using NuciCLI.Arguments;

namespace MoreCulturalNamesBuilder.Configuration
{
    public sealed class ModSettings(ArgumentsCollection args)
    {
        public string Id { get; } = args.Get<string>("id");

        public string Name { get; } = args.Get<string>("name");

        public string Version { get; set; } = ResolveVersion(args);

        public string Dependency { get; set; } = (string)args["dependency"];

        public string Game { get; } = args.Get<string>("game");

        public string GameVersion { get; } = args.Get<string>("game-version");

        private static string ResolveVersion(ArgumentsCollection args)
        {
            string version = (string)args["version"];

            if (!string.IsNullOrWhiteSpace(version))
            {
                return version;
            }

            string versionAlias = (string)args["ver"];

            if (!string.IsNullOrWhiteSpace(versionAlias))
            {
                return versionAlias;
            }

            throw new ArgumentException("Missing required argument: --version (alias: --ver).");
        }
    }
}
