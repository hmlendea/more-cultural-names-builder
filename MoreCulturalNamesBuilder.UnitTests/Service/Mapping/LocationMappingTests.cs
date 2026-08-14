using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;
using MoreCulturalNamesBuilder.UnitTests.TestInfrastructure;

namespace MoreCulturalNamesBuilder.UnitTests.Service.Mapping
{
    [TestFixture]
    public sealed class LocationMappingTests
    {
        [Test]
        public void GivenAServiceModel_WhenMappingToADataObject_ThenNestedValuesArePreserved()
        {
            Location serviceModel = BuildLocation("Cluj-Napoca", "c_cluj");

            LocationEntity dataObject = InternalMappingInvoker.Invoke<LocationEntity>(
                "LocationMapping",
                "ToDataObject",
                serviceModel);

            Assert.Multiple(() =>
            {
                Assert.That(dataObject.Id, Is.EqualTo("Cluj-Napoca"));
                Assert.That(dataObject.GeoNamesId, Is.EqualTo("681290"));
                Assert.That(dataObject.GameIds.Single().Id, Is.EqualTo("c_cluj"));
                Assert.That(dataObject.FallbackLocations, Is.EqualTo(new[] { "Oradea", "Dezmir" }));
                Assert.That(dataObject.Names.Single().Value, Is.EqualTo("Cluj-Napoca"));
            });
        }

        [Test]
        public void GivenEmptyNestedSequences_WhenMappingToADataObject_ThenEmptyListsAreReturned()
        {
            Location serviceModel = new()
            {
                Id = "Cluj-Napoca",
                GeoNamesId = null,
                GameIds = [],
                FallbackLocations = [],
                Names = []
            };

            LocationEntity dataObject = InternalMappingInvoker.Invoke<LocationEntity>(
                "LocationMapping",
                "ToDataObject",
                serviceModel);

            Assert.Multiple(() =>
            {
                Assert.That(dataObject.GameIds, Is.Empty);
                Assert.That(dataObject.FallbackLocations, Is.Empty);
                Assert.That(dataObject.Names, Is.Empty);
            });
        }

        [Test]
        public void GivenMultipleServiceModels_WhenMappingToDataObjects_ThenOrderIsPreserved()
        {
            IEnumerable<Location> serviceModels =
            [
                BuildLocation("Oradea", "c_oradea"),
                BuildLocation("Cluj-Napoca", "c_cluj")
            ];

            IEnumerable<LocationEntity> dataObjects = InternalMappingInvoker.Invoke<IEnumerable<LocationEntity>>(
                "LocationMapping",
                "ToDataObjects",
                serviceModels);

            Assert.That(dataObjects.Select(dataObject => dataObject.Id), Is.EqualTo(new[] { "Oradea", "Cluj-Napoca" }));
        }

        private static Location BuildLocation(string locationId, string locationGameId)
            => new()
            {
                Id = locationId,
                GeoNamesId = "681290",
                GameIds = [new() { Game = "CK3", Id = locationGameId }],
                FallbackLocations = ["Oradea", "Dezmir"],
                Names = [new() { LanguageId = "Romanian", Value = locationId }]
            };
    }
}