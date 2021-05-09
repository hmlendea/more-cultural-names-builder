using System.Linq;

using NUnit.Framework;

using MoreCulturalNamesModBuilder.Service;

namespace MoreCulturalNamesModBuilder.UnitTests.Service
{
    public class NameNormaliserTests
    {
        const string StringOfVariousCharacters = "‎[]^`{}´·ʹʺ–—‘’”‡′∃̧̣̤̦̓́̀̆̂̌̈̋̄̍͘áÁàÀăĂắẮẵâÂấẤầẦǎåÅäÄǟǞãÃȧąāĀảẢạẠậẬæÆǣǢḃḂḅḄćĆĉĈčČċĊçÇďĎḑđĐḍḌḏḎðÐɖƉɗƊéÉèÈĕêÊếẾềỀểỂěĚëËẽẼėĖęĘēĒḗḖẻẺẹẸệỆǝƎəƏɛƐǵǴğĞĝĜǧǦġĠģĢɣƔƣƢĥĤḧḦḩḨħĦḥḤḫḪʻíÍìÌĭĬîÎǐǏïÏḯĩĨİįĮīĪịỊıɩʲĵĴǰḱḰǩǨķĶḳḲḵḴƙƘĺĹľĽļĻłŁḷḶɬḿḾṃṂⁿńŃǹǸňŇñÑṅṄņŅṇṆɲƝŋŊóÓòÒŏŎôÔốỐồỒổỔǒǑöÖȫȪőŐõÕȯȮøØǿǾǫǪōŌṓṒơƠờỜỡỠọỌộỘœŒɔṕṔɸŕŔřŘṙṘŗŖṛṚśŚŝŜšŠṡṠşŞṣṢșȘßťŤẗţŢṭṬțȚŧŦúÚùÙŭŬûÛǔǓůŮüÜǜǛűŰũŨųŲūŪủưƯứỨụỤṳṲʊƱṿṾẅẄẍẌýÝỳỲŷŶÿŸẏẎȳȲȝȜźŹžŽżŻẓẒƶƵʐþÞƿǷʼʾʿαάὰεΕέθΘιΙΟόύаАәеЕіІјЈкКќЌоОтТџЏьэЭюя";
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
        [TestCase("Aĺbasiete", "Aĺbasiete")]
        [TestCase("an-Nāṣira", "an-Nāșira")]
        [TestCase("Anwākšūṭ", "Anwākšūț")]
        [TestCase("Åsele", "Åsele")]
        [TestCase("Basileia Rhṓmaiṓn", "Basileia Rhōmaiōn")]
        [TestCase("Budapeşt", "Budapeşt")]
        [TestCase("Bułgar Wielki", "Bułgar Wielki")]
        [TestCase("Bùyínuòsīàilìsī", "Bùyínuòsīàilìsī")]
        [TestCase("Český Krumlov", "Český Krumlov")]
        [TestCase("Chęciny", "Chęciny")]
        [TestCase("Danmǫrk", "Danmörk")]
        [TestCase("Đế quốc Nga", "Đê quôc Nga")]
        [TestCase("Ər-Rəqqə", "Ar-Raqqa")]
        [TestCase("Farƣona", "Farğona")]
        [TestCase("Góðviðra", "Góðviðra")]
        [TestCase("Jémanị", "Jémanį")]
        [TestCase("K’asablank’a", "K’asablank’a")]
        [TestCase("Kašuubimaa", "Kašuubimaa")]
        [TestCase("Lėnkėjė", "Lėnkėjė")]
        [TestCase("Lǐyuērènèilú", "Lĭyuērènèilú")]
        [TestCase("Lò̤-mā Dá̤-guók", "Lò-mā Dá-guók")]
        [TestCase("Loṙow", "Loŕow")]
        [TestCase("Lúksẹ́mbọ̀rg", "Lúksemborg")]
        [TestCase("Maďarsko", "Maďarsko")]
        [TestCase("Nörvêzi", "Nörvêzi")]
        [TestCase("Novyĭ Margelan", "Novyĭ Margelan")]
        [TestCase("Nowĩ", "Nowĩ")]
        [TestCase("Nɔɔrɩvɛɛzɩ", "Noorıveezı")]
        [TestCase("Nuorvegėjė", "Nuorvegėjė")]
        [TestCase("Nuwakicɔɔtɩ", "Nuwakicootı")]
        [TestCase("ɸlāryo", "Plāryo")]
        [TestCase("Starověký Řím", "Starověký Řím")]
        [TestCase("Sveti Đorđe", "Sveti Đorđe")]
        [TestCase("Taɖɛsalam", "Tadesalam")]
        [TestCase("Test ɸlāryoɸ", "Test Plāryop")]
        [TestCase("Vialikaja Poĺšča", "Vialikaja Poĺšča")]
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
        [TestCase("Aĺbasiete", "Aĺbasiete")]
        [TestCase("Anwākšūṭ", "Anwākšūţ")]
        [TestCase("Åsele", "Åsele")]
        [TestCase("Bùyínuòsīàilìsī", "Bùyínuòsīàilìsī")]
        [TestCase("Český Krumlov", "Český Krumlov")]
        [TestCase("Ər-Rəqqə", "Ar-Raqqa")]
        [TestCase("Góðviðra", "Góðviðra")]
        [TestCase("Jémanị", "Jémanį")]
        [TestCase("K’asablank’a", "K´asablank´a")]
        [TestCase("Kašuubimaa", "Kašuubimaa")]
        [TestCase("Lėnkėjė", "Lėnkėjė")]
        [TestCase("Lǐyuērènèilú", "Lĭyuērènèilú")]
        [TestCase("Lò̤-mā Dá̤-guók", "Lò-mā Dá-guók")]
        [TestCase("Loṙow", "Loŕow")]
        [TestCase("Maďarsko", "Maďarsko")]
        [TestCase("Nörvêzi", "Nörvêzi")]
        [TestCase("Nowĩ", "Nowï")]
        [TestCase("Nɔɔrɩvɛɛzɩ", "Noorıveezı")]
        [TestCase("Nuorvegėjė", "Nuorvegėjė")]
        [TestCase("Nuwakicɔɔtɩ", "Nuwakicootı")]
        [TestCase("ɸlāryo", "Plāryo")]
        [TestCase("Sveti Đorđe", "Sveti Đorđe")]
        [TestCase("Taɖɛsalam", "Tadesalam")]
        [TestCase("Test ɸlāryoɸ", "Test Plāryop")]
        [TestCase("Vialikaja Poĺšča", "Vialikaja Poĺšča")]
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
        [TestCase("Aĺbasiete", "Albasiete")]
        [TestCase("an-Nāṣira", "an-Nãsira")]
        [TestCase("Anwākšūṭ", "Anwãkšüt")]
        [TestCase("Åsele", "Åsele")]
        [TestCase("Basileia Rhṓmaiṓn", "Basileia Rhõmaiõn")]
        [TestCase("Brægentford", "Brægentford")]
        [TestCase("Budapeşt", "Budapest")]
        [TestCase("Bułgar Wielki", "Bulgar Wielki")]
        [TestCase("Bùyínuòsīàilìsī", "Bùyínuòsïàilìsï")]
        [TestCase("Český Krumlov", "Cheský Krumlov")]
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
        [TestCase("Lėnkėjė", "Lénkéjé")]
        [TestCase("Linkøbing", "Linkøbing")]
        [TestCase("Lǐyuērènèilú", "Lïyuërènèilú")]
        [TestCase("Lò̤-mā Dá̤-guók", "Lò-mã Dá-guók")]
        [TestCase("Loṙow", "Lorow")]
        [TestCase("Lúksẹ́mbọ̀rg", "Lúksemborg")]
        [TestCase("Maďarsko", "Madarsko")]
        [TestCase("Moscoƿ", "Moscouu")]
        [TestCase("Ngò-lò-sṳ̂", "Ngò-lò-sû")]
        [TestCase("Nörvêzi", "Nörvêzi")]
        [TestCase("Novyĭ Margelan", "Novyï Margelan")]
        [TestCase("Nowĩ", "Nowï")]
        [TestCase("Nɔɔrɩvɛɛzɩ", "Nooriveezi")]
        [TestCase("Nuorvegėjė", "Nuorvegéjé")]
        [TestCase("Nuwakicɔɔtɩ", "Nuwakicooti")]
        [TestCase("ɸlāryo", "Plãryo")]
        [TestCase("Semêndria", "Semêndria")]
        [TestCase("Starověký Řím", "Starovêký Rzím")]
        [TestCase("Sveti Đorđe", "Sveti Ðordže")]
        [TestCase("Taɖɛsalam", "Tadesalam")]
        [TestCase("Test ɸlāryoɸ", "Test Plãryop")]
        [TestCase("Tibískon", "Tibískon")]
        [TestCase("Vialikaja Poĺšča", "Vialikaja Polšcha")]
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
