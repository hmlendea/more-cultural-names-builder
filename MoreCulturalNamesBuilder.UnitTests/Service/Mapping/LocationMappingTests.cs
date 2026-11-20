using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;
using MoreCulturalNamesBuilder.UnitTests.TestInfrastructure;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.UnitTests.Service.Mapping
{
    [TestFixture]
    public sealed class LocationMappingTests
    {
        [Test]
        public void GivenAServiceModel_WhenMappingToADataObject_ThenNestedValuesArePreserved()
        {
            Location serviceModel = BuildLocation("Cluj-Napoca", "c_cluj");

            LocationDataObject dataObject = InternalMappingInvoker.Invoke<LocationDataObject>(
                "LocationMapping",
                "ToDataObject",
                serviceModel);

            Assert.Multiple(() =>
            {
                Assert.That(dataObject.Id, Is.EqualTo("Cluj-Napoca"));
                Assert.That(dataObject.GeoNamesId, Is.EqualTo("681290"));
                Assert.That(dataObject.GameIds.Single().Id, Is.EqualTo("c_cluj"));
                Assert.That(dataObject.FallbackLocations, Is.EqualTo(["Oradea", "Dezmir"]));
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

            LocationDataObject dataObject = InternalMappingInvoker.Invoke<LocationDataObject>(
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

            IEnumerable<LocationDataObject> dataObjects = InternalMappingInvoker.Invoke<IEnumerable<LocationDataObject>>(
                "LocationMapping",
                "ToDataObjects",
                serviceModels);

            Assert.That(dataObjects.Select(dataObject => dataObject.Id), Is.EqualTo(["Oradea", "Cluj-Napoca"]));
        }

        [Test]
        public void GivenADataObject_WhenMappingToAServiceModel_ThenNestedValuesArePreserved()
        {
            LocationDataObject dataObject = BuildDataObject("Cluj-Napoca", "c_cluj");

            Location serviceModel = InternalMappingInvoker.Invoke<Location>(
                "LocationMapping",
                "ToServiceModel",
                dataObject);

            Assert.Multiple(() =>
            {
                Assert.That(serviceModel.Id, Is.EqualTo("Cluj-Napoca"));
                Assert.That(serviceModel.GeoNamesId, Is.EqualTo("681290"));
                Assert.That(serviceModel.GameIds.Single().Id, Is.EqualTo("c_cluj"));
                Assert.That(serviceModel.FallbackLocations, Is.EqualTo(["Oradea", "Dezmir"]));
                Assert.That(serviceModel.Names.Single().Value, Is.EqualTo("Cluj-Napoca"));
            });
        }

        [Test]
        public void GivenMultipleDataObjects_WhenMappingToServiceModels_ThenOrderIsPreserved()
        {
            IEnumerable<LocationDataObject> dataObjects =
            [
                BuildDataObject("Oradea", "c_oradea"),
                BuildDataObject("Cluj-Napoca", "c_cluj")
            ];

            IEnumerable<Location> serviceModels = InternalMappingInvoker.Invoke<IEnumerable<Location>>(
                "LocationMapping",
                "ToServiceModels",
                dataObjects);

            Assert.That(serviceModels.Select(location => location.Id), Is.EqualTo(["Oradea", "Cluj-Napoca"]));
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

        private static LocationDataObject BuildDataObject(string locationId, string locationGameId)
            => new()
            {
                Id = locationId,
                GeoNamesId = "681290",
                GameIds = [new("CK3", locationGameId) { Type = "county" }],
                FallbackLocations = ["Oradea", "Dezmir"],
                Names = [new("Romanian", locationId)]
            };
    }
}
