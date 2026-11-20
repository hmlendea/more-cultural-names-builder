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
                        !tree.FilePath.EndsWith("InternalMappingInvoker.cs", StringComparison.Ordinal) &&
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

        [Test]
        public void TestFixturesMustBePublicSealedSubjectNamedClassesWithLifecycleNames()
        {
            List<string> violations = [];
            int selected = 0;

            foreach (SyntaxTree tree in RepositorySources.Trees.Where(tree => !RepositorySources.IsProduction(tree)))
            {
                foreach (ClassDeclarationSyntax fixture in tree.GetRoot().DescendantNodes()
                    .OfType<ClassDeclarationSyntax>())
                {
                    if (!fixture.Members.OfType<MethodDeclarationSyntax>().Any(method =>
                        HasAttribute(method.AttributeLists, "NUnit.Framework.TestAttribute") ||
                        HasAttribute(method.AttributeLists, "NUnit.Framework.TestCaseAttribute") ||
                        HasAttribute(method.AttributeLists, "NUnit.Framework.TestCaseSourceAttribute")))
                    {
                        continue;
                    }

                    selected += 1;

                    if (!fixture.Identifier.ValueText.EndsWith("Tests", StringComparison.Ordinal) ||
                        !fixture.Modifiers.Any(SyntaxKind.PublicKeyword) ||
                        !fixture.Modifiers.Any(SyntaxKind.SealedKeyword) ||
                        !HasAttribute(fixture.AttributeLists, "NUnit.Framework.TestFixtureAttribute"))
                    {
                        violations.Add(RepositorySources.Describe(fixture, "Expected a public sealed Tests fixture with TestFixture."));
                    }

                    foreach (MethodDeclarationSyntax method in fixture.Members.OfType<MethodDeclarationSyntax>())
                    {
                        if (HasAttribute(method.AttributeLists, "NUnit.Framework.SetUpAttribute") ||
                            HasAttribute(method.AttributeLists, "NUnit.Framework.OneTimeSetUpAttribute"))
                        {
                            if (method.Identifier.ValueText is not "SetUp")
                            {
                                violations.Add(RepositorySources.Describe(method, "Setup methods must be named SetUp."));
                            }
                        }

                        if (HasAttribute(method.AttributeLists, "NUnit.Framework.TearDownAttribute") ||
                            HasAttribute(method.AttributeLists, "NUnit.Framework.OneTimeTearDownAttribute"))
                        {
                            if (method.Identifier.ValueText is not "TearDown")
                            {
                                violations.Add(RepositorySources.Describe(method, "Teardown methods must be named TearDown."));
                            }
                        }
                    }
                }
            }

            Assert.That(selected, Is.GreaterThan(0), "No test fixtures were selected.");
            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void PrivateFixtureHelpersMustBeDeclaredAfterTheLastTest()
        {
            List<string> violations = [];
            int selected = 0;

            foreach (SyntaxTree tree in RepositorySources.Trees.Where(IsUnitOrIntegrationSource))
            {
                foreach (ClassDeclarationSyntax fixture in tree.GetRoot().DescendantNodes()
                    .OfType<ClassDeclarationSyntax>())
                {
                    MethodDeclarationSyntax[] tests = [.. fixture.Members.OfType<MethodDeclarationSyntax>()
                        .Where(method =>
                            HasAttribute(method.AttributeLists, "NUnit.Framework.TestAttribute") ||
                            HasAttribute(method.AttributeLists, "NUnit.Framework.TestCaseAttribute") ||
                            HasAttribute(method.AttributeLists, "NUnit.Framework.TestCaseSourceAttribute"))];

                    if (tests.Length == 0)
                    {
                        continue;
                    }

                    selected += 1;
                    int lastTestLine = tests.Max(method =>
                        method.SyntaxTree.GetLineSpan(method.Span).StartLinePosition.Line);

                    foreach (MethodDeclarationSyntax method in fixture.Members.OfType<MethodDeclarationSyntax>())
                    {
                        bool isTest = tests.Contains(method);

                        if (isTest || HasAttribute(method.AttributeLists, "NUnit.Framework.SetUpAttribute") ||
                            HasAttribute(method.AttributeLists, "NUnit.Framework.TearDownAttribute") ||
                            HasAttribute(method.AttributeLists, "NUnit.Framework.OneTimeSetUpAttribute") ||
                            HasAttribute(method.AttributeLists, "NUnit.Framework.OneTimeTearDownAttribute"))
                        {
                            continue;
                        }

                        if (method.SyntaxTree.GetLineSpan(method.Span).StartLinePosition.Line < lastTestLine)
                        {
                            violations.Add(RepositorySources.Describe(method, "Helpers must be declared at the bottom of the fixture."));
                        }
                    }
                }
            }

            Assert.That(selected, Is.GreaterThan(0), "No test fixtures were selected.");
            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void UnitTestMethodsMustBePublicVoidAndAssertOutcomes()
        {
            List<string> violations = [];
            int selected = 0;

            foreach (SyntaxTree tree in RepositorySources.Trees.Where(tree =>
                string.Equals(RepositorySources.GetProjectName(tree.FilePath),
                    "MoreCulturalNamesBuilder.UnitTests", StringComparison.Ordinal)))
            {
                foreach (ClassDeclarationSyntax fixture in tree.GetRoot().DescendantNodes()
                    .OfType<ClassDeclarationSyntax>())
                {
                    foreach (MethodDeclarationSyntax method in fixture.Members.OfType<MethodDeclarationSyntax>())
                    {
                        if (!HasAttribute(method.AttributeLists, "NUnit.Framework.TestAttribute") &&
                            !HasAttribute(method.AttributeLists, "NUnit.Framework.TestCaseAttribute") &&
                            !HasAttribute(method.AttributeLists, "NUnit.Framework.TestCaseSourceAttribute"))
                        {
                            continue;
                        }

                        selected += 1;

                        if (!method.Modifiers.Any(SyntaxKind.PublicKeyword) ||
                            method.Modifiers.Any(SyntaxKind.StaticKeyword) ||
                            method.Modifiers.Any(SyntaxKind.AsyncKeyword) ||
                            !ReturnsVoid(method))
                        {
                            violations.Add(RepositorySources.Describe(method, "Test methods must be public void instance methods."));
                        }

                        if (!AssertsAnOutcome(fixture, method))
                        {
                            violations.Add(RepositorySources.Describe(method, "Business unit tests require an assertion or Moq verification."));
                        }
                    }
                }
            }

            Assert.That(selected, Is.GreaterThan(0), "No unit tests were selected.");
            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        private static bool AssertsAnOutcome(ClassDeclarationSyntax fixture, MethodDeclarationSyntax method)
        {
            HashSet<string> visited = [];
            Queue<MethodDeclarationSyntax> pending = new();
            pending.Enqueue(method);

            while (pending.Count > 0)
            {
                MethodDeclarationSyntax current = pending.Dequeue();

                if (!visited.Add(current.Identifier.ValueText))
                {
                    continue;
                }

                if (DeclaresAssertion(current))
                {
                    return true;
                }

                foreach (InvocationExpressionSyntax invocation in current.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>())
                {
                    if (invocation.Expression is not IdentifierNameSyntax identifier)
                    {
                        continue;
                    }

                    MethodDeclarationSyntax? helper = fixture.Members.OfType<MethodDeclarationSyntax>()
                        .FirstOrDefault(candidate => string.Equals(candidate.Identifier.ValueText,
                            identifier.Identifier.ValueText, StringComparison.Ordinal));

                    if (helper is not null)
                    {
                        pending.Enqueue(helper);
                    }
                }
            }

            return false;
        }

        private static bool DeclaresAssertion(MethodDeclarationSyntax method) =>
            method.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Any(access =>
                access.Name.Identifier.ValueText is "That" or "Multiple" or "EnterMultipleScope" or "Verify" or "Pass") ||
            method.AttributeLists.SelectMany(list => list.Attributes).Any(attribute =>
                attribute.ArgumentList?.Arguments.Any(argument =>
                    string.Equals(argument.NameEquals?.Name.Identifier.ValueText, "ExpectedResult", StringComparison.Ordinal)) is true);

        private static bool ReturnsVoid(MethodDeclarationSyntax method) =>
            method.ReturnType is PredefinedTypeSyntax predefined && predefined.Keyword.IsKind(SyntaxKind.VoidKeyword);

        private static bool IsUnitOrIntegrationSource(SyntaxTree tree)
        {
            string projectName = RepositorySources.GetProjectName(tree.FilePath);

            return string.Equals(projectName, "MoreCulturalNamesBuilder.UnitTests", StringComparison.Ordinal) ||
                string.Equals(projectName, "MoreCulturalNamesBuilder.IntegrationTests", StringComparison.Ordinal);
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
