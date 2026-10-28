using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.ArchitectureTests
{
    [TestFixture]
    public sealed class TestSourceConventionTests
    {
        [Test]
        public void TestFixturesMustHaveNUnitAttributesAndSubjectBasedNames()
        {
            ClassDeclarationSyntax[] fixtures = GetTestMethods()
                .Select(method => method.Parent).OfType<ClassDeclarationSyntax>().Distinct().ToArray();
            Assert.That(fixtures, Is.Not.Empty, "No NUnit fixtures were selected.");
            List<string> violations = [];

            foreach (ClassDeclarationSyntax fixture in fixtures)
            {
                if (!fixture.Identifier.ValueText.EndsWith("Tests", StringComparison.Ordinal) ||
                    !HasAttribute(fixture.AttributeLists, "NUnit.Framework.TestFixtureAttribute"))
                {
                    violations.Add(RepositorySources.Describe(fixture, "Expected a subject-based Tests fixture with TestFixture."));
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void UnitTestsMustUseGivenWhenThenNamesAndAtLeastTwoParameterisedCases()
        {
            MethodDeclarationSyntax[] methods = GetTestMethods()
                .Where(method => string.Equals(RepositorySources.GetProjectName(method.SyntaxTree.FilePath),
                    "MoreCulturalNamesBuilder.UnitTests")).ToArray();
            Assert.That(methods, Is.Not.Empty, "No unit tests were selected.");
            List<string> violations = [];

            foreach (MethodDeclarationSyntax method in methods)
            {
                string[] sections = method.Identifier.ValueText.Split('_');
                int cases = method.AttributeLists.SelectMany(list => list.Attributes)
                    .Count(attribute => IsAttribute(attribute, "NUnit.Framework.TestCaseAttribute"));

                if (sections.Length != 3 || !sections[0].StartsWith("Given", StringComparison.Ordinal) ||
                    !sections[1].StartsWith("When", StringComparison.Ordinal) ||
                    !sections[2].StartsWith("Then", StringComparison.Ordinal) || cases == 1)
                {
                    violations.Add(RepositorySources.Describe(method, "Expected Given..._When..._Then... and at least two TestCases."));
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void UnitTestsMustNotInvokeNonPublicMembersThroughReflection()
        {
            List<string> violations = [];

            foreach (SyntaxTree tree in RepositorySources.Trees.Where(tree =>
                string.Equals(RepositorySources.GetProjectName(tree.FilePath), "MoreCulturalNamesBuilder.UnitTests")))
            {
                SemanticModel model = RepositorySources.GetSemanticModel(tree);

                foreach (MemberAccessExpressionSyntax access in tree.GetRoot().DescendantNodes()
                    .OfType<MemberAccessExpressionSyntax>())
                {
                    ISymbol? symbol = model.GetSymbolInfo(access).Symbol;

                    if (symbol is IFieldSymbol field && field.Name is "NonPublic" &&
                        field.ContainingType.ToDisplayString() is "System.Reflection.BindingFlags")
                    {
                        violations.Add(RepositorySources.Describe(access, "Unit tests must exercise public contracts, not NonPublic reflection."));
                    }
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void NUnitAssertionsMustUseTheConstraintModel()
        {
            List<string> violations = [];

            foreach (MethodDeclarationSyntax method in GetTestMethods())
            {
                SemanticModel model = RepositorySources.GetSemanticModel(method.SyntaxTree);

                foreach (InvocationExpressionSyntax invocation in method.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    IMethodSymbol? symbol = model.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

                    if (symbol?.ContainingType.ToDisplayString() is "NUnit.Framework.Assert" &&
                        symbol.Name is not "That" and not "EnterMultipleScope" and not "Multiple" and
                        not "Fail" and not "Ignore" and not "Inconclusive" and not "Pass" and not "Warn")
                    {
                        violations.Add(RepositorySources.Describe(invocation, "Use NUnit Assert.That constraints."));
                    }
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        private static IEnumerable<MethodDeclarationSyntax> GetTestMethods() =>
            RepositorySources.Trees.Where(tree => !RepositorySources.IsProduction(tree))
                .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                .Where(method => HasAttribute(method.AttributeLists, "NUnit.Framework.TestAttribute") ||
                    HasAttribute(method.AttributeLists, "NUnit.Framework.TestCaseAttribute") ||
                    HasAttribute(method.AttributeLists, "NUnit.Framework.TestCaseSourceAttribute"));

        private static bool HasAttribute(IEnumerable<AttributeListSyntax> lists, string metadataName) =>
            lists.SelectMany(list => list.Attributes).Any(attribute => IsAttribute(attribute, metadataName));

        private static bool IsAttribute(AttributeSyntax attribute, string metadataName) =>
            string.Equals(RepositorySources.GetSemanticModel(attribute.SyntaxTree).GetSymbolInfo(attribute)
                .Symbol?.ContainingType.ToDisplayString(), metadataName, StringComparison.Ordinal);
    }
}
