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
            processedName = Regex.Replace(processedName, "[ĴJ̌]", "J");
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
            processedName = Regex.Replace(processedName, "[ĵǰ]", "j");
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
            processedName = Regex.Replace(processedName, "[ĤḦḤḪĦ]", "H");
            processedName = Regex.Replace(processedName, "[İĮỊ]", "I");
            processedName = Regex.Replace(processedName, "[ĬĪĨ]", "Ï");
            processedName = Regex.Replace(processedName, "[ĴJ̌]", "J");
            processedName = Regex.Replace(processedName, "[Ƙ]", "K'");
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
            processedName = Regex.Replace(processedName, "[ĥḧḥḫħ]", "h");
            processedName = Regex.Replace(processedName, "[ıįị]", "i");
            processedName = Regex.Replace(processedName, "[ĭīĩ]", "ï");
            processedName = Regex.Replace(processedName, "[ĵǰ]", "j");
            processedName = Regex.Replace(processedName, "[ƙ]", "k'");
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
            processedName = Regex.Replace(processedName, "[Ǧ]", "Ğ");
            processedName = Regex.Replace(processedName, "[Ƣ]", "Ğ"); // Untested in the games
            processedName = Regex.Replace(processedName, "[ȜƔ]", "Gh");
            processedName = Regex.Replace(processedName, "[Ǐ]", "Ĭ");
            processedName = Regex.Replace(processedName, "[ІΙ]", "I");
            processedName = Regex.Replace(processedName, "[Ј]", "J");
            processedName = Regex.Replace(processedName, "[К]", "K");
            processedName = Regex.Replace(processedName, "[ОΟ]", "O");
            processedName = Regex.Replace(processedName, "[Ǒ]", "Ŏ");
            processedName = Regex.Replace(processedName, "[Θ]", "Th");
            processedName = Regex.Replace(processedName, "[Т]", "T");
            processedName = Regex.Replace(processedName, "[Ƿ]", "Uu");
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
            processedName = Regex.Replace(processedName, "[ǧ]", "ğ");
            processedName = Regex.Replace(processedName, "[ƣ]", "ğ"); // Untested in the games
            processedName = Regex.Replace(processedName, "[ȝɣ]", "gh");
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
            processedName = Regex.Replace(processedName, "[ƿ]", "uu");
            processedName = Regex.Replace(processedName, "[ʊ]", "u");
            processedName = Regex.Replace(processedName, "[ǔ]", "ŭ");
            processedName = Regex.Replace(processedName, "[ύ]", "ú");
            processedName = Regex.Replace(processedName, "[ǜ]", "ü");
            processedName = Regex.Replace(processedName, "[ƶ]", "z");

            processedName = Regex.Replace(processedName, "Ấ ", "Â´");
            processedName = Regex.Replace(processedName, "Ấa", "Âá");
            processedName = Regex.Replace(processedName, "Ấc", "Âć");
            processedName = Regex.Replace(processedName, "Ấg", "Âǵ");
            processedName = Regex.Replace(processedName, "Ấi", "Âí");
            processedName = Regex.Replace(processedName, "Ấm", "Âḿ");
            processedName = Regex.Replace(processedName, "Ấn", "Âń");
            processedName = Regex.Replace(processedName, "Ấp", "Âṕ");
            processedName = Regex.Replace(processedName, "Ấs", "Âś");
            processedName = Regex.Replace(processedName, "Ấu", "Âú");
            processedName = Regex.Replace(processedName, "Ấ", "Â");

            processedName = Regex.Replace(processedName, "ấ ", "â´");
            processedName = Regex.Replace(processedName, "ấa", "âá");
            processedName = Regex.Replace(processedName, "ấc", "âć");
            processedName = Regex.Replace(processedName, "ấg", "âǵ");
            processedName = Regex.Replace(processedName, "ấi", "âí");
            processedName = Regex.Replace(processedName, "ấm", "âḿ");
            processedName = Regex.Replace(processedName, "ấn", "âń");
            processedName = Regex.Replace(processedName, "ấp", "âṕ");
            processedName = Regex.Replace(processedName, "ấs", "âś");
            processedName = Regex.Replace(processedName, "ấu", "âú");
            processedName = Regex.Replace(processedName, "ấ", "â");

            processedName = Regex.Replace(processedName, "Ầ ", "Â`");
            processedName = Regex.Replace(processedName, "Ần", "Âǹ");
            processedName = Regex.Replace(processedName, "Ầu", "Âù");
            processedName = Regex.Replace(processedName, "Ầy", "Âỳ");
            processedName = Regex.Replace(processedName, "Ầ", "Â");

            processedName = Regex.Replace(processedName, "ầ ", "â`");
            processedName = Regex.Replace(processedName, "ần", "âǹ");
            processedName = Regex.Replace(processedName, "ầu", "âù");
            processedName = Regex.Replace(processedName, "ầy", "âỳ");
            processedName = Regex.Replace(processedName, "ầ", "â");

            processedName = Regex.Replace(processedName, "Ế ", "Ê´");
            processedName = Regex.Replace(processedName, "Ếa", "Êá");
            processedName = Regex.Replace(processedName, "Ếc", "Êć");
            processedName = Regex.Replace(processedName, "Ếg", "Êǵ");
            processedName = Regex.Replace(processedName, "Ếi", "Êí");
            processedName = Regex.Replace(processedName, "Ếm", "Êḿ");
            processedName = Regex.Replace(processedName, "Ến", "Êń");
            processedName = Regex.Replace(processedName, "Ếp", "Êṕ");
            processedName = Regex.Replace(processedName, "Ếs", "Êś");
            processedName = Regex.Replace(processedName, "Ếu", "Êú");
            processedName = Regex.Replace(processedName, "Ế", "Ê");

            processedName = Regex.Replace(processedName, "ế ", "ê´");
            processedName = Regex.Replace(processedName, "ếa", "êá");
            processedName = Regex.Replace(processedName, "ếc", "êć");
            processedName = Regex.Replace(processedName, "ếg", "êǵ");
            processedName = Regex.Replace(processedName, "ếi", "êí");
            processedName = Regex.Replace(processedName, "ếm", "êḿ");
            processedName = Regex.Replace(processedName, "ến", "êń");
            processedName = Regex.Replace(processedName, "ếp", "êṕ");
            processedName = Regex.Replace(processedName, "ếs", "êś");
            processedName = Regex.Replace(processedName, "ếu", "êú");
            processedName = Regex.Replace(processedName, "ế", "ê");

            processedName = Regex.Replace(processedName, "Ề ", "Ê`");
            processedName = Regex.Replace(processedName, "Ền", "Êǹ");
            processedName = Regex.Replace(processedName, "Ều", "Êù");
            processedName = Regex.Replace(processedName, "Ềy", "Êỳ");
            processedName = Regex.Replace(processedName, "Ề", "Ê");

            processedName = Regex.Replace(processedName, "ề ", "ê`");
            processedName = Regex.Replace(processedName, "ền", "êǹ");
            processedName = Regex.Replace(processedName, "ều", "êù");
            processedName = Regex.Replace(processedName, "ềy", "êỳ");
            processedName = Regex.Replace(processedName, "ề", "ê");

            processedName = Regex.Replace(processedName, "Ể ", "Êʾ");
            processedName = Regex.Replace(processedName, "Ểa", "Êả");
            processedName = Regex.Replace(processedName, "Ểe", "Êẻ");
            processedName = Regex.Replace(processedName, "Ể", "Ê");

            processedName = Regex.Replace(processedName, "ể ", "êʾ");
            processedName = Regex.Replace(processedName, "ểa", "êả");
            processedName = Regex.Replace(processedName, "ểe", "êẻ");
            processedName = Regex.Replace(processedName, "ể", "ê");
            
            processedName = Regex.Replace(processedName, "Ḗ ", "Ē´");
            processedName = Regex.Replace(processedName, "Ḗa", "Ēá");
            processedName = Regex.Replace(processedName, "Ḗc", "Ēć");
            processedName = Regex.Replace(processedName, "Ḗg", "Ēǵ");
            processedName = Regex.Replace(processedName, "Ḗi", "Ēí");
            processedName = Regex.Replace(processedName, "Ḗm", "Ēḿ");
            processedName = Regex.Replace(processedName, "Ḗn", "Ēń");
            processedName = Regex.Replace(processedName, "Ḗp", "Ēṕ");
            processedName = Regex.Replace(processedName, "Ḗs", "Ēś");
            processedName = Regex.Replace(processedName, "Ḗu", "Ēú");
            processedName = Regex.Replace(processedName, "Ḗ", "Ē");

            processedName = Regex.Replace(processedName, "ḗ ", "ē´");
            processedName = Regex.Replace(processedName, "ḗa", "ēá");
            processedName = Regex.Replace(processedName, "ḗc", "ēć");
            processedName = Regex.Replace(processedName, "ḗg", "ēǵ");
            processedName = Regex.Replace(processedName, "ḗi", "ēí");
            processedName = Regex.Replace(processedName, "ḗm", "ēḿ");
            processedName = Regex.Replace(processedName, "ḗn", "ēń");
            processedName = Regex.Replace(processedName, "ḗp", "ēṕ");
            processedName = Regex.Replace(processedName, "ḗs", "ēś");
            processedName = Regex.Replace(processedName, "ḗu", "ēú");
            processedName = Regex.Replace(processedName, "ḗ", "ē");

            processedName = Regex.Replace(processedName, "Ṓ ", "Ō´");
            processedName = Regex.Replace(processedName, "Ṓa", "Ōá");
            processedName = Regex.Replace(processedName, "Ṓc", "Ōć");
            processedName = Regex.Replace(processedName, "Ṓg", "Ōǵ");
            processedName = Regex.Replace(processedName, "Ṓi", "Ōí");
            processedName = Regex.Replace(processedName, "Ṓm", "Ōḿ");
            processedName = Regex.Replace(processedName, "Ṓn", "Ōń");
            processedName = Regex.Replace(processedName, "Ṓp", "Ōṕ");
            processedName = Regex.Replace(processedName, "Ṓs", "Ōś");
            processedName = Regex.Replace(processedName, "Ṓu", "Ōú");
            processedName = Regex.Replace(processedName, "Ṓ", "Ō");

            processedName = Regex.Replace(processedName, "ṓ ", "ō´");
            processedName = Regex.Replace(processedName, "ṓa", "ōá");
            processedName = Regex.Replace(processedName, "ṓc", "ōć");
            processedName = Regex.Replace(processedName, "ṓg", "ōǵ");
            processedName = Regex.Replace(processedName, "ṓi", "ōí");
            processedName = Regex.Replace(processedName, "ṓm", "ōḿ");
            processedName = Regex.Replace(processedName, "ṓn", "ōń");
            processedName = Regex.Replace(processedName, "ṓp", "ōṕ");
            processedName = Regex.Replace(processedName, "ṓs", "ōś");
            processedName = Regex.Replace(processedName, "ṓu", "ōú");
            processedName = Regex.Replace(processedName, "ṓ", "ō");
            
            processedName = Regex.Replace(processedName, "Ồ ", "Ô`");
            processedName = Regex.Replace(processedName, "Ồn", "Ôǹ");
            processedName = Regex.Replace(processedName, "Ồu", "Ôù");
            processedName = Regex.Replace(processedName, "Ồy", "Ôỳ");
            processedName = Regex.Replace(processedName, "Ồ", "Ô");

            processedName = Regex.Replace(processedName, "ồ ", "ô`");
            processedName = Regex.Replace(processedName, "ồn", "ôǹ");
            processedName = Regex.Replace(processedName, "ồu", "ôù");
            processedName = Regex.Replace(processedName, "ồy", "ôỳ");
            processedName = Regex.Replace(processedName, "ồ", "ô");
            
            processedName = Regex.Replace(processedName, "Ổ ", "Ôʾ");
            processedName = Regex.Replace(processedName, "Ổa", "Ôả");
            processedName = Regex.Replace(processedName, "Ổe", "Ôẻ");
            processedName = Regex.Replace(processedName, "Ổ", "Ô");

            processedName = Regex.Replace(processedName, "ổ ", "ôʾ");
            processedName = Regex.Replace(processedName, "ổa", "ôả");
            processedName = Regex.Replace(processedName, "ổe", "ôẻ");
            processedName = Regex.Replace(processedName, "ổ", "ô");

            processedName = Regex.Replace(processedName, "Ố ", "Ô´");
            processedName = Regex.Replace(processedName, "Ốa", "Ôá");
            processedName = Regex.Replace(processedName, "Ốc", "Ôć");
            processedName = Regex.Replace(processedName, "Ốg", "Ôǵ");
            processedName = Regex.Replace(processedName, "Ối", "Ôí");
            processedName = Regex.Replace(processedName, "Ốm", "Ôḿ");
            processedName = Regex.Replace(processedName, "Ốn", "Ôń");
            processedName = Regex.Replace(processedName, "Ốp", "Ôṕ");
            processedName = Regex.Replace(processedName, "Ốs", "Ôś");
            processedName = Regex.Replace(processedName, "Ốu", "Ôú");
            processedName = Regex.Replace(processedName, "Ố", "Ô");

            processedName = Regex.Replace(processedName, "ố ", "ô´");
            processedName = Regex.Replace(processedName, "ốa", "ôá");
            processedName = Regex.Replace(processedName, "ốc", "ôć");
            processedName = Regex.Replace(processedName, "ốg", "ôǵ");
            processedName = Regex.Replace(processedName, "ối", "ôí");
            processedName = Regex.Replace(processedName, "ốm", "ôḿ");
            processedName = Regex.Replace(processedName, "ốn", "ôń");
            processedName = Regex.Replace(processedName, "ốp", "ôṕ");
            processedName = Regex.Replace(processedName, "ốs", "ôś");
            processedName = Regex.Replace(processedName, "ốu", "ôú");
            processedName = Regex.Replace(processedName, "ố", "ô");

            processedName = Regex.Replace(processedName, "[ʾ‘ʻ’ʼ′]", "´");
            processedName = Regex.Replace(processedName, "[ʿ]", "`");
            processedName = Regex.Replace(processedName, "[ʿʲ]", "'");
            processedName = Regex.Replace(processedName, "[ʺⁿ]", "\"");
            processedName = Regex.Replace(processedName, "[–]", "-");
            processedName = Regex.Replace(processedName, "[‎·]", "");
            processedName = Regex.Replace(processedName, "[‎‎]", ""); // Invisible characters

            return processedName;
        }
    }
}
