using System.Linq;

using NUnit.Framework;

using MoreCulturalNamesModBuilder.Service;

namespace MoreCulturalNamesModBuilder.UnitTests.Service
{
    public class NameNormaliserTests
    {
        const string StringOfVariousCharacters = "‎[]`{}´·ʹʺ–—‘’”′∃̧̣̤̓́̀̆̂̌̈̄̍͘áÁàÀăĂắẮẵâÂấẤầẦǎåÅäÄǟǞãÃȧąāĀảẢạẠậẬæÆǣǢḃḂḅḄćĆĉĈčČċĊçÇďĎđĐḍḌḏḎðÐɖƉɗƊéÉèÈĕêÊếẾềỀểỂěĚëËẽẼėĖęĘēĒḗḖẻẺẹẸệỆǝƎəƏɛƐǵǴğĞĝĜǧǦġĠģĢɣƔƣƢĥĤḧḦḩḨħĦḥḤḫḪʻíÍìÌĭĬîÎǐǏïÏḯĩĨİįĮīĪịỊıɩʲĵĴǰǩǨķĶḳḲḵḴƙƘĺĹľĽļĻłŁḷḶɬḿḾṃṂⁿńŃǹǸňŇñÑṅṄņŅṇṆɲƝŋŊóÓòÒŏŎôÔốỐồỒổỔǒǑöÖȫȪőŐõÕȯȮøØǿǾǫǪōŌṓṒơƠờỜỡỠọỌộỘœŒɔṕṔŕŔřŘŗŖṛṚśŚŝŜšŠṡṠşŞṣṢșȘßťŤẗţŢṭṬțȚŧŦúÚùÙŭŬûÛǔǓůŮüÜǜǛűŰũŨųŲūŪủưƯứỨụỤṳṲʊƱṿṾẅẄẍẌýÝỳỲŷŶÿŸẏẎȳȲȝȜźŹžŽżŻẓẒƶƵʐþÞƿǷʼʾʿαάὰεΕέθΘιΙΟόύаАеЕіІјЈкКќЌоОтТџЏ";
        const string CK3Characters = ".­̦̒̕  _-–—,;:!¡?¿.…·'‘’‚‹›\"“”„«»()[]{}§¶@*/\\&#%‰†‡•`´˜^¯˘˙¨˚˝¸˛ˆˇ°©®∂∏+±÷×<=≠>¬|¦~−⁄∞≈¤¢$£¥€01¹½¼2²3³¾456789aAªáÁàÀăĂâÂåÅäÄãÃąĄāĀæÆǽǼbBcCćĆĉĈčČċĊçÇdDďĎđĐðÐeEéÉèÈĕĔêÊěĚëËėĖęĘēĒfFﬁﬂgGğĞĝĜġĠģĢhHĥĤħĦiIíÍìÌĭĬîÎïÏĩĨİįĮīĪĳĲıjJĵĴȷkKķĶlLĺĹľĽļĻłŁŀĿmMnNńŃňŇñÑņŅoOºóÓòÒŏŎôÔöÖőŐõÕøØǿǾōŌœŒpPqQĸrRŕŔřŘŗŖsSśŚŝŜšŠșȘşŞßtTťŤțȚţŢ™ŧŦuUúÚùÙŭŬûÛůŮüÜűŰũŨųŲūŪvVwWẃẂẁẀŵŴẅẄxXyYýÝỳỲŷŶÿŸzZźŹžŽżŻþÞŉµπ";
        const string HOI4MapCharacters = "­҈҉҆҅҄҇҃  _-,;:!¡?¿.·'\"”«»()[]{}§¶@*/\\&#%`´^¯¨¸°҂©®+±÷×<=>¬|¦~¤¢$£¥01¹½¼2²3³¾456789aAªáÁàÀăĂâÂåÅäÄãÃąĄāĀæÆbBcCćĆĉĈčČċĊçÇdDďĎđĐðÐeEéÉèÈĕĔêÊěĚëËėĖęĘēĒfFgGğĞĝĜġĠģĢhHĥĤħĦiIíÍìÌĭĬîÎïÏĩĨİįĮīĪіІїЇӀĳĲıjJĵĴkKķĶкКќЌқҚӄӃҡҠҟҞҝҜlLĺĹľĽļĻłŁŀĿmMмМӎӍnNńŃňŇñÑņŅŋŊиИѝЍӥӤӣӢҋҊйЙoOºóÓòÒŏŎôÔöÖőŐõÕøØōŌœŒоОӧӦөӨӫӪфФpPqQĸrRŕŔřŘŗŖsSśŚŝŜšŠşŞſßtTťŤţŢŧŦuUúÚùÙŭŬûÛůŮüÜűŰũŨųŲūŪvVwWŵŴxXхХӽӼӿӾҳҲyYýÝŷŶÿŸуУўЎӱӰӳӲӯӮүҮұҰzZźŹžŽżŻþÞŉµаАӑӐӓӒәӘӛӚӕӔбБвВгГѓЃґҐғҒӻӺҕҔӷӶдДђЂҙҘеЕѐЀӗӖёЁєЄжЖӂӁӝӜҗҖзЗӟӞѕЅӡӠјЈлЛӆӅљЉнНӊӉңҢӈӇҥҤњЊпПҧҦҁҀрРҏҎсСҫҪтТҭҬћЋѹѸһҺѡѠѿѾѽѼѻѺцЦҵҴчЧӵӴҷҶӌӋҹҸҽҼҿҾџЏшШщЩъЪыЫӹӸьЬҍҌѣѢэЭӭӬюЮяЯѥѤѧѦѫѪѩѨѭѬѯѮѱѰѳѲѵѴѷѶҩҨӏ";
        const string IRCharacters = "­̦ _-–—,;:!¡?¿.…·'‘’‚‹›\"“”„«»()[]{}§¶@*/\\&#%‰†‡•`´˜^¯˘˙¨˚˝¸˛ˆˇ°©®∂∏∑+±÷×<=≠>¬|¦~−⁄√∞∫≈≤≥◊¤¢$£¥€01¹½¼2²3³¾456789aAªáÁàÀăĂâÂåÅäÄãÃąĄāĀæÆbBcCćĆčČċĊçÇdDďĎđĐðÐeEéÉèÈêÊěĚëËėĖęĘēĒfFﬁﬂƒgGğĞġĠģĢhHħĦiIíÍìÌîÎïÏİįĮīĪĳĲıjJkKķĶlLĺĹľĽļĻłŁŀĿmMnNńŃňŇñÑņŅŋŊoOºóÓòÒôÔöÖőŐõÕøØōŌœŒpPqQrRŕŔřŘŗŖsSśŚšŠşŞșȘßtTťŤţŢțȚ™ŧŦuUúÚùÙûÛůŮüÜűŰųŲūŪvVwWẃẂẁẀŵŴẅẄxXyYýÝỳỲŷŶÿŸzZźŹžŽżŻþÞΔμπΩ";
        const string Windows1252Characters = "_-–—,;:!¡?¿.…·'‘’‚‹›\"“”„«»()[]{}§¶@*/\\&#%‰†‡•`´˜^¯¨¸ˆ°©®+±÷×<=>¬|¦~¤¢$£¥€01¹½¼2²3³¾456789aAªáÁàÀâÂåÅäÄãÃæÆbBcCçÇdDðÐeEéÉèÈêÊëËfFƒgGhHiIíÍìÌîÎïÏjJkKlLmMnNñÑoOºóÓòÒôÔöÖõÕøØœŒpPqQrRsSšŠßtT™uUúÚùÙûÛüÜvVwWxXyYýÝÿŸzZžŽþÞµ";
        
