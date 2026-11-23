# Mod Builders

This document describes the game-specific mod generation implementations for CK2, CK3, EU5, HOI4, Imperator: Rome, and Victoria 3.

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
    ├── EuropaUniversalis5ModBuilder
    │
    ├── ImperatorRomeModBuilder
    │
    └── Victoria3ModBuilder
```

## IModBuilder Interface

**Location:** [IModBuilder.cs](../MoreCulturalNamesBuilder/Service/ModBuilders/IModBuilder.cs)

```csharp
public interface IModBuilder
{
    void Build();
}
```

Single method contract - all build logic encapsulated in `Build()`.

## ModBuilder Base Class

**Location:** [ModBuilder.cs](../MoreCulturalNamesBuilder/Service/ModBuilders/ModBuilder.cs)

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

**Location:** [CK2ModBuilder.cs](../MoreCulturalNamesBuilder/Service/ModBuilders/CK2ModBuilder.cs)

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

Parallel retrieval of localisations for all location game IDs:

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

protected virtual void InitializeOrderedLocalisations()
    => localisationsOrderedByLanguageGameId = localisations
        .ToDictionary(
            localisationsByGameId => localisationsByGameId.Key,
            localisationsByGameId =>
                (IEnumerable<Localisation>)localisationsByGameId.Value
                    .OrderBy(localisation => localisation.LanguageGameId)
                    .ToArray());
```

`InitializeOrderedLocalisations()` is overridable so builders that never read the CK2 lookup table can skip building it. `CK3ModBuilder` overrides it with an empty dictionary, avoiding a full ordered copy of every localisation.

### Landed Title Identification

`LandedTitleIdPrefixes` supplies the character class used to recognise a landed-title line. `CK2ModBuilder` defaults to `"ekdcb"` (empire, kingdom, duchy, county, barony), while `CK3ModBuilder` overrides it with `"hekdcb"` to include the CK3 hegemony tier (`h_`). The property feeds three patterns: the title-id regular expression in `BuildLandedTitlesFile()`, and both cultural-name patterns in `DoCleanLandedTitlesFile()` and `CleanLandedTitlesFile()`. `CK3ModBuilder` forwards the same value to `CK3CulturalNameBlockMerger.Merge()` so block merging recognises every supported tier.

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
3. **Inject** - Insert cultural name blocks for each title with localisations
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

**Location:** [CK3ModBuilder.cs](../MoreCulturalNamesBuilder/Service/ModBuilders/CK3ModBuilder.cs)

### Inheritance
- Inherits from `CK2ModBuilder`
- Overrides key methods for CK3-specific format

### Key Overrides

| Property/Method | CK2 Value | CK3 Override |
|-----------------|-----------|--------------|
| `LocalisationDirectoryName` | `"localisation"` | `"localization"` (US spelling) |
| `LandedTitleIdPrefixes` | `"ekdcb"` (empire, kingdom, duchy, county, barony) | `"hekdcb"` (adds hegemony) |
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

**Location:** [CK3CulturalNameBlockMerger.cs](../MoreCulturalNamesBuilder/Service/ModBuilders/CK3CulturalNameBlockMerger.cs)

Merges multiple `cultural_names` blocks for the same title into a single block, deduplicating `name_list_` entries by key. `Merge()` receives the landed-title prefixes from the calling builder, so recognition corresponds to the builder's supported tiers. Block boundaries are located with a single-pass brace counter, and the content is returned unchanged when no title required merging. Only blocks that are direct children of the title are considered: scanning stops descending into any nested block, so a `cultural_names` block belonging to a nested county or barony remains in place instead of being hoisted into its parent title. Every title in the file is visited, so nested titles are merged against their own direct blocks.

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
    => $"cn_{canonicalLocalisation.Id}_{canonicalLocalisation.LanguageGameId}";
```

The canonical localisation is the entry with the lowest `Id` among all localisations sharing the same `LanguageGameId` and the same `ToCK3Charset(Name)`. `EnsureCanonicalLocalisations()` builds that lookup once, as a `LanguageGameId → normalised name → Localisation` index, and is reused by both the landed-titles injection and the dynamic localisation file generation.

## HOI4ModBuilder

**Location:** [HOI4ModBuilder.cs](../MoreCulturalNamesBuilder/Service/ModBuilders/HOI4ModBuilder.cs)

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

**Location:** [ImperatorRomeModBuilder.cs](../MoreCulturalNamesBuilder/Service/ModBuilders/ImperatorRomeModBuilder.cs)

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
| `provinceIdsByNumericOrder` | `Lazy<IEnumerable<string>>` | Province IDs sorted numerically once, on first use |

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
    provinceIdsByNumericOrder = new(() =>
        [.. localisations
            .Keys
            .OrderBy(provinceId => int.Parse(provinceId))]);
}
```

