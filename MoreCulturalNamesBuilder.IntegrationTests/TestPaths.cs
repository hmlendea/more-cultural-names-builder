using System;
using System.IO;
using System.Text;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    internal static class TestPaths
    {
        internal static string TestDataDirectory => Path.Combine(AppContext.BaseDirectory, "TestData");

        internal static string ExpectedDirectory => Path.Combine(AppContext.BaseDirectory, "Expected");

        internal static string LanguagesFilePath => Path.Combine(TestDataDirectory, "languages.xml");

        internal static string LocationsFilePath => Path.Combine(TestDataDirectory, "locations.xml");

        internal static string EmptyLanguagesFilePath => Path.Combine(TestDataDirectory, "empty_languages.xml");

        internal static string EmptyLocationsFilePath => Path.Combine(TestDataDirectory, "empty_locations.xml");

        internal static string MalformedLanguagesFilePath =>
            Path.Combine(TestDataDirectory, "malformed_languages.xml");

        internal static string MalformedLocationsFilePath =>
            Path.Combine(TestDataDirectory, "malformed_locations.xml");

        internal static string DuplicateLanguagesFilePath =>
            Path.Combine(TestDataDirectory, "duplicate_languages.xml");

        internal static string DuplicateLocationsFilePath =>
            Path.Combine(TestDataDirectory, "duplicate_locations.xml");

        internal static string NamelessLocationsFilePath =>
            Path.Combine(TestDataDirectory, "nameless_locations.xml");

        internal static string NonNumericHoI4LocationsFilePath =>
            Path.Combine(TestDataDirectory, "non_numeric_hoi4_locations.xml");

        internal static string NonNumericImperatorRomeLocationsFilePath =>
            Path.Combine(TestDataDirectory, "non_numeric_ir_locations.xml");

        internal static string DuplicateHoI4LocationsFilePath =>
            Path.Combine(TestDataDirectory, "duplicate_hoi4_locations.xml");

        internal static string DuplicateImperatorRomeLocationsFilePath =>
            Path.Combine(TestDataDirectory, "duplicate_ir_locations.xml");

        internal static string Ck2LandedTitlesFilePath =>
            Path.Combine(TestDataDirectory, "ck2_landed_titles.txt");

        internal static string Ck3LandedTitlesFilePath =>
            Path.Combine(TestDataDirectory, "ck3_landed_titles.txt");

        internal static string RichLanguagesFilePath =>
            Path.Combine(TestDataDirectory, "rich_languages.xml");

        internal static string RichLocationsFilePath =>
            Path.Combine(TestDataDirectory, "rich_locations.xml");

        internal static string RichCk2LandedTitlesFilePath =>
            Path.Combine(TestDataDirectory, "rich_ck2_landed_titles.txt");

        internal static string RichCk3LandedTitlesFilePath =>
            Path.Combine(TestDataDirectory, "rich_ck3_landed_titles.txt");

        internal static string FallbackLanguagesFilePath =>
            Path.Combine(TestDataDirectory, "fallback_languages.xml");

        internal static string FallbackLocationsFilePath =>
            Path.Combine(TestDataDirectory, "fallback_locations.xml");

        internal static string FallbackCk2LandedTitlesFilePath =>
            Path.Combine(TestDataDirectory, "fallback_ck2_landed_titles.txt");

        internal static string FallbackCk3LandedTitlesFilePath =>
            Path.Combine(TestDataDirectory, "fallback_ck3_landed_titles.txt");

        internal static string DanglingFallbackLanguagesFilePath =>
            Path.Combine(TestDataDirectory, "dangling_fallback_languages.xml");

        internal static string DanglingFallbackLocationsFilePath =>
            Path.Combine(TestDataDirectory, "dangling_fallback_locations.xml");

        internal static string DanglingFallbackCk2LandedTitlesFilePath =>
            Path.Combine(TestDataDirectory, "dangling_fallback_ck2_landed_titles.txt");

        internal static string UnknownLanguageLanguagesFilePath =>
            Path.Combine(TestDataDirectory, "unknown_language_languages.xml");

        internal static string UnknownLanguageLocationsFilePath =>
            Path.Combine(TestDataDirectory, "unknown_language_locations.xml");

        internal static string UnknownLanguageCk2LandedTitlesFilePath =>
            Path.Combine(TestDataDirectory, "unknown_language_ck2_landed_titles.txt");

        internal static string BlankNameLocationsFilePath =>
            Path.Combine(TestDataDirectory, "blank_name_locations.xml");

        internal static string BlankNameCk2LandedTitlesFilePath =>
            Path.Combine(TestDataDirectory, "blank_name_ck2_landed_titles.txt");

        internal static string DuplicateHoI4StatesFilePath =>
            Path.Combine(TestDataDirectory, "duplicate_hoi4_states.xml");

        internal static string MissingStoreFilePath => Path.Combine(TestDataDirectory, "missing_store.xml");

        internal static string MissingLandedTitlesFilePath =>
            Path.Combine(TestDataDirectory, "missing_landed_titles.txt");

        internal static Encoding Windows1252Encoding
        {
            get
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                return Encoding.GetEncoding("windows-1252");
            }
        }
    }
}
