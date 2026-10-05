# Name Normalisation

This document describes the linguistic transformation rules applied by `NameNormaliser` for each game target.

## Overview

`NameNormaliser` converts Unicode names to the limited character sets supported by each game engine. The transformation pipeline consists of shared preprocessing followed by game-specific character mappings.

## Transformation Pipeline

```
Input Name
    │
    ▼
ApplyCommonReplacements()  ← Shared preprocessing (all games)
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

## ApplyCommonReplacements (Shared)

Applied by all game-specific methods in order:

| Step | Pattern | Replacement | Description |
|------|---------|-------------|-------------|
| 1 | `\bɸ` | `P` | Greek phi at word start |
| 2 | Unicode → Latin | Various | CommonCharacterMappings (e.g., `А`→`A`, `α`→`a`) |
| 3 | Zero-width chars | `` | ZWJ, ZWNJ, zero-width spaces |
| 4 | Unicode apostrophes | `'`, `"`, `` ` ``, `´` | Apostrophe normalisation |
| 5 | En/em dashes | `-` | Dash normalisation |
| 6 | Directionality marks | `` | LTR/RTL marks |
| 7 | Invisible chars | `` | Various invisible Unicode |
| 8 | Letterform alternatives | Standard Latin | Unicode lookalikes (e.g., `𝖠`→`A`) |
| 9 | Floating diacritics | Precomposed | Combining marks → precomposed |
| 10 | Special | Various | `ḡ`→`ğ`, `ڭ`→`ġ`, etc. |

## Game-Specific Character Maps

### CK3CharacterMappings

Maps Unicode to CK3-supported Latin Extended characters.

| Unicode | Replacement | Notes |
|---------|-------------|-------|
| `Ǣ` | `Æ` | |
| `Ạ` | `A` | |
| `Ḃ` | `B` | |
| `Ǧ` | `Ğ` | |
| `J̌` | `Ĵ` | Special handling |
| `T̈` | `T` | Special handling |
| `āẗ` | `āh` | Special handling |

### Hoi4CityCharacterMappings

Maps to HOI4 city name charset.

| Unicode | Replacement | Notes |
|---------|-------------|-------|
| `Ǿ` | `Ø` | |
| `Ќ` | `Ќ` | |
| `Š` | `Sh` | |
| `'` (curly) | `´` | CurlyApostropheRegex |

### Hoi4StateCharacterMappings

Maps to HOI4 state name charset.

| Unicode | Replacement | Notes |
|---------|-------------|-------|
| `Č` | `Ch` | |
| `Ť` | `Ty` | |
| `Ř` | `Rz` | |
| `Ġh` | `Gh` | Special handling |
| `ġh` | `gh` | Special handling |

### ImperatorRomeCharacterMappings

Maps to IR-supported characters.

| Unicode | Replacement | Notes |
|---------|-------------|-------|
| `Ž` | `Zh` | |
| `Š` | `Sh` | |
| `Ќ` | `K` | |
| `J̌` | `J` | Special handling |
| `T̈` | `T` | Special handling |

## ReplaceUsingMap Algorithm

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

## Caching

Each method maintains its own `ConcurrentDictionary<string, string>` cache:

| Cache | Method |
|-------|--------|
| `ck3cache` | `ToCK3Charset()` |
| `hoi4citiesCache` | `ToHOI4CityCharset()` |
| `hoi4statesCache` | `ToHOI4StateCharset()` |
| `irCache` | `ToImperatorRomeCharset()` |

## Windows-1252 Encoding

Used by CK2 for CSV localisation files.

```csharp
string ToWindows1252(string text)
{
    // Uses nameNormaliser.ToWindows1252() internally
    // Cached in windows1252NameCache
}
```

## Testing

**Location:** `MoreCulturalNamesBuilder.UnitTests/Service/NameNormaliserTests.cs`, `NameNormaliserEdgeCaseTests.cs`

### Coverage
- Each game charset method
- Edge cases (empty strings, null inputs, special characters)
- Round-trip verification where applicable

### Gaps
- No integration tests with real localisation data
- No performance benchmarks
- No concurrent access stress tests