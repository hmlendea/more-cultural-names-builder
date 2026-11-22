using System;
using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class CharsetIntegrationTests
    {
        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(CharsetIntegrationTests));

        [TearDown]
        public void TearDown() => temporaryDirectory.Dispose();

        [Test]
        public void GivenDiacriticsAndNonLatinNames_WhenBuildingTheCk2Mod_ThenTheNamesAreTransliteratedToWindows1252()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK2"));

            byte[] content = TextContent.ReadBytes(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "m",
                "common",
                "landed_titles",
                "00_landed_titles.txt"));

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain((byte)0xC7), "C-cedilla must be encoded as 0xC7 in Windows-1252.");
                Assert.That(ContainsSequence(content, "upisan"u8.ToArray()), Is.True);
            });
        }

        [Test]
        public void GivenDiacriticsAndNonLatinNames_WhenBuildingTheCk3Mod_ThenTheNamesKeepTheirUnicodeCharacters()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK3"));

            string localisation = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK3",
                "m",
                "localization",
                "english",
                "m_titles_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Does.Contain("Florești"));
                Assert.That(localisation, Does.Contain("Çupișan"));
            });
        }

        [Test]
        public void GivenDiacriticsAndNonLatinNames_WhenBuildingTheHoI4Mod_ThenTheNamesUseTheHoI4Charset()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("HOI4"));

            string localisation = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "HOI4",
                "m",
                "localisation",
                "english",
                "zzz999_m_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Does.Contain("Floreşti"), "The comma-below s must survive as ş.");
                Assert.That(localisation, Does.Contain("Çupişan"));
            });
        }

        [Test]
        public void GivenDiacriticsAndNonLatinNames_WhenBuildingTheImperatorRomeMod_ThenTheNamesUseTheImperatorCharset()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("IR"));

            string localisation = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "IR",
                "m",
                "localization",
                "m_provincenames_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Does.Contain("Floresti"), "The comma-below s must be reduced to s.");
                Assert.That(localisation, Does.Contain("Çupisan"));
            });
        }

        [Test]
        public void GivenDiacriticsAndNonLatinNames_WhenBuildingTheVictoria3Mod_ThenTheNamesKeepTheirUnicodeCharacters()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("Vic3"));

            string localisation = TextContent.Read(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "Vic3",
                "m",
                "localization",
                "english",
                "m_state_names_l_english.yml"));

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Does.Contain("Florești"));
                Assert.That(localisation, Does.Contain("Çupișan"));
            });
        }

        [Test]
        public void GivenTheCk2Mod_WhenReadingTheLandedTitlesAsUtf8_ThenTheBytesRemainWindows1252()
        {
            ModBuildRunner.Run(temporaryDirectory.DirectoryPath, BuildRequest("CK2"));

            byte[] content = TextContent.ReadBytes(Path.Combine(
                temporaryDirectory.DirectoryPath,
                "CK2",
                "m",
                "common",
                "landed_titles",
                "00_landed_titles.txt"));

            Assert.That(ContainsSequence(content, "și"u8.ToArray()), Is.False);
        }

        private static bool ContainsSequence(byte[] content, byte[] sequence)
        {
            for (int index = 0; index <= content.Length - sequence.Length; index += 1)
            {
                bool matches = true;

                for (int offset = 0; offset < sequence.Length; offset += 1)
                {
                    if (content[index + offset] != sequence[offset])
                    {
                        matches = false;

                        break;
                    }
                }

                if (matches)
                {
                    return true;
                }
            }

            return false;
        }

        private static ModBuildRequest BuildRequest(string game)
        {
            ModBuildRequest request = new()
            {
                Game = game,
                ModId = "m",
                LandedTitlesFileName = "00_landed_titles.txt"
            };

            if (string.Equals(game, "CK3", StringComparison.Ordinal))
            {
                request.LandedTitlesFilePath = TestPaths.Ck3LandedTitlesFilePath;
            }

            else
            {
                request.LandedTitlesFilePath = TestPaths.Ck2LandedTitlesFilePath;
            }

            return request;
        }
    }
}
