# Mod Builders

This document describes the game-specific mod generation implementations for CK2, CK3, HOI4, and Imperator: Rome.

## Overview

All mod builders inherit from the abstract `ModBuilder` base class and implement the `IModBuilder` interface. The `ModBuilderFactory` selects the appropriate builder at runtime based on the `--game` argument.

## Class Hierarchy

```
IModBuilder (interface)
    ▲
    │
ModBuilder (abstract base)
    │
    ├── CK2ModBuilder
    │       ▲
    │       │
    │       └── CK3ModBuilder (inherits CK2ModBuilder)
    │
    ├── HOI4ModBuilder
    │
    └── ImperatorRomeModBuilder
```

## IModBuilder Interface

**Location:** `MoreCulturalNamesBuilder/Service/ModBuilders/IModBuilder.cs`

```csharp
public interface IModBuilder
{
    void Build();
}
```

Single method contract - all build logic encapsulated in `Build()`.

## ModBuilder Base Class

**Location:** `MoreCulturalNamesBuilder/Service/ModBuilders/ModBuilder.cs`

### Constructor

```csharp
public abstract class ModBuilder(
    IFileRepository<LanguageEntity> languageRepository,
    IFileRepository<LocationEntity> locationRepository,
    Settings settings) : IModBuilder
```

### Protected Properties

| Property | Description |
|----------|-------------|
| `OutputDirectoryPath` | `Path.Combine(Settings.Output.ModOutputDirectory, Settings.Mod.Game)` |
| `languageRepository` | Injected language repository |
| `locationRepository` | Injected location repository |
| `Settings` | Full settings object |
| `languages` | `Dictionary<string, Language>` - loaded language models |
| `locations` | `Dictionary<string, Location>` - loaded location models |
| `languageGameIds` | Language game IDs for target game |
| `locationGameIds` | Location game IDs for target game |
| `titleGameIds` | Title game IDs (CK2/CK3 only) |

### Build() Template Method

```csharp
public void Build()
{
    Console.WriteLine($" > Building the mod for {Settings.Mod.Game}...");

    StartTimedOperation("Fetching the data", () => LoadAllData());
    StartTimedOperation("Generating the files", () => GenerateFiles());
}
```

### LoadAllData()

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

    LoadData();  // Abstract - game-specific
}
```

### Abstract Methods

| Method | Purpose |
|--------|---------|
| `LoadData()` | Game-specific data loading (localisations, indexes) |
| `GenerateFiles()` | Game-specific file generation |

### StartTimedOperation Helper

```csharp
static void StartTimedOperation(string message, Action operation)
{
    Console.Write($"   > {message}...");
    DateTime start = DateTime.Now;
    operation();
    DateTime finish = DateTime.Now;
    TimeSpan duration = finish - start;
    Console.WriteLine($" (Finished in {Math.Round(duration.TotalSeconds)}s)");
}
```

## CK2ModBuilder

**Location:** `MoreCulturalNamesBuilder/Service/ModBuilders/CK2ModBuilder.cs`

### Inheritance
- Directly inherits from `ModBuilder`
- Base for `CK3ModBuilder`

### Constructor Dependencies

```csharp
public CK2ModBuilder(
    ILocalisationFetcher localisationFetcher,
    INameNormaliser nameNormaliser,
    IFileRepository<LanguageEntity> languageRepository,
    IFileRepository<LocationEntity> locationRepository,
    Settings settings)
