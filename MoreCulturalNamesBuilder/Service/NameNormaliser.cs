using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NuciText.Conversion;

namespace MoreCulturalNamesBuilder.Service
{
    public sealed class NameNormaliser(INuciTextConverter textConverter) : INameNormaliser
    {
        static readonly Regex StartsWithPhiRegex = new("\\bɸ", RegexOptions.Compiled);
        static readonly Regex TaMarbutaRegex = new("[a]*[ẗ]", RegexOptions.Compiled);
        static readonly Regex UpperGWithOptionalHRegex = new("[Ġ]([^h])", RegexOptions.Compiled);
        static readonly Regex LowerGWithOptionalHRegex = new("[ġ]([^h])", RegexOptions.Compiled);
        static readonly Regex UpperGWithHRegex = new("[Ġ](h)", RegexOptions.Compiled);
        static readonly Regex LowerGWithHRegex = new("[ġ](h)", RegexOptions.Compiled);
        static readonly Regex CurlyApostropheRegex = new("[‘’]", RegexOptions.Compiled);
        static readonly Regex FloatingDiacriticsRegex = new("[̧̣̤̦̓́̀̆̂̌̈̋̄̍̃͘᠌̬]", RegexOptions.Compiled);
        static readonly Regex OtherFloatingDiacriticsRegex = new("(ॎ|઼|‌ॎ)", RegexOptions.Compiled);
        static readonly Regex AttachedDiacriticsRegex = new("[・̲̥̮̱̇̐͡]", RegexOptions.Compiled);
        static readonly Regex ReversedGlottalStopRegex = new("[ʔ]", RegexOptions.Compiled);
        static readonly Regex AcuteApostropheLikeRegex = new("[ʾʻʼʽʹ′]", RegexOptions.Compiled);
        static readonly Regex BacktickApostropheLikeRegex = new("[ʿ]", RegexOptions.Compiled);
        static readonly Regex StraightApostropheLikeRegex = new("[ꞌʿˀʲь]", RegexOptions.Compiled);
        static readonly Regex QuoteLikeRegex = new("[ʺ″]", RegexOptions.Compiled);
        static readonly Regex ZeroWidthJoinersRegex = new("[‌‍]", RegexOptions.Compiled);
        static readonly Regex DashLikeRegex = new("[–—]", RegexOptions.Compiled);
        static readonly Regex ColonLikeRegex = new("[꞉]", RegexOptions.Compiled);
        static readonly Regex DirectionalityMarksRegex = new("[‎·]", RegexOptions.Compiled);
        static readonly Regex OtherInvisibleMarksRegex = new("[＝̷̯̰̊̒]", RegexOptions.Compiled);
        static readonly Regex ZeroWidthSpaceRegex = new("[​]", RegexOptions.Compiled);
        static readonly Regex InvisibleCharactersRegex = new("([‎‎])", RegexOptions.Compiled);
        static readonly Regex UpperAAlternativesRegex = new("(𝖠|A‍)", RegexOptions.Compiled);
        static readonly Regex UpperABeforeCanadianSyllabicsRegex = new("( ᐋ)", RegexOptions.Compiled);
        static readonly Regex UpperBAlternativesRegex = new("(B‍|B‌|پ)", RegexOptions.Compiled);
        static readonly Regex UpperMAlternativesRegex = new("(M̄|M̐)", RegexOptions.Compiled);
        static readonly Regex UpperPAlternativesRegex = new("(P‍|П)", RegexOptions.Compiled);
        static readonly Regex UpperRAlternativesRegex = new("(R‍|R‌)", RegexOptions.Compiled);
        static readonly Regex UpperSAlternativesRegex = new("(S‍|S‌)", RegexOptions.Compiled);
        static readonly Regex LowerAAlternativesRegex = new("(𝖺|a‍)", RegexOptions.Compiled);
        static readonly Regex LowerACanadianSyllabicsRegex = new("([^ ])ᐋ", RegexOptions.Compiled);
        static readonly Regex LowerBAlternativesRegex = new("(b‍|b‌)", RegexOptions.Compiled);
        static readonly Regex LowerDAlternativesRegex = new("(𝖽|d‍‌)", RegexOptions.Compiled);
        static readonly Regex LowerGAlternativesRegex = new("(g‍|g‌)", RegexOptions.Compiled);
        static readonly Regex LowerMAlternativesRegex = new("(m̄|m̐|m̃)", RegexOptions.Compiled);
        static readonly Regex LowerPAlternativesRegex = new("(p‍|п)", RegexOptions.Compiled);
        static readonly Regex LowerRAlternativesRegex = new("(r‍|r‌)", RegexOptions.Compiled);
        static readonly Regex LowerSAlternativesRegex = new("(s‍|s‌)", RegexOptions.Compiled);

        readonly ConcurrentDictionary<string, string> ck3cache = new();
        readonly ConcurrentDictionary<string, string> hoi4citiesCache = new();
        readonly ConcurrentDictionary<string, string> hoi4statesCache = new();
        readonly ConcurrentDictionary<string, string> irCache = new();

