# Scrabble Word Builder

A C# .NET 8 console application that finds the highest-scoring valid Scrabble word formable from a player's rack letters, optionally combined with letters from a word already on the board.

## Prerequisites

- .NET 8 SDK

Verify your installation:

```
dotnet --version
```

## How to Build

From the solution root or project directory:

```
dotnet build ScrabbleWordBuilder/ScrabbleWordBuilder.csproj
```

## How to Run

### CLI Arguments Mode

```
dotnet run --project ScrabbleWordBuilder/ScrabbleWordBuilder.csproj -- --rack <letters> [--word <boardword>]
```

- `--rack <letters>` (required): Your rack tiles, 1 to 7 letters, A-Z only.
- `--word <boardword>` (optional): A word already on the board. Its letters are added to the pool of available tiles.

### Interactive Mode

Run without arguments to be prompted:

```
dotnet run --project ScrabbleWordBuilder/ScrabbleWordBuilder.csproj
```

You will be asked to enter your rack letters and optionally a board word.

## Examples

**Find best word from rack only:**

```
dotnet run --project ScrabbleWordBuilder/ScrabbleWordBuilder.csproj -- --rack AIDOORW
```

Output:
```
DRAW
```

(DRAW, WARD, WOOD, and WORD all score 8 points; DRAW wins alphabetically.)

**Find best word using rack + board word:**

```
dotnet run --project ScrabbleWordBuilder/ScrabbleWordBuilder.csproj -- --rack AIDOORW --word WIZ
```

Output:
```
WIZARD
```

(WIZARD scores 19 points using letters from both the rack and the board word WIZ.)

**Invalid rack (too many letters):**

```
dotnet run --project ScrabbleWordBuilder/ScrabbleWordBuilder.csproj -- --rack ABCDEFGH
```

Output:
```
Invalid input: Rack must be 1-7 letters, got 8.
```

**No word found:**

```
dotnet run --project ScrabbleWordBuilder/ScrabbleWordBuilder.csproj -- --rack ZZZZZZ
```

Output:
```
Invalid input: Too many 'Z' tiles: used 6 but only 1 available in the game.
```

**High-value tile rack:**

```
dotnet run --project ScrabbleWordBuilder/ScrabbleWordBuilder.csproj -- --rack QUIZ
```

Output:
```
QUIZ
```

## Exit Codes

- `0`: Success (word found or no word found message printed)
- `1`: Invalid input or fatal error

## Assumptions and Design Decisions

### Word Selection
The best word is defined as the one with the highest Scrabble point score, computed as the sum of each letter's individual tile value. On a tie in score, the alphabetically first word (ascending lexicographic order) is chosen.

### Letter Pool
When a board word is provided via `--word`, its letters are added to the rack letters to form the combined pool. A valid result word must be formable entirely from this combined pool, respecting tile counts.

### Tile Count Validation
The validator checks that the number of times each letter appears across both the rack and the board word does not exceed the total number of that tile in the standard English Scrabble game. This prevents impossible letter combinations.

### No Blank Tiles
Blank/wildcard tiles are not supported. All input is A-Z only.

### No Board Placement Logic
This application does not simulate board placement, premium squares, cross-word scoring, or adjacency rules. It solely finds the highest raw-score word formable from the available letters.

### Dictionary
The dictionary is stored in `Data/dictionary.txt`, one word per line. Words between 2 and 15 letters are considered. Words are uppercased on load, so the file may contain lowercase or uppercase entries.

### Data File Discovery
On startup, the application locates the `Data/` directory by first checking the executable's output directory (set by `CopyToOutputDirectory`), then walking up the directory tree. This ensures it works whether run via `dotnet run`, from the build output, or from an installed location.

## Data Files

### `Data/dictionary.txt`

- **Source:** [ENABLE (Enhanced North American Benchmark LExicon)](https://raw.githubusercontent.com/dolph/dictionary/master/enable1.txt) — a free, public-domain English word list widely used in Scrabble-like games and spell-checkers.
- **Contents:** 172,823 valid English words, one per line, in lowercase.
- **How it was obtained:** Downloaded directly from the GitHub mirror of the ENABLE1 word list:
  ```
  curl -s "https://raw.githubusercontent.com/dolph/dictionary/master/enable1.txt" \
    -o ScrabbleWordBuilder/Data/dictionary.txt
  ```
- **At runtime:** Words are trimmed and uppercased when loaded, so the lowercase source file works without modification.
- **Why ENABLE:** It is a well-established, openly licensed word list that closely mirrors the official North American Scrabble tournament word list (TWL), making it appropriate for Scrabble-style scoring applications.

### `Data/letter_data.json`

- **Source:** Standard English Scrabble tile distribution, as defined by the official Hasbro/Mattel Scrabble rules.
- **Contents:** For each letter A–Z, the JSON records:
  - `score` — the point value of one tile (e.g. A=1, Q=10, Z=10)
  - `count` — the number of tiles of that letter in a standard 100-tile English Scrabble set (excluding the 2 blank tiles, which are not supported)
- **How it was constructed:** Manually authored based on the publicly documented standard Scrabble tile set:

  | Points | Letters (tile count) |
  |--------|----------------------|
  | 1 pt   | A×9, E×12, I×9, O×8, U×4, L×4, N×6, R×6, S×4, T×6 |
  | 2 pts  | D×4, G×3 |
  | 3 pts  | B×2, C×2, M×2, P×2 |
  | 4 pts  | F×2, H×2, V×2, W×2, Y×2 |
  | 5 pts  | K×1 |
  | 8 pts  | J×1, X×1 |
  | 10 pts | Q×1, Z×1 |

- **Used for:** Input validation (tile count limits) and scoring candidate words.

## File Descriptions

| File | Description |
|------|-------------|
| `ScrabbleWordBuilder/Program.cs` | Entry point; handles CLI args and interactive mode, coordinates services |
| `ScrabbleWordBuilder/Models/LetterInfo.cs` | Data models for letter tile information and the full letter dataset |
| `ScrabbleWordBuilder/Models/ValidationResult.cs` | Result type returned by the validator indicating success or failure |
| `ScrabbleWordBuilder/Services/LetterDataService.cs` | Loads and deserializes `letter_data.json` into a per-letter lookup dictionary |
| `ScrabbleWordBuilder/Services/InputValidator.cs` | Validates rack and board word input against format rules and tile count limits |
| `ScrabbleWordBuilder/Services/WordFinder.cs` | Loads `dictionary.txt` and finds the highest-scoring word formable from the letter pool |
| `ScrabbleWordBuilder/Data/letter_data.json` | Standard English Scrabble tile distribution (scores and counts for A–Z) |
| `ScrabbleWordBuilder/Data/dictionary.txt` | ENABLE word list — 172,823 valid English words, one per line |
| `ScrabbleWordBuilder/ScrabbleWordBuilder.csproj` | .NET 8 project file; declares data files for output copy and NuGet dependencies |
