using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;
using MoreCulturalNamesBuilder.UnitTests.TestInfrastructure;

namespace MoreCulturalNamesBuilder.UnitTests.Service.Mapping
{
    [TestFixture]
    public sealed class LanguageCodeMappingTests
    {
        [Test]
        public void GivenAServiceModel_WhenMappingToADataObject_ThenEveryCodeIsPreserved()
        {
            LanguageCode serviceModel = new()
            {
                ISO_639_1 = "ro",
                ISO_639_2 = "ron",
                ISO_639_3 = "ron"
            };

            LanguageCodeEntity dataObject = InternalMappingInvoker.Invoke<LanguageCodeEntity>(
                "LanguageCodeMapping",
                "ToDataObject",
                serviceModel);

            Assert.Multiple(() =>
            {
                Assert.That(dataObject.ISO_639_1, Is.EqualTo("ro"));
                Assert.That(dataObject.ISO_639_2, Is.EqualTo("ron"));
                Assert.That(dataObject.ISO_639_3, Is.EqualTo("ron"));
            });
        }

        [Test]
        public void GivenNullCodes_WhenMappingToADataObject_ThenNullCodesArePreserved()
        {
            LanguageCode serviceModel = new();

            LanguageCodeEntity dataObject = InternalMappingInvoker.Invoke<LanguageCodeEntity>(
                "LanguageCodeMapping",
                "ToDataObject",
                serviceModel);

            Assert.Multiple(() =>
            {
                Assert.That(dataObject.ISO_639_1, Is.Null);
                Assert.That(dataObject.ISO_639_2, Is.Null);
                Assert.That(dataObject.ISO_639_3, Is.Null);
            });
        }

        [Test]
        public void GivenMultipleDataObjects_WhenMappingToServiceModels_ThenOrderAndValuesArePreserved()
        {
            IEnumerable<LanguageCodeEntity> dataObjects =
            [
                new() { ISO_639_1 = "en", ISO_639_2 = "eng", ISO_639_3 = "eng" },
                new() { ISO_639_1 = "ro", ISO_639_2 = "ron", ISO_639_3 = "ron" }
            ];

            IEnumerable<LanguageCode> serviceModels = InternalMappingInvoker.Invoke<IEnumerable<LanguageCode>>(
                "LanguageCodeMapping",
                "ToServiceModels",
                dataObjects);

            Assert.That(serviceModels.Select(serviceModel => serviceModel.ISO_639_1), Is.EqualTo(new[] { "en", "ro" }));
        }

        [Test]
        public void GivenMultipleServiceModels_WhenMappingToDataObjects_ThenOrderAndValuesArePreserved()
        {
            IEnumerable<LanguageCode> serviceModels =
            [
                new() { ISO_639_1 = "en", ISO_639_2 = "eng", ISO_639_3 = "eng" },
                new() { ISO_639_1 = "ro", ISO_639_2 = "ron", ISO_639_3 = "ron" }
            ];

            IEnumerable<LanguageCodeEntity> dataObjects = InternalMappingInvoker.Invoke<IEnumerable<LanguageCodeEntity>>(
                "LanguageCodeMapping",
                "ToDataObjects",
                serviceModels);

            Assert.That(dataObjects.Select(dataObject => dataObject.ISO_639_1), Is.EqualTo(new[] { "en", "ro" }));
        }
    }
}