        readonly Dictionary<char, string> CommonCharacterMappings = new()
        {
            { 'А', "A" },
            { 'Α', "A" },
            { 'Ꭺ', "A" },
            { 'ꓮ', "A" },
            { 'Ά', "Á" },
            { 'Ὰ', "À" }, { 'Ȁ', "À" },
            { 'Ắ', "Ă" }, { 'Ặ', "Ă" },
            { 'Ẩ', "Â" },
            { 'Β', "B" }, { 'Ᏼ', "B" }, { 'ꓐ', "B" }, { 'Ḇ', "B" },
            { 'Χ', "Ch" },
            { 'С', "C" }, { 'Ϲ', "C" }, { 'Ꮯ', "C" }, { 'ꓚ', "C" },
            { 'Ĉ', "C" }, { 'Ц', "C" },
            { 'Ꭰ', "D" },
            { 'ꓓ', "D" },
            { 'Џ', "Dž" },
            { 'Ɖ', "Đ" },
            { 'Е', "E" }, { 'Ε', "E" }, { 'Ꭼ', "E" }, { 'ꓰ', "E" }, { 'Ɛ', "E" }, { 'Э', "E" },
            { 'Ё', "Ë" },
            { 'Έ', "É" },
            { '∃', "Ǝ" },
            { 'ꓝ', "F" }, { 'Ḟ', "F" },
            { 'Ꮐ', "G" }, { 'ꓖ', "G" },
            { 'Ƣ', "Ğ" }, // Untested in the games
            { 'Ȝ', "Gh" }, // Or G
            { 'Ɣ', "Gh" },
            { 'Ю', "Iu" },
            { 'Η', "H" }, { 'Ꮋ', "H" }, { 'ꓧ', "H" }, { 'Ḥ', "H" },
            { 'І', "I" }, { 'Ι', "I" }, { 'Ӏ', "I" }, { 'ӏ', "I" }, { 'Ί', "I" }, { 'Ɨ', "I" },
            { 'Ỉ', "Ì" },
            { 'Ї', "Ï" }, { 'Ϊ', "Ï" }, { 'Ḯ', "Ï" },
            { 'Ǐ', "Ĭ" },
            { 'Ј', "J" }, { 'Ꭻ', "J" }, { 'ꓙ', "J" },
            { 'К', "K" }, { 'Κ', "K" }, { 'Ꮶ', "K" }, { 'ꓗ', "K" },
            { 'Ќ', "Ḱ" },
            { 'Ꮮ', "L" }, { 'ꓡ', "L" }, { 'Լ', "L" },
            { 'М', "M" }, { 'Μ', "M" }, { 'Ꮇ', "M" }, { 'ꓟ', "M" }, { 'Ṁ', "M" },
            { 'Ǌ', "NJ" },
            { 'Н', "N" }, { 'Ν', "N" }, { 'ꓠ', "N" }, { 'Ṉ', "N" },
            { 'Ƞ', "Ŋ" },
            { 'О', "O" }, { 'Ο', "O" }, { 'ꓳ', "O" }, { 'Օ', "O" }, { 'Ɔ', "O" }, { 'Ợ', "O" },
            { 'Ӧ', "Ö" },
            { 'Ớ', "Ó" }, { 'Ό', "Ó" },
            { 'Ỏ', "Ò" },
            { 'Ỗ', "Ô" },
            { 'Ǒ', "Ŏ" },
            { 'Ǭ', "Ǫ" },
            { 'Р', "P" }, { 'Ρ', "P" }, { 'Ꮲ', "P" }, { 'ꓑ', "P" },
            { 'Ƿ', "Uu" }, { 'Ỽ', "Uu" }, // Or W
            { 'Ԛ', "Q" },
            { 'Ꮢ', "R" }, { 'ꓣ', "R" }, { 'Ṟ', "R" },
            { 'Ѕ', "S" }, { 'Ꮪ', "S" }, { 'ꓢ', "S" }, { 'Տ', "S" },
            { 'Ṯ', "Th" }, { 'Θ', "Th" },
            { 'Т', "T" }, { 'Τ', "T" }, { 'Ꭲ', "T" }, { 'ꓔ', "T" },
            { 'Ս', "U" }, { 'ꓴ', "U" }, { 'Ʊ', "U" },
            { 'Ǔ', "Ŭ" },
            { 'Ǚ', "Ŭ" }, // Or Ü
            { 'Ǜ', "Ü" },
            { 'В', "V" }, { 'Ꮩ', "V" }, { 'ꓦ', "V" },
            { 'Ꮃ', "W" }, { 'ꓪ', "W" }, { 'Ԝ', "W" },
            { 'Ẇ', "Ẃ" },
            { 'Х', "X" }, { 'ꓫ', "X" },
            { 'Ү', "Y" }, { 'Υ', "Y" }, { 'ꓬ', "Y" },
            { 'Ύ', "Ý" },
            { 'Ζ', "Z" }, { 'Ꮓ', "Z" }, { 'ꓜ', "Z" }, { 'Ƶ', "Z" },
            { 'Ǯ', "Ž" },

            { 'ә', "æ" },
            { 'α', "a" }, { 'а', "a" },
            { 'ὰ', "à" }, { 'ȁ', "à" },
            { 'ά', "á" }, { 'ȧ', "á" },
            { 'ӑ', "ă" }, { 'ắ', "ă" }, { 'ǎ', "ă" }, { 'ẵ', "ă" }, { 'ặ', "ă" },
            { 'ẩ', "â" },
            { 'ᏼ', "b" }, { 'ḇ', "b" },
            { 'χ', "ch" },
            { 'ĉ', "c" }, { 'ц', "c" },
            { 'ⅾ', "d" },
            { 'џ', "dž" },
            { 'е', "e" }, { 'ε', "e" }, { 'ɛ', "e" }, { 'э', "e" },
            { 'ĕ', "ě" },
            { 'ǝ', "ə" },
            { 'ё', "ë" },
            { 'έ', "é" },
            { 'ḟ', "f" },
            { 'г', "g" },
            { 'ƣ', "ğ" }, // Untested in the games
            { 'ȝ', "gh" }, // Or g
            { 'ɣ', "gh" },
            { 'ḥ', "h" },
            { 'ю', "iu" },
            { 'я', "ia" },
            { 'і', "i" }, { 'ι', "i" }, { 'ɨ', "i" },
            { 'ỉ', "ì" },
            { 'ɩ', "ı" },
            { 'ǐ', "ĭ" },
            { 'ї', "ï" }, { 'ϊ', "ï" }, { 'ΐ', "ï" }, { 'ḯ', "ï" },
            { 'ј', "j" },
            { 'к', "k" }, { 'κ', "k" },
            { 'ќ', "ḱ" },
            { 'ẖ', "kh" },
            { 'л', "l" },
            { 'ɬ', "ł" },
            { 'ƚ', "ł" },
            { 'ṁ', "m" },
            { 'н', "n" }, { 'ṉ', "n" },
            { 'ƞ', "ŋ" },
            { 'о', "o" }, { 'ο', "o" }, { 'օ', "o" }, { 'ɔ', "o" }, { 'ợ', "o" },
            { 'ӧ', "ö" },
            { 'ό', "ó" }, { 'ớ', "ó" },
            { 'ỏ', "ò" },
            { 'ỗ', "ô" },
            { 'ǒ', "ŏ" },
            { 'ǭ', "ǫ" },
            { 'р', "p" }, { 'ṗ', "p" }, { 'ɸ', "p" },
            { 'ԥ', "p" }, // It's actually ṗ but that doesn't work either
            { 'ꮢ', "r" }, { 'ṟ', "r" },
            { 'ṯ', "th" }, { 'θ', "th" },
            { 'т', "t" },
            { '‡', "t" }, // Guessed
            { 'ƿ', "uu" }, { 'ỽ', "uu" }, // Or w
            { 'у', "u" }, { 'ʊ', "u" },
            { 'ǔ', "ŭ" },
            { 'ǚ', "ŭ" }, // Or ü
            { 'ύ', "ú" },
            { 'ǜ', "ü" },
            { 'ẇ', "ẃ" },
            { 'γ', "y" },
            { 'ƶ', "z" }, { 'ᶻ', "z" },
            { 'ǯ', "ž" },

            // Characters with apostrophe that needs to be detached
            { 'ƙ', "k'" },
            { 'Ƙ', "K'" },
            { 'ư', "u'" },
            { 'Ư', "U'" },
            { 'ứ', "ú'" },
            { 'Ứ', "Ú'" },
            { 'ừ', "ù'" },
            { 'Ừ', "Ù'" },
            { 'ử', "ủ'" },
            { 'Ử', "Ủ'" },

            // Secondary accent diacritic
            { 'Ấ', "Â" },
            { 'Ḗ', "Ē" },
            { 'Ế', "Ê" },
            { 'Ṓ', "Ō" },
            { 'Ố', "Ô" },
            { 'ấ', "â" },
            { 'ḗ', "ē" },
            { 'ế', "ê" },
            { 'ṓ', "ō" },
            { 'ố', "ô" },

            // Secondary grave accent diacritic
            { 'Ầ', "Â" },
            { 'Ề', "Ê" },
            { 'Ồ', "Ô" },
            { 'ầ', "â" },
            { 'ề', "ê" },
            { 'ồ', "ô" },

            // Secondary hook diacritic
            { 'Ể', "Ê" },
            { 'Ổ', "Ô" },
            { 'ể', "ê" },
            { 'ổ', "ô" },
        };

