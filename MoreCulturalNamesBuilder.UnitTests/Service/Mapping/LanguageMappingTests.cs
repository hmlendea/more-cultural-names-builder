using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;
using MoreCulturalNamesBuilder.UnitTests.TestInfrastructure;

namespace MoreCulturalNamesBuilder.UnitTests.Service.Mapping
{
    [TestFixture]
    public sealed class LanguageMappingTests
    {
        [Test]
        public void GivenAServiceModelWithACode_WhenMappingToADataObject_ThenNestedValuesArePreserved()
        {
            Language serviceModel = BuildLanguage("Romanian", new() { ISO_639_1 = "ro" });

            LanguageEntity dataObject = InternalMappingInvoker.Invoke<LanguageEntity>(
                "LanguageMapping",
                "ToDataObject",
                serviceModel);

            Assert.Multiple(() =>
            {
                Assert.That(dataObject.Id, Is.EqualTo("Romanian"));
                Assert.That(dataObject.Code.ISO_639_1, Is.EqualTo("ro"));
                Assert.That(dataObject.GameIds.Single().Id, Is.EqualTo("romanian"));
                Assert.That(dataObject.FallbackLanguages, Is.EqualTo(new[] { "English", "French" }));
            });
        }

        [Test]
        public void GivenAServiceModelWithoutACode_WhenMappingToADataObject_ThenTheCodeRemainsNull()
        {
            Language serviceModel = BuildLanguage("Romanian", null);

            LanguageEntity dataObject = InternalMappingInvoker.Invoke<LanguageEntity>(
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

            IEnumerable<LanguageEntity> dataObjects = InternalMappingInvoker.Invoke<IEnumerable<LanguageEntity>>(
                "LanguageMapping",
                "ToDataObjects",
                serviceModels);

            Assert.That(dataObjects.Select(dataObject => dataObject.Id), Is.EqualTo(new[] { "English", "Romanian" }));
        }

        private static Language BuildLanguage(string languageId, LanguageCode code)
            => new()
            {
                Id = languageId,
                Code = code,
                GameIds = [new() { Game = "CK3", Id = languageId.ToLowerInvariant() }],
                FallbackLanguages = ["English", "French"]
            };
    }
}