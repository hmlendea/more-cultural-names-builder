# Deployment & Operations

This document describes build, release, and execution characteristics of the More Cultural Names Builder.

## Build Process

### Prerequisites
- .NET 10.0 SDK
- Linux/macOS/Windows (cross-platform)

### Build Commands

```bash
# Restore dependencies
dotnet restore

# Build (Debug)
dotnet build

# Build (Release)
dotnet build -c Release

# Publish (self-contained, single file)
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

### Build Output

| Configuration | Output Location |
|---------------|-----------------|
| Debug | `MoreCulturalNamesBuilder/bin/Debug/net10.0/` |
| Release | `MoreCulturalNamesBuilder/bin/Release/net10.0/` |
| Published | `MoreCulturalNamesBuilder/bin/Release/net10.0/linux-x64/publish/` |

## Release Process

### Release Workflow

Releases are produced by the [GitHub Release workflow](../.github/workflows/github-release.yml), triggered when a release is published on GitHub. The workflow resolves the .NET version from `MoreCulturalNamesBuilder/MoreCulturalNamesBuilder.csproj`, derives the release version from the `v`-prefixed tag, and runs the shared release helper from `https://raw.githubusercontent.com/hmlendea/dotnet-release-scripts/refs/heads/master/dotnet/<dotnet-version>.sh`.

### Version Management

- Version specified via `--version` CLI argument at build time
- No automatic versioning from git tags in the project file
- No assembly version attributes in project file
- The release workflow assigns `Version`, `AssemblyVersion`, `FileVersion`, and `InformationalVersion` from the release tag during publishing

### Artifacts

| Artifact | Description |
|----------|-------------|
| `more-cultural-names-builder_<version>_<runtime-identifier>.zip` | Self-contained archive for each supported runtime identifier |

## Execution Characteristics

### Process Model

| Characteristic | Value |
|----------------|-------|
| **Topology** | Single-threaded console process |
| **Concurrency** | Parallel.ForEach for localisation fetching only |
| **Memory** | All XML data loaded into memory at startup |
| **I/O** | Synchronous file reads/writes |
| **Exit Codes** | 0 = success, non-zero = exception |

### Resource Requirements

| Resource | Estimate |
|----------|----------|
| **Memory** | ~50-200 MB (depends on XML data size) |
| **CPU** | Single core + parallel localisation fetching |
| **Disk** | Output size proportional to localisation count × languages |
| **Network** | None (all data local) |

### Build Performance

Measured against the production data set (~44,000 locations, ~4,500 languages, ~220,000 resolved CK3 localisations) on a single machine:

| Game | Fetching the data | Generating the files | Total |
|------|-------------------|----------------------|-------|
| CK2 / CK2HIP / CK2RoI | ~2 s | ~4 s | ~7-9 s |
| CK3 / CK3AEP / CK3CE / CK3MBP / CK3TFE | ~2-4 s | ~3 s | ~7-9 s |
| HOI4 / HOI4MDM | ~1 s | ~1 s | ~4 s |
| Imperator: Rome | ~1 s | ~1 s | ~4 s |

Generation cost is linear in the number of localisations. The CK3 builder resolves every cultural name through a single canonical index, built once per build; it previously re-scanned the entire localisation set for each generated `name_list_` entry, which made the CK3 editions take over 30 minutes. Guard against reintroducing a similar lookup: [`CK3ModBuilderTests`](../MoreCulturalNamesBuilder.UnitTests/Service/ModBuilders/CK3ModBuilderTests.cs) asserts a bound on the number of name-normalisation calls.

### Environment Variables

None required. All configuration via CLI arguments.

## CLI Usage

### Required Arguments

```bash
MoreCulturalNamesBuilder \
  --lang /path/to/languages.xml \
  --loc /path/to/locations.xml \
  --id more-cultural-names \
  --name "More Cultural Names" \
  --version 1.0.0 \
  --game CK3 \
  --game-version "1.18.*" \
  --output /path/to/output
```

### Optional Arguments

| Argument | Description |
|----------|-------------|
| `--landed-titles` | Existing landed titles file (CK2/CK3) |
| `--landed-titles-name` | Output landed titles file name |
| `--dependency` / `--dep` | Mod dependency ID |
| `--verbose` | Enable verbose comments (`true`/`false`) |

### Game Targets

| Game | Argument Value | Output Directory |
|------|----------------|------------------|
| Crusader Kings II | `CK2` | `<output>/CK2/<mod-id>/` |
| Crusader Kings III | `CK3` | `<output>/CK3/<mod-id>/` |
| Hearts of Iron IV | `HOI4` | `<output>/HOI4/<mod-id>/` |
| Imperator: Rome | `IR` | `<output>/IR/<mod-id>/` |

## Output Structure

### CK2
```
<output>/CK2/<mod-id>/
├── <mod-id>.mod
├── common/landed_titles/<landed-titles-name>
└── localisation/000_<mod-id>_landed_titles.csv
```

