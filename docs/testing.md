# Testing

This document describes the test coverage, gaps, and verification strategies for the More Cultural Names Builder.

## Test Project Structure

The solution contains three test projects.

**Unit tests:** `MoreCulturalNamesBuilder.UnitTests/`

```
MoreCulturalNamesBuilder.UnitTests/
├── Configuration/
│   ├── SettingsTestFactory.cs
│   └── SettingsTests.cs
├── Service/
│   ├── LocalisationFetcherTests.cs
│   ├── NameNormaliserTests.cs
│   ├── NameNormaliserEdgeCaseTests.cs
│   ├── Mapping/
│   │   ├── LanguageMappingTests.cs
│   │   ├── LocationMappingTests.cs
│   │   ├── NameMappingTests.cs
│   │   ├── GameIdMappingTests.cs
│   │   └── LanguageCodeMappingTests.cs
│   └── ModBuilders/
│       ├── CK2ModBuilderTests.cs
│       ├── CK3ModBuilderTests.cs
│       ├── HOI4ModBuilderTests.cs
│       └── ImperatorRomeModBuilderTests.cs
└── TestInfrastructure/
    ├── InternalMappingInvoker.cs
    └── TemporaryDirectory.cs
```

**Integration tests:** `MoreCulturalNamesBuilder.IntegrationTests/`

```
MoreCulturalNamesBuilder.IntegrationTests/
├── Ck2ModBuilderIntegrationTests.cs
├── Ck3ModBuilderIntegrationTests.cs
├── HoI4ModBuilderIntegrationTests.cs
├── ImperatorRomeModBuilderIntegrationTests.cs
├── ProgramIntegrationTests.cs
├── ExpectedModContent.cs
├── IntegrationTestSetup.cs
├── ModBuildRequest.cs
├── ModBuildRunner.cs
├── TestArguments.cs
├── TestPaths.cs
├── TextContent.cs
├── TemporaryDirectory.cs
└── TestData/
    ├── languages.xml
    ├── locations.xml
    ├── empty_languages.xml
    ├── empty_locations.xml
    ├── malformed_languages.xml
    ├── duplicate_languages.xml
    ├── duplicate_locations.xml
    ├── nameless_locations.xml
    ├── non_numeric_hoi4_locations.xml
    ├── non_numeric_ir_locations.xml
    ├── duplicate_hoi4_locations.xml
    ├── duplicate_ir_locations.xml
    ├── ck2_landed_titles.txt
    └── ck3_landed_titles.txt
```

The integration tests invoke `Program.Main` directly with real XML data stores and real landed titles files, so the configuration layer, dependency injection container, repository deserialisation, localisation retrieval, name normalisation, and every game builder run as they do in production. The XML stores and landed titles inputs are read from the repository `TestData` directory, which is copied to the test output directory on build.

## Test Framework

- **Framework:** NUnit 5
- **Test Runner:** `dotnet test`
- **Mocking:** Moq (unit tests only)

## Integration Tests

The [integration test project](../MoreCulturalNamesBuilder.IntegrationTests/MoreCulturalNamesBuilder.IntegrationTests.csproj) is included in the solution and runs with `dotnet test`.

- Every game builder is exercised end to end against temporary output directories, asserting the complete content of the descriptor, landed titles, province name, and localisation files with exact string equality.
- The descriptor timestamp is the only non-deterministic element; assertions strip it before comparison.
- Failure scenarios assert the exception type and message that production code raises, including `ArgumentNullException` for absent required arguments, `ArgumentException` for absent aliases and unknown arguments, `NotImplementedException` for unsupported games, `FileNotFoundException` for absent data stores, `InvalidOperationException` for malformed XML, `DuplicateEntityException` for duplicate entity identifiers, and `DirectoryNotFoundException` for an unusable output path.
- Builder failure scenarios assert `AggregateException` wrapping the `FormatException` or `ArgumentException` raised by the parallel data-loading loops.
- A determinism test builds the same inputs twice and compares every generated file byte for byte.
- The integration tests are marked `[NonParallelizable]` because they share the static `Program.ServiceProvider`.

Run the integration tests on their own with:

