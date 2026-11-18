[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/more-cultural-names-builder)](https://github.com/hmlendea/more-cultural-names-builder/releases/latest)
[![Build Status](https://github.com/hmlendea/more-cultural-names-builder/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/more-cultural-names-builder/actions/workflows/dotnet.yml)
[![Security Scan](https://github.com/hmlendea/more-cultural-names-builder/actions/workflows/security.yml/badge.svg)](https://github.com/hmlendea/more-cultural-names-builder/actions/workflows/security.yml)
[![License](https://img.shields.io/github/license/hmlendea/more-cultural-names-builder)](https://github.com/hmlendea/more-cultural-names-builder/blob/master/LICENSE)

# More Cultural Names Mod Builder

A CLI tool that generates game mods which localise place names into different languages based on the faction controlling each location.

## 📑 Table of Contents

- [Capabilities](#capabilities)
- [Usage](#usage)
- [Known Limitations](#known-limitations)
- [Installation](#installation)
  - [Manual Installation](#manual-installation)
- [Configuration](#configuration)
  - [Command-Line Options](#command-line-options)
- [Compatibility](#compatibility)
- [Privacy and Data](#privacy-and-data)
- [Development](#development)
  - [Requirements](#requirements)
  - [Setup](#setup)
  - [Build](#build)
  - [Run](#run)
  - [Test](#test)
  - [Integration Tests](#integration-tests)
  - [Coverage](#coverage)
  - [Linting](#linting)
  - [Static Analysis](#static-analysis)
  - [Debugging and Profiling](#debugging-and-profiling)
  - [Continuous Integration](#continuous-integration)
  - [Release](#release)
  - [Dependencies](#dependencies)
- [GitHub Actions](#github-actions)
- [Project Structure](#project-structure)
  - [Projects and Packages](#projects-and-packages)
  - [Directories](#directories)
- [Architecture](#architecture)
- [Documentation](#documentation)
- [Data Formats and Protocols](#data-formats-and-protocols)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [Code of Conduct](#code-of-conduct)
- [Security](#security)
- [Support](#support)
- [Project Engagement](#project-engagement)
- [License](#license)

## ✨ Capabilities

Generates localised mod files for multiple game engines:

- **Crusader Kings II** (via `CK2`)
- **Crusader Kings III** (via `CK3`)
- **Hearts of Iron IV** (via `HOI4`)
- **Imperator: Rome** (via `IR`)

Each generation run:

- Retrieves language and location definitions from caller-supplied XML data stores
- Resolves every location against the languages that supply a cultural name for it
- Normalises names to the character set supported by the target game engine
- Patches an existing landed titles file for the CK series rather than replacing it
- Emits mod descriptors, localisation files, and game-specific data files

## 🚀 Usage

Typical invocation:

```bash
./MoreCulturalNamesBuilder \
  --lang /data/languages.xml \
  --loc /data/locations.xml \
  --output /mods/more-cultural-names \
  --id more-cultural-names \
  --name "More Cultural Names" \
  --version 1.0.0 \
  --game CK3 \
  --game-version "1.18.*"
```

Complete example with all available options:

```bash
./MoreCulturalNamesBuilder \
  --lang /data/languages.xml \
  --loc /data/locations.xml \
  --output /mods/more-cultural-names \
  --id more-cultural-names \
  --name "More Cultural Names" \
  --version 1.0.0 \
  --game CK3 \
  --game-version "1.18.*" \
  --landed-titles /game/common/landed_titles/00_landed_titles.txt \
  --landed-titles-name landed_titles.txt \
  --dep other-mod-id \
  --verbose true
```

Note: Use either `--output` or `--out` (required), and either `--version` or `--ver` (required).

## ⚠️ Known Limitations

- The application does not create intermediate output directories; the target directory must exist and be writable prior to execution.
- Builds are not transactional; a failed run leaves partial output, so clean the output directory prior to repeating a build.
- The alias pairs `--output`/`--out` and `--version`/`--ver` must both be supplied, even when only one value is intended; this legacy contract produces confusing validation messages.
- The `--game` argument requires an exact identifier correspondence (`CK2`, `CK3`, `HOI4`, `IR`); abbreviations are not recognised.
- Generated mods are not validated within the target games; their syntax is asserted only against the expectations of the builders.

## 📦 Installation

[![Obtain it from GitHub](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/github.png)](https://github.com/hmlendea/more-cultural-names-builder/releases)

### Manual Installation

Download the archive for your platform from the [latest release](https://github.com/hmlendea/more-cultural-names-builder/releases/latest), extract it, and run `MoreCulturalNamesBuilder` from the extracted directory. The published archives are self-contained builds for `linux-arm`, `linux-arm64`, `linux-x64`, `osx-arm64`, `osx-x64`, `win-arm64`, and `win-x64`.

## ⚙️ Configuration

More Cultural Names Mod Builder is configured entirely via command-line arguments; it reads no configuration files and no environment variables.

### Command-Line Options

| Option | Value | Default | Description |
|--------|-------|---------|-------------|
| `--lang` | `<path>` | — | Path to the languages XML data store (required) |
| `--loc` | `<path>` | — | Path to the locations XML data store (required) |
| `--output` | `<path>` | — | Output directory where the mod will be written (required unless `--out` is used) |
| `--out` | `<path>` | — | Alias for `--output` (required unless `--output` is used) |
| `--id` | `<value>` | — | Mod identifier, e.g. `more-cultural-names` (required) |
| `--name` | `<value>` | — | Human-readable mod name (required) |
| `--version` | `<value>` | — | Mod version, e.g. `1.0.0` (required unless `--ver` is used) |
| `--ver` | `<value>` | — | Alias for `--version` (required unless `--version` is used) |
| `--game` | `<value>` | — | Target game: `CK2`, `CK3`, `HOI4`, or `IR` (required) |
| `--game-version` | `<value>` | — | Supported game version, e.g. `1.12.*` (required) |
| `--landed-titles` | `<path>` | — | Path to an existing landed titles file to patch (CK2/CK3 only) |
| `--landed-titles-name` | `<value>` | — | File name to use for the output landed titles file |
| `--dependency` | `<value>` | — | Mod ID that this mod depends on |
| `--dep` | `<value>` | — | Alias for `--dependency` |
| `--verbose` | `true` | `false` | Include verbose comments in the generated output |

## 🧩 Compatibility

| Component | Supported Versions | Notes |
|-----------|--------------------|-------|
| .NET runtime | 10.0 | Archives are published as self-contained builds; the .NET 10.0 SDK is required only to build from source |
| Crusader Kings II | All versions | Selected via the `CK2` game identifier |
| Crusader Kings III | All versions | Selected via the `CK3` game identifier |
| Hearts of Iron IV | All versions | Selected via the `HOI4` game identifier |
| Imperator: Rome | All versions | Selected via the `IR` game identifier |

## 🛡️ Privacy and Data

More Cultural Names Mod Builder handles no personal data; it runs entirely on the operator machine, reads caller-supplied XML data stores, and writes generated mod files to a caller-specified directory.

For the detailed description of how the application handles privacy and personal data, see the [Privacy Statement](./PRIVACY.md).

## 🛠️ Development

### Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Setup

```bash
git clone https://github.com/hmlendea/more-cultural-names-builder.git
cd more-cultural-names-builder
dotnet restore
```

### Build

```bash
dotnet build MoreCulturalNamesBuilder.slnx
```

### Run

```bash
dotnet run --project MoreCulturalNamesBuilder -- \
  --lang /path/to/languages.xml \
  --loc /path/to/locations.xml \
  --output /path/to/output \
  --id more-cultural-names \
  --name "More Cultural Names" \
  --version 1.0.0 \
  --game CK3 \
  --game-version "1.18.*"
```

### Test

```bash
dotnet test
```

### Integration Tests

Integration tests are part of the solution and run with `dotnet test`. They invoke the entry point with the XML data stores and landed titles files under `MoreCulturalNamesBuilder.IntegrationTests/TestData`, which are copied to the test output directory on build.

Run them on their own with:

```bash
dotnet test MoreCulturalNamesBuilder.IntegrationTests/MoreCulturalNamesBuilder.IntegrationTests.csproj
```

### Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Coverage reports are written to the `TestResults` directory of each test project.

### Linting

Code-style and analyser rules are enforced during compilation, with warnings treated as errors:

```bash
dotnet build MoreCulturalNamesBuilder.slnx
```

### Static Analysis

```bash
dotnet list package --vulnerable --include-transitive
```

### Debugging and Profiling

The verbose comment switch includes explanatory comments in the generated files:

```bash
dotnet run --project MoreCulturalNamesBuilder -- --verbose true --lang ... --loc ... --output ...
```

Build duration is reported to standard output for each phase, which is sufficient for profiling the data-retrieving and file-generation stages.

### Continuous Integration

The [.NET workflow](https://github.com/hmlendea/more-cultural-names-builder/blob/master/.github/workflows/dotnet.yml) restores the solution, builds it, and runs the test suite on every push and pull request targeting `master`. Reproduce the required checks locally with `dotnet build MoreCulturalNamesBuilder.slnx` and `dotnet test`.

### Release

Releases are published via the [GitHub Release workflow](https://github.com/hmlendea/more-cultural-names-builder/blob/master/.github/workflows/github-release.yml), which runs automatically when a release is published on GitHub:

1. Create and publish a GitHub release with a `v`-prefixed semantic version tag, for example `v7.1.9`.
2. The workflow resolves the .NET version from `MoreCulturalNamesBuilder/MoreCulturalNamesBuilder.csproj` and strips the `v` prefix from the tag to obtain the release version.
3. The workflow downloads and runs the shared release helper from `https://raw.githubusercontent.com/hmlendea/dotnet-release-scripts/refs/heads/master/dotnet/10.0.sh`, which publishes self-contained archives for `linux-arm`, `linux-arm64`, `linux-x64`, `osx-arm64`, `osx-x64`, `win-arm64`, and `win-x64` into `MoreCulturalNamesBuilder/bin/Release/`.
4. The workflow attaches every `MoreCulturalNamesBuilder/bin/Release/more-cultural-names-builder_*` archive to the published release.

Versions are taken exclusively from the GitHub release tag; the project file does not declare an assembly version.

**Note:** Piping into `bash` is an intensely controversial topic. Please review any external scripts before running them in your environment!

### Dependencies

| Package | Version | Scope | Purpose |
|---------|---------|-------|---------|
| `Microsoft.Extensions.Configuration` | 11.0.0 | Runtime | Configuration primitives |
| `Microsoft.Extensions.Configuration.Binder` | 11.0.0 | Runtime | Binding configuration values to objects |
| `Microsoft.Extensions.Configuration.Json` | 11.0.0 | Runtime | JSON configuration providers |
| `Microsoft.Extensions.DependencyInjection` | 11.0.0 | Runtime | Service composition for the console pipeline |
| `NuciCLI.Arguments` | 1.0.3 | Runtime | Command-line argument parsing |
| `NuciDAL` | 4.3.1 | Runtime | XML repository and entity base types |
| `NuciExtensions` | 6.0.3 | Runtime | Shared extension helpers |
| `NuciText.Conversion` | 1.0.3 | Runtime | Charset conversion used by the name normaliser |

## ⚙️ GitHub Actions

| Workflow | Purpose | What it does |
|----------|---------|--------------|
| [ .NET ](https://github.com/hmlendea/more-cultural-names-builder/blob/master/.github/workflows/dotnet.yml) | Compilation and test validation | Restores dependencies, builds the solution, and runs the test suite on every push and pull request targeting `master` |
| [ Security ](https://github.com/hmlendea/more-cultural-names-builder/blob/master/.github/workflows/security.yml) | Automated security scanning | Checks for vulnerable NuGet packages and runs CodeQL static analysis for C# on every push to `master`, pull request targeting `master`, and a weekly schedule |
| [ GitHub Release ](https://github.com/hmlendea/more-cultural-names-builder/blob/master/.github/workflows/github-release.yml) | Release publication | Builds the cross-platform release artefacts when a release is published and attaches them to the GitHub release |

## 🗂️ Project Structure

The solution comprises four projects:

### Projects and Packages

| Project | Type | Purpose |
|---------|------|---------|
| `MoreCulturalNamesBuilder` | Console Application | Generates mod files for supported games |
| `MoreCulturalNamesBuilder.UnitTests` | Test Suite | Validates configuration parsing, mappings, services, and builder output |
| `MoreCulturalNamesBuilder.IntegrationTests` | Test Suite | Runs the complete pipeline against repository fixtures and asserts every generated file |
| `MoreCulturalNamesBuilder.ArchitectureTests` | Test Suite | Enforces dependency direction, project conventions, and source conventions |

### Directories

| Directory | Purpose |
|-----------|---------|
| [Configuration](MoreCulturalNamesBuilder/Configuration/) | Settings and argument parsing |
| [Data Objects](MoreCulturalNamesBuilder/DataAccess/DataObjects/) | XML data object definitions |
| [Service](MoreCulturalNamesBuilder/Service/) | Business logic and transformations |
| [Mapping](MoreCulturalNamesBuilder/Service/Mapping/) | Data mapping between data objects and models |
| [Mod Builders](MoreCulturalNamesBuilder/Service/ModBuilders/) | Game-specific mod file generation |
| [Models](MoreCulturalNamesBuilder/Service/Models/) | Application domain models |

## 🏗️ Architecture

More Cultural Names Builder uses a console pipeline with XML data objects, localisation services, and game-specific mod builders.

See the [Architecture Documentation](./ARCHITECTURE.md) for the system context, principal components, runtime flows, ownership boundaries, dependencies, constraints, and extension points.

## 📚 Documentation

Additional technical documentation is available in the `docs/` directory.

| Resource | Description |
|----------|-------------|
| [Documentation Index](docs/index.md) | Entry point to the technical documentation set |
| [Architecture Overview](docs/architecture-overview.md) | Layered decomposition, dependency rules, and runtime sequences |
| [Configuration System](docs/configuration-system.md) | Argument parsing, validation, and settings resolution |
| [Data Access Layer](docs/data-access-layer.md) | XML repositories, entities, and data loading |
| [Service Layer](docs/service-layer.md) | Localisation retrieving and name normalisation |
| [Mod Builders](docs/mod-builders.md) | Game-specific output formats and generation logic |
| [Data Models](docs/data-models.md) | Domain models and entity mappings |
| [Name Normalisation](docs/name-normalisation.md) | Character-set transformation rules per game |
| [Testing](docs/testing.md) | Test structure, coverage, and verification strategy |
| [Deployment and Operations](docs/deployment-operations.md) | Build, release, execution, and troubleshooting characteristics |

## 🧾 Data Formats and Protocols

| Format or Protocol | Specification | Purpose |
|--------------------|---------------|---------|
| Language and location XML | Defined by the data objects in [DataAccess/DataObjects](MoreCulturalNamesBuilder/DataAccess/DataObjects/) | Input data stores supplied via `--lang` and `--loc` |
| Landed titles script | Game-specific format patched via `--landed-titles` (CK2 and CK3) | Source file whose cultural names are merged and rewritten |
| Mod descriptor | Game-specific descriptor file | Identifies and configures the generated mod |
| Localisation files | Game-specific localisation format | Maps locations to cultural names per language |
| Province name data | Imperator: Rome province-name format | Supplies province names per culture |

## 🩺 Troubleshooting

| Symptom | Probable Cause | Resolution |
|---------|----------------|------------|
| `FileNotFoundException` | Incorrect XML path | Verify the `--lang` and `--loc` paths |
| `XmlException` | Malformed XML input | Validate the input against the expected schema |
| `ArgumentException` | Absent required argument | Supply every required argument |
| `NotImplementedException` | Unsupported game | Use one of `CK2`, `CK3`, `HOI4`, or `IR` |
| Output directory not created | The application creates no directories | Create the output directory prior to execution |

## 🤝 Contributing

You are welcome to submit any suggestion, response, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Preserve the existing public contract unless a breaking change is intentional
- Submit focused pull requests that conform to the existing code style
- Maintain your branch synchronised with `master`
- Revise the documentation when functionality changes
- Properly test all modifications, including edge cases and error conditions
- Include tests for additional or modified functionality
- Raise a new [issue](https://github.com/hmlendea/more-cultural-names-builder/issues) for problems or suggestions

See the [Contributing Guidelines](./CONTRIBUTING.md) for details on how to report issues, suggest enhancements, and submit changes.

## 📜 Code of Conduct

See the [Code of Conduct](./CODE_OF_CONDUCT.md) for the standards of conduct expected in all project spaces.

## 🔒 Security

More Cultural Names Mod Builder runs locally without network access and manages no credentials, personal data, or secrets.

For information on reporting security vulnerabilities, see the [Security Policy](./SECURITY.md).

## 💬 Support

For support, please [open an issue](https://github.com/hmlendea/more-cultural-names-builder/issues).

## 💝 Project Engagement

Discovered a problem with More Cultural Names Mod Builder or have a suggestion for it? [Open an issue](https://github.com/hmlendea/more-cultural-names-builder/issues)!

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding) or starring ⭐️ it on GitHub!

[![Donate](https://raw.githubusercontent.com/hmlendea/readme-assets/master/donate_generic.png)](https://hmlendea.go.ro/funding)

## 📄 License

This project is being distributed under the `GNU General Public License v3.0` or later.
See [LICENSE](./LICENSE) for additional information.
