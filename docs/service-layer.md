# Service Layer

This document describes the service layer components: `LocalisationFetcher`, `NameNormaliser`, and mapping extensions.

## Overview

The service layer provides stateless, reusable data transformation services consumed by all game-specific mod builders. Services depend only on data entities and interfaces - never on builder implementations.

## LocalisationFetcher

**Location:** `MoreCulturalNamesBuilder/Service/LocalisationFetcher.cs`
**Interface:** `ILocalisationFetcher`

### Purpose

Retrieves localised names for game locations across all supported languages, implementing a sophisticated fallback resolution chain.

### Constructor Dependencies

```csharp
public LocalisationFetcher(
    IFileRepository<LanguageEntity> languageRepository,
    IFileRepository<LocationEntity> locationRepository)
```

### Internal State

| Field | Type | Purpose |
|-------|------|---------|
| `languageRepository` | `IFileRepository<LanguageEntity>` | Source of language data |
| `locationRepository` | `IFileRepository<LocationEntity>` | Source of location data |
| `locations` | `Dictionary<string, Location>` | Location entities keyed by ID |
| `languages` | `Dictionary<string, Language>` | Language entities keyed by ID |
| `locationGameIdIndex` | `Dictionary<(Game, Id), Location>` | Lookup by game+location ID |
| `locationGameIdWithTypeIndex` | `Dictionary<(Game, Id, Type), Location>` | Lookup by game+location ID+type |
| `languageGameIdsByGame` | `Dictionary<Game, Dictionary<GameId, LanguageId>>` | Language game IDs per game |
| `locationNamesByLanguage` | `Dictionary<LocationId, Dictionary<LanguageId, Name>>` | Names indexed by location+language |
| `locationIdsToCheckByLocationId` | `Dictionary<LocationId, string[]>` | Fallback location chain |
| `languageIdsToCheckByLanguageId` | `Dictionary<LanguageId, string[]>` | Fallback language chain |
| `resolvedLocalisationCache` | `ConcurrentDictionary<(LocationId, LanguageId), CachedLocalisation>` | Resolved localisation cache |

### Data Loading (LoadData)

Executed in constructor. Builds all lookup dictionaries from repository data:

1. **Load entities** → Convert to service models → Index by ID
2. **Build location indexes**:
   - `locationGameIdIndex`: Maps `(game, locationGameId)` → `Location`
   - `locationGameIdWithTypeIndex`: Maps `(game, locationGameId, type)` → `Location`
   - `locationNamesByLanguage`: Maps `locationId` → `{languageId → Name}`
   - `locationIdsToCheckByLocationId`: Maps `locationId` → fallback chain
3. **Build language indexes**:
   - `languageGameIdsByGame`: Maps `game` → `{gameId → languageId}`
   - `languageIdsToCheckByLanguageId`: Maps `languageId` → fallback chain

### Public API

#### GetGameLocationLocalisations (2 overloads)

```csharp
// Overload 1: Without type
IEnumerable<Localisation> GetGameLocationLocalisations(
    string locationGameId,
    string gameId);

// Overload 2: With type
IEnumerable<Localisation> GetGameLocationLocalisations(
    string locationGameId,
    string locationGameIdType,
    string gameId);
```

**Parameters:**
- `locationGameId` - Game-specific location identifier (e.g., `c_paris`, `123`)
- `locationGameIdType` - Optional type filter (e.g., `"State"`, `"City"` for HOI4)
- `gameId` - Target game identifier (CK2, CK3, HOI4, IR)

**Returns:** `IEnumerable<Localisation>` - One localisation per language for the location

### Resolution Algorithm

```
1. Look up Location by (game, locationGameId[, type])
   → If not found, return empty list

2. For each language game ID in the game:
   a. Check cache for (location.Id, languageId)
   b. If cached, return cached result
   c. If not cached, resolve via fallback chain:
      i.   For each location ID in fallback chain:
           For each language ID in fallback chain:
               If name exists → cache and return
      ii.  If no name found → cache as missing, return null
   d. If localisation found:
      - Set GameId = locationGameId
      - Set LanguageGameId = language game ID
      - Add to results
   e. If localisation is null → skip (continue to next language)

3. Return all resolved localisations
```

### Fallback Chain Construction

#### Location Fallback Chain (BuildLocationIdsToCheck)
```csharp
string[] BuildLocationIdsToCheck(Location location)
{
    List<string> idsToCheck = [location.Id];
    idsToCheck.AddRange(location.FallbackLocations);
    return idsToCheck.ToArray();
}
```
- Starts with the location's own ID
- Appends all fallback location IDs in order

