namespace MoreCulturalNamesBuilder.IntegrationTests
{
    internal sealed class ModBuildRequest
    {
        internal string Game { get; set; } = "CK2";

        internal string GameVersion { get; set; } = "1.18.*";

        internal string ModId { get; set; } = "more-cultural-names";

        internal string ModName { get; set; } = "More Cultural Names";

        internal string ModVersion { get; set; } = "1.0.0";

        internal string LanguageStorePath { get; set; } = TestPaths.LanguagesFilePath;

        internal string LocationStorePath { get; set; } = TestPaths.LocationsFilePath;

        internal string? LandedTitlesFilePath { get; set; }

        internal string? LandedTitlesFileName { get; set; }

        internal string? Dependency { get; set; }

        internal bool AreVerboseCommentsEnabled { get; set; } = true;
    }
}
