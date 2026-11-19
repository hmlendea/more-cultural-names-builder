using System;

using MoreCulturalNamesBuilder.Configuration;

using NUnit.Framework;

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

        [Test]
        public void GivenCanonicalArguments_WhenParsingSettings_ThenSettingsAreParsed()
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
        public void GivenAliasArguments_WhenParsingSettings_ThenSettingsAreParsed()
        {
            string[] arguments =
            [
                "--lang", "languages.xml",
                "--loc", "locations.xml",
                "--id", "more-cultural-names",
                "--name", "More Cultural Names",
                "--ver", "2.4.0",
                "--dep", "agot",
                "--game", "CK2",
                "--game-version", "3.3.*",
                "--out", "./alias-output"
            ];

            Settings settings = new(arguments);

            Assert.That(settings.Mod.Version, Is.EqualTo("2.4.0"));
            Assert.That(settings.Mod.Dependency, Is.EqualTo("agot"));
            Assert.That(settings.Output.ModOutputDirectory, Is.EqualTo("./alias-output"));
        }

        [Test]
        public void GivenMissingVersionArguments_WhenParsingSettings_ThenAnExceptionIsThrown()
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

            Assert.That(
                () => new Settings(arguments),
                Throws.TypeOf<ArgumentException>().With.Message.Contains("--version"));
        }

        [Test]
        public void GivenMissingOutputArguments_WhenParsingSettings_ThenAnExceptionIsThrown()
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

            Assert.That(
                () => new Settings(arguments),
                Throws.TypeOf<ArgumentException>().With.Message.Contains("--output"));
        }

        [TestCase("--lang")]
        [TestCase("--loc")]
        [TestCase("--id")]
        [TestCase("--name")]
        [TestCase("--game")]
        [TestCase("--game-version")]
        public void GivenAMissingRequiredArgument_WhenParsingSettings_ThenAnArgumentNullExceptionIsThrown(
            string missingArgument)
        {
            string[] arguments = SettingsTestArguments.Except(missingArgument);

            Assert.That(
                () => new Settings(arguments),
                Throws.TypeOf<ArgumentNullException>().With.Property("ParamName")
                    .EqualTo(missingArgument.TrimStart('-')));
        }

        [TestCase("true", true)]
        [TestCase("false", false)]
        [TestCase("True", false)]
        [TestCase("TRUE", false)]
        [TestCase("False", false)]
        [TestCase("1", false)]
        [TestCase("0", false)]
        [TestCase("", false)]
        [TestCase(" ", false)]
        public void GivenAVerboseValue_WhenParsingSettings_ThenOnlyTheLowercaseTrueEnablesComments(
            string verboseValue,
            bool expectedValue)
        {
            string[] arguments = SettingsTestArguments.With("--verbose", verboseValue);

            Settings settings = new(arguments);

            Assert.That(settings.Output.AreVerboseCommentsEnabled, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GivenBothVersionForms_WhenParsingSettings_ThenTheCanonicalArgumentWins()
        {
            string[] arguments =
            [
                .. SettingsTestArguments.Except("--version"),
                "--version", "1.0.0",
                "--ver", "2.0.0"
            ];

            Settings settings = new(arguments);

            Assert.That(settings.Mod.Version, Is.EqualTo("1.0.0"));
        }

        [Test]
        public void GivenBothDependencyForms_WhenParsingSettings_ThenTheCanonicalArgumentWins()
        {
            string[] arguments =
            [
                .. SettingsTestArguments.Except("--dependency"),
                "--dependency", "A Game of Thrones",
                "--dep", "agot"
            ];

            Settings settings = new(arguments);

            Assert.That(settings.Mod.Dependency, Is.EqualTo("A Game of Thrones"));
        }

        [TestCase("")]
        [TestCase(" ")]
        public void GivenABlankAliasValue_WhenParsingSettings_ThenTheCanonicalValueIsUsed(
            string blankValue)
        {
            string[] arguments =
            [
                .. SettingsTestArguments.Except("--dependency"),
                "--dependency", "A Game of Thrones",
                "--dep", blankValue
            ];

            Settings settings = new(arguments);

            Assert.That(settings.Mod.Dependency, Is.EqualTo("A Game of Thrones"));
        }

        [TestCase("CK2")]
        [TestCase("CK3")]
        public void GivenAnOptionalDependency_WhenParsingSettings_ThenItIsPreserved(string game)
        {
            string[] arguments = SettingsTestArguments.WithGameAndDependency(
                game,
                "A Game of Thrones");

            Settings settings = new(arguments);

            Assert.Multiple(() =>
            {
                Assert.That(settings.Mod.Dependency, Is.EqualTo("A Game of Thrones"));
                Assert.That(settings.Mod.Game, Is.EqualTo(game));
            });
        }

        [Test]
        public void GivenNoOptionalArguments_WhenParsingSettings_ThenOptionalValuesAreNull()
        {
            string[] arguments = SettingsTestArguments.WithoutOptionalArguments();

            Settings settings = new(arguments);

            Assert.Multiple(() =>
            {
                Assert.That(settings.Input.LandedTitlesFilePath, Is.Null);
                Assert.That(settings.Output.LandedTitlesFileName, Is.Null);
                Assert.That(settings.Mod.Dependency, Is.Null);
            });
        }
    }
}
