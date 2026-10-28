using System;

using NuciCLI.Arguments;

namespace MoreCulturalNamesBuilder.Configuration
{
    public sealed class ModSettings(ArgumentsCollection args)
    {
        public string Id { get; set; } = args.Get<string>("id");

        public string Name { get; set; } = args.Get<string>("name");

        public string Version { get; set; } = ResolveVersion(args);

        public string? Dependency { get; set; } = ResolveDependency(args);

        public string Game { get; set; } = args.Get<string>("game");

        public string GameVersion { get; set; } = args.Get<string>("game-version");

        private static string ResolveVersion(ArgumentsCollection args)
        {
            string? version = args["version"] as string;

            if (!string.IsNullOrWhiteSpace(version))
            {
                return version;
            }

            string? versionAlias = args["ver"] as string;

            if (!string.IsNullOrWhiteSpace(versionAlias))
            {
                return versionAlias;
            }

            throw new ArgumentException("Missing required argument: --version (alias: --ver).");
        }

        private static string? ResolveDependency(ArgumentsCollection args)
        {
            string? dependency = args["dependency"] as string;

            if (!string.IsNullOrWhiteSpace(dependency))
            {
                return dependency;
            }

            string? dependencyAlias = args["dep"] as string;

            if (!string.IsNullOrWhiteSpace(dependencyAlias))
            {
                return dependencyAlias;
            }

            return dependency;
        }
    }
}
