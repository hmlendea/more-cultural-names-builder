using System.Linq;

using NUnit.Framework;

using MoreCulturalNamesModBuilder.Service;

namespace MoreCulturalNamesModBuilder.UnitTests.Service
{
    public class NameNormaliserTests
    {
        const string StringOfVariousCharacters = "‎‎‎[]`{}´·ʹʺ–—‘’”′∃̧̣̤̓́̀̆̂̌̈̄̍͘áÁàÀăĂắẮẵâÂấẤầẦǎåÅäÄǟǞãÃȧąāĀảẢạẠậẬæÆǣǢḃḂḅḄćĆĉĈčČċĊçÇďĎđĐḍḌḏḎðÐɖƉɗƊéÉèÈĕêÊếẾềỀểỂěĚëËẽẼėĖęĘēĒḗḖẻẺẹẸệỆǝƎəƏɛƐǵǴğĞĝĜǧǦġĠģĢɣƔƣƢĥĤḧḦḩḨħĦḥḤḫḪʻíÍìÌĭĬîÎǐǏïÏḯĩĨİįĮīĪịỊıɩʲĵĴǰǩǨķĶḳḲḵḴƙƘĺĹľĽļĻłŁḷḶɬḿḾṃṂⁿńŃǹǸňŇñÑṅṄņŅṇṆɲƝŋŊóÓòÒŏŎôÔốỐồỒổỔǒǑöÖȫȪőŐõÕȯȮøØǿǾǫǪōŌṓṒơƠờỜỡỠọỌộỘœŒɔṕṔŕŔřŘŗŖṛṚśŚŝŜšŠṡṠşŞṣṢșȘßťŤẗţŢṭṬțȚŧŦúÚùÙŭŬûÛǔǓůŮüÜǜǛűŰũŨųŲūŪủưƯứỨụỤṳṲʊƱṿṾẅẄẍẌýÝỳỲŷŶÿŸẏẎȳȲȝȜźŹžŽżŻẓẒƶƵʐþÞƿǷʼʾʿαάὰεΕέθΘιΙΟόύаАеЕіІјЈкКоОтТџЏ";
        const string CK3Characters = ".­̦̒̕  _-–—,;:!¡?¿.…·'‘’‚‹›\"“”„«»()[]{}§¶@*/\\&#%‰†‡•`´˜^¯˘˙¨˚˝¸˛ˆˇ°©®∂∏+±÷×<=≠>¬|¦~−⁄∞≈¤¢$£¥€01¹½¼2²3³¾456789aAªáÁàÀăĂâÂåÅäÄãÃąĄāĀæÆǽǼbBcCćĆĉĈčČċĊçÇdDďĎđĐðÐeEéÉèÈĕĔêÊěĚëËėĖęĘēĒfFﬁﬂgGğĞĝĜġĠģĢhHĥĤħĦiIíÍìÌĭĬîÎïÏĩĨİįĮīĪĳĲıjJĵĴȷkKķĶlLĺĹľĽļĻłŁŀĿmMnNńŃňŇñÑņŅoOºóÓòÒŏŎôÔöÖőŐõÕøØǿǾōŌœŒpPqQĸrRŕŔřŘŗŖsSśŚŝŜšŠșȘşŞßtTťŤțȚţŢ™ŧŦuUúÚùÙŭŬûÛůŮüÜűŰũŨųŲūŪvVwWẃẂẁẀŵŴẅẄxXyYýÝỳỲŷŶÿŸzZźŹžŽżŻþÞŉµπ";
        const string IRCharacters = "­̦ _-–—,;:!¡?¿.…·'‘’‚‹›\"“”„«»()[]{}§¶@*/\\&#%‰†‡•`´˜^¯˘˙¨˚˝¸˛ˆˇ°©®∂∏∑+±÷×<=≠>¬|¦~−⁄√∞∫≈≤≥◊¤¢$£¥€01¹½¼2²3³¾456789aAªáÁàÀăĂâÂåÅäÄãÃąĄāĀæÆbBcCćĆčČċĊçÇdDďĎđĐðÐeEéÉèÈêÊěĚëËėĖęĘēĒfFﬁﬂƒgGğĞġĠģĢhHħĦiIíÍìÌîÎïÏİįĮīĪĳĲıjJkKķĶlLĺĹľĽļĻłŁŀĿmMnNńŃňŇñÑņŅŋŊoOºóÓòÒôÔöÖőŐõÕøØōŌœŒpPqQrRŕŔřŘŗŖsSśŚšŠşŞșȘßtTťŤţŢțȚ™ŧŦuUúÚùÙûÛůŮüÜűŰųŲūŪvVwWẃẂẁẀŵŴẅẄxXyYýÝỳỲŷŶÿŸzZźŹžŽżŻþÞΔμπΩ";
        const string Windows1252Characters = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~€‚ƒ„…†‡ˆ‰Š‹ŒŽ‘’“”•–—˜™š›œžŸ¡¢£¤¥¦§¨©ª«¬®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";
        
