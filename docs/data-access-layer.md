# Data Access Layer

This document describes the XML repository integration, entity definitions, and data loading mechanisms.

## Overview

The data access layer uses `NuciDAL` (a custom XML repository library) to deserialize user-provided XML files into domain entity objects. The layer is deliberately passive - it performs no business logic, only data loading and entity materialisation.

## Repository Integration

### DI Registration (Program.cs)

```csharp
.AddSingleton<IFileRepository<LanguageEntity>>(
    s => new XmlRepository<LanguageEntity>(settings.Input.LanguageStorePath))
.AddSingleton<IFileRepository<LocationEntity>>(
    s => new XmlRepository<LocationEntity>(settings.Input.LocationStorePath))
```

### Repository Interface
```csharp
public interface IFileRepository<T> where T : EntityBase
{
    IEnumerable<T> GetAll();
}
```

### Implementation
- `XmlRepository<T>` from `NuciDAL.Repositories`
- Loads entire XML file into memory on first `GetAll()` call
- Uses XML serialization attributes on entity classes for mapping
- No streaming or pagination - full dataset loaded at startup

## Entity Definitions

All entities inherit from `NuciDAL.DataObjects.EntityBase` (provides `Id` property).

### LanguageEntity
**Location:** `MoreCulturalNamesBuilder/DataAccess/DataObjects/LanguageEntity.cs`

```csharp
[XmlType("Language")]
public class LanguageEntity : EntityBase
{
    public LanguageCodeEntity Code { get; set; }
    public List<GameIdEntity> GameIds { get; set; }
    [XmlArrayItem(ElementName="LanguageId")]
    public List<string> FallbackLanguages { get; set; }
}
```

**XML Structure:**
```xml
<Language id="eng">
  <Code iso-639-1="en" iso-639-2="eng" iso-639-3="eng" />
  <GameIds>
    <GameId game="CK3" type="Language" defaultLanguage="english">english</GameId>
  </GameIds>
  <FallbackLanguages>
    <LanguageId>eng</LanguageId>
  </FallbackLanguages>
</Language>
```

### LocationEntity
**Location:** `MoreCulturalNamesBuilder/DataAccess/DataObjects/LocationEntity.cs`

```csharp
public class LocationEntity : EntityBase
{
    public string GeoNamesId { get; set; }
    [XmlArrayItem("LocationId")]
    public List<string> FallbackLocations { get; set; }
    public List<GameIdEntity> GameIds { get; set; }
    public List<NameEntity> Names { get; set; }
}
```

**XML Structure:**
```xml
<Location id="paris">
  <GeoNamesId>2988507</GeoNamesId>
  <FallbackLocations>
    <LocationId>paris_fallback</LocationId>
  </FallbackLocations>
  <GameIds>
    <GameId game="CK3" type="Province" defaultLanguage="english">c_paris</GameId>
    <GameId game="HOI4" type="State" defaultLanguage="english">123</GameId>
    <GameId game="HOI4" type="City" defaultLanguage="english">456</GameId>
  </GameIds>
  <Names>
    <Name language="eng" value="Paris" adjective="Parisian" comment="Capital of France" />
    <Name language="fra" value="Paris" adjective="Parisien" />
  </Names>
</Location>
```

### NameEntity
**Location:** `MoreCulturalNamesBuilder/DataAccess/DataObjects/NameEntity.cs`

```csharp
[XmlType("Name")]
public class NameEntity
{
    [XmlAttribute("language")]
    public string LanguageId { get; set; }

    [XmlAttribute("value")]
    public string Value { get; set; }

    [XmlAttribute("adjective")]
    public string Adjective { get; set; }

    [XmlAttribute("comment")]
    public string Comment { get; set; }
}
```

### LanguageCodeEntity
**Location:** `MoreCulturalNamesBuilder/DataAccess/DataObjects/LanguageCodeEntity.cs`

```csharp
[XmlType("LanguageCode")]
public class LanguageCodeEntity
{
    [XmlAttribute("iso-639-1")]
    public string ISO_639_1 { get; set; }
    [XmlAttribute("iso-639-2")]
    public string ISO_639_2 { get; set; }
    [XmlAttribute("iso-639-3")]
    public string ISO_639_3 { get; set; }
}
```

### GameIdEntity
**Location:** `MoreCulturalNamesBuilder/DataAccess/DataObjects/GameIdEntity.cs`

```csharp
[XmlType("GameId")]
public class GameIdEntity
{
    [XmlAttribute("game")]
    public string Game { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; }

    [XmlAttribute("parent")]
    public string Parent { get; set; }

    [XmlAttribute("defaultLanguage")]
    public string DefaultNameLanguageId { get; set; }

    [XmlText]
    public string Id { get; set; }
}
```

