using System;
using System.Collections.Generic;
using System.IO;

using Moq;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service;
using MoreCulturalNamesBuilder.Service.ModBuilders;
using MoreCulturalNamesBuilder.UnitTests.Configuration;
using MoreCulturalNamesBuilder.UnitTests.TestInfrastructure;

using NuciDAL.Repositories;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.UnitTests.Service.ModBuilders
{
    [TestFixture]
    public sealed class EuropaUniversalis5ModBuilderTests
    {
        private Mock<ILocalisationFetcher> localisationFetcher = null!;
        private Mock<INameNormaliser> nameNormaliser = null!;
        private Mock<IFileRepository<LanguageDataObject>> languageRepository = null!;
        private Mock<IFileRepository<LocationDataObject>> locationRepository = null!;
        private TemporaryDirectory temporaryDirectory = null!;
        private EuropaUniversalis5ModBuilder modBuilder = null!;

        private static string Game => "EU5";

        private static string ModId => "more-cultural-names";

        [SetUp]
        public void SetUp()
        {
            temporaryDirectory = new(nameof(EuropaUniversalis5ModBuilderTests));
            localisationFetcher = new();
            nameNormaliser = new();
            languageRepository = new();
            locationRepository = new();

            nameNormaliser
                .Setup(normaliser => normaliser.ToEuropaUniversalis5Charset(It.IsAny<string>()))
                .Returns((string value) => $"EU5 {value}");

            LanguageDataObject romanian = ModBuilderTestDataFactory.CreateLanguage(Game, "Romanian", "romanian_language");
            LanguageDataObject english = ModBuilderTestDataFactory.CreateLanguage(Game, "English", "english_language");
            LocationDataObject cluj = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Cluj-Napoca",
                "cluj",
                null,
                "Romanian");
            LocationDataObject oradea = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Oradea",
                "oradea",
                null,
                "Romanian");

            ConfigureRepositories([romanian, english], [cluj, oradea]);
            ConfigureLocalisations();
            modBuilder = CreateModBuilder(true);
        }

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenVariedLocationLocalisations_WhenBuilding_ThenTheMetadataAndLocalisationAreGenerated()
        {
            modBuilder.Build();

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, Game, ModId);
            string localisations = File.ReadAllText(Path.Combine(
                mainDirectoryPath,
                "main_menu",
                "localization",
                "english",
                "replace",
                $"{GetFilePrefix()}_location_names_l_english.yml"));
            string metadata = File.ReadAllText(Path.Combine(mainDirectoryPath, ".metadata", "metadata.json"));

            Assert.Multiple(() =>
            {
                Assert.That(localisations, Does.StartWith("l_english:"));
                Assert.That(localisations, Does.Contain(" cluj: \"EU5 Cluj-Napoca\""));
                Assert.That(localisations, Does.Contain(" cluj.romanian_language: \"EU5 Cluj-Napoca\""));
                Assert.That(localisations, Does.Contain(" cluj.english_language: \"EU5 Newport\""));
                Assert.That(localisations, Does.Not.Contain("oradea\""), "A location without localisations must not be emitted.");
                Assert.That(metadata, Does.Contain("\"game_id\": \"eu5\""));
                Assert.That(metadata, Does.Contain("\"supported_game_version\": \"1.12.*\""));
                Assert.That(Directory.GetFiles(mainDirectoryPath, "*.yml", SearchOption.AllDirectories), Has.Length.EqualTo(11));
            });
        }

        [Test]
        public void GivenVerboseCommentsAreDisabled_WhenBuilding_ThenLanguageAnnotationsAreOmitted()
        {
            modBuilder = CreateModBuilder(false);

            modBuilder.Build();

            string localisations = ReadLocalisation();

            Assert.That(localisations, Does.Not.Contain("Language="));
        }

        [Test]
        public void GivenNoLocations_WhenBuilding_ThenOnlyTheLocalisationHeaderIsGenerated()
        {
            LanguageDataObject romanian = ModBuilderTestDataFactory.CreateLanguage(Game, "Romanian", "romanian_language");
            ConfigureRepositories([romanian], []);
            modBuilder = CreateModBuilder(false);

            modBuilder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(ReadLocalisation(), Is.EqualTo("l_english:" + Environment.NewLine));
                Assert.That(
                    File.Exists(Path.Combine(temporaryDirectory.DirectoryPath, Game, ModId, ".metadata", "metadata.json")),
                    Is.True);
            });
        }

        [Test]
        public void GivenACustomGameVersion_WhenBuilding_ThenTheMetadataDeclaresIt()
        {
            modBuilder = CreateModBuilder(true, "1.13.1");

            modBuilder.Build();

            Assert.That(ReadMetadata(), Does.Contain("\"supported_game_version\": \"1.13.1\""));
        }

        private void ConfigureRepositories(
            IEnumerable<LanguageDataObject> languages,
            IEnumerable<LocationDataObject> locations)
        {
            languageRepository.Setup(repository => repository.GetAll()).Returns(languages);
            locationRepository.Setup(repository => repository.GetAll()).Returns(locations);
        }

        private void ConfigureLocalisations()
        {
            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("cluj", Game))
                .Returns(
                [
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Cluj-Napoca",
                        "cluj",
                        "Romanian",
                        "romanian_language",
                        "Cluj-Napoca",
                        null,
                        null),
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Cluj-Napoca",
                        "cluj",
                        "English",
                        "english_language",
                        "Newport",
                        null,
                        null)
                ]);

            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("oradea", Game))
                .Returns([]);
        }

        private string ReadLocalisation() =>
            File.ReadAllText(Path.Combine(
                temporaryDirectory.DirectoryPath,
                Game,
                ModId,
                "main_menu",
                "localization",
                "english",
                "replace",
                $"{GetFilePrefix()}_location_names_l_english.yml"));

        private string ReadMetadata() =>
            File.ReadAllText(Path.Combine(
                temporaryDirectory.DirectoryPath,
                Game,
                ModId,
                ".metadata",
                "metadata.json"));

        private EuropaUniversalis5ModBuilder CreateModBuilder(bool areVerboseCommentsEnabled)
            => CreateModBuilder(areVerboseCommentsEnabled, "1.12.*");

        private EuropaUniversalis5ModBuilder CreateModBuilder(
            bool areVerboseCommentsEnabled,
            string gameVersion)
        {
            Settings settings = SettingsTestFactory.Create(
                Game,
                temporaryDirectory.DirectoryPath,
                null,
                null,
                areVerboseCommentsEnabled,
                null,
                gameVersion);

            return new(
                localisationFetcher.Object,
                nameNormaliser.Object,
                languageRepository.Object,
                locationRepository.Object,
                settings);
        }

        private static string GetFilePrefix() => ModId.Replace('-', '_');
    }
}
