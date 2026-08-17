using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    public sealed class CK3ModBuilderTests
    {
        private static string Game => "CK3";
        private static string ModId => "more-cultural-names";

        private Mock<ILocalisationFetcher> localisationFetcher = null!;
        private Mock<INameNormaliser> nameNormaliser = null!;
        private Mock<IFileRepository<LanguageEntity>> languageRepository = null!;
        private Mock<IFileRepository<LocationEntity>> locationRepository = null!;
        private TemporaryDirectory temporaryDirectory = null!;
        private CK3ModBuilder modBuilder = null!;
        private string landedTitlesFilePath = null!;

        [SetUp]
        public void SetUp()
        {
            temporaryDirectory = new(nameof(CK3ModBuilderTests));
            landedTitlesFilePath = Path.Combine(temporaryDirectory.DirectoryPath, "landed_titles.txt");

            localisationFetcher = new();
            nameNormaliser = new();
            languageRepository = new();
            locationRepository = new();

            nameNormaliser
                .Setup(normaliser => normaliser.ToCK3Charset(It.IsAny<string>()))
                .Returns((string value) => value);

            LanguageEntity romanian = ModBuilderTestDataFactory.CreateLanguage(Game, "Romanian", "romanian");
            romanian.GameIds.Add(new("CK2", "romanian"));
            LanguageEntity english = ModBuilderTestDataFactory.CreateLanguage(Game, "English", "english");

            LocationEntity cluj = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Cluj-Napoca",
                "c_cluj",
                "county",
                "Romanian");
            cluj.GameIds.Add(new("CK2", "c_cluj"));
            LocationEntity oradea = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Oradea",
                "c_oradea",
                "county",
                " ");
            LocationEntity dezmir = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Dezmir",
                "c_dezmir",
                "county",
                "English");

            ConfigureRepositories([romanian, english], [cluj, oradea, dezmir]);
            ConfigureLocalisations(new Dictionary<string, IEnumerable<Localisation>>
            {
                ["c_cluj"] =
                [
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Cluj-Napoca",
                        "c_cluj",
                        "Romanian",
                        "romanian",
                        "Cluj-Napoca",
                        "Clujean",
                        "Praise the Sun!"),
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Cluj-Napoca",
                        "c_cluj",
                        "English",
                        "english",
                        "Newport",
                        null,
                        null)
                ],
                ["c_oradea"] =
                [
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Cluj-Napoca",
                        "c_oradea",
                        "Romanian",
                        "romanian",
                        "Cluj-Napoca",
                        "Clujean",
                        "Praise the Sun!")
                ],
                ["c_dezmir"] =
                [
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Dezmir",
                        "c_dezmir",
                        "Romanian",
                        "romanian",
                        "Dezmir",
                        "Dezmirian",
                        null)
                ]
            });

            File.WriteAllText(
                landedTitlesFilePath,
                "c_cluj = {\n" +
                "    name_list_romanian = original_name\n" +
                "}\n" +
                "allow = {\n" +
                "c_oradea = {\n" +
                "}\n" +
                "c_dezmir = {\n" +
                "has_holder = yes\n" +
                "}\n" +
                "c_newport = {\n" +
                "    cultural_names = {\n" +
                "    }\n" +
                "}\n");

            modBuilder = CreateModBuilder(BuildSettings(true));
        }

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenVariedLocalisations_WhenBuilding_ThenAllCK3FilesContainTheExpectedContent()
        {
            modBuilder.Build();

            string outputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, Game);
            string mainDirectoryPath = Path.Combine(outputDirectoryPath, ModId);
            string mainDescriptorPath = Path.Combine(outputDirectoryPath, $"{ModId}.mod");
            string innerDescriptorPath = Path.Combine(mainDirectoryPath, "descriptor.mod");
            string landedTitlesOutputPath = Path.Combine(
                mainDirectoryPath,
                "common",
                "landed_titles",
                "00_landed_titles.txt");
            string defaultLocalisationsPath = Path.Combine(
                mainDirectoryPath,
                "localization",
                $"{ModId}_titles_l_english.yml");
            string dynamicLocalisationsPath = Path.Combine(
                mainDirectoryPath,
                "localization",
                $"{ModId}_titles_cultural_names_l_english.yml");

            string mainDescriptor = File.ReadAllText(mainDescriptorPath);
            string innerDescriptor = File.ReadAllText(innerDescriptorPath);
            string landedTitles = File.ReadAllText(landedTitlesOutputPath);
            string defaultLocalisations = File.ReadAllText(defaultLocalisationsPath);
            string dynamicLocalisations = File.ReadAllText(dynamicLocalisationsPath);

            Assert.Multiple(() =>
            {
                Assert.That(mainDescriptor, Does.Contain($"path=\"mod/{ModId}\""));
                Assert.That(innerDescriptor, Does.Not.Contain("path="));
                Assert.That(innerDescriptor, Does.Contain("supported_version=\"1.12.*\""));
                Assert.That(landedTitles, Does.Contain("cultural_names = {"));
                Assert.That(landedTitles, Does.Contain("name_list_romanian = cn_Cluj-Napoca_romanian # Cluj-Napoca # Language=Romanian # Praise the Sun!"));
                Assert.That(landedTitles, Does.Not.Contain("original_name"));
                Assert.That(defaultLocalisations, Does.StartWith("l_english:"));
                Assert.That(defaultLocalisations, Does.Contain(" c_cluj:0 \"Cluj-Napoca\" # Language=Romanian # Praise the Sun!"));
                Assert.That(defaultLocalisations, Does.Contain(" c_cluj_adj:0 \"Clujean\" # Language=Romanian # Praise the Sun!"));
                Assert.That(defaultLocalisations, Does.Not.Contain("c_oradea"));
                Assert.That(defaultLocalisations, Does.Not.Contain("c_dezmir"));
                Assert.That(dynamicLocalisations, Does.Contain(" cn_Cluj-Napoca_romanian:0 \"Cluj-Napoca\""));
                Assert.That(dynamicLocalisations, Does.Contain(" cn_Cluj-Napoca_romanian_adj:0 \"Clujean\""));
                Assert.That(dynamicLocalisations, Does.Contain(" cn_Dezmir_romanian_adj:0 \"Dezmirian\""));
                Assert.That(Directory.GetFiles(Path.Combine(mainDirectoryPath, "localization")), Has.Length.EqualTo(8));
                Assert.That(
                    dynamicLocalisations.Split("cn_Cluj-Napoca_romanian:0").Length,
                    Is.EqualTo(2));
            });
        }

        [Test]
        public void GivenNonVerboseLocalisationsWithoutComments_WhenBuilding_ThenNoCommentsAreGenerated()
        {
            LanguageEntity romanian = ModBuilderTestDataFactory.CreateLanguage(Game, "Romanian", "romanian");
            LocationEntity cluj = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Cluj-Napoca",
                "c_cluj",
                "county",
                "Romanian");
            ConfigureRepositories([romanian], [cluj]);
            ConfigureLocalisations(new Dictionary<string, IEnumerable<Localisation>>
            {
                ["c_cluj"] =
                [
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Cluj-Napoca",
                        "c_cluj",
                        "Romanian",
                        "romanian",
                        "Cluj-Napoca",
                        null,
                        null)
                ]
            });
            File.WriteAllText(landedTitlesFilePath, "c_cluj = {\n}\n");
            modBuilder = CreateModBuilder(BuildSettings(false));

            modBuilder.Build();

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, Game, ModId);
            string landedTitles = File.ReadAllText(Path.Combine(
                mainDirectoryPath,
                "common",
                "landed_titles",
                "00_landed_titles.txt"));
            string defaultLocalisations = File.ReadAllText(Path.Combine(
                mainDirectoryPath,
                "localization",
                $"{ModId}_titles_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(landedTitles, Does.Contain("# Cluj-Napoca"));
                Assert.That(landedTitles, Does.Not.Contain("Language="));
                Assert.That(defaultLocalisations, Does.Contain(" c_cluj:0 \"Cluj-Napoca\""));
                Assert.That(defaultLocalisations, Does.Not.Contain("#"));
                Assert.That(defaultLocalisations, Does.Not.Contain("c_cluj_adj"));
            });
        }

        private void ConfigureRepositories(
            IEnumerable<LanguageEntity> languages,
            IEnumerable<LocationEntity> locations)
        {
            languageRepository.Setup(repository => repository.GetAll()).Returns(languages);
            locationRepository.Setup(repository => repository.GetAll()).Returns(locations);
        }

        private void ConfigureLocalisations(
            IDictionary<string, IEnumerable<Localisation>> localisations)
            => localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations(It.IsAny<string>(), Game))
                .Returns((string locationGameId, string game) => localisations[locationGameId]);

        private Settings BuildSettings(bool areVerboseCommentsEnabled)
            => SettingsTestFactory.Create(
                Game,
                temporaryDirectory.DirectoryPath,
                landedTitlesFilePath,
                "00_landed_titles.txt",
                areVerboseCommentsEnabled,
                null);

        private CK3ModBuilder CreateModBuilder(Settings settings)
            => new(
                localisationFetcher.Object,
                nameNormaliser.Object,
                languageRepository.Object,
                locationRepository.Object,
                settings);
    }
}