**Key Attributes:**
- `Game` - Target game identifier (CK2, CK3, HOI4, IR)
- `Type` - Entity type within game (e.g., "Province", "State", "City", "Language")
- `Parent` - Hierarchical parent ID (optional)
- `DefaultNameLanguageId` - Language ID for default name
- `Id` (XmlText) - The actual game-specific identifier

## Data Loading Flow

```
User XML Files (--lang, --loc)
        │
        ▼
XmlRepository<LanguageEntity>.GetAll()
XmlRepository<LocationEntity>.GetAll()
        │
        ▼
LanguageEntity[] , LocationEntity[] (in-memory collections)
        │
        ▼
Mapping Extensions (.ToServiceModels())
        │
        ▼
Language[] , Location[] (Service Models)
        │
        ▼
Consumed by LocalisationFetcher, ModBuilders
```

## Mapping to Service Models

**Location:** `MoreCulturalNamesBuilder/Service/Mapping/`

Each entity has a corresponding mapping extension:

| Entity | Mapping | Service Model |
|--------|---------|---------------|
| `LanguageEntity` | `LanguageMapping.ToServiceModel()` | `Language` |
| `LocationEntity` | `LocationMapping.ToServiceModel()` | `Location` |
| `NameEntity` | `NameMapping.ToServiceModel()` | `Name` |
| `GameIdEntity` | `GameIdMapping.ToServiceModel()` | `GameId` |
| `LanguageCodeEntity` | `LanguageCodeMapping.ToServiceModel()` | `LanguageCode` |

### Mapping Pattern
```csharp
static class LanguageMapping
{
    internal static Language ToServiceModel(this LanguageEntity dataObject) => new()
    {
        Id = dataObject.Id,
        Code = dataObject.Code?.ToServiceModel(),
        GameIds = dataObject.GameIds.ToServiceModels(),
        FallbackLanguages = dataObject.FallbackLanguages.ToList()
    };

    internal static IEnumerable<Language> ToServiceModels(this IEnumerable<LanguageEntity> dataObjects)
        => dataObjects.Select(dataObject => dataObject.ToServiceModel());
}
```

## Data Loading in Builders

### Base ModBuilder.LoadAllData()
**Location:** `MoreCulturalNamesBuilder/Service/ModBuilders/ModBuilder.cs`

```csharp
void LoadAllData()
{
    locations = locationRepository
        .GetAll()
        .ToServiceModels()
        .ToDictionary(key => key.Id, val => val);

    languages = languageRepository
        .GetAll()
        .ToServiceModels()
        .ToDictionary(key => key.Id, val => val);

    locationGameIds = locations.Values
        .SelectMany(x => x.GameIds)
        .Where(x => x.Game == Settings.Mod.Game)
        .OrderBy(x => x.Id)
        .ToList();

    languageGameIds = languages.Values
        .SelectMany(x => x.GameIds)
        .Where(x => x.Game == Settings.Mod.Game)
        .OrderBy(x => x.Id)
        .ToList();

    LoadData();  // Game-specific additional loading
}
```

### Game-Specific Loading

#### CK2/CK3 (CK2ModBuilder.LoadData)
```csharp
protected override void LoadData()
{
    ConcurrentDictionary<string, IEnumerable<Localisation>> concurrentLocalisations = new();

    Parallel.ForEach(locationGameIds, locationGameId =>
    {
        IEnumerable<Localisation> locationLocalisations =
            localisationFetcher.GetGameLocationLocalisations(locationGameId.Id, Settings.Mod.Game);
        concurrentLocalisations.TryAdd(locationGameId.Id, locationLocalisations);
    });

    localisations = concurrentLocalisations.ToDictionary(x => x.Key, x => x.Value);
    localisationsOrderedByLanguageGameId = localisations
        .ToDictionary(
            localisationsByGameId => localisationsByGameId.Key,
            localisationsByGameId =>
                (IEnumerable<Localisation>)localisationsByGameId.Value
                    .OrderBy(localisation => localisation.LanguageGameId)
                    .ToArray());

    locationGameIdsById = locationGameIds
        .GroupBy(gameId => gameId.Id)
        .ToDictionary(group => group.Key, group => group.First());
}
```

