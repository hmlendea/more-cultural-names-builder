using System.Linq;

using NUnit.Framework;

using MoreCulturalNamesBuilder.Service;

namespace MoreCulturalNamesBuilder.UnitTests.Service
{
    public class NameNormaliserTests
    {
        const string StringOfVariousCharacters = "​‌‍*[]`°´·ʹ–‘’”‡′″꞉＝̷̧̲̬̯̣̤̥̦̮̰̓́̀̆̂̌̊̈̋̃̇̄̍̒̐͘͡ᶻźŹžŽżŻẓẒƶƵʐǯþÞƿǷʔˀʼʾꞌʿΆγθΊΐΌΎχаАәёіІїјЈоџьᐋ";
        const string CK3Characters = ".­̦̒̕  _-–—,;:!¡?¿.…·'‘’‚‹›\"“”„«»()[]{}§¶@*/\\&#%‰†‡•`´˜^¯˘˙¨˚˝¸˛ˆˇ°©®∂∏+±÷×<=≠>¬|¦~−⁄∞≈¤¢$£¥€01¹½¼2²3³¾456789aAªáÁàÀăĂâÂåÅäÄãÃąĄāĀæÆǽǼbBcCćĆĉĈčČċĊçÇdDďĎđĐðÐeEéÉèÈĕĔêÊěĚëËėĖęĘēĒfFﬁﬂgGğĞĝĜġĠģĢhHĥĤħĦiIíÍìÌĭĬîÎïÏĩĨİįĮīĪĳĲıjJĵĴȷkKķĶlLĺĹľĽļĻłŁŀĿmMnNńŃňŇñÑņŅoOºóÓòÒŏŎôÔöÖőŐõÕøØǿǾōŌœŒpPqQĸrRŕŔřŘŗŖsSśŚŝŜšŠșȘşŞßtTťŤțȚţŢ™ŧŦuUúÚùÙŭŬûÛůŮüÜűŰũŨųŲūŪvVwWẃẂẁẀŵŴẅẄxXyYýÝỳỲŷŶÿŸzZźŹžŽżŻþÞŉµπ";
        const string HOI4MapCharacters = "­҈҉҆҅҄҇҃  _-,;:!¡?¿.·'\"”«»()[]{}§¶@*/\\&#%`´^¯¨¸°҂©®+±÷×<=>¬|¦~¤¢$£¥01¹½¼2²3³¾456789aAªáÁàÀăĂâÂåÅäÄãÃąĄāĀæÆbBcCćĆĉĈčČċĊçÇdDďĎđĐðÐeEéÉèÈĕĔêÊěĚëËėĖęĘēĒfFgGğĞĝĜġĠģĢhHĥĤħĦiIíÍìÌĭĬîÎïÏĩĨİįĮīĪіІїЇӀĳĲıjJĵĴkKķĶкКќЌқҚӄӃҡҠҟҞҝҜlLĺĹľĽļĻłŁŀĿmMмМӎӍnNńŃňŇñÑņŅŋŊиИѝЍӥӤӣӢҋҊйЙoOºóÓòÒŏŎôÔöÖőŐõÕøØōŌœŒоОӧӦөӨӫӪфФpPqQĸrRŕŔřŘŗŖsSśŚŝŜšŠşŞſßtTťŤţŢŧŦuUúÚùÙŭŬûÛůŮüÜűŰũŨųŲūŪvVwWŵŴxXхХӽӼӿӾҳҲyYýÝŷŶÿŸуУўЎӱӰӳӲӯӮүҮұҰzZźŹžŽżŻþÞŉµаАӑӐӓӒәӘӛӚӕӔбБвВгГѓЃґҐғҒӻӺҕҔӷӶдДђЂҙҘеЕѐЀӗӖёЁєЄжЖӂӁӝӜҗҖзЗӟӞѕЅӡӠјЈлЛӆӅљЉнНӊӉңҢӈӇҥҤњЊпПҧҦҁҀрРҏҎсСҫҪтТҭҬћЋѹѸһҺѡѠѿѾѽѼѻѺцЦҵҴчЧӵӴҷҶӌӋҹҸҽҼҿҾџЏшШщЩъЪыЫӹӸьЬҍҌѣѢэЭӭӬюЮяЯѥѤѧѦѫѪѩѨѭѬѯѮѱѰѳѲѵѴѷѶҩҨӏ";
        const string IRCharacters = "­̦ _-–—,;:!¡?¿.…·'‘’‚‹›\"“”„«»()[]{}§¶@*/\\&#%‰†‡•`´˜^¯˘˙¨˚˝¸˛ˆˇ°©®∂∏∑+±÷×<=≠>¬|¦~−⁄√∞∫≈≤≥◊¤¢$£¥€01¹½¼2²3³¾456789aAªáÁàÀăĂâÂåÅäÄãÃąĄāĀæÆbBcCćĆčČċĊçÇdDďĎđĐðÐeEéÉèÈêÊěĚëËėĖęĘēĒfFﬁﬂƒgGğĞġĠģĢhHħĦiIíÍìÌîÎïÏİįĮīĪĳĲıjJkKķĶlLĺĹľĽļĻłŁŀĿmMnNńŃňŇñÑņŅŋŊoOºóÓòÒôÔöÖőŐõÕøØōŌœŒpPqQrRŕŔřŘŗŖsSśŚšŠşŞșȘßtTťŤţŢțȚ™ŧŦuUúÚùÙûÛůŮüÜűŰųŲūŪvVwWẃẂẁẀŵŴẅẄxXyYýÝỳỲŷŶÿŸzZźŹžŽżŻþÞΔμπΩ";
        const string Windows1252Characters = "_-–—,;:!¡?¿.…·'‘’‚‹›\"“”„«»()[]{}§¶@*/\\&#%‰†‡•`´˜^¯¨¸ˆ°©®+±÷×<=>¬|¦~¤¢$£¥€01¹½¼2²3³¾456789aAªáÁàÀâÂåÅäÄãÃæÆbBcCçÇdDðÐeEéÉèÈêÊëËfFƒgGhHiIíÍìÌîÎïÏjJkKlLmMnNñÑoOºóÓòÒôÔöÖõÕøØœŒpPqQrRsSšŠßtT™uUúÚùÙûÛüÜvVwWxXyYýÝÿŸzZžŽþÞµ";

