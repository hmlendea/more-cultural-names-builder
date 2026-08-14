using System;

using Moq;

using NuciDAL.Repositories;

using NUnit.Framework;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service;
using MoreCulturalNamesBuilder.Service.ModBuilders;
using MoreCulturalNamesBuilder.UnitTests.Configuration;

namespace MoreCulturalNamesBuilder.UnitTests.Service.ModBuilders
{
    [TestFixture]
    public sealed class ModBuilderFactoryTests
    {
        private ModBuilderFactory modBuilderFactory = null!;

        [SetUp]
        public void SetUp()
        {
            Mock<ILocalisationFetcher> localisationFetcher = new();
            Mock<INameNormaliser> nameNormaliser = new();
            Mock<IFileRepository<LanguageEntity>> languageRepository = new();
            Mock<IFileRepository<LocationEntity>> locationRepository = new();

            modBuilderFactory = new(
                localisationFetcher.Object,
                nameNormaliser.Object,
                languageRepository.Object,
                locationRepository.Object);
        }

        [TestCase("CK2", typeof(CK2ModBuilder))]
        [TestCase(" ck2 total conversion ", typeof(CK2ModBuilder))]
        [TestCase("CK3", typeof(CK3ModBuilder))]
        [TestCase("ck3 beta", typeof(CK3ModBuilder))]
        [TestCase("HOI4", typeof(HOI4ModBuilder))]
        [TestCase(" hoi4 beta ", typeof(HOI4ModBuilder))]
        [TestCase("IR", typeof(ImperatorRomeModBuilder))]
        [TestCase("ir beta", typeof(ImperatorRomeModBuilder))]
        [TestCase("ImperatorRome", typeof(ImperatorRomeModBuilder))]
        [TestCase(" imperatorrome beta ", typeof(ImperatorRomeModBuilder))]
        public void GivenASupportedGame_WhenGettingAModBuilder_ThenTheCorrespondingBuilderIsReturned(
            string game,
            Type expectedBuilderType)
        {
            Settings settings = SettingsTestFactory.Create(game, "mods");

            IModBuilder modBuilder = modBuilderFactory.GetModBuilder(settings);

            Assert.That(modBuilder, Is.TypeOf(expectedBuilderType));
        }

        [TestCase("CK")]
        [TestCase("Hearts of Iron IV")]
        [TestCase("Imperator Rome")]
        [TestCase("Minecraft")]
        [TestCase("")]
        [TestCase("   ")]
        public void GivenAnUnsupportedGame_WhenGettingAModBuilder_ThenANotImplementedExceptionIsThrown(
            string game)
        {
            Settings settings = SettingsTestFactory.Create(game, "mods");

            Assert.That(
                () => modBuilderFactory.GetModBuilder(settings),
                Throws.TypeOf<NotImplementedException>()
                    .With.Message.EqualTo($"The game \"{game}\" is not supported"));
        }
    }
}