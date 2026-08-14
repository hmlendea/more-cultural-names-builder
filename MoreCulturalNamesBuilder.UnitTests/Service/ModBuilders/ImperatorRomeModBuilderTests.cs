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
    public sealed class ImperatorRomeModBuilderTests
    {
        private static string Game => "IR";
        private static string ModId => "more-cultural-names";

        private Mock<ILocalisationFetcher> localisationFetcher = null!;
        private Mock<INameNormaliser> nameNormaliser = null!;
        private Mock<IFileRepository<LanguageEntity>> languageRepository = null!;
        private Mock<IFileRepository<LocationEntity>> locationRepository = null!;
        private TemporaryDirectory temporaryDirectory = null!;
        private ImperatorRomeModBuilder modBuilder = null!;

        [SetUp]
        public void SetUp()
        {
            temporaryDirectory = new(nameof(ImperatorRomeModBuilderTests));
            localisationFetcher = new();
            nameNormaliser = new();
            languageRepository = new();
            locationRepository = new();

            nameNormaliser
                .Setup(normaliser => normaliser.ToImperatorRomeCharset(It.IsAny<string>()))
                .Returns((string value) => $"IR {value}");

            LanguageEntity romanian = ModBuilderTestDataFactory.CreateLanguage(Game, "Romanian", "romanian");
            romanian.GameIds.Add(new("CK3", "romanian"));
            LanguageEntity english = ModBuilderTestDataFactory.CreateLanguage(Game, "English", "english");
            LocationEntity cluj = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Cluj-Napoca",
                "42",
                "Province",
                "Romanian");
            cluj.GameIds.Add(new("CK3", "c_cluj"));
            LocationEntity oradea = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Oradea",
                "4",
                "Province",
                "English");
            LocationEntity dezmir = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Dezmir",
                "8",
                "Province",
                null);

            ConfigureRepositories([romanian, english], [cluj, oradea, dezmir]);
            ConfigureLocalisations();
            modBuilder = CreateModBuilder(true);
        }

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenVariedProvinceLocalisations_WhenBuilding_ThenAllImperatorRomeFilesContainTheExpectedContent()
        {
            modBuilder.Build();

            string outputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, Game);
            string mainDirectoryPath = Path.Combine(outputDirectoryPath, ModId);
            string provinceNamesDirectoryPath = Path.Combine(mainDirectoryPath, "common", "province_names");
            string romanianDataPath = Path.Combine(provinceNamesDirectoryPath, "romanian.txt");
            string englishDataPath = Path.Combine(provinceNamesDirectoryPath, "english.txt");
            string localisationPath = Path.Combine(
                mainDirectoryPath,
                "localization",
                $"{ModId}_provincenames_l_english.yml");
            string mainDescriptorPath = Path.Combine(outputDirectoryPath, $"{ModId}.mod");
            string innerDescriptorPath = Path.Combine(mainDirectoryPath, "descriptor.mod");

            string romanianData = File.ReadAllText(romanianDataPath);
            string englishData = File.ReadAllText(englishDataPath);
            string localisations = File.ReadAllText(localisationPath);
            string mainDescriptor = File.ReadAllText(mainDescriptorPath);
            string innerDescriptor = File.ReadAllText(innerDescriptorPath);

            Assert.Multiple(() =>
            {
                Assert.That(romanianData, Does.Contain("4 = PROV4_romanian # IR Oradea # Language=Romanian"));
                Assert.That(romanianData, Does.Contain("8 = PROV8_romanian # IR Dezmir # Language=Romanian"));
                Assert.That(romanianData, Does.Contain("42 = PROV42_romanian # IR Cluj-Napoca # Language=Romanian # Praise the Sun!"));
                Assert.That(romanianData.IndexOf("4 =", StringComparison.Ordinal), Is.LessThan(romanianData.IndexOf("8 =", StringComparison.Ordinal)));
                Assert.That(romanianData.IndexOf("8 =", StringComparison.Ordinal), Is.LessThan(romanianData.IndexOf("42 =", StringComparison.Ordinal)));
                Assert.That(englishData, Does.Contain("42 = PROV42_english # IR Newport # Language=English"));
                Assert.That(englishData, Does.Not.Contain("PROV4_english"));
                Assert.That(englishData, Does.Not.Contain("PROV8_english"));
                Assert.That(localisations, Does.StartWith("l_english:"));
                Assert.That(localisations, Does.Contain(" PROV42:0 \"IR Cluj-Napoca\" # Language=Romanian # Praise the Sun!"));
                Assert.That(localisations, Does.Contain(" PROV42_english:0 \"IR Newport\" # Language=English"));
                Assert.That(localisations, Does.Contain(" PROV4_romanian:0 \"IR Oradea\" # Language=Romanian"));
                Assert.That(localisations, Does.Not.Contain(" PROV4:0"));
                Assert.That(localisations, Does.Not.Contain(" PROV8:0"));
                Assert.That(Directory.GetFiles(Path.Combine(mainDirectoryPath, "localization")), Has.Length.EqualTo(4));
                Assert.That(mainDescriptor, Does.Contain($"path=\"mod/{ModId}\""));
                Assert.That(innerDescriptor, Does.Not.Contain("path="));
            });
        }

        [Test]
        public void GivenNonVerboseLocalisationsWithoutComments_WhenBuilding_ThenNoCommentsAreGenerated()
        {
            LanguageEntity romanian = ModBuilderTestDataFactory.CreateLanguage(Game, "Romanian", "romanian");
            LocationEntity cluj = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Cluj-Napoca",
                "42",
                "Province",
                "Romanian");
            Localisation localisation = ModBuilderTestDataFactory.CreateLocalisation(
                "Cluj-Napoca",
                "42",
                "Romanian",
                "romanian",
                "Cluj-Napoca",
                null,
                null);
            ConfigureRepositories([romanian], [cluj]);
            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("42", Game))
                .Returns([localisation]);
            modBuilder = CreateModBuilder(false);

            modBuilder.Build();

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, Game, ModId);
            string data = File.ReadAllText(Path.Combine(
                mainDirectoryPath,
                "common",
                "province_names",
                "romanian.txt"));
            string localisations = File.ReadAllText(Path.Combine(
                mainDirectoryPath,
                "localization",
                $"{ModId}_provincenames_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(data, Does.Contain("42 = PROV42_romanian # IR Cluj-Napoca"));
                Assert.That(data, Does.Not.Contain("Language="));
                Assert.That(localisations, Does.Contain(" PROV42:0 \"IR Cluj-Napoca\""));
                Assert.That(localisations, Does.Not.Contain("#"));
            });
        }

        [Test]
        public void GivenNoLocations_WhenBuilding_ThenHeaderOnlyLocalisationAndEmptyDataFilesAreGenerated()
        {
            LanguageEntity romanian = ModBuilderTestDataFactory.CreateLanguage(Game, "Romanian", "romanian");
            ConfigureRepositories([romanian], []);
            modBuilder = CreateModBuilder(false);

            modBuilder.Build();

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, Game, ModId);
            string data = File.ReadAllText(Path.Combine(
                mainDirectoryPath,
                "common",
                "province_names",
                "romanian.txt"));
            string localisations = File.ReadAllText(Path.Combine(
                mainDirectoryPath,
                "localization",
                $"{ModId}_provincenames_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(data, Is.EqualTo("romanian = {" + Environment.NewLine + "}"));
                Assert.That(localisations, Is.EqualTo("l_english:" + Environment.NewLine));
            });
        }

        [TestCase("Cluj-Napoca", typeof(FormatException))]
        [TestCase("", typeof(FormatException))]
        [TestCase(" ", typeof(FormatException))]
        [TestCase("3.14", typeof(FormatException))]
        [TestCase("2147483648", typeof(OverflowException))]
        public void GivenANonIntegerProvinceIdentifier_WhenBuilding_ThenAnAggregateExceptionContainsTheParseFailure(
            string locationGameId,
            Type expectedExceptionType)
        {
            LocationEntity location = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Cluj-Napoca",
                locationGameId,
                "Province",
                "Romanian");
            LanguageEntity language = ModBuilderTestDataFactory.CreateLanguage(Game, "Romanian", "romanian");
            ConfigureRepositories([language], [location]);
            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations(locationGameId, Game))
                .Returns([]);
            modBuilder = CreateModBuilder(false);

            Assert.That(
                () => modBuilder.Build(),
                Throws.TypeOf<AggregateException>()
                    .With.InnerException.TypeOf(expectedExceptionType));
        }

        [Test]
        public void GivenDuplicateCultureLocalisations_WhenBuilding_ThenAnAggregateExceptionIsThrown()
        {
            LocationEntity location = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Cluj-Napoca",
                "42",
                "Province",
                "Romanian");
            Localisation localisation = ModBuilderTestDataFactory.CreateLocalisation(
                "Cluj-Napoca",
                "42",
                "Romanian",
                "romanian",
                "Cluj-Napoca",
                null,
                null);
            ConfigureRepositories([], [location]);
            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("42", Game))
                .Returns([localisation, localisation]);
            modBuilder = CreateModBuilder(false);

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
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("42", Game))
                .Returns(
                [
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Cluj-Napoca",
                        "42",
                        "Romanian",
                        "romanian",
                        "Cluj-Napoca",
                        null,
                        "Praise the Sun!"),
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Cluj-Napoca",
                        "42",
                        "English",
                        "english",
                        "Newport",
                        null,
                        null)
                ]);
            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("4", Game))
                .Returns(
                [
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Oradea",
                        "4",
                        "Romanian",
                        "romanian",
                        "Oradea",
                        null,
                        " ")
                ]);
            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("8", Game))
                .Returns(
                [
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Dezmir",
                        "8",
                        "Romanian",
                        "romanian",
                        "Dezmir",
                        null,
                        null)
                ]);
        }

        private ImperatorRomeModBuilder CreateModBuilder(bool areVerboseCommentsEnabled)
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