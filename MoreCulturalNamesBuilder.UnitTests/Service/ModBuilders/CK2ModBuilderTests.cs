using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
    public sealed class CK2ModBuilderTests
    {
        private static string Game => "CK2";
        private static string ModId => "more-cultural-names";

        private Mock<ILocalisationFetcher> localisationFetcher = null!;
        private Mock<INameNormaliser> nameNormaliser = null!;
        private Mock<IFileRepository<LanguageEntity>> languageRepository = null!;
        private Mock<IFileRepository<LocationEntity>> locationRepository = null!;
        private TemporaryDirectory temporaryDirectory = null!;
        private CK2ModBuilder modBuilder = null!;
        private string landedTitlesFilePath = null!;

        [SetUp]
        public void SetUp()
        {
            temporaryDirectory = new(nameof(CK2ModBuilderTests));
            landedTitlesFilePath = Path.Combine(temporaryDirectory.DirectoryPath, "landed_titles.txt");

            localisationFetcher = new();
            nameNormaliser = new();
            languageRepository = new();
            locationRepository = new();

            nameNormaliser
                .Setup(normaliser => normaliser.ToWindows1252(It.IsAny<string>()))
                .Returns((string value) => value);

            ConfigureRepositories(
                [
                    BuildLanguage("Romanian", "romanian"),
                    BuildLanguage("English", "english"),
                    BuildLanguage("French", "french")
                ],
                [
                    BuildLocation("Cluj-Napoca", "c_cluj", "Romanian"),
                    BuildLocation("Oradea", "c_oradea", "Romanian"),
                    BuildLocation("Dezmir", "c_dezmir", "Romanian"),
                    BuildLocation("Newport", "c_newport", "English")
                ]);
            ConfigureLocalisations(new Dictionary<string, IEnumerable<Localisation>>
            {
                ["c_cluj"] =
                [
                    BuildLocalisation("c_cluj", "Romanian", "romanian", "Cluj-Napoca", "Clujean", "Praise the Sun!"),
                    BuildLocalisation("c_cluj", "English", "english", "Newport", null, null),
                    BuildLocalisation("c_cluj", "French", "french", "Solaire", "Solaire", " ")
                ],
                ["c_oradea"] = [],
                ["c_dezmir"] = [],
                ["c_newport"] = []
            });

            File.WriteAllText(
                landedTitlesFilePath,
                "c_cluj={ romanian=\"Original\" } # Remove this\r\n" +
                "allow = {\n" +
                "c_oradea = {\n" +
                "}\n" +
                "c_dezmir = {\n" +
                "has_holder = yes\n" +
                "}\n" +
                "c_newport = { }\n" +
                "not_a_title = {\n" +
                "}\n");

            Settings settings = BuildSettings(true, "A Game of Thrones");
            modBuilder = CreateModBuilder(settings);
        }

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenVerboseLocalisationsAndADependency_WhenBuilding_ThenAllCK2FilesContainTheExpectedContent()
        {
            modBuilder.Build();

            string outputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, Game);
            string mainDirectoryPath = Path.Combine(outputDirectoryPath, ModId);
            string descriptorFilePath = Path.Combine(outputDirectoryPath, $"{ModId}.mod");
            string landedTitlesOutputPath = Path.Combine(
                mainDirectoryPath,
                "common",
                "landed_titles",
                "00_landed_titles.txt");
            string localisationsFilePath = Path.Combine(
                mainDirectoryPath,
                "localisation",
                $"000_{ModId}_landed_titles.csv");

            string descriptor = File.ReadAllText(descriptorFilePath);
            string landedTitles = File.ReadAllText(
                landedTitlesOutputPath,
                Encoding.GetEncoding("windows-1252"));
            string localisations = File.ReadAllText(
                localisationsFilePath,
                Encoding.GetEncoding("windows-1252"));

            Assert.Multiple(() =>
            {
                Assert.That(descriptor, Does.Contain("name = \"More Cultural Names\""));
                Assert.That(descriptor, Does.Contain("dependencies = { \"A Game of Thrones\" }"));
                Assert.That(descriptor, Does.Contain($"path = \"mod/{ModId}\""));
                Assert.That(landedTitles, Does.Contain("romanian = \"Cluj-Napoca\" # Language=Romanian # Praise the Sun!"));
                Assert.That(landedTitles, Does.Contain("english = \"Newport\" # Language=English"));
                Assert.That(landedTitles, Does.Not.Contain("Original"));
                Assert.That(landedTitles, Does.Not.Contain("\r"));
                Assert.That(landedTitles, Does.Not.Contain("\t"));
                Assert.That(localisations, Does.Contain("c_cluj;Cluj-Napoca;Cluj-Napoca;Cluj-Napoca;;Cluj-Napoca;;;;;;;;;x"));
                Assert.That(localisations, Does.Contain("c_cluj_adj_romanian;Clujean;Clujean;Clujean;;Clujean;;;;;;;;;x"));
                Assert.That(localisations, Does.Contain("c_cluj_adj;Clujean;Clujean;Clujean;;Clujean;;;;;;;;;x"));
                Assert.That(localisations, Does.Contain("c_cluj_adj_french;Solaire;Solaire;Solaire;;Solaire;;;;;;;;;x"));
                Assert.That(localisations, Does.Not.Contain("c_cluj_adj_english"));
            });
        }

        [Test]
        public void GivenNoGeneratedLocalisationLines_WhenBuilding_ThenTheOptionalLocalisationFileIsNotCreated()
        {
            ConfigureRepositories(
                [BuildLanguage("English", "english")],
                [BuildLocation("Cluj-Napoca", "c_cluj", "Romanian")]);
            ConfigureLocalisations(new Dictionary<string, IEnumerable<Localisation>>
            {
                ["c_cluj"] =
                [BuildLocalisation("c_cluj", "English", "english", "Cluj-Napoca", " ", " ")]
            });
            Settings settings = BuildSettings(false, null);
            modBuilder = CreateModBuilder(settings);
            File.WriteAllText(landedTitlesFilePath, "c_cluj = {\n}\n");

            modBuilder.Build();

            string outputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, Game);
            string descriptorFilePath = Path.Combine(outputDirectoryPath, $"{ModId}.mod");
            string localisationsFilePath = Path.Combine(
                outputDirectoryPath,
                ModId,
                "localisation",
                $"000_{ModId}_landed_titles.csv");
            string landedTitlesOutputPath = Path.Combine(
                outputDirectoryPath,
                ModId,
                "common",
                "landed_titles",
                "00_landed_titles.txt");

            string descriptor = File.ReadAllText(descriptorFilePath);
            string landedTitles = File.ReadAllText(
                landedTitlesOutputPath,
                Encoding.GetEncoding("windows-1252"));

            Assert.Multiple(() =>
            {
                Assert.That(descriptor, Does.Not.Contain("dependencies"));
                Assert.That(landedTitles, Does.Contain("english = \"Cluj-Napoca\""));
                Assert.That(landedTitles, Does.Not.Contain("Language="));
                Assert.That(File.Exists(localisationsFilePath), Is.False);
                localisationFetcher.Verify(
                    fetcher => fetcher.GetGameLocationLocalisations("c_cluj", Game),
                    Times.Once());
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
                .Returns((string locationGameId, string game) =>
                {
                    if (localisations.TryGetValue(locationGameId, out IEnumerable<Localisation> values))
                    {
                        return values;
                    }

                    return [];
                });

        private Settings BuildSettings(bool areVerboseCommentsEnabled, string dependency)
            => SettingsTestFactory.Create(
                Game,
                temporaryDirectory.DirectoryPath,
                landedTitlesFilePath,
                "00_landed_titles.txt",
                areVerboseCommentsEnabled,
                dependency);

        private CK2ModBuilder CreateModBuilder(Settings settings)
            => new(
                localisationFetcher.Object,
                nameNormaliser.Object,
                languageRepository.Object,
                locationRepository.Object,
                settings);

        private static LanguageEntity BuildLanguage(string languageId, string languageGameId)
            => new()
            {
                Id = languageId,
                Code = null,
                GameIds =
                [
                    new(Game, languageGameId),
                    new("CK3", $"name_list_{languageGameId}")
                ],
                FallbackLanguages = []
            };

        private static LocationEntity BuildLocation(
            string locationId,
            string locationGameId,
            string defaultNameLanguageId)
            => new()
            {
                Id = locationId,
                GeoNamesId = null,
                GameIds =
                [
                    new(Game, locationGameId)
                    {
                        Type = "county",
                        DefaultNameLanguageId = defaultNameLanguageId
                    },
                    new("CK3", locationGameId)
                ],
                FallbackLocations = [],
                Names = []
            };

        private static Localisation BuildLocalisation(
            string gameId,
            string languageId,
            string languageGameId,
            string name,
            string adjective,
            string comment)
            => new()
            {
                Id = gameId,
                GameId = gameId,
                LanguageId = languageId,
                LanguageGameId = languageGameId,
                Name = name,
                Adjective = adjective,
                Comment = comment
            };
    }
}