The numeric ordering is computed lazily and only once, then reused by every culture's province-names file. Laziness keeps malformed identifiers surfacing as an `AggregateException` from the parallel generation loop.

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

## Victoria3ModBuilder

**Location:** [Victoria3ModBuilder.cs](../MoreCulturalNamesBuilder/Service/ModBuilders/Victoria3ModBuilder.cs)

### Inheritance

- Directly inherits from `ModBuilder` (not `CK2ModBuilder`)

### Constructor Dependencies

```csharp
public Victoria3ModBuilder(
    ILocalisationFetcher localisationFetcher,
    INameNormaliser nameNormaliser,
    IFileRepository<LanguageDataObject> languageRepository,
    IFileRepository<LocationDataObject> locationRepository,
    Settings settings)
```

### Game Model

Victoria 3 has no title or province hierarchy. Cultural names are driven by *dynamic state and hub names*:

| Concept | Game identifier | Localisation key |
|---------|-----------------|------------------|
| State (state region) | `type="State"`, e.g. `STATE_ABRUZZO` | `STATE_ABRUZZO` |
| Hub | `type="Hub"`, e.g. `STATE_ABRUZZO_city` | `HUB_NAME_STATE_ABRUZZO_city` |
| Culture | language game ID, e.g. `english` | n/a |

A hub identifier is `<state id>_<hub type>`, where the hub type is one of `city`, `port`, `mine`, `farm`, or `wood`. The state is derived by stripping the trailing hub type.

The language game IDs map to Victoria 3 **culture** identifiers (188 of the 193 shipped identifiers resolve against `common/cultures`), so a cultural name is gated with `country_has_primary_culture = cu:<culture>`. Culture-specific localisation keys are `<key>_<culture>`, which vanilla also uses for its own dynamic names.

### Key Fields

| Field | Type | Purpose |
|-------|------|---------|
| `stateIds` | `IList<string>` | Ordered state identifiers |
| `cultureIds` | `IList<string>` | Ordered culture identifiers referenced by the data |
| `hubIdsByStateId` | `IDictionary<string, IList<string>>` | Hubs grouped by their state |
| `localisationsByGameId` | `IDictionary<string, IDictionary<string, Localisation>>` | Localisations per state or hub, keyed by culture |

### Dynamic Name Mechanism

Victoria 3 resolves dynamic names through scripted effects that are invoked by `on_actions`. Because a vanilla on action's `effect` block cannot be appended to, the builder emits its behaviour as a separate on action and appends it to the vanilla on action's `on_actions` list, which is the mechanism the game documents for exactly this purpose.

The generated scripted effects are:

| Effect | Responsibility |
|--------|----------------|
| `{prefix}_evaluate_and_assign_state_hub_dynamic_names` | Honours the `no_dynamic_naming` game rule, otherwise dispatches |
| `{prefix}_assign_state_hub_dynamic_names` | Dispatches to the assignment effect of the current state region |
| `{prefix}_<state>_state_name_assign` | Assigns the state name and its hub names, or resets to defaults |

`{prefix}` is the mod identifier with `-` replaced by `_`.

The read-only `on_actions` hooks are emitted for `on_state_owner_change`, `on_state_created`, `on_become_subject`, and `on_become_independent`; the two country-scope on actions wrap the evaluation in `every_scope_state`.

### LoadData()

```csharp
protected override void LoadData()
{
    stateIds = [.. locationGameIds
        .Where(gameId => string.Equals(gameId.Type, "State", StringComparison.Ordinal))
        .Select(gameId => gameId.Id)
        .Distinct(StringComparer.Ordinal)
        .OrderBy(stateId => stateId, StringComparer.Ordinal)];

    Parallel.ForEach(stateIds, stateId =>
        concurrentLocalisations.TryAdd(stateId, GetLocalisations(stateId, "State")));

    Parallel.ForEach(
        locationGameIds.Where(gameId => string.Equals(gameId.Type, "Hub", StringComparison.Ordinal)),
        hubGameId =>
        {
            string stateId = GetStateId(hubGameId.Id);
            concurrentLocalisations.TryAdd(hubGameId.Id, GetLocalisations(hubGameId.Id, "Hub"));
            hubStatePairs.Add(new(stateId, hubGameId.Id));
        });

    // Hubs are grouped by state and cultures are collected from every loaded localisation.
}
```

