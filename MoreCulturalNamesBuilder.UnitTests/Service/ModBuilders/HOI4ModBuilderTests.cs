using System;
using System.Collections.Generic;
using System.IO;

using Moq;

using NuciDAL.Repositories;

using NUnit.Framework;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service;
using MoreCulturalNamesBuilder.Service.ModBuilders;
using MoreCulturalNamesBuilder.Service.Models;
using MoreCulturalNamesBuilder.UnitTests.Configuration;
using MoreCulturalNamesBuilder.UnitTests.TestInfrastructure;

namespace MoreCulturalNamesBuilder.UnitTests.Service.ModBuilders
{
    [TestFixture]
    public sealed class HOI4ModBuilderTests
    {
        private static string Game => "HOI4";
        private static string ModId => "more-cultural-names";

        private Mock<ILocalisationFetcher> localisationFetcher = null!;
        private Mock<INameNormaliser> nameNormaliser = null!;
        private Mock<IFileRepository<LanguageEntity>> languageRepository = null!;
        private Mock<IFileRepository<LocationEntity>> locationRepository = null!;
        private TemporaryDirectory temporaryDirectory = null!;
        private HOI4ModBuilder modBuilder = null!;

        [SetUp]
        public void SetUp()
        {
            temporaryDirectory = new(nameof(HOI4ModBuilderTests));
            localisationFetcher = new();
            nameNormaliser = new();
            languageRepository = new();
            locationRepository = new();

            nameNormaliser
                .Setup(normaliser => normaliser.ToHOI4StateCharset(It.IsAny<string>()))
                .Returns((string value) => $"State {value}");
            nameNormaliser
                .Setup(normaliser => normaliser.ToHOI4CityCharset(It.IsAny<string>()))
                .Returns((string value) => $"City {value}");

            LanguageEntity language = ModBuilderTestDataFactory.CreateLanguage(Game, "Romanian", "romanian");
            language.GameIds.Add(new("CK3", "romanian"));
            LocationEntity firstState = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Romania",
                "42",
                "State",
                "Romanian");
            LocationEntity secondState = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Scotland",
                "4",
                "State",
                "English");
            LocationEntity firstCity = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Cluj-Napoca",
                "613",
                "City",
                "Romanian");
            LocationEntity secondCity = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Newport",
                "8",
                "City",
                "English");
            LocationEntity otherType = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Oradea",
                "16",
                "Province",
                "Romanian");
            LocationEntity otherGame = ModBuilderTestDataFactory.CreateLocation(
                "CK3",
                "Dezmir",
                "32",
                "State",
                "Romanian");

            ConfigureRepositories(
                [language],
                [firstState, secondState, firstCity, secondCity, otherType, otherGame]);
            ConfigureLocalisations();
            modBuilder = CreateModBuilder();
        }

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenStatesAndCities_WhenBuilding_ThenEveryHOI4LocalisationAndDescriptorFileIsGenerated()
        {
            modBuilder.Build();

            string outputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, Game);
            string mainDirectoryPath = Path.Combine(outputDirectoryPath, ModId);
            string localisationDirectoryPath = Path.Combine(mainDirectoryPath, "localisation");
            string englishLocalisationPath = Path.Combine(
                localisationDirectoryPath,
                "english",
                $"zzz999_{ModId}_l_english.yml");
            string mainDescriptorPath = Path.Combine(outputDirectoryPath, $"{ModId}.mod");
            string innerDescriptorPath = Path.Combine(mainDirectoryPath, "descriptor.mod");

            string localisations = File.ReadAllText(englishLocalisationPath);
            string mainDescriptor = File.ReadAllText(mainDescriptorPath);
            string innerDescriptor = File.ReadAllText(innerDescriptorPath);

            Assert.Multiple(() =>
            {
                Assert.That(localisations, Does.StartWith("l_english:"));
                Assert.That(localisations, Does.Contain(" english_STATE_4:0 \"State Scotland\""));
                Assert.That(localisations, Does.Contain(" romanian_STATE_42:0 \"State Romania\""));
                Assert.That(localisations, Does.Contain(" english_VICTORY_POINTS_8:0 \"City Newport\""));
                Assert.That(localisations, Does.Contain(" romanian_VICTORY_POINTS_613:0 \"City Cluj-Napoca\""));
                Assert.That(localisations, Does.Not.Contain("Oradea"));
                Assert.That(localisations, Does.Not.Contain("Dezmir"));
                Assert.That(Directory.GetDirectories(localisationDirectoryPath), Has.Length.EqualTo(5));
                Assert.That(mainDescriptor, Does.Contain($"path=\"mod/{ModId}\""));
                Assert.That(innerDescriptor, Does.Not.Contain("path="));
                Assert.That(innerDescriptor, Does.Contain("\"Historical\""));
            });
        }

        [Test]
        public void GivenNoLocations_WhenBuilding_ThenHeaderOnlyLocalisationFilesAreGenerated()
        {
            ConfigureRepositories([], []);
            modBuilder = CreateModBuilder();

            modBuilder.Build();

            string englishLocalisationPath = Path.Combine(
                temporaryDirectory.DirectoryPath,
                Game,
                ModId,
                "localisation",
                "english",
                $"zzz999_{ModId}_l_english.yml");

            Assert.That(File.ReadAllText(englishLocalisationPath), Is.EqualTo("l_english:" + Environment.NewLine));
        }

        [TestCase("Cluj-Napoca", typeof(FormatException))]
        [TestCase("", typeof(FormatException))]
        [TestCase(" ", typeof(FormatException))]
        [TestCase("3.14", typeof(FormatException))]
        [TestCase("2147483648", typeof(OverflowException))]
        public void GivenANonIntegerLocationIdentifier_WhenBuilding_ThenAFormatOrOverflowExceptionIsThrown(
            string locationGameId,
            Type expectedExceptionType)
        {
            LocationEntity location = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Cluj-Napoca",
                locationGameId,
                "City",
                "Romanian");
            ConfigureRepositories([], [location]);
            modBuilder = CreateModBuilder();

            Assert.That(
                () => modBuilder.Build(),
                Throws.TypeOf<AggregateException>()
                    .With.InnerException.TypeOf(expectedExceptionType));
        }

        [Test]
        public void GivenDuplicateLanguageLocalisations_WhenBuilding_ThenAnAggregateExceptionIsThrown()
        {
            LocationEntity location = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Cluj-Napoca",
                "613",
                "City",
                "Romanian");
            Localisation localisation = ModBuilderTestDataFactory.CreateLocalisation(
                "Cluj-Napoca",
                "613",
                "Romanian",
                "romanian",
                "Cluj-Napoca",
                null,
                null);
            ConfigureRepositories([], [location]);
            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("613", "City", Game))
                .Returns([localisation, localisation]);
            modBuilder = CreateModBuilder();

            Assert.That(() => modBuilder.Build(), Throws.TypeOf<AggregateException>());
        }

        private void ConfigureRepositories(
            IEnumerable<LanguageEntity> languages,
            IEnumerable<LocationEntity> locations)
        {
            languageRepository.Setup(repository => repository.GetAll()).Returns(languages);
            locationRepository.Setup(repository => repository.GetAll()).Returns(locations);
        }

        private void ConfigureLocalisations()
        {
            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("42", "State", Game))
                .Returns(
                [
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Romania",
                        "42",
                        "Romanian",
                        "romanian",
                        "Romania",
                        null,
                        null)
                ]);
            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("4", "State", Game))
                .Returns(
                [
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Scotland",
                        "4",
                        "English",
                        "english",
                        "Scotland",
                        null,
                        null)
                ]);
            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("613", "City", Game))
                .Returns(
                [
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Cluj-Napoca",
                        "613",
                        "Romanian",
                        "romanian",
                        "Cluj-Napoca",
                        null,
                        null)
                ]);
            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("8", "City", Game))
                .Returns(
                [
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Newport",
                        "8",
                        "English",
                        "english",
                        "Newport",
                        null,
                        null)
                ]);
        }

        private HOI4ModBuilder CreateModBuilder()
        {
            Settings settings = SettingsTestFactory.Create(Game, temporaryDirectory.DirectoryPath);

            return new(
                localisationFetcher.Object,
                nameNormaliser.Object,
                languageRepository.Object,
                locationRepository.Object,
                settings);
        }
    }
}