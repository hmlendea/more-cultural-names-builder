using System.Linq;

using NUnit.Framework;

using MoreCulturalNamesModBuilder.Service;

namespace MoreCulturalNamesModBuilder.UnitTests.Service
{
    public class NameNormaliserTests
    {
        const string Windows1252Characters = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~€‚ƒ„…†‡ˆ‰Š‹ŒŽ‘’“”•–—˜™š›œžŸ¡¢£¤¥¦§¨©ª«¬®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";
        
        private INameNormaliser nameNormaliser;

        [SetUp]
        public void SetUp()
        {
            this.nameNormaliser = new NameNormaliser();
        }

        [Test]
        [TestCase("Basileia Rhṓmaiṓn", "Basileia Rhōmaiōn")]
        [TestCase("Budapeşt", "Budapeşt")]
        [TestCase("Đế quốc Nga", "Đê quôc Nga")]
        [TestCase("Farƣona", "Farğona")]
        [TestCase("Lò̤-mā Dá̤-guók", "Lò-mā Dá-guók")]
        [TestCase("Lúksẹ́mbọ̀rg", "Lúksẹḿbọrg")]
        [TestCase("Novyĭ Margelan", "Novyĭ Margelan")]
        [TestCase("Starověký Řím", "Starověký Řím")]
        public void GivenAName_WhenNormalisingForCK3_ReturnsTheExpectedNormalisedName(
            string name,
            string expectedResult)
        {
            string actualResult = nameNormaliser.ToCK3Charset(name);

            Assert.AreEqual(expectedResult, actualResult);
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

        [Test]
        [TestCase("‎[]`{}´·ʹʺ–‘’”′∃̧̣̤̓́̀̆̂̌̈̄̍͘áÁàÀăĂắẮẵâÂấẤầẦǎåÅäÄǟãÃȧąāĀảẢạẠậẬæÆǣǢḃḂḅḄćĆĉĈčČċĊçÇďĎđĐḍḌḏḎðÐɖƉɗƊéÉèÈĕêÊếẾềỀểỂěĚëËẽẼėĖęĘēĒḗḖẻẹẸệỆǝƎəƏɛƐǵǴğĞĝĜǧǦġĠģĢɣƔƣƢĥĤḧḦḩḨħĦḥḤḫḪʻíÍìÌĭĬîÎǐǏïÏḯĩĨİįĮīĪịỊıɩʲĵĴǰǩǨķĶḳḲḵḴƙƘĺĹľĽļĻłŁḷḶɬḿḾṃṂⁿńŃǹǸňŇñÑṅṄņŅṇṆɲƝŋŊóÓòÒŏŎôÔốỐồỒổỔǒǑöÖȫȪőŐõÕȯøØǿǾǫǪōŌṓṒơƠờỜỡỠọỌộỘœŒɔṕṔŕŔřŘṛṚśŚŝŜšŠṡṠşŞṣṢșȘßťŤẗţŢṭṬțȚŧŦúÚùÙŭŬûÛǔǓůŮüÜǜǛűŰũŨųŲūŪủưƯứỨụỤṳṲʊƱṿṾẅẄẍẌýÝỳỲŷŶÿẏẎȳȲȝȜźŹžŽżŻẓẒƶƵʐþÞƿǷʼʾʿαάὰεΕέθΘιΙΟόύаАеЕіІјЈкКоОтТџЏ")]
        public void GivenAName_WhenNormalisingForWindow1252_ReturnsTheNameWithoutCharactersOutsideCharset(
            string name)
        {
            string actualResult = nameNormaliser.ToWindows1252(name);

            string charsOutisdeCharset = string.Empty;

            foreach (char c in actualResult)
            {
                if (!Windows1252Characters.Contains(c))
                {
                    charsOutisdeCharset += c;
                }
            }

            Assert.AreEqual(string.Empty, charsOutisdeCharset);
        }
    }
}