        private NameNormaliser nameNormaliser;

        [SetUp]
        public void SetUp() => nameNormaliser = new();

        // Crusader Kings 3
        [Test]
        [TestCase("Ab‌khajiyā", "Abkhajiyā")]
        [TestCase("Aǧīm", "Ağīm")]
        [TestCase("Aḫmīm", "Akhmīm")]
        [TestCase("Ais‍lyāṇḍ", "Aislyāņd")]
        [TestCase("Aǩsubaj", "Aksubaj")]
        [TestCase("al-Basīṭ", "al-Basīț")]
        [TestCase("al-Ǧazīraḧ al-Ḫaḍrāʼ", "al-Ğazīrah al-Khadrā´")]
        [TestCase("al-Ǧubayl", "al-Ğubayl")]
        [TestCase("al-Hāmā al-Arāġūn", "al-Hāmā al-Arāġūn")]
        [TestCase("al-H̱ānīẗ", "al-Khānīah")]
        [TestCase("āl-Zāwyẗ", "āl-Zāwyah")]
        [TestCase("al-ʼAḥsāʼ", "al-´Ahsā´")]
        [TestCase("Aĺbasiete", "Aĺbasiete")]
        [TestCase("Āl‌jāsa", "Āljāsa")]
        [TestCase("an-Nāṣira", "an-Nāșira")]
        [TestCase("And‍riyā", "Andriyā")]
        [TestCase("Anwākšūṭ", "Anwākšūț")]
        [TestCase("Aṗsny", "Apsny")]
        [TestCase("Åsele", "Åsele")]
        [TestCase("Â-ngì-pî-sṳ̂ sân", "Â-ngì-pî-sû sân")]
        [TestCase("Bāḇel", "Bābel")]
        [TestCase("Basileia Rhṓmaiṓn", "Basileia Rhōmaiōn")]
        [TestCase("Bạt Đế Mỗ", "Bat Đê Mô")]
        [TestCase("Blāsīnṯīā", "Blāsīnthīā")]
        [TestCase("Brægentford", "Brægentford")]
        [TestCase("B‍risṭ‍riṭā", "Brisțrițā")]
        [TestCase("Br̥̄sels", "Bŕusels")]
        [TestCase("Budapeşt", "Budapeşt")]
        [TestCase("Bułgar Wielki", "Bułgar Wielki")]
        [TestCase("Bùyínuòsīàilìsī", "Bùyínuòsīàilìsī")]
        [TestCase("Český Krumlov", "Český Krumlov")]
        [TestCase("Cetiǌe", "Cetinje")]
        [TestCase("Chęciny", "Chęciny")]
        [TestCase("Cửu Trại Câu", "Ců'u Trai Câu")]
        [TestCase("Đakovo", "Đakovo")]
        [TestCase("Danmǫrk", "Danmörk")]
        [TestCase("Dasavleti Virǯinia", "Dasavleti Viržinia")]
        [TestCase("Đặng Khẩu", "Đăng Khâu")]
        [TestCase("Đế quốc Nga", "Đê quôc Nga")]
        [TestCase("Dobřany", "Dobřany")]
        [TestCase("Dᶻidᶻəlal̓ič", "Dzidzalalič")]
        [TestCase("Egeyan Kġziner", "Egeyan Kġziner")]
        [TestCase("Enkoriџ", "Enkoridž")]
        [TestCase("Ĕnṭrima", "Ĕnțrima")]
        [TestCase("Ər-Rəqqə", "Ar-Raqqa")]
        [TestCase("Farƣona", "Farğona")]
        [TestCase("Ġhaūdeš", "Ġhaūdeš")]
        [TestCase("Glideroχ", "Glideroch")]
        [TestCase("Góðviðra", "Góðviðra")]
        [TestCase("Grɨnlɛɛn", "Grinleen")]
        [TestCase("G‍roseṭō", "Grosețō")]
        [TestCase("Ǧuzur al-Īǧẗ", "Ğuzur al-Īğah")]
        [TestCase("Ḥadīṯẗ", "Hadīthah")]
        [TestCase("Ȟaȟáwakpa", "Ĥaĥáwakpa")]
        [TestCase("Ḥamāẗ", "Hamāh")]
        [TestCase("H̱rūnīnġn", "Khrūnīnġn")]
        [TestCase("Ins Br̥k", "Ins Bruk")]
        [TestCase("Iṉspruk", "Iņspruk")]
        [TestCase("Ja઼āgreba", "Jaāgreba")]
        [TestCase("Jālaॎs‌burga", "Jālasburga")]
        [TestCase("Jémanị", "Jémanį")]
        [TestCase("Jhānjāṁ", "Jhānjām")]
        [TestCase("K’asablank’a", "K’asablank’a")]
        [TestCase("Kaer Gradaỽc", "Kaer Gradauuc")]
        [TestCase("Kalɩfɔrnii", "Kalıfornii")]
        [TestCase("Kašuubimaa", "Kašuubimaa")]
        [TestCase("Kȁzahstān", "Kàzahstān")]
        [TestCase("Khar‌gōn", "Khargōn")]
        [TestCase("K‍ragujevak", "Kragujevak")]
        [TestCase("Lablaẗ", "Lablah")]
        [TestCase("Lāip‌ॎsiśa", "Lāipsiśa")]
        [TestCase("Lėnkėjė", "Lėnkėjė")]
        [TestCase("Likṟṟaṉ‌sṟṟaiṉ", "Likrransrrain")]
        [TestCase("Linkøbing", "Linkøbing")]
        [TestCase("Lǐyuērènèilú", "Lĭyuērènèilú")]
        [TestCase("Lò̤-mā Dá̤-guók", "Lò-mā Dá-guók")]
        [TestCase("Loṙow", "Loŕow")]
        [TestCase("Luật Tước Đàm", "Luât Tu'óc Đàm")]
        [TestCase("Lǚfádēng", "Lŭfádēng")]
        [TestCase("Lúksẹ́mbọ̀rg", "Lúksemborg")]
        [TestCase("Łużyce", "Łużyce")]
        [TestCase("Maďarsko", "Maďarsko")]
        [TestCase("Mīdīlbūrẖ", "Mīdīlbūrkh")]
        [TestCase("Miniṡoṡeiyoḣdoke Otoƞwe", "Minisoseiyohdoke Otonwe")]
        [TestCase("Miniᐋpulis", "Miniâpulis")]
        [TestCase("Moscoƿ", "Moscouu")]
        [TestCase("Mūrīṭanīẗ al-Ṭinǧīẗ", "Mūrīțanīah al-Ținğīah")]
        [TestCase("Nam̐si", "Namsi")]
        [TestCase("Nazareḟŭ", "Nazarefŭ")]
        [TestCase("Ngò-lò-sṳ̂", "Ngò-lò-sû")]
        [TestCase("Nörvêzi", "Nörvêzi")]
        [TestCase("Novyĭ Margelan", "Novyĭ Margelan")]
        [TestCase("Nowĩ", "Nowĩ")]
        [TestCase("Nɔɔrɩvɛɛzɩ", "Noorıveezı")]
        [TestCase("Nuorvegėjė", "Nuorvegėjė")]
        [TestCase("Nūrṯāmbtūn", "Nūrthāmbtūn")]
        [TestCase("Nuwakicɔɔtɩ", "Nuwakicootı")]
        [TestCase("Ɔsɩloo", "Osıloo")]
        [TestCase("Perejäslavľĭ", "Perejäslavľĭ")]
        [TestCase("Permė", "Permė")]
        [TestCase("Phin‌sṭrām", "Phinsțrām")]
        [TestCase("P‍ṭiyuj", "Pțiyuj")]
        [TestCase("Purūkḷiṉ", "Purūkļin")]
        [TestCase("ɸlāryo", "Plāryo")]
        [TestCase("Qart-Ḥadašt", "Qart-Hadašt")]
        [TestCase("Ra‍yājāna", "Rayājāna")]
        [TestCase("R‍hods", "Rhods")]
        [TestCase("Rừng Bohemia", "Rù'ng Bohemia")]
        [TestCase("Sāg‍rab", "Sāgrab")]
        [TestCase("Sai Ngǭn", "Sai Ngön")]
        [TestCase("Sālj‌barg‌", "Sāljbarg")]
        [TestCase("Šaqūbīẗ", "Šaqūbīah")]
        [TestCase("Saraẖs", "Sarakhs")]
        [TestCase("Semêndria", "Semêndria")]
        [TestCase("Starověký Řím", "Starověký Řím")]
        [TestCase("Sveti Đorđe", "Sveti Đorđe")]
        [TestCase("Taɖɛsalam", "Tadesalam")]
        [TestCase("Taϊpéi", "Taïpéi")]
        [TestCase("Test ɸlāryoɸ", "Test Plāryop")]
        [TestCase("Thượng Volta", "Thu'ong Volta")]
        [TestCase("Tibískon", "Tibískon")]
        [TestCase("Tłnáʔəč", "Tłná´ač")]
        [TestCase("Ṭ‍renṭō", "Țrențō")]
        [TestCase("Truǧālẗ", "Truğālah")]
        [TestCase("Užhorod", "Užhorod")]
        [TestCase("Vialikaja Poĺšča", "Vialikaja Poĺšča")]
        [TestCase("Vюrцby’rg", "Viurcby’rg")]
        [TestCase("Ẇel‌ś‌", "Ẃelś")]
        [TestCase("Вуллонгонг", "Vullongong")]
        [TestCase("Эstoniья", "Estoni'ia")]
        [TestCase("Юli’h", "Iuli’h")]
        [TestCase("پwyrṭūrīkū", "Bwyrțūrīkū")]
        public void WhenNormalisingForCK3_ReturnsTheExpectedNormalisedName(
            string name,
            string expectedResult)
            => Assert.That(nameNormaliser.ToCK3Charset(name), Is.EqualTo(expectedResult));

