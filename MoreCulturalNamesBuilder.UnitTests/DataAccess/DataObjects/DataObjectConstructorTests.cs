using MoreCulturalNamesBuilder.DataAccess.DataObjects;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.UnitTests.DataAccess.DataObjects
{
    [TestFixture]
    public sealed class DataObjectConstructorTests
    {
        [TestCase("CK3", "c_cluj")]
        [TestCase("Vic3", "STATE_CLUJ")]
        public void GivenAnIdentifier_WhenConstructingAGameIdDataObject_ThenOnlyTheGameAndIdentifierAreSet(
            string game,
            string identifier)
        {
            GameIdDataObject dataObject = new(game, identifier);

            Assert.Multiple(() =>
            {
                Assert.That(dataObject.Game, Is.EqualTo(game));
                Assert.That(dataObject.Id, Is.EqualTo(identifier));
                Assert.That(dataObject.Type, Is.Null);
                Assert.That(dataObject.Parent, Is.Null);
                Assert.That(dataObject.DefaultNameLanguageId, Is.Null);
            });
        }

        [TestCase("Romanian", "Cluj-Napoca")]
        [TestCase("English", "Newport")]
        public void GivenALanguageAndValue_WhenConstructingANameDataObject_ThenOnlyTheLanguageAndValueAreSet(
            string languageId,
            string value)
        {
            NameDataObject dataObject = new(languageId, value);

            Assert.Multiple(() =>
            {
                Assert.That(dataObject.LanguageId, Is.EqualTo(languageId));
                Assert.That(dataObject.Value, Is.EqualTo(value));
                Assert.That(dataObject.Adjective, Is.Null);
                Assert.That(dataObject.Comment, Is.Null);
            });
        }
    }
}
