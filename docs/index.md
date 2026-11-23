# More Cultural Names Builder - Documentation

This directory contains comprehensive technical documentation for the More Cultural Names Builder project.

## Documentation Structure

| Document | Description |
|----------|-------------|
| [Architecture Overview](architecture-overview.md) | High-level architectural decomposition, runtime sequences, and component relationships |
| [Configuration System](configuration-system.md) | CLI argument parsing, validation, and settings management |
| [Data Access Layer](data-access-layer.md) | XML repository integration, entity definitions, and data loading |
| [Service Layer](service-layer.md) | Localisation retrieval, name normalisation, and data transformation |
| [Mod Builders](mod-builders.md) | Game-specific mod generation for CK2, CK3, EU5, HOI4, Imperator: Rome, and Victoria 3 |
| [Data Models](data-models.md) | Domain models, entity mappings, and data flow transformations |
| [Name Normalisation](name-normalisation.md) | Linguistic transformation rules per game target |
| [Testing](testing.md) | Test coverage, gaps, and verification strategies |
| [Deployment & Operations](deployment-operations.md) | Build, release, and execution characteristics |

## Quick Reference

### Supported Games
- **Crusader Kings II** (CK2) - Generates `.mod` descriptor, landed titles patches, and CSV localisation
- **Crusader Kings III** (CK3) - Generates `.mod` descriptor, merged cultural name blocks, and YAML localisation
- **Europa Universalis V** (EU5) - Generates `.metadata` launcher metadata and language-suffixed location name localisation
- **Hearts of Iron IV** (HOI4) - Generates `.mod` descriptor and YAML localisation for states/cities
- **Imperator: Rome** (IR) - Generates `.mod` descriptor, province name data files, and YAML localisation
- **Victoria 3** (Vic3) - Generates `.metadata` launcher metadata, dynamic state and hub name effects, and YAML localisation

### Key Entry Points
- **Program.cs** - Application entry point, dependency injection container configuration, builder factory invocation
- **Settings.cs** - CLI argument parsing and validation
- **ModBuilderFactory.cs** - Runtime game-specific builder selection
- **IModBuilder.Build()** - Main build pipeline entry point

### Core Interfaces
- `ILocalisationFetcher` - Retrieves localised names for locations across languages
- `INameNormaliser` - Applies game-specific character set transformations
- `IModBuilder` - Game-specific mod file generation contract
- `IModBuilderFactory` - Selects appropriate builder based on `--game` argument

### Data Flow Summary
```
CLI Args → Settings → DI Container → ModBuilderFactory → Game-Specific Builder
                                                              ↓
XML Repositories → Entities → Service Models → LocalisationFetcher/NameNormaliser
                                                              ↓
                                                    Generated Mod Files
```