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

- **Framework:** xUnit (inferred from project structure)
- **Test Runner:** `dotnet test`
- **Mocking:** Manual test doubles (no mocking framework detected)

## Coverage by Component

### Configuration Layer

| Test File | Coverage |
|-----------|----------|
| `SettingsTests.cs` | Required argument detection, alias normalisation |

**Gaps:**
- Invalid argument combinations
- File path validation (existence, readability)
- Game version pattern validation
- Verbose flag edge cases (`"True"`, `"1"`, etc.)

### Data Access Layer (Mapping)

| Test File | Coverage |
|-----------|----------|
| `LanguageMappingTests.cs` | Entity ↔ Model round-trip |
| `LocationMappingTests.cs` | Entity ↔ Model round-trip |
| `NameMappingTests.cs` | Entity ↔ Model round-trip |
| `GameIdMappingTests.cs` | Entity ↔ Model round-trip |
| `LanguageCodeMappingTests.cs` | Entity ↔ Model round-trip |

**Gaps:**
- XML deserialization edge cases
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
| `CK2ModBuilderTests.cs` | Factory selection, basic instantiation |
| `CK3ModBuilderTests.cs` | Factory selection, basic instantiation |
| `HOI4ModBuilderTests.cs` | Factory selection, basic instantiation |
| `ImperatorRomeModBuilderTests.cs` | Factory selection, basic instantiation |

**Major Gaps:**
- No end-to-end tests generating actual mod files
- No output format validation
- No game compatibility verification
- No integration tests with real XML data
- No descriptor content validation
- No localisation file content validation

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
   - Capture expected output for known inputs
   - Compare generated files against snapshots
   - Detect unintended format changes

3. **Game Compatibility Tests**
   - Load generated mods in target games (CI)
   - Verify localisation displays correctly
   - Check for encoding issues

4. **Performance Benchmarks**
   - Measure build time for large datasets
   - Profile memory usage during localisation fetching
   - Benchmark parallel vs sequential processing

5. **Property-Based Testing**
   - Test normalisation idempotency
   - Test fallback chain termination
   - Test mapping round-trip properties

## CI/CD Integration

**Current:** No CI configuration detected in repository

**Recommended:**
```yaml
# .github/workflows/test.yml
name: Test
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet test --no-restore
```

## Test Data Management

### Current
- No test data files in repository
- Tests use inline/mock data

### Recommended
- Add minimal valid `languages.xml` and `locations.xml` to `test-data/`
- Version control test data alongside tests
- Document test data schema and update process

## Related Documentation
- [Architecture Overview](architecture-overview.md) - System context
- [Service Layer](service-layer.md) - Components under test
- [Mod Builders](mod-builders.md) - Builder test gaps
- [Data Access Layer](data-access-layer.md) - Mapping test coverage