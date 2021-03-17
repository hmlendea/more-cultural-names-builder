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
            string processedName = name;

            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            if (ck3cache.ContainsKey(name))
            {
                return ck3cache[name];
            }

            processedName = Regex.Replace(processedName, "[Ḗ]", "É");
            processedName = Regex.Replace(processedName, "[Ḫ]", "H");
            processedName = Regex.Replace(processedName, "[ḫ]", "h");
            processedName = Regex.Replace(processedName, "[ɬ]", "ł");
            processedName = Regex.Replace(processedName, "[ṓ]", "ö");

            ck3cache.TryAdd(name, processedName);

            return processedName;
        }

        public string ToHOI4Charset(string name)
        {
            string processedName = name;

            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            if (hoi4cache.ContainsKey(name))
            {
                return hoi4cache[name];
            }

            processedName = Regex.Replace(processedName, "[ḂḄ]", "B");
            processedName = Regex.Replace(processedName, "[Ə]", "E");
            processedName = Regex.Replace(processedName, "[ƘḲ]", "K");
            processedName = Regex.Replace(processedName, "[ȚṬТ]", "T");
            processedName = Regex.Replace(processedName, "[Ș]", "Ş");
            processedName = Regex.Replace(processedName, "[ḃḅ]", "b");
            processedName = Regex.Replace(processedName, "[еə]", "e");
            processedName = Regex.Replace(processedName, "[ƙкḳ]", "k");
            processedName = Regex.Replace(processedName, "[ɬ]", "ł");
            processedName = Regex.Replace(processedName, "[țṭ]", "t");
            processedName = Regex.Replace(processedName, "[ș]", "ş");

            hoi4cache.TryAdd(name, processedName);

            return processedName;
        }

        public string ToWindows1252(string name)
        {
            string processedName = name;

            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            if (windows1252cache.ContainsKey(name))
            {
                return windows1252cache[name];
            }

            processedName = Regex.Replace(processedName, "[ĂĀ]", "Ã");
            processedName = Regex.Replace(processedName, "[ḂḄ]", "B");
            processedName = Regex.Replace(processedName, "[Ć]", "C");
            processedName = Regex.Replace(processedName, "[Č]", "Ch");
            processedName = Regex.Replace(processedName, "[ƊḌ]", "D");
            processedName = Regex.Replace(processedName, "[Đ]", "Dj");
            processedName = Regex.Replace(processedName, "[Ē]", "Ë");
            processedName = Regex.Replace(processedName, "[Ḗ]", "É");
            processedName = Regex.Replace(processedName, "[ĘƏ]", "E");
            processedName = Regex.Replace(processedName, "[Ğ]", "G");
            processedName = Regex.Replace(processedName, "[Ḫ]", "H");
            processedName = Regex.Replace(processedName, "[İ]", "I");
            processedName = Regex.Replace(processedName, "[Ī]", "Ï");
            processedName = Regex.Replace(processedName, "[ƘḲ]", "K");
            processedName = Regex.Replace(processedName, "[Ł]", "L");
            processedName = Regex.Replace(processedName, "[Ń]", "N");
            processedName = Regex.Replace(processedName, "[Ō]", "Ö");
            processedName = Regex.Replace(processedName, "[Ő]", "Õ");
            processedName = Regex.Replace(processedName, "[ȘŞṢŚ]", "S");
            processedName = Regex.Replace(processedName, "[ȚṬТ]", "T");
            processedName = Regex.Replace(processedName, "[Ť]", "Ty");
            processedName = Regex.Replace(processedName, "[Ū]", "Ü");
            processedName = Regex.Replace(processedName, "[Ư]", "U'");
            processedName = Regex.Replace(processedName, "[ŹŻ]", "Z");
            processedName = Regex.Replace(processedName, "[ą]", "a");
            processedName = Regex.Replace(processedName, "[ăā]", "ã");
            processedName = Regex.Replace(processedName, "[ḃḅ]", "b");
            processedName = Regex.Replace(processedName, "[ć]", "c");
            processedName = Regex.Replace(processedName, "[č]", "ch");
            processedName = Regex.Replace(processedName, "[đɗḍ]", "d");
            processedName = Regex.Replace(processedName, "[ě]", "ie");
            processedName = Regex.Replace(processedName, "[ē]", "ë");
            processedName = Regex.Replace(processedName, "[ė]", "è");
            processedName = Regex.Replace(processedName, "[ęеə]", "e");
            processedName = Regex.Replace(processedName, "[ğ]", "g");
            processedName = Regex.Replace(processedName, "[ḫ]", "h");
            processedName = Regex.Replace(processedName, "[ı]", "i");
            processedName = Regex.Replace(processedName, "[ī]", "ï");
            processedName = Regex.Replace(processedName, "[ƙкḳ]", "k");
            processedName = Regex.Replace(processedName, "[ł]", "l");
            processedName = Regex.Replace(processedName, "[ɬ]", "thl");
            processedName = Regex.Replace(processedName, "[ń]", "n");
            processedName = Regex.Replace(processedName, "[ō]", "õ");
            processedName = Regex.Replace(processedName, "[ő]", "ö");
            processedName = Regex.Replace(processedName, "[ř]", "rz");
            processedName = Regex.Replace(processedName, "[șşṣś]", "s");
            processedName = Regex.Replace(processedName, "[țṭ]", "t");
            processedName = Regex.Replace(processedName, "[ū]", "ü");
            processedName = Regex.Replace(processedName, "[ύ]", "ú");
            processedName = Regex.Replace(processedName, "[źż]", "z");

            processedName = Regex.Replace(processedName, "[ʻʿ]", "'");

            windows1252cache.TryAdd(name, processedName);

            return processedName;
        }
    }
}
