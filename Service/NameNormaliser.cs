using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace MoreCulturalNamesModBuilder.Service
{
    public sealed class NameNormaliser : INameNormaliser
    {
        ConcurrentDictionary<string, string> windows1252cache;

        public NameNormaliser()
        {
            windows1252cache = new ConcurrentDictionary<string, string>();
        }

        public string ToWindows1252(string name)
        {
            string processedName = name;

            if (windows1252cache.ContainsKey(name))
            {
                return windows1252cache[name];
            }

            processedName = Regex.Replace(processedName, "[ĂĀ]", "Ã");
            processedName = Regex.Replace(processedName, "[ḂḄ]", "B");
            processedName = Regex.Replace(processedName, "[Ć]", "C");
            processedName = Regex.Replace(processedName, "[Č]", "Ch");
            processedName = Regex.Replace(processedName, "[ĐƊḌ]", "D");
            processedName = Regex.Replace(processedName, "[Ē]", "Ë");
            processedName = Regex.Replace(processedName, "[ĘƏ]", "E");
            processedName = Regex.Replace(processedName, "[Ğ]", "G");
            processedName = Regex.Replace(processedName, "[İ]", "I");
            processedName = Regex.Replace(processedName, "[Ī]", "Ï");
            processedName = Regex.Replace(processedName, "[ƘḲ]", "K");
            processedName = Regex.Replace(processedName, "[Ł]", "L");
            processedName = Regex.Replace(processedName, "[Ń]", "N");
            processedName = Regex.Replace(processedName, "[Ō]", "Ö");
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
            processedName = Regex.Replace(processedName, "[ı]", "i");
            processedName = Regex.Replace(processedName, "[ī]", "ï");
            processedName = Regex.Replace(processedName, "[ƙкḳ]", "k");
            processedName = Regex.Replace(processedName, "[ł]", "l");
            processedName = Regex.Replace(processedName, "[ń]", "n");
            processedName = Regex.Replace(processedName, "[ō]", "ö");
            processedName = Regex.Replace(processedName, "[ř]", "rz");
            processedName = Regex.Replace(processedName, "[șşṣś]", "s");
            processedName = Regex.Replace(processedName, "[țṭ]", "t");
            processedName = Regex.Replace(processedName, "[ū]", "ü");
            processedName = Regex.Replace(processedName, "[źż]", "z");

            processedName = Regex.Replace(processedName, "[ʻ]", "'");

            windows1252cache.TryAdd(name, processedName);

            return processedName;
        }
    }
}
