using System;
using System.Collections.Generic;
using System.IO;

using Moq;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service;
using MoreCulturalNamesBuilder.Service.ModBuilders;
using MoreCulturalNamesBuilder.Service.Models;
using MoreCulturalNamesBuilder.UnitTests.Configuration;
using MoreCulturalNamesBuilder.UnitTests.TestInfrastructure;

using NuciDAL.Repositories;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.UnitTests.Service.ModBuilders
{
    [TestFixture]
    [NonParallelizable]
    public sealed class Victoria3ModBuilderEdgeCaseTests
    {
        private Mock<ILocalisationFetcher> localisationFetcher = null!;
        private Mock<INameNormaliser> nameNormaliser = null!;
        private Mock<IFileRepository<LanguageDataObject>> languageRepository = null!;
        private Mock<IFileRepository<LocationDataObject>> locationRepository = null!;
        private TemporaryDirectory temporaryDirectory = null!;
        private List<LocationDataObject> storedLocations = null!;

        private static string Game => "Vic3";

        private static string MainDirectoryName => "more-cultural-names";

        [SetUp]
        public void SetUp()
        {
            temporaryDirectory = new(nameof(Victoria3ModBuilderEdgeCaseTests));
            localisationFetcher = new();
            nameNormaliser = new();
            languageRepository = new();
            locationRepository = new();
            storedLocations = [];

            nameNormaliser
                .Setup(normaliser => normaliser.ToVictoria3Charset(It.IsAny<string>()))
                .Returns((string value) => $"V3 {value}");
        }

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [TestCase("STATE_CLUJ")]
        [TestCase("STATE_CLUJ_outpost")]
        public void GivenAHubIdentifierWithoutAKnownSuffix_WhenBuilding_ThenNoHubNameIsAssigned(
            string hubGameId)
        {
            ConfigureStores(
                ["Romanian"],
                [CreateLocation("Cluj-Napoca", "STATE_CLUJ", "State")]);
            AddHub("Cluj-Napoca", hubGameId);
            ConfigureStateLocalisations("STATE_CLUJ", "Romanian");

            Build();

            Assert.That(ReadEffects(), Does.Not.Contain("set_hub_name = {"));
        }

        [Test]
        public void GivenAHubBelongingToTheSameState_WhenBuilding_ThenTheHubNameIsAssignedWithinTheCultureBlock()
        {
            ConfigureStores(
                ["Romanian"],
                [CreateLocation("Cluj-Napoca", "STATE_CLUJ", "State")]);
            AddHub("Cluj-Napoca", "STATE_CLUJ_city");
            AddHub("Cluj-Napoca", "STATE_CLUJ_city");
            ConfigureStateLocalisations("STATE_CLUJ", "Romanian");
            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("STATE_CLUJ_city", "Hub", Game))
                .Returns(
                [
                    CreateLocalisation("Cluj-Napoca", "STATE_CLUJ_city", "Romanian", "Cluj-Napoca")
                ]);

            Build();

            string effects = ReadEffects();

            Assert.Multiple(() =>
            {
                Assert.That(effects, Does.Contain("set_state_name = STATE_CLUJ_romanian"));
                Assert.That(effects, Does.Contain("set_hub_name = { type = city name = HUB_NAME_STATE_CLUJ_city_romanian }"));
            });
        }

        [Test]
        public void GivenAHubCultureThatTheStateDoesNotSupport_WhenBuilding_ThenNoCultureBlockIsEmittedForIt()
        {
            ConfigureStores(
                ["Romanian", "English"],
                [CreateLocation("Cluj-Napoca", "STATE_CLUJ", "State")]);
            AddHub("Cluj-Napoca", "STATE_CLUJ_city");
            ConfigureStateLocalisations("STATE_CLUJ", "Romanian");
            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("STATE_CLUJ_city", "Hub", Game))
                .Returns(
                [
                    CreateLocalisation("Newport", "STATE_CLUJ_city", "English", "Newport")
                ]);

            Build();

            string effects = ReadEffects();

            Assert.Multiple(() =>
            {
                Assert.That(effects, Does.Contain("set_state_name = STATE_CLUJ_romanian"));
                Assert.That(effects, Does.Not.Contain("country_has_primary_culture = cu:english"));
                Assert.That(effects, Does.Not.Contain("HUB_NAME_STATE_CLUJ_city_english"));
            });
        }

        [TestCase("CK2")]
        [TestCase("HOI4")]
        public void GivenLocationGameIdentifiersForAnotherGame_WhenBuilding_ThenTheyAreIgnored(
            string otherGame)
        {
            LocationDataObject location = new()
            {
                Id = "Cluj-Napoca",
                GeoNamesId = "681290",
                GameIds = [new(otherGame, "STATE_CLUJ") { Type = "State" }],
                FallbackLocations = [],
                Names = []
            };
            ConfigureStores(["Romanian"], [location]);

            Build();

            Assert.Multiple(() =>
            {
                Assert.That(ReadEffects(), Does.Not.Contain("state_name_assign = {"));
                Assert.That(ReadLocalisation(), Is.EqualTo("l_english:" + Environment.NewLine));
            });
        }

        [TestCase("1.12.*")]
        [TestCase("1.13.1")]
        public void GivenAGameVersion_WhenBuilding_ThenTheMetadataDeclaresIt(string gameVersion)
        {
            ConfigureStores(["Romanian"], []);
            Settings settings = BuildSettings(gameVersion, null);

            Build(settings);

            Assert.That(ReadMetadata(), Does.Contain($"\"supported_game_version\": \"{gameVersion}\""));
        }

        [TestCase("A Game of Thrones")]
        [TestCase("The Fallen Eagle")]
        public void GivenADependency_WhenBuilding_ThenTheMetadataDeclaresNoRelationships(string dependency)
        {
            ConfigureStores(["Romanian"], []);
            Settings settings = BuildSettings("1.12.*", dependency);

            Build(settings);

            Assert.That(ReadMetadata(), Does.Contain("\"relationships\": []"));
        }

        [Test]
        public void GivenACommentOnTheLocalisation_WhenVerboseCommentsAreDisabled_ThenOnlyTheCommentIsEmitted()
        {
            ConfigureStores(
                ["Romanian"],
                [CreateLocation("Cluj-Napoca", "STATE_CLUJ", "State")]);
            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("STATE_CLUJ", "State", Game))
                .Returns(
                [
                    CreateLocalisation("Cluj-Napoca", "STATE_CLUJ", "Romanian", "Cluj-Napoca", "Praise the Sun!")
                ]);

            Build(BuildSettings("1.12.*", null, false));

            Assert.Multiple(() =>
            {
                Assert.That(ReadLocalisation(), Does.Contain(" STATE_CLUJ_romanian: \"V3 Cluj-Napoca\" # Praise the Sun!"));
                Assert.That(ReadLocalisation(), Does.Not.Contain("Language="));
            });
        }

        [Test]
        public void GivenAStateWithoutLocalisations_WhenBuilding_ThenTheStateIsNotDispatched()
        {
            ConfigureStores(
                ["Romanian"],
                [
                    CreateLocation("Cluj-Napoca", "STATE_CLUJ", "State"),
                    CreateLocation("Oradea", "STATE_ORADEA", "State")
                ]);
            ConfigureStateLocalisations("STATE_CLUJ", "Romanian");
            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("STATE_ORADEA", "State", Game))
                .Returns([]);

            Build();

            string effects = ReadEffects();

            Assert.Multiple(() =>
            {
                Assert.That(effects, Does.Contain("more_cultural_names_STATE_CLUJ_state_name_assign = yes"));
                Assert.That(effects, Does.Not.Contain("STATE_ORADEA"));
            });
        }

        [Test]
        public void GivenMultipleLocalisationLocales_WhenBuilding_ThenEveryLocaleDirectoryContainsOneFile()
        {
            ConfigureStores(["Romanian"], []);

            Build();

            string localisationDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                Game,
                MainDirectoryName,
                "localization");

            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetDirectories(localisationDirectoryPath), Has.Length.EqualTo(11));
                Assert.That(
                    Directory.GetFiles(localisationDirectoryPath, "*.yml", SearchOption.AllDirectories),
                    Has.Length.EqualTo(11));
            });
        }

        [TestCase("A Game of Thrones")]
        [TestCase("The Fallen Eagle")]
        public void GivenADependency_WhenBuilding_ThenTheDescriptorFilesAreNotEmitted(string dependency)
        {
            ConfigureStores(["Romanian"], []);

            Build(BuildSettings("1.12.*", dependency));

            string mainDirectoryPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                Game,
                MainDirectoryName);

            Assert.Multiple(() =>
            {
                Assert.That(
                    File.Exists(Path.Combine(temporaryDirectory.DirectoryPath, Game, $"{MainDirectoryName}.mod")),
                    Is.False);
                Assert.That(File.Exists(Path.Combine(mainDirectoryPath, "descriptor.mod")), Is.False);
                Assert.That(
                    File.Exists(Path.Combine(mainDirectoryPath, ".metadata", "metadata.json")),
                    Is.True);
            });
        }

        private static LocationDataObject CreateLocation(
            string locationId,
            string locationGameId,
            string locationGameIdType)
            => new()
            {
                Id = locationId,
                GeoNamesId = "681290",
                GameIds = [new(Game, locationGameId) { Type = locationGameIdType }],
                FallbackLocations = [],
                Names = []
            };

        private static Localisation CreateLocalisation(
            string id,
            string gameId,
            string languageId,
            string name,
            string? comment)
            => new()
            {
                Id = id,
                GameId = gameId,
                LanguageId = languageId,
                LanguageGameId = languageId.ToLowerInvariant(),
                Name = name,
                Adjective = null,
                Comment = comment
            };

        private static Localisation CreateLocalisation(
            string id,
            string gameId,
            string languageId,
            string name)
            => CreateLocalisation(id, gameId, languageId, name, null);

        private void ConfigureStores(
            IEnumerable<string> languageIds,
            IEnumerable<LocationDataObject> locations)
        {
            List<LanguageDataObject> languages = [];

            foreach (string languageId in languageIds)
            {
                languages.Add(ModBuilderTestDataFactory.CreateLanguage(
                    Game,
                    languageId,
                    languageId.ToLowerInvariant()));
            }

            languageRepository.Setup(repository => repository.GetAll()).Returns(languages);
            storedLocations.AddRange(locations);
            locationRepository.Setup(repository => repository.GetAll()).Returns(storedLocations);
        }

        private void AddHub(string locationId, string hubGameId)
        {
            LocationDataObject? hubLocation = null;

            foreach (LocationDataObject location in storedLocations)
            {
                if (string.Equals(location.Id, locationId, StringComparison.Ordinal))
                {
                    hubLocation = location;
                }
            }

            hubLocation!.GameIds.Add(new(Game, hubGameId) { Type = "Hub" });
        }

        private void ConfigureStateLocalisations(string stateGameId, params string[] languageIds)
        {
            List<Localisation> localisations = [];

            foreach (string languageId in languageIds)
            {
                localisations.Add(CreateLocalisation(
                    "Cluj-Napoca",
                    stateGameId,
                    languageId,
                    "Cluj-Napoca"));
            }

            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations(stateGameId, "State", Game))
                .Returns(localisations);
        }

        private Settings BuildSettings(string gameVersion, string? dependency)
            => BuildSettings(gameVersion, dependency, true);

        private Settings BuildSettings(
            string gameVersion,
            string? dependency,
            bool areVerboseCommentsEnabled)
        {
            Settings settings = SettingsTestFactory.Create(
                Game,
                temporaryDirectory.DirectoryPath,
                null,
                null,
                areVerboseCommentsEnabled,
                dependency);

            settings.Mod.GameVersion = gameVersion;

            return settings;
        }

        private void Build() => Build(BuildSettings("1.12.*", null));

        private void Build(Settings settings)
            => new Victoria3ModBuilder(
                localisationFetcher.Object,
                nameNormaliser.Object,
                languageRepository.Object,
                locationRepository.Object,
                settings).Build();

        private string ReadEffects()
            => File.ReadAllText(Path.Combine(
                temporaryDirectory.DirectoryPath,
                Game,
                MainDirectoryName,
                "common",
                "scripted_effects",
                "more_cultural_names_state_names.txt"));

        private string ReadLocalisation()
            => File.ReadAllText(Path.Combine(
                temporaryDirectory.DirectoryPath,
                Game,
                MainDirectoryName,
                "localization",
                "english",
                "more_cultural_names_state_names_l_english.yml"));

        private string ReadMetadata()
            => File.ReadAllText(Path.Combine(
                temporaryDirectory.DirectoryPath,
                Game,
                MainDirectoryName,
                ".metadata",
                "metadata.json"));
    }
}