        // Hearts of Iron 4 Cities
        [Test]
        [TestCase("Â-ngì-pî-sṳ̂ sân", "Â-ngì-pî-sû sân")]
        [TestCase("Ab‌khajiyā", "Abkhajiyā")]
        [TestCase("Aǧīm", "Ağīm")]
        [TestCase("Aḫmīm", "Akhmīm")]
        [TestCase("Ais‍lyāṇḍ", "Aislyāņd")]
        [TestCase("Aǩsubaj", "Aќsubaj")]
        [TestCase("al-Basīṭ", "al-Basīţ")]
        [TestCase("al-Ǧubayl", "al-Ğubayl")]
        [TestCase("al-Hāmā al-Arāġūn", "al-Hāmā al-Arāġūn")]
        [TestCase("al-H̱ānīẗ", "al-Khānīah")]
        [TestCase("āl-Zāwyẗ", "āl-Zāwyah")]
        [TestCase("al-ʼAḥsāʼ", "al-´Ahsā´")]
        [TestCase("Aĺbasiete", "Aĺbasiete")]
        [TestCase("Āl‌jāsa", "Āljāsa")]
        [TestCase("an-Nāṣira", "an-Nāsira")]
        [TestCase("And‍riyā", "Andriyā")]
        [TestCase("Anwākšūṭ", "Anwākšūţ")]
        [TestCase("Aṗsny", "Apsny")]
        [TestCase("Åsele", "Åsele")]
        [TestCase("Bāḇel", "Bābel")]
        [TestCase("Basileia Rhṓmaiṓn", "Basileia Rhōmaiōn")]
        [TestCase("Bạt Đế Mỗ", "Bat Đê Mô")]
        [TestCase("Blāsīnṯīā", "Blāsīnthīā")]
        [TestCase("Brægentford", "Brægentford")]
        [TestCase("B‍risṭ‍riṭā", "Brisţriţā")]
        [TestCase("Br̥̄sels", "Bŕusels")]
        [TestCase("Budapeşt", "Budapeşt")]
        [TestCase("Bułgar Wielki", "Bułgar Wielki")]
        [TestCase("Bùyínuòsīàilìsī", "Bùyínuòsīàilìsī")]
        [TestCase("Český Krumlov", "Český Krumlov")]
        [TestCase("Cetiǌe", "Cetinje")]
        [TestCase("Chęciny", "Chęciny")]
        [TestCase("Đakovo", "Đakovo")]
        [TestCase("Đặng Khẩu", "Đăng Khâu")]
        [TestCase("Danmǫrk", "Danmörk")]
        [TestCase("پwyrṭūrīkū", "Bwyrţūrīkū")]
        [TestCase("Dasavleti Virǯinia", "Dasavleti Viržinia")]
        [TestCase("Đế quốc Nga", "Đê quôc Nga")]
        [TestCase("Dobřany", "Dobřany")]
        [TestCase("Dᶻidᶻəlal̓ič", "Dzidzalalič")]
        [TestCase("Egeyan Kġziner", "Egeyan Kġziner")]
        [TestCase("Enkoriџ", "Enkoridž")]
        [TestCase("Ĕnṭrima", "Ĕnţrima")]
        [TestCase("Ər-Rəqqə", "Ar-Raqqa")]
        [TestCase("Farƣona", "Farğona")]
        [TestCase("Ġhaūdeš", "Ġhaūdeš")]
        [TestCase("Glideroχ", "Glideroch")]
        [TestCase("Góðviðra", "Góðviðra")]
        [TestCase("Grɨnlɛɛn", "Grinleen")]
        [TestCase("G‍roseṭō", "Groseţō")]
        [TestCase("Ḥadīṯẗ", "Hadīthah")]
        [TestCase("Ȟaȟáwakpa", "Ĥaĥáwakpa")]
        [TestCase("Ḥamāẗ", "Hamāh")]
        [TestCase("H̱rūnīnġn", "Khrūnīnġn")]
        [TestCase("Ins Br̥k", "Ins Bruk")]
        [TestCase("Iṉspruk", "Iņspruk")]
        [TestCase("Ja઼āgreba", "Jaāgreba")]
        [TestCase("Jālaॎs‌burga", "Jālasburga")]
        [TestCase("Jémanị", "Jémanį")]
        [TestCase("Jhānjāṁ", "Jhānjām")]
        [TestCase("K’asablank’a", "K´asablank´a")]
        [TestCase("Kaer Gradaỽc", "Kaer Gradauuc")]
        [TestCase("Kalɩfɔrnii", "Kalıfornii")]
        [TestCase("Kašuubimaa", "Kašuubimaa")]
        [TestCase("Kȁzahstān", "Kàzahstān")]
        [TestCase("Khar‌gōn", "Khargōn")]
        [TestCase("K‍ragujevak", "Kragujevak")]
        [TestCase("Lablaẗ", "Lablah")]
        [TestCase("Lāip‌ॎsiśa", "Lāipsiśa")]
        [TestCase("Lėnkėjė", "Lėnkėjė")]
        [TestCase("Likṟṟaṉ‌sṟṟaiṉ", "Likrransrrain")]
        [TestCase("Linkøbing", "Linkøbing")]
        [TestCase("Lǐyuērènèilú", "Lĭyuērènèilú")]
        [TestCase("Lò̤-mā Dá̤-guók", "Lò-mā Dá-guók")]
        [TestCase("Loṙow", "Loŕow")]
        [TestCase("Luật Tước Đàm", "Luât Tu'óc Đàm")]
        [TestCase("Lǚfádēng", "Lŭfádēng")]
        [TestCase("Lúksẹ́mbọ̀rg", "Lúksemborg")]
        [TestCase("Łużyce", "Łużyce")]
        [TestCase("Maďarsko", "Maďarsko")]
        [TestCase("Mīdīlbūrẖ", "Mīdīlbūrkh")]
        [TestCase("Miniṡoṡeiyoḣdoke Otoƞwe", "Minisoseiyohdoke Otoŋwe")]
        [TestCase("Miniᐋpulis", "Miniâpulis")]
        [TestCase("Moscoƿ", "Moscouu")]
        [TestCase("Mūrīṭanīẗ al-Ṭinǧīẗ", "Mūrīţanīah al-Ţinğīah")]
        [TestCase("Nam̐si", "Namsi")]
        [TestCase("Nazareḟŭ", "Nazarefŭ")]
        [TestCase("Ngò-lò-sṳ̂", "Ngò-lò-sû")]
        [TestCase("Nörvêzi", "Nörvêzi")]
        [TestCase("Novyĭ Margelan", "Novyĭ Margelan")]
        [TestCase("Nowĩ", "Nowï")]
        [TestCase("Nɔɔrɩvɛɛzɩ", "Noorıveezı")]
        [TestCase("Nuorvegėjė", "Nuorvegėjė")]
        [TestCase("Nūrṯāmbtūn", "Nūrthāmbtūn")]
        [TestCase("Nuwakicɔɔtɩ", "Nuwakicootı")]
        [TestCase("Ɔsɩloo", "Osıloo")]
        [TestCase("Perejäslavľĭ", "Perejäslavľĭ")]
        [TestCase("Permė", "Permė")]
        [TestCase("Phin‌sṭrām", "Phinsţrām")]
        [TestCase("P‍ṭiyuj", "Pţiyuj")]
        [TestCase("Purūkḷiṉ", "Purūkļin")]
        [TestCase("ɸlāryo", "Plāryo")]
        [TestCase("Qart-Ḥadašt", "Qart-Hadašt")]
        [TestCase("Ra‍yājāna", "Rayājāna")]
        [TestCase("R‍hods", "Rhods")]
        [TestCase("Rừng Bohemia", "Rù'ng Bohemia")]
        [TestCase("Sāg‍rab", "Sāgrab")]
        [TestCase("Sai Ngǭn", "Sai Ngön")]
        [TestCase("Sālj‌barg‌", "Sāljbarg")]
        [TestCase("Šaqūbīẗ", "Šaqūbīah")]
        [TestCase("Saraẖs", "Sarakhs")]
        [TestCase("Semêndria", "Semêndria")]
        [TestCase("Starověký Řím", "Starověký Řím")]
        [TestCase("Sveti Đorđe", "Sveti Đorđe")]
        [TestCase("Taɖɛsalam", "Tadesalam")]
        [TestCase("Taϊpéi", "Taïpéi")]
        [TestCase("Test ɸlāryoɸ", "Test Plāryop")]
        [TestCase("Thượng Volta", "Thu'ong Volta")]
        [TestCase("Tibískon", "Tibískon")]
        [TestCase("Tłnáʔəč", "Tłná´ač")]
        [TestCase("Ṭ‍renṭō", "Ţrenţō")]
        [TestCase("Truǧālẗ", "Truğālah")]
        [TestCase("Užhorod", "Užhorod")]
        [TestCase("Vialikaja Poĺšča", "Vialikaja Poĺšča")]
        [TestCase("Vюrцby’rg", "Viurcby´rg")]
        [TestCase("Ẇel‌ś‌", "Ŵelś")]
        [TestCase("Вуллонгонг", "Vullongong")]
        [TestCase("Эstoniья", "Estoni'ia")]
        [TestCase("Юli’h", "Iuli´h")]
        public void WhenNormalisingForHOI4City_ReturnsTheExpectedNormalisedName(
            string name,
            string expectedResult)
            => Assert.That(nameNormaliser.ToHOI4CityCharset(name), Is.EqualTo(expectedResult));

