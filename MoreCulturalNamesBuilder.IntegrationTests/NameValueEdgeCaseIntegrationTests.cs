using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class NameValueEdgeCaseIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(NameValueEdgeCaseIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenANameContainingASemicolon_WhenBuildingTheCk2Mod_ThenTheValueIsPassedThroughUnescaped()
        {
            WriteStores("CK2", "english", "c_semi", "Semi;Colon", "Semi;ish");
            WriteLandedTitles("c_semi = {\n}\n");

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK2"));

            string localisation = TextContent.ReadWindows1252(
                OutputPath("CK2", "localisation", "000_m_landed_titles.csv"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Does.Contain("c_semi;Semi;Colon;Semi;Colon;"));
                Assert.That(localisation, Does.Contain("c_semi_adj;Semi;ish;"));
            });
        }

        [Test]
        public void GivenANameContainingASemicolon_WhenBuildingTheHoI4Mod_ThenTheValueIsPassedThroughUnescaped()
        {
            WriteStores("HOI4", "english", "2", "Semi;Colon", "Semi;ish");

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("HOI4"));

            string localisation = TextContent.Read(
                OutputPath("HOI4", "localisation", "english", "zzz999_m_l_english.yml"));

            Assert.That(localisation, Does.Contain("english_STATE_2:0 \"Semi;Colon\""));
        }

        [Test]
        public void GivenANameContainingQuoteCharacters_WhenBuildingTheCk3Mod_ThenTheQuotesAreEmbeddedVerbatim()
        {
            WriteStores("CK3", "english", "c_quote", "The &quot;Quote&quot;", "Quotish");
            WriteLandedTitles("c_quote = {\n}\n");

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK3"));

            string titles = TextContent.Read(OutputPath("CK3", "localization", "english", "m_titles_l_english.yml"));

            Assert.That(titles, Does.Contain("c_quote:0 \"The \"Quote\"\""));
        }

        [Test]
        public void GivenANameContainingQuoteCharacters_WhenBuildingTheImperatorRomeMod_ThenTheQuotesAreEmbeddedVerbatim()
        {
            WriteStores("IR", "english", "2", "The &quot;Quote&quot;", "Quotish");

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("IR"));

            string localisation = TextContent.Read(
                OutputPath("IR", "localization", "m_provincenames_l_english.yml"));

            Assert.That(localisation, Does.Contain("PROV2_english:0 \"The \"Quote\"\""));
        }

        [Test]
        public void GivenANameWithLeadingAndTrailingWhitespace_WhenBuildingTheCk3Mod_ThenTheWhitespaceIsPreserved()
        {
            WriteStores("CK3", "english", "c_padded", "  Padded  ", "  Paddedish  ");
            WriteLandedTitles("c_padded = {\n}\n");

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK3"));

            string titles = TextContent.Read(OutputPath("CK3", "localization", "english", "m_titles_l_english.yml"));

            Assert.That(titles, Does.Contain("c_padded:0 \"  Padded  \""));
        }

        [Test]
        public void GivenANameWithNonBreakingSpace_WhenBuildingTheCk3Mod_ThenTheNameIsEmittedVerbatim()
        {
            WriteStores("CK3", "english", "c_nbsp", "Alpha\u00A0Beta", "Alphan");
            WriteLandedTitles("c_nbsp = {\n}\n");

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK3"));

            string titles = TextContent.Read(OutputPath("CK3", "localization", "english", "m_titles_l_english.yml"));

            Assert.That(titles, Does.Contain("c_nbsp:0 \"Alpha\u00A0Beta\""));
        }

        [Test]
        public void GivenALanguageGameIdentifierWithDifferentCasing_WhenBuildingTheCk2Mod_ThenItIsNotMatched()
        {
            WriteStores("ck2", "english", "c_case", "Newport", "Newportian");
            WriteLandedTitles("c_case = {\n}\n");

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK2"));

            string landedTitles = TextContent.ReadWindows1252(
                OutputPath("CK2", "common", "landed_titles", "00_landed_titles.txt"));

            Assert.That(
                landedTitles,
                Is.EqualTo("c_case = {\n}"),
                "The game identifier is matched case-sensitively, so a lowercase 'ck2' must not resolve.");
        }

        private void WriteStores(
            string game,
            string languageGameId,
            string locationGameId,
            string name,
            string adjective)
        {
            File.WriteAllText(
                Path.Combine(temporaryDirectory.DirectoryPath, "languages.xml"),
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<ArrayOfLanguage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n" +
                "  <Language>\n" +
                "    <Id>English</Id>\n" +
                "    <Code iso-639-1=\"en\" iso-639-2=\"eng\" iso-639-3=\"eng\" />\n" +
                "    <GameIds>\n" +
                $"      <GameId game=\"{game}\">{languageGameId}</GameId>\n" +
                "    </GameIds>\n" +
                "    <FallbackLanguages />\n" +
                "  </Language>\n" +
                "</ArrayOfLanguage>\n");
            File.WriteAllText(
                Path.Combine(temporaryDirectory.DirectoryPath, "locations.xml"),
                BuildLocationStore(game, locationGameId, name, adjective));
        }

        private static string BuildLocationStore(string game, string locationGameId, string name, string adjective)
            =>
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<ArrayOfLocationEntity xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n" +
                "  <LocationEntity>\n" +
                "    <Id>Sample</Id>\n" +
                "    <FallbackLocations />\n" +
                "    <GameIds>\n" +
                $"      <GameId game=\"{game}\" type=\"county\" defaultLanguage=\"English\">{locationGameId}</GameId>\n" +
                $"      <GameId game=\"{game}\" type=\"State\" defaultLanguage=\"English\">{locationGameId}</GameId>\n" +
                $"      <GameId game=\"{game}\" type=\"Province\" defaultLanguage=\"English\">{locationGameId}</GameId>\n" +
                "    </GameIds>\n" +
                "    <Names>\n" +
                $"      <Name language=\"English\" value=\"{name}\" adjective=\"{adjective}\" />\n" +
                "    </Names>\n" +
                "  </LocationEntity>\n" +
                "</ArrayOfLocationEntity>\n";

        private void WriteLandedTitles(string content)
            => File.WriteAllText(Path.Combine(temporaryDirectory.DirectoryPath, "landed_titles.txt"), content);

        private string OutputPath(string game, params string[] segments)
            => Path.Combine([temporaryDirectory.DirectoryPath, game, "m", .. segments]);

        private ModBuildRequest BuildRequest(string game)
            => new()
            {
                Game = game,
                ModId = "m",
                LanguageStorePath = Path.Combine(temporaryDirectory.DirectoryPath, "languages.xml"),
                LocationStorePath = Path.Combine(temporaryDirectory.DirectoryPath, "locations.xml"),
                LandedTitlesFilePath = Path.Combine(temporaryDirectory.DirectoryPath, "landed_titles.txt"),
                LandedTitlesFileName = "00_landed_titles.txt"
            };
    }
}
