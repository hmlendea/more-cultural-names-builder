using System;
using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class ArgumentEdgeCaseIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        private string emptyLandedTitlesFilePath => Path.Combine(temporaryDirectory.DirectoryPath, "empty_landed_titles.txt");

        private string lineEndingLandedTitlesFilePath => Path.Combine(temporaryDirectory.DirectoryPath, "line_ending_landed_titles.txt");

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(ArgumentEdgeCaseIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [TestCase("EU5")]
        [TestCase("Vic3")]
        public void GivenABlankOutputDirectoryAlias_WhenBuildingTheMod_ThenAnArgumentExceptionIsThrown(string game)
        {
            ModBuildRequest request = BuildRequest(game);
            string[] arguments = TestArguments.Build(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                () => ModBuildRunner.Run(TestArguments.With(arguments, "--output", string.Empty)),
                Throws.TypeOf<ArgumentException>());
        }

        [TestCase("EU5")]
        [TestCase("Vic3")]
        public void GivenABlankVersionAlias_WhenBuildingTheMod_ThenAnArgumentExceptionIsThrown(string game)
        {
            ModBuildRequest request = BuildRequest(game);
            string[] arguments = TestArguments.Build(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                () => ModBuildRunner.Run(TestArguments.With(arguments, "--version", string.Empty)),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GivenABlankModName_WhenBuildingTheCk2Mod_ThenAnEmptyNameIsDeclared()
        {
            ModBuildRequest request = BuildRequest("CK2");
            request.ModName = string.Empty;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, "CK2", "m.mod")),
                Does.Contain("name = \"\""));
        }

        [Test]
        public void GivenABlankModName_WhenBuildingTheEuropaUniversalis5Mod_ThenAnEmptyNameIsDeclared()
        {
            ModBuildRequest request = BuildRequest("EU5");
            request.ModName = string.Empty;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, "EU5", "m", ".metadata", "metadata.json")),
                Does.Contain("\"name\": \"\""));
        }

        [Test]
        public void GivenAWhitespaceModName_WhenBuildingTheEuropaUniversalis5Mod_ThenItIsPreserved()
        {
            ModBuildRequest request = BuildRequest("EU5");
            request.ModName = "   ";

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, "EU5", "m", ".metadata", "metadata.json")),
                Does.Contain("\"name\": \"   \""));
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        public void GivenABlankLandedTitlesFileName_WhenBuildingTheMod_ThenTheTargetDirectoryPathFails(string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.LandedTitlesFileName = string.Empty;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.Exception,
                "The blank file name resolves to the landed titles directory, which cannot be overwritten as a file.");
        }

        [Test]
        public void GivenANestedLandedTitlesFileName_WhenBuildingTheCk2Mod_ThenADirectoryNotFoundExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest("CK2");
            request.LandedTitlesFileName = "sub/nested.txt";

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<DirectoryNotFoundException>());
        }

        [TestCase("HOI4")]
        [TestCase("IR")]
        [TestCase("EU5")]
        [TestCase("Vic3")]
        public void GivenALandedTitlesPathWithoutAFileName_WhenBuildingAStructureOnlyMod_ThenItIsIgnored(string game)
        {
            ModBuildRequest request = BuildRequest(game);
            request.LandedTitlesFilePath = TestPaths.Ck2LandedTitlesFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.Nothing);
            Assert.That(Directory.Exists(Path.Combine(temporaryDirectory.DirectoryPath, game, "m")), Is.True);
        }

        [Test]
        public void GivenTheVerboseFlagIsAbsent_WhenBuildingTheCk2Mod_ThenLanguageAnnotationsAreOmitted()
        {
            string[] arguments = TestArguments.Without(
                TestArguments.Build(temporaryDirectory.DirectoryPath, BuildRequest("CK2")),
                "--verbose");

            ModBuildRunner.Run(arguments);

            Assert.That(
                TextContent.ReadWindows1252(Path.Combine(
                    temporaryDirectory.DirectoryPath,
                    "CK2",
                    "m",
                    "common",
                    "landed_titles",
                    "00_landed_titles.txt")),
                Does.Not.Contain("Language="));
        }

        [Test]
        public void GivenTheVerboseFlagIsLowercaseTrue_WhenBuildingTheCk2Mod_ThenLanguageAnnotationsAreIncluded()
        {
            string[] arguments = TestArguments.Without(
                TestArguments.Build(temporaryDirectory.DirectoryPath, BuildRequest("CK2")),
                "--verbose");
            string[] updatedArguments = TestArguments.With(arguments, "--verbose", "true");

            ModBuildRunner.Run(updatedArguments);

            Assert.That(
                TextContent.ReadWindows1252(Path.Combine(
                    temporaryDirectory.DirectoryPath,
                    "CK2",
                    "m",
                    "common",
                    "landed_titles",
                    "00_landed_titles.txt")),
                Does.Contain("Language="));
        }

        [Test]
        public void GivenAnEmptyLandedTitlesFile_WhenBuildingTheCk2Mod_ThenAnEmptyFileIsEmitted()
        {
            File.WriteAllText(emptyLandedTitlesFilePath, string.Empty);
            ModBuildRequest request = BuildRequest("CK2");
            request.LandedTitlesFilePath = emptyLandedTitlesFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                TextContent.ReadBytes(Path.Combine(
                    temporaryDirectory.DirectoryPath,
                    "CK2",
                    "m",
                    "common",
                    "landed_titles",
                    "00_landed_titles.txt")),
                Is.Empty);
        }

        [Test]
        public void GivenALandedTitlesFileWithCarriageReturns_WhenBuildingTheCk2Mod_ThenTheOutputUsesOnlyLineFeeds()
        {
            File.WriteAllText(
                lineEndingLandedTitlesFilePath,
                "c_cluj = {\r\n    romanian = \"Original\" # Original name\r\n}\r\n");
            ModBuildRequest request = BuildRequest("CK2");
            request.LandedTitlesFilePath = lineEndingLandedTitlesFilePath;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                TextContent.ReadBytes(
                    Path.Combine(temporaryDirectory.DirectoryPath, "CK2", "m", "common", "landed_titles", "00_landed_titles.txt")),
                Does.Not.Contains((byte)'\r'));
        }

        [Test]
        public void GivenALandedTitlesFileWithTabs_WhenBuildingTheCk2Mod_ThenTabsAreExpandedToFourSpaces()
        {
            File.WriteAllText(
                lineEndingLandedTitlesFilePath,
                "c_cluj = {\n\tromanian = \"Original\"\n}\n");
            ModBuildRequest request = BuildRequest("CK2");
            request.LandedTitlesFilePath = lineEndingLandedTitlesFilePath;
            request.AreVerboseCommentsEnabled = false;

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            byte[] content = TextContent.ReadBytes(
                Path.Combine(temporaryDirectory.DirectoryPath, "CK2", "m", "common", "landed_titles", "00_landed_titles.txt"));

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Not.Contains((byte)'\t'));
                Assert.That(content, Does.Contain((byte)' '));
            });
        }

        [Test]
        public void GivenAMissingLandedTitlesFile_WhenBuildingTheCk2Mod_ThenTheServiceProviderIsStillDisposed()
        {
            ModBuildRequest request = BuildRequest("CK2");
            request.LandedTitlesFilePath = TestPaths.MissingLandedTitlesFilePath;

            Assert.That(() => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request), Throws.Exception);
            Assert.That(
                () => Program.ServiceProvider.GetService(typeof(object)),
                Throws.TypeOf<ObjectDisposedException>(),
                "The service provider must be disposed even when a build fails.");
        }

        private static ModBuildRequest BuildRequest(string game)
            => new()
            {
                Game = game,
                ModId = "m",
                LandedTitlesFilePath = ResolveLandedTitlesFilePath(game),
                LandedTitlesFileName = "00_landed_titles.txt"
            };

        private static string ResolveLandedTitlesFilePath(string game)
        {
            if (string.Equals(game, "CK3", StringComparison.Ordinal))
            {
                return TestPaths.Ck3LandedTitlesFilePath;
            }

            return TestPaths.Ck2LandedTitlesFilePath;
        }
    }
}
