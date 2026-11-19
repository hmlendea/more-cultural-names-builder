using MoreCulturalNamesBuilder.Service.Models;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.UnitTests.Service.Models
{
    [TestFixture]
    public sealed class TypedGameLocationKeyTests
    {
        [TestCase("CK3", "c_cluj", "county", "CK3", "c_cluj", "county", true)]
        [TestCase("CK3", "c_cluj", "county", "CK3", "c_cluj", "duchy", false)]
        [TestCase("CK3", "c_cluj", "county", "CK3", "c_oradea", "county", false)]
        [TestCase("CK3", "c_cluj", "county", "CK2", "c_cluj", "county", false)]
        [TestCase("CK3", "c_cluj", "county", "CK3", "c_cluj", null, false)]
        public void GivenTwoKeys_WhenComparingThem_ThenEqualityFollowsGameIdAndType(
            string firstGame,
            string firstId,
            string? firstType,
            string secondGame,
            string secondId,
            string? secondType,
            bool areEqual)
        {
            TypedGameLocationKey firstKey = new(firstGame, firstId, firstType);
            TypedGameLocationKey secondKey = new(secondGame, secondId, secondType);

            Assert.That(firstKey.Equals(secondKey), Is.EqualTo(areEqual));
        }

        [TestCase("Hub")]
        [TestCase("State")]
        public void GivenANullType_WhenComparingWithATypedKey_ThenTheyAreNotEqual(
            string type)
        {
            TypedGameLocationKey untypedKey = new("Vic3", "STATE_CLUJ", null);
            TypedGameLocationKey typedKey = new("Vic3", "STATE_CLUJ", type);

            Assert.That(untypedKey.Equals(typedKey), Is.False);
        }

        [Test]
        public void GivenAKey_WhenComparingWithNull_ThenTheyAreNotEqual()
            => Assert.That(
                new TypedGameLocationKey("CK3", "c_cluj", "county").Equals(null),
                Is.False);

        [Test]
        public void GivenAKey_WhenComparingWithAnotherObjectType_ThenTheyAreNotEqual()
        {
            TypedGameLocationKey key = new("CK3", "c_cluj", "county");
            object otherObject = "county";

            Assert.That(key.Equals(otherObject), Is.False);
        }

        [TestCase("HOI4", "42", "State")]
        [TestCase("HOI4", "42", null)]
        public void GivenEqualKeys_WhenRequestingHashCodes_ThenTheHashCodesMatch(
            string game,
            string id,
            string? type)
        {
            TypedGameLocationKey firstKey = new(game, id, type);
            TypedGameLocationKey secondKey = new(game, id, type);

            Assert.That(firstKey.GetHashCode(), Is.EqualTo(secondKey.GetHashCode()));
        }
    }
}
