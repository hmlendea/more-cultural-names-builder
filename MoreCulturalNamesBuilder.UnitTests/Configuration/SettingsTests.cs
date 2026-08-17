using System;

using NUnit.Framework;

using MoreCulturalNamesBuilder.Configuration;

namespace MoreCulturalNamesBuilder.UnitTests.Configuration
{
    public class SettingsTests
    {
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