        readonly Dictionary<char, string> CK3CharacterMappings = new()
        {
            { 'Ǣ', "Æ" },
            { 'Ạ', "A" }, { 'Ə', "A" },
            { 'Ả', "À" },
            { 'Ǟ', "Ä" },
            { 'Ậ', "Â" },
            { 'Ḃ', "B" }, { 'Ḅ', "B" },
            { 'Ḏ', "D" }, { 'Ḍ', "D" }, { 'Ɗ', "D" }, { 'Ḑ', "D" },
            { 'Ẹ', "E" }, { 'Ǝ', "E" },
            { 'Ẻ', "È" },
            { 'Ệ', "Ê" },
            { 'Ẽ', "Ē" },
            { 'Ǵ', "G" },
            { 'Ḧ', "H" }, { 'Ḩ', "H" },
            { 'Ȟ', "Ĥ" },
            { 'Ị', "Į" },
            { 'Ǧ', "Ğ" }, // J
            { 'Ḫ', "Kh" },
            { 'Ḱ', "K" }, { 'Ǩ', "K" },
            { 'Ḳ', "Ķ" }, { 'Ḵ', "Ķ" },
            { 'Ḷ', "Ļ" },
            { 'Ḿ', "M" }, { 'Ṃ', "M" },
            { 'Ɲ', "N" }, { 'Ŋ', "N" },
            { 'Ǹ', "En" },
            { 'Ṅ', "Ń" },
            { 'Ṇ', "Ņ" },
            { 'Ọ', "O" }, { 'Ơ', "O" },
            { 'Ȯ', "Ó" },
            { 'Ờ', "Ò" },
            { 'Ǫ', "Ö" },
            { 'Ȫ', "Õ" }, { 'Ỡ', "Õ" },
            { 'Ộ', "Ô" },
            { 'Ṕ', "P" },
            { 'Ṙ', "Ŕ" },
            { 'Ṛ', "Ŗ" },
            { 'Ṡ', "Ś" },
            { 'Ṣ', "Ș" },
            { 'Ṭ', "Ț" },
            { 'Ụ', "U" },
            { 'Ṳ', "Ü" },
            { 'Ủ', "Ů" },
            { 'Ṿ', "V" },
            { 'Ẍ', "X" },
            { 'Ẏ', "Ý" },
            { 'Ȳ', "Ÿ" },
            { 'Ẓ', "Z" },

            { 'ǣ', "æ" },
            { 'ạ', "a" }, { 'ə', "a" },
            { 'ả', "à" },
            { 'ǟ', "ä" },
            { 'ậ', "â" },
            { 'ḃ', "b" }, { 'ḅ', "b" },
            { 'ḏ', "d" }, { 'ḍ', "d" }, { 'ɗ', "d" }, { 'ɖ', "d" }, { 'ḑ', "d" },
            { 'ẹ', "e" },
            { 'ẻ', "è" },
            { 'ệ', "ê" },
            { 'ẽ', "ē" },
            { 'ǵ', "g" },
            { 'ǧ', "ğ" }, // j
            { 'ḧ', "h" }, { 'ḩ', "h" },
            { 'ḫ', "kh" },
            { 'ȟ', "ĥ" },
            { 'ị', "į" },
            { 'ǰ', "ĵ" },
            { 'ḱ', "k" }, { 'ǩ', "k" },
            { 'ḳ', "ķ" }, { 'ḵ', "ķ" },
            { 'ḷ', "ļ" },
            { 'ḿ', "m" }, { 'ṃ', "m" },
            { 'ɲ', "n" }, { 'ŋ', "n" },
            { 'ǹ', "en" },
            { 'ṅ', "ń" },
            { 'ṇ', "ņ" },
            { 'ọ', "o" }, { 'ơ', "o" },
            { 'ȯ', "ó" },
            { 'ờ', "ò" },
            { 'ǫ', "ö" },
            { 'ȫ', "õ" }, { 'ỡ', "õ" },
            { 'ộ', "ô" },
            { 'ṕ', "p" },
            { 'ṙ', "ŕ" },
            { 'ṛ', "ŗ" },
            { 'ṡ', "ś" },
            { 'ṣ', "ș" },
            { 'ṭ', "ț" },
            { 'ụ', "u" },
            { 'ṳ', "ü" },
            { 'ủ', "ů" },
            { 'ṿ', "v" },
            { 'ẍ', "x" },
            { 'ẏ', "ý" },
            { 'ȳ', "ÿ" },
            { 'ẓ', "z" }, { 'ʐ', "z" },
        };

