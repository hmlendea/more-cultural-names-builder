using System.Globalization;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    [SetUpFixture]
    public sealed class IntegrationTestSetup
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        }
    }
}
