using MoreCulturalNamesBuilder.Service.Models;
using MoreCulturalNamesBuilder.UnitTests.TestInfrastructure;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.UnitTests.Service.Mapping
{
    [TestFixture]
    public sealed class CachedLocalisationMappingTests
    {
        [TestCase("Romanian", "Cluj-Napoca", "Clujean", "Praise the Sun!")]
        [TestCase("English", "Newport", null, null)]
        public void GivenACachedLocalisation_WhenMappingToAServiceModel_ThenTheCachedValuesArePreserved(
            string languageId,
            string name,
            string? adjective,
            string? comment)
        {
            CachedLocalisation cachedLocalisation = new(
                true,
                "Cluj-Napoca",
                languageId,
                name,
                adjective,
                comment);

            Localisation? localisation = InternalMappingInvoker.Invoke<Localisation?>(
                "CachedLocalisationMapping",
                "ToServiceModel",
                cachedLocalisation);

            Assert.Multiple(() =>
            {
                Assert.That(localisation, Is.Not.Null);
                Assert.That(localisation!.Id, Is.EqualTo("Cluj-Napoca"));
                Assert.That(localisation.LanguageId, Is.EqualTo(languageId));
                Assert.That(localisation.Name, Is.EqualTo(name));
                Assert.That(localisation.Adjective, Is.EqualTo(adjective));
                Assert.That(localisation.Comment, Is.EqualTo(comment));
            });
        }

        [Test]
        public void GivenACachedLocalisationWithoutAValue_WhenMappingToAServiceModel_ThenNullIsReturned()
        {
            CachedLocalisation cachedLocalisation = new(false, null, null, null, null, null);

            Localisation? localisation = InternalMappingInvoker.Invoke<Localisation?>(
                "CachedLocalisationMapping",
                "ToServiceModel",
                cachedLocalisation);

            Assert.That(localisation, Is.Null);
        }

        [TestCase("Praise the Sun!")]
        [TestCase(null)]
        public void GivenACachedCommentValue_WhenMappingToAServiceModel_ThenTheCommentIsPreserved(
            string? comment)
        {
            CachedLocalisation cachedLocalisation = new(
                true,
                "Florești",
                "Romanian",
                "Florești",
                null,
                comment);

            Localisation? localisation = InternalMappingInvoker.Invoke<Localisation?>(
                "CachedLocalisationMapping",
                "ToServiceModel",
                cachedLocalisation);

            Assert.That(localisation!.Comment, Is.EqualTo(comment));
        }
    }
}
