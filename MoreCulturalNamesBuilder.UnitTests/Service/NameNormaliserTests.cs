using NUnit.Framework;

using MoreCulturalNamesModBuilder.Service;

namespace MoreCulturalNamesModBuilder.UnitTests.Service
{
    public class NameNormaliserTests
    {
        private INameNormaliser nameNormaliser;

        [SetUp]
        public void SetUp()
        {
            this.nameNormaliser = new NameNormaliser();
        }

        [Test]
        [TestCase("Brægentford", "Brægentford")]
        [TestCase("Linkøbing", "Linkøbing")]
        [TestCase("Lúksẹ́mbọ̀rg", "Lúksemborg")]
        [TestCase("Semêndria", "Semêndria")]
        [TestCase("Tibískon", "Tibískon")]
        public void GivenAName_WhenNormalisingForWindow1252_ReturnsTheExpectedNormalisedName(
            string name,
            string expectedResult)
        {
            string actualResult = nameNormaliser.ToWindows1252(name);

            Assert.AreEqual(expectedResult, actualResult);
        }
    }
}
