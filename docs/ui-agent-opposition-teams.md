# UI Agent Task — Opposition Teams Pages

## What to build

Two pages consuming the cricket club API. The backend is complete and deployed. Implement exactly what is described here; do not guess at missing details.

---

## API endpoints

### Landing page
```
GET /api/Teams/summaries
```
Returns an array. Each item:
```ts
interface TeamSummary {
  id:               number;
  name:             string;
  homeVenueName:    string | null;
  difficultyRating: 'red' | 'amber' | 'green' | 'unknown' | null;
  difficultyScore:  number | null;   // –1 to +1; null = fewer than 3 matches
  winPercentage:    number;          // 0–1 fraction — multiply by 100 to display
  played:           number;
  won:              number;
  lost:             number;
  noResult:         number;          // abandoned matches
}
```

### Detail page
```
GET /api/Teams/{id}/details
```
Returns:
```ts
interface TeamDetail {
  id:               number;
  name:             string;
  logoUrl:          string;          // always a valid URL; falls back to placeholder
  websiteUrl:       string | null;
  homeVenueId:      number | null;
  homeVenueName:    string | null;
  difficultyRating: 'red' | 'amber' | 'green' | 'unknown' | null;
  difficultyScore:  number | null;
  winPercentage:    number;          // 0–1 fraction
  matches:          MatchResult[];   // most recent first
}

interface MatchResult {
  matchId:        number;
  matchDate:      string;            // "YYYY-MM-DD"
  homeTeamName:   string;
  homeTeamScore:  string;            // e.g. "185 for 6" or "162 all out"
  awayTeamName:   string;
  awayTeamScore:  string;
  resultText:     string;            // e.g. "Won by 23 runs"
  isWinner:       boolean | null;    // true=we won, false=we lost, null=draw/no result
  isDrawn:        boolean;
  isAbandoned:    boolean;
  ourScore:       number;
  ourWickets:     number;
  theirScore:     number;
  theirWickets:   number;
  venueName:      string | null;
  matchReportText:  string | null;
  matchReportImage: string | null;
}
```

---

## Page 1 — Opposition Teams landing (`/teams`)

### Layout
Full-width table with a filter bar above it. No pagination (up to ~180 real teams after test data removed).

### Filter bar
- Free-text search box filtering on `name` and `homeVenueName` (case-insensitive, client-side)
- Difficulty toggle buttons: **All** · **Hard** · **Medium** · **Easy** · **New**
  - "New" = `difficultyRating === 'unknown' || difficultyRating == null`

### Table columns (all sortable by clicking header)

| Column | Source | Display |
|---|---|---|
| Team | `name` | Text link → `/teams/{id}` |
| Home ground | `homeVenueName` | Text, or `—` if null |
| Played | `played` | Integer |
| Won | `won` | Integer |
| Lost | `lost` | Integer |
| Win % | `winPercentage` | `Math.round(v * 100) + "%"`, or `—` if `played === 0` |
| Difficulty | `difficultyRating` | `<DifficultyBadge>` component (see below) |

**Default sort:** name ascending.

**Difficulty column sort:** use `difficultyScore` (not the bucket string) so teams sort precisely within buckets. Nulls always last regardless of direction.

```js
// sort comparator for difficulty column
(a, b, direction) => {
  if (a.difficultyScore == null && b.difficultyScore == null) return 0;
  if (a.difficultyScore == null) return 1;
  if (b.difficultyScore == null) return -1;
  const d = a.difficultyScore - b.difficultyScore;
  return direction === 'asc' ? d : -d;
}
```

### Loading / empty states
- Show a skeleton table (5 rows) while fetching.
- If the request fails show an inline error message with a retry button.
- If the filtered result is empty show "No teams match your search."

---

## Page 2 — Team detail (`/teams/:id`)

### Header card
```
┌──────────────────────────────────────────────────────┐
│ [logo 64×64]  Team Name              [DifficultyBadge]│
│               Home Ground · website.com               │
│                                                       │
│  Played  Won  Lost  No result  Win %                  │
└──────────────────────────────────────────────────────┘
```
- Logo: `<img src={logoUrl}>` — always renders (backend provides placeholder)
- Website: only show if `websiteUrl` is non-null; open in new tab
- Stats row: omit "No result" cell if it would show 0
- Win %: `Math.round(winPercentage * 100) + "%"`, or `—` if `played === 0`

### Match history table
Columns: **Date** · **Result** · **Scores** · **Venue**

| Column | Detail |
|---|---|
| Date | `matchDate` formatted as "13 Jul 2024" |
| Result | `resultText` with a coloured icon: ✅ green if `isWinner === true`, ❌ red if `false`, `—` grey if null |
| Scores | `homeTeamScore v awayTeamScore` (both team names shown in header, not repeated per row) |
| Venue | `venueName` or `—` |

Row background: light green if `isWinner === true`, light red if `false`, no tint if null.

If `matchReportText` is non-null, make the row expandable (chevron) to reveal the report text and image inline.

### Loading / error
- Spinner while fetching.
- 404 → "Team not found" message with a back link.

---

## `DifficultyBadge` component

```ts
interface DifficultyBadgeProps {
  rating: 'red' | 'amber' | 'green' | 'unknown' | null;
  // optional — show numeric score as tooltip
  score?: number | null;
}
```

| `rating` | Background | Text | Label |
|---|---|---|---|
| `"red"` | `#d9534f` | white | Hard |
| `"amber"` | `#f0ad4e` | `#333` | Medium |
| `"green"` | `#5cb85c` | white | Easy |
| `"unknown"` / `null` | `#aaaaaa` | white | New |

- Pill shape, 14 px font
- `aria-label="Difficulty: Hard"` (substitute label)
- If `score` is provided, render it as a `title` tooltip: e.g. `"Difficulty score: 0.32"`

---

## Difficulty column header tooltip

Add an info icon (ⓘ) next to the "Difficulty" column header with this tooltip:

> Difficulty is based on the margin of wins and losses, not just the win/loss count. A 10-wicket defeat counts harder than a 1-wicket defeat; a crushing win counts easier than a narrow one. Ratings are relative: the hardest third of rated teams are Red, the middle third Amber, the easiest third Green. Teams with fewer than 3 completed matches are shown as New.

---

## Notes
- `winPercentage` is **always a 0–1 fraction** from this API — never use it raw as a percentage string.
- `difficultyScore` is for sorting only; display uses `difficultyRating`.
- Both pages are public (no auth required).