### GenerateFiles()

```csharp
protected override void GenerateFiles()
{
    string mainDirectoryPath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id);
    string effectsDirectoryPath = Path.Combine(mainDirectoryPath, "common", "scripted_effects");
    string onActionsDirectoryPath = Path.Combine(mainDirectoryPath, "common", "on_actions");
    string localisationDirectoryPath = Path.Combine(mainDirectoryPath, "localization");
    string metadataDirectoryPath = Path.Combine(mainDirectoryPath, ".metadata");

    // Create the directories, then:
    CreateStateNamesFile(effectsDirectoryPath);
    CreateOnActionsFile(onActionsDirectoryPath);
    CreateLocalisationFiles(localisationDirectoryPath);
    CreateMetadataFiles(metadataDirectoryPath);
}
```

### Output Structure (Victoria 3)

```text
<output>/Vic3/<mod-id>/
├── .metadata/
│   └── metadata.json                          # Launcher metadata, UTF-8 without BOM
├── common/
│   ├── on_actions/
│   │   └── <prefix>_state_names.txt           # Hooks appended to the vanilla on actions
│   └── scripted_effects/
│       └── <prefix>_state_names.txt           # Evaluation, dispatch, and assignment effects
└── localization/
    ├── english/
    │   └── <prefix>_state_names_l_english.yml
    └── <language>/                            # 11 locales
```

There is no outer `.mod` descriptor; Victoria 3 identifies a mod solely by the `.metadata` folder.

### Script Generation

```text
<prefix>_<state>_state_name_assign = {
    if = {
        limit = { owner = { country_has_primary_culture = cu:romanian } }
        set_state_name = STATE_CLUJ_romanian # Language=Romanian
        set_hub_name = { type = city name = HUB_NAME_STATE_CLUJ_city_romanian } # Cluj-Napoca
    }
    else = { # Switches back to the default names
        reset_state_name = yes
        reset_hub_names = yes
    }
}
```

Each hub of a state gets its own singular `set_hub_name`, because the culture suffix of the hub localisation key differs from the hub type. The `else` block always resets the names so a state reverts to its defaults when an unsupported culture takes it.

### Localisation Files

**Format:** YAML (UTF-8 with BOM)
**Languages:** braz_por, english, french, german, japanese, korean, polish, russian, simp_chinese, spanish, turkish

**File naming:** `<prefix>_state_names_l_<language>.yml`, in a `<language>/` sub-folder

**Content:**
```yaml
l_english:
 HUB_NAME_STATE_CLUJ_city_romanian: "Cluj-Napoca" # Language=Romanian
 STATE_CLUJ_romanian: "Cluj-Napoca" # Language=Romanian
```

**Normalisation:** `nameNormaliser.ToVictoria3Charset()`

### Metadata Format

```json
{
  "name": "More Cultural Names",
  "id": "more-cultural-names",
  "version": "1.0.0",
  "game_id": "victoria3",
  "picture": "thumbnail.png",
  "supported_game_version": "1.15.*",
  "short_description": "More Cultural Names",
  "tags": [
    "Culture",
    "Historical",
    "Map",
    "Translation"
  ],
  "relationships": [],
  "game_custom_data": {
    "multiplayer_synchronized": true
  }
}
```

## EuropaUniversalis5ModBuilder

**Location:** [EuropaUniversalis5ModBuilder.cs](../MoreCulturalNamesBuilder/Service/ModBuilders/EuropaUniversalis5ModBuilder.cs)

### Inheritance

- Directly inherits from `ModBuilder` (not `CK2ModBuilder`)

### Constructor Dependencies

```csharp
public EuropaUniversalis5ModBuilder(
    ILocalisationFetcher localisationFetcher,
    INameNormaliser nameNormaliser,
    IFileRepository<LanguageDataObject> languageRepository,
    IFileRepository<LocationDataObject> locationRepository,
    Settings settings)
```

### Game Model

Europa Universalis V has no title hierarchy and no scripting hook for renaming a location. Location names are resolved from localisation keys whose suffix is the **language or dialect** the location is displayed in:

