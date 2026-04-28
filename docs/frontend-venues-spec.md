# Frontend Spec — Venues Pages

**Relates to backend changes in:** `GET /api/Venues/summaries`, `GET /api/Venues/{id}/details`  
**Date:** April 2026

---

## 1  Overview

Two new pages need building:

| Page | Purpose | Primary endpoint |
|---|---|---|
| **Venues landing** | Lists every venue with headline stats and a pitch-rating badge | `GET /api/Venues/summaries` |
| **Venue detail** | Full match history for one venue, with batting-difficulty explanation | `GET /api/Venues/{id}/details` |

The existing `GET /api/Venues` and `GET /api/Venues/{id}` endpoints are unchanged; they remain available for admin/create/edit flows.

---

## 2  Core concept — pitch rating

Each venue is assigned a **batting-friendliness score** (0–100) based on how many runs are typically scored there across all completed innings:

| Score range | Label | Meaning |
|---|---|---|
| 0–20 | **Minefield** | Batsmen really struggle; wickets fall cheaply |
| 21–40 | **Difficult** | Below-average scoring; bowlers have the edge |
| 41–60 | **Balanced** | Neither batsmen nor bowlers dominate |
| 61–80 | **Batting-friendly** | Above-average scoring; batsmen comfortable |
| 81–100 | **Road** | Batsmen make loads of runs |
| — | **Unknown** | Fewer than 3 completed matches; insufficient data |

The score is calculated as `averageRunsPerInnings / 300 × 100`, capped at 100. Both teams' innings contribute to the average. A venue averaging 150 runs per innings scores 50 (balanced); a venue averaging 50 runs per innings scores ~17 (minefield).

---

## 3  TypeScript types

```ts
interface VenueStatsV1 {
  matchesPlayed: number;
  averageRunsPerInnings: number;
  difficultyScore: number | null;   // null when matchesPlayed < 3
  difficultyLabel: 'minefield' | 'difficult' | 'balanced' | 'batting-friendly' | 'road' | 'unknown';
}

interface VenueSummaryV1 {
  id: number;
  name: string;
  description: string | null;
  latitude: number | null;
  longitude: number | null;
  mapUrl: string | null;
  stats: VenueStatsV1;
}

interface VenueDetailV1 {
  id: number;
  name: string;
  description: string | null;
  latitude: number | null;
  longitude: number | null;
  mapUrl: string | null;
  stats: VenueStatsV1;
  matches: ResultV1[];             // past matches only, most-recent first
}

// ResultV1 is the same shape used on the Teams pages — see §4.3 of frontend-opposition-teams-spec.md
```

---

## 4  `GET /api/Venues/summaries` — Landing page data

### 4.1  Endpoint

```
GET /api/Venues/summaries
```

Returns all venues in a single response, ordered alphabetically by name.  
No query parameters. No authentication required.

### 4.2  Response shape

```jsonc
[
  {
    "id": 3,
    "name": "Bournemouth Sports Ground",
    "description": "Town ground with a ridge-and-furrow outfield.",
    "latitude": 50.7192,
    "longitude": -1.8808,
    "mapUrl": "https://maps.google.com/?q=...",
    "stats": {
      "matchesPlayed": 14,
      "averageRunsPerInnings": 148.5,
      "difficultyScore": 49.5,          // null when matchesPlayed < 3
      "difficultyLabel": "balanced"     // "minefield"|"difficult"|"balanced"|"batting-friendly"|"road"|"unknown"
    }
  }
]
```

### 4.3  Landing page table spec

Recommended columns (all sortable):

| Column | Source field | Display format | Notes |
|---|---|---|---|
| Venue | `name` | Plain text link → detail page | |
| Matches played | `stats.matchesPlayed` | Integer | |
| Avg runs/innings | `stats.averageRunsPerInnings` | `v.toFixed(1)` or "—" if 0 | Show "—" if `matchesPlayed === 0` |
| Pitch rating | `stats.difficultyLabel` | Coloured badge (see §6) | |
| Location | `mapUrl` | 📍 icon link or "—" | Open in new tab |

**Default sort:** alphabetical by name.  
**Recommended secondary sort:** pitch rating score descending (road → minefield → unknown last).

### 4.4  Filtering

Recommend a client-side filter bar:

- Free-text search on `name` and `description`
- Pitch-rating toggle buttons: **All / Road / Batting-friendly / Balanced / Difficult / Minefield / Unknown**

### 4.5  Loading state

Single fetch — show a skeleton table or spinner. No pagination needed.

---

## 5  `GET /api/Venues/{id}/details` — Detail page

### 5.1  Endpoint

```
GET /api/Venues/{id}/details
```

Returns 404 if the venue does not exist.

### 5.2  Response shape

```jsonc
{
  "id": 3,
  "name": "Bournemouth Sports Ground",
  "description": "Town ground with a ridge-and-furrow outfield.",
  "latitude": 50.7192,
  "longitude": -1.8808,
  "mapUrl": "https://maps.google.com/?q=...",
  "stats": {
    "matchesPlayed": 14,
    "averageRunsPerInnings": 148.5,
    "difficultyScore": 49.5,
    "difficultyLabel": "balanced"
  },
  "matches": [
    {
      "matchId": 201,
      "matchDate": "2024-08-03",
      "homeTeamName": "The Village CC",
      "homeTeamScore": "162 for 7",
      "awayTeamName": "Bournemouth CC",
      "awayTeamScore": "145 all out",
      "resultText": "Won by 17 runs",
      "isWinner": true,
      "isDrawn": false,
      "isAbandoned": false,
      "ourScore": 162,
      "ourWickets": 7,
      "theirScore": 145,
      "theirWickets": 10,
      "venueName": "Bournemouth Sports Ground",
      "matchReportText": "…",    // null if no report
      "matchReportImage": "…"   // null if no report image
    }
  ]
}
```

