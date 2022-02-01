using System.Linq;

using NUnit.Framework;

using MoreCulturalNamesBuilder.Service;

namespace MoreCulturalNamesBuilder.UnitTests.Service
{
    public class NameNormaliserTests
    {
        const string StringOfVariousCharacters = "‎[]^`{ẖ}´·ʹʺ–—‘’”‡′∃̧̣̤̦̓́̀̆̂̌̈̋̄̍͘áÁàÀăĂắẮẵâÂấẤầẦǎåÅäÄǟǞãÃȧąāĀảẢạẠậẬæÆǣǢḃḂḅḄćĆĉĈčČċĊçÇďĎḑđĐḍḌḏḎðÐɖƉɗƊéÉèÈĕêÊếẾềỀểỂěĚëËẽẼėĖęĘēĒḗḖẻẺẹẸệỆǝƎəƏɛƐǵǴğĞĝĜǧǦġĠģĢɣƔƣƢĥĤḧḦḩḨħĦḥḤḫḪʻíÍìÌĭĬîÎǐǏïÏḯĩĨİįĮīĪịỊıɩʲĵĴǰḱḰǩǨķĶḳḲḵḴƙƘĺĹľĽļĻłŁḷḶɬḿḾṃṂⁿńŃǹǸňŇñÑṅṄņŅṇṆɲƝŋŊóÓòÒŏŎôÔốỐồỒổỔǒǑöÖȫȪőŐõÕȯȮøØǿǾǫǪōŌṓṒơƠờỜỡỠọỌộỘœŒɔṕṔɸŕŔřŘṙṘŗŖṛṚśŚŝŜšŠṡṠşŞṣṢșȘßťŤẗţŢṭṬțȚŧŦúÚùÙŭŬûÛǔǓůŮüÜǜǛűŰũŨųŲūŪủưƯứỨụỤṳṲʊƱṿṾẅẄẍẌýÝỳỲŷŶÿŸẏẎȳȲȝȜźŹžŽżŻẓẒƶƵʐþÞƿǷʼʾʿαάὰεΕέθΘιΙΟόύаАәеЕіІјЈкКќЌоОтТџЏьэЭюяṯ";
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
        [TestCase("al-H̱ānīẗ", "al-Khānīa")]
        [TestCase("āl-Zāwyẗ", "āl-Zāwya")]
        [TestCase("al-ʼAḥsāʼ", "al-´Ahsā´")]
        [TestCase("Aĺbasiete", "Aĺbasiete")]
        [TestCase("an-Nāṣira", "an-Nāșira")]
        [TestCase("Anwākšūṭ", "Anwākšūț")]
        [TestCase("Åsele", "Åsele")]
        [TestCase("Basileia Rhṓmaiṓn", "Basileia Rhōmaiōn")]
        [TestCase("Blāsīnṯīā", "Blāsīnthīā")]
        [TestCase("Brægentford", "Brægentford")]
        [TestCase("B‍risṭ‍riṭā", "Brisțrițā")]
        [TestCase("Budapeşt", "Budapeşt")]
        [TestCase("Bułgar Wielki", "Bułgar Wielki")]
        [TestCase("Bùyínuòsīàilìsī", "Bùyínuòsīàilìsī")]
        [TestCase("Český Krumlov", "Český Krumlov")]
        [TestCase("Chęciny", "Chęciny")]
        [TestCase("Đakovo", "Đakovo")]
        [TestCase("Danmǫrk", "Danmörk")]
        [TestCase("Đế quốc Nga", "Đê quôc Nga")]
        [TestCase("Dobřany", "Dobřany")]
        [TestCase("Ər-Rəqqə", "Ar-Raqqa")]
        [TestCase("Farƣona", "Farğona")]
        [TestCase("Ġhaūdeš", "Ġhaūdeš")]
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
        [TestCase("Mūrīṭnīẗ al-Ṭinǧīẗ", "Mūrīțnīa al-Ținğīa")]
        [TestCase("Nörvêzi", "Nörvêzi")]
        [TestCase("Novyĭ Margelan", "Novyĭ Margelan")]
        [TestCase("Nowĩ", "Nowĩ")]
        [TestCase("Nɔɔrɩvɛɛzɩ", "Noorıveezı")]
        [TestCase("Nuorvegėjė", "Nuorvegėjė")]
        [TestCase("Nūrṯāmbtūn", "Nūrthāmbtūn")]
        [TestCase("Nuwakicɔɔtɩ", "Nuwakicootı")]
        [TestCase("Perejäslavľĭ", "Perejäslavľĭ")]
        [TestCase("Permė", "Permė")]
        [TestCase("ɸlāryo", "Plāryo")]
        [TestCase("Sāg‍rab", "Sāgrab")]
        [TestCase("Saraẖs", "Sarakhs")]
        [TestCase("Starověký Řím", "Starověký Řím")]
        [TestCase("Sveti Đorđe", "Sveti Đorđe")]
        [TestCase("Taɖɛsalam", "Tadesalam")]
        [TestCase("Test ɸlāryoɸ", "Test Plāryop")]
        [TestCase("Truǧālẗ", "Truğāla")]
        [TestCase("Užhorod", "Užhorod")]
        [TestCase("Vialikaja Poĺšča", "Vialikaja Poĺšča")]
        [TestCase("Vюrцby’rg", "Viurcby’rg")]
        [TestCase("Эstoniья", "Estoni'ia")]
        [TestCase("Юli’h", "Iuli’h")]
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
        [TestCase("al-H̱ānīẗ", "al-Khānīa")]
        [TestCase("āl-Zāwyẗ", "āl-Zāwya")]
        [TestCase("al-ʼAḥsāʼ", "al-´Ahsā´")]
        [TestCase("Aĺbasiete", "Aĺbasiete")]
        [TestCase("Anwākšūṭ", "Anwākšūţ")]
        [TestCase("Åsele", "Åsele")]
        [TestCase("Basileia Rhṓmaiṓn", "Basileia Rhōmaiōn")]
        [TestCase("Blāsīnṯīā", "Blāsīnthīā")]
        [TestCase("Brægentford", "Brægentford")]
        [TestCase("B‍risṭ‍riṭā", "Brisţriţā")]
        [TestCase("Bùyínuòsīàilìsī", "Bùyínuòsīàilìsī")]
        [TestCase("Český Krumlov", "Český Krumlov")]
        [TestCase("Đakovo", "Đakovo")]
        [TestCase("Dobřany", "Dobřany")]
        [TestCase("Ər-Rəqqə", "Ar-Raqqa")]
        [TestCase("Ġhaūdeš", "Ġhaūdeš")]
        [TestCase("Góðviðra", "Góðviðra")]
        [TestCase("Jémanị", "Jémanį")]
        [TestCase("K’asablank’a", "K´asablank´a")]
        [TestCase("Kašuubimaa", "Kašuubimaa")]
        [TestCase("Lėnkėjė", "Lėnkėjė")]
        [TestCase("Lǐyuērènèilú", "Lĭyuērènèilú")]
        [TestCase("Lò̤-mā Dá̤-guók", "Lò-mā Dá-guók")]
        [TestCase("Loṙow", "Loŕow")]
        [TestCase("Maďarsko", "Maďarsko")]
        [TestCase("Mūrīṭnīẗ al-Ṭinǧīẗ", "Mūrīţnīa al-Ţinğīa")]
        [TestCase("Nörvêzi", "Nörvêzi")]
        [TestCase("Nowĩ", "Nowï")]
        [TestCase("Nɔɔrɩvɛɛzɩ", "Noorıveezı")]
        [TestCase("Nuorvegėjė", "Nuorvegėjė")]
        [TestCase("Nūrṯāmbtūn", "Nūrthāmbtūn")]
        [TestCase("Nuwakicɔɔtɩ", "Nuwakicootı")]
        [TestCase("Perejäslavľĭ", "Perejäslavľĭ")]
        [TestCase("Permė", "Permė")]
        [TestCase("ɸlāryo", "Plāryo")]
        [TestCase("Sāg‍rab", "Sāgrab")]
        [TestCase("Saraẖs", "Sarakhs")]
        [TestCase("Sveti Đorđe", "Sveti Đorđe")]
        [TestCase("Taɖɛsalam", "Tadesalam")]
        [TestCase("Test ɸlāryoɸ", "Test Plāryop")]
        [TestCase("Truǧālẗ", "Truğāla")]
        [TestCase("Užhorod", "Užhorod")]
        [TestCase("Vialikaja Poĺšča", "Vialikaja Poĺšča")]
        [TestCase("Vюrцby’rg", "Viurcby´rg")]
        [TestCase("Эstoniья", "Estoni'ia")]
        [TestCase("Юli’h", "Iuli´h")]
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
        [TestCase("al-H̱ānīẗ", "al-Khãnïa")]
        [TestCase("āl-Zāwyẗ", "ãl-Zãwya")]
        [TestCase("al-ʼAḥsāʼ", "al-´Ahsã´")]
        [TestCase("Aĺbasiete", "Albasiete")]
        [TestCase("an-Nāṣira", "an-Nãsira")]
        [TestCase("Anwākšūṭ", "Anwãkšüt")]
        [TestCase("Åsele", "Åsele")]
        [TestCase("Basileia Rhṓmaiṓn", "Basileia Rhõmaiõn")]
        [TestCase("Blāsīnṯīā", "Blãsïnthïã")]
        [TestCase("Brægentford", "Brægentford")]
        [TestCase("B‍risṭ‍riṭā", "Bristritã")]
        [TestCase("Budapeşt", "Budapest")]
        [TestCase("Bułgar Wielki", "Bulgar Wielki")]
        [TestCase("Bùyínuòsīàilìsī", "Bùyínuòsïàilìsï")]
        [TestCase("Český Krumlov", "Cheský Krumlov")]
        [TestCase("Chęciny", "Checiny")]
        [TestCase("Đakovo", "Ðakovo")]
        [TestCase("Danmǫrk", "Danmörk")]
        [TestCase("Đế quốc Nga", "Ðê quôc Nga")]
        [TestCase("Dobřany", "Dobrzany")]
        [TestCase("Enkoriџ", "Enkoridž")]
        [TestCase("Ər-Rəqqə", "Ar-Raqqa")]
        [TestCase("Farƣona", "Fargona")]
        [TestCase("Ġhaūdeš", "Ghaüdeš")]
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
        [TestCase("Mūrīṭnīẗ al-Ṭinǧīẗ", "Mürïtnïa al-Tinjïa")]
        [TestCase("Ngò-lò-sṳ̂", "Ngò-lò-sû")]
        [TestCase("Nörvêzi", "Nörvêzi")]
        [TestCase("Novyĭ Margelan", "Novyï Margelan")]
        [TestCase("Nowĩ", "Nowï")]
        [TestCase("Nɔɔrɩvɛɛzɩ", "Nooriveezi")]
        [TestCase("Nuorvegėjė", "Nuorvegéjé")]
        [TestCase("Nūrṯāmbtūn", "Nürthãmbtün")]
        [TestCase("Nuwakicɔɔtɩ", "Nuwakicooti")]
        [TestCase("Perejäslavľĭ", "Perejäslavlï")]
        [TestCase("Permė", "Permé")]
        [TestCase("ɸlāryo", "Plãryo")]
        [TestCase("Sāg‍rab", "Sãgrab")]
        [TestCase("Saraẖs", "Sarakhs")]
        [TestCase("Semêndria", "Semêndria")]
        [TestCase("Starověký Řím", "Starovêký Rzím")]
        [TestCase("Sveti Đorđe", "Sveti Ðordže")]
        [TestCase("Taɖɛsalam", "Tadesalam")]
        [TestCase("Test ɸlāryoɸ", "Test Plãryop")]
        [TestCase("Tibískon", "Tibískon")]
        [TestCase("Truǧālẗ", "Trujãla")]
        [TestCase("Užhorod", "Užhorod")]
        [TestCase("Vialikaja Poĺšča", "Vialikaja Polšcha")]
        [TestCase("Vюrцby’rg", "Viurcby’rg")]
        [TestCase("Эstoniья", "Estoni'ia")]
        [TestCase("Юli’h", "Iuli’h")]
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