```

### Key Fields

| Field | Type | Purpose |
|-------|------|---------|
| `localisationFetcher` | `ILocalisationFetcher` | Retrieves localisations |
| `nameNormaliser` | `INameNormaliser` | Normalises names to Windows-1252 |
| `windows1252NameCache` | `ConcurrentDictionary<string, string>` | Caches Windows-1252 conversions |
| `localisations` | `Dictionary<string, IEnumerable<Localisation>>` | Localisations by location game ID |
| `localisationsOrderedByLanguageGameId` | `Dictionary<string, IEnumerable<Localisation>>` | Localisations sorted by language game ID |
| `locationGameIdsById` | `Dictionary<string, GameId>` | Location game IDs keyed by ID |

### LoadData()

Parallel fetches localisations for all location game IDs:

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

### GenerateFiles()

```csharp
protected override void GenerateFiles()
{
    string mainDirectoryPath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id);
    string commonDirectoryPath = Path.Combine(mainDirectoryPath, "common");
    string landedTitlesDirectoryPath = Path.Combine(commonDirectoryPath, "landed_titles");
    string localisationDirectoryPath = Path.Combine(mainDirectoryPath, LocalisationDirectoryName);

    Directory.CreateDirectory(mainDirectoryPath);
    Directory.CreateDirectory(commonDirectoryPath);
    Directory.CreateDirectory(landedTitlesDirectoryPath);
    Directory.CreateDirectory(localisationDirectoryPath);

    CreateDescriptorFiles();
    CreateLandedTitlesFile(landedTitlesDirectoryPath);
    CreateLocalisationFiles(localisationDirectoryPath);
}
```

### Output Structure (CK2)

```
<output>/CK2/<mod-id>/
├── <mod-id>.mod                    # Main descriptor
├── common/
│   └── landed_titles/
│       └── <landed-titles-name>    # Patched landed titles
└── localisation/
    └── 000_<mod-id>_landed_titles.csv  # CSV localisation
```

### Descriptor Generation

**Main Descriptor** (`<mod-id>.mod`):
```text
# Version 1.0.0 (2026-10-05)
# for CK2 1.18.*
name = "More Cultural Names"
dependencies = { "other-mod-id" }
picture = "thumbnail.png"
tags = { map immersion }
path = "mod/more-cultural-names"
```

**Inner Descriptor** (not used in CK2 - only main descriptor)

### Landed Titles Processing

1. **Read** existing landed titles file (Windows-1252 encoding)
2. **Clean** - Remove existing cultural names, standardise formatting
3. **Inject** - Add cultural name blocks for each title with localisations
4. **Write** - Output as Windows-1252 encoded file

#### CleanLandedTitlesFile() Operations

```csharp
// 1. Remove carriage returns
// 2. Replace tabs with 4 spaces
// 3. Standardise spacing around equals
// 4. Remove comments
// 5. Remove empty/whitespace lines
// 6. Remove trailing whitespace
// 7. Break inline cultural names into multiple lines
// 8. Remove existing cultural name lines
// 9. Break empty blocks {} into multiple lines
// 10. DoCleanLandedTitlesFile() - additional game-specific cleaning
```

#### GetTitleLocalisationsContent()

Generates cultural name block for a title:

```text
    cultural_names = {
        english = "Paris" # Language=eng
        french = "Paris" # Language=fra
    }
