# Data Models

This document describes the domain models, entity mappings, and data flow transformations in the More Cultural Names Builder.

## Model Hierarchy

```
Data Access Entities (XML)          Service Models (Application)
┌─────────────────────────┐         ┌─────────────────────────┐
│ LanguageEntity          │  ──►    │ Language                │
│ LocationEntity          │  ──►    │ Location                │
│ NameEntity              │  ──►    │ Name                    │
│ GameIdEntity            │  ──►    │ GameId                  │
│ LanguageCodeEntity      │  ──►    │ LanguageCode            │
└─────────────────────────┘         └─────────────────────────┘
                                              │
                                              ▼
                                    ┌─────────────────────────┐
                                    │ Localisation            │
                                    │ (Derived/Computed)      │
                                    └─────────────────────────┘
```

## Data Access Entities

**Location:** `MoreCulturalNamesBuilder/DataAccess/DataObjects/`

### LanguageEntity

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

| Property | XML Source | Description |
|----------|------------|-------------|
| `Id` (inherited) | `id` attribute | Unique language identifier (e.g., `eng`, `fra`) |
| `Code` | `<Code>` element | ISO language codes |
| `GameIds` | `<GameIds><GameId>...</GameId></GameIds>` | Game-specific language identifiers |
| `FallbackLanguages` | `<FallbackLanguages><LanguageId>...</LanguageId></FallbackLanguages>` | Fallback chain for localisation |

### LocationEntity

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

| Property | XML Source | Description |
|----------|------------|-------------|
| `Id` (inherited) | `id` attribute | Unique location identifier (e.g., `paris`, `london`) |
| `GeoNamesId` | `<GeoNamesId>` element | GeoNames database reference |
| `FallbackLocations` | `<FallbackLocations><LocationId>...</LocationId></FallbackLocations>` | Fallback chain for localisation |
| `GameIds` | `<GameIds><GameId>...</GameId></GameIds>` | Game-specific location identifiers |
| `Names` | `<Names><Name>...</Name></Names>` | Localised names per language |

### NameEntity

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

| Property | XML Source | Description |
|----------|------------|-------------|
| `LanguageId` | `language` attribute | Language identifier |
| `Value` | `value` attribute | Primary name |
| `Adjective` | `adjective` attribute | Adjective form (optional) |
| `Comment` | `comment` attribute | Additional context (optional) |

### GameIdEntity

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

| Property | XML Source | Description |
|----------|------------|-------------|
| `Game` | `game` attribute | Target game (CK2, CK3, HOI4, IR) |
| `Type` | `type` attribute | Entity type (Language, Province, State, City, etc.) |
| `Parent` | `parent` attribute | Hierarchical parent ID (optional) |
| `DefaultNameLanguageId` | `defaultLanguage` attribute | Default language for name |
| `Id` | Element text content | Game-specific identifier |

### LanguageCodeEntity

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

| Property | XML Source | Description |
|----------|------------|-------------|
| `ISO_639_1` | `iso-639-1` attribute | 2-letter code (e.g., `en`) |
| `ISO_639_2` | `iso-639-2` attribute | 3-letter bibliographic code (e.g., `eng`) |
| `ISO_639_3` | `iso-639-3` attribute | 3-letter terminology code (e.g., `eng`) |

## Service Models

**Location:** `MoreCulturalNamesBuilder/Service/Models/`

### Language

```csharp
public sealed class Language
{
    public string Id { get; set; }
    public LanguageCode Code { get; set; }
    public IEnumerable<GameId> GameIds { get; set; }
    public IEnumerable<string> FallbackLanguages { get; set; }
}
```

### Location

```csharp
public sealed class Location
{
    public string Id { get; set; }
    public string GeoNamesId { get; set; }
    public IEnumerable<GameId> GameIds { get; set; }
    public IEnumerable<string> FallbackLocations { get; set; }
    public IEnumerable<Name> Names { get; set; }

    public bool IsEmpty() => !Names.Any() && !FallbackLocations.Any();
}
```

