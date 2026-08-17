using Moq;

using NuciText.Conversion;

using NUnit.Framework;

using MoreCulturalNamesBuilder.Service;

namespace MoreCulturalNamesBuilder.UnitTests.Service
{
    [TestFixture]
    public sealed class NameNormaliserEdgeCaseTests
    {
        private NameNormaliser nameNormaliser = null!;

        [SetUp]
        public void SetUp() => nameNormaliser = new(new NuciTextConverter());

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\r")]
        [TestCase("\n")]
        [TestCase("\r\n")]
        [TestCase(" \t\r\n ")]
        public void GivenAnEmptyName_WhenNormalisingForCK3_ThenAnEmptyNameIsReturned(string name)
            => Assert.That(nameNormaliser.ToCK3Charset(name), Is.Empty);

        [Test]
        public void GivenAPreviouslyNormalisedName_WhenNormalisingForCK3_ThenTheCachedNameIsReturned()
        {
            string firstResult = nameNormaliser.ToCK3Charset("Cluj-Napoca");

            string secondResult = nameNormaliser.ToCK3Charset("Cluj-Napoca");

            Assert.That(secondResult, Is.SameAs(firstResult));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\r")]
        [TestCase("\n")]
        [TestCase("\r\n")]
        [TestCase(" \t\r\n ")]
        public void GivenAnEmptyName_WhenNormalisingForAHOI4City_ThenAnEmptyNameIsReturned(string name)
            => Assert.That(nameNormaliser.ToHOI4CityCharset(name), Is.Empty);

        [Test]
        public void GivenAPreviouslyNormalisedName_WhenNormalisingForAHOI4City_ThenTheCachedNameIsReturned()
        {
            string firstResult = nameNormaliser.ToHOI4CityCharset("Cluj-Napoca");

            string secondResult = nameNormaliser.ToHOI4CityCharset("Cluj-Napoca");

            Assert.That(secondResult, Is.SameAs(firstResult));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\r")]
        [TestCase("\n")]
        [TestCase("\r\n")]
        [TestCase(" \t\r\n ")]
        public void GivenAnEmptyName_WhenNormalisingForAHOI4State_ThenAnEmptyNameIsReturned(string name)
            => Assert.That(nameNormaliser.ToHOI4StateCharset(name), Is.Empty);

        [TestCase("Romania", "Romania")]
        [TestCase("Ġhana", "Ghana")]
        [TestCase("ġhana", "ghana")]
        [TestCase("Ġana", "Ghana")]
        [TestCase("ġana", "ghana")]
        [TestCase("iīẗ", "iyyah")]
        [TestCase("īẗ", "iyah")]
        [TestCase("Ġhana ġhana Ġana ġana iīẗ īẗ", "Ghana ghana Ghana ghana iyyah iyah")]
        public void GivenAStateName_WhenNormalisingForAHOI4State_ThenTheExpectedNameIsReturned(
            string name,
            string expectedName)
            => Assert.That(nameNormaliser.ToHOI4StateCharset(name), Is.EqualTo(expectedName));

        [Test]
        public void GivenAPreviouslyNormalisedName_WhenNormalisingForAHOI4State_ThenTheCachedNameIsReturned()
        {
            string firstResult = nameNormaliser.ToHOI4StateCharset("Romania");

            string secondResult = nameNormaliser.ToHOI4StateCharset("Romania");

            Assert.That(secondResult, Is.SameAs(firstResult));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\r")]
        [TestCase("\n")]
        [TestCase("\r\n")]
        [TestCase(" \t\r\n ")]
        public void GivenAnEmptyName_WhenNormalisingForImperatorRome_ThenAnEmptyNameIsReturned(string name)
            => Assert.That(nameNormaliser.ToImperatorRomeCharset(name), Is.Empty);

        [Test]
        public void GivenAPreviouslyNormalisedName_WhenNormalisingForImperatorRome_ThenTheCachedNameIsReturned()
        {
            string firstResult = nameNormaliser.ToImperatorRomeCharset("Cluj-Napoca");

            string secondResult = nameNormaliser.ToImperatorRomeCharset("Cluj-Napoca");

            Assert.That(secondResult, Is.SameAs(firstResult));
        }

        [Test]
        public void GivenAName_WhenNormalisingForWindows1252_ThenTheTextConverterResultIsReturned()
        {
            Mock<INuciTextConverter> textConverter = new();
            textConverter
                .Setup(converter => converter.ToWindows1252("Cluj-Napoca"))
                .Returns("Newport");
            nameNormaliser = new(textConverter.Object);

            string result = nameNormaliser.ToWindows1252("Cluj-Napoca");

            Assert.That(result, Is.EqualTo("Newport"));
        }
    }
}