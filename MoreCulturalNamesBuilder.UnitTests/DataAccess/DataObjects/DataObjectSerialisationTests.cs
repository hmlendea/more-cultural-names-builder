using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

using NUnit.Framework;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;

namespace MoreCulturalNamesBuilder.UnitTests.DataAccess.DataObjects
{
    [TestFixture]
    public sealed class DataObjectSerialisationTests
    {
        [TestCase(typeof(GameIdDataObject), "GameId")]
        [TestCase(typeof(LanguageCodeDataObject), "LanguageCode")]
        [TestCase(typeof(LanguageDataObject), "Language")]
        [TestCase(typeof(LocationDataObject), "LocationEntity")]
        [TestCase(typeof(NameDataObject), "Name")]
        public void GivenARenamedDataObject_WhenSerialising_ThenTheOriginalXmlTypeNameIsPreserved(
            Type dataObjectType,
            string expectedElementName)
        {
            XmlSerializer serialiser = new(dataObjectType);
            using StringWriter writer = new();
            serialiser.Serialize(writer, Activator.CreateInstance(dataObjectType));
            XDocument document = XDocument.Parse(writer.ToString());

            Assert.That(
                document.Root!.Name.LocalName,
                Is.EqualTo(expectedElementName));
        }

        [Test]
        public void GivenAnExistingLocationDocument_WhenDeserialising_ThenIdentifiersAndNamesArePreserved()
        {
            XmlSerializer serialiser = new(typeof(LocationDataObject));
            using StringReader reader = new(
                "<LocationEntity id=\"Cluj-Napoca\">" +
                "<GameIds><GameId game=\"CK3\" type=\"county\">c_cluj</GameId></GameIds>" +
                "<FallbackLocations><LocationId>Dezmir</LocationId></FallbackLocations>" +
                "<Names><Name language=\"Romanian\" value=\"Cluj-Napoca\" /></Names>" +
                "</LocationEntity>");
            LocationDataObject location = (LocationDataObject)serialiser.Deserialize(reader)!;

            Assert.Multiple(() =>
            {
                Assert.That(location.GameIds, Has.Count.EqualTo(1));
                Assert.That(location.GameIds[0].Id, Is.EqualTo("c_cluj"));
                Assert.That(location.FallbackLocations, Is.EqualTo(new[] { "Dezmir" }));
                Assert.That(location.Names, Has.Count.EqualTo(1));
                Assert.That(location.Names[0].Value, Is.EqualTo("Cluj-Napoca"));
            });
        }
    }
}