        [Test]
        [TestCase("Â-ngì-pî-sṳ̂ sân", "Â-ngì-pî-sû sân")]
        [TestCase("Ab‌khajiyā", "Abkhajiyã")]
        [TestCase("Aǧīm", "Ajïm")]
        [TestCase("Aḫmīm", "Akhmïm")]
        [TestCase("Ais‍lyāṇḍ", "Aislyãnd")]
        [TestCase("Aǩsubaj", "Aksubaj")]
        [TestCase("al-Basīṭ", "al-Basït")]
        [TestCase("al-Ǧubayl", "al-Jubayl")]
        [TestCase("al-Hāmā al-Arāġūn", "al-Hãmã al-Arãghün")]
        [TestCase("al-H̱ānīẗ", "al-Khãniyah")]
        [TestCase("āl-Zāwyẗ", "ãl-Zãwyah")]
        [TestCase("al-ʼAḥsāʼ", "al-´Ahsã´")]
        [TestCase("Aĺbasiete", "Albasiete")]
        [TestCase("Āl‌jāsa", "Ãljãsa")]
        [TestCase("an-Nāṣira", "an-Nãsira")]
        [TestCase("And‍riyā", "Andriyã")]
        [TestCase("Anwākšūṭ", "Anwãkšüt")]
        [TestCase("Aṗsny", "Apsny")]
        [TestCase("Åsele", "Åsele")]
        [TestCase("Bāḇel", "Bãbel")]
        [TestCase("Basileia Rhṓmaiṓn", "Basileia Rhõmaiõn")]
        [TestCase("Bạt Đế Mỗ", "Bat Ðê Mô")]
        [TestCase("Blāsīnṯīā", "Blãsïnthïã")]
        [TestCase("Brægentford", "Brægentford")]
        [TestCase("B‍risṭ‍riṭā", "Bristritã")]
        [TestCase("Br̥̄sels", "Brusels")]
        [TestCase("Budapeşt", "Budapest")]
        [TestCase("Bułgar Wielki", "Bulgar Wielki")]
        [TestCase("Bùyínuòsīàilìsī", "Bùyínuòsïàilìsï")]
        [TestCase("Český Krumlov", "Cheský Krumlov")]
        [TestCase("Cetiǌe", "Cetinje")]
        [TestCase("Chęciny", "Checiny")]
        [TestCase("Đakovo", "Ðakovo")]
        [TestCase("Đặng Khẩu", "Ðãng Khâu")]
        [TestCase("Danmǫrk", "Danmörk")]
        [TestCase("پwyrṭūrīkū", "Bwyrtürïkü")]
        [TestCase("Dasavleti Virǯinia", "Dasavleti Viržinia")]
        [TestCase("Đế quốc Nga", "Ðê quôc Nga")]
        [TestCase("Dobřany", "Dobrzany")]
        [TestCase("Dᶻidᶻəlal̓ič", "Dzidzalalich")]
        [TestCase("Egeyan Kġziner", "Egeyan Kghziner")]
        [TestCase("Enkoriџ", "Enkoridž")]
        [TestCase("Ĕnṭrima", "Êntrima")]
        [TestCase("Ər-Rəqqə", "Ar-Raqqa")]
        [TestCase("Farƣona", "Fargona")]
        [TestCase("Ġhaūdeš", "Ghaüdeš")]
        [TestCase("Glideroχ", "Glideroch")]
        [TestCase("Góðviðra", "Góðviðra")]
        [TestCase("Grɨnlɛɛn", "Grinleen")]
        [TestCase("G‍roseṭō", "Grosetõ")]
        [TestCase("Ḥadīṯẗ", "Hadïthah")]
        [TestCase("Ȟaȟáwakpa", "Haháwakpa")]
        [TestCase("Ḥamāẗ", "Hamãh")]
        [TestCase("H̱rūnīnġn", "Khrünïnghn")]
        [TestCase("Ins Br̥k", "Ins Bruk")]
        [TestCase("Iṉspruk", "Inspruk")]
        [TestCase("Ja઼āgreba", "Jaãgreba")]
        [TestCase("Jālaॎs‌burga", "Jãlasburga")]
        [TestCase("Jémanị", "Jémani")]
        [TestCase("Jhānjāṁ", "Jhãnjãm")]
        [TestCase("K’asablank’a", "K’asablank’a")]
        [TestCase("Kaer Gradaỽc", "Kaer Gradauuc")]
        [TestCase("Kalɩfɔrnii", "Kalifornii")]
        [TestCase("Kašuubimaa", "Kašuubimaa")]
        [TestCase("Kȁzahstān", "Kàzahstãn")]
        [TestCase("Khar‌gōn", "Khargõn")]
        [TestCase("K‍ragujevak", "Kragujevak")]
        [TestCase("Lablaẗ", "Lablah")]
        [TestCase("Lāip‌ॎsiśa", "Lãipsisa")]
        [TestCase("Lėnkėjė", "Lénkéjé")]
        [TestCase("Likṟṟaṉ‌sṟṟaiṉ", "Likrransrrain")]
        [TestCase("Linkøbing", "Linkøbing")]
        [TestCase("Lǐyuērènèilú", "Lïyuërènèilú")]
        [TestCase("Lò̤-mā Dá̤-guók", "Lò-mã Dá-guók")]
        [TestCase("Loṙow", "Lorow")]
        [TestCase("Luật Tước Đàm", "Luât Tu'óc Ðàm")]
        [TestCase("Lǚfádēng", "Lüfádëng")]
        [TestCase("Lúksẹ́mbọ̀rg", "Lúksemborg")]
        [TestCase("Łużyce", "Lužyce")]
        [TestCase("Maďarsko", "Madarsko")]
        [TestCase("Mīdīlbūrẖ", "Mïdïlbürkh")]
        [TestCase("Miniṡoṡeiyoḣdoke Otoƞwe", "Minisoseiyohdoke Otonwe")]
        [TestCase("Miniᐋpulis", "Miniâpulis")]
        [TestCase("Moscoƿ", "Moscouu")]
        [TestCase("Mūrīṭanīẗ al-Ṭinǧīẗ", "Mürïtaniyah al-Tinjiyah")]
        [TestCase("Nam̐si", "Namsi")]
        [TestCase("Nazareḟŭ", "Nazarefü")]
        [TestCase("Ngò-lò-sṳ̂", "Ngò-lò-sû")]
        [TestCase("Nörvêzi", "Nörvêzi")]
        [TestCase("Novyĭ Margelan", "Novyï Margelan")]
        [TestCase("Nowĩ", "Nowï")]
        [TestCase("Nɔɔrɩvɛɛzɩ", "Nooriveezi")]
        [TestCase("Nuorvegėjė", "Nuorvegéjé")]
        [TestCase("Nūrṯāmbtūn", "Nürthãmbtün")]
        [TestCase("Nuwakicɔɔtɩ", "Nuwakicooti")]
        [TestCase("Ɔsɩloo", "Osiloo")]
        [TestCase("Perejäslavľĭ", "Perejäslavlï")]
        [TestCase("Permė", "Permé")]
        [TestCase("Phin‌sṭrām", "Phinstrãm")]
        [TestCase("P‍ṭiyuj", "Ptiyuj")]
        [TestCase("Purūkḷiṉ", "Purüklin")]
        [TestCase("ɸlāryo", "Plãryo")]
        [TestCase("Qart-Ḥadašt", "Qart-Hadašt")]
        [TestCase("Ra‍yājāna", "Rayãjãna")]
        [TestCase("R‍hods", "Rhods")]
        [TestCase("Rừng Bohemia", "Rù'ng Bohemia")]
        [TestCase("Sāg‍rab", "Sãgrab")]
        [TestCase("Sai Ngǭn", "Sai Ngön")]
        [TestCase("Sālj‌barg‌", "Sãljbarg")]
        [TestCase("Šaqūbīẗ", "Šaqübiyah")]
        [TestCase("Saraẖs", "Sarakhs")]
        [TestCase("Semêndria", "Semêndria")]
        [TestCase("Starověký Řím", "Starovêký Rzím")]
        [TestCase("Sveti Đorđe", "Sveti Ðordže")]
        [TestCase("Taɖɛsalam", "Tadesalam")]
        [TestCase("Taϊpéi", "Taïpéi")]
        [TestCase("Test ɸlāryoɸ", "Test Plãryop")]
        [TestCase("Thượng Volta", "Thu'ong Volta")]
        [TestCase("Tibískon", "Tibískon")]
        [TestCase("Tłnáʔəč", "Tlná´ach")]
        [TestCase("Ṭ‍renṭō", "Trentõ")]
        [TestCase("Truǧālẗ", "Trujãlah")]
        [TestCase("Užhorod", "Užhorod")]
        [TestCase("Vialikaja Poĺšča", "Vialikaja Polšcha")]
        [TestCase("Vюrцby’rg", "Viurcby’rg")]
        [TestCase("Ẇel‌ś‌", "Wels")]
        [TestCase("Вуллонгонг", "Vullongong")]
        [TestCase("Эstoniья", "Estoni'ia")]
        [TestCase("Юli’h", "Iuli’h")]
        public void WhenNormalisingForWindow1252_ReturnsTheExpectedNormalisedName(
            string name,
            string expectedResult)
            => Assert.That(nameNormaliser.ToWindows1252(name), Is.EqualTo(expectedResult));