#### Language Fallback Chain (BuildLanguageIdsToCheck)
```csharp
string[] BuildLanguageIdsToCheck(Language language)
{
    List<string> idsToCheck = [language.Id];
    idsToCheck.AddRange(language.FallbackLanguages);
    return idsToCheck.ToArray();
}
```
- Starts with the language's own ID
- Appends all fallback language IDs in order

### Caching Strategy

- **Cache Key**: `(LocationId, LanguageId)` tuple
- **Cache Value**: `CachedLocalisation` (internal struct)
- **Cache Type**: `ConcurrentDictionary` for thread safety
- **Cache Population**: Lazy - populated on first resolution
- **Cache Scope**: Per `LocalisationFetcher` instance (singleton)
- **Cache Invalidation**: None - data is immutable after load

### CachedLocalisation (Internal)

```csharp
// Inferred from usage - not directly visible in source
struct CachedLocalisation
{
    bool Found { get; }
    string ResolvedLocationId { get; }
    string ResolvedLanguageId { get; }
    string Name { get; }
    string Adjective { get; }
    string Comment { get; }
}
```

## NameNormaliser

**Location:** `MoreCulturalNamesBuilder/Service/NameNormaliser.cs`
**Interface:** `INameNormaliser`

### Purpose

Applies game-specific character set transformations to localised names, converting Unicode characters to the limited character sets supported by each game engine.

### Constructor Dependencies

```csharp
public NameNormaliser(INuciTextConverter textConverter)
```

### Public API

| Method | Game Target | Description |
|--------|-------------|-------------|
| `ToCK3Charset(string name)` | Crusader Kings III | Converts to CK3-supported characters |
| `ToHOI4CityCharset(string name)` | Hearts of Iron IV (Cities) | Converts to HOI4 city name charset |
| `ToHOI4StateCharset(string name)` | Hearts of Iron IV (States) | Converts to HOI4 state name charset |
| `ToImperatorRomeCharset(string name)` | Imperator: Rome | Converts to IR-supported characters |
| `ToWindows1252(string name)` | All games (file encoding) | Converts to Windows-1252 encoding |

### Transformation Pipeline

Each game-specific method follows this pattern:

```
Input Name
    │
    ▼
ApplyCommonReplacements()  ← Shared preprocessing
    │
    ▼
Game-Specific Replacements  ← Game-specific post-processing
    │
    ▼
ReplaceUsingMap()           ← Game-specific character mapping
    │
    ▼
Game-Specific Regex/Replace ← Final adjustments
    │
    ▼
Cache Result                ← Per-game cache
```

### ApplyCommonReplacements

Shared preprocessing applied by all game-specific methods:

