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

### Current Release Script

**Location:** `release.sh`

```bash
#!/bin/bash
# Builds and packages release artifacts
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
# Additional packaging steps...
```

### Version Management

- Version specified via `--version` CLI argument at build time
- No automatic versioning from git tags
- No assembly version attributes in project file

### Artifacts

| Artifact | Description |
|----------|-------------|
| `MoreCulturalNamesBuilder` | Self-contained executable (Linux x64) |
| `MoreCulturalNamesBuilder.exe` | Self-contained executable (Windows x64) |

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

## CI/CD Recommendations

### Build Pipeline
```yaml
# .github/workflows/build.yml
name: Build
on: [push, pull_request]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet restore
      - run: dotnet build -c Release --no-restore
      - run: dotnet test --no-restore
```

### Release Pipeline
```yaml
# .github/workflows/release.yml
name: Release
on:
  push:
    tags: ['v*']
jobs:
  release:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true
      - run: dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
      - uses: softprops/action-gh-release@v1
        with:
          files: |
            bin/Release/net10.0/linux-x64/publish/MoreCulturalNamesBuilder
            bin/Release/net10.0/win-x64/publish/MoreCulturalNamesBuilder.exe
```

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