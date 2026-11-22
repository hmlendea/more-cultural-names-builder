using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class GameIdentifierIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(GameIdentifierIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [TestCase("EuropaUniversalis5")]
        [TestCase("EUROPAUNIVERSALIS5")]
        [TestCase("europauniversalis5")]
        public void GivenAFullEuropaUniversalis5GameName_WhenBuildingTheMod_ThenTheMetadataIsEmitted(string game)
        {
            ModBuildRequest request = new() { Game = game };

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.Multiple(() =>
            {
                Assert.That(
                    File.Exists(Path.Combine(temporaryDirectory.DirectoryPath, game, "more-cultural-names", ".metadata", "metadata.json")),
                    Is.True);
                Assert.That(
                    TextContent.Read(Path.Combine(
                        temporaryDirectory.DirectoryPath,
                        game,
                        "more-cultural-names",
                        ".metadata",
                        "metadata.json")),
                    Does.Contain("\"game_id\": \"eu5\""));
            });
        }

        [TestCase("ImperatorRome")]
        [TestCase("IMPERATORROME")]
        [TestCase("imperatorrome")]
        public void GivenAFullImperatorRomeGameName_WhenBuildingTheMod_ThenTheDescriptorIsEmitted(string game)
        {
            ModBuildRequest request = new() { Game = game };

            Assert.That(
                () => ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request),
                Throws.Nothing);
            Assert.That(
                File.Exists(Path.Combine(temporaryDirectory.DirectoryPath, game, "more-cultural-names.mod")),
                Is.True);
        }

        [TestCase("HOI4")]
        [TestCase("hoi4")]
        [TestCase("HoI4")]
        public void GivenANormalisedHoI4GameName_WhenBuildingTheMod_ThenTheStateLocalisationIsEmitted(string game)
        {
            ModBuildRequest request = new() { Game = game };

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                File.Exists(Path.Combine(
                    temporaryDirectory.DirectoryPath,
                    game,
                    "more-cultural-names",
                    "localisation",
                    "english",
                    "zzz999_more-cultural-names_l_english.yml")),
                Is.True);
        }

        [Test]
        public void GivenAnUppercaseModIdentifier_WhenBuildingTheEuropaUniversalis5Mod_ThenTheFileNameKeepsIt()
        {
            ModBuildRequest request = new() { Game = "EU5", ModId = "My-Mod" };

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            string mainDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "EU5", "My-Mod");

            Assert.Multiple(() =>
            {
                Assert.That(
                    TextContent.Read(Path.Combine(mainDirectoryPath, ".metadata", "metadata.json")),
                    Does.Contain("\"id\": \"My-Mod\""));
                Assert.That(
                    File.Exists(Path.Combine(
                        mainDirectoryPath,
                        "main_menu",
                        "localization",
                        "english",
                        "replace",
                        "My_Mod_location_names_l_english.yml")),
                    Is.True);
            });
        }

        [Test]
        public void GivenAnUppercaseModIdentifier_WhenBuildingTheVictoria3Mod_ThenTheEffectsFileKeepsIt()
        {
            ModBuildRequest request = new() { Game = "Vic3", ModId = "My-Mod" };

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                TextContent.Read(Path.Combine(
                    temporaryDirectory.DirectoryPath,
                    "Vic3",
                    "My-Mod",
                    "common",
                    "scripted_effects",
                    "My_Mod_state_names.txt")),
                Does.Contain("My_Mod_evaluate_and_assign_state_hub_dynamic_names = {"));
        }

        [Test]
        public void GivenABlankModIdentifier_WhenBuildingTheEuropaUniversalis5Mod_ThenTheTreeUsesEmptyNames()
        {
            ModBuildRequest request = new() { Game = "EU5", ModId = string.Empty };

            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, request);

            Assert.That(
                File.Exists(Path.Combine(
                    temporaryDirectory.DirectoryPath,
                    "EU5",
                    "main_menu",
                    "localization",
                    "english",
                    "replace",
                    "_location_names_l_english.yml")),
                Is.True);
        }
    }
}
