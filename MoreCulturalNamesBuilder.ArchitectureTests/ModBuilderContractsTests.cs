using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.Service.ModBuilders;

namespace MoreCulturalNamesBuilder.ArchitectureTests
{
    [TestFixture]
    public sealed class ModBuilderContractsTests
    {
        private static readonly string[] gameIdentifiers =
        [
            "CK2",
            "CK3",
            "Civ5",
            "Civilization5",
            "EU5",
            "EuropaUniversalis5",
            "HOI4",
            "IR",
            "ImperatorRome",
            "Vic3",
            "Victoria3"
        ];

        [Test]
        public void EveryGameBuilderMustBeReachableThroughTheFactoryAndTheFactoryMustNotExposeAnythingElse()
        {
            ModBuilderFactory factory = new(null!, null!, null!, null!);
            List<Type> resolvedTypes = [];

            foreach (string game in gameIdentifiers)
            {
                Settings settings = CreateSettings(game);
                IModBuilder modBuilder = factory.GetModBuilder(settings);

                resolvedTypes.Add(modBuilder.GetType());
            }

            Type[] inventory = typeof(ModBuilder).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract &&
                    typeof(IModBuilder).IsAssignableFrom(type) &&
                    type.Namespace is not null)
                .ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(inventory, Is.Not.Empty, "The game-builder inventory must not be empty.");
                Assert.That(resolvedTypes.Distinct(), Is.EquivalentTo(inventory),
                    "Every game builder must be reachable through the factory identifiers.");
            });
        }

        [Test]
        public void GameBuildersMustOnlyBeConstructedByTheFactory()
        {
            Type[] inventory = typeof(ModBuilder).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract &&
                    typeof(IModBuilder).IsAssignableFrom(type))
                .ToArray();
            List<string> violations = [];
            int selected = 0;

            foreach (SyntaxTree tree in RepositorySources.Trees.Where(RepositorySources.IsProduction))
            {
                SemanticModel model = RepositorySources.GetSemanticModel(tree);

                foreach (ObjectCreationExpressionSyntax creation in tree.GetRoot().DescendantNodes()
                    .OfType<ObjectCreationExpressionSyntax>())
                {
                    ITypeSymbol? createdType = model.GetTypeInfo(creation).Type;

                    if (createdType is null || !inventory.Any(type => string.Equals(
                        type.FullName,
                        createdType.ToDisplayString(),
                        StringComparison.Ordinal)))
                    {
                        continue;
                    }

                    selected += 1;
                    string? owner = creation.Ancestors().OfType<ClassDeclarationSyntax>()
                        .FirstOrDefault()?.Identifier.ValueText;

                    if (owner is not "ModBuilderFactory")
                    {
                        violations.Add(RepositorySources.Describe(creation,
                            "Game builders must only be constructed by ModBuilderFactory."));
                    }
                }
            }

            Assert.That(selected, Is.GreaterThan(0), "No game-builder construction was selected.");
            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void EveryGameBuilderMustDeclareItsOwnFilePipelineInsteadOfInheritingAnotherBuilder()
        {
            Type[] inheritanceChainViolations = typeof(ModBuilder).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract &&
                    typeof(IModBuilder).IsAssignableFrom(type) &&
                    type.BaseType is not null &&
                    !string.Equals(type.BaseType.Name, "ModBuilder", StringComparison.Ordinal) &&
                    !string.Equals(type.Name, "CK3ModBuilder", StringComparison.Ordinal))
                .ToArray();

            Assert.That(inheritanceChainViolations, Is.Empty,
                "Only CK3 may specialise another builder; every other builder must derive from ModBuilder.");
        }

        private static Settings CreateSettings(string game)
            => new(
            [
                "--lang", "languages.xml",
                "--loc", "locations.xml",
                "--output", "mods",
                "--id", "more-cultural-names",
                "--name", "More Cultural Names",
                "--version", "1.0.0",
                "--game", game,
                "--game-version", "1.12.*"
            ]);
    }
}
