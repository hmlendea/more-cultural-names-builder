# Configuration System

This document describes the CLI argument parsing, validation, and settings management implementation.

## Overview

The configuration system is the entry point for all external input. It parses command-line arguments using `NuciCLI.Arguments.ArgumentParser`, validates required fields, and exposes normalised settings through immutable configuration objects.

## Class Hierarchy

```
Settings (root container)
├── InputSettings
├── ModSettings
└── OutputSettings
```

## Settings.cs - Root Configuration Container

**Location:** `MoreCulturalNamesBuilder/Configuration/Settings.cs`

### Responsibilities
- Instantiate `ArgumentParser` and register all supported arguments
- Parse command-line arguments into `ArgumentsCollection`
- Delegate to sub-settings constructors
- Expose `Input`, `Mod`, `Output` properties

### Argument Registration

```csharp
ArgumentParser parser = new();

// InputSettings arguments
parser.AddArgument("lang", required: true);
parser.AddArgument("loc", required: true);
parser.AddArgument("landed-titles");  // optional

// OutputSettings arguments
parser.AddArgument("output");
parser.AddArgument("out");  // alias
parser.AddArgument("verbose", defaultValue: "false");
parser.AddArgument("landed-titles-name");  // optional

// ModSettings arguments
parser.AddArgument("id", required: true);
parser.AddArgument("name", required: true);
parser.AddArgument("version");
parser.AddArgument("ver");  // alias
parser.AddArgument("dependency");
parser.AddArgument("dep");  // alias
parser.AddArgument("game", required: true);
parser.AddArgument("game-version", required: true);
```

### Validation Behaviour
- Required arguments (`lang`, `loc`, `id`, `name`, `game`, `game-version`) throw an exception when omitted
- Alias pairs (`output`/`out`, `version`/`ver`, `dependency`/`dep`) - **both must be provided or validation fails** (a known validation gap for legacy script support)
- No I/O operations during construction (parsing only)

## InputSettings.cs

**Location:** `MoreCulturalNamesBuilder/Configuration/InputSettings.cs`

### Properties

| Property | Source Argument | Required | Description |
|----------|-----------------|----------|-------------|
| `LanguageStorePath` | `--lang` | Yes | Path to the languages XML data store |
| `LocationStorePath` | `--loc` | Yes | Path to the locations XML data store |
| `LandedTitlesFilePath` | `--landed-titles` | No | Path to an existing landed titles file (CK2 and CK3 only) |

### Implementation Notes
- Uses `args.Get<string>("key")` for required arguments (throws when omitted)
- Uses the indexer `args["key"]` for optional arguments (returns null when omitted)
- No validation of file existence - deferred to the data access layer

## ModSettings.cs

**Location:** `MoreCulturalNamesBuilder/Configuration/ModSettings.cs`

### Properties

| Property | Source Argument | Required | Description |
|----------|-----------------|----------|-------------|
| `Id` | `--id` | Yes | Mod identifier (e.g., `more-cultural-names`) |
| `Name` | `--name` | Yes | Human-readable mod name |
| `Version` | `--version` / `--ver` | Yes | Mod version (e.g., `1.0.0`) |
| `Dependency` | `--dependency` / `--dep` | No | Mod ID this mod depends on |
| `Game` | `--game` | Yes | Target game: `CK2`, `CK3`, `EU5`, `HOI4`, `IR` |
| `GameVersion` | `--game-version` | Yes | Supported game version (e.g., `1.18.*`) |

### Version Resolution Logic
```csharp
private static string ResolveVersion(ArgumentsCollection args)
{
    string version = (string)args["version"];
    if (!string.IsNullOrWhiteSpace(version)) return version;

    string versionAlias = (string)args["ver"];
    if (!string.IsNullOrWhiteSpace(versionAlias)) return versionAlias;

    throw new ArgumentException("Missing required argument: --version (alias: --ver).");
}
```

### Dependency Resolution Logic
```csharp
private static string ResolveDependency(ArgumentsCollection args)
{
    string dependency = (string)args["dependency"];
    if (!string.IsNullOrWhiteSpace(dependency)) return dependency;

    string dependencyAlias = (string)args["dep"];
    if (!string.IsNullOrWhiteSpace(dependencyAlias)) return dependencyAlias;

    return dependency;  // returns null if neither provided
}
```