### Name

```csharp
public class Name
{
    public string LanguageId { get; set; }
    public string Value { get; set; }
    public string Adjective { get; set; }
    public string Comment { get; set; }
}
```

### GameId

```csharp
public sealed class GameId
{
    public string Game { get; set; }
    public string Type { get; set; }
    public string Parent { get; set; }
    public string DefaultNameLanguageId { get; set; }
    public string Id { get; set; }
}
```

### LanguageCode

```csharp
public class LanguageCode
{
    public string ISO_639_1 { get; set; }
    public string ISO_639_2 { get; set; }
    public string ISO_639_3 { get; set; }
}
```

### Localisation (Derived/Computed)

```csharp
public sealed class Localisation
{
    public string Id { get; set; }
    public string GameId { get; set; }
    public string LanguageId { get; set; }
    public string LanguageGameId { get; set; }
    public string Name { get; set; }
    public string Adjective { get; set; }
    public string Comment { get; set; }
}
```

**Note:** `Localisation` is not directly mapped from an entity. It is constructed by `LocalisationFetcher` during resolution, combining data from `Location`, `Language`, and `Name` models.

## Mapping Extensions

**Location:** `MoreCulturalNamesBuilder/Service/Mapping/`

Each entity-model pair has a static mapping class with extension methods:

### LanguageMapping

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

    internal static LanguageEntity ToDataObject(this Language serviceModel) => new()
    {
        Id = serviceModel.Id,
        Code = serviceModel.Code?.ToDataObject(),
        GameIds = serviceModel.GameIds.ToDataObjects().ToList(),
        FallbackLanguages = serviceModel.FallbackLanguages.ToList()
    };

    internal static IEnumerable<Language> ToServiceModels(this IEnumerable<LanguageEntity> dataObjects)
        => dataObjects.Select(dataObject => dataObject.ToServiceModel());

    internal static IEnumerable<LanguageEntity> ToDataObjects(this IEnumerable<Language> serviceModels)
        => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
}
```

### LocationMapping

```csharp
static class LocationMapping
{
    internal static Location ToServiceModel(this LocationEntity dataObject) => new()
    {
        Id = dataObject.Id,
        GeoNamesId = dataObject.GeoNamesId,
        GameIds = dataObject.GameIds.ToServiceModels(),
        FallbackLocations = dataObject.FallbackLocations,
        Names = dataObject.Names.ToServiceModels()
    };

    internal static LocationEntity ToDataObject(this Location serviceModel) => new()
    {
        Id = serviceModel.Id,
        GeoNamesId = serviceModel.GeoNamesId,
        GameIds = serviceModel.GameIds.ToDataObjects().ToList(),
        FallbackLocations = serviceModel.FallbackLocations.ToList(),
        Names = serviceModel.Names.ToDataObjects().ToList()
    };

    internal static IEnumerable<Location> ToServiceModels(this IEnumerable<LocationEntity> dataObjects)
        => dataObjects.Select(dataObject => dataObject.ToServiceModel());

    internal static IEnumerable<LocationEntity> ToDataObjects(this IEnumerable<Location> serviceModels)
        => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
}
```

### NameMapping

```csharp
static class NameMapping
{
    internal static Name ToServiceModel(this NameEntity dataObject) => new()
    {
        LanguageId = dataObject.LanguageId,
        Value = dataObject.Value,
        Adjective = dataObject.Adjective,
        Comment = dataObject.Comment
    };

    internal static NameEntity ToDataObject(this Name serviceModel) => new()
    {
        LanguageId = serviceModel.LanguageId,
        Value = serviceModel.Value,
        Adjective = serviceModel.Adjective,
        Comment = serviceModel.Comment
    };

    internal static IEnumerable<Name> ToServiceModels(this IEnumerable<NameEntity> dataObjects)
        => dataObjects.Select(dataObject => dataObject.ToServiceModel());