```

### Localisation File Generation

**Format:** CSV (Windows-1252 encoding)
**File:** `000_<mod-id>_landed_titles.csv`

**Line Format:**
```
<key>;<name>;<name>;<name>;;<name>;;;;;;;;x
```

**Keys Generated:**
- `{locationGameId}` - Base name
- `{locationGameId}_adj_{languageGameId}` - Language-specific adjective
- `{locationGameId}_adj` - Default adjective (from default language)

### ToWindows1252() Helper

```csharp
string ToWindows1252(string text)
{
    if (text is null) return null;
    if (windows1252NameCache.TryGetValue(text, out string cached)) return cached;

    string normalisedText = nameNormaliser.ToWindows1252(text);
    windows1252NameCache.TryAdd(text, normalisedText);
    return normalisedText;
}
```

## CK3ModBuilder

**Location:** `MoreCulturalNamesBuilder/Service/ModBuilders/CK3ModBuilder.cs`

### Inheritance
- Inherits from `CK2ModBuilder`
- Overrides key methods for CK3-specific format

### Key Overrides

| Property/Method | CK2 Value | CK3 Override |
|-----------------|-----------|--------------|
| `LocalisationDirectoryName` | `"localisation"` | `"localization"` (US spelling) |
| `ForbiddenTokensForPreviousLine` | `["allow", "dejure_liege_title", "gain_effect", "limit", "trigger"]` | `["allow", "limit", "trigger"]` |
| `ForbiddenTokensForNextLine` | `["any_direct_de_jure_vassal_title", "has_holder", "is_titular", "owner", "show_scope_change"]` | `["has_holder"]` |
| `GenerateMainDescriptorContent()` | Includes `path` | Includes `path="mod/{id}"` |
| `GenerateDescriptorContent()` | Simple format | Full descriptor with version, supported_version, tags |
| `ReadLandedTitlesFile()` | Windows-1252 | UTF-8 (default) |
| `WriteLandedTitlesFile()` | Windows1252File.WriteAllText | File.WriteAllText + CK3CulturalNameBlockMerger |
| `CleanLandedTitlesFile()` | Full cleaning | No-op (returns content unchanged) |
| `GetTitleLocalisationsContent()` | CSV-style | Dynamic localisation keys with `name_list_` |
| `CreateLocalisationFiles()` | Single CSV | Multiple YAML files per language |
| `CreateDescriptorFiles()` | Single .mod file | Main .mod + inner descriptor.mod |

### CK3 Descriptor Format

**Main Descriptor** (`<mod-id>.mod`):
```text
# Version 1.0.0 (2026-10-05)
name="More Cultural Names"
version="1.0.0"
supported_version="1.18.*"
tags={
    "Culture"
    "Historical"
    "Map"
    "Translation"
}
path="mod/more-cultural-names"
```

**Inner Descriptor** (`mod/<mod-id>/descriptor.mod`):
```text
# Version 1.0.0 (2026-10-05)
name="More Cultural Names"
version="1.0.0"
supported_version="1.18.*"
tags={
    "Culture"
    "Historical"
    "Map"
    "Translation"
}
```

### Cultural Name Block Merging

**Location:** `MoreCulturalNamesBuilder/Service/ModBuilders/CK3CulturalNameBlockMerger.cs`

Merges multiple `cultural_names` blocks for the same title into a single block, deduplicating `name_list_` entries by key.

### Localisation Files

**Format:** YAML (UTF-8)
**Languages:** english, french, german, polish, spanish, simp_chinese, russian, korean, japanese
**Files per language:**
- `titles_l_<language>.yml` - Default names
- `titles_cultural_names_l_<language>.yml` - Dynamic cultural names

**YAML Structure:**
```yaml
l_english:
 c_paris:0 "Paris"
 c_paris_adj:0 "Parisian"
 name_list_english:0 "Paris" # Paris
 name_list_french:0 "Paris" # Paris
```

### Dynamic Localisation Keys

```csharp
string GetDynamicLocalisationKey(Localisation localisation)
    => $"{localisation.GameId}_{localisation.LanguageGameId}";
```

## HOI4ModBuilder

**Location:** `MoreCulturalNamesBuilder/Service/ModBuilders/HOI4ModBuilder.cs`

### Inheritance
- Directly inherits from `ModBuilder` (not CK2ModBuilder)

### Constructor Dependencies

```csharp
public HOI4ModBuilder(
    ILocalisationFetcher localisationFetcher,
    INameNormaliser nameNormaliser,
    IFileRepository<LanguageEntity> languageRepository,
    IFileRepository<LocationEntity> locationRepository,
    Settings settings)
