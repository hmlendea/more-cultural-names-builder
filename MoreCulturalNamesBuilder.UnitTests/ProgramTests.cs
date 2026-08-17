using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service;
using MoreCulturalNamesBuilder.Service.ModBuilders;
using MoreCulturalNamesBuilder.UnitTests.TestInfrastructure;

namespace MoreCulturalNamesBuilder.UnitTests
{
    [TestFixture]
    [NonParallelizable]
    public sealed class ProgramTests
    {
        private static string ModId => "more-cultural-names";

        private TemporaryDirectory temporaryDirectory = null!;

        [SetUp]
        public void SetUp() => temporaryDirectory = new(nameof(ProgramTests));

        [TearDown]
        public void TearDown()
        {
            if (Program.ServiceProvider is IDisposable disposableServiceProvider)
            {
                disposableServiceProvider.Dispose();
            }

            temporaryDirectory.Dispose();
        }

        [Test]
        public void GivenValidArgumentsAndEmptyStores_WhenRunningTheProgram_ThenServicesAndModFilesAreCreated()
        {
            string languageStorePath = Path.Combine(temporaryDirectory.DirectoryPath, "languages.xml");
            string locationStorePath = Path.Combine(temporaryDirectory.DirectoryPath, "locations.xml");
            WriteXmlCollection(languageStorePath, new List<LanguageEntity>());
            WriteXmlCollection(locationStorePath, new List<LocationEntity>());
            string[] arguments =
            [
                "--lang", languageStorePath,
                "--loc", locationStorePath,
                "--output", temporaryDirectory.DirectoryPath,
                "--id", ModId,
                "--name", "More Cultural Names",
                "--version", "1.0.0",
                "--game", "IR",
                "--game-version", "1.12.*"
            ];

            Program.Main(arguments);

            string outputDirectoryPath = Path.Combine(temporaryDirectory.DirectoryPath, "IR");
            string mainDirectoryPath = Path.Combine(outputDirectoryPath, ModId);
            Assert.Multiple(() =>
            {
                Assert.That(Program.ServiceProvider, Is.Not.Null);
                Assert.That(Program.ServiceProvider.GetService<ILocalisationFetcher>(), Is.TypeOf<LocalisationFetcher>());
                Assert.That(Program.ServiceProvider.GetService<INameNormaliser>(), Is.TypeOf<NameNormaliser>());
                Assert.That(Program.ServiceProvider.GetService<IModBuilderFactory>(), Is.TypeOf<ModBuilderFactory>());
                Assert.That(File.Exists(Path.Combine(outputDirectoryPath, $"{ModId}.mod")));
                Assert.That(File.Exists(Path.Combine(mainDirectoryPath, "descriptor.mod")));
                Assert.That(
                    File.Exists(Path.Combine(
                        mainDirectoryPath,
                        "localization",
                        $"{ModId}_provincenames_l_english.yml")));
            });
        }

        private static void WriteXmlCollection<T>(string filePath, IEnumerable<T> items)
        {
            XmlSerializer serialiser = new(typeof(List<T>));
            using FileStream stream = File.Create(filePath);
            serialiser.Serialize(stream, items.ToList());
        }
    }
}