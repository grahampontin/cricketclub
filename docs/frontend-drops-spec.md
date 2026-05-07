# Frontend Spec — Dropped Catches

**Relates to backend changes in:** `GET /api/Scorecards/{id}`, `POST /api/Scorecards/{id}`  
**Date:** May 2026

---

## 1  Overview

Two UI changes are required to support recording and displaying dropped catches:

| Area | Change |
|------|--------|
| **Scorecard admin** | New **Drops** tab added to the existing scorecard admin interface, allowing the scorer to record how many catches each Village player dropped during the match. |
| **Batting scorecard display** | Drop icon(s) shown inline next to a player's name in the **Our Innings** batting card — one icon per drop. |

The backend is complete. Drops are stored as part of the scorecard: the `drops` field is returned on every `GET /api/Scorecards/{id}` response and is saved when a scorecard `POST` includes it.

---

## 2  API contract

### 2.1  Types

```ts
interface MatchDropV1 {
  playerId: number;
  drops:    number;   // count of catches this player dropped (always >= 1 when present)
}

interface MatchScorecardV1 {
  ourInnings:       InningsScoreCardV1;
  theirInnings:     InningsScoreCardV1;
  matchConditions:  MatchConditionsV1;
  matchReport:      MatchReportV1;
  drops:            MatchDropV1[] | null;  // null = not yet loaded; [] = no drops recorded
}
```

### 2.2  GET — reading drops

```
GET /api/Scorecards/{matchId}
```

The `drops` array is always populated in the response (never `null` on read). Each entry is one record per player who dropped at least one catch:

```json
{
  "drops": [
    { "playerId": 5, "drops": 2 },
    { "playerId": 8, "drops": 1 }
  ]
}
```

A player who dropped no catches will not appear in the array. An empty `[]` means nobody dropped anything in the match.

### 2.3  POST — saving drops

```
POST /api/Scorecards/{matchId}
Content-Type: application/json
```

The existing scorecard POST body gains the optional `drops` field:

```json
{
  "matchConditions": { ... },
  "ourInnings":      { ... },
  "theirInnings":    { ... },
  "matchReport":     { ... },
  "drops": [
    { "playerId": 5, "drops": 2 },
    { "playerId": 8, "drops": 1 }
  ]
}
```

**Semantics:**

| `drops` value in request | Effect on stored data |
|--------------------------|----------------------|
| Field omitted (`null`) | Existing drop records are left unchanged |
| `[]` (empty array) | All drops for the match are cleared |
| `[{ playerId, drops }, …]` | All existing drops replaced with the submitted values |

Players with `drops: 0` are silently ignored.

The response body is the same `MatchScorecardV1` shape including the updated `drops` array.

---

## 3  Part 1 — Scorecard admin: Drops tab

### 3.1  Where it lives

The scorecard admin interface currently has tabs that cover our innings batting, our innings bowling, their innings batting, their innings bowling, fall of wickets, match conditions, and match report. Add a new **Drops** tab as the last tab in that list.

### 3.2  What the tab shows

The tab contains a simple list of all Village players who batted in the match. The list is derived from `ourInnings.batting.entries` (the same entries already loaded for the batting scorecard tab), each entry having a `playerId` and `playerName`.

For each player the row shows:

```
[player name]        [ - ]  [ count ]  [ + ]
```

- The count defaults to **0** when the scorecard is first opened.
- On load the count is populated from `drops` (look up each entry's `playerId` in the `drops` array; default to `0` if not found).
- `[ - ]` decrements the count (minimum 0); `[ + ]` increments it (no upper bound).
- Alternatively, the count can be a direct numeric text input.

Players who did not bat (dismissal = `DidNotBat`) may be hidden from this list, or shown with a greyed-out row — either is acceptable.

### 3.3  What "Save" does

When the user saves the scorecard from the Drops tab (or from any other tab in the same save action), include the `drops` array in the POST body:

```json
{
  "drops": [
    { "playerId": 5, "drops": 2 },
    { "playerId": 8, "drops": 1 }
  ]
}
```

Only include players whose count is >= 1. Players with a count of 0 should be omitted (or included with `drops: 0` — the backend ignores them either way).

If the complete scorecard is saved as a whole (i.e. one POST contains all tabs' data), include `drops` in that same request so the data is always kept in sync.

### 3.4  Loading / error states

| State | Behaviour |
|-------|-----------|
| On open | Populate counts from the loaded scorecard's `drops` array before the tab is first displayed. |
| Save in flight | Disable the save button; show a spinner. |
| Save succeeds | Same success feedback as the rest of the scorecard admin. |
| Save fails | Same error feedback as the rest of the scorecard admin. |

---

## 4  Part 2 — Batting scorecard display: drop icons

### 4.1  Where the icons appear

Drop icons appear in the **Our Innings** batting card only (the Village players' batting table). They are shown inline, immediately after the player's name.

The opposition batting card shows **no drop icons** — drops are a Village fielding stat and there is no equivalent data for the opposition.

### 4.2  Icon design

Use a single icon to represent one dropped catch. Repeat it once per drop (so a player who dropped two catches shows two icons side by side).

Suggested icon options (in order of preference):

1. **Custom SVG** — a gloved hand failing to close on a ball, or a ball with a downward arrow indicating it slipped through.
2. **Unicode / emoji fallback** — `🤲` (open hands, U+1F932) works well at small size; alternatively `⬇️` in a subdued colour.  
3. **A simple coloured disc/badge** with the drop count if multiple drops are hard to render side-by-side.

The icon should be:
- Small: roughly the same height as the text line (e.g. `1em`).
- Subdued in colour: amber/orange or muted grey — visually noticeable but not jarring against the scorecard.
- Accompanied by an accessible `title` or `aria-label` on hover/screen reader: `"Dropped {n} catch"` / `"Dropped {n} catches"`.

**Example layout for a player who dropped 2 catches:**

```
5. A. Smith   🤲🤲   c Jones b Williams  42
```

### 4.3  Data wiring

The batting entry has a `playerId`. Cross-reference it against the `drops` array from the scorecard:

```ts
function getDropsForPlayer(playerId: number, drops: MatchDropV1[]): number {
  return drops?.find(d => d.playerId === playerId)?.drops ?? 0;
}
```

Render one icon for each integer from 1 to `getDropsForPlayer(entry.playerId, scorecard.drops)`.

If `scorecard.drops` is `null` or the player is not found in the array, render nothing (no icons).

### 4.4  Players not in the drops array

Only players with at least one drop appear in the `drops` array. A missing entry is equivalent to `drops: 0` — no icon is shown. No special "zero drops" state is needed.

---

## 5  Out of scope

- No drop information is shown on the opposition batting card.
- No aggregate "drops" column is added to the batting stats table at this stage.
- No "undo" or history for drops — the admin just re-saves the scorecard with corrected counts.
- No validation is required: the scorer is trusted to enter the correct value.