        readonly Dictionary<char, string> Hoi4CityCharacterMappings = new()
        {
            { 'Ǣ', "Æ" },
            { 'Ạ', "A" }, { 'Ə', "A" },
            { 'Ả', "À" },
            { 'Ǟ', "Ä" },
            { 'Ậ', "Â" },
            { 'Ḃ', "B" }, { 'Ḅ', "B" },
            { 'Ḏ', "D" }, { 'Ḍ', "D" }, { 'Ɗ', "D" }, { 'Ḑ', "D" },
            { 'Ẹ', "E" }, { 'Ǝ', "E" },
            { 'Ẻ', "È" },
            { 'Ệ', "Ê" },
            { 'Ẽ', "Ē" },
            { 'Ǵ', "G" },
            { 'Ḧ', "H" }, { 'Ḩ', "H" },
            { 'Ȟ', "Ĥ" },
            { 'Ị', "Į" },
            { 'Ǧ', "Ğ" }, // J
            { 'Ḫ', "Kh" },
            { 'Ḱ', "Ќ" }, { 'Ǩ', "Ќ" },
            { 'Ḵ', "Ķ" }, { 'Ḳ', "Ķ" },
            { 'Ḷ', "Ļ" },
            { 'Ḿ', "M" }, { 'Ṃ', "M" },
            { 'Ɲ', "N" },
            { 'Ǹ', "En" },
            { 'Ṅ', "Ń" },
            { 'Ṇ', "Ņ" },
            { 'Ọ', "O" }, { 'Ơ', "O" },
            { 'Ȯ', "Ó" },
            { 'Ờ', "Ò" },
            { 'Ȫ', "Õ" }, { 'Ỡ', "Õ" },
            { 'Ǫ', "Ö" },
            { 'Ộ', "Ô" },
            { 'Ǿ', "Ø" },
            { 'Ṕ', "P" },
            { 'Ṛ', "R" },
            { 'Ṙ', "Ŕ" },
            { 'Ș', "Ş" },
            { 'Ṡ', "Ś" },
            { 'Ṣ', "S" },
            { 'Ț', "Ţ" }, { 'Ṭ', "Ţ" },
            { 'Ụ', "U" },
            { 'Ṳ', "Ü" },
            { 'Ủ', "Ů" },
            { 'Ṿ', "V" },
            { 'Ẅ', "W" },
            { 'Ẃ', "Ŵ" },
            { 'Ẍ', "X" },
            { 'Ỳ', "Ý" },
            { 'Ȳ', "Ÿ" }, { 'Ẏ', "Ÿ" },
            { 'Ẓ', "Z" },

            { 'ǣ', "æ" },
            { 'ạ', "a" }, { 'ə', "a" },
            { 'ả', "à" },
            { 'ǟ', "ä" },
            { 'ậ', "â" },
            { 'ḃ', "b" }, { 'ḅ', "b" },
            { 'ḏ', "d" }, { 'ḍ', "d" }, { 'ɗ', "d" }, { 'ɖ', "d" }, { 'ḑ', "d" },
            { 'ẹ', "e" },
            { 'ẻ', "è" },
            { 'ệ', "ê" },
            { 'ẽ', "ē" },
            { 'ǵ', "g" },
            { 'ǧ', "ğ" }, // j
            { 'ḧ', "h" }, { 'ḩ', "h" },
            { 'ȟ', "ĥ" },
            { 'ḫ', "kh" },
            { 'ĩ', "ï" },
            { 'ị', "į" },
            { 'ḱ', "ќ" }, { 'ǩ', "ќ" },
            { 'ḵ', "ķ" }, { 'ḳ', "ķ" },
            { 'ḷ', "ļ" },
            { 'ḿ', "m" }, { 'ṃ', "m" },
            { 'ɲ', "n" },
            { 'ǹ', "en" },
            { 'ṅ', "ń" },
            { 'ṇ', "ņ" },
            { 'ọ', "o" }, { 'ơ', "o" },
            { 'ȯ', "ó" },
            { 'ờ', "ò" },
            { 'ȫ', "õ" },
            { 'ỡ', "õ" },
            { 'ǫ', "ö" },
            { 'ộ', "ô" },
            { 'ǿ', "ø" },
            { 'ṕ', "p" },
            { 'ṛ', "r" },
            { 'ṙ', "ŕ" },
            { 'ș', "ş" },
            { 'ṡ', "ś" },
            { 'ṣ', "s" },
            { 'ț', "ţ" }, { 'ṭ', "ţ" },
            { 'ụ', "u" },
            { 'ṳ', "ü" },
            { 'ủ', "ů" },
            { 'ṿ', "v" },
            { 'ẅ', "w" },
            { 'ẃ', "ŵ" },
            { 'ẍ', "x" },
            { 'ỳ', "ý" }, { 'ẏ', "ý" },
            { 'ȳ', "ÿ" },
            { 'ẓ', "z" }, { 'ʐ', "z" },
        };

