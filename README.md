[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/more-cultural-names-builder)](https://github.com/hmlendea/more-cultural-names-builder/releases/latest)
[![Build Status](https://github.com/hmlendea/more-cultural-names-builder/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/more-cultural-names-builder/actions/workflows/dotnet.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://gnu.org/licenses/gpl-3.0)

# More Cultural Names Mod Builder

A CLI tool that generates game mods which localise place names into different languages based on the faction controlling each location.

## Supported Games

| Game | `--game` value |
|---|---|
| Crusader Kings II | `CK2` |
| Crusader Kings III | `CK3` |
| Hearts of Iron IV | `HOI4` |
| Imperator: Rome | `IR` |

## Requirements

- .NET SDK/runtime with support for `net10.0`

## Usage

```
MoreCulturalNamesBuilder \
  --lang <path>          \
  --loc <path>           \
  --output <path>        \
  --id <mod-id>          \
  --name <mod-name>      \
  --version <mod-version>\
  --game <game>          \
  --game-version <ver>   \
  [--landed-titles <path>]       \
  [--landed-titles-name <name>]  \
  [--dependency <mod-id>]        \
  [--verbose true]
```

### Arguments

| Argument | Required | Description |
|---|---|---|
| `--lang <path>` | Yes | Path to the languages XML data store |
| `--loc <path>` | Yes | Path to the locations XML data store |
| `--output <path>` | Yes | Output directory where the mod will be written |
| `--id <value>` | Yes | Mod identifier (e.g. `more-cultural-names`) |
| `--name <value>` | Yes | Human-readable mod name |
| `--version <value>` | Yes | Mod version (e.g. `1.0.0`) |
| `--game <value>` | Yes | Target game (see table above) |
| `--game-version <value>` | Yes | Supported game version (e.g. `1.12.*`) |
| `--landed-titles <path>` | No | Path to an existing landed titles file to patch (CK2/CK3) |
| `--landed-titles-name <value>` | No | File name to use for the output landed titles file |
| `--dependency <value>` | No | Mod ID this mod depends on |
| `--verbose true` | No | Include verbose comments in the generated output |

### Example

```bash
dotnet run -- \
  --lang /data/languages.xml \
  --loc /data/locations.xml \
  --output /mods/more-cultural-names \
  --id more-cultural-names \
  --name "More Cultural Names" \
  --version 1.0.0 \
  --game CK3 \
  --game-version "1.12.*"
```

## Development

### Build

```bash
dotnet build
```

### test

```bash
dotnet test
```

### Release

The repository includes `release.sh`, which delegates to the upstream deployment script used by the project maintainer.

```bash
bash ./release.sh 1.0.0
```

This script downloads and executes an external release helper from: `https://raw.githubusercontent.com/hmlendea/deployment-scripts/master/release/dotnet/10.0.sh`

**Note:** Piping into `bash` is an intensely controversial topic. Please review any external scripts before running them in your environment!

## Contributing

Contributions are welcome. Please:
- Keep changes cross-platform
- Keep the existing public API intact unless a breaking change is intentional
- Keep pull requests focused and consistent with the existing code style
- Update documentation when behaviour changes

## Support

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding).

## License

Licensed under the GNU General Public License v3.0 or later.
See [LICENSE](./LICENSE) for details.
