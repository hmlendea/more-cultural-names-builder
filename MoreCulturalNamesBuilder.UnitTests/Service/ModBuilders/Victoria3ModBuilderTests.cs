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
    public sealed class Victoria3ModBuilderTests
    {
        private Mock<ILocalisationFetcher> localisationFetcher = null!;
        private Mock<INameNormaliser> nameNormaliser = null!;
        private Mock<IFileRepository<LanguageDataObject>> languageRepository = null!;
        private Mock<IFileRepository<LocationDataObject>> locationRepository = null!;
        private TemporaryDirectory temporaryDirectory = null!;
        private Victoria3ModBuilder modBuilder = null!;

        private static string Game => "Vic3";

        private static string ModId => "more-cultural-names";

        [SetUp]
        public void SetUp()
        {
            temporaryDirectory = new(nameof(Victoria3ModBuilderTests));
            localisationFetcher = new();
            nameNormaliser = new();
            languageRepository = new();
            locationRepository = new();

            nameNormaliser
                .Setup(normaliser => normaliser.ToVictoria3Charset(It.IsAny<string>()))
                .Returns((string value) => $"V3 {value}");

            LanguageDataObject romanian = ModBuilderTestDataFactory.CreateLanguage(Game, "Romanian", "romanian");
            LanguageDataObject english = ModBuilderTestDataFactory.CreateLanguage(Game, "English", "english");
            LocationDataObject cluj = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Cluj-Napoca",
                "STATE_CLUJ",
                "State",
                "Romanian");
            cluj.GameIds.Add(new(Game, "STATE_CLUJ_city") { Type = "Hub" });
            LocationDataObject oradea = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Oradea",
                "STATE_ORADEA",
                "State",
                "Romanian");

            ConfigureRepositories([romanian, english], [cluj, oradea]);
            ConfigureLocalisations();
            modBuilder = CreateModBuilder(true);
        }

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenVariedStateLocalisations_WhenBuilding_ThenTheScriptsAndLocalisationAreGenerated()
        {
            modBuilder.Build();

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, Game, ModId);
            string effects = File.ReadAllText(Path.Combine(
                mainDirectoryPath,
                "common",
                "scripted_effects",
                "more_cultural_names_state_names.txt"));
            string onActions = File.ReadAllText(Path.Combine(
                mainDirectoryPath,
                "common",
                "on_actions",
                "more_cultural_names_state_names.txt"));
            string localisations = File.ReadAllText(Path.Combine(
                mainDirectoryPath,
                "localization",
                "english",
                "more_cultural_names_state_names_l_english.yml"));
            string metadata = File.ReadAllText(Path.Combine(mainDirectoryPath, ".metadata", "metadata.json"));

            Assert.Multiple(() =>
            {
                Assert.That(effects, Does.Contain("more_cultural_names_STATE_CLUJ_state_name_assign = {"));
                Assert.That(effects, Does.Contain("limit = { owner = { country_has_primary_culture = cu:romanian } }"));
                Assert.That(effects, Does.Contain("set_state_name = STATE_CLUJ_romanian"));
                Assert.That(effects, Does.Contain("set_hub_name = { type = city name = HUB_NAME_STATE_CLUJ_city_romanian }"));
                Assert.That(effects, Does.Contain("reset_state_name = yes"));
                Assert.That(effects, Does.Not.Contain("STATE_ORADEA_state_name_assign = {"), "A state without localisations must not get an assignment block.");
                Assert.That(onActions, Does.Contain("on_state_owner_change = {"));
                Assert.That(onActions, Does.Contain("on_actions = { more_cultural_names_on_state_owner_change }"));
                Assert.That(onActions, Does.Contain("every_scope_state = {"));
                Assert.That(localisations, Does.StartWith("l_english:"));
                Assert.That(localisations, Does.Contain(" STATE_CLUJ_romanian: \"V3 Cluj-Napoca\""));
                Assert.That(localisations, Does.Contain(" HUB_NAME_STATE_CLUJ_city_romanian: \"V3 Cluj-Napoca\""));
                Assert.That(metadata, Does.Contain("\"game_id\": \"victoria3\""));
                Assert.That(metadata, Does.Contain("\"supported_game_version\": \"1.12.*\""));
                Assert.That(Directory.GetFiles(mainDirectoryPath, "*.yml", SearchOption.AllDirectories), Has.Length.EqualTo(11));
            });
        }

        [Test]
        public void GivenVerboseCommentsAreDisabled_WhenBuilding_ThenLanguageAnnotationsAreOmitted()
        {
            modBuilder = CreateModBuilder(false);

            modBuilder.Build();

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, Game, ModId);
            string effects = File.ReadAllText(Path.Combine(
                mainDirectoryPath,
                "common",
                "scripted_effects",
                "more_cultural_names_state_names.txt"));
            string localisations = File.ReadAllText(Path.Combine(
                mainDirectoryPath,
                "localization",
                "english",
                "more_cultural_names_state_names_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(effects, Does.Not.Contain("Language="));
                Assert.That(localisations, Does.Not.Contain("Language="));
            });
        }

        [Test]
        public void GivenNoLocations_WhenBuilding_ThenOnlyTheEvaluationAndDispatchEffectsAreGenerated()
        {
            LanguageDataObject romanian = ModBuilderTestDataFactory.CreateLanguage(Game, "Romanian", "romanian");
            ConfigureRepositories([romanian], []);
            modBuilder = CreateModBuilder(false);

            modBuilder.Build();

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, Game, ModId);
            string effects = File.ReadAllText(Path.Combine(
                mainDirectoryPath,
                "common",
                "scripted_effects",
                "more_cultural_names_state_names.txt"));
            string localisations = File.ReadAllText(Path.Combine(
                mainDirectoryPath,
                "localization",
                "english",
                "more_cultural_names_state_names_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(effects, Does.Contain("more_cultural_names_evaluate_and_assign_state_hub_dynamic_names = {"));
                Assert.That(effects, Does.Contain("more_cultural_names_assign_state_hub_dynamic_names = {"));
                Assert.That(effects, Does.Not.Contain("_state_name_assign = {"));
                Assert.That(localisations, Is.EqualTo("l_english:" + Environment.NewLine));
            });
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
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("STATE_CLUJ", "State", Game))
                .Returns(
                [
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Cluj-Napoca",
                        "STATE_CLUJ",
                        "Romanian",
                        "romanian",
                        "Cluj-Napoca",
                        null,
                        null),
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Cluj-Napoca",
                        "STATE_CLUJ",
                        "English",
                        "english",
                        "Newport",
                        null,
                        null)
                ]);
            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("STATE_CLUJ_city", "Hub", Game))
                .Returns(
                [
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Cluj-Napoca",
                        "STATE_CLUJ_city",
                        "Romanian",
                        "romanian",
                        "Cluj-Napoca",
                        null,
                        null)
                ]);
        }

        private Victoria3ModBuilder CreateModBuilder(bool areVerboseCommentsEnabled)
        {
            Settings settings = SettingsTestFactory.Create(
                Game,
                temporaryDirectory.DirectoryPath,
                null,
                null,
                areVerboseCommentsEnabled,
                null);

            return new(
                localisationFetcher.Object,
                nameNormaliser.Object,
                languageRepository.Object,
                locationRepository.Object,
                settings);
        }
    }
}
