using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace MoreCulturalNamesModBuilder.Service
{
    public sealed class NameNormaliser : INameNormaliser
    {
        ConcurrentDictionary<string, string> windows1252cache;
        ConcurrentDictionary<string, string> ck3cache;
        ConcurrentDictionary<string, string> hoi4citiesCache;
        ConcurrentDictionary<string, string> hoi4statesCache;

        public NameNormaliser()
        {
            windows1252cache = new ConcurrentDictionary<string, string>();
            ck3cache = new ConcurrentDictionary<string, string>();
            hoi4citiesCache = new ConcurrentDictionary<string, string>();
            hoi4statesCache = new ConcurrentDictionary<string, string>();
        }

        public string ToCK3Charset(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            if (ck3cache.ContainsKey(name))
            {
                return ck3cache[name];
            }

            string processedName = ApplyCommonReplacements(name);

            // Crusader Kings III
            processedName = Regex.Replace(processedName, "[ḂḄ]", "B");
            processedName = Regex.Replace(processedName, "[Ḏ]", "D");
            processedName = Regex.Replace(processedName, "[Ǝ]", "E");
            processedName = Regex.Replace(processedName, "[Ḫ]", "H");
            processedName = Regex.Replace(processedName, "[Ƙ]", "K'");
            processedName = Regex.Replace(processedName, "[Ḵ]", "K");
            processedName = Regex.Replace(processedName, "[Ǫ]", "Ọ");
            processedName = Regex.Replace(processedName, "[Ṡ]", "Ś");
            processedName = Regex.Replace(processedName, "[Ṭ]", "Ț");
            processedName = Regex.Replace(processedName, "[Ṳ]", "Ü");
            processedName = Regex.Replace(processedName, "[Ẍ]", "X");
            processedName = Regex.Replace(processedName, "[Ẓ]", "Z");
            processedName = Regex.Replace(processedName, "[ḃḅ]", "b");
            processedName = Regex.Replace(processedName, "[ḏɖ]", "d");
            processedName = Regex.Replace(processedName, "[ḫ]", "ḥ");
            processedName = Regex.Replace(processedName, "[ƙ]", "k'");
            processedName = Regex.Replace(processedName, "[ḵ]", "k");
            processedName = Regex.Replace(processedName, "[ǫ]", "ọ");
            processedName = Regex.Replace(processedName, "[ṡ]", "ś");
            processedName = Regex.Replace(processedName, "[ṭ]", "ț");
            processedName = Regex.Replace(processedName, "[ṳ]", "ü");
            processedName = Regex.Replace(processedName, "[ẍ]", "x");
            processedName = Regex.Replace(processedName, "[ẓʐ]", "z");

            ck3cache.TryAdd(name, processedName);

            return processedName;
        }

        public string ToHOI4CityCharset(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            if (hoi4citiesCache.ContainsKey(name))
            {
                return hoi4citiesCache[name];
            }

            string processedName = ApplyCommonReplacements(name);

            // Hearts of Iron IV
            processedName = Regex.Replace(processedName, "[ẠƏ]", "A");
            processedName = Regex.Replace(processedName, "[ḂḄ]", "B");
            processedName = Regex.Replace(processedName, "[ḎḌƊ]", "D");
            processedName = Regex.Replace(processedName, "[Ẽ]", "Ē");
            processedName = Regex.Replace(processedName, "[Ǝ]", "E");
            processedName = Regex.Replace(processedName, "[ḤḪ]", "H");
            processedName = Regex.Replace(processedName, "[Ḳ]", "Ķ");
            processedName = Regex.Replace(processedName, "[Ƙ]", "K'");
            processedName = Regex.Replace(processedName, "[Ḵ]", "K");
            processedName = Regex.Replace(processedName, "[Ḷ]", "Ļ");
            processedName = Regex.Replace(processedName, "[Ṃ]", "M");
            processedName = Regex.Replace(processedName, "[Ṅ]", "Ń");
            processedName = Regex.Replace(processedName, "[Ṇ]", "Ņ");
            processedName = Regex.Replace(processedName, "[Ǿ]", "Ø");
            processedName = Regex.Replace(processedName, "[Ộ]", "Ô");
            processedName = Regex.Replace(processedName, "[ǪỌ]", "O");
            processedName = Regex.Replace(processedName, "[Ṛ]", "R");
            processedName = Regex.Replace(processedName, "[Ș]", "Ş");
            processedName = Regex.Replace(processedName, "[Ṡ]", "Ś");
            processedName = Regex.Replace(processedName, "[Ṣ]", "S");
            processedName = Regex.Replace(processedName, "[ȚṬ]", "Ţ");
            processedName = Regex.Replace(processedName, "[Ư]", "U'");
            processedName = Regex.Replace(processedName, "[Ṳ]", "Ü");
            processedName = Regex.Replace(processedName, "[Ẍ]", "X");
            processedName = Regex.Replace(processedName, "[Ȳ]", "Y");
            processedName = Regex.Replace(processedName, "[Ẓ]", "Z");
            processedName = Regex.Replace(processedName, "[ạə]", "a");
            processedName = Regex.Replace(processedName, "[ḃḅ]", "b");
            processedName = Regex.Replace(processedName, "[ḏḍɗɖ]", "d");
            processedName = Regex.Replace(processedName, "[ẽ]", "ē");
            processedName = Regex.Replace(processedName, "[ệ]", "ê");
            processedName = Regex.Replace(processedName, "[ẹ]", "e");
            processedName = Regex.Replace(processedName, "[ḥḫ]", "h");
            processedName = Regex.Replace(processedName, "[ĩ]", "ï");
            processedName = Regex.Replace(processedName, "[ḳ]", "ķ");
            processedName = Regex.Replace(processedName, "[ƙ]", "k'");
            processedName = Regex.Replace(processedName, "[ḵ]", "k");
            processedName = Regex.Replace(processedName, "[ḷ]", "ļ");
            processedName = Regex.Replace(processedName, "[ṃ]", "m");
            processedName = Regex.Replace(processedName, "[ṅ]", "ń");
            processedName = Regex.Replace(processedName, "[ṇ]", "ņ");
            processedName = Regex.Replace(processedName, "[ǿ]", "ø");
            processedName = Regex.Replace(processedName, "[ộ]", "ô");
            processedName = Regex.Replace(processedName, "[ǫọ]", "o");
            processedName = Regex.Replace(processedName, "[ṛ]", "r");
            processedName = Regex.Replace(processedName, "[ș]", "ş");
            processedName = Regex.Replace(processedName, "[ṡ]", "ś");
            processedName = Regex.Replace(processedName, "[ṣ]", "s");
            processedName = Regex.Replace(processedName, "[țṭ]", "ţ");
            processedName = Regex.Replace(processedName, "[ṳ]", "ü");
            processedName = Regex.Replace(processedName, "[ư]", "u");
            processedName = Regex.Replace(processedName, "[ẍ]", "x");
            processedName = Regex.Replace(processedName, "[ȳ]", "y");
            processedName = Regex.Replace(processedName, "[ẓʐ]", "z");

            hoi4citiesCache.TryAdd(name, processedName);

            return processedName;
        }

        public string ToHOI4StateCharset(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            if (hoi4statesCache.ContainsKey(name))
            {
                return hoi4statesCache[name];
            }

            string processedName = ToHOI4CityCharset(name);

            // Hearts of Iron IV 
            processedName = Regex.Replace(processedName, "[ĂĀ]", "Ã");
            processedName = Regex.Replace(processedName, "[ĆĈČĊ]", "C");
            processedName = Regex.Replace(processedName, "[Ď]", "D");
            processedName = Regex.Replace(processedName, "[Ē]", "Ë");
            processedName = Regex.Replace(processedName, "[Ė]", "È");
            processedName = Regex.Replace(processedName, "[Ě]", "Ê");
            processedName = Regex.Replace(processedName, "[Ę]", "E");
            processedName = Regex.Replace(processedName, "[ĞĜĠĢ]", "G");
            processedName = Regex.Replace(processedName, "[Ĥ]", "H");
            processedName = Regex.Replace(processedName, "[İ]", "I");
            processedName = Regex.Replace(processedName, "[ĬĪĨ]", "Ï");
            processedName = Regex.Replace(processedName, "[ĹĽĻ]", "L");
            processedName = Regex.Replace(processedName, "[Ň]", "Ñ");
            processedName = Regex.Replace(processedName, "[ŃŅ]", "N");
            processedName = Regex.Replace(processedName, "[Ō]", "Õ");
            processedName = Regex.Replace(processedName, "[Ő]", "Ö");
            processedName = Regex.Replace(processedName, "[Ŏ]", "Ô");
            processedName = Regex.Replace(processedName, "[ŔŘ]", "R");
            processedName = Regex.Replace(processedName, "[ŚŜŞ]", "S");
            processedName = Regex.Replace(processedName, "[ŤŢ]", "T");
            processedName = Regex.Replace(processedName, "[ŪŬŰ]", "Ü");
            processedName = Regex.Replace(processedName, "[ŮŲ]", "U");
            processedName = Regex.Replace(processedName, "[Ŷ]", "Y");
            processedName = Regex.Replace(processedName, "[ŹŻ]", "Z");
            processedName = Regex.Replace(processedName, "[ăā]", "ã");
            processedName = Regex.Replace(processedName, "[ą]", "a");
            processedName = Regex.Replace(processedName, "[ćĉčċ]", "c");
            processedName = Regex.Replace(processedName, "[ď]", "d");
            processedName = Regex.Replace(processedName, "[ē]", "ë");
            processedName = Regex.Replace(processedName, "[ė]", "è");
            processedName = Regex.Replace(processedName, "[ě]", "ê");
            processedName = Regex.Replace(processedName, "[ēėę]", "e");
            processedName = Regex.Replace(processedName, "[ğĝġģ]", "g");
            processedName = Regex.Replace(processedName, "[ĥ]", "h");
            processedName = Regex.Replace(processedName, "[ĭīĩ]", "ï");
            processedName = Regex.Replace(processedName, "[ĺľļ]", "l");
            processedName = Regex.Replace(processedName, "[ň]", "ñ");
            processedName = Regex.Replace(processedName, "[ńņ]", "n");
            processedName = Regex.Replace(processedName, "[ō]", "õ");
            processedName = Regex.Replace(processedName, "[ő]", "ö");
            processedName = Regex.Replace(processedName, "[ŏ]", "ô");
            processedName = Regex.Replace(processedName, "[ŕř]", "r");
            processedName = Regex.Replace(processedName, "[śŝş]", "s");
            processedName = Regex.Replace(processedName, "[ťţ]", "t");
            processedName = Regex.Replace(processedName, "[ūŭű]", "ü");
            processedName = Regex.Replace(processedName, "[ůų]", "u");
            processedName = Regex.Replace(processedName, "[ŷ]", "y");
            processedName = Regex.Replace(processedName, "[źż]", "z");

            hoi4statesCache.TryAdd(name, processedName);

            return processedName;
        }

        public string ToWindows1252(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            if (windows1252cache.ContainsKey(name))
            {
                return windows1252cache[name];
            }

            string processedName = ApplyCommonReplacements(name);

            // Crusader Kings II
            processedName = processedName.Replace("A̓", "Á");
            processedName = Regex.Replace(processedName, "[ĂĀ]", "Ã");
            processedName = Regex.Replace(processedName, "[ẠƏ]", "A");
            processedName = Regex.Replace(processedName, "[ḂḄ]", "B");
            processedName = Regex.Replace(processedName, "[ĆĊ]", "C");
            processedName = Regex.Replace(processedName, "[Č]", "Ch");
            processedName = Regex.Replace(processedName, "[ḎƊḌ]", "D");
            processedName = Regex.Replace(processedName, "[ĐƉ]", "Ð");
            processedName = Regex.Replace(processedName, "[ĒẼ]", "Ë");
            processedName = Regex.Replace(processedName, "[Ė]", "È");
            processedName = Regex.Replace(processedName, "[Ě]", "Ê");
            processedName = Regex.Replace(processedName, "[ĘƎ]", "E");
            processedName = Regex.Replace(processedName, "[ĞĜĠĢ]", "G");
            processedName = Regex.Replace(processedName, "[ĤḤḪĦ]", "H");
            processedName = Regex.Replace(processedName, "[İ]", "I");
            processedName = Regex.Replace(processedName, "[ĬĪĨ]", "Ï");
            processedName = Regex.Replace(processedName, "[Ƙ]", "K'");
            processedName = Regex.Replace(processedName, "[ḲḴ]", "K");
            processedName = Regex.Replace(processedName, "[ĹŁĽḶĻ]", "L");
            processedName = Regex.Replace(processedName, "[Ṃ]", "M");
            processedName = Regex.Replace(processedName, "[Ň]", "Ñ");
            processedName = Regex.Replace(processedName, "[ŃŅṄṆŊ]", "N");
            processedName = Regex.Replace(processedName, "[Ō]", "Õ");
            processedName = Regex.Replace(processedName, "[Ő]", "Ö");
            processedName = Regex.Replace(processedName, "[Ǿ]", "Ø");
            processedName = Regex.Replace(processedName, "[Ộ]", "Ô");
            processedName = Regex.Replace(processedName, "[Ŏ̤Ŏ]", "Ô"); // Maybe replace with "Eo"
            processedName = Regex.Replace(processedName, "[ǪỌ]", "O");
            processedName = Regex.Replace(processedName, "[Ř]", "Rz");
            processedName = Regex.Replace(processedName, "[ŔṚ]", "R");
            processedName = Regex.Replace(processedName, "[ŚŜŞȘṢṠ]", "S");
            processedName = Regex.Replace(processedName, "[Ť]", "Ty");
            processedName = Regex.Replace(processedName, "[ȚŢṬŦ]", "T");
            processedName = Regex.Replace(processedName, "[ŪŬŰṲ]", "Ü");
            processedName = Regex.Replace(processedName, "[Ư]", "U'");
            processedName = Regex.Replace(processedName, "[ŮŲ]", "U");
            processedName = Regex.Replace(processedName, "[Ẍ]", "X");
            processedName = Regex.Replace(processedName, "[ŶȲ]", "Y");
            processedName = Regex.Replace(processedName, "[ŹŻẒ]", "Z");
            processedName = Regex.Replace(processedName, "[ạəą]", "a");
            processedName = Regex.Replace(processedName, "[ăā]", "ã");
            processedName = Regex.Replace(processedName, "[ḃḅ]", "b");
            processedName = Regex.Replace(processedName, "[ćċ]", "c");
            processedName = Regex.Replace(processedName, "[č]", "ch");
            processedName = Regex.Replace(processedName, "[đ]", "dž");
            processedName = Regex.Replace(processedName, "[ḏɗɖḍ]", "d");
            processedName = Regex.Replace(processedName, "[ēẽ]", "ë");
            processedName = Regex.Replace(processedName, "[ė]", "è");
            processedName = Regex.Replace(processedName, "[ệě]", "ê");
            processedName = Regex.Replace(processedName, "[ęẹ]", "e");
            processedName = Regex.Replace(processedName, "[ğĝġģ]", "g");
            processedName = Regex.Replace(processedName, "[ĥḥḫħ]", "h");
            processedName = Regex.Replace(processedName, "[ı]", "i");
            processedName = Regex.Replace(processedName, "[ĭīĩ]", "ï");
            processedName = Regex.Replace(processedName, "[ƙ]", "k'");
            processedName = Regex.Replace(processedName, "[ḳḵ]", "k");
            processedName = Regex.Replace(processedName, "[ĺłľḷļ]", "l");
            processedName = Regex.Replace(processedName, "[ṃ]", "m");
            processedName = Regex.Replace(processedName, "[ň]", "ñ");
            processedName = Regex.Replace(processedName, "[ńņṅṇŋ]", "n");
            processedName = Regex.Replace(processedName, "[ō]", "õ");
            processedName = Regex.Replace(processedName, "[ő]", "ö");
            processedName = Regex.Replace(processedName, "[ǿ]", "ø");
            processedName = Regex.Replace(processedName, "[ộ]", "ô");
            processedName = Regex.Replace(processedName, "[ŏ̤ŏ]", "ô"); // Maybe replace with "eo"
            processedName = Regex.Replace(processedName, "[ǫọ]", "o");
            processedName = Regex.Replace(processedName, "[ř]", "rz");
            processedName = Regex.Replace(processedName, "[ŕṛ]", "r");
            processedName = Regex.Replace(processedName, "[śŝşșṣṡ]", "s");
            processedName = Regex.Replace(processedName, "[țţṭŧ]", "t");
            processedName = Regex.Replace(processedName, "[ūŭűṳ]", "ü");
            processedName = Regex.Replace(processedName, "[ư]", "u'");
            processedName = Regex.Replace(processedName, "[ůų]", "u");
            processedName = Regex.Replace(processedName, "[ẍ]", "x");
            processedName = Regex.Replace(processedName, "[ŷȳ]", "y");
            processedName = Regex.Replace(processedName, "[źżẓʐ]", "z");

            windows1252cache.TryAdd(name, processedName);

            return processedName;
        }

        private string ApplyCommonReplacements(string name)
        {
            string processedName = name;
            
            processedName = Regex.Replace(processedName, "[Ắ]", "Ă");
            processedName = Regex.Replace(processedName, "[Ɖ]", "Đ");
            processedName = Regex.Replace(processedName, "[∃]", "Ǝ");
            processedName = Regex.Replace(processedName, "[Ḗ]", "Ē");
            processedName = Regex.Replace(processedName, "[Ǧ]", "Ğ");
            processedName = Regex.Replace(processedName, "[Ƣ]", "Ğ"); // Untested in the games
            processedName = Regex.Replace(processedName, "[Ȝ]", "G");
            processedName = Regex.Replace(processedName, "[Ǐ]", "Ĭ");
            processedName = Regex.Replace(processedName, "[Ι]", "I");
            processedName = Regex.Replace(processedName, "[Ǒ]", "Ŏ");
            processedName = Regex.Replace(processedName, "[Ο]", "O");
            processedName = Regex.Replace(processedName, "[Θ]", "Th");
            processedName = Regex.Replace(processedName, "[Т]", "T");
            processedName = Regex.Replace(processedName, "[Ǔ]", "Ŭ");
            processedName = Regex.Replace(processedName, "[Ƶ]", "Z");
            processedName = Regex.Replace(processedName, "[αа]", "a");
            processedName = Regex.Replace(processedName, "[ὰ]", "à");
            processedName = Regex.Replace(processedName, "[ά]", "á");
            processedName = Regex.Replace(processedName, "[ắǎẵ]", "ă");
            processedName = Regex.Replace(processedName, "[ầ]", "â");
            processedName = Regex.Replace(processedName, "[ĕ]", "ě");
            processedName = Regex.Replace(processedName, "[ếềể]", "ê");
            processedName = Regex.Replace(processedName, "[ḗ]", "ē");
            processedName = Regex.Replace(processedName, "[ǝ]", "ə");
            processedName = Regex.Replace(processedName, "[ɛе]", "e");
            processedName = Regex.Replace(processedName, "[έ]", "é");
            processedName = Regex.Replace(processedName, "[ǧ]", "ğ");
            processedName = Regex.Replace(processedName, "[ƣ]", "ğ"); // Untested in the games
            processedName = Regex.Replace(processedName, "[ȝ]", "g");
            processedName = Regex.Replace(processedName, "[ɩ]", "ı");
            processedName = Regex.Replace(processedName, "[ǐ]", "ĭ");
            processedName = Regex.Replace(processedName, "[ḯ]", "ï");
            processedName = Regex.Replace(processedName, "[ι]", "i");
            processedName = Regex.Replace(processedName, "[к]", "k");
            processedName = Regex.Replace(processedName, "[ɬ]", "ł");
            processedName = Regex.Replace(processedName, "[ό]", "ó");
            processedName = Regex.Replace(processedName, "[ṓ]", "ō");
            processedName = Regex.Replace(processedName, "[ǒ]", "ŏ");
            processedName = Regex.Replace(processedName, "[ốồ]", "ô");
            processedName = Regex.Replace(processedName, "[ɔо]", "o");
            processedName = Regex.Replace(processedName, "[θ]", "th");
            processedName = Regex.Replace(processedName, "[ǔ]", "ŭ");
            processedName = Regex.Replace(processedName, "[ύ]", "ú");
            processedName = Regex.Replace(processedName, "[ǜ]", "ü");
            processedName = Regex.Replace(processedName, "[ƶ]", "z");
            processedName = Regex.Replace(processedName, "[‘ʻ’ʼʿʾʹʲ]", "'");
            processedName = Regex.Replace(processedName, "[ⁿ]", "\"");
            processedName = Regex.Replace(processedName, "[–]", "-");
            processedName = Regex.Replace(processedName, "[‎·]", "");
            processedName = Regex.Replace(processedName, "[‎‎]", ""); // Invisible characters

            return processedName;
        }
    }
}