```

### Key Fields

| Field | Type | Purpose |
|-------|------|---------|
| `stateGameIds` | `IEnumerable<GameId>` | State-type location game IDs |
| `cityGameIds` | `IEnumerable<GameId>` | City-type location game IDs |
| `stateLocalisations` | `Dictionary<string, Dictionary<string, Localisation>>` | State localisations by state ID |
| `cityLocalisations` | `Dictionary<string, Dictionary<string, Localisation>>` | City localisations by city ID |

### LoadData()

Separates states and cities by `GameId.Type`:

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

### GenerateFiles()

```csharp
protected override void GenerateFiles()
{
    string mainDirectoryPath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id);
    string localisationDirectoryPath = Path.Combine(mainDirectoryPath, "localisation");

    Directory.CreateDirectory(mainDirectoryPath);
    Directory.CreateDirectory(localisationDirectoryPath);

    CreateLocalisationFiles(localisationDirectoryPath);
    CreateDescriptorFiles();
}
```

### Output Structure (HOI4)

```
<output>/HOI4/<mod-id>/
├── <mod-id>.mod                    # Main descriptor
├── descriptor.mod                  # Inner descriptor
└── localisation/
    ├── english/
    │   └── zzz999_<mod-id>_l_english.yml
    ├── french/
    │   └── zzz999_<mod-id>_l_french.yml
    ├── german/
    │   └── zzz999_<mod-id>_l_german.yml
    ├── polish/
    │   └── zzz999_<mod-id>_l_polish.yml
    └── spanish/
        └── zzz999_<mod-id>_l_spanish.yml
```

### Localisation Format

**File:** `zzz999_<mod-id>_l_<language>.yml`
**Encoding:** UTF-8
**Prefix:** `l_<language>:`

**State Lines:**
```
 <languageGameId>_STATE_<stateId>:0 "<normalised name>"
```

**City Lines:**
```
 <languageGameId>_VICTORY_POINTS_<cityId>:0 "<normalised name>"
```

**Normalisation:**
- States: `nameNormaliser.ToHOI4StateCharset()`
- Cities: `nameNormaliser.ToHOI4CityCharset()`

### Descriptor Format

**Main Descriptor:**
```text
# Version 1.0.0 (2026-10-05)
name="More Cultural Names"
version="1.0.0"
supported_version="1.18.*"
tags={
    "Historical"
}
path="mod/more-cultural-names"
```

**Inner Descriptor:** Same content without `path`

## ImperatorRomeModBuilder

**Location:** `MoreCulturalNamesBuilder/Service/ModBuilders/ImperatorRomeModBuilder.cs`

### Inheritance
- Directly inherits from `ModBuilder`

### Constructor Dependencies

```csharp
public ImperatorRomeModBuilder(
    ILocalisationFetcher localisationFetcher,
    INameNormaliser nameNormaliser,
    IFileRepository<LanguageEntity> languageRepository,
    IFileRepository<LocationEntity> locationRepository,
    Settings settings)
```

### Key Fields

| Field | Type | Purpose |
|-------|------|---------|
| `localisations` | `Dictionary<string, Dictionary<string, Localisation>>` | Province localisations by province ID |
| `locationGameIdsById` | `Dictionary<string, GameId>` | Location game IDs keyed by ID |

### LoadData()

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

### GenerateFiles()

```csharp
protected override void GenerateFiles()
{
    string mainDirectoryPath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id);
    string localisationDirectoryPath = Path.Combine(mainDirectoryPath, "localization");
    string commonDirectoryPath = Path.Combine(mainDirectoryPath, "common");
    string provinceNamesDirectoryPath = Path.Combine(commonDirectoryPath, "province_names");

    Directory.CreateDirectory(mainDirectoryPath);
    Directory.CreateDirectory(commonDirectoryPath);
    Directory.CreateDirectory(localisationDirectoryPath);
    Directory.CreateDirectory(provinceNamesDirectoryPath);

    CreateDataFiles(provinceNamesDirectoryPath);
    CreateLocalisationFiles(localisationDirectoryPath);
    CreateDescriptorFiles();
}
```

### Output Structure (Imperator: Rome)

```
<output>/IR/<mod-id>/
├── <mod-id>.mod                              # Main descriptor
├── descriptor.mod                            # Inner descriptor
├── common/
│   └── province_names/
│       ├── english.txt                       # Province name data
│       ├── french.txt
│       ├── german.txt
│       └── spanish.txt
└── localization/
    ├── <mod-id>_provincenames_l_english.yml
    ├── <mod-id>_provincenames_l_french.yml
    ├── <mod-id>_provincenames_l_german.yml
    └── <mod-id>_provincenames_l_spanish.yml
