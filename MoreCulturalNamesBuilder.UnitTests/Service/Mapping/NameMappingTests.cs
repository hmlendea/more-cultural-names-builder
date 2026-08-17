using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;
using MoreCulturalNamesBuilder.UnitTests.TestInfrastructure;

namespace MoreCulturalNamesBuilder.UnitTests.Service.Mapping
{
    [TestFixture]
    public sealed class NameMappingTests
    {
        [Test]
        public void GivenAServiceModel_WhenMappingToADataObject_ThenEveryValueIsPreserved()
        {
            Name serviceModel = BuildName("Romanian", "Cluj-Napoca");

            NameEntity dataObject = InternalMappingInvoker.Invoke<NameEntity>(
                "NameMapping",
                "ToDataObject",
                serviceModel);

            Assert.Multiple(() =>
            {
                Assert.That(dataObject.LanguageId, Is.EqualTo("Romanian"));
                Assert.That(dataObject.Value, Is.EqualTo("Cluj-Napoca"));
                Assert.That(dataObject.Adjective, Is.EqualTo("Clujean"));
                Assert.That(dataObject.Comment, Is.EqualTo("Praise the Sun!"));
            });
        }

        [Test]
        public void GivenNullOptionalValues_WhenMappingToADataObject_ThenNullValuesArePreserved()
        {
            Name serviceModel = new()
            {
                LanguageId = "Romanian",
                Value = "Cluj-Napoca",
                Adjective = null,
                Comment = null
            };

            NameEntity dataObject = InternalMappingInvoker.Invoke<NameEntity>(
                "NameMapping",
                "ToDataObject",
                serviceModel);

            Assert.Multiple(() =>
            {
                Assert.That(dataObject.Adjective, Is.Null);
                Assert.That(dataObject.Comment, Is.Null);
            });
        }

        [Test]
        public void GivenMultipleServiceModels_WhenMappingToDataObjects_ThenOrderAndValuesArePreserved()
        {
            IEnumerable<Name> serviceModels =
            [
                BuildName("English", "Newport"),
                BuildName("Romanian", "Cluj-Napoca")
            ];

            IEnumerable<NameEntity> dataObjects = InternalMappingInvoker.Invoke<IEnumerable<NameEntity>>(
                "NameMapping",
                "ToDataObjects",
                serviceModels);

            Assert.That(dataObjects.Select(dataObject => dataObject.Value), Is.EqualTo(new[] { "Newport", "Cluj-Napoca" }));
        }

        private static Name BuildName(string languageId, string value)
            => new()
            {
                LanguageId = languageId,
                Value = value,
                Adjective = "Clujean",
                Comment = "Praise the Sun!"
            };
    }
}