#### HOI4 (HOI4ModBuilder.LoadData)
```csharp
protected override void LoadData()
{
    stateGameIds = locations.Values
        .SelectMany(x => x.GameIds)
        .Where(x => x.Game == Settings.Mod.Game && x.Type.Equals("State"))
        .OrderBy(x => int.Parse(x.Id));

    cityGameIds = locations.Values
        .SelectMany(x => x.GameIds)
        .Where(x => x.Game == Settings.Mod.Game && x.Type.Equals("City"))
        .OrderBy(x => int.Parse(x.Id));

    stateLocalisations = new ConcurrentDictionary<string, IDictionary<string, Localisation>>();
    cityLocalisations = new ConcurrentDictionary<string, IDictionary<string, Localisation>>();

    Parallel.ForEach(stateGameIds, stateGameId =>
    {
        IDictionary<string, Localisation> localisations = localisationFetcher
            .GetGameLocationLocalisations(stateGameId.Id, "State", Settings.Mod.Game)
            .ToDictionary(x => x.LanguageGameId, x => x);
        stateLocalisations.Add(stateGameId.Id, localisations);
    });

    Parallel.ForEach(cityGameIds, cityGameId =>
    {
        IDictionary<string, Localisation> localisations = localisationFetcher
            .GetGameLocationLocalisations(cityGameId.Id, "City", Settings.Mod.Game)
            .ToDictionary(x => x.LanguageGameId, x => x);
        cityLocalisations.Add(cityGameId.Id, localisations);
    });
}
```

#### Imperator: Rome (ImperatorRomeModBuilder.LoadData)
```csharp
protected override void LoadData()
{
    ConcurrentDictionary<string, IDictionary<string, Localisation>> concurrentLocalisations = new();

    Parallel.ForEach(locationGameIds, locationGameId =>
    {
        IDictionary<string, Localisation> locationLocalisations = localisationFetcher
            .GetGameLocationLocalisations(locationGameId.Id, Settings.Mod.Game)
            .ToDictionary(x => x.LanguageGameId, x => x);
        concurrentLocalisations.TryAdd(locationGameId.Id, locationLocalisations);
    });

    localisations = concurrentLocalisations.ToDictionary(x => x.Key, x => x.Value);
    locationGameIdsById = locationGameIds
        .GroupBy(gameId => gameId.Id)
        .ToDictionary(group => group.Key, group => group.First());
}
```

## Parallel Processing

All builders use `Parallel.ForEach` for localisation fetching:
- **Thread Safety**: `ConcurrentDictionary` used for intermediate storage
- **Performance**: Parallelises I/O-bound localisation lookups across locations
- **Ordering**: Results sorted after parallel completion for deterministic output

## Error Handling

### Repository Errors
- `FileNotFoundException` - XML file not found at specified path
- `XmlException` - Malformed XML or deserialization mismatch
- `InvalidOperationException` - EntityBase.Id missing or duplicate

### Propagation
All repository exceptions bubble up to `Program.Main` → unhandled → non-zero exit code

### No Retry Logic
- No automatic retry on transient failures
- No partial load recovery
- Fail-fast philosophy consistent with console pipeline architecture

## Data Invariants

### Entity Immutability
- Entities are read-only after repository load
- No setters exposed beyond XML deserialization
- Mutations only in mapping/service layers

### ID Uniqueness
- `EntityBase.Id` must be unique within each repository
- `GameIdEntity.Id` unique per (Game, Type) combination
- Duplicate IDs cause undefined behaviour (last write wins in dictionary)

### Fallback Chains
- `LanguageEntity.FallbackLanguages` - ordered list of language IDs to try
- `LocationEntity.FallbackLocations` - ordered list of location IDs to try
- Used by `LocalisationFetcher` for localisation resolution

## Testing

**Location:** `MoreCulturalNamesBuilder.UnitTests/Service/Mapping/`

### Current Coverage
- Mapping extension method correctness (entity ↔ model round-trip)
- Collection mapping (ToServiceModels)

### Gaps
- XML deserialization edge cases (missing attributes, malformed data)
- Large file performance
- Concurrent repository access
- Fallback chain resolution

## Extension Points

To add new entity types:
1. Create entity class in `DataAccess/DataObjects/` with XML attributes
2. Add mapping extension in `Service/Mapping/`
3. Register repository in `Program.cs` DI container
4. Update `LocalisationFetcher` if new data needed for localisation
5. Update builders if new data needed for output generation

## Related Documentation
- [Architecture Overview](architecture-overview.md) - Data access in context
- [Service Layer](service-layer.md) - How entities are transformed
- [Localisation Fetcher](service-layer.md#localisationfetcher) - Consumes loaded entities
- [Mod Builders](mod-builders.md) - Game-specific data loading