        readonly Dictionary<char, string> Hoi4StateCharacterMappings = new()
        {
            { 'Ă', "Ã" }, { 'Ā', "Ã" },
            { 'Č', "Ch" },
            { 'Ć', "C" }, { 'Ĉ', "C" }, { 'Ċ', "C" },
            { 'Ď', "D" },
            { 'Ē', "Ë" },
            { 'Ė', "É" },
            { 'Ě', "Ê" },
            { 'Ę', "E" },
            { 'Ğ', "G" }, { 'Ĝ', "G" }, { 'Ģ', "G" },
            { 'Ǧ', "J" },
            { 'Ĥ', "H" },
            { 'İ', "I" },
            { 'Ĭ', "Ï" }, { 'Ī', "Ï" }, { 'Ĩ', "Ï" },
            { 'Ĺ', "L" }, { 'Ľ', "L" }, { 'Ļ', "L" },
            { 'Ň', "Ñ" },
            { 'Ń', "N" }, { 'Ņ', "N" },
            { 'Ō', "Õ" },
            { 'Ő', "Ö" },
            { 'Ŏ', "Ô" },
            { 'Ŕ', "R" }, { 'Ř', "R" },
            { 'Ś', "S" }, { 'Ŝ', "S" }, { 'Ş', "S" },
            { 'Ť', "Ty" },
            { 'Ţ', "T" },
            { 'Ů', "U" }, { 'Ų', "U" },
            { 'Ū', "Ü" }, { 'Ŭ', "Ü" }, { 'Ű', "Ü" },
            { 'Ŷ', "Y" },
            { 'Ź', "Z" },
            { 'Ż', "Ž" },

            { 'ă', "ã" }, { 'ā', "ã" },
            { 'ą', "a" },
            { 'č', "ch" },
            { 'ć', "c" }, { 'ĉ', "c" }, { 'ċ', "c" },
            { 'ď', "d" },
            { 'ē', "ë" },
            { 'ė', "é" },
            { 'ě', "ê" },
            { 'ę', "e" },
            { 'ğ', "g" }, { 'ĝ', "g" }, { 'ģ', "g" },
            { 'ǧ', "j" },
            { 'ĥ', "h" },
            { 'ĭ', "ï" }, { 'ī', "ï" }, { 'ĩ', "ï" },
            { 'ĺ', "l" }, { 'ľ', "l" }, { 'ļ', "l" },
            { 'ň', "ñ" },
            { 'ń', "n" }, { 'ņ', "n" },
            { 'ō', "õ" },
            { 'ő', "ö" },
            { 'ŏ', "ô" },
            { 'ŕ', "r" }, { 'ř', "r" },
            { 'ś', "s" }, { 'ŝ', "s" }, { 'ş', "s" },
            { 'ť', "ty" },
            { 'ţ', "t" },
            { 'ů', "u" }, { 'ų', "u" },
            { 'ū', "ü" }, { 'ŭ', "ü" }, { 'ű', "ü" },
            { 'ŷ', "y" },
            { 'ź', "z" },
            { 'ż', "ž" },
        };