```bash
dotnet test MoreCulturalNamesBuilder.IntegrationTests/MoreCulturalNamesBuilder.IntegrationTests.csproj
```

## Architecture Fitness Tests

The [architecture test project](../MoreCulturalNamesBuilder.ArchitectureTests/MoreCulturalNamesBuilder.ArchitectureTests.csproj) is included in the solution and runs with `dotnet test`.

- ArchUnit.NET checks the explicitly inventoried production assembly, documented dependency directions, independent game builders, shared builder inheritance, and production assembly isolation.
- Roslyn checks handwritten production and test sources for file/type correspondence, namespaces, syntax conventions, interface contracts, model declarations, mappings, collection contracts, and NUnit conventions.
- Separate project tests inspect declared XML and evaluated MSBuild metadata for Debug and Release, production references, solution inventory, and analyser enforcement.
- Source checks require a repository checkout, exclude generated files and build outputs, and fail when expected sources or selectors are absent.
- Existing violations remain failures, including builder dependencies on configuration and CK3 dependencies on CK2; production code is not changed or exempted to make the suite green.
- These structural checks do not prove runtime behaviour, security, game compatibility, or exhaustive compliance with contextual and review-only instructions.

## Coverage by Component

### Configuration Layer

| Test File | Coverage |
|-----------|----------|
| `SettingsTests.cs` | Required argument detection, alias normalisation |
| `ProgramIntegrationTests.cs` | Required and alias arguments, unknown arguments, unsupported games, absent and malformed stores, duplicate entities, unusable output paths, output directory creation |

**Gaps:**
- Invalid argument combinations
- Game version pattern validation
- Verbose flag edge cases (`"True"`, `"1"`, et cetera.)

### Data Access Layer (Mapping)

| Test File | Coverage |
|-----------|----------|
| `LanguageMappingTests.cs` | Entity ↔ Model round-trip |
| `LocationMappingTests.cs` | Entity ↔ Model round-trip |
| `NameMappingTests.cs` | Entity ↔ Model round-trip |
| `GameIdMappingTests.cs` | Entity ↔ Model round-trip |
| `LanguageCodeMappingTests.cs` | Entity ↔ Model round-trip |

**Gaps:**
- Missing/optional XML attributes
- Large file performance

### Service Layer

| Test File | Coverage |
|-----------|----------|
| `LocalisationFetcherTests.cs` | Fallback chain resolution |
| `NameNormaliserTests.cs` | Game charset transformations |
| `NameNormaliserEdgeCaseTests.cs` | Empty strings, null inputs, special chars |
| Game builder integration tests | Real charset transformation applied to the canonical fixture names for every target game |

**Gaps:**
- No performance benchmarks for large datasets
- No concurrent access stress tests
- Cache invalidation scenarios (not applicable - immutable data)

### Mod Builders

| Test File | Coverage |
|-----------|----------|
| `CK2ModBuilderTests.cs` | Descriptor, landed-titles patching, CSV localisation, optional-file suppression |
| `CK3ModBuilderTests.cs` | Descriptor pair, block merging, default and dynamic localisation files, landed-title prefix tiers including `h_`, name-normalisation call bound |
| `HOI4ModBuilderTests.cs` | State/city localisation, descriptors, empty-input handling, identifier and duplicate failures |
| `ImperatorRomeModBuilderTests.cs` | Province name data, localisation, descriptors, empty-input handling, identifier and duplicate failures |
| `Victoria3ModBuilderTests.cs` | Metadata, dynamic state and hub name effects, on action hooks, localisation, empty-input handling |

**Integration coverage:**

| Test File | Coverage |
|-----------|----------|
| `Ck2ModBuilderIntegrationTests.cs` | Full CK2 output content, Windows-1252 encoding, comment suppression, dependency declaration, empty and nameless data stores, absent landed titles inputs |
| `Ck3ModBuilderIntegrationTests.cs` | Full CK3 output content, descriptor pair, block merging, prefix tiers, dynamic name keys, comment suppression, empty data stores, absent landed titles inputs |
| `HoI4ModBuilderIntegrationTests.cs` | Full HOI4 output content, locale directories, header-only files, non-numeric and duplicate identifiers |
| `ImperatorRomeModBuilderIntegrationTests.cs` | Full Imperator: Rome output content, province name data, header-only files, non-numeric and duplicate identifiers |
| `Victoria3ModBuilderIntegrationTests.cs` | Full Victoria 3 output content, metadata, dynamic state and hub name effects, on action hooks, comment suppression, empty data stores |

