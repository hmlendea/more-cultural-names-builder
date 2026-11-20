using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.ArchitectureTests
{
    [TestFixture]
    public sealed class SourceFileConventionTests
    {
        [Test]
        public void FilesMustDeclareOneIdenticallyNamedType()
        {
            List<string> violations = [];

            foreach (SyntaxTree tree in RepositorySources.Trees)
            {
                SyntaxNode[] declarations = tree.GetRoot().DescendantNodes()
                    .Where(node => node is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax).ToArray();

                if (declarations.Length != 1)
                {
                    violations.Add($"{tree.FilePath}: Expected exactly one type, found {declarations.Length}.");

                    continue;
                }

                string identifier = GetIdentifier(declarations[0]);

                if (!string.Equals(Path.GetFileNameWithoutExtension(tree.FilePath), identifier,
                    StringComparison.Ordinal))
                {
                    violations.Add($"{tree.FilePath}: Filename must exactly match {identifier}.");
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void NamespacesMustMatchProjectAndDirectory()
        {
            List<string> violations = [];

            foreach (SyntaxTree tree in RepositorySources.Trees)
            {
                string expected = Path.GetDirectoryName(Path.GetRelativePath(RepositorySources.Root, tree.FilePath))!
                    .Replace(Path.DirectorySeparatorChar, '.');
                BaseNamespaceDeclarationSyntax[] namespaces = tree.GetRoot().DescendantNodes()
                    .OfType<BaseNamespaceDeclarationSyntax>().ToArray();

                if (namespaces.Length != 1 ||
                    namespaces[0] is not NamespaceDeclarationSyntax ||
                    !string.Equals(namespaces[0].Name.ToString(), expected, StringComparison.Ordinal))
                {
                    violations.Add($"{tree.FilePath}: Expected one block namespace named {expected}.");
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [TestCase("ExplicitTypes")]
        [TestCase("NoConditionalExpressions")]
        [TestCase("NoTuplesOrRefParameters")]
        [TestCase("NoOptionalParameters")]
        [TestCase("SevenParameterLimit")]
        [TestCase("ExplicitAccessibility")]
        [TestCase("BracedControlFlow")]
        [TestCase("NoNullCoalescingOrEquality")]
        [TestCase("BareRethrows")]
        [TestCase("NoInitAccessors")]
        [TestCase("NoConstants")]
        [TestCase("NoRegionsOrNullableEnable")]
        [TestCase("NoTopLevelStatements")]
        [TestCase("ExpressionBodiedSingleStatements")]
        [TestCase("TargetTypedConstruction")]
        [TestCase("PartialTypesRequireGenerators")]
        [TestCase("MemberOrdering")]
        [TestCase("Naming")]
        [TestCase("ExternalUsings")]
        [TestCase("NoInitCollections")]
        [TestCase("NoIncrementOrDecrement")]
        [TestCase("NoInlineQualifiedTypes")]
        [TestCase("NoRedundantDefaults")]
        [TestCase("CompoundAssignments")]
        [TestCase("ElseIfOnOneLine")]
        [TestCase("NoStringEqualityOperator")]
        [TestCase("SingleCharacterStringCalls")]
        [TestCase("NoGlobalUsings")]
        [TestCase("CultureInvariantDateTime")]
        public void HandwrittenSourcesMustFollowCodingConventions(string convention)
        {
            List<string> violations = [];

            foreach (SyntaxTree tree in RepositorySources.Trees)
            {
                foreach (SyntaxNode node in tree.GetRoot().DescendantNodes(descendIntoTrivia: true))
                {
                    if (ViolatesConvention(node, convention))
                    {
                        violations.Add(RepositorySources.Describe(node, convention));
                    }
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [TestCase("SealedClasses")]
        [TestCase("EquivalentInterfaces")]
        [TestCase("InterfaceParameterNames")]
        [TestCase("CollectionContracts")]
        [TestCase("DataContractAutoProperties")]
        [TestCase("StaticStateFreeMethods")]
        [TestCase("EntryPoint")]
        [TestCase("EnumerableCollectionContracts")]
        [TestCase("DataObjectListCollections")]
        [TestCase("EquatableContract")]
        [TestCase("InterfaceContractCompleteness")]
        [TestCase("InterfaceColocation")]
        [TestCase("NoCustomExceptions")]
        [TestCase("BclExceptionThrows")]
        public void ProductionSymbolsMustFollowSemanticConventions(string convention)
        {
            List<string> violations = [];
            int selected = 0;

            foreach (SyntaxTree tree in RepositorySources.Trees.Where(RepositorySources.IsProduction))
            {
                SemanticModel model = RepositorySources.GetSemanticModel(tree);

                foreach (ClassDeclarationSyntax declaration in tree.GetRoot().DescendantNodes()
                    .OfType<ClassDeclarationSyntax>())
                {
                    INamedTypeSymbol? symbol = model.GetDeclaredSymbol(declaration);
                    Assert.That(symbol, Is.Not.Null, RepositorySources.Describe(declaration, "Type could not be resolved."));
                    selected += 1;
                    violations.AddRange(GetSymbolViolations(declaration, symbol!, convention, model));
                }
            }

            Assert.That(selected, Is.GreaterThan(0), "No production symbols were selected.");
            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        internal static IEnumerable<string> GetSymbolViolations(
            ClassDeclarationSyntax declaration,
            INamedTypeSymbol symbol,
            string convention,
            SemanticModel model)
        {
            if (convention is "SealedClasses" && !symbol.IsStatic && !symbol.IsAbstract && !symbol.IsSealed &&
                !HasDerivedType(symbol))
            {
                yield return RepositorySources.Describe(declaration, "Classes not designed for inheritance must be sealed.");
            }

            if (convention is "EquivalentInterfaces" && !symbol.IsStatic && !symbol.IsAbstract &&
                !IsDataContract(declaration.SyntaxTree) && symbol.GetMembers().OfType<IMethodSymbol>()
                    .Any(method => method.MethodKind is MethodKind.Ordinary && method.DeclaredAccessibility is Accessibility.Public) &&
                !symbol.AllInterfaces.Any(contract => string.Equals(contract.Name, $"I{symbol.Name}") &&
                    SymbolEqualityComparer.Default.Equals(contract.ContainingNamespace, symbol.ContainingNamespace)))
            {
                yield return RepositorySources.Describe(declaration, "Public service contracts need equivalent colocated interfaces.");
            }

            foreach (MethodDeclarationSyntax method in declaration.Members.OfType<MethodDeclarationSyntax>())
            {
                foreach (string violation in GetMethodViolations(method, symbol, convention, model))
                {
                    yield return violation;
                }
            }

            if (convention is "DataContractAutoProperties" && IsDataContract(declaration.SyntaxTree))
            {
                foreach (PropertyDeclarationSyntax property in declaration.Members.OfType<PropertyDeclarationSyntax>())
                {
                    if (property.AccessorList is null || property.AccessorList.Accessors.Count != 2 ||
                        property.AccessorList.Accessors.Any(accessor => accessor.Body is not null ||
                            accessor.ExpressionBody is not null || accessor.IsKind(SyntaxKind.InitAccessorDeclaration)))
                    {
                        yield return RepositorySources.Describe(property, "Data contracts require get/set auto-properties.");
                    }
                }
            }

            if (convention is "EnumerableCollectionContracts")
            {
                foreach (string violation in GetCollectionContractViolations(declaration, symbol, model))
                {
                    yield return violation;
                }
            }

            if (convention is "DataObjectListCollections" &&
                declaration.SyntaxTree.FilePath.Contains(".DataObjects", StringComparison.Ordinal))
            {
                foreach (PropertyDeclarationSyntax property in declaration.Members.OfType<PropertyDeclarationSyntax>())
                {
                    ITypeSymbol? propertyType = model.GetTypeInfo(property.Type).Type;

                    if (propertyType is INamedTypeSymbol named && IsCollection(named) &&
                        named.OriginalDefinition.ToDisplayString() is not "System.Collections.Generic.List<T>")
                    {
                        yield return RepositorySources.Describe(property,
                            "XML-serialised data objects require List<T> collection properties.");
                    }
                }
            }

            if (convention is "EquatableContract" && DeclaresEquatable(declaration))
            {
                bool hasObjectEquals = symbol.GetMembers().OfType<IMethodSymbol>().Any(member =>
                    member.Name is "Equals" && member.IsOverride && member.Parameters.Length == 1 &&
                    member.Parameters[0].Type.SpecialType is SpecialType.System_Object);
                bool hasHashCode = symbol.GetMembers().OfType<IMethodSymbol>()
                    .Any(member => member.Name is "GetHashCode" && member.IsOverride);

                if (!hasObjectEquals || !hasHashCode)
                {
                    yield return RepositorySources.Describe(declaration,
                        "Types with meaningful equality must override Equals(object) and GetHashCode().");
                }
            }

            if (convention is "NoCustomExceptions" && symbol.Name.EndsWith("Exception", StringComparison.Ordinal))
            {
                yield return RepositorySources.Describe(declaration,
                    "Custom exception types are forbidden; a BCL exception must be used instead.");
            }

            if (convention is "InterfaceContractCompleteness" && !symbol.IsAbstract && !symbol.IsStatic)
            {
                foreach (string violation in GetInterfaceCompletenessViolations(declaration, symbol))
                {
                    yield return violation;
                }
            }
        }

        private static IEnumerable<string> GetCollectionContractViolations(
            ClassDeclarationSyntax declaration,
            INamedTypeSymbol symbol,
            SemanticModel model)
        {
            if (IsDataContract(declaration.SyntaxTree) || declaration.SyntaxTree.FilePath.Contains(".DataObjects", StringComparison.Ordinal))
            {
                yield break;
            }

            foreach (PropertyDeclarationSyntax property in declaration.Members.OfType<PropertyDeclarationSyntax>())
            {
                if (!IsExposed(property.Modifiers))
                {
                    continue;
                }

                if (model.GetTypeInfo(property.Type).Type is INamedTypeSymbol named && IsConcreteCollection(named))
                {
                    yield return RepositorySources.Describe(property,
                        $"Collection members must expose IEnumerable<T>, not {named.Name}.");
                }
            }

            foreach (MethodDeclarationSyntax method in declaration.Members.OfType<MethodDeclarationSyntax>())
            {
                if (!IsExposed(method.Modifiers))
                {
                    continue;
                }

                IMethodSymbol? methodSymbol = model.GetDeclaredSymbol(method);

                if (methodSymbol is null)
                {
                    continue;
                }

                foreach (ITypeSymbol type in methodSymbol.Parameters.Select(parameter => parameter.Type)
                    .Append(methodSymbol.ReturnType))
                {
                    if (type is INamedTypeSymbol named && IsConcreteCollection(named))
                    {
                        yield return RepositorySources.Describe(method, $"Collection members must expose IEnumerable<T>, not {named.Name}.");

                        break;
                    }
                }
            }
        }

        private static IEnumerable<string> GetInterfaceCompletenessViolations(
            ClassDeclarationSyntax declaration,
            INamedTypeSymbol symbol)
        {
            INamedTypeSymbol? contract = symbol.AllInterfaces.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, $"I{symbol.Name}", StringComparison.Ordinal) &&
                SymbolEqualityComparer.Default.Equals(candidate.ContainingNamespace, symbol.ContainingNamespace));

            if (contract is null)
            {
                yield break;
            }

            string[] contractMembers = [.. contract.GetMembers().OfType<IMethodSymbol>().Select(member => member.Name)];

            foreach (IMethodSymbol member in symbol.GetMembers().OfType<IMethodSymbol>())
            {
                if (member.MethodKind is MethodKind.Ordinary &&
                    member.DeclaredAccessibility is Accessibility.Public &&
                    !member.IsOverride &&
                    !contractMembers.Contains(member.Name, StringComparer.Ordinal))
                {
                    yield return RepositorySources.Describe(declaration,
                        $"{symbol.Name}.{member.Name} must be declared on {contract.Name}.");
                }
            }
        }

        private static bool DeclaresEquatable(ClassDeclarationSyntax declaration) =>
            declaration.BaseList?.Types.Any(baseType =>
                baseType.Type.ToString().StartsWith("IEquatable", StringComparison.Ordinal)) is true;

        private static bool IsExposed(SyntaxTokenList modifiers) =>
            modifiers.Any(token => token.IsKind(SyntaxKind.PublicKeyword) || token.IsKind(SyntaxKind.ProtectedKeyword));

        private static bool IsConcreteCollection(INamedTypeSymbol type) =>
            type.OriginalDefinition.ToDisplayString() is
                "System.Collections.Generic.List<T>" or
                "System.Collections.Generic.Dictionary<TKey, TValue>" or
                "System.Collections.Generic.IDictionary<TKey, TValue>" or
                "System.Collections.Generic.IList<T>" or
                "System.Collections.Generic.HashSet<T>" or
                "System.Collections.Generic.ICollection<T>" or
                "System.Collections.Generic.IReadOnlyList<T>" or
                "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>" or
                "System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>";

        private static IEnumerable<string> GetMethodViolations(
            MethodDeclarationSyntax method,
            INamedTypeSymbol owner,
            string convention,
            SemanticModel model)
        {
            IMethodSymbol symbol = model.GetDeclaredSymbol(method)!;

            if (convention is "CollectionContracts" && !IsRequiredEntryPoint(symbol) &&
                symbol.DeclaredAccessibility is Accessibility.Public)
            {
                foreach (ITypeSymbol type in symbol.Parameters.Select(parameter => parameter.Type).Append(symbol.ReturnType))
                {
                    if (IsCollection(type) && type.OriginalDefinition.ToDisplayString() is not "System.Collections.Generic.IEnumerable<T>")
                    {
                        yield return RepositorySources.Describe(method, "Collection parameters and returns must use IEnumerable<T>.");
                    }
                }
            }

            if (convention is "BclExceptionThrows")
            {
                foreach (ThrowStatementSyntax statement in method.DescendantNodes().OfType<ThrowStatementSyntax>())
                {
                    if (statement.Expression is not BaseObjectCreationExpressionSyntax creation)
                    {
                        continue;
                    }

                    ITypeSymbol? thrown = model.GetTypeInfo(creation).Type;

                    if (thrown is null)
                    {
                        continue;
                    }

                    string namespaceName = thrown.ContainingNamespace.ToDisplayString();

                    if (!(string.Equals(namespaceName, "System", StringComparison.Ordinal) ||
                        namespaceName.StartsWith("System.", StringComparison.Ordinal)))
                    {
                        yield return RepositorySources.Describe(statement,
                            $"Only BCL exceptions may be thrown, not {thrown.Name}.");
                    }
                }
            }

            if (convention is "InterfaceParameterNames")
            {
                foreach (IMethodSymbol contract in owner.AllInterfaces.SelectMany(contract => contract.GetMembers())
                    .OfType<IMethodSymbol>().Where(contract =>
                        SymbolEqualityComparer.Default.Equals(owner.FindImplementationForInterfaceMember(contract), symbol)))
                {
                    if (!contract.Parameters.Select(parameter => parameter.Name)
                        .SequenceEqual(symbol.Parameters.Select(parameter => parameter.Name), StringComparer.Ordinal))
                    {
                        yield return RepositorySources.Describe(method, $"Parameter names must match {contract.ToDisplayString()}.");
                    }
                }
            }

            if (convention is "StaticStateFreeMethods" && !symbol.IsStatic && !symbol.IsVirtual &&
                !symbol.IsOverride && !symbol.IsAbstract && symbol.MethodKind is MethodKind.Ordinary &&
                method.Body is not null && !owner.AllInterfaces.Any(contract => contract.GetMembers().Any(member =>
                    SymbolEqualityComparer.Default.Equals(owner.FindImplementationForInterfaceMember(member), symbol))))
            {
                DataFlowAnalysis? flow = model.AnalyzeDataFlow(method.Body);

                if (flow is { Succeeded: true } && !flow.ReadInside.Concat(flow.WrittenInside)
                    .Any(member => member is IParameterSymbol { IsThis: true }))
                {
                    yield return RepositorySources.Describe(method, "State-free non-contract methods must be static.");
                }
            }

            if (convention is "EntryPoint" && owner.Name is "Program" && symbol.Name is "Main" &&
                (!symbol.IsStatic || !symbol.ReturnsVoid))
            {
                yield return RepositorySources.Describe(method, "Program requires an explicit static void Main.");
            }
        }

        private static bool IsCollection(ITypeSymbol type) =>
            type.SpecialType is not SpecialType.System_String &&
            (type is IArrayTypeSymbol || type.AllInterfaces.Append(type).Any(contract =>
                contract.OriginalDefinition.ToDisplayString() is "System.Collections.Generic.IEnumerable<T>"));

        private static bool IsRequiredEntryPoint(IMethodSymbol method) =>
            method.Name is "Main" && method.IsStatic && method.ReturnsVoid &&
            method.ContainingType.ToDisplayString() is "MoreCulturalNamesBuilder.Program" &&
            method.Parameters.Length == 1 &&
            method.Parameters[0].Type is IArrayTypeSymbol { Rank: 1 } array &&
            array.ElementType.SpecialType is SpecialType.System_String;

        [Test]
        public void RequiredMainSignatureMustBeAllowedByCollectionContracts()
        {
            MethodDeclarationSyntax method = RepositorySources.Trees.Where(RepositorySources.IsProduction)
                .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                .Single(declaration => declaration.Identifier.ValueText is "Main");
            SemanticModel model = RepositorySources.GetSemanticModel(method.SyntaxTree);
            IMethodSymbol symbol = model.GetDeclaredSymbol(method)!;
            Assert.That(IsRequiredEntryPoint(symbol), "The production entry-point signature must remain static void Main(string[] args).");
            Assert.That(GetMethodViolations(method, symbol.ContainingType, "CollectionContracts", model), Is.Empty);
        }

        [Test]
        public void ExplicitServiceCollectionConstructionMustBeAllowed()
        {
            ObjectCreationExpressionSyntax[] creations = RepositorySources.Trees.Where(RepositorySources.IsProduction)
                .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
                .Where(creation => RepositorySources.GetSemanticModel(creation.SyntaxTree).GetTypeInfo(creation)
                    .Type?.ToDisplayString() is "Microsoft.Extensions.DependencyInjection.ServiceCollection")
                .ToArray();
            Assert.That(creations, Is.Not.Empty, "The composition root must retain explicit ServiceCollection construction.");

            foreach (ObjectCreationExpressionSyntax creation in creations)
            {
                Assert.That(!ViolatesConvention(creation, "NoInitCollections"));
            }
        }

        private static bool HasDerivedType(INamedTypeSymbol symbol) =>
            RepositorySources.Trees.Where(RepositorySources.IsProduction)
                .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                .Any(declaration => SymbolEqualityComparer.Default.Equals(
                    RepositorySources.GetSemanticModel(declaration.SyntaxTree).GetDeclaredSymbol(declaration)?.BaseType, symbol));

        [Test]
        public void ProductionModelsSettingsAndDataObjectsMustBePublicSealedClasses()
        {
            ClassDeclarationSyntax[] declarations = RepositorySources.Trees.Where(RepositorySources.IsProduction)
                .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                .Where(declaration => IsDataContract(declaration.SyntaxTree)).ToArray();
            Assert.That(declarations, Is.Not.Empty, "No production models, data objects, or settings were selected.");
            IEnumerable<string> violations = declarations
                .Where(declaration => !declaration.Modifiers.Any(SyntaxKind.PublicKeyword) ||
                    !declaration.Modifiers.Any(SyntaxKind.SealedKeyword))
                .Select(declaration => RepositorySources.Describe(declaration, "Data contracts must be public sealed."));
            Assert.That(violations, Is.Empty);
        }

        [Test]
        public void MappingMethodsMustBeInternalStaticExtensionsInDedicatedMappingFiles()
        {
            MethodDeclarationSyntax[] methods = RepositorySources.Trees.Where(RepositorySources.IsProduction)
                .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                .Where(method => IsMappingName(method.Identifier.ValueText)).ToArray();
            Assert.That(methods, Is.Not.Empty, "No production mapping methods were selected.");
            List<string> violations = [];

            foreach (MethodDeclarationSyntax method in methods)
            {
                ClassDeclarationSyntax? owner = method.Parent as ClassDeclarationSyntax;
                bool isValid = owner is not null &&
                    owner.Identifier.ValueText.EndsWith("MappingExtensions", StringComparison.Ordinal) &&
                    owner.Modifiers.Any(SyntaxKind.InternalKeyword) && owner.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                    method.Modifiers.Any(SyntaxKind.InternalKeyword) && method.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                    method.ParameterList.Parameters.FirstOrDefault()?.Modifiers.Any(SyntaxKind.ThisKeyword) is true &&
                    method.Ancestors().OfType<NamespaceDeclarationSyntax>().Any(declaration =>
                        declaration.Name.ToString().EndsWith(".Mapping", StringComparison.Ordinal));

                if (!isValid)
                {
                    violations.Add(RepositorySources.Describe(method, "Mapping placement or extension contract is invalid."));
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void WhitespaceMustUseFourSpacesAndNoRedundantBlankLines()
        {
            List<string> violations = [];

            foreach (SyntaxTree tree in RepositorySources.Trees)
            {
                string content = tree.GetText().ToString();
                string[] lines = content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

                for (int lineIndex = 0; lineIndex < lines.Length; lineIndex += 1)
                {
                    string line = lines[lineIndex];
                    int indentation = line.Length - line.TrimStart(' ').Length;

                    if (line.Contains('\t') || indentation % 4 != 0 ||
                        (lineIndex > 0 && string.IsNullOrWhiteSpace(line) &&
                        string.IsNullOrWhiteSpace(lines[lineIndex - 1])))
                    {
                        violations.Add($"{tree.FilePath}:{lineIndex + 1}: Invalid indentation or repeated blank line.");
                    }
                }

                if (!content.EndsWith('\n'))
                {
                    violations.Add($"{tree.FilePath}: Missing final newline.");
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void RepositoryImplementationsMustOnlyBeConstructedAtTheCompositionRoot()
        {
            List<string> violations = [];
            int selected = 0;

            foreach (SyntaxTree tree in RepositorySources.Trees.Where(RepositorySources.IsProduction))
            {
                SemanticModel model = RepositorySources.GetSemanticModel(tree);

                foreach (BaseObjectCreationExpressionSyntax creation in tree.GetRoot().DescendantNodes()
                    .OfType<BaseObjectCreationExpressionSyntax>())
                {
                    IMethodSymbol? constructor = model.GetSymbolInfo(creation).Symbol as IMethodSymbol;

                    if (constructor?.ContainingType.ContainingNamespace.ToDisplayString() is "NuciDAL.Repositories")
                    {
                        selected += 1;

                        if (creation.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault()
                            ?.Identifier.ValueText is not "Program")
                        {
                            violations.Add(RepositorySources.Describe(creation, "Only Program may construct repositories."));
                        }
                    }
                }
            }

            Assert.That(selected, Is.GreaterThan(0), "No repository construction was selected.");
            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void ControlFlowStatementsMustBeSeparatedByBlankLines()
        {
            List<string> violations = [];
            int selected = 0;

            foreach (SyntaxTree tree in RepositorySources.Trees)
            {
                foreach (BlockSyntax block in tree.GetRoot().DescendantNodes().OfType<BlockSyntax>())
                {
                    StatementSyntax[] statements = [.. block.Statements];
                    selected += statements.Length;

                    for (int index = 1; index < statements.Length; index += 1)
                    {
                        StatementSyntax current = statements[index];
                        StatementSyntax previous = statements[index - 1];

                        if (RequiresBlankLineAbove(current, previous) &&
                            tree.GetLineSpan(current.Span).StartLinePosition.Line -
                            tree.GetLineSpan(previous.Span).EndLinePosition.Line < 2)
                        {
                            violations.Add(RepositorySources.Describe(current, "Control flow requires a blank line above."));
                        }
                    }
                }
            }

            Assert.That(selected, Is.GreaterThan(0), "No statements were selected.");
            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        private static bool RequiresBlankLineAbove(StatementSyntax current, StatementSyntax previous)
        {
            if (IsControlFlowStatement(previous))
            {
                return false;
            }

            return IsBlockControlFlowStatement(current) ||
                current is ReturnStatementSyntax or ThrowStatementSyntax or BreakStatementSyntax or ContinueStatementSyntax;
        }

        private static bool IsControlFlowStatement(StatementSyntax statement) =>
            IsBlockControlFlowStatement(statement) || statement is ReturnStatementSyntax or ThrowStatementSyntax ||
                statement is BreakStatementSyntax or ContinueStatementSyntax;

        private static bool IsBlockControlFlowStatement(StatementSyntax statement) =>
            statement is IfStatementSyntax or ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or
                DoStatementSyntax or SwitchStatementSyntax or UsingStatementSyntax or LockStatementSyntax;

        [Test]
        public void OverloadsMustBeGroupedAndOrderedByIncreasingParameterCount()
        {
            List<string> violations = [];
            int selected = 0;

            foreach (SyntaxTree tree in RepositorySources.Trees)
            {
                foreach (TypeDeclarationSyntax type in tree.GetRoot().DescendantNodes()
                    .OfType<TypeDeclarationSyntax>())
                {
                    Dictionary<string, List<int>> positions = [];

                    foreach (MethodDeclarationSyntax method in type.Members.OfType<MethodDeclarationSyntax>())
                    {
                        string name = method.Identifier.ValueText;
                        selected += 1;

                        if (!positions.TryGetValue(name, out List<int>? seen))
                        {
                            positions[name] = [method.ParameterList.Parameters.Count];

                            continue;
                        }

                        if (seen.Count > 0 && method.ParameterList.Parameters.Count < seen[^1])
                        {
                            violations.Add(RepositorySources.Describe(method,
                                "Overloads must be adjacent and ordered from fewest to most parameters."));
                        }

                        seen.Add(method.ParameterList.Parameters.Count);
                    }
                }
            }

            Assert.That(selected, Is.GreaterThan(0), "No methods were selected.");
            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        [Test]
        public void ControlFlowBracesAndMethodSpacingMustFollowSourceLayout()
        {
            List<string> violations = [];

            foreach (SyntaxTree tree in RepositorySources.Trees)
            {
                foreach (SyntaxToken token in tree.GetRoot().DescendantTokens())
                {
                    if (token.IsKind(SyntaxKind.OpenBraceToken) &&
                        token.Parent is BlockSyntax or BaseTypeDeclarationSyntax or NamespaceDeclarationSyntax)
                    {
                        SyntaxToken previous = token.GetPreviousToken();

                        if (previous.RawKind != 0 && tree.GetLineSpan(previous.Span).EndLinePosition.Line ==
                            tree.GetLineSpan(token.Span).StartLinePosition.Line)
                        {
                            violations.Add(RepositorySources.Describe(token.Parent!, "Opening braces require their own line."));
                        }
                    }
                }

                foreach (ClassDeclarationSyntax declaration in tree.GetRoot().DescendantNodes()
                    .OfType<ClassDeclarationSyntax>())
                {
                    MemberDeclarationSyntax? previous = null;

                    foreach (MemberDeclarationSyntax member in declaration.Members)
                    {
                        if (previous is BaseMethodDeclarationSyntax && member is BaseMethodDeclarationSyntax &&
                            member.GetLocation().GetLineSpan().StartLinePosition.Line -
                            previous.GetLocation().GetLineSpan().EndLinePosition.Line != 2)
                        {
                            violations.Add(RepositorySources.Describe(member, "Methods require exactly one separating blank line."));
                        }

                        previous = member;
                    }
                }
            }

            Assert.That(violations, Is.Empty, string.Join(Environment.NewLine, violations));
        }

        private static bool ViolatesConvention(SyntaxNode node, string convention) =>
            ViolatesConvention(node, convention, RepositorySources.GetSemanticModel);

        internal static bool ViolatesConvention(
            SyntaxNode node,
            string convention,
            Func<SyntaxTree, SemanticModel> modelResolver) => convention switch
        {
            "ExplicitTypes" => node is VariableDeclarationSyntax { Type.IsVar: true } ||
                node is ForEachStatementSyntax { Type.IsVar: true },
            "NoConditionalExpressions" => node is ConditionalExpressionSyntax,
            "NoTuplesOrRefParameters" => node is TupleExpressionSyntax or TupleTypeSyntax ||
                node is ParameterSyntax parameter && parameter.Modifiers.Any(SyntaxKind.RefKeyword),
            "NoOptionalParameters" => node is ParameterSyntax { Default: not null },
            "SevenParameterLimit" => node is ParameterListSyntax { Parameters.Count: > 7 } ||
                node is BracketedParameterListSyntax { Parameters.Count: > 7 },
            "ExplicitAccessibility" => MissingAccessibility(node),
            "BracedControlFlow" => HasUnbracedBody(node),
            "NoNullCoalescingOrEquality" => node is BinaryExpressionSyntax binary &&
                (binary.IsKind(SyntaxKind.CoalesceExpression) ||
                (binary.IsKind(SyntaxKind.EqualsExpression) || binary.IsKind(SyntaxKind.NotEqualsExpression)) &&
                (binary.Left.IsKind(SyntaxKind.NullLiteralExpression) ||
                binary.Right.IsKind(SyntaxKind.NullLiteralExpression))),
            "BareRethrows" => node is ThrowStatementSyntax { Expression: IdentifierNameSyntax },
            "NoInitAccessors" => node is AccessorDeclarationSyntax accessor &&
                accessor.IsKind(SyntaxKind.InitAccessorDeclaration),
            "NoConstants" => node is FieldDeclarationSyntax field && field.Modifiers.Any(SyntaxKind.ConstKeyword) ||
                node is LocalDeclarationStatementSyntax local && local.Modifiers.Any(SyntaxKind.ConstKeyword),
            "NoRegionsOrNullableEnable" => node is RegionDirectiveTriviaSyntax or EndRegionDirectiveTriviaSyntax ||
                node is NullableDirectiveTriviaSyntax nullable && nullable.SettingToken.IsKind(SyntaxKind.EnableKeyword),
            "NoTopLevelStatements" => node is GlobalStatementSyntax,
            "ExpressionBodiedSingleStatements" => node is MethodDeclarationSyntax method &&
                method.Body is { Statements.Count: 1 } &&
                method.Body.Statements[0] is ReturnStatementSyntax or ThrowStatementSyntax or ExpressionStatementSyntax,
            "TargetTypedConstruction" => node is VariableDeclarationSyntax variable && !variable.Type.IsVar &&
                variable.Variables.Any(declarator => declarator.Initializer?.Value is ObjectCreationExpressionSyntax),
            "PartialTypesRequireGenerators" => node is TypeDeclarationSyntax declaration &&
                declaration.Modifiers.Any(SyntaxKind.PartialKeyword) && !HasGeneratedRegex(declaration),
            "MemberOrdering" => node is TypeDeclarationSyntax owner && HasInvalidMemberOrder(owner),
            "Naming" => HasInvalidName(node),
            "ExternalUsings" => node is UsingDirectiveSyntax { Parent: BaseNamespaceDeclarationSyntax },
            "NoInitCollections" => node is ObjectCreationExpressionSyntax creation &&
                RepositorySources.GetSemanticModel(node.SyntaxTree).GetTypeInfo(creation).Type is ITypeSymbol type &&
                IsCollection(type) &&
                // DI registration is framework construction, not inline collection initialisation.
                type.ToDisplayString() is not "Microsoft.Extensions.DependencyInjection.ServiceCollection" &&
                !node.SyntaxTree.FilePath.Contains("MoreCulturalNamesBuilder.UnitTests", StringComparison.Ordinal),
            "NoIncrementOrDecrement" => node.IsKind(SyntaxKind.PreIncrementExpression) ||
                node.IsKind(SyntaxKind.PostIncrementExpression) || node.IsKind(SyntaxKind.PreDecrementExpression) ||
                node.IsKind(SyntaxKind.PostDecrementExpression),
            "NoInlineQualifiedTypes" => node is QualifiedNameSyntax qualified &&
                !qualified.Ancestors().Any(parent => parent is UsingDirectiveSyntax) &&
                qualified.Parent is not BaseNamespaceDeclarationSyntax &&
                modelResolver(node.SyntaxTree).GetSymbolInfo(qualified).Symbol is ITypeSymbol,
            "NoRedundantDefaults" => IsRedundantDefault(node),
            "CompoundAssignments" => node is AssignmentExpressionSyntax assignment && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                assignment.Right is BinaryExpressionSyntax binary &&
                (binary.IsKind(SyntaxKind.AddExpression) || binary.IsKind(SyntaxKind.SubtractExpression)) &&
                string.Equals(binary.Left.ToString(), assignment.Left.ToString(), StringComparison.Ordinal),
            "ElseIfOnOneLine" => node is ElseClauseSyntax clause && clause.Statement is IfStatementSyntax statement &&
                clause.SyntaxTree.GetLineSpan(clause.ElseKeyword.Span).StartLinePosition.Line !=
                clause.SyntaxTree.GetLineSpan(statement.IfKeyword.Span).StartLinePosition.Line,
            "NoStringEqualityOperator" => IsStringEqualityComparison(node, modelResolver),
            "SingleCharacterStringCalls" => IsSingleCharacterStringCall(node, modelResolver),
            "NoGlobalUsings" => node is UsingDirectiveSyntax { GlobalKeyword.RawKind: not 0 },
            "CultureInvariantDateTime" => IsCultureSensitiveDateTimeCall(node, modelResolver),
            _ => false
        };

        private static bool IsCultureSensitiveDateTimeCall(SyntaxNode node, Func<SyntaxTree, SemanticModel> modelResolver)
        {
            if (node is not InvocationExpressionSyntax invocation ||
                modelResolver(node.SyntaxTree).GetSymbolInfo(invocation).Symbol is not IMethodSymbol symbol ||
                symbol.ContainingType.SpecialType is not SpecialType.System_DateTime)
            {
                return false;
            }

            if (symbol.Name is not "Parse" and not "TryParse" and not "ParseExact" and not "TryParseExact" and not "ToString")
            {
                return false;
            }

            return !invocation.ArgumentList.Arguments.Any(argument =>
                IsFormatProvider(modelResolver(node.SyntaxTree).GetTypeInfo(argument.Expression).Type));
        }

        private static bool IsFormatProvider(ITypeSymbol? type) =>
            type is not null && (type.ToDisplayString() is "System.IFormatProvider" ||
                type.AllInterfaces.Any(contract => contract.ToDisplayString() is "System.IFormatProvider"));

        private static bool IsRedundantDefault(SyntaxNode node)
        {
            ExpressionSyntax? initialiser = node switch
            {
                FieldDeclarationSyntax field => field.Declaration.Variables
                    .Select(variable => variable.Initializer?.Value).FirstOrDefault(value => value is not null),
                PropertyDeclarationSyntax property => property.Initializer?.Value,
                _ => null
            };

            return initialiser is LiteralExpressionSyntax literal &&
                (literal.IsKind(SyntaxKind.FalseLiteralExpression) || literal.IsKind(SyntaxKind.NullLiteralExpression) ||
                (literal.Token.Value is int number && number == 0) || (literal.Token.Value is double value && value == 0));
        }

        private static bool IsStringEqualityComparison(SyntaxNode node, Func<SyntaxTree, SemanticModel> modelResolver)
        {
            if (node is not BinaryExpressionSyntax binary ||
                (!binary.IsKind(SyntaxKind.EqualsExpression) && !binary.IsKind(SyntaxKind.NotEqualsExpression)))
            {
                return false;
            }

            SemanticModel model = modelResolver(node.SyntaxTree);

            return model.GetTypeInfo(binary.Left).Type?.SpecialType is SpecialType.System_String &&
                model.GetTypeInfo(binary.Right).Type?.SpecialType is SpecialType.System_String;
        }

        private static bool IsSingleCharacterStringCall(SyntaxNode node, Func<SyntaxTree, SemanticModel> modelResolver)
        {
            if (node is not InvocationExpressionSyntax invocation ||
                invocation.ArgumentList.Arguments.Count != 1 ||
                invocation.ArgumentList.Arguments[0].Expression is not LiteralExpressionSyntax literal ||
                literal.Token.Value is not string text || text.Length != 1)
            {
                return false;
            }

            return modelResolver(node.SyntaxTree).GetSymbolInfo(invocation).Symbol is IMethodSymbol symbol &&
                symbol.ContainingType.SpecialType is SpecialType.System_String;
        }

        private static bool MissingAccessibility(SyntaxNode node)
        {
            if (node is not MemberDeclarationSyntax member ||
                member is BaseNamespaceDeclarationSyntax or EnumMemberDeclarationSyntax or GlobalStatementSyntax)
            {
                return false;
            }

            SyntaxTokenList modifiers = member.Modifiers;

            return !modifiers.Any(token => token.IsKind(SyntaxKind.PublicKeyword) ||
                token.IsKind(SyntaxKind.ProtectedKeyword) || token.IsKind(SyntaxKind.InternalKeyword) ||
                token.IsKind(SyntaxKind.PrivateKeyword));
        }

        private static bool HasUnbracedBody(SyntaxNode node) => node switch
        {
            IfStatementSyntax statement => statement.Statement is not BlockSyntax,
            ElseClauseSyntax clause => clause.Statement is not BlockSyntax and not IfStatementSyntax,
            ForStatementSyntax statement => statement.Statement is not BlockSyntax,
            ForEachStatementSyntax statement => statement.Statement is not BlockSyntax,
            WhileStatementSyntax statement => statement.Statement is not BlockSyntax,
            DoStatementSyntax statement => statement.Statement is not BlockSyntax,
            UsingStatementSyntax statement => statement.Statement is not BlockSyntax,
            LockStatementSyntax statement => statement.Statement is not BlockSyntax,
            _ => false
        };

        private static bool HasGeneratedRegex(TypeDeclarationSyntax declaration) =>
            declaration.Members.SelectMany(member => member.AttributeLists)
                .SelectMany(list => list.Attributes).Any(attribute =>
                    RepositorySources.GetSemanticModel(attribute.SyntaxTree).GetSymbolInfo(attribute).Symbol
                        ?.ContainingType.ToDisplayString() is "System.Text.RegularExpressions.GeneratedRegexAttribute");

        private static bool HasInvalidMemberOrder(TypeDeclarationSyntax owner)
        {
            int previous = -1;

            foreach (MemberDeclarationSyntax member in owner.Members)
            {
                int current = GetMemberOrder(member);

                if (current < previous)
                {
                    return true;
                }

                previous = current;
            }

            return false;
        }

        private static int GetMemberOrder(MemberDeclarationSyntax member) => member switch
        {
            FieldDeclarationSyntax => 0,
            PropertyDeclarationSyntax or IndexerDeclarationSyntax => 1,
            EventDeclarationSyntax or EventFieldDeclarationSyntax => 2,
            ConstructorDeclarationSyntax or DestructorDeclarationSyntax => 3,
            _ => 4
        };

        private static bool HasInvalidName(SyntaxNode node)
        {
            if (node is InterfaceDeclarationSyntax declaration)
            {
                string name = declaration.Identifier.ValueText;

                return name.Length < 2 || name[0] != 'I' || !char.IsUpper(name[1]);
            }

            if (node is VariableDeclaratorSyntax variable &&
                variable.Parent?.Parent is FieldDeclarationSyntax field &&
                field.Modifiers.Any(SyntaxKind.PrivateKeyword))
            {
                return variable.Identifier.ValueText.StartsWith('_') ||
                    !char.IsLower(variable.Identifier.ValueText[0]);
            }

            if (node is TypeDeclarationSyntax type)
            {
                return !char.IsUpper(type.Identifier.ValueText[0]);
            }

            if (node is MethodDeclarationSyntax method)
            {
                return !char.IsUpper(method.Identifier.ValueText[0]);
            }

            return false;
        }

        private static string GetIdentifier(SyntaxNode node) => node switch
        {
            BaseTypeDeclarationSyntax declaration => declaration.Identifier.ValueText,
            DelegateDeclarationSyntax declaration => declaration.Identifier.ValueText,
            _ => throw new ArgumentException("Expected a type declaration.", nameof(node))
        };

        private static bool IsDataContract(SyntaxTree tree) =>
            tree.FilePath.Contains($"{Path.DirectorySeparatorChar}Models{Path.DirectorySeparatorChar}") ||
            tree.FilePath.Contains($"{Path.DirectorySeparatorChar}DataObjects{Path.DirectorySeparatorChar}") ||
            tree.FilePath.Contains($"{Path.DirectorySeparatorChar}Configuration{Path.DirectorySeparatorChar}");

        private static bool IsMappingName(string name) =>
            name is "ToServiceModel" or "ToServiceModels" or "ToDataObject" or "ToDataObjects";
    }
}