| Concept | Game identifier | Localisation key |
|---------|-----------------|------------------|
| Location | the location key, e.g. `stockholm` | `stockholm` (default) |
| Language-specific name | language game ID, e.g. `romanian_language` | `stockholm.romanian_language` |

When a location has no explicit culture-specific entry, vanilla falls back to the base `stockholm` key, which is why the builder writes both the language-suffixed keys and the location's own default name.

The language game IDs therefore carry both the language identifier and its dialect namespace (`romanian_language`, `english_language`, `bavarian_dialect`), and the key uses the identifier verbatim after the dot, because the engine resolves dotted keys against the culture language and dialect names exactly as declared in `common/languages/`.

### Key Fields

| Field | Type | Purpose |
|-------|------|---------|
| `locationIds` | `IList<string>` | Ordinal-ordered location identifiers |
| `localisationsByLocationId` | `IDictionary<string, IDictionary<string, Localisation>>` | Localisations per location, keyed by language game ID |
| `defaultLanguageIdsByLocationId` | `IDictionary<string, string>` | Default name language declared by each location game ID |

### LoadData()

```csharp
protected override void LoadData()
{
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
}
```

### GenerateFiles()

```csharp
protected override void GenerateFiles()
{
    string mainDirectoryPath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id);
    string metadataDirectoryPath = Path.Combine(mainDirectoryPath, ".metadata");
    string localisationDirectoryPath = Path.Combine(mainDirectoryPath, "main_menu", "localization");

    Directory.CreateDirectory(metadataDirectoryPath);
    Directory.CreateDirectory(localisationDirectoryPath);

    CreateMetadataFile(metadataDirectoryPath);
    CreateLocalisationFiles(localisationDirectoryPath);
}
```

### Output Structure (Europa Universalis V)

```text
<output>/EU5/<mod-id>/
├── .metadata/
│   └── metadata.json                                       # Launcher metadata, UTF-8 without BOM
└── main_menu/
    └── localization/
        └── <language>/
            └── replace/
                └── <prefix>_location_names_l_<language>.yml  # 11 locales
```

There is no outer `.mod` descriptor, and no `in_game/` or `loading_screen/` content: Europa Universalis V identifies a mod solely by the `.metadata` folder, and vanilla location names live under `main_menu/localization/`.

### Localisation Placement

Every display language the game ships is emitted: `braz_por`, `english`, `french`, `german`, `japanese`, `korean`, `polish`, `russian`, `simp_chinese`, `spanish`, and `turkish`. The file is written under a `replace/` sub-folder, because localisation files load in reverse alphabetical order and later-loaded keys do not overwrite earlier ones; only `localization/<language>/replace/` reliably overrides a vanilla location name.

### Localisation Files

**Format:** YAML (UTF-8 with BOM)
**File naming:** `<prefix>_location_names_l_<language>.yml`, in a `<language>/replace/` sub-folder

**Content:**
```yaml
l_english:
 cluj.english_language: "Newport" # Language=English
 cluj.romanian_language: "Cluj-Napoca" # Language=Romanian # Praise the Sun!
 cluj: "Cluj-Napoca" # Language=Romanian # Praise the Sun!
```

The location's own key carries the name supplied by its default language, so the name persists when no language-specific entry matches.

**Normalisation:** `nameNormaliser.ToEuropaUniversalis5Charset()` (delegates to the CK3 charset)

### Deliberate Limitations

- Adjectives are not emitted. Vanilla declares a language-independent `<location>_ADJ` key for only a handful of locations, so generating one for every location would invent content the engine does not consume.
- The builder only writes the localisation layer. Europa Universalis V derives location names from the key suffix and never needs map or setup files, so no `in_game/` content is generated.
- A location's own default name is emitted only when the default language declared by its game identifier resolves to a loaded localisation; otherwise its language-suffixed keys stand alone.

### Metadata Format

```json
{
  "name": "More Cultural Names",
  "id": "more-cultural-names",
  "version": "1.0.0",
  "game_id": "eu5",
  "picture": "thumbnail.png",
  "supported_game_version": "1.18.*",
  "short_description": "More Cultural Names",
  "tags": [
    "Culture & Art",
    "Historical",
    "Map",
    "Translation"
  ],
  "relationships": [],
  "game_custom_data": {
    "multiplayer_synchronized": true
  }
}
```

The tags are drawn from `main_menu/create_mod_tags.json`, so they are valid launcher categories.

