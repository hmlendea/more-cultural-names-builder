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
        [TestCase("Â-ngì-pî-sṳ̂ sân", "Â-ngì-pî-sû sân")]
        [TestCase("Basileia Rhṓmaiṓn", "Basileia Rhõmaiõn")]
        [TestCase("Brægentford", "Brægentford")]
        [TestCase("Budapeşt", "Budapest")]
        [TestCase("Đế quốc Nga", "Ðê quôc Nga")]
        [TestCase("Enkoriџ", "Enkoridž")]
        [TestCase("Farƣona", "Fargona")]
        [TestCase("Linkøbing", "Linkøbing")]
        [TestCase("Lò̤-mā Dá̤-guók", "Lò-mã Dá-guók")]
        [TestCase("Lúksẹ́mbọ̀rg", "Lúksemborg")]
        [TestCase("Moscoƿ", "Moscouu")]
        [TestCase("Ngò-lò-sṳ̂", "Ngò-lò-sû")]
        [TestCase("Novyĭ Margelan", "Novyï Margelan")]
        [TestCase("Semêndria", "Semêndria")]
        [TestCase("Starověký Řím", "Starovêký Rzím")]
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
