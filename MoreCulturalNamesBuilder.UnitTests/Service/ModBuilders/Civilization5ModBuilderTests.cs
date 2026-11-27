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
    public sealed class Civilization5ModBuilderTests
    {
        private static readonly string[] expectedLocales =
        [
            "en_US",
            "DE_DE",
            "ES_ES",
            "FR_FR",
            "IT_IT",
            "JA_JP",
            "KO_KR",
            "PL_PL",
            "RU_RU",
            "ZH_Hant_HK",
        ];

        private Mock<ILocalisationFetcher> localisationFetcher = null!;
        private Mock<INameNormaliser> nameNormaliser = null!;
        private Mock<IFileRepository<LanguageDataObject>> languageRepository = null!;
        private Mock<IFileRepository<LocationDataObject>> locationRepository = null!;
        private TemporaryDirectory temporaryDirectory = null!;
        private Civilization5ModBuilder modBuilder = null!;

        private static string Game => "Civ5";

        private static string ModId => "more-cultural-names";

        [SetUp]
        public void SetUp()
        {
            temporaryDirectory = new(nameof(Civilization5ModBuilderTests));
            localisationFetcher = new();
            nameNormaliser = new();
            languageRepository = new();
            locationRepository = new();

            nameNormaliser
                .Setup(normaliser => normaliser.ToCivilization5Charset(It.IsAny<string>()))
                .Returns((string value) => $"Civ5 {value}");

            LanguageDataObject romanian = ModBuilderTestDataFactory.CreateLanguage(Game, "Romanian", "CIVILIZATION_ROME");
            LanguageDataObject english = ModBuilderTestDataFactory.CreateLanguage(Game, "English", "CIVILIZATION_ENGLAND");
            LocationDataObject cluj = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Cluj-Napoca",
                "TXT_KEY_CITY_NAME_CLUJ_NAPOCA",
                null,
                null);
            LocationDataObject oradea = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Oradea",
                "TXT_KEY_CITY_NAME_ORADEA",
                null,
                null);

            ConfigureRepositories([romanian, english], [cluj, oradea]);
            ConfigureLocalisations();
            modBuilder = CreateModBuilder(true);
        }

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenCivilizationLocalisations_WhenBuilding_ThenTheDescriptorLocalisationAndLuaAreGenerated()
        {
            modBuilder.Build();

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, Game, ModId);
            string descriptor = File.ReadAllText(Path.Combine(mainDirectoryPath, $"{ModId}.modinfo"));
            string localisation = File.ReadAllText(Path.Combine(mainDirectoryPath, "XML", $"{GetFilePrefix()}.xml"));
            string lua = File.ReadAllText(Path.Combine(mainDirectoryPath, "Lua", $"{GetFilePrefix()}.lua"));

            Assert.Multiple(() =>
            {
                Assert.That(descriptor, Does.Contain("<Mod id="));
                Assert.That(descriptor, Does.Contain("import=\"1\">Lua/more_cultural_names.lua</File>"));
                Assert.That(descriptor, Does.Contain("import=\"0\">XML/more_cultural_names.xml</File>"));
                Assert.That(descriptor, Does.Contain("<UpdateDatabase>XML/more_cultural_names.xml</UpdateDatabase>"));
                Assert.That(descriptor, Does.Contain("type=\"InGameUIAddin\" file=\"Lua/more_cultural_names.lua\""));

                Assert.That(localisation, Does.StartWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>"));
                Assert.That(localisation, Does.EndWith("</GameData>"));
                Assert.That(
                    localisation,
                    Does.Contain("<Row Tag=\"TXT_KEY_CITY_NAME_CLUJ_NAPOCA_CIVILIZATION_ENGLAND\">"));
                Assert.That(localisation, Does.Contain("<Text>Civ5 Newport</Text>"));
                Assert.That(localisation, Does.Contain("<Text>Civ5 Cluj-Napoca</Text>"));

                foreach (string locale in expectedLocales)
                {
                    Assert.That(localisation, Does.Contain($"<Language_{locale}>"));
                    Assert.That(localisation, Does.Contain($"</Language_{locale}>"));
                }

                Assert.That(lua, Does.Contain("city:SetName("));
                Assert.That(lua, Does.Contain("Locale.HasTextKey("));
                Assert.That(lua, Does.Contain("Events.SerialEventCityCaptured.Add("));
                Assert.That(lua, Does.Contain("Events.SerialEventCityCreated.Add("));
                Assert.That(lua, Does.Contain("Events.LoadScreenClose.Add("));
                Assert.That(lua, Does.Contain("MAX_CIV_PLAYERS"));
            });
        }

        [Test]
        public void GivenNoCivilizations_WhenBuilding_ThenNoLocalisationRowIsEmitted()
        {
            LanguageDataObject romanian = ModBuilderTestDataFactory.CreateLanguage(Game, "Romanian", "CIVILIZATION_ROME");
            LocationDataObject cluj = ModBuilderTestDataFactory.CreateLocation(
                Game,
                "Cluj-Napoca",
                "TXT_KEY_CITY_NAME_CLUJ_NAPOCA",
                null,
                null);
            ConfigureRepositories([romanian], [cluj]);

            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("TXT_KEY_CITY_NAME_CLUJ_NAPOCA", Game))
                .Returns([]);

            modBuilder = CreateModBuilder(true);

            modBuilder.Build();

            Assert.That(ReadLocalisation(), Does.Not.Contain("<Row Tag="));
        }

        [Test]
        public void GivenVerboseCommentsAreDisabled_WhenBuilding_ThenLanguageAnnotationsAreOmitted()
        {
            modBuilder = CreateModBuilder(false);

            modBuilder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(ReadLocalisation(), Does.Not.Contain("<!-- Language="));
                Assert.That(ReadLua(), Does.Not.Contain("Generated by the More Cultural Names Builder"));
            });
        }

        [Test]
        public void GivenACustomGameVersion_WhenBuilding_ThenTheDescriptorDeclaresTheBuildVersion()
        {
            modBuilder = CreateModBuilder(true, "1.20.1");

            modBuilder.Build();

            Assert.Multiple(() =>
            {
                Assert.That(ReadDescriptor(), Does.Contain("<!-- Version 1.0.0 -->"));
                Assert.That(ReadDescriptor(), Does.Contain("<Mod id="));
            });
        }

        [Test]
        public void GivenTheSameModIdentifier_WhenBuilding_ThenTheDescriptorIdentifierIsStable()
        {
            modBuilder.Build();

            string firstDescriptor = ReadDescriptor();

            TemporaryDirectory otherDirectory = new($"{nameof(Civilization5ModBuilderTests)}-stable");

            try
            {
                Civilization5ModBuilder otherBuilder = CreateModBuilder(otherDirectory.DirectoryPath);
                otherBuilder.Build();

                string secondDescriptor = File.ReadAllText(Path.Combine(
                    otherDirectory.DirectoryPath,
                    Game,
                    ModId,
                    $"{ModId}.modinfo"));

                Assert.That(secondDescriptor, Is.EqualTo(firstDescriptor));
            }
            finally
            {
                otherDirectory.Dispose();
            }
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
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("TXT_KEY_CITY_NAME_CLUJ_NAPOCA", Game))
                .Returns(
                [
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Cluj-Napoca",
                        "TXT_KEY_CITY_NAME_CLUJ_NAPOCA",
                        "Romanian",
                        "CIVILIZATION_ROME",
                        "Cluj-Napoca",
                        null,
                        "Praise the Sun!"),
                    ModBuilderTestDataFactory.CreateLocalisation(
                        "Cluj-Napoca",
                        "TXT_KEY_CITY_NAME_CLUJ_NAPOCA",
                        "English",
                        "CIVILIZATION_ENGLAND",
                        "Newport",
                        null,
                        null)
                ]);

            localisationFetcher
                .Setup(fetcher => fetcher.GetGameLocationLocalisations("TXT_KEY_CITY_NAME_ORADEA", Game))
                .Returns([]);
        }

        private string ReadDescriptor() => Read("more-cultural-names.modinfo");

        private string ReadLocalisation() => Read(Path.Combine("XML", $"{GetFilePrefix()}.xml"));

        private string ReadLua() => Read(Path.Combine("Lua", $"{GetFilePrefix()}.lua"));

        private string Read(string relativeFilePath) => File.ReadAllText(Path.Combine(
            temporaryDirectory.DirectoryPath,
            Game,
            ModId,
            relativeFilePath));

        private Civilization5ModBuilder CreateModBuilder(bool areVerboseCommentsEnabled)
            => CreateModBuilder(areVerboseCommentsEnabled, "1.18.*");

        private Civilization5ModBuilder CreateModBuilder(string outputDirectoryPath)
            => CreateModBuilder(outputDirectoryPath, true, "1.18.*");

        private Civilization5ModBuilder CreateModBuilder(bool areVerboseCommentsEnabled, string gameVersion)
            => CreateModBuilder(
                temporaryDirectory.DirectoryPath,
                areVerboseCommentsEnabled,
                gameVersion);

        private Civilization5ModBuilder CreateModBuilder(
            string outputDirectoryPath,
            bool areVerboseCommentsEnabled,
            string gameVersion)
        {
            Settings settings = SettingsTestFactory.Create(
                Game,
                outputDirectoryPath,
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
