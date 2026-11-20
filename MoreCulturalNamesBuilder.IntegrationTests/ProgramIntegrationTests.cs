using System;
using System.Collections.Generic;
using System.IO;

using NuciDAL.Repositories;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class ProgramIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        private static IEnumerable<string> RequiredArgumentNames =>
            ["--lang", "--loc", "--id", "--name", "--game", "--game-version"];

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(ProgramIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenMissingRequiredArguments_WhenRunningTheProgram_ThenAnArgumentNullExceptionIsThrown()
        {
            string[] arguments = TestArguments.Build(temporaryDirectory.DirectoryPath, BuildRequest());

            foreach (string argumentName in RequiredArgumentNames)
            {
                string[] incompleteArguments = TestArguments.Without(arguments, argumentName);

                Assert.That(
                    () => ModBuildRunner.Run(incompleteArguments),
                    Throws.TypeOf<ArgumentNullException>()
                        .With.Property("ParamName").EqualTo(argumentName.TrimStart('-')),
                    argumentName);
            }
        }

        [TestCase("--output")]
        [TestCase("--version")]
        public void GivenMissingOutputOrVersion_WhenRunningTheProgram_ThenAnArgumentExceptionIsThrown(string argumentName)
        {
            string[] arguments = TestArguments.Without(
                TestArguments.Build(temporaryDirectory.DirectoryPath, BuildRequest()),
                argumentName);

            Assert.That(
                () => ModBuildRunner.Run(arguments),
                Throws.TypeOf<ArgumentException>().With.Message.Contains(argumentName));
        }

        [TestCase("--unknown")]
        [TestCase("--langs")]
        public void GivenAnUnknownArgument_WhenRunningTheProgram_ThenAnArgumentExceptionIsThrown(string argumentName)
        {
            string[] arguments = TestArguments.With(
                TestArguments.Build(temporaryDirectory.DirectoryPath, BuildRequest()),
                argumentName,
                "value");

            Assert.That(
                () => ModBuildRunner.Run(arguments),
                Throws.TypeOf<ArgumentException>().With.Message.Contains("Unknown argument"));
        }

        [TestCase("Minecraft")]
        [TestCase("Imperator Rome")]
        [TestCase("")]
        public void GivenAnUnsupportedGame_WhenRunningTheProgram_ThenANotImplementedExceptionIsThrown(string game)
        {
            ModBuildRequest request = BuildRequest();
            request.Game = game;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<NotImplementedException>());
        }

        [Test]
        public void GivenAMissingLanguageStore_WhenRunningTheProgram_ThenAFileNotFoundExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest();
            request.LanguageStorePath = TestPaths.MissingStoreFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<FileNotFoundException>());
        }

        [Test]
        public void GivenAMissingLocationStore_WhenRunningTheProgram_ThenAFileNotFoundExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest();
            request.LocationStorePath = TestPaths.MissingStoreFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<FileNotFoundException>());
        }

        [Test]
        public void GivenAMalformedLanguageStore_WhenRunningTheProgram_ThenAnInvalidOperationExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest();
            request.LanguageStorePath = TestPaths.MalformedLanguagesFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void GivenDuplicateLanguageIdentifiers_WhenRunningTheProgram_ThenADuplicateEntityExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest();
            request.LanguageStorePath = TestPaths.DuplicateLanguagesFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<DuplicateEntityException>());
        }

        [Test]
        public void GivenDuplicateLocationIdentifiers_WhenRunningTheProgram_ThenADuplicateEntityExceptionIsThrown()
        {
            ModBuildRequest request = BuildRequest();
            request.LocationStorePath = TestPaths.DuplicateLocationsFilePath;

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.TypeOf<DuplicateEntityException>());
        }

        [Test]
        public void GivenAnUnwritableOutputPath_WhenRunningTheProgram_ThenADirectoryNotFoundExceptionIsThrown()
        {
            string blockingFilePath = Path.Combine(temporaryDirectory.DirectoryPath, "blocking-file");
            File.WriteAllText(blockingFilePath, "not a directory");

            Assert.That(
                () => ModBuildRunner.Run(blockingFilePath, BuildRequest()),
                Throws.TypeOf<DirectoryNotFoundException>());
        }

        [Test]
        public void GivenAMissingOutputDirectory_WhenRunningTheProgram_ThenTheOutputTreeIsCreated()
        {
            string outputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "nested", "deeper");

            ModBuildRunner.Run(outputDirectoryPath, BuildRequest());

            Assert.That(
                Directory.Exists(Path.Combine(outputDirectoryPath, "CK2", "more-cultural-names")),
                Is.True);
        }

        [Test]
        public void GivenValidArguments_WhenRunningTheProgram_ThenTheServiceProviderIsReplacedAndDisposed()
        {
            string[] arguments = TestArguments.Build(temporaryDirectory.DirectoryPath, BuildRequest());

            ModBuildRunner.Run(arguments);

            Assert.Multiple(() =>
            {
                Assert.That(Program.ServiceProvider, Is.Not.Null);
                Assert.That(
                    () => Program.ServiceProvider.GetService(typeof(object)),
                    Throws.TypeOf<ObjectDisposedException>(),
                    "The previous service provider must be disposed after every run.");
            });
        }

        [Test]
        public void GivenTheSameInputs_WhenBuildingTwice_ThenTheGeneratedFilesAreIdentical()
        {
            string firstOutputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "first");
            string secondOutputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "second");
            ModBuildRequest request = BuildRequest();
            request.Game = "CK3";
            request.LandedTitlesFilePath = TestPaths.Ck3LandedTitlesFilePath;
            request.LandedTitlesFileName = "00_landed_titles.txt";

            ModBuildRunner.Run(firstOutputDirectoryPath, request);
            ModBuildRunner.Run(secondOutputDirectoryPath, request);

            string firstMainDirectoryPath = Path.Combine(firstOutputDirectoryPath, "CK3", "more-cultural-names");
            string secondMainDirectoryPath = Path.Combine(secondOutputDirectoryPath, "CK3", "more-cultural-names");

            Assert.That(
                GeneratedFiles.FindDifferences(firstMainDirectoryPath, secondMainDirectoryPath),
                Is.Empty);
        }

        private static ModBuildRequest BuildRequest()
            => new()
            {
                Game = "CK2",
                LandedTitlesFilePath = TestPaths.Ck2LandedTitlesFilePath,
                LandedTitlesFileName = "00_landed_titles.txt"
            };
    }
}