    internal static IEnumerable<NameEntity> ToDataObjects(this IEnumerable<Name> serviceModels)
        => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
}
```

### GameIdMapping

```csharp
static class GameIdMapping
{
    internal static GameId ToServiceModel(this GameIdEntity dataObject) => new()
    {
        Game = dataObject.Game,
        Type = dataObject.Type,
        Parent = dataObject.Parent,
        DefaultNameLanguageId = dataObject.DefaultNameLanguageId,
        Id = dataObject.Id
    };

    internal static GameIdEntity ToDataObject(this GameId serviceModel) => new()
    {
        Game = serviceModel.Game,
        Type = serviceModel.Type,
        Parent = serviceModel.Parent,
        DefaultNameLanguageId = serviceModel.DefaultNameLanguageId,
        Id = serviceModel.Id
    };

    internal static IEnumerable<GameId> ToServiceModels(this IEnumerable<GameIdEntity> dataObjects)
        => dataObjects.Select(dataObject => dataObject.ToServiceModel());

    internal static IEnumerable<GameIdEntity> ToDataObjects(this IEnumerable<GameId> serviceModels)
        => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
}
```

### LanguageCodeMapping

```csharp
static class LanguageCodeMapping
{
    internal static LanguageCode ToServiceModel(this LanguageCodeEntity dataObject) => new()
    {
        ISO_639_1 = dataObject.ISO_639_1,
        ISO_639_2 = dataObject.ISO_639_2,
        ISO_639_3 = dataObject.ISO_639_3
    };

    internal static LanguageCodeEntity ToDataObject(this LanguageCode serviceModel) => new()
    {
        ISO_639_1 = serviceModel.ISO_639_1,
        ISO_639_2 = serviceModel.ISO_639_2,
        ISO_639_3 = serviceModel.ISO_639_3
    };

    internal static IEnumerable<LanguageCode> ToServiceModels(this IEnumerable<LanguageCodeEntity> dataObjects)
        => dataObjects.Select(dataObject => dataObject.ToServiceModel());

    internal static IEnumerable<LanguageCodeEntity> ToDataObjects(this IEnumerable<LanguageCode> serviceModels)
        => serviceModels.Select(serviceModel => serviceModel.ToDataObject());
}
```

## Data Flow Transformations

### 1. XML → Entities (NuciDAL XmlRepository)

```
languages.xml ──► XmlRepository<LanguageEntity>.GetAll() ──► LanguageEntity[]
locations.xml ──► XmlRepository<LocationEntity>.GetAll() ──► LocationEntity[]
```

### 2. Entities → Service Models (Mapping Extensions)

```
LanguageEntity[] ──► .ToServiceModels() ──► Language[]
LocationEntity[] ──► .ToServiceModels() ──► Location[]
```

### 3. Service Models → Indexed Dictionaries (ModBuilder.LoadAllData)

```
Language[] ──► .ToDictionary(k => k.Id) ──► Dictionary<string, Language>
Location[] ──► .ToDictionary(k => k.Id) ──► Dictionary<string, Location>
```

### 4. Game ID Filtering (ModBuilder.LoadAllData)

```
Location[].GameIds ──► .Where(g => g.Game == targetGame) ──► locationGameIds
Language[].GameIds ──► .Where(g => g.Game == targetGame) ──► languageGameIds
```

### 5. Localisation Resolution (LocalisationFetcher)

```
Location + Language + Name ──► LocalisationFetcher.GetGameLocationLocalisations()
    │
    ├── Fallback location chain
    ├── Fallback language chain
    ├── Name lookup by (locationId, languageId)
    └── Cache result
        │
        ▼
Localisation {
    Id = location.Id,
    GameId = locationGameId,
    LanguageId = language.Id,
    LanguageGameId = languageGameId,
    Name = name.Value,
    Adjective = name.Adjective,
    Comment = name.Comment
}
```

### 6. Localisation → Game Output (ModBuilder.GenerateFiles)

```
Localisation ──► NameNormaliser.To{Game}Charset() ──► Normalised Name
    │
    ├── CK2: CSV line with Windows-1252 encoding
    ├── CK3: YAML with dynamic localisation keys
    ├── HOI4: YAML with STATE_/VICTORY_POINTS_ prefixes
    └── IR: Province name data + YAML localisation