        // Imperator Rome
        [Test]
        [TestCase("Â-ngì-pî-sṳ̂ sân", "Â-ngì-pî-sû sân")]
        [TestCase("Ab‌khajiyā", "Abkhajiyā")]
        [TestCase("Aǧīm", "Ajīm")]
        [TestCase("Aḫmīm", "Aḫmīm")]
        [TestCase("Ais‍lyāṇḍ", "Aislyāṇḍ")]
        [TestCase("Aǩsubaj", "Aksubaj")]
        [TestCase("al-Basīṭ", "al-Basīt")]
        [TestCase("al-Ǧubayl", "al-Jubayl")]
        [TestCase("al-Hāmā al-Arāġūn", "al-Hāmā al-Arāghūn")]
        [TestCase("al-H̱ānīẗ", "al-Khāniyah")]
        [TestCase("āl-Zāwyẗ", "āl-Zāwyah")]
        [TestCase("al-ʼAḥsāʼ", "al-´Ahsā´")]
        [TestCase("Aĺbasiete", "Albasiete")]
        [TestCase("Āl‌jāsa", "Āljāsa")]
        [TestCase("an-Nāṣira", "an-Nāsira")]
        [TestCase("And‍riyā", "Andriyā")]
        [TestCase("Anwākšūṭ", "Anwākshūt")]
        [TestCase("Aṗsny", "Apsny")]
        [TestCase("Åsele", "Åsele")]
        [TestCase("Bāḇel", "Bābel")]
        [TestCase("Basileia Rhṓmaiṓn", "Basileia Rhōmaiōn")]
        [TestCase("Bạt Đế Mỗ", "Bat Đê Mô")]
        [TestCase("Blāsīnṯīā", "Blāsīnthīā")]
        [TestCase("Brægentford", "Brægentford")]
        [TestCase("B‍risṭ‍riṭā", "Bristritā")]
        [TestCase("Br̥̄sels", "Bŕusels")]
        [TestCase("Budapeşt", "Budapest")]
        [TestCase("Bułgar Wielki", "Bulgar Wielki")]
        [TestCase("Bùyínuòsīàilìsī", "Bùyínuòsīàilìsī")]
        [TestCase("Český Krumlov", "Cheský Krumlov")]
        [TestCase("Cetiǌe", "Cetinje")]
        [TestCase("Chęciny", "Checiny")]
        [TestCase("Đakovo", "Đakovo")]
        [TestCase("Đặng Khẩu", "Đăng Khâu")]
        [TestCase("Danmǫrk", "Danmǫrk")]
        [TestCase("Dasavleti Virǯinia", "Dasavleti Virzhinia")]
        [TestCase("Đế quốc Nga", "Đê quôc Nga")]
        [TestCase("Dobřany", "Dobrzany")]
        [TestCase("Dᶻidᶻəlal̓ič", "Dzidzalalich")]
        [TestCase("Egeyan Kġziner", "Egeyan Kghziner")]
        [TestCase("Enkoriџ", "Enkoridzh")]
        [TestCase("Ĕnṭrima", "Ĕntrima")]
        [TestCase("Ər-Rəqqə", "Ar-Raqqa")]
        [TestCase("Farƣona", "Fargona")]
        [TestCase("Ġhaūdeš", "Ghaūdesh")]
        [TestCase("Glideroχ", "Glideroch")]
        [TestCase("Góðviðra", "Góðviðra")]
        [TestCase("پwyrṭūrīkū", "Bwyrtūrīkū")]
        [TestCase("Grɨnlɛɛn", "Grinleen")]
        [TestCase("G‍roseṭō", "Grosetō")]
        [TestCase("Ḥadīṯẗ", "Hadīthah")]
        [TestCase("Ȟaȟáwakpa", "Haháwakpa")]
        [TestCase("Ḥamāẗ", "Hamāh")]
        [TestCase("H̱rūnīnġn", "Khrūnīnghn")]
        [TestCase("Ins Br̥k", "Ins Bruk")]
        [TestCase("Iṉspruk", "Inspruk")]
        [TestCase("Ja઼āgreba", "Jaāgreba")]
        [TestCase("Jālaॎs‌burga", "Jālasburga")]
        [TestCase("Jémanị", "Jémani")]
        [TestCase("Jhānjāṁ", "Jhānjām")]
        [TestCase("K’asablank’a", "K’asablank’a")]
        [TestCase("Kaer Gradaỽc", "Kaer Gradauuc")]
        [TestCase("Kalɩfɔrnii", "Kalıfornii")]
        [TestCase("Kašuubimaa", "Kashuubimaa")]
        [TestCase("Kȁzahstān", "Kàzahstān")]
        [TestCase("Khar‌gōn", "Khargōn")]
        [TestCase("K‍ragujevak", "Kragujevak")]
        [TestCase("Lablaẗ", "Lablah")]
        [TestCase("Lāip‌ॎsiśa", "Lāipsisa")]
        [TestCase("Lėnkėjė", "Lénkéjé")]
        [TestCase("Likṟṟaṉ‌sṟṟaiṉ", "Likrransrrain")]
        [TestCase("Linkøbing", "Linkøbing")]
        [TestCase("Lǐyuērènèilú", "Līyuērènèilú")]
        [TestCase("Lò̤-mā Dá̤-guók", "Lò-mā Dá-guók")]
        [TestCase("Loṙow", "Loŕow")]
        [TestCase("Luật Tước Đàm", "Luât Tu'óc Đàm")]
        [TestCase("Lǚfádēng", "Lüfádēng")]
        [TestCase("Lúksẹ́mbọ̀rg", "Lúksembọrg")]
        [TestCase("Łużyce", "Luzyce")]
        [TestCase("Maďarsko", "Madarsko")]
        [TestCase("Mīdīlbūrẖ", "Mīdīlbūrkh")]
        [TestCase("Miniṡoṡeiyoḣdoke Otoƞwe", "Minisoseiyohdoke Otonwe")]
        [TestCase("Miniᐋpulis", "Miniâpulis")]
        [TestCase("Moscoƿ", "Moscouu")]
        [TestCase("Mūrīṭanīẗ al-Ṭinǧīẗ", "Mūrītaniyah al-Tinjiyah")]
        [TestCase("Nam̐si", "Namsi")]
        [TestCase("Nazareḟŭ", "Nazarefü")]
        [TestCase("Ngò-lò-sṳ̂", "Ngò-lò-sû")]
        [TestCase("Nörvêzi", "Nörvêzi")]
        [TestCase("Novyĭ Margelan", "Novyī Margelan")]
        [TestCase("Nowĩ", "Nowī")]
        [TestCase("Nɔɔrɩvɛɛzɩ", "Noorıveezı")]
        [TestCase("Nuorvegėjė", "Nuorvegéjé")]
        [TestCase("Nūrṯāmbtūn", "Nūrthāmbtūn")]
        [TestCase("Nuwakicɔɔtɩ", "Nuwakicootı")]
        [TestCase("Ɔsɩloo", "Osıloo")]
        [TestCase("Perejäslavľĭ", "Perejäslavlī")]
        [TestCase("Permė", "Permé")]
        [TestCase("Phin‌sṭrām", "Phinstrām")]
        [TestCase("P‍ṭiyuj", "Ptiyuj")]
        [TestCase("Purūkḷiṉ", "Purūklin")]
        [TestCase("ɸlāryo", "Plāryo")]
        [TestCase("Qart-Ḥadašt", "Qart-Hadasht")]
        [TestCase("Ra‍yājāna", "Rayājāna")]
        [TestCase("R‍hods", "Rhods")]
        [TestCase("Rừng Bohemia", "Rù'ng Bohemia")]
        [TestCase("Sāg‍rab", "Sāgrab")]
        [TestCase("Sai Ngǭn", "Sai Ngǫn")]
        [TestCase("Sālj‌barg‌", "Sāljbarg")]
        [TestCase("Šaqūbīẗ", "Shaqūbiyah")]
        [TestCase("Saraẖs", "Sarakhs")]
        [TestCase("Semêndria", "Semêndria")]
        [TestCase("Starověký Řím", "Starověký Rzím")]
        [TestCase("Sveti Đorđe", "Sveti Đorđe")]
        [TestCase("Taɖɛsalam", "Taɖesalam")]
        [TestCase("Taϊpéi", "Taïpéi")]
        [TestCase("Test ɸlāryoɸ", "Test Plāryop")]
        [TestCase("Thượng Volta", "Thu'ong Volta")]
        [TestCase("Tibískon", "Tibískon")]
        [TestCase("Tłnáʔəč", "Tlná´ach")]
        [TestCase("Ṭ‍renṭō", "Trentō")]
        [TestCase("Truǧālẗ", "Trujālah")]
        [TestCase("Užhorod", "Uzhhorod")]
        [TestCase("Vialikaja Poĺšča", "Vialikaja Polshcha")]
        [TestCase("Vюrцby’rg", "Viurcby’rg")]
        [TestCase("Ẇel‌ś‌", "Ẃels")]
        [TestCase("Вуллонгонг", "Vullongong")]
        [TestCase("Эstoniья", "Estoni'ia")]
        [TestCase("Юli’h", "Iuli’h")]
        public void WhenNormalisingForImperatorRome_ReturnsTheExpectedNormalisedName(
            string name,
            string expectedResult)
            => Assert.That(nameNormaliser.ToImperatorRomeCharset(name), Is.EqualTo(expectedResult));

