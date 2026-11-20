using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.ArchitectureTests
{
    [TestFixture]
    public sealed class ConventionDetectorTests
    {
        [TestCase("ExplicitTypes", "public class Sample { public void M() { var value = 1; } }")]
        [TestCase("NoConditionalExpressions", "public class Sample { public int M(bool flag) => flag ? 1 : 2; }")]
        [TestCase("NoOptionalParameters", "public class Sample { public void M(int value = 1) { } }")]
        [TestCase("SevenParameterLimit", "public class Sample { public void M(int a, int b, int c, int d, int e, int f, int g, int h) { } }")]
        [TestCase("BracedControlFlow", "public class Sample { public void M(bool flag) { if (flag) return; } }")]
        [TestCase("NoNullCoalescingOrEquality", "public class Sample { public string M(string value) => value ?? \"\"; }")]
        [TestCase("NoConstants", "public class Sample { private const int Value = 1; }")]
        [TestCase("NoTopLevelStatements", "int value = 1;")]
        [TestCase("TargetTypedConstruction", "public class Sample { public void M() { Sample sample = new Sample(); } }")]
        [TestCase("NoIncrementOrDecrement", "public class Sample { public void M(int value) { value++; } }")]
        [TestCase("NoRedundantDefaults", "public class Sample { private bool flag = false; }")]
        [TestCase("CompoundAssignments", "public class Sample { public void M(int value) { value = value + 1; } }")]
        [TestCase("ElseIfOnOneLine", "public class Sample { public void M(int value) { if (value == 1) { } else\nif (value == 2) { } } }")]
        [TestCase("NoStringEqualityOperator", "public class Sample { public bool M(string a, string b) => a == b; }")]
        [TestCase("SingleCharacterStringCalls", "public class Sample { public bool M(string value) => value.StartsWith(\"x\"); }")]
        [TestCase("NoGlobalUsings", "global using System;")]
        [TestCase("CultureInvariantDateTime", "public class Sample { public string M() => System.DateTime.Parse(\"2026-08-05\").ToString(); }")]
        public void ConventionsMustDetectTheirViolations(string convention, string source)
        {
            SyntaxTree tree = RepositorySources.RegisterSnippet($"{convention}.cs", source);
            bool detected = tree.GetRoot().DescendantNodes(descendIntoTrivia: true)
                .Any(node => SourceFileConventionTests.ViolatesConvention(
                    node, convention, RepositorySources.GetSemanticModel));

            Assert.That(detected, Is.True, $"{convention} failed to detect its documented violation.");
        }

        [TestCase("ExplicitTypes", "public class Sample { public void M() { int value = 1; } }")]
        [TestCase("NoConditionalExpressions", "public class Sample { public int M(bool flag) { if (flag) { return 1; } return 2; } }")]
        [TestCase("NoNullCoalescingOrEquality", "public class Sample { public string M(string value) { if (value is null) { return \"\"; } return value; } }")]
        [TestCase("TargetTypedConstruction", "public class Sample { public void M() { Sample sample = new(); } }")]
        [TestCase("NoRedundantDefaults", "public class Sample { private int count; }")]
        [TestCase("CompoundAssignments", "public class Sample { public void M(int value) { value += 1; } }")]
        [TestCase("NoStringEqualityOperator", "public class Sample { public bool M(string a, string b) => string.Equals(a, b); }")]
        [TestCase("SingleCharacterStringCalls", "public class Sample { public bool M(string value) => value.StartsWith('x'); }")]
        [TestCase("CultureInvariantDateTime", "public class Sample { public string M() => System.DateTime.Parse(\"2026-08-05\", System.Globalization.CultureInfo.InvariantCulture).ToString(System.Globalization.CultureInfo.InvariantCulture); }")]
        public void ConventionsMustNotReportCompliantCode(string convention, string source)
        {
            SyntaxTree tree = RepositorySources.RegisterSnippet($"{convention}-clean.cs", source);
            bool detected = tree.GetRoot().DescendantNodes(descendIntoTrivia: true)
                .Any(node => SourceFileConventionTests.ViolatesConvention(
                    node, convention, RepositorySources.GetSemanticModel));

            Assert.That(detected, Is.False, $"{convention} produced a false positive on compliant code.");
        }

        [Test]
        public void SemanticConventionsMustDetectTheirViolations()
        {
            string source =
                "using System;\n" +
                "using System.Collections.Generic;\n" +
                "namespace Snippet\n" +
                "{\n" +
                "    public sealed class Sample : IEquatable<Sample>\n" +
                "    {\n" +
                "        public List<string> Values { get; set; } = new();\n" +
                "\n" +
                "        public bool Equals(Sample? other) => other is not null;\n" +
                "    }\n" +
                "}";
            SyntaxTree tree = RepositorySources.RegisterSnippet("SemanticSnippet.cs", source);
            SemanticModel model = RepositorySources.GetSemanticModel(tree);
            ClassDeclarationSyntax declaration = tree.GetRoot().DescendantNodes()
                .OfType<ClassDeclarationSyntax>().Single();
            INamedTypeSymbol symbol = (INamedTypeSymbol)model.GetDeclaredSymbol(declaration)!;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Collect("EquatableContract", declaration, symbol, model), Is.Not.Empty);
                Assert.That(Collect("EnumerableCollectionContracts", declaration, symbol, model), Is.Not.Empty);
            }
        }

        [Test]
        public void SemanticConventionsMustAcceptCompliantCode()
        {
            string source =
                "using System;\n" +
                "using System.Collections.Generic;\n" +
                "namespace Snippet\n" +
                "{\n" +
                "    public sealed class Sample : IEquatable<Sample>\n" +
                "    {\n" +
                "        public IEnumerable<string> Values { get; set; } = [];\n" +
                "\n" +
                "        public bool Equals(Sample? other) => other is not null;\n" +
                "\n" +
                "        public override bool Equals(object? other) => Equals(other as Sample);\n" +
                "\n" +
                "        public override int GetHashCode() => 0;\n" +
                "\n" +
                "        public void DoWork() { throw new ArgumentException(\"message\"); }\n" +
                "    }\n" +
                "}";
            SyntaxTree tree = RepositorySources.RegisterSnippet("SemanticSnippetClean.cs", source);
            SemanticModel model = RepositorySources.GetSemanticModel(tree);
            ClassDeclarationSyntax declaration = tree.GetRoot().DescendantNodes()
                .OfType<ClassDeclarationSyntax>().Single();
            INamedTypeSymbol symbol = (INamedTypeSymbol)model.GetDeclaredSymbol(declaration)!;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Collect("EquatableContract", declaration, symbol, model), Is.Empty);
                Assert.That(Collect("EnumerableCollectionContracts", declaration, symbol, model), Is.Empty);
                Assert.That(Collect("BclExceptionThrows", declaration, symbol, model), Is.Empty);
            }
        }

        [Test]
        public void BclExceptionConventionMustRejectCustomExceptionTypes()
        {
            string source =
                "using System;\n" +
                "namespace Snippet\n" +
                "{\n" +
                "    public sealed class SampleException : Exception\n" +
                "    {\n" +
                "    }\n" +
                "\n" +
                "    public sealed class Thrower\n" +
                "    {\n" +
                "        public void DoWork() { throw new SampleException(); }\n" +
                "    }\n" +
                "}";
            SyntaxTree tree = RepositorySources.RegisterSnippet("CustomExceptionSnippet.cs", source);
            SemanticModel model = RepositorySources.GetSemanticModel(tree);
            ClassDeclarationSyntax thrower = tree.GetRoot().DescendantNodes()
                .OfType<ClassDeclarationSyntax>().Single(declaration => declaration.Identifier.ValueText is "Thrower");
            INamedTypeSymbol symbol = (INamedTypeSymbol)model.GetDeclaredSymbol(thrower)!;

            Assert.That(Collect("BclExceptionThrows", thrower, symbol, model), Is.Not.Empty);
        }

        private static List<string> Collect(
            string convention,
            ClassDeclarationSyntax declaration,
            INamedTypeSymbol symbol,
            SemanticModel model) =>
            [.. SourceFileConventionTests.GetSymbolViolations(declaration, symbol, convention, model)];
    }
}
