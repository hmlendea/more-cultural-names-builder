# Testing

This document describes the test coverage, gaps, and verification strategies for the More Cultural Names Builder.

## Test Project Structure

**Location:** `MoreCulturalNamesBuilder.UnitTests/`

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

## Test Framework

- **Framework:** NUnit 5
- **Test Runner:** `dotnet test`
- **Mocking:** Moq

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

**Gaps:**
- Invalid argument combinations
- File path validation (existence, readability)
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
- XML deserialisation edge cases
- Missing/optional XML attributes
- Malformed XML handling
- Large file performance

### Service Layer

| Test File | Coverage |
|-----------|----------|
| `LocalisationFetcherTests.cs` | Fallback chain resolution |
| `NameNormaliserTests.cs` | Game charset transformations |
| `NameNormaliserEdgeCaseTests.cs` | Empty strings, null inputs, special chars |

**Gaps:**
- No integration tests with real XML data files
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

**Major Gaps:**
- No game compatibility verification (generated mods are not loaded in the target games)
- No integration tests with real XML data
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
- Unit tests for isolated components
- Manual verification of generated mod files
- No automated integration testing

### Recommended Improvements

1. **Integration Tests**
   - Create minimal valid XML test data
   - Run full build pipeline for each game
   - Validate output file structure and content

2. **Snapshot Testing**
   - Capture the expected output for known inputs
   - Compare generated files against snapshots
   - Detect unintended format changes

3. **Game Compatibility Tests**
   - Load generated mods in the target games (CI)
   - Verify that localisation displays correctly
   - Check for encoding issues

4. **Performance Benchmarks**
   - Measure build time for large data sets
   - Profile memory usage during localisation retrieval
   - Benchmark parallel versus sequential processing

5. **Property-Based Testing**
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
- No test data files in the repository
- Tests use inline or mock data

### Recommended
- Add minimal valid `languages.xml` and `locations.xml` to `test-data/`
- Version control test data alongside the tests
- Document the test data configuration and update process

## Related Documentation
- [Architecture Overview](architecture-overview.md) - System context
- [Service Layer](service-layer.md) - Components under test
- [Mod Builders](mod-builders.md) - Builder test gaps
- [Data Access Layer](data-access-layer.md) - Mapping test coverage