```

### Data Files (province_names)

**File:** `<languageGameId>.txt` (lowercase)
**Format:**
```text
english = {
    PROV123 = PROV123_english # Paris
    PROV456 = PROV456_english # London
}
```

**Key Format:** `PROV{provinceId}_{languageGameId}`
**Comment:** `# {normalised name}` + optional `# Language={languageId}` + optional `# {comment}`

### Localisation Files

**File:** `<mod-id>_provincenames_l_<language>.yml`
**Encoding:** UTF-8
**Prefix:** `l_<language>:`

**Lines:**
```
 PROV{provinceId}:0 "<normalised default name>"
 PROV{provinceId}_{languageGameId}:0 "<normalised cultural name>"
```

**Normalisation:** `nameNormaliser.ToImperatorRomeCharset()`

### Descriptor Format

**Main Descriptor:**
```text
# Version 1.0.0 (2026-10-05)
name="More Cultural Names"
version="1.0.0"
supported_version="1.18.*"
tags={
    "Historical"
}
path="mod/more-cultural-names"
```

**Inner Descriptor:** Same content without `path`

## ModBuilderFactory

**Location:** `MoreCulturalNamesBuilder/Service/ModBuilders/ModBuilderFactory.cs`

### Selection Logic

```csharp
public IModBuilder GetModBuilder(Settings settings)
{
    string normalisedGame = settings.Mod.Game.ToUpperInvariant().Trim();

    if (normalisedGame.StartsWith("CK2"))
        return new CK2ModBuilder(...);

    if (normalisedGame.StartsWith("CK3"))
        return new CK3ModBuilder(...);

    if (normalisedGame.StartsWith("HOI4"))
        return new HOI4ModBuilder(...);

    if (normalisedGame.StartsWith("IR") ||
        normalisedGame.StartsWith("IMPERATORROME"))
        return new ImperatorRomeModBuilder(...);

    throw new NotImplementedException($"The game \"{settings.Mod.Game}\" is not supported");
}
```

**Matching:** Prefix-based, case-insensitive
- `CK2*` → CK2ModBuilder
- `CK3*` → CK3ModBuilder
- `HOI4*` → HOI4ModBuilder
- `IR*` or `IMPERATORROME*` → ImperatorRomeModBuilder

## Output Comparison Summary

| Aspect | CK2 | CK3 | HOI4 | Imperator: Rome |
|--------|-----|-----|------|-----------------|
| **Descriptor** | Single `.mod` | Main + inner `.mod` | Main + inner `.mod` | Main + inner `.mod` |
| **Landed Titles** | Patched file | Merged + patched | N/A | N/A |
| **Localisation Format** | CSV (Win-1252) | YAML (UTF-8) | YAML (UTF-8) | YAML (UTF-8) |
| **Localisation Files** | 1 CSV | 2 YAML × 9 langs | 1 YAML × 5 langs | 1 YAML × 4 langs |
| **Data Files** | None | None | None | Province names (txt) |
| **Name Normalisation** | Windows-1252 | CK3 charset | HOI4 city/state | IR charset |
| **Directory Name** | `localisation` | `localization` | `localisation` | `localization` |

## Testing Coverage

**Location:** `MoreCulturalNamesBuilder.UnitTests/Service/ModBuilders/`

### Current Coverage
- Factory selection logic
- Basic builder instantiation

### Major Gaps
- No end-to-end tests generating actual mod files
- No output format validation
- No game compatibility verification
- No integration tests with real XML data

## Extension Points

To add a new game target:
1. Create new builder class inheriting from `ModBuilder`
2. Implement `LoadData()` and `GenerateFiles()`
3. Add case to `ModBuilderFactory.GetModBuilder()`
4. Add game-specific normalisation to `NameNormaliser` if needed
5. Add tests for new builder

## Related Documentation
- [Architecture Overview](architecture-overview.md) - Builder layer in context
- [Service Layer](service-layer.md) - Services consumed by builders
- [Name Normalisation](name-normalisation.md) - Game-specific charsets
- [Data Models](data-models.md) - Models used in generation