### Known Issues
- **Validation Gap**: Both `--version` and `--ver` must be provided or validation fails, even when only one is used. This was introduced to support legacy scripts but produces confusing error messages.
- **Game Validation**: No validation of the `--game` value in `ModSettings`; validation is deferred to `ModBuilderFactory`, which throws `NotImplementedException` for unsupported games.

## OutputSettings.cs

**Location:** `MoreCulturalNamesBuilder/Configuration/OutputSettings.cs`

### Properties

| Property | Source Argument | Required | Default | Description |
|----------|-----------------|----------|---------|-------------|
| `ModOutputDirectory` | `--output` / `--out` | Yes | - | Output directory for the generated mod files |
| `AreVerboseCommentsEnabled` | `--verbose` | No | `false` | Include verbose comments in the generated output |
| `LandedTitlesFileName` | `--landed-titles-name` | No | - | File name for the output landed titles file |

### Output Directory Resolution
```csharp
private static string ResolveOutputDirectory(ArgumentsCollection args)
{
    string outputDirectory = (string)args["output"];
    if (!string.IsNullOrWhiteSpace(outputDirectory)) return outputDirectory;

    string outputDirectoryAlias = (string)args["out"];
    if (!string.IsNullOrWhiteSpace(outputDirectoryAlias)) return outputDirectoryAlias;

    throw new ArgumentException("Missing required argument: --output (alias: --out).");
}
```

### Verbose Flag Parsing
```csharp
AreVerboseCommentsEnabled = args.Get<string>("verbose") == "true";
```
- Only exact string `"true"` enables verbose mode (case-sensitive)
- Any other value (including `"false"`, `"True"`, `"1"`) results in `false`

## Argument Aliases Summary

| Primary | Alias | Used By | Notes |
|---------|-------|---------|-------|
| `--output` | `--out` | OutputSettings | Both must be present or validation fails |
| `--version` | `--ver` | ModSettings | Both must be present or validation fails |
| `--dependency` | `--dep` | ModSettings | Optional; returns null when neither is provided |

## Error Handling

### Exception Types Thrown
- `ArgumentException` - Missing required arguments (from `ArgumentParser` or custom resolution)
- `NotImplementedException` - Unsupported game value (from `ModBuilderFactory`, not the configuration layer)

### Error Messages
- `"Missing required argument: --output (alias: --out)."`
- `"Missing required argument: --version (alias: --ver)."`
- `"The game \"{game}\" is not supported"` (from factory)

### Fail-Fast Guarantee
All validation occurs in the `Settings` constructor before any I/O or build operations begin. Invalid configuration prevents application startup entirely.

## Usage in Program.cs

```csharp
public static void Main(string[] args)
{
    settings = new Settings(args);  // Parses and validates
    BuildServiceProvider();         // Sets up DI container

    ServiceProvider
        .GetService<IModBuilderFactory>()
        .GetModBuilder(settings)    // Selects game-specific builder
        .Build();                   // Executes build pipeline
}
```

## Testing Coverage

**Location:** `MoreCulturalNamesBuilder.UnitTests/Configuration/SettingsTests.cs`

### Tested Scenarios
- Required argument detection
- Alias argument normalisation (`--out` → `OutputSettings.ModOutputDirectory`)
- Version resolution (`--ver` fallback)
- Dependency resolution (`--dep` fallback)
- Verbose flag parsing

### Coverage Gaps
- File path validation (existence, readability)
- Game value validation
- Game version format validation
- Output directory writability checks

## Extension Points

To include new configuration options:
1. Add argument registration in the `Settings` constructor
2. Add a property to the appropriate sub-settings class (`InputSettings`, `ModSettings`, or `OutputSettings`)
3. Implement resolution logic when aliases or complex validation are required
4. Update `SettingsTestFactory` and add tests in `SettingsTests.cs`

## Related Documentation
- [Architecture Overview](architecture-overview.md) - The configuration layer in context
- [Data Access Layer](data-access-layer.md) - How settings feed into repositories
- [Mod Builders](mod-builders.md) - How settings drive builder selection