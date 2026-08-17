using System;
using System.Collections.Generic;
using System.Linq;

using Moq;

using NuciDAL.Repositories;

using NUnit.Framework;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.UnitTests.Service
{
    [TestFixture]
    public sealed class LocalisationFetcherTests
    {
        private static string Game => "Crusader Kings III";
        private static string LocationGameId => "c_cluj";
        private static string LocationId => "Cluj-Napoca";

        private Mock<IFileRepository<LanguageEntity>> languageRepository = null!;
        private Mock<IFileRepository<LocationEntity>> locationRepository = null!;
        private LocalisationFetcher localisationFetcher = null!;

        [SetUp]
        public void SetUp()
        {
            languageRepository = new();
            locationRepository = new();
            localisationFetcher = CreateFetcher([BuildRomanianLanguage()], [BuildClujLocation()]);
        }

        [Test]
        public void GivenAMatchingLocationAndLanguage_WhenGettingLocalisations_ThenTheMappedLocalisationIsReturned()
        {
            Localisation localisation = localisationFetcher
                .GetGameLocationLocalisations(LocationGameId, Game)
                .Single();

            Assert.Multiple(() =>
            {
                Assert.That(localisation.Id, Is.EqualTo(LocationId));
                Assert.That(localisation.GameId, Is.EqualTo(LocationGameId));
                Assert.That(localisation.LanguageId, Is.EqualTo("Romanian"));
                Assert.That(localisation.LanguageGameId, Is.EqualTo("romanian"));
                Assert.That(localisation.Name, Is.EqualTo(LocationId));
                Assert.That(localisation.Adjective, Is.EqualTo("Clujean"));
                Assert.That(localisation.Comment, Is.EqualTo("La umbra Nucului Bătrân"));
            });
        }

        [Test]
        public void GivenAMatchingTypedLocation_WhenGettingLocalisations_ThenTheLocationIsReturned()
            => Assert.That(
                localisationFetcher.GetGameLocationLocalisations(LocationGameId, "county", Game),
                Has.Exactly(1).Items);

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\r\n")]
        public void GivenAnEmptyLocationType_WhenGettingLocalisations_ThenTheIndexedLocationIsReturned(
            string locationGameIdType)
            => Assert.That(
                localisationFetcher.GetGameLocationLocalisations(LocationGameId, locationGameIdType, Game),
                Has.Exactly(1).Items);

        [TestCase("c_oradea", "county", "Crusader Kings III")]
        [TestCase("c_cluj", "duchy", "Crusader Kings III")]
        [TestCase("c_cluj", "county", "CK2")]
        [TestCase("", "county", "Crusader Kings III")]
        [TestCase("c_cluj", "", "CK2")]
        public void GivenANonMatchingLocation_WhenGettingLocalisations_ThenNoLocalisationsAreReturned(
            string locationGameId,
            string locationGameIdType,
            string game)
            => Assert.That(
                localisationFetcher.GetGameLocationLocalisations(locationGameId, locationGameIdType, game),
                Is.Empty);

        [Test]
        public void GivenAnEmptyLocation_WhenGettingLocalisations_ThenNoLocalisationsAreReturned()
        {
            LocationEntity emptyLocation = BuildLocation(LocationId, LocationGameId, [], []);
            localisationFetcher = CreateFetcher([BuildRomanianLanguage()], [emptyLocation]);

            IEnumerable<Localisation> localisations = localisationFetcher
                .GetGameLocationLocalisations(LocationGameId, Game);

            Assert.That(localisations, Is.Empty);
        }

        [Test]
        public void GivenALocationFallback_WhenGettingLocalisations_ThenTheFallbackLocationNameIsReturned()
        {
            LocationEntity location = BuildLocation(LocationId, LocationGameId, [], ["Oradea"]);
            LocationEntity fallbackLocation = BuildLocation(
                "Oradea",
                "c_oradea",
                [new("Romanian", "Oradea")],
                []);
            localisationFetcher = CreateFetcher(
                [BuildRomanianLanguage()],
                [location, fallbackLocation]);

            Localisation localisation = localisationFetcher
                .GetGameLocationLocalisations(LocationGameId, Game)
                .Single();

            Assert.Multiple(() =>
            {
                Assert.That(localisation.Id, Is.EqualTo("Oradea"));
                Assert.That(localisation.Name, Is.EqualTo("Oradea"));
            });
        }

        [Test]
        public void GivenALanguageFallback_WhenGettingLocalisations_ThenTheFallbackLanguageNameIsReturned()
        {
            LanguageEntity language = BuildLanguage("Romanian", "romanian", ["English"]);
            LocationEntity location = BuildLocation(
                LocationId,
                LocationGameId,
                [new("English", "Cluj-Napoca")],
                []);
            localisationFetcher = CreateFetcher([language], [location]);

            Localisation localisation = localisationFetcher
                .GetGameLocationLocalisations(LocationGameId, Game)
                .Single();

            Assert.That(localisation.LanguageId, Is.EqualTo("English"));
        }

        [Test]
        public void GivenBothFallbackTypes_WhenGettingLocalisations_ThenTheCurrentLocationLanguageFallbackHasPrecedence()
        {
            LanguageEntity language = BuildLanguage("Romanian", "romanian", ["English"]);
            LocationEntity location = BuildLocation(
                LocationId,
                LocationGameId,
                [new("English", "Cluj-Napoca")],
                ["Oradea"]);
            LocationEntity fallbackLocation = BuildLocation(
                "Oradea",
                "c_oradea",
                [new("Romanian", "Oradea")],
                []);
            localisationFetcher = CreateFetcher([language], [location, fallbackLocation]);

            Localisation localisation = localisationFetcher
                .GetGameLocationLocalisations(LocationGameId, Game)
                .Single();

            Assert.Multiple(() =>
            {
                Assert.That(localisation.Id, Is.EqualTo(LocationId));
                Assert.That(localisation.LanguageId, Is.EqualTo("English"));
            });
        }

        [Test]
        public void GivenNamesWithoutAMatchingLanguage_WhenGettingLocalisations_ThenNoLocalisationsAreReturned()
        {
            LocationEntity location = BuildLocation(
                LocationId,
                LocationGameId,
                [new("English", "Cluj-Napoca")],
                []);
            localisationFetcher = CreateFetcher([BuildRomanianLanguage()], [location]);

            IEnumerable<Localisation> localisations = localisationFetcher
                .GetGameLocationLocalisations(LocationGameId, Game);

            Assert.That(localisations, Is.Empty);
        }

        [Test]
        public void GivenMultipleLanguagesWithOneTranslation_WhenGettingLocalisations_ThenOnlyTheTranslationIsReturned()
        {
            LanguageEntity english = BuildLanguage("English", "english", []);
            localisationFetcher = CreateFetcher(
                [BuildRomanianLanguage(), english],
                [BuildClujLocation()]);

            IEnumerable<Localisation> localisations = localisationFetcher
                .GetGameLocationLocalisations(LocationGameId, Game);

            Assert.That(localisations.Single().LanguageId, Is.EqualTo("Romanian"));
        }

        [Test]
        public void GivenNoLanguagesForTheGame_WhenGettingLocalisations_ThenNoLocalisationsAreReturned()
        {
            LanguageEntity language = BuildRomanianLanguage();
            language.GameIds[0].Game = "CK2";
            localisationFetcher = CreateFetcher([language], [BuildClujLocation()]);

            IEnumerable<Localisation> localisations = localisationFetcher
                .GetGameLocationLocalisations(LocationGameId, Game);

            Assert.That(localisations, Is.Empty);
        }

        [Test]
        public void GivenAPreviouslyQueriedGame_WhenGettingLocalisationsAgain_ThenTheCachedLanguageIndexIsUsed()
        {
            IEnumerable<Localisation> firstLocalisations = localisationFetcher
                .GetGameLocationLocalisations(LocationGameId, Game);

            IEnumerable<Localisation> secondLocalisations = localisationFetcher
                .GetGameLocationLocalisations(LocationGameId, Game);

            Assert.Multiple(() =>
            {
                Assert.That(firstLocalisations, Has.Exactly(1).Items);
                Assert.That(secondLocalisations, Has.Exactly(1).Items);
                languageRepository.Verify(repository => repository.GetAll(), Times.Once);
                locationRepository.Verify(repository => repository.GetAll(), Times.Once);
            });
        }

        [Test]
        public void GivenALanguageWithoutACode_WhenCreatingTheFetcher_ThenTheLanguageIsLoaded()
        {
            LanguageEntity language = BuildRomanianLanguage();
            language.Code = null;

            Assert.That(
                () => CreateFetcher([language], [BuildClujLocation()]),
                Throws.Nothing);
        }

        [Test]
        public void GivenAMissingFallbackLocation_WhenGettingLocalisations_ThenAKeyNotFoundExceptionIsThrown()
        {
            LocationEntity location = BuildLocation(LocationId, LocationGameId, [], ["Oradea"]);
            localisationFetcher = CreateFetcher([BuildRomanianLanguage()], [location]);

            Assert.That(
                () => localisationFetcher.GetGameLocationLocalisations(LocationGameId, Game).ToList(),
                Throws.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void GivenDuplicateLocationIdentifiers_WhenCreatingTheFetcher_ThenAnArgumentExceptionIsThrown()
            => Assert.That(
                () => CreateFetcher(
                    [BuildRomanianLanguage()],
                    [BuildClujLocation(), BuildClujLocation()]),
                Throws.TypeOf<ArgumentException>());

        [Test]
        public void GivenDuplicateLanguageIdentifiers_WhenCreatingTheFetcher_ThenAnArgumentExceptionIsThrown()
            => Assert.That(
                () => CreateFetcher(
                    [BuildRomanianLanguage(), BuildRomanianLanguage()],
                    [BuildClujLocation()]),
                Throws.TypeOf<ArgumentException>());

        [Test]
        public void GivenANullLocationSequence_WhenCreatingTheFetcher_ThenAnArgumentNullExceptionIsThrown()
        {
            locationRepository.Setup(repository => repository.GetAll()).Returns((IEnumerable<LocationEntity>)null);
            languageRepository.Setup(repository => repository.GetAll()).Returns([BuildRomanianLanguage()]);

            Assert.That(
                () => new LocalisationFetcher(languageRepository.Object, locationRepository.Object),
                Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GivenANullLanguageSequence_WhenCreatingTheFetcher_ThenAnArgumentNullExceptionIsThrown()
        {
            locationRepository.Setup(repository => repository.GetAll()).Returns([BuildClujLocation()]);
            languageRepository.Setup(repository => repository.GetAll()).Returns((IEnumerable<LanguageEntity>)null);

            Assert.That(
                () => new LocalisationFetcher(languageRepository.Object, locationRepository.Object),
                Throws.TypeOf<ArgumentNullException>());
        }

        private LocalisationFetcher CreateFetcher(
            IEnumerable<LanguageEntity> languages,
            IEnumerable<LocationEntity> locations)
        {
            languageRepository.Setup(repository => repository.GetAll()).Returns(languages);
            locationRepository.Setup(repository => repository.GetAll()).Returns(locations);

            return new(languageRepository.Object, locationRepository.Object);
        }

        private static LanguageEntity BuildRomanianLanguage()
            => BuildLanguage("Romanian", "romanian", []);

        private static LanguageEntity BuildLanguage(
            string languageId,
            string languageGameId,
            IEnumerable<string> fallbackLanguages)
            => new()
            {
                Id = languageId,
                Code = new()
                {
                    ISO_639_1 = "ro",
                    ISO_639_2 = "ron",
                    ISO_639_3 = "ron"
                },
                GameIds =
                [
                    new(Game, languageGameId)
                    {
                        Type = "localisation",
                        Parent = "latin",
                        DefaultNameLanguageId = "Romanian"
                    }
                ],
                FallbackLanguages = fallbackLanguages.ToList()
            };

        private static LocationEntity BuildClujLocation()
            => BuildLocation(
                LocationId,
                LocationGameId,
                [
                    new("Romanian", "Cluj-Napoca")
                    {
                        Adjective = "Clujean",
                        Comment = "La umbra Nucului Bătrân"
                    }
                ],
                []);

        private static LocationEntity BuildLocation(
            string locationId,
            string locationGameId,
            IEnumerable<NameEntity> names,
            IEnumerable<string> fallbackLocations)
            => new()
            {
                Id = locationId,
                GeoNamesId = "681290",
                GameIds = [new(Game, locationGameId) { Type = "county" }],
                FallbackLocations = fallbackLocations.ToList(),
                Names = names.ToList()
            };
    }
}