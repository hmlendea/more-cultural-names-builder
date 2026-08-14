using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;
using MoreCulturalNamesBuilder.UnitTests.TestInfrastructure;

namespace MoreCulturalNamesBuilder.UnitTests.Service.Mapping
{
    [TestFixture]
    public sealed class GameIdMappingTests
    {
        [Test]
        public void GivenAServiceModel_WhenMappingToADataObject_ThenEveryValueIsPreserved()
        {
            GameId serviceModel = new()
            {
                Game = "CK3",
                Type = "county",
                Parent = "d_romania",
                DefaultNameLanguageId = "Romanian",
                Id = "c_cluj"
            };

            GameIdEntity dataObject = InternalMappingInvoker.Invoke<GameIdEntity>(
                "GameIdMapping",
                "ToDataObject",
                serviceModel);

            Assert.Multiple(() =>
            {
                Assert.That(dataObject.Game, Is.EqualTo("CK3"));
                Assert.That(dataObject.Type, Is.EqualTo("county"));
                Assert.That(dataObject.Parent, Is.EqualTo("d_romania"));
                Assert.That(dataObject.DefaultNameLanguageId, Is.EqualTo("Romanian"));
                Assert.That(dataObject.Id, Is.EqualTo("c_cluj"));
            });
        }

        [Test]
        public void GivenMultipleServiceModels_WhenMappingToDataObjects_ThenOrderAndValuesArePreserved()
        {
            IEnumerable<GameId> serviceModels =
            [
                new() { Game = "CK2", Id = "c_oradea" },
                new() { Game = "CK3", Id = "c_cluj" }
            ];

            IEnumerable<GameIdEntity> dataObjects = InternalMappingInvoker.Invoke<IEnumerable<GameIdEntity>>(
                "GameIdMapping",
                "ToDataObjects",
                serviceModels);

            Assert.That(dataObjects.Select(dataObject => dataObject.Id), Is.EqualTo(new[] { "c_oradea", "c_cluj" }));
        }

        [Test]
        public void GivenAnEmptyServiceModelSequence_WhenMappingToDataObjects_ThenAnEmptySequenceIsReturned()
        {
            IEnumerable<GameIdEntity> dataObjects = InternalMappingInvoker.Invoke<IEnumerable<GameIdEntity>>(
                "GameIdMapping",
                "ToDataObjects",
                new List<GameId>());

            Assert.That(dataObjects, Is.Empty);
        }
    }
}