**Major Gaps:**
- No game compatibility verification (generated mods are not loaded in the target games)
- No performance benchmarks

## Test Infrastructure

### SettingsTestFactory

Creates test `Settings` instances with minimal valid arguments:

```csharp
public static class SettingsTestFactory
{
    public static Settings CreateValidSettings(
        string game = "CK3",
        string outputDir = "/tmp/test-output")
    {
        return new Settings(new[]
        {
            "--lang", "/tmp/languages.xml",
            "--loc", "/tmp/locations.xml",
            "--id", "test-mod",
            "--name", "Test Mod",
            "--version", "1.0.0",
            "--game", game,
            "--game-version", "1.0.*",
            "--output", outputDir
        });
    }
}
```

### TemporaryDirectory

Provides disposable temporary directories for file I/O tests:

```csharp
public class TemporaryDirectory : IDisposable
{
    public string Path { get; }

    public TemporaryDirectory() { ... }
    public void Dispose() { ... }
}
```

### InternalMappingInvoker

Uses reflection to invoke internal mapping methods for testing:

```csharp
public static class InternalMappingInvoker
{
    public static TModel InvokeToServiceModel<TEntity, TModel>(TEntity entity)
    {
        // Reflection-based invocation of internal extension methods
    }
}
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~SettingsTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

## Verification Strategy

### Current Approach
- Unit tests for isolated components with mocked collaborators
- Integration tests that run the full pipeline for every game against real XML data stores and real landed titles inputs
- Exact-equality assertions on every generated file, which detect unintended format changes
- Manual verification of generated mod files in the target games

### Recommended Improvements

1. **Snapshot Testing**
   - Store the expected output as external snapshot files rather than inline expectations
   - Compare generated files against snapshots
   - Detect unintended format changes with less review effort

2. **Game Compatibility Tests**
   - Load generated mods in the target games (CI)
   - Verify that localisation displays correctly
   - Check for encoding issues

3. **Performance Benchmarks**
   - Measure build time for large data sets
   - Profile memory usage during localisation retrieval
   - Benchmark parallel versus sequential processing

4. **Property-Based Testing**
   - Test normalisation idempotency
   - Test fallback chain termination
   - Test mapping round-trip properties

## CI/CD Integration

**Current:** The [.NET workflow](../.github/workflows/dotnet.yml) restores dependencies, builds the solution, and runs the full test suite on .NET 10.0 for every push and pull request targeting `master`. The [Security workflow](../.github/workflows/security.yml) adds dependency vulnerability scanning and CodeQL analysis.

Reproduce the required checks locally with:

```bash
dotnet build MoreCulturalNamesBuilder.slnx
dotnet test
```

## Test Data Management

### Current
- Integration test data files live in `MoreCulturalNamesBuilder.IntegrationTests/TestData/`
- The canonical `languages.xml` and `locations.xml` stores hold two languages, seven locations, and identifiers for every supported game
- Additional stores cover empty, malformed, duplicate, nameless, and non-numeric identifier scenarios
- The CK2 and CK3 landed titles inputs exercise comment stripping, inline block expansion, nested titles, repeated cultural name blocks, and forbidden-token guards
- The `TestData` directory is copied to the test output directory on build
- Unit tests use inline or mocked data

### Update Process
- Prefer `test-values` fixture values when adding a location, language, or name
- Update `ExpectedModContent` whenever a fixture change alters generated output, and re-run the integration tests
- Keep the stores valid against the XML serialisation attributes in `DataAccess/DataObjects`

## Related Documentation
- [Architecture Overview](architecture-overview.md) - System context
- [Service Layer](service-layer.md) - Components under test
- [Mod Builders](mod-builders.md) - Builder test gaps
- [Data Access Layer](data-access-layer.md) - Mapping test coverage