        private INameNormaliser nameNormaliser;

        [SetUp]
        public void SetUp()
        {
            this.nameNormaliser = new NameNormaliser();
        }

        [Test]
        [TestCase("an-Nāṣira", "an-Nāșira")]
        [TestCase("Basileia Rhṓmaiṓn", "Basileia Rhōmaiōn")]
        [TestCase("Budapeşt", "Budapeşt")]
        [TestCase("Bułgar Wielki", "Bułgar Wielki")]
        [TestCase("Chęciny", "Chęciny")]
        [TestCase("Danmǫrk", "Danmörk")]
        [TestCase("Đế quốc Nga", "Đê quôc Nga")]
        [TestCase("Farƣona", "Farğona")]
        [TestCase("K’asablank’a", "K´asablank´a")]
        [TestCase("Lò̤-mā Dá̤-guók", "Lò-mā Dá-guók")]
        [TestCase("Lúksẹ́mbọ̀rg", "Lúksemborg")]
        [TestCase("Novyĭ Margelan", "Novyĭ Margelan")]
        [TestCase("Starověký Řím", "Starověký Řím")]
        public void WhenNormalisingForCK3_ReturnsTheExpectedNormalisedName(
            string name,
            string expectedResult)
        {
            string actualResult = nameNormaliser.ToCK3Charset(name);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        [TestCase("Â-ngì-pî-sṳ̂ sân", "Â-ngì-pî-sû sân")]
        [TestCase("an-Nāṣira", "an-Nãsira")]
        [TestCase("Basileia Rhṓmaiṓn", "Basileia Rhõmaiõn")]
        [TestCase("Brægentford", "Brægentford")]
        [TestCase("Budapeşt", "Budapest")]
        [TestCase("Bułgar Wielki", "Bulgar Wielki")]
        [TestCase("Chęciny", "Checiny")]
        [TestCase("Danmǫrk", "Danmörk")]
        [TestCase("Đế quốc Nga", "Ðê quôc Nga")]
        [TestCase("Enkoriџ", "Enkoridž")]
        [TestCase("Farƣona", "Fargona")]
        [TestCase("K’asablank’a", "K´asablank´a")]
        [TestCase("Linkøbing", "Linkøbing")]
        [TestCase("Lò̤-mā Dá̤-guók", "Lò-mã Dá-guók")]
        [TestCase("Lúksẹ́mbọ̀rg", "Lúksemborg")]
        [TestCase("Moscoƿ", "Moscouu")]
        [TestCase("Ngò-lò-sṳ̂", "Ngò-lò-sû")]
        [TestCase("Novyĭ Margelan", "Novyï Margelan")]
        [TestCase("Semêndria", "Semêndria")]
        [TestCase("Starověký Řím", "Starovêký Rzím")]
        [TestCase("Tibískon", "Tibískon")]
        public void WhenNormalisingForWindow1252_ReturnsTheExpectedNormalisedName(
            string name,
            string expectedResult)
        {
            string actualResult = nameNormaliser.ToWindows1252(name);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        [TestCase(StringOfVariousCharacters)]
        public void WhenNormalisingForCK3_ReturnsTheNameWithoutCharsOutsideCharset(
            string name)
        {
            string actualResult = nameNormaliser.ToCK3Charset(name);
            TestCharsNotOutsideSet(actualResult, CK3Characters);
        }

        [Test]
        [TestCase(StringOfVariousCharacters)]
        public void WhenNormalisingForIR_ReturnsTheNameWithoutCharsOutsideCharset(
            string name)
        {
            string actualResult = nameNormaliser.ToImperatorRomeCharset(name);
            TestCharsNotOutsideSet(actualResult, IRCharacters);
        }

        [Test]
        [TestCase(StringOfVariousCharacters)]
        public void WhenNormalisingForWindow1252_ReturnsTheNameWithoutCharsOutsideCharset(
            string name)
        {
            string actualResult = nameNormaliser.ToWindows1252(name);
            TestCharsNotOutsideSet(actualResult, Windows1252Characters);
        }

        void TestCharsNotOutsideSet(string str, string charset)
        {
            string charsOutisdeCharset = string.Empty;

            foreach (char c in str)
            {
                if (!charset.Contains(c))
                {
                    charsOutisdeCharset += c;
                }
            }

            Assert.AreEqual(string.Empty, charsOutisdeCharset);
        }
    }
}