## ModBuilderFactory

**Location:** [ModBuilderFactory.cs](../MoreCulturalNamesBuilder/Service/ModBuilders/ModBuilderFactory.cs)

### Selection Logic

```csharp
public IModBuilder GetModBuilder(Settings settings)
{
    string normalisedGame = settings.Mod.Game.ToUpperInvariant().Trim();

    if (normalisedGame.StartsWith("CK2"))
        return new CK2ModBuilder(...);

    if (normalisedGame.StartsWith("CK3"))
        return new CK3ModBuilder(...);

    if (normalisedGame.StartsWith("EU5") ||
        normalisedGame.StartsWith("EUROPAUNIVERSALIS5"))
        return new EuropaUniversalis5ModBuilder(...);

    if (normalisedGame.StartsWith("HOI4"))
        return new HOI4ModBuilder(...);

    if (normalisedGame.StartsWith("IR") ||
        normalisedGame.StartsWith("IMPERATORROME"))
        return new ImperatorRomeModBuilder(...);

    if (normalisedGame.StartsWith("VIC3") ||
        normalisedGame.StartsWith("VICTORIA3"))
        return new Victoria3ModBuilder(...);

    throw new NotImplementedException($"The game \"{settings.Mod.Game}\" is not supported");
}
```

**Matching:** Prefix-based, case-insensitive
- `CK2*` → CK2ModBuilder
- `CK3*` → CK3ModBuilder
- `EU5*` or `EUROPAUNIVERSALIS5*` → EuropaUniversalis5ModBuilder
- `HOI4*` → HOI4ModBuilder
- `IR*` or `IMPERATORROME*` → ImperatorRomeModBuilder
- `VIC3*` or `VICTORIA3*` → Victoria3ModBuilder

## Output Comparison Summary

| Aspect | CK2 | CK3 | EU5 | HOI4 | Imperator: Rome | Victoria 3 |
|--------|-----|-----|-----|------|-----------------|------------|
| **Descriptor** | Single `.mod` | Main + inner `.mod` | `.metadata/metadata.json` | Main + inner `.mod` | Main + inner `.mod` | `.metadata/metadata.json` |
| **Landed Titles** | Patched file | Merged + patched | N/A | N/A | N/A | N/A |
| **Localisation Format** | CSV (Win-1252) | YAML (UTF-8) | YAML (UTF-8 BOM) | YAML (UTF-8) | YAML (UTF-8) | YAML (UTF-8 BOM) |
| **Localisation Files** | 1 CSV | 2 YAML × 9 langs | 1 YAML × 11 langs | 1 YAML × 5 langs | 1 YAML × 4 langs | 1 YAML × 11 langs |
| **Data Files** | None | None | None | None | Province names (txt) | Scripted effects + on actions (txt) |
| **Name Normalisation** | Windows-1252 | CK3 charset | EU5 charset | HOI4 city/state | IR charset | Victoria 3 charset |
| **Directory Name** | `localisation` | `localization` | `main_menu/localization` | `localisation` | `localization` | `localization` |

## Testing Coverage

**Location:** [Mod Builder Tests](../MoreCulturalNamesBuilder.UnitTests/Service/ModBuilders/), [Mod Builder Integration Tests](../MoreCulturalNamesBuilder.IntegrationTests/)

### Current Coverage
- Factory selection logic
- Builder instantiation
- End-to-end builds against temporary directories, asserting descriptor, landed-titles, data file, and localisation content
- Coverage of the per-game output structures and identifier handling
- Error propagation for malformed identifiers and duplicate localisations
- Bounded name-normalisation invocation counts (CK3)
- Integration builds that run the complete pipeline against real XML data stores and landed titles files, asserting the exact content, encoding, and file inventory of every generated mod file

### Major Gaps
- No game compatibility verification (generated mods are not loaded in the games)

## Extension Points

To include a new game target:
1. Create new builder class inheriting from `ModBuilder`
2. Implement `LoadData()` and `GenerateFiles()`
3. Add case to `ModBuilderFactory.GetModBuilder()`
4. Add game-specific normalisation to `NameNormaliser` if needed
5. Include tests for the new builder

## Related Documentation
- [Architecture Overview](architecture-overview.md) - Builder layer in context
- [Service Layer](service-layer.md) - Services consumed by builders
- [Name Normalisation](name-normalisation.md) - Game-specific charsets
- [Data Models](data-models.md) - Models used in generation