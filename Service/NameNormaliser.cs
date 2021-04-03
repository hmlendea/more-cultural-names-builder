using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace MoreCulturalNamesModBuilder.Service
{
    public sealed class NameNormaliser : INameNormaliser
    {
        ConcurrentDictionary<string, string> windows1252cache;
        ConcurrentDictionary<string, string> ck3cache;
        ConcurrentDictionary<string, string> hoi4cache;

        public NameNormaliser()
        {
            windows1252cache = new ConcurrentDictionary<string, string>();
            ck3cache = new ConcurrentDictionary<string, string>();
            hoi4cache = new ConcurrentDictionary<string, string>();
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

            processedName = Regex.Replace(processedName, "[Ḏ]", "D");
            processedName = Regex.Replace(processedName, "[Ḫ]", "H");
            processedName = Regex.Replace(processedName, "[ḫ]", "h");
            processedName = Regex.Replace(processedName, "[ɬ]", "ł");
            processedName = Regex.Replace(processedName, "[ǫ]", "ọ");
            processedName = Regex.Replace(processedName, "[ṭ]", "ț");
            processedName = Regex.Replace(processedName, "[ẓʐ]", "z");

            ck3cache.TryAdd(name, processedName);

            return processedName;
        }

        public string ToHOI4Charset(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            if (hoi4cache.ContainsKey(name))
            {
                return hoi4cache[name];
            }

            string processedName = ApplyCommonReplacements(name);

            processedName = Regex.Replace(processedName, "[ḂḄ]", "B");
            processedName = Regex.Replace(processedName, "[Ə]", "E");
            processedName = Regex.Replace(processedName, "[ƘḲ]", "K");
            processedName = Regex.Replace(processedName, "[ȚṬТ]", "T");
            processedName = Regex.Replace(processedName, "[Ș]", "Ş");
            processedName = Regex.Replace(processedName, "[ḃḅ]", "b");
            processedName = Regex.Replace(processedName, "[ə]", "e");
            processedName = Regex.Replace(processedName, "[ƙḳ]", "k");
            processedName = Regex.Replace(processedName, "[ɬ]", "ł");
            processedName = Regex.Replace(processedName, "[țṭ]", "t");
            processedName = Regex.Replace(processedName, "[ș]", "ş");

            hoi4cache.TryAdd(name, processedName);

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

            processedName = processedName.Replace("A̓", "A");
            processedName = Regex.Replace(processedName, "[ĂĀ]", "Ã");
            processedName = Regex.Replace(processedName, "[ḂḄ]", "B");
            processedName = Regex.Replace(processedName, "[Ć]", "C");
            processedName = Regex.Replace(processedName, "[Č]", "Ch");
            processedName = Regex.Replace(processedName, "[ƊḌ]", "D");
            processedName = Regex.Replace(processedName, "[Đ]", "Dj");
            processedName = Regex.Replace(processedName, "[Ē]", "Ë");
            processedName = Regex.Replace(processedName, "[ĘƏƎ]", "E");
            processedName = Regex.Replace(processedName, "[Ğ]", "G");
            processedName = Regex.Replace(processedName, "[ḪḤ]", "H");
            processedName = Regex.Replace(processedName, "[İ]", "I");
            processedName = Regex.Replace(processedName, "[Ī]", "Ï");
            processedName = Regex.Replace(processedName, "[ƘḲ]", "K");
            processedName = Regex.Replace(processedName, "[Ł]", "L");
            processedName = Regex.Replace(processedName, "[Ń]", "N");
            processedName = Regex.Replace(processedName, "[Ō]", "Õ");
            processedName = Regex.Replace(processedName, "[Ő]", "Ö");
            processedName = Regex.Replace(processedName, "[Ǿ]", "Ø");
            processedName = Regex.Replace(processedName, "[ȘŞṢŚ]", "S");
            processedName = Regex.Replace(processedName, "[ȚṬТ]", "T");
            processedName = Regex.Replace(processedName, "[Ť]", "Ty");
            processedName = Regex.Replace(processedName, "[Ū]", "Ü");
            processedName = Regex.Replace(processedName, "[Ư]", "U'");
            processedName = Regex.Replace(processedName, "[ŹŻ]", "Z");
            processedName = Regex.Replace(processedName, "[ą]", "a");
            processedName = Regex.Replace(processedName, "[ăǎā]", "ã");
            processedName = Regex.Replace(processedName, "[ḃḅ]", "b");
            processedName = Regex.Replace(processedName, "[ć]", "c");
            processedName = Regex.Replace(processedName, "[č]", "ch");
            processedName = Regex.Replace(processedName, "[đɗḍḏ]", "d");
            processedName = Regex.Replace(processedName, "[ě]", "ie");
            processedName = Regex.Replace(processedName, "[ēẽ]", "ë");
            processedName = Regex.Replace(processedName, "[ė]", "è");
            processedName = Regex.Replace(processedName, "[ęəǝ]", "e");
            processedName = Regex.Replace(processedName, "[ğ]", "g");
            processedName = Regex.Replace(processedName, "[ḫḥ]", "h");
            processedName = Regex.Replace(processedName, "[ı]", "i");
            processedName = Regex.Replace(processedName, "[ī]", "ï");
            processedName = Regex.Replace(processedName, "[ƙḳ]", "k");
            processedName = Regex.Replace(processedName, "[ł]", "l");
            processedName = Regex.Replace(processedName, "[ɬ]", "thl");
            processedName = Regex.Replace(processedName, "[ń]", "n");
            processedName = Regex.Replace(processedName, "[ō]", "õ");
            processedName = Regex.Replace(processedName, "[ő]", "ö");
            processedName = Regex.Replace(processedName, "[ǿ]", "ø");
            processedName = Regex.Replace(processedName, "[ǫọ]", "o");
            processedName = Regex.Replace(processedName, "[ř]", "rz");
            processedName = Regex.Replace(processedName, "[șşṣś]", "s");
            processedName = Regex.Replace(processedName, "[țṭ]", "t");
            processedName = Regex.Replace(processedName, "[ū]", "ü");
            processedName = Regex.Replace(processedName, "[źżẓʐ]", "z");

            windows1252cache.TryAdd(name, processedName);

            return processedName;
        }

        private string ApplyCommonReplacements(string name)
        {
            string processedName = name;

            processedName = Regex.Replace(processedName, "[Ḗ]", "Ē");
            processedName = Regex.Replace(processedName, "[Ο]", "O");
            processedName = Regex.Replace(processedName, "[αа]", "a");
            processedName = Regex.Replace(processedName, "[ὰ]", "à");
            processedName = Regex.Replace(processedName, "[ά]", "á");
            processedName = Regex.Replace(processedName, "[ḗ]", "ē");
            processedName = Regex.Replace(processedName, "[е]", "e");
            processedName = Regex.Replace(processedName, "[έ]", "é");
            processedName = Regex.Replace(processedName, "[ι]", "i");
            processedName = Regex.Replace(processedName, "[к]", "k");
            processedName = Regex.Replace(processedName, "[ό]", "ó");
            processedName = Regex.Replace(processedName, "[ṓ]", "ō");
            processedName = Regex.Replace(processedName, "[о]", "o");
            processedName = Regex.Replace(processedName, "[ύ]", "ú");
            processedName = Regex.Replace(processedName, "[ʻʿ]", "'");
            processedName = Regex.Replace(processedName, "[‎]", ""); // Invisible characters

            return processedName;
        }
    }
}
