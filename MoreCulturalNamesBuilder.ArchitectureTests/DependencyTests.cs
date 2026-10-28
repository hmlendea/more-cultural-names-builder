using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;

using NUnit.Framework;

using MoreCulturalNamesBuilder.Service.ModBuilders;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

using Assembly = System.Reflection.Assembly;
using Type = System.Type;

namespace MoreCulturalNamesBuilder.ArchitectureTests
{
    [TestFixture]
    public sealed class DependencyTests
    {
        private static readonly Assembly[] productionAssemblies = [typeof(Program).Assembly];

        private static readonly Architecture architecture = new ArchLoader()
            .LoadAssemblies(productionAssemblies).Build();

        [TestCase("Configuration", "DataAccess")]
        [TestCase("Configuration", "Service")]
        [TestCase("DataAccess", "Configuration")]
        [TestCase("DataAccess", "Service")]
        [TestCase("Service", "Configuration")]
        [TestCase("Service.Models", "DataAccess")]
        [TestCase("Service.Models", "Service.Mapping")]
        [TestCase("Service.Models", "Service.ModBuilders")]
        [TestCase("Service.Mapping", "Service.ModBuilders")]
        public void LayersMustRespectDocumentedDependencyDirection(string source, string target)
        {
            AssertNamespaceExists(source);
            AssertNamespaceExists(target);
            Types().That().ResideInNamespaceOrBelow($"MoreCulturalNamesBuilder.{source}")
                .Should().NotDependOnAny(Types().That()
                    .ResideInNamespaceOrBelow($"MoreCulturalNamesBuilder.{target}"))
                .Because($"{source} must not depend on {target}; see ARCHITECTURE.md")
                .Check(architecture);
        }

        [TestCase("Service")]
        [TestCase("Service.Mapping")]
        public void ServicesMustNotDependOnBuilderImplementations(string source)
        {
            AssertNamespaceExists(source);
            AssertNamespaceExists("Service.ModBuilders");
            Types().That().ResideInNamespace($"MoreCulturalNamesBuilder.{source}")
                .Should().NotDependOnAny(Types().That()
                    .ResideInNamespaceOrBelow("MoreCulturalNamesBuilder.Service.ModBuilders"))
                .Because("Services must remain independent of the builder layer")
                .Check(architecture);
        }

        [Test]
        public void MappingMustNotDependOnServiceImplementations()
        {
            AssertNamespaceExists("Service.Mapping");
            AssertNamespaceExists("Service");
            Types().That().ResideInNamespace("MoreCulturalNamesBuilder.Service.Mapping")
            .Should().NotDependOnAny(Types().That().ResideInNamespace("MoreCulturalNamesBuilder.Service"))
            .Because("Services may use mappings, but mappings must not create a reverse dependency cycle")
            .Check(architecture);
        }

        [Test]
        public void ModelsMustNotDependOnServiceImplementations()
        {
            AssertNamespaceExists("Service.Models");
            AssertNamespaceExists("Service");
            Types().That().ResideInNamespace("MoreCulturalNamesBuilder.Service.Models")
            .Should().NotDependOnAny(Types().That().ResideInNamespace("MoreCulturalNamesBuilder.Service"))
            .Because("Models must not depend on services that consume them")
            .Check(architecture);
        }

        [TestCase(typeof(CK2ModBuilder))]
        [TestCase(typeof(CK3ModBuilder))]
        [TestCase(typeof(HOI4ModBuilder))]
        [TestCase(typeof(ImperatorRomeModBuilder))]
        public void GameBuildersMustNotDependOnOtherGameBuilders(Type builder)
        {
            Type[] others = typeof(ModBuilder).Assembly.GetTypes()
                .Where(type => type != builder && !type.IsAbstract && type.IsSubclassOf(typeof(ModBuilder)))
                .ToArray();
            Assert.That(others, Is.Not.Empty, "The independent game-builder inventory must not be empty.");

            foreach (Type other in others)
            {
                Types().That().Are(builder).Should().NotDependOnAny(Types().That().Are(other))
                    .Because("Game builders are independent; only ModBuilderFactory selects implementations")
                    .Check(architecture);
            }
        }

        [Test]
        public void AllGameBuildersMustInheritTheSharedContract()
        {
            Type[] builders = productionAssemblies.SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass && !type.IsAbstract &&
                    typeof(IModBuilder).IsAssignableFrom(type)).ToArray();
            Assert.That(builders, Has.Length.EqualTo(4), "Update the explicit game inventory when adding a builder.");

            foreach (Type builder in builders)
            {
                Assert.That(builder.IsSubclassOf(typeof(ModBuilder)), $"{builder} must inherit ModBuilder.");
            }
        }

        [Test]
        public void ProductionAssembliesMustNotExposeInternalsToTests()
        {
            Assert.That(productionAssemblies, Is.Not.Empty);

            foreach (Assembly assembly in productionAssemblies)
            {
                Assert.That(assembly.GetCustomAttributes<InternalsVisibleToAttribute>(), Is.Empty,
                    $"{assembly.GetName().Name} must not declare InternalsVisibleTo.");
                Assert.That(assembly.GetReferencedAssemblies().Select(reference => reference.Name),
                    Has.None.Contains("Tests"), "Production assemblies must not reference test assemblies.");
            }
        }

        private static void AssertNamespaceExists(string suffix) =>
            Assert.That(productionAssemblies.SelectMany(assembly => assembly.GetTypes())
                .Any(type => type.Namespace is not null &&
                    (string.Equals(type.Namespace, $"MoreCulturalNamesBuilder.{suffix}") ||
                    type.Namespace.StartsWith($"MoreCulturalNamesBuilder.{suffix}.", StringComparison.Ordinal))),
                $"No production types selected for {suffix}; a boundary must not pass vacuously.");
    }
}
