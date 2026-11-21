# Data Access Layer

This document describes the XML repository integration, entity definitions, and data loading mechanisms.

## Overview

The data access layer employs `NuciDAL` (a custom XML repository library) to deserialise user-provided XML files into domain entity objects. The layer is deliberately passive - it performs no business logic, only data loading and entity materialisation.

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
- Loads the entire XML file into memory on the first `GetAll()` call
- Uses XML serialisation attributes on entity classes for mapping
- No streaming or pagination - the full data set is loaded at startup

## Entity Definitions

All entities inherit from `NuciDAL.DataObjects.EntityBase` (which provides the `Id` property).

### LanguageEntity
**Location:** [LanguageDataObject.cs](../MoreCulturalNamesBuilder/DataAccess/DataObjects/LanguageDataObject.cs)

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
**Location:** [LocationDataObject.cs](../MoreCulturalNamesBuilder/DataAccess/DataObjects/LocationDataObject.cs)

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
**Location:** [NameDataObject.cs](../MoreCulturalNamesBuilder/DataAccess/DataObjects/NameDataObject.cs)

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
**Location:** [LanguageCodeDataObject.cs](../MoreCulturalNamesBuilder/DataAccess/DataObjects/LanguageCodeDataObject.cs)

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
**Location:** [GameIdDataObject.cs](../MoreCulturalNamesBuilder/DataAccess/DataObjects/GameIdDataObject.cs)

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
- `Game` - Target game identifier (CK2, CK3, EU5, HOI4, IR)
- `Type` - Entity type within the game (e.g., "Province", "State", "City", "Language")
- `Parent` - Hierarchical parent identifier (optional)
- `DefaultNameLanguageId` - Language identifier for the default name
- `Id` (XmlText) - The game-specific identifier itself

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

**Location:** [Mapping](../MoreCulturalNamesBuilder/Service/Mapping/)

Each entity has a corresponding mapping extension:

| Entity | Mapping | Service Model |
|--------|---------|---------------|
| `LanguageEntity` | `LanguageMapping.ToServiceModel()` | `Language` |
| `LocationEntity` | `LocationMapping.ToServiceModel()` | `Location` |
| `NameEntity` | `NameMapping.ToServiceModel()` | `Name` |
| `GameIdEntity` | `GameIdMapping.ToServiceModel()` | `GameId` |
| `LanguageCodeEntity` | `LanguageCodeMapping.ToServiceModel()` | `LanguageCode` |

### Mapping Configuration
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

## Data Loading in the Builders

### Base ModBuilder.LoadAllData()
**Location:** [ModBuilder.cs](../MoreCulturalNamesBuilder/Service/ModBuilders/ModBuilder.cs)

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

    InitializeOrderedLocalisations();

    locationGameIdsById = locationGameIds
        .GroupBy(gameId => gameId.Id)
        .ToDictionary(group => group.Key, group => group.First());
}
```

`InitializeOrderedLocalisations()` populates `localisationsOrderedByLanguageGameId` for CK2. `CK3ModBuilder` overrides it with an empty dictionary, since CK3 resolves title names through its own canonical index instead.

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

#### Europa Universalis V (EuropaUniversalis5ModBuilder.LoadData)
```csharp
protected override void LoadData()
{
    ConcurrentDictionary<string, IDictionary<string, Localisation>> concurrentLocalisations = [];
    ConcurrentDictionary<string, string> concurrentDefaultLanguages = [];

    locationIds = [.. locationGameIds
        .Where(gameId => !string.IsNullOrWhiteSpace(gameId.Id))
        .Select(gameId => gameId.Id)
        .Distinct(StringComparer.Ordinal)
        .OrderBy(locationId => locationId, StringComparer.Ordinal)];

    Parallel.ForEach(locationIds, locationId =>
    {
        concurrentLocalisations[locationId] = localisationFetcher
            .GetGameLocationLocalisations(locationId, Settings.Mod.Game)
            .ToDictionary(localisation => localisation.LanguageGameId, localisation => localisation);

        string? defaultLanguageId = locationGameIds
            .FirstOrDefault(gameId => string.Equals(gameId.Id, locationId, StringComparison.Ordinal))
            ?.DefaultNameLanguageId;

        if (!string.IsNullOrWhiteSpace(defaultLanguageId))
        {
            concurrentDefaultLanguages[locationId] = defaultLanguageId;
        }
    });

    localisationsByLocationId = concurrentLocalisations;
    defaultLanguageIdsByLocationId = concurrentDefaultLanguages;
}
```

## Parallel Processing

All builders use `Parallel.ForEach` for localisation retrieval:
- **Thread Safety**: `ConcurrentDictionary` used for intermediate storage
- **Performance**: Parallelises I/O-bound localisation lookups throughout locations
- **Ordering**: Results are sorted after parallel completion for deterministic output

## Error Handling

### Repository Errors
- `FileNotFoundException` - XML file not found at the specified path
- `XmlException` - Malformed XML or a deserialisation mismatch
- `InvalidOperationException` - `EntityBase.Id` absent or duplicated

### Propagation
All repository exceptions propagate to `Program.Main` → unhandled → non-zero exit code

### No Retry Logic
- No automatic retry on transient failures
- No partial load recovery
- Fail-fast philosophy consistent with the console pipeline architecture

## Data Invariants

### Entity Immutability
- Entities are read-only after repository loading
- No setters exposed beyond XML deserialisation
- Mutations occur only in mapping and service layers

### ID Uniqueness
- `EntityBase.Id` must be unique within each repository
- `GameIdEntity.Id` unique per (Game, Type) combination
- Duplicate identifiers cause undefined behaviour (the last write takes precedence in the dictionary)

### Fallback Chains
- `LanguageEntity.FallbackLanguages` - ordered list of language identifiers to attempt
- `LocationEntity.FallbackLocations` - ordered list of location identifiers to attempt
- Used by `LocalisationFetcher` for localisation resolution

## Testing

**Location:** [Mapping Tests](../MoreCulturalNamesBuilder.UnitTests/Service/Mapping/)

### Current Coverage
- Mapping extension method correctness (entity ↔ model round-trip)
- Collection mapping (ToServiceModels)

### Gaps
- XML deserialisation edge cases (absent attributes, malformed data)
- Large file performance
- Concurrent repository access
- Fallback chain resolution

## Extension Points

To include new entity types:
1. Create the entity class in `DataAccess/DataObjects/` with XML attributes
2. Include a mapping extension in [Mapping](../MoreCulturalNamesBuilder/Service/Mapping/).
3. Register the repository in the `Program.cs` dependency injection container
4. Update `LocalisationFetcher` when the new data participates in localisation
5. Revise the builders when the new data participates in output generation

## Related Documentation
- [Architecture Overview](architecture-overview.md) - The data access layer in context
- [Service Layer](service-layer.md) - How entities are transformed
- [Localisation Fetcher](service-layer.md#localisationfetcher) - Consumes loaded entities
- [Mod Builders](mod-builders.md) - Game-specific data loading