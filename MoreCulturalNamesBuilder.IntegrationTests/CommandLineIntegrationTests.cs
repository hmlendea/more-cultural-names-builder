using System;
using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class CommandLineIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(CommandLineIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [TestCase("True")]
        [TestCase("TRUE")]
        [TestCase("tRuE")]
        public void GivenAMixedCaseTrueVerboseValue_WhenBuildingTheCk2Mod_ThenAnnotationsAreOmitted(string verboseValue)
        {
            string[] arguments = TestArguments.Without(
                TestArguments.Build(temporaryDirectory.DirectoryPath, BuildRequest("CK2")),
                "--verbose");

            ModBuildRunner.Run(TestArguments.With(arguments, "--verbose", verboseValue));

            Assert.That(
                TextContent.ReadWindows1252(Path.Combine(
                    temporaryDirectory.DirectoryPath,
                    "CK2",
                    "m",
                    "common",
                    "landed_titles",
                    "00_landed_titles.txt")),
                Does.Not.Contain("Language="),
                $"Only the exact lowercase 'true' enables annotations, so '{verboseValue}' must omit them.");
        }

        [TestCase("1")]
        [TestCase("yes")]
        [TestCase("on")]
        public void GivenANonBooleanVerboseValue_WhenBuildingTheCk2Mod_ThenAnnotationsAreOmitted(string verboseValue)
        {
            string[] arguments = TestArguments.Without(
                TestArguments.Build(temporaryDirectory.DirectoryPath, BuildRequest("CK2")),
                "--verbose");

            ModBuildRunner.Run(TestArguments.With(arguments, "--verbose", verboseValue));

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
        public void GivenADuplicateVerboseFlag_WhenBuildingTheCk2Mod_ThenTheLastValueWins()
        {
            string[] arguments = TestArguments.Without(
                TestArguments.Build(temporaryDirectory.DirectoryPath, BuildRequest("CK2")),
                "--verbose");
            arguments = TestArguments.With(arguments, "--verbose", "false");
            arguments = TestArguments.With(arguments, "--verbose", "true");

            ModBuildRunner.Run(arguments);

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
        public void GivenADuplicateLandedTitlesName_WhenBuildingTheCk2Mod_ThenTheLastValueWins()
        {
            string[] arguments = TestArguments.Without(
                TestArguments.Build(temporaryDirectory.DirectoryPath, BuildRequest("CK2")),
                "--landed-titles-name");
            arguments = TestArguments.With(arguments, "--landed-titles-name", "first_name.txt");
            arguments = TestArguments.With(arguments, "--landed-titles-name", "second_name.txt");

            ModBuildRunner.Run(arguments);

            string landedTitlesDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "m",
                "common",
                "landed_titles");

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(Path.Combine(landedTitlesDirectoryPath, "second_name.txt")), Is.True);
                Assert.That(File.Exists(Path.Combine(landedTitlesDirectoryPath, "first_name.txt")), Is.False);
            });
        }

        [Test]
        public void GivenBothTheOutputAndItsAlias_WhenBuildingTheCk2Mod_ThenTheFullNameWins()
        {
            string aliasDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "alias-output");
            string[] arguments = TestArguments.With(
                TestArguments.Build(temporaryDirectory.DirectoryPath, BuildRequest("CK2")),
                "--out",
                aliasDirectoryPath);

            ModBuildRunner.Run(arguments);

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(Path.Combine(temporaryDirectory.DirectoryPath, "CK2", "m.mod")), Is.True);
                Assert.That(Directory.Exists(aliasDirectoryPath), Is.False);
            });
        }

        [Test]
        public void GivenBothTheVersionAndItsAlias_WhenBuildingTheCk2Mod_ThenTheFullNameWins()
        {
            string[] arguments = TestArguments.With(
                TestArguments.Build(temporaryDirectory.DirectoryPath, BuildRequest("CK2")),
                "--ver",
                "9.9.9");

            ModBuildRunner.Run(arguments);

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, "CK2", "m.mod")),
                    Does.Contain("# Version 1.0.0"));
                Assert.That(
                    TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, "CK2", "m.mod")),
                    Does.Not.Contain("9.9.9"));
            });
        }

        [Test]
        public void GivenBothTheDependencyAndItsAlias_WhenBuildingTheCk2Mod_ThenTheFullNameWins()
        {
            string[] arguments = TestArguments.With(
                TestArguments.With(
                    TestArguments.Build(temporaryDirectory.DirectoryPath, BuildRequest("CK2")),
                    "--dependency",
                    "Full Name Dependency"),
                "--dep",
                "Alias Dependency");

            ModBuildRunner.Run(arguments);

            string descriptor = TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, "CK2", "m.mod"));

            Assert.Multiple(() =>
            {
                Assert.That(descriptor, Does.Contain("Full Name Dependency"));
                Assert.That(descriptor, Does.Not.Contain("Alias Dependency"));
            });
        }

        [Test]
        public void GivenAnAliasOnlyDependency_WhenBuildingTheCk2Mod_ThenTheAliasIsUsed()
        {
            string[] arguments = TestArguments.With(
                TestArguments.Build(temporaryDirectory.DirectoryPath, BuildRequest("CK2")),
                "--dep",
                "Alias Only Dependency");

            ModBuildRunner.Run(arguments);

            Assert.That(
                TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, "CK2", "m.mod")),
                Does.Contain("dependencies = { \"Alias Only Dependency\" }"));
        }

        [Test]
        public void GivenAnOutputDirectoryWithSpaces_WhenBuildingTheCk2Mod_ThenTheTreeIsCreated()
        {
            string outputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "output with spaces");

            ModBuildRunner.Run(outputDirectoryPath, BuildRequest("CK2"));

            Assert.That(File.Exists(Path.Combine(outputDirectoryPath, "CK2", "m.mod")), Is.True);
        }

        [Test]
        public void GivenAnOutputDirectoryWithATrailingSeparator_WhenBuildingTheCk2Mod_ThenTheTreeIsCreated()
        {
            string outputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "trailing")
                + Path.DirectorySeparatorChar;

            ModBuildRunner.Run(outputDirectoryPath, BuildRequest("CK2"));

            Assert.That(File.Exists(Path.Combine(temporaryDirectory.DirectoryPath, "trailing", "CK2", "m.mod")), Is.True);
        }

        [Test]
        public void GivenABlankGameVersion_WhenBuildingTheCk2Mod_ThenAnEmptyGameVersionIsDeclared()
        {
            string[] arguments = TestArguments.Without(
                TestArguments.Build(temporaryDirectory.DirectoryPath, BuildRequest("CK2")),
                "--game-version");
            arguments = TestArguments.With(arguments, "--game-version", string.Empty);

            ModBuildRunner.Run(arguments);

            Assert.That(
                TextContent.Read(Path.Combine(temporaryDirectory.DirectoryPath, "CK2", "m.mod")),
                Does.Contain($"# for CK2 {Environment.NewLine}"));
        }

        private static ModBuildRequest BuildRequest(string game)
            => new()
            {
                Game = game,
                ModId = "m",
                LandedTitlesFilePath = TestPaths.Ck2LandedTitlesFilePath,
                LandedTitlesFileName = "00_landed_titles.txt"
            };
    }
}