```

## Model Relationships

### Language ↔ Location (via LocalisationFetcher)

```
Language
    ├── GameIds (per game)
    ├── FallbackLanguages
    └── Code (ISO codes)

Location
    ├── GameIds (per game, with Type)
    ├── FallbackLocations
    └── Names (per language)
        ├── LanguageId
        ├── Value
        ├── Adjective
        └── Comment
```

### GameId Type Variations by Game

| Game | Location Types | Language Types |
|------|----------------|----------------|
| CK2 | Province, Title | Language |
| CK3 | Province, Title | Language |
| HOI4 | State, City | Language |
| IR | Province | Language |

### Fallback Resolution Graph

```
Requested: (Location L, Language Lang, Game G)
    │
    ├── Location fallback chain: L → L.fallback1 → L.fallback2 → ...
    │
    └── Language fallback chain: Lang → Lang.fallback1 → Lang.fallback2 → ...
    │
    Cross-product search:
    For each loc in locationChain:
        For each lang in languageChain:
            If loc.Names contains lang:
                Return Localisation(loc, lang, name)
    │
    └── If none found: Return null (no localisation for this combination)
```

## Invariants and Constraints

### Entity Invariants
- `EntityBase.Id` is unique within each repository
- `GameIdEntity.Id` is unique per (Game, Type) combination
- `NameEntity.LanguageId` must reference a valid `LanguageEntity.Id`
- `LocationEntity.FallbackLocations` must reference valid `LocationEntity.Id` values
- `LanguageEntity.FallbackLanguages` must reference valid `LanguageEntity.Id` values

### Service Model Invariants
- `Language.Id` matches `LanguageEntity.Id`
- `Location.Id` matches `LocationEntity.Id`
- `GameId.Game` matches target game filter
- `Localisation.LanguageGameId` comes from `Language.GameIds` for target game
- `Localisation.GameId` comes from `Location.GameIds` for target game

### Transformation Invariants
- Mapping is lossless for all defined properties
- Collections preserve order (List → IEnumerable → List)
- Null collections become empty enumerables, not null
- `Location.IsEmpty()` returns true when no names and no fallbacks

## Usage in Builders

### CK2/CK3 Builders
```csharp
// Localisations indexed by location game ID
Dictionary<string, IEnumerable<Localisation>> localisations;

// Ordered by language game ID for deterministic output
Dictionary<string, IEnumerable<Localisation>> localisationsOrderedByLanguageGameId;

// Location game IDs for title localisation injection
Dictionary<string, GameId> locationGameIdsById;
```

### HOI4 Builder
```csharp
// Separate state and city localisations
Dictionary<string, Dictionary<string, Localisation>> stateLocalisations;
Dictionary<string, Dictionary<string, Localisation>> cityLocalisations;

// Keyed by language game ID for direct lookup
```

### Imperator: Rome Builder
```csharp
// Province localisations by province ID
Dictionary<string, Dictionary<string, Localisation>> localisations;

// Location game IDs for default language lookup
Dictionary<string, GameId> locationGameIdsById;
```

## Testing

**Location:** `MoreCulturalNamesBuilder.UnitTests/Service/Mapping/`

### Test Coverage
- Round-trip mapping (Entity → Model → Entity)
- Collection mapping
- Null handling in mappings

### Gaps
- XML deserialization edge cases
- Fallback chain resolution with real data
- Localisation model construction verification

## Related Documentation
- [Data Access Layer](data-access-layer.md) - Entity definitions and XML loading
- [Service Layer](service-layer.md) - LocalisationFetcher and NameNormaliser
- [Mod Builders](mod-builders.md) - Model consumption in generation
- [Architecture Overview](architecture-overview.md) - Data architecture section