Matches are pre-sorted **most recent first** by the API.

### 5.3  Detail page layout spec

```
┌──────────────────────────────────────────────────────────┐
│  Bournemouth Sports Ground          [Pitch rating badge]  │
│  Town ground with a ridge-and-furrow outfield.            │
│                                                           │
│  📍 View on Google Maps                                   │
│                                                           │
│  Played: 14   Avg runs/innings: 148.5                     │
│  Pitch rating: Balanced (score: 49.5 / 100)               │
└──────────────────────────────────────────────────────────┘

Match history at this venue (most recent first)
┌────────────┬──────────────────────┬───────────────┬───────────────┐
│ Date       │ Opponents            │ Result        │ Scores        │
├────────────┼──────────────────────┼───────────────┼───────────────┤
│ 03 Aug 24  │ Bournemouth CC       │ ✅ Won 17 runs │ 162/7 v 145ao │
│ 12 Jun 23  │ Dorset Rovers        │ ❌ Lost 2 wkts │ 140ao v 141/8 │
│ 07 May 23  │ Poole & Sandbanks CC │ — Abandoned   │ —             │
└────────────┴──────────────────────┴───────────────┴───────────────┘
```

**Result colouring:** green row/icon for `isWinner === true`, red for `false`, grey for `null` (draw / abandoned).

**Map link:** if `mapUrl` is non-null, render a 📍 link that opens in a new tab. If `latitude` and `longitude` are also present, optionally embed a small map tile.

---

## 6  Pitch rating badge component

Reusable component, analogous to the `DifficultyBadge` on the Teams pages.

### Props

```ts
interface PitchRatingBadgeProps {
  label: 'minefield' | 'difficult' | 'balanced' | 'batting-friendly' | 'road' | 'unknown';
  score?: number | null;   // shown in tooltip if provided
}
```

### Render spec

| `label` | Background | Text colour | Display text |
|---|---|---|---|
| `"minefield"` | `#d9534f` (red) | White | Minefield |
| `"difficult"` | `#e07020` (orange) | White | Difficult |
| `"balanced"` | `#f0ad4e` (amber) | `#333` (dark) | Balanced |
| `"batting-friendly"` | `#5bc0de` (blue) | White | Batting-friendly |
| `"road"` | `#5cb85c` (green) | White | Road |
| `"unknown"` | `#aaaaaa` (grey) | White | New |

Recommended: pill/badge shape, 14 px font, accessible `aria-label="Pitch rating: Balanced"`.

**Tooltip (on hover):** if `score` is provided, show e.g. `"Score: 49.5 / 100 (based on 28 innings)"`. Use `stats.completedInningsCount` if available from a future API extension, or omit the innings count.

---

## 7  Pitch rating legend / info text

Include an info icon (ⓘ) next to the "Pitch rating" column header on the listing page and near the badge on the detail page. Tooltip / popover text:

> Pitch rating measures how batting-friendly a venue is, based on the average runs scored **per wicket** (batting average) there across all recorded matches. **Road** venues see batsmen dominate and wickets fall rarely; **Minefield** venues produce cheap dismissals and low totals. Venues with fewer than 3 completed matches are marked **New** — not enough data to rate.

---

## 8  `difficultyScore` — precise sorting

`stats.difficultyScore` is a continuous 0–100 value suitable for precise ordering. `stats.difficultyLabel` is for display only.

**Sort nulls last (unknowns to the bottom):**

```js
venues.sort((a, b) => {
  const sa = a.stats.difficultyScore;
  const sb = b.stats.difficultyScore;
  if (sa == null && sb == null) return 0;
  if (sa == null) return 1;    // unknown sinks to bottom
  if (sb == null) return -1;
  return sb - sa;              // descending = road first
});
```

---

## 9  Map integration (optional enhancement)

Because every venue may have `latitude` and `longitude`, an optional "Map view" toggle on the listing page could plot all venues as pins. Suggested approach:

- Use **Leaflet** (already familiar in the JS ecosystem, no API key required with OpenStreetMap tiles).
- Pin colour matches pitch rating badge colour (red = minefield, green = road).
- Clicking a pin navigates to the venue detail page.
- Fall back to table view when coordinates are null.

---

## 10  Navigation / routing

| Route | Component | Notes |
|---|---|---|
| `/venues` | `VenuesListPage` | Calls `GET /api/Venues/summaries` |
| `/venues/:id` | `VenueDetailPage` | Calls `GET /api/Venues/{id}/details` |

Add a **Venues** link to the main nav alongside the existing Teams link.

From the venue detail page, each match row should link to the existing match scorecard page (`/matches/:matchId` or equivalent).

From the Teams detail page (`/teams/:id`), the `homeVenueName` field (where present) should link to `/venues/:homeVenueId`.

---

## 11  Admin endpoint (internal only)

```
POST /api/Venues/recalculate-stats
```

Triggers a full rebuild of the `venue_stats_cache` table. Only needed after bulk data imports or manual database edits — the cache is kept current automatically whenever a match is saved. Do **not** expose this on any public-facing page; add it to an admin/developer tools screen only.

