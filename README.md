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
- [Command Reference](#command-reference)
- [Configuration](#configuration)
  - [Command-Line Options](#command-line-options)
- [Compatibility](#compatibility)
- [Development](#development)
  - [Requirements](#requirements)
  - [Build](#build)
  - [Test](#test)
  - [Release](#release)
- [GitHub Actions](#github-actions)
- [Project Structure](#project-structure)
  - [Projects and Packages](#projects-and-packages)
  - [Directories](#directories)
- [Architecture](#architecture)
- [Security](#security)
- [Contributing](#contributing)
- [Privacy](#privacy)
- [Project Engagement](#project-engagement)
- [License](#license)

## ✨ Capabilities

Generates localised mod files for multiple game engines:

- **Crusader Kings II** (via `CK2`)
- **Crusader Kings III** (via `CK3`)
- **Hearts of Iron IV** (via `HOI4`)
- **Imperator: Rome** (via `IR`)

## 🚀 Usage

Basic usage:

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

## ⌨️ Command Reference

| Command | Description |
|---------|-------------|
| `./MoreCulturalNamesBuilder` | Generate mod files based on language and location data |

## ⚙️ Configuration

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
|-----------|-------------------|-------|
| .NET Runtime | 10.0+ | Requires .NET 10.0 SDK or compatible runtime |
| Crusader Kings II | All versions | Via `CK2` game identifier |
| Crusader Kings III | All versions | Via `CK3` game identifier |
| Hearts of Iron IV | All versions | Via `HOI4` game identifier |
| Imperator: Rome | All versions | Via `IR` game identifier |

## 🛠️ Development

### Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Build

```bash
dotnet build
```

### Test

```bash
dotnet test
```

### Release

Releases are published through the [GitHub Release workflow](https://github.com/hmlendea/more-cultural-names-builder/blob/master/.github/workflows/github-release.yml), which runs automatically when a release is published on GitHub:

1. Create and publish a GitHub release with a `v`-prefixed semantic version tag, for example `v7.1.5`.
2. The workflow resolves the .NET version from `MoreCulturalNamesBuilder/MoreCulturalNamesBuilder.csproj` and strips the `v` prefix from the tag to obtain the release version.
3. The workflow downloads and runs the shared release helper from `https://raw.githubusercontent.com/hmlendea/dotnet-release-scripts/refs/heads/master/dotnet/10.0.sh`, which publishes self-contained archives for `linux-arm`, `linux-arm64`, `linux-x64`, `osx-arm64`, `osx-x64`, `win-arm64`, and `win-x64` into `MoreCulturalNamesBuilder/bin/Release/`.
4. The workflow attaches every `MoreCulturalNamesBuilder/bin/Release/more-cultural-names-builder_*` archive to the published release.

Versions are taken exclusively from the GitHub release tag; the project file does not declare an assembly version.

**Note:** Piping into `bash` is an intensely controversial topic. Please review any external scripts before running them in your environment!

## ⚙️ GitHub Actions

| Workflow | Purpose | What it does |
|----------|---------|--------------|
| [ .NET ](https://github.com/hmlendea/more-cultural-names-builder/blob/master/.github/workflows/dotnet.yml) | Build and test validation | Restores dependencies, builds the solution, and runs the unit test suite on every push and pull request targeting `master` |
| [ Security ](https://github.com/hmlendea/more-cultural-names-builder/blob/master/.github/workflows/security.yml) | Automated security scanning | Checks for vulnerable NuGet packages and runs CodeQL static analysis for C# on every push to `master`, pull request targeting `master`, and a weekly schedule |
| [ GitHub Release ](https://github.com/hmlendea/more-cultural-names-builder/blob/master/.github/workflows/github-release.yml) | Release publication | Builds the cross-platform release artefacts when a release is published and attaches them to the GitHub release |

## 🗂️ Project Structure

The solution comprises two projects:

### Projects and Packages

| Project | Type | Purpose |
|---------|------|---------|
| `MoreCulturalNamesBuilder` | Console Application | Generates mod files for supported games |
| `MoreCulturalNamesBuilder.UnitTests` | Unit Test Suite | Validates configuration parsing and data transformations |

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

See the [architecture documentation](./ARCHITECTURE.md) for the system context, principal components, runtime flows, ownership boundaries, dependencies, constraints, and extension points.

## 🤝 Contributing

You are welcome to submit any suggestion, feedback, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Preserve the existing public contract unless a breaking change is intentional
- Submit focused pull requests that conform to the existing code style
- Maintain your branch synchronised with `master`
- Revise the documentation when functionality changes
- Properly test all modifications, including edge cases and error conditions
- Include tests for additional or modified functionality

See the [contributing guidelines](CONTRIBUTING.md) for details on how to report issues, suggest enhancements, and submit changes.

## 📜 Code of Conduct

See the [Code of Conduct](CODE_OF_CONDUCT.md) for the standards of conduct expected in all project spaces.

## 🔒 Security

This project uses automated security scanning via GitHub Actions:

- **Security Scan Workflow** (`.github/workflows/security.yml`) runs on every push, pull request, and weekly schedule
- **Vulnerable Package Detection** via `dotnet list package --vulnerable --include-transitive`
- **CodeQL Static Analysis** for C# code security issues

See [SECURITY.md](./SECURITY.md) for the full security policy, supported versions, and vulnerability reporting procedures.

## 🔒 Privacy

See [PRIVACY.md](./PRIVACY.md) for the data handling practices of this application.

## 💝 Project Engagement

Discovered a problem or have a suggestion? [Open an issue](https://github.com/hmlendea/more-cultural-names-builder/issues)!

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding) or starring ⭐️ it on GitHub!

[![Donate](https://raw.githubusercontent.com/hmlendea/readme-assets/master/donate_generic.png)](https://hmlendea.go.ro/funding)

## 📄 License

This project is being distributed under the `GNU General Public License v3.0` or later.
See [LICENSE](./LICENSE) for further information.