### CK3
```
<output>/CK3/<mod-id>/
├── <mod-id>.mod
├── mod/<mod-id>/descriptor.mod
├── common/landed_titles/<landed-titles-name>
└── localization/
    ├── titles_l_english.yml
    ├── titles_cultural_names_l_english.yml
    └── ... (9 languages)
```

### HOI4
```
<output>/HOI4/<mod-id>/
├── <mod-id>.mod
├── descriptor.mod
└── localisation/
    ├── english/zzz999_<mod-id>_l_english.yml
    ├── french/zzz999_<mod-id>_l_french.yml
    └── ... (5 languages)
```

### Imperator: Rome
```
<output>/IR/<mod-id>/
├── <mod-id>.mod
├── descriptor.mod
├── common/province_names/
│   ├── english.txt
│   ├── french.txt
│   └── ... (4 languages)
└── localization/
    ├── <mod-id>_provincenames_l_english.yml
    └── ... (4 languages)
```

## Operational Considerations

### Idempotency
- **Not idempotent**: Re-running with same arguments overwrites output files
- **No partial rollback**: Failed builds leave partial output
- **Recommendation**: Clean output directory before re-running

### Parallel Execution
- Multiple builds can run simultaneously if output directories differ
- No shared state or locks between processes
- Safe for CI/CD parallelisation

### Error Handling
- All exceptions bubble to `Main` → non-zero exit
- No structured error codes (all failures = exit code 1)
- Error messages written to stderr

### Logging
- Console output only (stdout for progress, stderr for errors)
- No log files generated
- No log levels or structured logging

## Continuous Integration and Delivery

### Build and Test Workflow

The build and test workflow (`.github/workflows/dotnet.yml`) runs on every push and pull request targeting `master`. It restores dependencies, builds the solution, and runs the test suite with .NET 10.0.

### Release Workflow

The release workflow (`.github/workflows/github-release.yml`) is triggered when a GitHub release is published. It:

1. Determines the .NET version from `MoreCulturalNamesBuilder/MoreCulturalNamesBuilder.csproj`
2. Extracts the release version from the GitHub tag
3. Runs the shared release helper from `https://raw.githubusercontent.com/hmlendea/dotnet-release-scripts/refs/heads/master/dotnet/<version>.sh`, which publishes self-contained archives for `linux-arm`, `linux-arm64`, `linux-x64`, `osx-arm64`, `osx-x64`, `win-arm64`, and `win-x64`
4. Attaches the produced `MoreCulturalNamesBuilder/bin/Release/more-cultural-names-builder_*` archives to the GitHub release

### Security Scanning

The repository includes a dedicated security scanning workflow (`.github/workflows/security.yml`) that runs:

- **On every push** to `master` branch
- **On every pull request** targeting `master` branch
- **Weekly schedule** (Sundays at 00:00 UTC)

The security scan performs:

1. **Dependency Vulnerability Check** - Uses `dotnet list package --vulnerable --include-transitive` to detect known vulnerabilities in NuGet packages
2. **CodeQL Static Analysis** - GitHub's CodeQL engine analyses C# code for security issues including:
   - SQL injection vulnerabilities
   - Path traversal vulnerabilities
   - Cross-site scripting (XSS) patterns
   - Insecure deserialization
   - Hardcoded secrets
   - And other common security anti-patterns

Results are published to the GitHub Security tab and can be viewed in the Actions workflow runs. Failed security scans will block merges when branch protection rules are configured.

## Troubleshooting

### Common Issues

| Issue | Cause | Resolution |
|-------|-------|------------|
| `FileNotFoundException` | XML file path incorrect | Verify `--lang` and `--loc` paths |
| `XmlException` | Malformed XML | Validate XML against expected schema |
| `ArgumentException` | Missing required args | Check all required arguments provided |
| `NotImplementedException` | Unsupported game | Use CK2, CK3, HOI4, or IR |
| Output directory not created | App doesn't create dirs | Create output directory manually |

### Debugging

```bash
# Enable verbose output
--verbose true

# Check exit code
echo $?

# Run with .NET diagnostic
dotnet run -- --lang ... --loc ... --output ...
```

## Scaling Considerations

### Current Limitations
- Single process, in-memory data loading
- No streaming for large XML files
- No distributed processing

### Horizontal Scaling
- Run multiple independent processes with different output directories
- Partition input data by location ranges (manual)
- No built-in sharding support

### Vertical Scaling
- Increase memory for larger datasets
- More CPU cores benefit parallel localisation fetching
- SSD improves I/O for large output generation

## Related Documentation
- [Architecture Overview](architecture-overview.md) - Deployment constraints
- [Configuration System](configuration-system.md) - CLI arguments
- [Mod Builders](mod-builders.md) - Output formats
- [Testing](testing.md) - Verification strategies