        readonly Dictionary<char, string> ImperatorRomeCharacterMappings = new()
        {
            { 'Ǣ', "Æ" },
            { 'Ạ', "A" }, { 'Ə', "A" },
            { 'Ǟ', "Ä" },
            { 'Ậ', "Â" },
            { 'Ả', "À" },
            { 'Č', "Ch" },
            { 'Ć', "C" }, { 'Ĉ', "C" }, { 'Ċ', "C" },
            { 'Ď', "D" },
            { 'Ḑ', "Ḍ" },
            { 'Ę', "E" }, { 'Ẹ', "E" }, { 'Ǝ', "E" },
            { 'Ė', "É" },
            { 'Ẻ', "È" },
            { 'Ệ', "Ê" },
            { 'Ğ', "G" }, { 'Ĝ', "G" }, { 'Ģ', "G" }, { 'Ǵ', "G" },
            { 'Ǧ', "J" },
            { 'Ĥ', "H" }, { 'Ȟ', "H" }, { 'Ḧ', "H" }, { 'Ḩ', "H" }, { 'Ħ', "H" },
            { 'İ', "I" }, { 'Į', "I" }, { 'Ị', "I" },
            { 'Ĭ', "Ī" }, { 'Ĩ', "Ī" },
            { 'Ĵ', "J" },
            { 'Ḱ', "K" }, { 'Ḳ', "K" }, { 'Ķ', "K" }, { 'Ḵ', "K" }, { 'Ǩ', "K" }, { 'Ќ', "K" },
            { 'Ĺ', "L" }, { 'Ł', "L" }, { 'Ľ', "L" }, { 'Ḷ', "L" }, { 'Ļ', "L" },
            { 'Ṃ', "M" }, { 'Ḿ', "M" },
            { 'Ǹ', "En" },
            { 'Ņ', "N" }, { 'Ŋ', "N" }, { 'Ɲ', "N" },
            { 'Ơ', "O" },
            { 'Ȯ', "Ó" },
            { 'Ờ', "Ò" },
            { 'Ỡ', "Õ" }, { 'Ȫ', "Õ" },
            { 'Ŏ', "Õ" }, // Maybe replace with Oe
            { 'Ő', "Ö" },
            { 'Ṕ', "P" },
            { 'Ř', "Rz" },
            { 'Ṙ', "Ŕ" },
            { 'Š', "Sh" },
            { 'Ś', "S" }, { 'Ŝ', "S" }, { 'Ş', "S" }, { 'Ṣ', "S" }, { 'Ș', "S" },
            { 'Ť', "Ty" },
            { 'Ț', "T" },
            { 'Ţ', "T" },
            { 'Ṭ', "T" },
            { 'Ŧ', "T" },
            { 'Ů', "U" }, { 'Ų', "U" }, { 'Ụ', "U" },
            { 'Ǔ', "Ü" }, { 'Ŭ', "Ü" }, { 'Ű', "Ü" }, { 'Ṳ', "Ü" },
            { 'Ũ', "Ū" },
            { 'Ủ', "Ů" },
            { 'Ṿ', "V" },
            { 'Ẍ', "X" },
            { 'Ŷ', "Y" }, { 'Ẏ', "Y" },
            { 'Ȳ', "Ÿ" },
            { 'Ž', "Zh" },
            { 'Ƶ', "Z" }, { 'Ź', "Z" }, { 'Ż', "Z" }, { 'Ẓ', "Z" },

            { 'ǣ', "æ" },
            { 'ạ', "a" }, { 'ə', "a" }, { 'ą', "a" },
            { 'ǟ', "ä" },
            { 'ậ', "â" },
            { 'ả', "à" },
            { 'ć', "c" }, { 'ĉ', "c" }, { 'ċ', "c" },
            { 'č', "ch" },
            { 'ď', "d" },
            { 'ḑ', "ḍ" },
            { 'ę', "e" }, { 'ẹ', "e" },
            { 'ė', "é" },
            { 'ẻ', "è" },
            { 'ẽ', "ē" },
            { 'ğ', "g" }, { 'ĝ', "g" }, { 'ģ', "g" }, { 'ǵ', "g" },
            { 'ĥ', "h" }, { 'ȟ', "h" }, { 'ḧ', "h" }, { 'ḩ', "h" }, { 'ħ', "h" },
            { 'į', "i" }, { 'ị', "i" },
            { 'ĭ', "ī" }, { 'ĩ', "ī" },
            { 'ĵ', "j" }, { 'ǰ', "j" }, { 'ǧ', "j" },
            { 'ḱ', "k" }, { 'ḳ', "k" }, { 'ķ', "k" }, { 'ḵ', "k" }, { 'ǩ', "k" }, { 'ќ', "k" },
            { 'ĺ', "l" }, { 'ł', "l" }, { 'ľ', "l" }, { 'ḷ', "l" }, { 'ļ', "l" },
            { 'ṃ', "m" }, { 'ḿ', "m" },
            { 'ǹ', "en" },
            { 'ņ', "n" }, { 'ŋ', "n" }, { 'ɲ', "n" },
            { 'ơ', "o" },
            { 'ờ', "ò" },
            { 'ȯ', "ó" },
            { 'ỡ', "õ" }, { 'ȫ', "õ" },
            { 'ŏ', "õ" }, // Maybe replace with oe
            { 'ő', "ö" },
            { 'ṕ', "p" },
            { 'ř', "rz" },
            { 'ṙ', "ŕ" },
            { 'ś', "s" }, { 'ŝ', "s" }, { 'ş', "s" }, { 'ṣ', "s" }, { 'ș', "s" },
            { 'ß', "ss" },
            { 'š', "sh" },
            { 'ț', "t" }, { 'ţ', "t" }, { 'ṭ', "t" }, { 'ŧ', "t" },
            { 'ť', "ty" },
            { 'ů', "u" }, { 'ų', "u" }, { 'ụ', "u" },
            { 'ǔ', "ü" }, { 'ŭ', "ü" }, { 'ű', "ü" }, { 'ṳ', "ü" },
            { 'ũ', "ū" },
            { 'ủ', "ů" },
            { 'ṿ', "v" },
            { 'ẍ', "x" },
            { 'ŷ', "y" }, { 'ẏ', "y" },
            { 'ȳ', "ÿ" },
            { 'ƶ', "z" }, { 'ź', "z" }, { 'ż', "z" }, { 'ẓ', "z" }, { 'ʐ', "z" },
            { 'ž', "zh" }
        };

