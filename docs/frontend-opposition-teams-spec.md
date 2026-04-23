# Frontend Spec — Opposition Teams Pages

**Relates to backend changes in:** `GET /api/Teams/summaries`, `GET /api/Teams/{id}/details`  
**Date:** April 2026

---

## 1  Overview

Two pages need building / updating:

| Page | Purpose | Primary endpoint |
|---|---|---|
| **Opposition Teams landing** | Lists every opposition team with headline stats and a difficulty badge | `GET /api/Teams/summaries` |
| **Team detail** | Full match history for one team | `GET /api/Teams/{id}/details` |

---

## 2  Breaking changes from the previous `winPercentage` version

### 2.1  `winPercentage` is now a fraction (0–1), not a percentage (0–100)

| Old value | New value | Meaning |
|---|---|---|
| `48.0` | `0.48` | Won 48% of matches |
| `100.0` | `1.0` | Won every match |
| `0.0` | `0.0` | Won no matches |

**Action required:** multiply by 100 before displaying, or format with a `%` formatter:

```js
// ✅ correct
const display = `${(team.winPercentage * 100).toFixed(0)}%`;  // "48%"

// ❌ old (wrong — would show "0.48%")
const display = `${team.winPercentage.toFixed(0)}%`;
```

### 2.2  `difficultyRating` has a new `"unknown"` value

Previously the field could only be `"red"`, `"amber"`, or `"green"`.  
Teams with **fewer than 3 completed matches** now return `"unknown"`.

| Value | Meaning | Suggested UI treatment |
|---|---|---|
| `"red"` | Hardest third of rated teams | Red badge / icon |
| `"amber"` | Middle third | Amber badge / icon |
| `"green"` | Easiest third | Green badge / icon |
| `"unknown"` | Fewer than 3 completed matches, insufficient data | Grey badge labelled "New" or "—" |

`difficultyRating` can also be `null` for teams that have never appeared in the stats cache (brand-new teams with no matches at all). Treat `null` the same as `"unknown"`.

```ts
type DifficultyRating = 'red' | 'amber' | 'green' | 'unknown' | null;

function difficultyLabel(rating: DifficultyRating): string {
  switch (rating) {
    case 'red':    return 'Hard';
    case 'amber':  return 'Medium';
    case 'green':  return 'Easy';
    default:       return 'New';   // "unknown" or null
  }
}

function difficultyColour(rating: DifficultyRating): string {
  switch (rating) {
    case 'red':    return '#d9534f';
    case 'amber':  return '#f0ad4e';
    case 'green':  return '#5cb85c';
    default:       return '#aaaaaa';
  }
}
```

### 2.3  Difficulty rating is now margin-weighted, not win%-ranked

This is a **display-only clarification** — no code change needed — but the tooltip / legend copy should be updated to reflect the new meaning:

> **Old copy:** "Difficulty is based on win/loss record against this team."  
> **New copy:** "Difficulty is based on the margin of wins and losses against this team. Heavy defeats count more than narrow ones. Ratings are relative to all other opposition teams."

---

## 3  `GET /api/Teams/summaries` — Landing page data

### 3.1  Endpoint

```
GET /api/Teams/summaries
```

Returns all opposition teams in a single response, ordered alphabetically by name.  
No query parameters. No authentication required (public endpoint).

### 3.2  Response shape

```jsonc
[
  {
    "id": 42,
    "name": "Oakwood CC",
    "homeVenueName": "Oakwood Ground",     // null if unknown
    "difficultyRating": "red",             // "red" | "amber" | "green" | "unknown" | null
    "winPercentage": 0.32,                 // fraction 0–1; multiply by 100 to display
    "played": 22,
    "won": 7,
    "lost": 13,
    "noResult": 2                          // abandoned matches
  }
]
```

### 3.3  Landing page table spec

Recommended columns (all sortable):

| Column | Source field | Display format | Notes |
|---|---|---|---|
| Team | `name` | Plain text link → detail page | |
| Home ground | `homeVenueName` | Plain text or "—" if null | |
| Played | `played` | Integer | |
| Won | `won` | Integer | |
| Lost | `lost` | Integer | |
| No result | `noResult` | Integer | Hide column if all values are 0 |
| Win % | `winPercentage` | `(v * 100).toFixed(0) + "%"` | Show "—" if `played === 0` |
| Difficulty | `difficultyRating` | Coloured badge (see §2.2) | |

**Default sort:** alphabetical by name.  
**Recommended secondary sort option:** difficulty (red → amber → green → unknown), then win% descending.

### 3.4  Filtering

Recommend a client-side filter bar:

- Free-text search on `name` and `homeVenueName`
- Difficulty toggle buttons: All / Hard / Medium / Easy / New

