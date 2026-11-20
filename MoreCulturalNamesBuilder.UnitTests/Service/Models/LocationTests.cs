using System.Collections.Generic;

using MoreCulturalNamesBuilder.Service.Models;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.UnitTests.Service.Models
{
    [TestFixture]
    public sealed class LocationTests
    {
        [Test]
        public void GivenNamesWithoutFallbackLocations_WhenCheckingIfEmpty_ThenTheLocationIsNotEmpty()
        {
            Location location = BuildLocation(
                [new() { LanguageId = "Romanian", Value = "Cluj-Napoca" }],
                []);

            Assert.That(location.IsEmpty(), Is.False);
        }

        [TestCase("Oradea")]
        [TestCase("Dezmir")]
        public void GivenFallbackLocationsWithoutNames_WhenCheckingIfEmpty_ThenTheLocationIsNotEmpty(
            string fallbackLocation)
        {
            Location location = BuildLocation([], [fallbackLocation]);

            Assert.That(location.IsEmpty(), Is.False);
        }

        [Test]
        public void GivenNeitherNamesNorFallbackLocations_WhenCheckingIfEmpty_ThenTheLocationIsEmpty()
        {
            Location location = BuildLocation([], []);

            Assert.That(location.IsEmpty());
        }

        private static Location BuildLocation(
            IEnumerable<Name> names,
            IEnumerable<string> fallbackLocations)
            => new()
            {
                Id = "Cluj-Napoca",
                GeoNamesId = "681290",
                GameIds = [new() { Game = "CK3", Id = "c_cluj" }],
                FallbackLocations = fallbackLocations,
                Names = names
            };
    }
}