        public string ToCK3Charset(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            if (ck3cache.TryGetValue(name, out string value))
            {
                return value;
            }

            string processedName = ApplyCommonReplacements(name);
            processedName = ReplaceUsingMap(processedName, CK3CharacterMappings);

            // Crusader Kings III
            processedName = processedName.Replace("J̌", "Ĵ");
            processedName = processedName.Replace("T̈", "T");
            processedName = processedName.Replace("āẗ", "āh");
            processedName = TaMarbutaRegex.Replace(processedName, "ah");

            ck3cache.TryAdd(name, processedName);

            return processedName;
        }

        public string ToHOI4CityCharset(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            if (hoi4citiesCache.TryGetValue(name, out string value))
            {
                return value;
            }

            string processedName = ApplyCommonReplacements(name);

            // Hearts of Iron IV Cities
            processedName = processedName.Replace("āẗ", "āh");
            processedName = TaMarbutaRegex.Replace(processedName, "ah");

            processedName = ReplaceUsingMap(processedName, Hoi4CityCharacterMappings);

            processedName = CurlyApostropheRegex.Replace(processedName, "´");

            hoi4citiesCache.TryAdd(name, processedName);

            return processedName;
        }

        public string ToHOI4StateCharset(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            if (hoi4statesCache.TryGetValue(name, out string value))
            {
                return value;
            }

            string processedName = name
                .Replace("Ġh", "Gh")
                .Replace("ġh", "gh")
                .Replace("iīẗ", "iyyah")
                .Replace("īẗ", "iyah");

            processedName = UpperGWithOptionalHRegex.Replace(processedName, "Gh$1");
            processedName = LowerGWithOptionalHRegex.Replace(processedName, "gh$1");

            processedName = ReplaceUsingMap(processedName, Hoi4StateCharacterMappings);
            processedName = ToHOI4CityCharset(processedName);

            hoi4statesCache.TryAdd(name, processedName);

            return processedName;
        }

        public string ToImperatorRomeCharset(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            if (irCache.TryGetValue(name, out string value))
            {
                return value;
            }

            string processedName = name;

            processedName = Regex.Replace(processedName, "iīẗ", "iyyah");
            processedName = Regex.Replace(processedName, "īẗ", "iyah");

            processedName = ApplyCommonReplacements(processedName);

            // Imperator: Rome
            processedName = UpperGWithOptionalHRegex.Replace(processedName, "Gh$1");
            processedName = UpperGWithHRegex.Replace(processedName, "Gh");
            processedName = processedName.Replace("J̌", "J");
            processedName = processedName.Replace("T̈", "T");
            processedName = processedName.Replace("āẗ", "āh");
            processedName = TaMarbutaRegex.Replace(processedName, "ah");
            processedName = LowerGWithOptionalHRegex.Replace(processedName, "gh$1");
            processedName = LowerGWithHRegex.Replace(processedName, "gh");

            processedName = ReplaceUsingMap(processedName, ImperatorRomeCharacterMappings);

            irCache.TryAdd(name, processedName);

            return processedName;
        }

        public string ToWindows1252(string name)
            => textConverter.ToWindows1252(name);

