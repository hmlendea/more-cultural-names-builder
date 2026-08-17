using System;

using NUnit.Framework;

using MoreCulturalNamesBuilder.Configuration;

namespace MoreCulturalNamesBuilder.UnitTests.Configuration
{
    [TestFixture]
    public sealed class SettingsTests
    {
        [Test]
        public void GivenAllArguments_WhenCreatingSettings_ThenEveryValueIsParsed()
        {
            string[] arguments =
            [
                "--lang", "languages.xml",
                "--loc", "locations.xml",
                "--landed-titles", "landed_titles.txt",
                "--output", "mods",
                "--verbose", "true",
                "--landed-titles-name", "00_landed_titles.txt",
                "--id", "more-cultural-names",
                "--name", "More Cultural Names",
                "--version", "1.0.0",
                "--dependency", "A Game of Thrones",
                "--game", "CK3",
                "--game-version", "1.12.*"
            ];

            Settings settings = new(arguments);

            Assert.Multiple(() =>
            {
                Assert.That(settings.Input.LanguageStorePath, Is.EqualTo("languages.xml"));
                Assert.That(settings.Input.LocationStorePath, Is.EqualTo("locations.xml"));
                Assert.That(settings.Input.LandedTitlesFilePath, Is.EqualTo("landed_titles.txt"));
                Assert.That(settings.Output.ModOutputDirectory, Is.EqualTo("mods"));
                Assert.That(settings.Output.AreVerboseCommentsEnabled);
                Assert.That(settings.Output.LandedTitlesFileName, Is.EqualTo("00_landed_titles.txt"));
                Assert.That(settings.Mod.Id, Is.EqualTo("more-cultural-names"));
                Assert.That(settings.Mod.Name, Is.EqualTo("More Cultural Names"));
                Assert.That(settings.Mod.Version, Is.EqualTo("1.0.0"));
                Assert.That(settings.Mod.Dependency, Is.EqualTo("A Game of Thrones"));
                Assert.That(settings.Mod.Game, Is.EqualTo("CK3"));
                Assert.That(settings.Mod.GameVersion, Is.EqualTo("1.12.*"));
            });
        }

        [TestCase("false")]
        [TestCase("False")]
        [TestCase("TRUE")]
        [TestCase("0")]
        [TestCase("")]
        [TestCase(" ")]
        public void GivenANonLowercaseTrueVerboseValue_WhenCreatingSettings_ThenVerboseCommentsAreDisabled(
            string verboseValue)
        {
            string[] arguments =
            [
                "--lang", "languages.xml",
                "--loc", "locations.xml",
                "--output", "mods",
                "--verbose", verboseValue,
                "--id", "more-cultural-names",
                "--name", "More Cultural Names",
                "--version", "1.0.0",
                "--game", "CK2",
                "--game-version", "1.12.*"
            ];

            Settings settings = new(arguments);

            Assert.That(settings.Output.AreVerboseCommentsEnabled, Is.False);
        }

        [Test]
        public void WhenUsingCanonicalArguments_ThenSettingsAreParsed()
        {
            string[] arguments =
            [
                "--lang", "languages.xml",
                "--loc", "locations.xml",
                "--id", "more-cultural-names",
                "--name", "More Cultural Names",
                "--version", "1.0.0",
                "--game", "CK2",
                "--game-version", "3.3.*",
                "--output", "./output"
            ];

            Settings settings = new(arguments);

            Assert.That(settings.Mod.Version, Is.EqualTo("1.0.0"));
            Assert.That(settings.Output.ModOutputDirectory, Is.EqualTo("./output"));
        }

        [Test]
        public void WhenUsingAliasArguments_ThenSettingsAreParsed()
        {
            string[] arguments =
            [
                "--lang", "languages.xml",
                "--loc", "locations.xml",
                "--id", "more-cultural-names",
                "--name", "More Cultural Names",
                "--ver", "2.4.0",
                "--game", "CK2",
                "--game-version", "3.3.*",
                "--out", "./alias-output"
            ];

            Settings settings = new(arguments);

            Assert.That(settings.Mod.Version, Is.EqualTo("2.4.0"));
            Assert.That(settings.Output.ModOutputDirectory, Is.EqualTo("./alias-output"));
        }

        [Test]
        public void WhenVersionArgumentsAreMissing_ThenThrows()
        {
            string[] arguments =
            [
                "--lang", "languages.xml",
                "--loc", "locations.xml",
                "--id", "more-cultural-names",
                "--name", "More Cultural Names",
                "--game", "CK2",
                "--game-version", "3.3.*",
                "--output", "./output"
            ];

            ArgumentException exception = Assert.Throws<ArgumentException>(() => new Settings(arguments));

            Assert.That(exception.Message, Does.Contain("--version"));
        }

        [Test]
        public void WhenOutputArgumentsAreMissing_ThenThrows()
        {
            string[] arguments =
            [
                "--lang", "languages.xml",
                "--loc", "locations.xml",
                "--id", "more-cultural-names",
                "--name", "More Cultural Names",
                "--version", "2.4.0",
                "--game", "CK2",
                "--game-version", "3.3.*"
            ];

            ArgumentException exception = Assert.Throws<ArgumentException>(() => new Settings(arguments));

            Assert.That(exception.Message, Does.Contain("--output"));
        }
    }
}