        private INameNormaliser nameNormaliser;

        [SetUp]
        public void SetUp()
        {
            this.nameNormaliser = new NameNormaliser();
        }

        [Test]
        [TestCase("Aǧīm", "Ağīm")]
        [TestCase("Aḫmīm", "Akhmīm")]
        [TestCase("Aǩsubaj", "Aksubaj")]
        [TestCase("al-Basīṭ", "al-Basīț")]
        [TestCase("al-Ǧubayl", "al-Ğubayl")]
        [TestCase("al-ʼAḥsāʼ", "al-´Ahsā´")]
        [TestCase("an-Nāṣira", "an-Nāșira")]
        [TestCase("Åsele", "Åsele")]
        [TestCase("Basileia Rhṓmaiṓn", "Basileia Rhōmaiōn")]
        [TestCase("Budapeşt", "Budapeşt")]
        [TestCase("Bułgar Wielki", "Bułgar Wielki")]
        [TestCase("Bùyínuòsīàilìsī", "Bùyínuòsīàilìsī")]
        [TestCase("Chęciny", "Chęciny")]
        [TestCase("Danmǫrk", "Danmörk")]
        [TestCase("Đế quốc Nga", "Đê quôc Nga")]
        [TestCase("Ər-Rəqqə", "Ar-Raqqa")]
        [TestCase("Farƣona", "Farğona")]
        [TestCase("Góðviðra", "Góðviðra")]
        [TestCase("Jémanị", "Jémanį")]
        [TestCase("K’asablank’a", "K’asablank’a")]
        [TestCase("Kašuubimaa", "Kašuubimaa")]
        [TestCase("Lǐyuērènèilú", "Lĭyuērènèilú")]
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
        [TestCase("Aǧīm", "Ağīm")]
        [TestCase("Aḫmīm", "Akhmīm")]
        [TestCase("Aǩsubaj", "Aќsubaj")]
        [TestCase("al-Basīṭ", "al-Basīţ")]
        [TestCase("al-Ǧubayl", "al-Ğubayl")]
        [TestCase("al-ʼAḥsāʼ", "al-´Ahsā´")]
        [TestCase("Åsele", "Åsele")]
        [TestCase("Bùyínuòsīàilìsī", "Bùyínuòsīàilìsī")]
        [TestCase("Ər-Rəqqə", "Ar-Raqqa")]
        [TestCase("Góðviðra", "Góðviðra")]
        [TestCase("Jémanị", "Jémanį")]
        [TestCase("K’asablank’a", "K´asablank´a")]
        [TestCase("Kašuubimaa", "Kašuubimaa")]
        [TestCase("Lǐyuērènèilú", "Lĭyuērènèilú")]
        public void WhenNormalisingForHOI4City_ReturnsTheExpectedNormalisedName(
            string name,
            string expectedResult)
        {
            string actualResult = nameNormaliser.ToHOI4CityCharset(name);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        [TestCase("Â-ngì-pî-sṳ̂ sân", "Â-ngì-pî-sû sân")]
        [TestCase("Aǧīm", "Ajïm")]
        [TestCase("Aḫmīm", "Akhmïm")]
        [TestCase("Aǩsubaj", "Aksubaj")]
        [TestCase("al-Basīṭ", "al-Basït")]
        [TestCase("al-Ǧubayl", "al-Jubayl")]
        [TestCase("al-ʼAḥsāʼ", "al-´Ahsã´")]
        [TestCase("an-Nāṣira", "an-Nãsira")]
        [TestCase("Åsele", "Åsele")]
        [TestCase("Basileia Rhṓmaiṓn", "Basileia Rhõmaiõn")]
        [TestCase("Brægentford", "Brægentford")]
        [TestCase("Budapeşt", "Budapest")]
        [TestCase("Bułgar Wielki", "Bulgar Wielki")]
        [TestCase("Bùyínuòsīàilìsī", "Bùyínuòsïàilìsï")]
        [TestCase("Chęciny", "Checiny")]
        [TestCase("Danmǫrk", "Danmörk")]
        [TestCase("Đế quốc Nga", "Ðê quôc Nga")]
        [TestCase("Enkoriџ", "Enkoridž")]
        [TestCase("Ər-Rəqqə", "Ar-Raqqa")]
        [TestCase("Farƣona", "Fargona")]
        [TestCase("Góðviðra", "Góðviðra")]
        [TestCase("Jémanị", "Jémani")]
        [TestCase("K’asablank’a", "K’asablank’a")]
        [TestCase("Kašuubimaa", "Kašuubimaa")]
        [TestCase("Linkøbing", "Linkøbing")]
        [TestCase("Lǐyuērènèilú", "Lïyuërènèilú")]
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
        public void WhenNormalisingForHOI4City_ReturnsTheNameWithoutCharsOutsideCharset(
            string name)
        {
            string actualResult = nameNormaliser.ToHOI4CityCharset(name);
            TestCharsNotOutsideSet(actualResult, HOI4MapCharacters);
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
            string charsOutisdeCharset = string.Concat(
                str.Where(c => !charset.Contains(c)));

            Assert.AreEqual(string.Empty, charsOutisdeCharset);
        }
    }
}