        private string ApplyCommonReplacements(string name)
        {
            string processedName = name;

            processedName = StartsWithPhiRegex.Replace(processedName, "P");

            processedName = ReplaceUsingMap(processedName, CommonCharacterMappings);

            processedName = processedName
                .Replace("D‍", "D")
                .Replace("G‍", "G")
                .Replace("H̱", "Kh")
                .Replace("Ϊ́", "Ï")
                .Replace("K‍", "K")
                .Replace("L‌", "L")
                .Replace("N‌", "N")
                .Replace("Ṉ", "Ņ")
                .Replace("R̥̄", "Ŕu")
                .Replace("R̥", "Ru")
                .Replace("Ṭ‍", "Ṭ");

            processedName = UpperAAlternativesRegex.Replace(processedName, "A");
            processedName = UpperABeforeCanadianSyllabicsRegex.Replace(processedName, " Â");
            processedName = UpperBAlternativesRegex.Replace(processedName, "B");
            processedName = UpperMAlternativesRegex.Replace(processedName, "M");
            processedName = UpperPAlternativesRegex.Replace(processedName, "P");
            processedName = UpperRAlternativesRegex.Replace(processedName, "R");
            processedName = UpperSAlternativesRegex.Replace(processedName, "S");

            processedName = processedName
                .Replace("ḡ", "ğ") // Untested in the games
                .Replace("ڭ", "ġ")
                .Replace("j‌", "j")
                .Replace("k‍", "k")
                .Replace("l‌", "l")
                .Replace("ǌ", "nj")
                .Replace("ⁿ", "n") // Superscript n - nasal sound
                .Replace("n‌", "n")
                .Replace("ṉ", "ņ")
                .Replace("r̥̄", "ŕu")
                .Replace("r̥", "ru")
                .Replace("ṭ‍", "ṭ");

            processedName = LowerAAlternativesRegex.Replace(processedName, "a");
            processedName = LowerACanadianSyllabicsRegex.Replace(processedName, "$1â");
            processedName = LowerBAlternativesRegex.Replace(processedName, "b");
            processedName = LowerDAlternativesRegex.Replace(processedName, "d");
            processedName = LowerGAlternativesRegex.Replace(processedName, "g");
            processedName = LowerMAlternativesRegex.Replace(processedName, "m");
            processedName = LowerPAlternativesRegex.Replace(processedName, "p");
            processedName = LowerRAlternativesRegex.Replace(processedName, "r");
            processedName = LowerSAlternativesRegex.Replace(processedName, "s");

            // Floating vertical lines
            processedName = processedName
                .Replace("a̍", "ȧ")
                .Replace("e̍", "ė")
                .Replace("i̍", "i")
                .Replace("o̍", "ȯ")
                .Replace("u̍", "ú");

            // Floating accents
            processedName = processedName
                .Replace("á", "á")
                .Replace("ć", "ć")
                .Replace("é", "é")
                .Replace("ǵ", "ǵ")
                .Replace("í", "í")
                .Replace("ḿ", "ḿ")
                .Replace("ń", "ń")
                .Replace("ṕ", "ṕ")
                .Replace("ŕ", "ŕ")
                .Replace("ś", "ś")
                .Replace("ú", "ú")
                .Replace("ý", "ý")
                .Replace("ź", "ź");

            // Floating grave accents
            processedName = processedName
                .Replace("ì", "ì")
                .Replace("ǹ", "ǹ")
                .Replace("ò", "ò")
                .Replace("ù", "ù")
                .Replace("ỳ", "ỳ");

            // Floating umlauts
            processedName = processedName
                .Replace("T̈", "T̈")
                .Replace("ä", "ä")
                .Replace("ā̈", "ǟ")
                .Replace("ą̈", "ą̈")
                .Replace("b̈", "b̈")
                .Replace("c̈", "c̈")
                .Replace("ë", "ë")
                .Replace("ɛ̈̈", "ë")
                .Replace("ḧ", "ḧ")
                .Replace("ï", "ï")
                .Replace("j̈", "j̈")
                .Replace("k̈", "k̈")
                .Replace("l̈", "l̈")
                .Replace("m̈", "m̈")
                .Replace("n̈", "n̈")
                .Replace("ö", "ö")
                .Replace("ō̈", "ȫ")
                .Replace("ǫ̈", "ǫ̈")
                .Replace("ɔ̈̈", "ö")
                .Replace("p̈", "p̈")
                .Replace("q̈", "q̈")
                .Replace("q̣̈", "q̣̈")
                .Replace("r̈", "r̈")
                .Replace("s̈", "s̈")
                .Replace("ẗ", "t") // Because ẗ is a
                .Replace("ü", "ü")
                .Replace("v̈", "v̈")
                .Replace("ẅ", "ẅ")
                .Replace("ẍ", "ẍ")
                .Replace("ÿ", "ÿ")
                .Replace("z̈", "z̈");

            // Floating tildas
            processedName = processedName
                .Replace("ã", "ã")
                .Replace("ẽ", "ẽ")
                .Replace("ĩ", "ĩ")
                .Replace("ñ", "ñ")
                .Replace("õ", "õ")
                .Replace("ũ", "ũ")
                .Replace("ṽ", "ṽ")
                .Replace("ỹ", "ỹ");

            // Floating carets
            processedName = processedName.Replace("ṳ̂", "û");

            // Floating commas
            processedName = processedName.Replace("A̓", "Á"); // Or Á?

            // Other floating diacritics
            processedName = FloatingDiacriticsRegex.Replace(processedName, "");
            processedName = OtherFloatingDiacriticsRegex.Replace(processedName, ""); // ???
            processedName = AttachedDiacriticsRegex.Replace(processedName, ""); // Diacritics that attach to characters... I guess

            processedName = ReversedGlottalStopRegex.Replace(processedName, "ʾ");
            processedName = AcuteApostropheLikeRegex.Replace(processedName, "´");
            processedName = BacktickApostropheLikeRegex.Replace(processedName, "`");
            processedName = StraightApostropheLikeRegex.Replace(processedName, "'");
            processedName = QuoteLikeRegex.Replace(processedName, "\"");
            processedName = ZeroWidthJoinersRegex.Replace(processedName, "");
            processedName = DashLikeRegex.Replace(processedName, "-");
            processedName = ColonLikeRegex.Replace(processedName, ":");
            processedName = DirectionalityMarksRegex.Replace(processedName, "");
            processedName = OtherInvisibleMarksRegex.Replace(processedName, "");
            processedName = ZeroWidthSpaceRegex.Replace(processedName, "");
            processedName = InvisibleCharactersRegex.Replace(processedName, ""); // Invisible characters

            return processedName;
        }

        private static string ReplaceUsingMap(string input, Dictionary<char, string> map)
        {
            if (input is null)
            {
                return null;
            }

            int firstReplaceableCharacterIndex = -1;

            for (int characterIndex = 0; characterIndex < input.Length; characterIndex += 1)
            {
                if (map.ContainsKey(input[characterIndex]))
                {
                    firstReplaceableCharacterIndex = characterIndex;
                    break;
                }
            }

            if (firstReplaceableCharacterIndex is -1)
            {
                return input;
            }

            StringBuilder sb = new(input.Length);
            sb.Append(input, 0, firstReplaceableCharacterIndex);

            for (int characterIndex = firstReplaceableCharacterIndex; characterIndex < input.Length; characterIndex += 1)
            {
                char currentCharacter = input[characterIndex];

                if (map.TryGetValue(currentCharacter, out string replacement))
                {
                    sb.Append(replacement);
                    continue;
                }

                sb.Append(currentCharacter);
            }

            return sb.ToString();
        }
    }
}
