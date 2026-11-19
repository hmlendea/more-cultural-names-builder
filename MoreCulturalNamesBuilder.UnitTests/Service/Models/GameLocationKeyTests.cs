using MoreCulturalNamesBuilder.Service.Models;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.UnitTests.Service.Models
{
    [TestFixture]
    public sealed class GameLocationKeyTests
    {
        [TestCase("CK3", "c_cluj", "CK3", "c_cluj", true)]
        [TestCase("CK3", "c_cluj", "CK2", "c_cluj", false)]
        [TestCase("CK3", "c_cluj", "CK3", "c_oradea", false)]
        [TestCase("CK3", "c_cluj", "ck3", "c_cluj", false)]
        public void GivenTwoKeys_WhenComparingThem_ThenEqualityFollowsGameAndIdentifier(
            string firstGame,
            string firstId,
            string secondGame,
            string secondId,
            bool areEqual)
        {
            GameLocationKey firstKey = new(firstGame, firstId);
            GameLocationKey secondKey = new(secondGame, secondId);

            Assert.That(firstKey.Equals(secondKey), Is.EqualTo(areEqual));
        }

        [Test]
        public void GivenAKey_WhenComparingWithNull_ThenTheyAreNotEqual()
            => Assert.That(new GameLocationKey("CK3", "c_cluj").Equals(null), Is.False);

        [Test]
        public void GivenAKey_WhenComparingWithAnotherObjectType_ThenTheyAreNotEqual()
        {
            GameLocationKey key = new("CK3", "c_cluj");
            object otherObject = "CK3";

            Assert.That(key.Equals(otherObject), Is.False);
        }

        [TestCase("Vic3", "STATE_CLUJ")]
        [TestCase("IR", "42")]
        [TestCase("HOI4", "4")]
        public void GivenEqualKeys_WhenRequestingHashCodes_ThenTheHashCodesMatch(
            string game,
            string id)
        {
            GameLocationKey firstKey = new(game, id);
            GameLocationKey secondKey = new(game, id);

            Assert.That(firstKey.GetHashCode(), Is.EqualTo(secondKey.GetHashCode()));
        }

        [Test]
        public void GivenAnEqualKeyBoxedAsAnObject_WhenComparing_ThenTheyAreEqual()
        {
            GameLocationKey key = new("IR", "42");
            object boxedKey = key;

            Assert.Multiple(() =>
            {
                Assert.That(boxedKey, Is.TypeOf<GameLocationKey>());
                Assert.That(key.Equals(boxedKey), Is.True);
                Assert.That(new GameLocationKey("IR", "42").Equals(boxedKey), Is.True);
            });
        }
    }
}
