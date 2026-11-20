using System.Collections.Generic;
using System.Linq;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;
using MoreCulturalNamesBuilder.UnitTests.TestInfrastructure;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.UnitTests.Service.Mapping
{
    [TestFixture]
    public sealed class LanguageMappingTests
    {
        [Test]
        public void GivenAServiceModelWithACode_WhenMappingToADataObject_ThenNestedValuesArePreserved()
        {
            Language serviceModel = BuildLanguage("Romanian", new() { ISO_639_1 = "ro" });

            LanguageDataObject dataObject = InternalMappingInvoker.Invoke<LanguageDataObject>(
                "LanguageMapping",
                "ToDataObject",
                serviceModel);

            Assert.Multiple(() =>
            {
                Assert.That(dataObject.Id, Is.EqualTo("Romanian"));
                Assert.That(dataObject.Code!.ISO_639_1, Is.EqualTo("ro"));
                Assert.That(dataObject.GameIds.Single().Id, Is.EqualTo("romanian"));
                Assert.That(dataObject.FallbackLanguages, Is.EqualTo(["English", "French"]));
            });
        }

        [Test]
        public void GivenAServiceModelWithoutACode_WhenMappingToADataObject_ThenTheCodeRemainsNull()
        {
            Language serviceModel = BuildLanguage("Romanian", null);

            LanguageDataObject dataObject = InternalMappingInvoker.Invoke<LanguageDataObject>(
                "LanguageMapping",
                "ToDataObject",
                serviceModel);

            Assert.That(dataObject.Code, Is.Null);
        }

        [Test]
        public void GivenMultipleServiceModels_WhenMappingToDataObjects_ThenOrderIsPreserved()
        {
            IEnumerable<Language> serviceModels =
            [
                BuildLanguage("English", new() { ISO_639_1 = "en" }),
                BuildLanguage("Romanian", new() { ISO_639_1 = "ro" })
            ];

            IEnumerable<LanguageDataObject> dataObjects = InternalMappingInvoker.Invoke<IEnumerable<LanguageDataObject>>(
                "LanguageMapping",
                "ToDataObjects",
                serviceModels);

            Assert.That(dataObjects.Select(dataObject => dataObject.Id), Is.EqualTo(["English", "Romanian"]));
        }

        [Test]
        public void GivenADataObjectWithACode_WhenMappingToAServiceModel_ThenNestedValuesArePreserved()
        {
            LanguageDataObject dataObject = BuildDataObject(
                "Romanian",
                new() { ISO_639_1 = "ro", ISO_639_2 = "ron", ISO_639_3 = "ron" });

            Language serviceModel = InternalMappingInvoker.Invoke<Language>(
                "LanguageMapping",
                "ToServiceModel",
                dataObject);

            Assert.Multiple(() =>
            {
                Assert.That(serviceModel.Id, Is.EqualTo("Romanian"));
                Assert.That(serviceModel.Code!.ISO_639_1, Is.EqualTo("ro"));
                Assert.That(serviceModel.GameIds.Single().Id, Is.EqualTo("romanian"));
                Assert.That(serviceModel.FallbackLanguages, Is.EqualTo(["English", "French"]));
            });
        }

        [Test]
        public void GivenADataObjectWithoutACode_WhenMappingToAServiceModel_ThenTheCodeRemainsNull()
        {
            LanguageDataObject dataObject = BuildDataObject("Romanian", null);

            Language serviceModel = InternalMappingInvoker.Invoke<Language>(
                "LanguageMapping",
                "ToServiceModel",
                dataObject);

            Assert.That(serviceModel.Code, Is.Null);
        }

        [Test]
        public void GivenMultipleDataObjects_WhenMappingToServiceModels_ThenOrderIsPreserved()
        {
            IEnumerable<LanguageDataObject> dataObjects =
            [
                BuildDataObject("English", new() { ISO_639_1 = "en" }),
                BuildDataObject("Romanian", new() { ISO_639_1 = "ro" })
            ];

            IEnumerable<Language> serviceModels = InternalMappingInvoker.Invoke<IEnumerable<Language>>(
                "LanguageMapping",
                "ToServiceModels",
                dataObjects);

            Assert.That(serviceModels.Select(language => language.Id), Is.EqualTo(["English", "Romanian"]));
        }

        private static Language BuildLanguage(string languageId, LanguageCode? code)
            => new()
            {
                Id = languageId,
                Code = code,
                GameIds = [new() { Game = "CK3", Id = languageId.ToLowerInvariant() }],
                FallbackLanguages = ["English", "French"]
            };

        private static LanguageDataObject BuildDataObject(string languageId, LanguageCodeDataObject? code)
            => new()
            {
                Id = languageId,
                Code = code,
                GameIds = [new("CK3", languageId.ToLowerInvariant())],
                FallbackLanguages = ["English", "French"]
            };
    }
}
