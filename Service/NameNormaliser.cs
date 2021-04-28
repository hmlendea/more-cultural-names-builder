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
        ConcurrentDictionary<string, string> irCache;

        public NameNormaliser()
        {
            windows1252cache = new ConcurrentDictionary<string, string>();
            ck3cache = new ConcurrentDictionary<string, string>();
            hoi4citiesCache = new ConcurrentDictionary<string, string>();
            hoi4statesCache = new ConcurrentDictionary<string, string>();
            irCache = new ConcurrentDictionary<string, string>();
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
            processedName = Regex.Replace(processedName, "[Ǧ]", "Ğ");
            processedName = Regex.Replace(processedName, "[Ƙ]", "K'");
            processedName = Regex.Replace(processedName, "[Ḫ]", "Kh");
            processedName = Regex.Replace(processedName, "[Ḵ]", "K");
            processedName = Regex.Replace(processedName, "[Ǫ]", "Ọ");
            processedName = Regex.Replace(processedName, "[Ṡ]", "Ś");
            processedName = Regex.Replace(processedName, "[Ṭ]", "Ț");
            processedName = Regex.Replace(processedName, "[Ṳ]", "Ü");
            processedName = Regex.Replace(processedName, "[Ẍ]", "X");
            processedName = Regex.Replace(processedName, "[Ẓ]", "Z");
            processedName = Regex.Replace(processedName, "[ḃḅ]", "b");
            processedName = Regex.Replace(processedName, "[ḏɖ]", "d");
            processedName = Regex.Replace(processedName, "[ǧ]", "ğ");
            processedName = Regex.Replace(processedName, "[ƙ]", "k'");
            processedName = Regex.Replace(processedName, "[ḫ]", "kh");
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
            processedName = Regex.Replace(processedName, "[Ǧ]", "Ğ");
            processedName = Regex.Replace(processedName, "[Ḥ]", "H");
            processedName = Regex.Replace(processedName, "[Ƙ]", "K'");
            processedName = Regex.Replace(processedName, "[Ḫ]", "Kh");
            processedName = Regex.Replace(processedName, "[Ḵ]", "K");
            processedName = Regex.Replace(processedName, "[Ḳ]", "Ķ");
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
            processedName = Regex.Replace(processedName, "[ǧ]", "ğ");
            processedName = Regex.Replace(processedName, "[ḥ]", "h");
            processedName = Regex.Replace(processedName, "[ĩ]", "ï");
            processedName = Regex.Replace(processedName, "[ƙ]", "k'");
            processedName = Regex.Replace(processedName, "[ḫ]", "kh");
            processedName = Regex.Replace(processedName, "[ḵ]", "k");
            processedName = Regex.Replace(processedName, "[ḳ]", "ķ");
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
            processedName = Regex.Replace(processedName, "[Ǧ]", "J");
            processedName = Regex.Replace(processedName, "[ĬĪĨ]", "Ï");
            processedName = Regex.Replace(processedName, "[ĹĽĻ]", "L");
            processedName = Regex.Replace(processedName, "[Ň]", "Ñ");
            processedName = Regex.Replace(processedName, "[ŃŅ]", "N");
            processedName = Regex.Replace(processedName, "[Ō]", "Õ");
            processedName = Regex.Replace(processedName, "[Ő]", "Ö");
            processedName = Regex.Replace(processedName, "[Ŏ]", "Ô");
            processedName = Regex.Replace(processedName, "[ŔŘ]", "R");
            processedName = Regex.Replace(processedName, "[ŚŜŞ]", "S");
            processedName = Regex.Replace(processedName, "[Ť]", "Ty");
            processedName = Regex.Replace(processedName, "[Ţ]", "T");
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
            processedName = Regex.Replace(processedName, "[ǧ]", "j");
            processedName = Regex.Replace(processedName, "[ĺľļ]", "l");
            processedName = Regex.Replace(processedName, "[ň]", "ñ");
            processedName = Regex.Replace(processedName, "[ńņ]", "n");
            processedName = Regex.Replace(processedName, "[ō]", "õ");
            processedName = Regex.Replace(processedName, "[ő]", "ö");
            processedName = Regex.Replace(processedName, "[ŏ]", "ô");
            processedName = Regex.Replace(processedName, "[ŕř]", "r");
            processedName = Regex.Replace(processedName, "[śŝş]", "s");
            processedName = Regex.Replace(processedName, "[ť]", "ty");
            processedName = Regex.Replace(processedName, "[ţ]", "t");
            processedName = Regex.Replace(processedName, "[ūŭű]", "ü");
            processedName = Regex.Replace(processedName, "[ůų]", "u");
            processedName = Regex.Replace(processedName, "[ŷ]", "y");
            processedName = Regex.Replace(processedName, "[źż]", "z");

            hoi4statesCache.TryAdd(name, processedName);

            return processedName;
        }

        public string ToImperatorRomeCharset(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            if (irCache.ContainsKey(name))
            {
                return irCache[name];
            }

            string processedName = ToHOI4CityCharset(name);

            // Imperator: Rome
            processedName = Regex.Replace(processedName, "[ẠƏ]", "A");
            processedName = Regex.Replace(processedName, "[Ǣ]", "Æ");
            processedName = Regex.Replace(processedName, "[Ậ]", "Â");
            processedName = Regex.Replace(processedName, "[Ả]", "À");
            processedName = Regex.Replace(processedName, "[ĆĈČĊ]", "C");
            processedName = Regex.Replace(processedName, "[Ď]", "D");
            processedName = Regex.Replace(processedName, "[ĘẸƎ]", "E");
            processedName = Regex.Replace(processedName, "[Ė]", "È");
            processedName = Regex.Replace(processedName, "[ĞĜĠĢǴ]", "G");
            processedName = Regex.Replace(processedName, "[ĤḦĦ]", "H");
            processedName = Regex.Replace(processedName, "[İĮỊ]", "I");
            processedName = Regex.Replace(processedName, "[ĬĨ]", "Ī");
            processedName = Regex.Replace(processedName, "[ĴJ̌Ǧ]", "J");
            processedName = Regex.Replace(processedName, "[ḲĶḴ]", "K");
            processedName = Regex.Replace(processedName, "[ĹŁĽḶĻ]", "L");
            processedName = Regex.Replace(processedName, "[ṂḾ]", "M");
            processedName = Regex.Replace(processedName, "[ŅŊƝ]", "N");
            processedName = Regex.Replace(processedName, "[Ơ]", "O");
            processedName = Regex.Replace(processedName, "[Ờ]", "Ò");
            processedName = Regex.Replace(processedName, "[Ỡ]", "Õ");
            processedName = Regex.Replace(processedName, "[Ŏ]", "Õ"); // Maybe replace with Oe
            processedName = Regex.Replace(processedName, "[Ő]", "Ö");
            processedName = Regex.Replace(processedName, "[Ṕ]", "P");
            processedName = Regex.Replace(processedName, "[Ř]", "Rz");
            processedName = Regex.Replace(processedName, "[Ŕ]", "R");
            processedName = Regex.Replace(processedName, "[Š]", "Sh");
            processedName = Regex.Replace(processedName, "[ŚŜŞṢȘ]", "S");
            processedName = Regex.Replace(processedName, "[Ť]", "Ty");
            processedName = Regex.Replace(processedName, "[ȚŢṬŦ]", "T");
            processedName = Regex.Replace(processedName, "[Ư]", "U'");
            processedName = Regex.Replace(processedName, "[Ứ]", "Ú'");
            processedName = Regex.Replace(processedName, "[ŮŲỤ]", "U");
            processedName = Regex.Replace(processedName, "[ǛŰṲ]", "Ü");
            processedName = Regex.Replace(processedName, "[ũ]", "Ū");
            processedName = Regex.Replace(processedName, "[Ǔ]", "Ŭ");
            processedName = Regex.Replace(processedName, "[Ṿ]", "V");
            processedName = Regex.Replace(processedName, "[Ẍ]", "X");
            processedName = Regex.Replace(processedName, "[ŶȲẎ]", "Y");
            processedName = Regex.Replace(processedName, "[Ž]", "Zh");
            processedName = Regex.Replace(processedName, "[ƵŹŻẒ]", "Z");
            processedName = Regex.Replace(processedName, "[ạəą]", "a");
            processedName = Regex.Replace(processedName, "[ǣ]", "æ");
            processedName = Regex.Replace(processedName, "[ậ]", "â");
            processedName = Regex.Replace(processedName, "[ả]", "à");
            processedName = Regex.Replace(processedName, "[ćĉčċ]", "c");
            processedName = Regex.Replace(processedName, "[ď]", "d");
            processedName = Regex.Replace(processedName, "[ęẹ]", "e");
            processedName = Regex.Replace(processedName, "[ė]", "è");
            processedName = Regex.Replace(processedName, "[ẽ]", "ē");
            processedName = Regex.Replace(processedName, "[ğĝġģǵ]", "g");
            processedName = Regex.Replace(processedName, "[ĥḧħ]", "h");
            processedName = Regex.Replace(processedName, "[įị]", "i");
            processedName = Regex.Replace(processedName, "[ĭĩ]", "ī");
            processedName = Regex.Replace(processedName, "[ĵǰǧ]", "j");
            processedName = Regex.Replace(processedName, "[ḳķḵ]", "k");
            processedName = Regex.Replace(processedName, "[ĺłľḷļ]", "l");
            processedName = Regex.Replace(processedName, "[ṃḿ]", "m");
            processedName = Regex.Replace(processedName, "[ņŋɲ]", "n");
            processedName = Regex.Replace(processedName, "[ơ]", "o");
            processedName = Regex.Replace(processedName, "[ờ]", "ò");
            processedName = Regex.Replace(processedName, "[ỡ]", "õ");
            processedName = Regex.Replace(processedName, "[ŏ]", "õ"); // Maybe replace with oe
            processedName = Regex.Replace(processedName, "[ő]", "ö");
            processedName = Regex.Replace(processedName, "[ṕ]", "p");
            processedName = Regex.Replace(processedName, "[ř]", "rz");
            processedName = Regex.Replace(processedName, "[ŕ]", "r");
            processedName = Regex.Replace(processedName, "[ß]", "ss");
            processedName = Regex.Replace(processedName, "[š]", "sh");
            processedName = Regex.Replace(processedName, "[śŝşṣș]", "s");
            processedName = Regex.Replace(processedName, "[ť]", "ty");
            processedName = Regex.Replace(processedName, "[țţṭŧ]", "t");
            processedName = Regex.Replace(processedName, "[ư]", "u'");
            processedName = Regex.Replace(processedName, "[ứ]", "ú'");
            processedName = Regex.Replace(processedName, "[ůųụủ]", "u");
            processedName = Regex.Replace(processedName, "[ǜűṳ]", "ü");
            processedName = Regex.Replace(processedName, "[ũ]", "ū");
            processedName = Regex.Replace(processedName, "[ǔ]", "ŭ");
            processedName = Regex.Replace(processedName, "[ṿ]", "v");
            processedName = Regex.Replace(processedName, "[ẍ]", "x");
            processedName = Regex.Replace(processedName, "[ž]", "zh");
            processedName = Regex.Replace(processedName, "[ŷȳẏ]", "y");
            processedName = Regex.Replace(processedName, "[ƶźżẓʐ]", "z");

            irCache.TryAdd(name, processedName);

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
            processedName = Regex.Replace(processedName, "[Ǣ]", "Æ");
            processedName = Regex.Replace(processedName, "[ẠƏ]", "A");
            processedName = Regex.Replace(processedName, "[Ả]", "À");
            processedName = Regex.Replace(processedName, "[Ậ]", "Â");
            processedName = Regex.Replace(processedName, "[ĂĀ]", "Ã");
            processedName = Regex.Replace(processedName, "[ḂḄ]", "B");
            processedName = Regex.Replace(processedName, "[ĆĊ]", "C");
            processedName = Regex.Replace(processedName, "[Č]", "Ch");
            processedName = Regex.Replace(processedName, "[ḎƊĎḌ]", "D");
            processedName = Regex.Replace(processedName, "[ĐƉ]", "Ð");
            processedName = Regex.Replace(processedName, "[ĒẸẼ]", "Ë");
            processedName = Regex.Replace(processedName, "[Ė]", "È");
            processedName = Regex.Replace(processedName, "[ỆĚ]", "Ê");
            processedName = Regex.Replace(processedName, "[ĘƎ]", "E");
            processedName = Regex.Replace(processedName, "[ĞĜĠĢǴ]", "G");
            processedName = Regex.Replace(processedName, "[ĤḦḤḨĦ]", "H");
            processedName = Regex.Replace(processedName, "[İĮỊ]", "I");
            processedName = Regex.Replace(processedName, "[ĬĪĨ]", "Ï");
            processedName = Regex.Replace(processedName, "[ĴJ̌Ǧ]", "J");
            processedName = Regex.Replace(processedName, "[Ƙ]", "K'");
            processedName = Regex.Replace(processedName, "[Ḫ]", "Kh");
            processedName = Regex.Replace(processedName, "[ḲĶḴ]", "K");
            processedName = Regex.Replace(processedName, "[ĹŁĽḶĻ]", "L");
            processedName = Regex.Replace(processedName, "[ṂḾ]", "M");
            processedName = Regex.Replace(processedName, "[Ň]", "Ñ");
            processedName = Regex.Replace(processedName, "[ŃǸŅṄṆŊƝ]", "N");
            processedName = Regex.Replace(processedName, "[ƠǪỌ]", "O");
            processedName = Regex.Replace(processedName, "[Ờ]", "Ò");
            processedName = Regex.Replace(processedName, "[ỠŌ]", "Õ");
            processedName = Regex.Replace(processedName, "[Ŏ̤Ŏ]", "Õ"); // Maybe replace with "Eo"
            processedName = Regex.Replace(processedName, "[Ő]", "Ö");
            processedName = Regex.Replace(processedName, "[Ǿ]", "Ø");
            processedName = Regex.Replace(processedName, "[Ộ]", "Ô");
            processedName = Regex.Replace(processedName, "[Ṕ]", "P");
            processedName = Regex.Replace(processedName, "[Ř]", "Rz");
            processedName = Regex.Replace(processedName, "[ŔṚ]", "R");
            processedName = Regex.Replace(processedName, "[ŚŜŞȘṢṠ]", "S");
            processedName = Regex.Replace(processedName, "[Ť]", "Ty");
            processedName = Regex.Replace(processedName, "[ȚŢṬŦ]", "T");
            processedName = Regex.Replace(processedName, "[Ư]", "U'");
            processedName = Regex.Replace(processedName, "[Ứ]", "Ú'");
            processedName = Regex.Replace(processedName, "[ŮŲỤ]", "U");
            processedName = Regex.Replace(processedName, "[ŪŬŰṲ]", "Ü");
            processedName = Regex.Replace(processedName, "[Ṿ]", "V");
            processedName = Regex.Replace(processedName, "[Ẍ]", "X");
            processedName = Regex.Replace(processedName, "[ŶȲ]", "Y");
            processedName = Regex.Replace(processedName, "[ỲẎ]", "Ý");
            processedName = Regex.Replace(processedName, "[ŹŻẒ]", "Z");
            processedName = Regex.Replace(processedName, "[ǣ]", "æ");
            processedName = Regex.Replace(processedName, "[ạəą]", "a");
            processedName = Regex.Replace(processedName, "[ả]", "à");
            processedName = Regex.Replace(processedName, "[ậ]", "â");
            processedName = Regex.Replace(processedName, "[ăā]", "ã");
            processedName = Regex.Replace(processedName, "[ḃḅ]", "b");
            processedName = Regex.Replace(processedName, "[ćċ]", "c");
            processedName = Regex.Replace(processedName, "[č]", "ch");
            processedName = Regex.Replace(processedName, "[đ]", "dž");
            processedName = Regex.Replace(processedName, "[ḏɗɖďḍ]", "d");
            processedName = Regex.Replace(processedName, "[ēẽ]", "ë");
            processedName = Regex.Replace(processedName, "[ė]", "è");
            processedName = Regex.Replace(processedName, "[ệě]", "ê");
            processedName = Regex.Replace(processedName, "[ęẹ]", "e");
            processedName = Regex.Replace(processedName, "[ğĝġģǵ]", "g");
            processedName = Regex.Replace(processedName, "[ĥḧḥħḩ]", "h");
            processedName = Regex.Replace(processedName, "[ıįị]", "i");
            processedName = Regex.Replace(processedName, "[ĭīĩ]", "ï");
            processedName = Regex.Replace(processedName, "[ĵǰǧ]", "j");
            processedName = Regex.Replace(processedName, "[ƙ]", "k'");
            processedName = Regex.Replace(processedName, "[ḫ]", "kh");
            processedName = Regex.Replace(processedName, "[ḳķḵ]", "k");
            processedName = Regex.Replace(processedName, "[ĺłľḷļ]", "l");
            processedName = Regex.Replace(processedName, "[ṃḿ]", "m");
            processedName = Regex.Replace(processedName, "[ň]", "ñ");
            processedName = Regex.Replace(processedName, "[ńǹņṅṇŋɲ]", "n");
            processedName = Regex.Replace(processedName, "[ơǫọ]", "o");
            processedName = Regex.Replace(processedName, "[ờ]", "ò");
            processedName = Regex.Replace(processedName, "[ỡō]", "õ");
            processedName = Regex.Replace(processedName, "[ŏ̤ŏ]", "õ"); // Maybe replace with "eo"
            processedName = Regex.Replace(processedName, "[ő]", "ö");
            processedName = Regex.Replace(processedName, "[ǿ]", "ø");
            processedName = Regex.Replace(processedName, "[ộ]", "ô");
            processedName = Regex.Replace(processedName, "[ṕ]", "p");
            processedName = Regex.Replace(processedName, "[ř]", "rz");
            processedName = Regex.Replace(processedName, "[ŕṛ]", "r");
            processedName = Regex.Replace(processedName, "[śŝşșṣṡ]", "s");
            processedName = Regex.Replace(processedName, "[ť]", "ty");
            processedName = Regex.Replace(processedName, "[țţṭŧ]", "t");
            processedName = Regex.Replace(processedName, "[ư]", "u'");
            processedName = Regex.Replace(processedName, "[ứ]", "ú'");
            processedName = Regex.Replace(processedName, "[ůųụ]", "u");
            processedName = Regex.Replace(processedName, "[ūŭűṳ]", "ü");
            processedName = Regex.Replace(processedName, "[ủ]", "ù");
            processedName = Regex.Replace(processedName, "[ṿ]", "v");
            processedName = Regex.Replace(processedName, "[ẍ]", "x");
            processedName = Regex.Replace(processedName, "[ŷȳ]", "y");
            processedName = Regex.Replace(processedName, "[ỳẏ]", "ý");
            processedName = Regex.Replace(processedName, "[źżẓʐ]", "z");

            windows1252cache.TryAdd(name, processedName);

            return processedName;
        }

        private string ApplyCommonReplacements(string name)
        {
            string processedName = name;
            
            processedName = Regex.Replace(processedName, "[Ắ]", "Ă");
            processedName = Regex.Replace(processedName, "[Џ]", "Dž");
            processedName = Regex.Replace(processedName, "[Ɖ]", "Đ");
            processedName = Regex.Replace(processedName, "[ЕΕƐ]", "E");
            processedName = Regex.Replace(processedName, "[∃]", "Ǝ");
            processedName = Regex.Replace(processedName, "[Ƣ]", "Ğ"); // Untested in the games
            processedName = Regex.Replace(processedName, "[Ȝ]", "Gh"); // Or G
            processedName = Regex.Replace(processedName, "[Ɣ]", "Gh");
            processedName = Regex.Replace(processedName, "[Ǐ]", "Ĭ");
            processedName = Regex.Replace(processedName, "[ІΙ]", "I");
            processedName = Regex.Replace(processedName, "[Ј]", "J");
            processedName = Regex.Replace(processedName, "[К]", "K");
            processedName = Regex.Replace(processedName, "[ОΟ]", "O");
            processedName = Regex.Replace(processedName, "[Ǒ]", "Ŏ");
            processedName = Regex.Replace(processedName, "[Θ]", "Th");
            processedName = Regex.Replace(processedName, "[Т]", "T");
            processedName = Regex.Replace(processedName, "[Ƿ]", "Uu"); // Or W
            processedName = Regex.Replace(processedName, "[Ʊ]", "U");
            processedName = Regex.Replace(processedName, "[Ǔ]", "Ŭ");
            processedName = Regex.Replace(processedName, "[Ƶ]", "Z");
            processedName = Regex.Replace(processedName, "[αа]", "a");
            processedName = Regex.Replace(processedName, "[ὰ]", "à");
            processedName = Regex.Replace(processedName, "[ά]", "á");
            processedName = Regex.Replace(processedName, "[ắǎẵ]", "ă");
            processedName = Regex.Replace(processedName, "[џ]", "dž");
            processedName = Regex.Replace(processedName, "[ĕ]", "ě");
            processedName = Regex.Replace(processedName, "[ǝ]", "ə");
            processedName = Regex.Replace(processedName, "[еεɛ]", "e");
            processedName = Regex.Replace(processedName, "[έ]", "é");
            processedName = Regex.Replace(processedName, "[ƣ]", "ğ"); // Untested in the games
            processedName = Regex.Replace(processedName, "[ȝ]", "gh"); // Or g
            processedName = Regex.Replace(processedName, "[ɣ]", "gh");
            processedName = Regex.Replace(processedName, "[ɩ]", "ı");
            processedName = Regex.Replace(processedName, "[ǐ]", "ĭ");
            processedName = Regex.Replace(processedName, "[ḯ]", "ï");
            processedName = Regex.Replace(processedName, "[іι]", "i");
            processedName = Regex.Replace(processedName, "[ј]", "j");
            processedName = Regex.Replace(processedName, "[к]", "k");
            processedName = Regex.Replace(processedName, "[ɬ]", "ł");
            processedName = Regex.Replace(processedName, "[ɔо]", "o");
            processedName = Regex.Replace(processedName, "[ό]", "ó");
            processedName = Regex.Replace(processedName, "[ǒ]", "ŏ");
            processedName = Regex.Replace(processedName, "[θ]", "th");
            processedName = Regex.Replace(processedName, "[т]", "t");
            processedName = Regex.Replace(processedName, "[ƿ]", "uu"); // Or w
            processedName = Regex.Replace(processedName, "[ʊ]", "u");
            processedName = Regex.Replace(processedName, "[ǔ]", "ŭ");
            processedName = Regex.Replace(processedName, "[ύ]", "ú");
            processedName = Regex.Replace(processedName, "[ǜ]", "ü");
            processedName = Regex.Replace(processedName, "[ƶ]", "z");

            processedName = ReplaceCharWithEmbeddedAccentOnNextChar(processedName, 'Ấ', 'Â');
            processedName = ReplaceCharWithEmbeddedAccentOnNextChar(processedName, 'Ḗ', 'Ē');
            processedName = ReplaceCharWithEmbeddedAccentOnNextChar(processedName, 'Ế', 'Ê');
            processedName = ReplaceCharWithEmbeddedAccentOnNextChar(processedName, 'Ṓ', 'Ō');
            processedName = ReplaceCharWithEmbeddedAccentOnNextChar(processedName, 'Ố', 'Ô');
            processedName = ReplaceCharWithEmbeddedAccentOnNextChar(processedName, 'ấ', 'â');
            processedName = ReplaceCharWithEmbeddedAccentOnNextChar(processedName, 'ḗ', 'ē');
            processedName = ReplaceCharWithEmbeddedAccentOnNextChar(processedName, 'ế', 'ê');
            processedName = ReplaceCharWithEmbeddedAccentOnNextChar(processedName, 'ṓ', 'ō');
            processedName = ReplaceCharWithEmbeddedAccentOnNextChar(processedName, 'ố', 'ô');
            processedName = ReplaceCharWithEmbeddedGraveAccentOnNextChar(processedName, 'Ầ', 'Â');
            processedName = ReplaceCharWithEmbeddedGraveAccentOnNextChar(processedName, 'Ề', 'Ê');
            processedName = ReplaceCharWithEmbeddedGraveAccentOnNextChar(processedName, 'Ồ', 'Ô');
            processedName = ReplaceCharWithEmbeddedGraveAccentOnNextChar(processedName, 'ầ', 'â');
            processedName = ReplaceCharWithEmbeddedGraveAccentOnNextChar(processedName, 'ề', 'ê');
            processedName = ReplaceCharWithEmbeddedGraveAccentOnNextChar(processedName, 'ồ', 'ô');
            processedName = ReplaceCharWithEmbeddedHookOnNextChar(processedName, 'Ể', 'Ê');
            processedName = ReplaceCharWithEmbeddedHookOnNextChar(processedName, 'Ổ', 'Ô');
            processedName = ReplaceCharWithEmbeddedHookOnNextChar(processedName, 'ể', 'ê');
            processedName = ReplaceCharWithEmbeddedHookOnNextChar(processedName, 'ổ', 'ô');

            processedName = Regex.Replace(processedName, "[ʾ‘ʻ’ʼ′]", "´");
            processedName = Regex.Replace(processedName, "[ʿ]", "`");
            processedName = Regex.Replace(processedName, "[ʿʲ]", "'");
            processedName = Regex.Replace(processedName, "[ʺⁿ]", "\"");
            processedName = Regex.Replace(processedName, "[–]", "-");
            processedName = Regex.Replace(processedName, "[‎·]", "");
            processedName = Regex.Replace(processedName, "[‎‎]", ""); // Invisible characters

            return processedName;
        }

        string ReplaceCharWithEmbeddedAccentOnNextChar(string name, char originalChar, char replacementChar)
        {
            string processedName = name;

            processedName = Regex.Replace(processedName, $"{originalChar} ", $"{replacementChar}´");
            processedName = Regex.Replace(processedName, $"{originalChar}a", $"{replacementChar}á");
            processedName = Regex.Replace(processedName, $"{originalChar}c", $"{replacementChar}ć");
            processedName = Regex.Replace(processedName, $"{originalChar}e", $"{replacementChar}é");
            processedName = Regex.Replace(processedName, $"{originalChar}g", $"{replacementChar}ǵ");
            processedName = Regex.Replace(processedName, $"{originalChar}i", $"{replacementChar}í");
            processedName = Regex.Replace(processedName, $"{originalChar}m", $"{replacementChar}ḿ");
            processedName = Regex.Replace(processedName, $"{originalChar}n", $"{replacementChar}ń");
            processedName = Regex.Replace(processedName, $"{originalChar}p", $"{replacementChar}ṕ");
            processedName = Regex.Replace(processedName, $"{originalChar}r", $"{replacementChar}ŕ");
            processedName = Regex.Replace(processedName, $"{originalChar}s", $"{replacementChar}ś");
            processedName = Regex.Replace(processedName, $"{originalChar}u", $"{replacementChar}ú");
            processedName = Regex.Replace(processedName, $"{originalChar}y", $"{replacementChar}ý");
            processedName = Regex.Replace(processedName, $"{originalChar}z", $"{replacementChar}ź");
            processedName = Regex.Replace(processedName, $"{originalChar}", $"{replacementChar}");

            return processedName;
        }

        string ReplaceCharWithEmbeddedGraveAccentOnNextChar(string name, char originalChar, char replacementChar)
        {
            string processedName = name;

            processedName = Regex.Replace(processedName, $"{originalChar} ", $"{replacementChar}`");
            processedName = Regex.Replace(processedName, $"{originalChar}i", $"{replacementChar}ì");
            processedName = Regex.Replace(processedName, $"{originalChar}n", $"{replacementChar}ǹ");
            processedName = Regex.Replace(processedName, $"{originalChar}o", $"{replacementChar}ò");
            processedName = Regex.Replace(processedName, $"{originalChar}u", $"{replacementChar}ù");
            processedName = Regex.Replace(processedName, $"{originalChar}y", $"{replacementChar}ỳ");
            processedName = Regex.Replace(processedName, $"{originalChar}", $"{replacementChar}");

            return processedName;
        }

        string ReplaceCharWithEmbeddedHookOnNextChar(string name, char originalChar, char replacementChar)
        {
            string processedName = name;

            processedName = Regex.Replace(processedName, $"{originalChar} ", $"{replacementChar}ʾ");
            processedName = Regex.Replace(processedName, $"{originalChar}a", $"{replacementChar}ả");
            processedName = Regex.Replace(processedName, $"{originalChar}e", $"{replacementChar}ẻ");
            processedName = Regex.Replace(processedName, $"{originalChar}", $"{replacementChar}");

            return processedName;
        }
    }
}
