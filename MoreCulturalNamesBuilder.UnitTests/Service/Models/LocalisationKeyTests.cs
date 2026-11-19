using MoreCulturalNamesBuilder.Service.Models;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.UnitTests.Service.Models
{
    [TestFixture]
    public sealed class LocalisationKeyTests
    {
        [TestCase("Cluj-Napoca", "Romanian", "Cluj-Napoca", "Romanian", true)]
        [TestCase("Cluj-Napoca", "Romanian", "Cluj-Napoca", "English", false)]
        [TestCase("Cluj-Napoca", "Romanian", "Oradea", "Romanian", false)]
        [TestCase("Cluj-Napoca", "Romanian", "cluj-napoca", "Romanian", false)]
        public void GivenTwoKeys_WhenComparingThem_ThenEqualityFollowsLocationAndLanguage(
            string firstLocationId,
            string firstLanguageId,
            string secondLocationId,
            string secondLanguageId,
            bool areEqual)
        {
            LocalisationKey firstKey = new(firstLocationId, firstLanguageId);
            LocalisationKey secondKey = new(secondLocationId, secondLanguageId);

            Assert.That(firstKey.Equals(secondKey), Is.EqualTo(areEqual));
        }

        [Test]
        public void GivenAKey_WhenComparingWithNull_ThenTheyAreNotEqual()
            => Assert.That(
                new LocalisationKey("Cluj-Napoca", "Romanian").Equals(null),
                Is.False);

        [Test]
        public void GivenAKey_WhenComparingWithAnotherObjectType_ThenTheyAreNotEqual()
        {
            LocalisationKey key = new("Cluj-Napoca", "Romanian");
            object otherObject = "Romanian";

            Assert.That(key.Equals(otherObject), Is.False);
        }

        [TestCase("Cluj-Napoca", "Romanian")]
        [TestCase("STATE_CLUJ", "english")]
        public void GivenEqualKeys_WhenRequestingHashCodes_ThenTheHashCodesMatch(
            string locationId,
            string languageId)
        {
            LocalisationKey firstKey = new(locationId, languageId);
            LocalisationKey secondKey = new(locationId, languageId);

            Assert.That(firstKey.GetHashCode(), Is.EqualTo(secondKey.GetHashCode()));
        }
    }
}