### 3.5  Loading state

Single fetch — show a skeleton table or spinner. No pagination required (up to ~800 rows; the response is lightweight).

---

## 4  `GET /api/Teams/{id}/details` — Detail page

### 4.1  Endpoint

```
GET /api/Teams/{id}/details
```

### 4.2  Response shape (relevant fields)

```jsonc
{
  "id": 42,
  "name": "Oakwood CC",
  "logoUrl": "https://…/images/teams/42.png",   // falls back to /images/teams/0.png
  "websiteUrl": "https://oakwoodcc.example.com", // null if unknown
  "homeVenueId": 7,                              // null if unknown
  "homeVenueName": "Oakwood Ground",             // null if unknown
  "difficultyRating": "red",                     // same values as summary (see §2.2)
  "winPercentage": 0.32,                         // fraction 0–1 (see §2.1)
  "matches": [ /* ResultV1[] — see §4.3 */ ]
}
```

### 4.3  Match history

Each entry in `matches` is a `ResultV1`:

```jsonc
{
  "matchId": 101,
  "matchDate": "2024-07-13",
  "homeTeamName": "The Village CC",
  "homeTeamScore": "185 for 6",
  "awayTeamName": "Oakwood CC",
  "awayTeamScore": "162 all out",
  "resultText": "Won by 23 runs",
  "resultMargin": "23 runs",
  "winningTeam": "The Village CC",
  "losingTeam": "Oakwood CC",
  "isWinner": true,           // true = we won; false = we lost; null = draw/no result
  "isDrawn": false,
  "isTied": false,
  "isAbandoned": false,
  "ourScore": 185,
  "ourWickets": 6,
  "theirScore": 162,
  "theirWickets": 10,
  "venueName": "Oakwood Ground",
  "matchReportText": "…",     // null if no report
  "matchReportImage": "…"     // null if no report image
}
```

Matches are pre-sorted **most recent first** by the API.

### 4.4  Detail page layout spec

```
┌─────────────────────────────────────────────────────┐
│  [Logo]  Oakwood CC           [Difficulty badge]     │
│          Oakwood Ground  ·  oakwoodcc.example.com    │
│                                                      │
│  Played: 22  Won: 7  Lost: 13  No result: 2          │
│  Win rate: 32%                                       │
└─────────────────────────────────────────────────────┘

Match history (most recent first)
┌────────────┬───────────────────┬──────────────┬──────┐
│ Date       │ Result            │ Scores       │ Venue│
├────────────┼───────────────────┼──────────────┼──────┤
│ 13 Jul 24  │ ✅ Won by 23 runs │ 185/6 v 162ao│ …   │
│ 05 Aug 23  │ ❌ Lost by 8 wkts │ 98ao v 99/2  │ …   │
└────────────┴───────────────────┴──────────────┴──────┘
```

**Result colouring:** green row/icon for `isWinner === true`, red for `false`, grey for null.

---

## 5  Difficulty badge component

Reusable component used on both pages.

### Props

```ts
interface DifficultyBadgeProps {
  rating: 'red' | 'amber' | 'green' | 'unknown' | null;
}
```

### Render spec

| `rating` | Background | Text colour | Label |
|---|---|---|---|
| `"red"` | `#d9534f` | White | Hard |
| `"amber"` | `#f0ad4e` | `#333` (dark) | Medium |
| `"green"` | `#5cb85c` | White | Easy |
| `"unknown"` or `null` | `#aaaaaa` | White | New |

Recommended: pill/badge shape, 14 px font, accessible `aria-label="Difficulty: Hard"`.

---

## 6  Tooltip / legend copy

Include a small info icon (ⓘ) next to the "Difficulty" column header. Tooltip text:

> Difficulty is calculated from the margin of every result against this team, not just win/loss counts. A 10-wicket defeat counts as much harder than a 1-wicket defeat, and a crushing run victory counts as much easier than a narrow one. Ratings are relative: the hardest third of teams (by weighted margin) are Red, the middle third Amber, and the easiest third Green. Teams with fewer than 3 completed matches are shown as New.

---

## 7  Migration notes for existing code

If the frontend previously called `GET /api/Teams/{id}/details` in a loop to build a teams list, **replace that pattern** entirely with a single call to `GET /api/Teams/summaries`. The summaries endpoint returns all ~800 teams at once with pre-computed stats.

| Old pattern | New pattern |
|---|---|
| `Promise.all(teamIds.map(id => fetch(/details/${id})))` | `fetch(/api/Teams/summaries)` |
| `winPercentage` used raw as `%` value | Multiply by 100: `(v * 100).toFixed(0) + "%"` |
| 3-value difficulty: `red \| amber \| green` | 4-value: add `unknown` / `null` → grey "New" badge |

