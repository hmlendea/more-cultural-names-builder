using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class PropagationIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(PropagationIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("EU5")]
        [TestCase("Vic3")]
        public void GivenACustomModIdentifier_WhenBuildingAnyMod_ThenEveryPathCarriesIt(string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.ModId = "zigzag-id";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string[] pathsWithoutIdentifier = [.. Directory
                .GetFiles(Path.Combine(temporaryDirectory.DirectoryPath, game), "*", SearchOption.AllDirectories)
                .Select(filePath => Path.GetRelativePath(Path.Combine(temporaryDirectory.DirectoryPath, game), filePath))
                .Where(relativePath => !relativePath.Contains("zigzag", StringComparison.Ordinal) &&
                    !relativePath.EndsWith("00_landed_titles.txt", StringComparison.Ordinal) &&
                    !string.Equals(relativePath, "descriptor.mod", StringComparison.Ordinal))];

            Assert.That(
                pathsWithoutIdentifier,
                Is.Empty,
                $"Every generated path of {game} must derive from the mod identifier.");
        }

        [TestCase("CK2", "more-cultural-names.mod", "# Version 4.5.6")]
        [TestCase("CK3", "more-cultural-names.mod", "# Version 4.5.6")]
        [TestCase("HOI4", "more-cultural-names.mod", "# Version 4.5.6")]
        [TestCase("IR", "more-cultural-names.mod", "# Version 4.5.6")]
        [TestCase("EU5", "more-cultural-names/.metadata/metadata.json", "\"version\": \"4.5.6\"")]
        [TestCase("Vic3", "more-cultural-names/.metadata/metadata.json", "\"version\": \"4.5.6\"")]
        public void GivenACustomModVersion_WhenBuildingAnyMod_ThenTheDescriptorDeclaresIt(
            string game,
            string descriptorRelativePath,
            string expectedFragment)
        {
            ModBuildRequest request = BuildRequest(game);
            request.ModVersion = "4.5.6";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, game, descriptorRelativePath)),
                Does.Contain(expectedFragment));
        }

        [TestCase("CK2", "more-cultural-names.mod", "name = \"Zigzag Mod\"")]
        [TestCase("CK3", "more-cultural-names.mod", "name=\"Zigzag Mod\"")]
        [TestCase("HOI4", "more-cultural-names.mod", "name=\"Zigzag Mod\"")]
        [TestCase("IR", "more-cultural-names.mod", "name=\"Zigzag Mod\"")]
        [TestCase("EU5", "more-cultural-names/.metadata/metadata.json", "\"name\": \"Zigzag Mod\"")]
        [TestCase("Vic3", "more-cultural-names/.metadata/metadata.json", "\"name\": \"Zigzag Mod\"")]
        public void GivenACustomModName_WhenBuildingAnyMod_ThenTheDescriptorDeclaresIt(
            string game,
            string descriptorRelativePath,
            string expectedFragment)
        {
            ModBuildRequest request = BuildRequest(game);
            request.ModName = "Zigzag Mod";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, game, descriptorRelativePath)),
                Does.Contain(expectedFragment));
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("EU5")]
        [TestCase("Vic3")]
        public void GivenACustomGameVersion_WhenBuildingAnyMod_ThenTheDescriptorDeclaresIt(string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.GameVersion = "12.34.*";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string descriptorPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                game,
                ResolveDescriptorRelativePath(game));

            Assert.That(TextContent.Read(descriptorPath), Does.Contain("12.34.*"));
        }

        [TestCase("CK3")]
        [TestCase("HOI4")]
        [TestCase("IR")]
        public void GivenTheCanonicalDataStores_WhenBuildingAStructuralMod_ThenTheInnerDescriptorOmitsThePath(string game)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            string innerDescriptorPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                game,
                "more-cultural-names",
                "descriptor.mod");
            string rootDescriptorPath = Path.Combine(temporaryDirectory.DirectoryPath, game, "more-cultural-names.mod");

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(rootDescriptorPath), Is.True);
                Assert.That(File.Exists(innerDescriptorPath), Is.True);
                Assert.That(TextContent.Read(rootDescriptorPath), Does.Contain("path=\"mod/more-cultural-names\""));
                Assert.That(TextContent.Read(innerDescriptorPath), Does.Not.Contain("path="));
            });
        }

        [Test]
        public void GivenTheCanonicalDataStores_WhenBuildingTheCk2Mod_ThenOnlyTheRootDescriptorIsEmitted()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK2"));

            Assert.Multiple(() =>
            {
                Assert.That(
                    File.Exists(Path.Combine(temporaryDirectory.DirectoryPath, "CK2", "more-cultural-names.mod")),
                    Is.True);
                Assert.That(
                    File.Exists(Path.Combine(temporaryDirectory.DirectoryPath, "CK2", "more-cultural-names", "descriptor.mod")),
                    Is.False);
            });
        }

        [TestCase("CK3", "tags={")]
        [TestCase("EU5", "\"tags\": [")]
        [TestCase("Vic3", "\"tags\": [")]
        public void GivenTheCanonicalDataStores_WhenBuildingTheMod_ThenTheDescriptorDeclaresTags(
            string game,
            string expectedFragment)
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest(game));

            string descriptorPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                game,
                ResolveDescriptorRelativePath(game));

            Assert.That(TextContent.Read(descriptorPath), Does.Contain(expectedFragment));
        }

        private static ModBuildRequest BuildRequest(string game)
        {
            ModBuildRequest request = new() { Game = game };

            if (string.Equals(game, "CK2", StringComparison.Ordinal))
            {
                request.LandedTitlesFilePath = TestPaths.Ck2LandedTitlesFilePath;
                request.LandedTitlesFileName = "00_landed_titles.txt";
            }

            if (string.Equals(game, "CK3", StringComparison.Ordinal))
            {
                request.LandedTitlesFilePath = TestPaths.Ck3LandedTitlesFilePath;
                request.LandedTitlesFileName = "00_landed_titles.txt";
            }

            return request;
        }

        private static string ResolveDescriptorRelativePath(string game)
        {
            if (string.Equals(game, "EU5", StringComparison.Ordinal) ||
                string.Equals(game, "Vic3", StringComparison.Ordinal))
            {
                return "more-cultural-names/.metadata/metadata.json";
            }

            return "more-cultural-names.mod";
        }
    }
}
