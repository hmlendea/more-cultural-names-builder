using System;
using System.IO;
using System.Text;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    internal static class TestPaths
    {
        internal static string TestDataDirectory => Path.Combine(AppContext.BaseDirectory, "TestData");

        internal static string LanguagesFilePath => Path.Combine(TestDataDirectory, "languages.xml");

        internal static string LocationsFilePath => Path.Combine(TestDataDirectory, "locations.xml");

        internal static string EmptyLanguagesFilePath => Path.Combine(TestDataDirectory, "empty_languages.xml");

        internal static string EmptyLocationsFilePath => Path.Combine(TestDataDirectory, "empty_locations.xml");

        internal static string MalformedLanguagesFilePath =>
            Path.Combine(TestDataDirectory, "malformed_languages.xml");

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
