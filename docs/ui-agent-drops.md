# UI Agent Task — Dropped Catches

## What to build

Two targeted UI changes to an existing cricket scorecard app. The backend is complete. Read the full design rationale in `docs/frontend-drops-spec.md`; this file contains only the implementation instructions.

---

## API

```
GET  /api/Scorecards/{matchId}   → MatchScorecardV1
POST /api/Scorecards/{matchId}   ← MatchScorecardV1 → MatchScorecardV1
```

Relevant new types:

```ts
interface MatchDropV1 {
  playerId: number;
  drops:    number;   // number of catches this player dropped; always >= 1 when present
}

// Added to MatchScorecardV1:
drops: MatchDropV1[] | null;
```

`drops` in the GET response is always an array (never `null`). Players who dropped no catches are absent from the array — treat a missing entry as `0`.

When POSTing:
- Omit `drops` (or send `null`) to leave existing drop records unchanged.
- Send `[]` to clear all drops.
- Send `[{ playerId, drops }, …]` to replace all drops. Entries with `drops: 0` are ignored by the backend but can be omitted locally.

---

## Change 1 — Scorecard admin: new "Drops" tab

### Where

Add a **Drops** tab as the last tab in the existing scorecard admin interface.

### Player list

Populate the tab from `ourInnings.batting.entries` (already loaded). This gives the Village players who batted. Show each player as one row:

```
[player name]     [ − ]  [count]  [ + ]
```

- Initialise each count from `scorecard.drops`: `drops.find(d => d.playerId === entry.playerId)?.drops ?? 0`.
- `[ − ]` decrements, minimum 0. `[ + ]` increments, no maximum.
- A direct numeric input is also acceptable instead of stepper buttons.
- Players with `modeOfDismissal === 'DidNotBat'` may be hidden or rendered greyed-out — your choice.

### Saving

When the scorecard is saved (regardless of which tab triggered it), include `drops` in the POST body:

```ts
const dropsPayload: MatchDropV1[] = players
  .filter(p => p.dropCount > 0)
  .map(p => ({ playerId: p.playerId, drops: p.dropCount }));

// Include in POST body:
{ ...existingScorecardPayload, drops: dropsPayload }
```

Always send the `drops` field whenever a save occurs — even if it is empty (`[]`), so that the server replaces any stale data.

---

## Change 2 — Batting scorecard display: drop icons

### Where

Our Innings batting card only — no icon on the opposition batting card.

### What to render

```ts
function dropsForPlayer(playerId: number, drops: MatchDropV1[] | null): number {
  return drops?.find(d => d.playerId === playerId)?.drops ?? 0;
}
```

After each player's name in the batting table, render one icon per drop:

```tsx
{Array.from({ length: dropsForPlayer(entry.playerId, scorecard.drops) }).map((_, i) => (
  <DropIcon key={i} />
))}
```

### Icon

Implement a small `DropIcon` component. Requirements:

| Property | Value |
|----------|-------|
| Size | `1em` (matches text line height) |
| Colour | Amber / orange (`#f59e0b` or equivalent) |
| Shape | Open hands or ball-dropping motif. Use `🤲` as a Unicode fallback if a custom SVG is not available. |
| Accessible label | `aria-label="Dropped catch"` on each icon, or a wrapping element with `title="Dropped {n} catch(es)"` for the group. |

Example output for a player who dropped 2 catches:

```
5.  A. Smith  🤲🤲   c Jones b Williams   42
```

### No-drop case

If the player has 0 drops (not in the `drops` array, or `drops` is `null`), render nothing — no empty space, no placeholder.

---

## Implementation notes

- No new API calls are required. The `drops` data comes from the same `GET /api/Scorecards/{id}` that already loads the scorecard.
- No new routes or pages are needed — both changes are within existing components.
- The POST body can be assembled once and sent in a single request; no separate "save drops" call is needed.