1. **StartsWithPhi**: `\bɸ` → `P` (Greek phi at word start)
2. **CommonCharacterMappings**: Unicode → Latin transliteration (e.g., `А` → `A`, `α` → `a`)
3. **Zero-width character removal**: ZWJ, ZWNJ, zero-width spaces
4. **Apostrophe normalisation**: Various Unicode apostrophes → `'`, `"`, `` ` ``, `´`
5. **Dash normalisation**: En/em dashes → `-`
6. **Directionality marks removal**: LTR/RTL marks
7. **Invisible character removal**: Various invisible Unicode characters
8. **Letterform alternatives**: Unicode lookalikes → standard Latin (e.g., `𝖠` → `A`)
9. **Floating diacritic resolution**: Combining marks → precomposed characters
10. **Special replacements**: `ḡ` → `ğ`, `ڭ` → `ġ`, etc.

### Game-Specific Character Maps

#### CK3CharacterMappings
- Maps Unicode characters to CK3-supported Latin Extended characters
- Examples: `Ǣ` → `Æ`, `Ạ` → `A`, `Ḃ` → `B`, `Ǧ` → `Ğ`
- Includes special handling for `J̌` → `Ĵ`, `T̈` → `T`, `āẗ` → `āh`

#### Hoi4CityCharacterMappings
- Maps to HOI4 city name charset
- Examples: `Ǿ` → `Ø`, `Ќ` → `Ќ`, `Š` → `Sh`
- Includes `CurlyApostropheRegex` → `´`

#### Hoi4StateCharacterMappings
- Maps to HOI4 state name charset
- Examples: `Č` → `Ch`, `Ť` → `Ty`, `Ř` → `Rz`
- Includes special handling for `Ġh` → `Gh`, `ġh` → `gh`

#### ImperatorRomeCharacterMappings
- Maps to IR-supported characters
- Examples: `Ž` → `Zh`, `Š` → `Sh`, `Ќ` → `K`
- Includes special handling for `J̌` → `J`, `T̈` → `T`

### Caching

Each method maintains its own `ConcurrentDictionary<string, string>` cache:
- `ck3cache` - CK3 normalisation results
- `hoi4citiesCache` - HOI4 city normalisation results
- `hoi4statesCache` - HOI4 state normalisation results
- `irCache` - Imperator: Rome normalisation results

### ReplaceUsingMap Algorithm

```csharp
private static string ReplaceUsingMap(string input, Dictionary<char, string> map)
{
    // Find first replaceable character
    int firstReplaceableCharacterIndex = -1;
    for (int i = 0; i < input.Length; i++)
    {
        if (map.ContainsKey(input[i]))
        {
            firstReplaceableCharacterIndex = i;
            break;
        }
    }

    // Early exit if no replacements needed
    if (firstReplaceableCharacterIndex == -1)
        return input;

    // Build result with StringBuilder
    StringBuilder sb = new(input.Length);
    sb.Append(input, 0, firstReplaceableCharacterIndex);

    for (int i = firstReplaceableCharacterIndex; i < input.Length; i++)
    {
        char c = input[i];
        if (map.TryGetValue(c, out string replacement))
            sb.Append(replacement);
        else
            sb.Append(c);
    }

    return sb.ToString();
}
```

**Optimization**: Early exit when no characters need replacement avoids StringBuilder allocation.

## Mapping Extensions

**Location:** `MoreCulturalNamesBuilder/Service/Mapping/`

### Overview

Static extension classes that convert between Data Access entities and Service Layer models.

| Mapping Class | Entity | Model |
|---------------|--------|-------|
| `LanguageMapping` | `LanguageEntity` | `Language` |
| `LocationMapping` | `LocationEntity` | `Location` |
| `NameMapping` | `NameEntity` | `Name` |
| `GameIdMapping` | `GameIdEntity` | `GameId` |
| `LanguageCodeMapping` | `LanguageCodeEntity` | `LanguageCode` |

### Pattern

Each mapping class provides:
- `ToServiceModel(this Entity)` - Single entity conversion
- `ToDataObject(this Model)` - Single model conversion (reverse)
- `ToServiceModels(this IEnumerable<Entity>)` - Collection conversion
- `ToDataObjects(this IEnumerable<Model>)` - Collection conversion (reverse)

### Usage

```csharp
// In LocalisationFetcher.LoadData():
locations = locationRepository
    .GetAll()
    .ToServiceModels()  // Extension method from LocationMapping
    .ToDictionary(key => key.Id, val => val);

// In ModBuilder.LoadAllData():
languages = languageRepository
    .GetAll()
    .ToServiceModels()  // Extension method from LanguageMapping
    .ToDictionary(key => key.Id, val => val);
```

## Service Layer Invariants

### Statelessness
- Services hold no mutable state between calls (except caches)
- All dependencies injected via constructor
- Thread-safe by design (ConcurrentDictionary for caches)

### Immutability
- Service models are read-only after creation
- No setters exposed beyond initial mapping
- Collections are not modified after population

### Fail-Fast
- Null inputs handled gracefully (return empty/null)
- Invalid data causes exceptions to propagate
- No silent data corruption

## Testing

**Locations:**
- `MoreCulturalNamesBuilder.UnitTests/Service/LocalisationFetcherTests.cs`
- `MoreCulturalNamesBuilder.UnitTests/Service/NameNormaliserTests.cs`
- `MoreCulturalNamesBuilder.UnitTests/Service/NameNormaliserEdgeCaseTests.cs`
- `MoreCulturalNamesBuilder.UnitTests/Service/Mapping/`

### Coverage Areas
- Localisation resolution with fallback chains
- Name normalisation for each game charset
- Edge cases (empty strings, null inputs, special characters)
- Mapping round-trip correctness

### Gaps
- No integration tests with real XML data files
- No performance benchmarks for large datasets
- No concurrent access stress tests

## Related Documentation
- [Architecture Overview](architecture-overview.md) - Service layer in context
- [Data Access Layer](data-access-layer.md) - Entity definitions
- [Data Models](data-models.md) - Service model definitions
- [Mod Builders](mod-builders.md) - Service consumers
- [Name Normalisation](name-normalisation.md) - Detailed normalisation rules