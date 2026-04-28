# UI Agent Task — Venues Pages

## What to build

Two new pages consuming the cricket club API. The backend is complete.
Implement exactly what is described here; do not guess at missing details.

---

## API endpoints

### Listing page
```
GET /api/Venues/summaries
```
Returns an array, ordered alphabetically by name. Each item:
```ts
interface VenueSummaryV1 {
  id:          number;
  name:        string;
  description: string | null;
  latitude:    number | null;
  longitude:   number | null;
  mapUrl:      string | null;
  stats:       VenueStatsV1;
}

interface VenueStatsV1 {
  matchesPlayed:         number;
  averageRunsPerWicket:  number;   // PRIMARY metric — batting average at this venue
  averageRunsPerInnings: number;   // supplementary context only
  difficultyScore:       number | null;  // 0–100; null when matchesPlayed < 3
  difficultyLabel:       'minefield' | 'difficult' | 'balanced' | 'batting-friendly' | 'road' | 'unknown';
}
```

### Detail page
```
GET /api/Venues/{id}/details
```
Returns 404 if venue not found.
```ts
interface VenueDetailV1 extends VenueSummaryV1 {
  matches: MatchResult[];   // most recent first
}

interface MatchResult {
  matchId:          number;
  matchDate:        string;           // "YYYY-MM-DD"
  homeTeamName:     string;
  homeTeamScore:    string;           // e.g. "185 for 6" or "162 all out"
  awayTeamName:     string;
  awayTeamScore:    string;
  resultText:       string;           // e.g. "Won by 23 runs"
  isWinner:         boolean | null;   // true=won, false=lost, null=draw/no result
  isDrawn:          boolean;
  isAbandoned:      boolean;
  ourScore:         number;
  ourWickets:       number;
  theirScore:       number;
  theirWickets:     number;
  venueName:        string | null;
  matchReportText:  string | null;
  matchReportImage: string | null;
}
```

---

## Page 1 — Venues listing (`/venues`)

### Layout
Full-width table with a filter bar above it. No pagination.

### Filter bar
- Free-text search on `name` and `description` (case-insensitive, client-side)
- Pitch-rating toggle buttons: **All · Road · Batting-friendly · Balanced · Difficult · Minefield · Unknown**
  - "Unknown" = `difficultyLabel === 'unknown'`

### Table columns (all sortable)

| Column | Source | Display |
|---|---|---|
| Venue | `name` | Text link → `/venues/{id}` |
| Matches played | `stats.matchesPlayed` | Integer |
| Avg runs/wicket | `stats.averageRunsPerWicket` | `v.toFixed(1)` or `—` if `matchesPlayed === 0` |
| Pitch rating | `stats.difficultyLabel` | `<PitchRatingBadge>` component (see below) |
| Location | `mapUrl` | 📍 icon link, open in new tab; `—` if null |

**Default sort:** alphabetical by name.

**Pitch rating sort:** use `stats.difficultyScore` (not the label string). Nulls always last regardless of direction.

```js
(a, b, direction) => {
  const sa = a.stats.difficultyScore;
  const sb = b.stats.difficultyScore;
  if (sa == null && sb == null) return 0;
  if (sa == null) return 1;
  if (sb == null) return -1;
  const d = sa - sb;
  return direction === 'asc' ? d : -d;
}
```

### Loading / empty states
- Skeleton table (5 rows) while fetching.
- Request failure → inline error with a retry button.
- Empty filtered result → "No venues match your search."

---

## Page 2 — Venue detail (`/venues/:id`)

### Header card
```
┌────────────────────────────────────────────────────┐
│  Bournemouth Sports Ground      [PitchRatingBadge]  │
│  Town ground with a ridge-and-furrow outfield.      │
│                                                     │
│  📍 View on Google Maps                             │
│                                                     │
│  Played: 14   Avg runs/wicket: 22.4                 │
│  Pitch rating: Difficult (score: 40.9 / 100)        │
└────────────────────────────────────────────────────┘
```
- Map link: only render if `mapUrl` is non-null; open in new tab.
- Score line: only render if `difficultyScore` is non-null (i.e. `matchesPlayed >= 3`).

### Match history table
**Heading:** "Match history at this venue (most recent first)"

Columns: **Date · Opponents · Result · Scores**

| Column | Detail |
|---|---|
| Date | `matchDate` formatted as "13 Jul 2024" |
| Opponents | `homeTeamName` if that isn't us, otherwise `awayTeamName` |
| Result | `resultText` with ✅ green icon if `isWinner === true`, ❌ red if `false`, `—` grey if null |
| Scores | `homeTeamScore v awayTeamScore` |

Row background: light green if `isWinner === true`, light red if `false`, no tint if null.

If `matchReportText` is non-null, make the row expandable (chevron) to reveal the report inline.

### Loading / error
- Spinner while fetching.
- 404 → "Venue not found" with a back link to `/venues`.

---

## `PitchRatingBadge` component

```ts
interface PitchRatingBadgeProps {
  label: 'minefield' | 'difficult' | 'balanced' | 'batting-friendly' | 'road' | 'unknown';
  score?: number | null;   // shown in tooltip when provided
}
```

| `label` | Background | Text colour | Display text |
|---|---|---|---|
| `"minefield"` | `#d9534f` (red) | white | Minefield |
| `"difficult"` | `#e07020` (orange) | white | Difficult |
| `"balanced"` | `#f0ad4e` (amber) | `#333` | Balanced |
| `"batting-friendly"` | `#5bc0de` (blue) | white | Batting-friendly |
| `"road"` | `#5cb85c` (green) | white | Road |
| `"unknown"` | `#aaaaaa` (grey) | white | New |

- Pill shape, 14 px font.
- `aria-label="Pitch rating: Balanced"` (adjust label).
- Tooltip on hover: `"Score: 40.9 / 100"` (omit if `score` is null).

---

## Pitch rating — info tooltip

Add an ⓘ icon next to the "Pitch rating" column header and near the badge on the detail page.

> Pitch rating measures how batting-friendly a venue is, based on the average runs scored **per wicket** (batting average) across all recorded matches there. **Road** venues see batsmen dominate and wickets fall rarely; **Minefield** venues produce cheap dismissals and low totals. Venues with fewer than 3 completed matches are shown as **New** — not enough data to rate.

> **Note:** runs-per-wicket is a better measure than runs-per-innings because it captures both scoring rate *and* how hard it is to survive. A team dismissed for 150 all out is on a harder pitch than one that scored 150 for 3.

---

## Navigation / routing

| Route | Component | Notes |
|---|---|---|
| `/venues` | `VenuesListPage` | Calls `GET /api/Venues/summaries` |
| `/venues/:id` | `VenueDetailPage` | Calls `GET /api/Venues/{id}/details` |

- Add a **Venues** link to the main nav alongside the existing **Teams** link.
- Each match row should link to the existing match scorecard page (`/matches/:matchId` or equivalent).
- On the **Teams detail page**, the `homeVenueName` field should link to `/venues/:homeVenueId` where `homeVenueId` is available.

---

## Notes
- `difficultyScore` is a continuous 0–100 value — use it for sorting only; display uses `difficultyLabel`.
- `averageRunsPerWicket` is the **primary** metric on the detail page header. `averageRunsPerInnings` may be shown as supplementary context if desired but is not required.
- Both pages are public (no authentication required).
- The `POST /api/Venues/recalculate-stats` endpoint exists but is for admin/developer tools only. Do **not** expose it on any public-facing page.