        [Test]
        [TestCase(StringOfVariousCharacters)]
        public void WhenNormalisingForCK3_ReturnsTheNameWithoutCharsOutsideCharset(string name)
            => TestCharsNotOutsideSet(nameNormaliser.ToCK3Charset(name), CK3Characters);

        [Test]
        [TestCase(StringOfVariousCharacters)]
        public void WhenNormalisingForHOI4City_ReturnsTheNameWithoutCharsOutsideCharset(string name)
            => TestCharsNotOutsideSet(nameNormaliser.ToHOI4CityCharset(name), HOI4MapCharacters);

        [Test]
        [TestCase(StringOfVariousCharacters)]
        public void WhenNormalisingForWindow1252_ReturnsTheNameWithoutCharsOutsideCharset(string name)
            => TestCharsNotOutsideSet(nameNormaliser.ToWindows1252(name), Windows1252Characters);

        static void TestCharsNotOutsideSet(string str, string charset)
        {
            string actualCharset = charset + " ";
            string charsOutisdeCharset = string.Concat(str.Where(c => !actualCharset.Contains(c)));

            if (string.IsNullOrWhiteSpace(charsOutisdeCharset))
            {
                charsOutisdeCharset = string.Empty;
            }

            Assert.That(charsOutisdeCharset, Is.Empty);
        }
    }
}
