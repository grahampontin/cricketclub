# UI Agent Task — Opposition Ball-by-Ball Scoring

## What to build

Extend the live scoring screen so the scorer can optionally record the opposition innings ball-by-ball (same granularity as our own innings). The backend is complete. Read the full design rationale in `docs/frontend-opposition-ball-by-ball-spec.md`; this file contains only the implementation instructions.

All changes are within the existing live scoring feature. The summary (per-over) scoring path is **completely unchanged**.

---

## API

```
POST /api/LiveScoring/{matchId}/start-opposition-ball-by-ball
POST /api/LiveScoring/{matchId}/opposition-over
DELETE /api/LiveScoring/{matchId}/last-opposition-over
```

All three return `MatchStateV1` on `200`. See §2 of the spec for full type definitions.

### New fields on existing types

```ts
// MatchStateV1 gains:
theirInningsIsBallByBall:       boolean
oppositionPlayers?:             OppositionBatterStateV1[] | null
oppositionOnStrikeBatsmanName?: string | null
oppositionLastCompletedOver:    number

// InPlayScorecardV1 gains:
theirInningsIsBallByBall:   boolean
theirLastCompletedOver:     number
theirOnStrikeBatsman?:      OppositionBatterStateV1 | null
theirOtherBatsman?:         OppositionBatterStateV1 | null
theirYetToBat?:             OppositionBatterStateV1[] | null
theirLiveBattingCard?:      OppositionBatterScorecardLineV1[] | null
theirLiveBowlingCard?:      OppositionBowlerDetailsV1[] | null
```

New `nextState` value: `"OppositionBattingOver"` — handle this like `"BattingOver"` but using the opposition-specific components.

### Key types

```ts
interface StartOppositionBallByBallInningsV1 {
  batsmanNames: string[];  // just the opening pair (2 entries)
}

interface OppositionBallV1 {
  ballNumber:     number;
  batsmanName:    string;      // free-text name — NOT a player ID
  bowlerPlayerId: number;      // OUR player ID from the XI
  thing:          string;      // "" | "wd" | "nb" | "b" | "lb" | "p"
  amount:         number;
  wicket?:        OppositionWicketV1 | null;
  angle?:         number | null;
  isWide: boolean; isNoBall: boolean; isBoundary: boolean; isSix: boolean;
}

interface OppositionWicketV1 {
  batsmanName:     string;
  bowlerPlayerId:  number;
  fielderPlayerId?: number | null;  // OUR fielder
  modeOfDismissal: string;
  description?:    string | null;
}

interface OppositionOverV1 {
  overNumber:  number;
  balls:       OppositionBallV1[];
  commentary?: string | null;
}

interface OppositionBatterStateV1 {
  batsmanName:  string;
  position:     number;   // 1 = first opener, 2 = second opener, 3 = 1st-wicket-down, …
  state:        'Batting' | 'Waiting' | 'Out';
  currentScore: number;  ballsFaced: number;  fours: number;  sixes: number;  strikeRate: number;
}

interface OppositionInningsUpdateV1 {
  lastCompletedOver:   number;
  onStrikeBatsmanName: string;
  over:                OppositionOverV1;
  players:             OppositionBatterStateV1[];   // ALL batsmen encountered so far, not just current pair
}
```

---

## Change 1 — Scoring mode selector

### When to show

Replace the existing per-over score entry UI with a mode selector when:

```ts
nextState === "BowlingOver"
&& inPlayData.theirScore === 0
&& inPlayData.theirOver === 0
&& !inPlayData.theirInningsIsBallByBall
```

### What to render

```
How do you want to score the opposition innings?

  [ Over-by-over summary ]   [ Ball by ball ]
```

- **Over-by-over summary** — dismiss the selector; resume existing `BowlingOver` flow (no API call).
- **Ball by ball** — show the opening batters form (Change 2 below).

---

## Change 2 — Opening batters form

Shown after choosing "Ball by ball":

```
Opening batsmen

  Batter 1 (on strike)   [                    ]
  Batter 2 (non-striker) [                    ]

                     [ Start ball-by-ball scoring ]
```

- Both fields are **free-text** (`<input type="text">`); no player lookup.
- Button disabled until both are non-empty.
- On submit: `POST .../start-opposition-ball-by-ball` with `{ batsmanNames: [batter1, batter2] }`.
- On `200`: store `MatchStateV1`; `nextState` is now `"OppositionBattingOver"`.
- On error: show API error message inline; keep form open.

**Local state to initialise on success:**
```ts
knownBatsmen = [
  { name: batter1, position: 1, state: 'Batting' },
  { name: batter2, position: 2, state: 'Batting' },
]
nextPosition = 3
onStrikeName = batter1
```

---

## Change 3 — Ball-by-ball opposition over entry

Triggered when `nextState === "OppositionBattingOver"`.

Reuse the **existing ball-entry component** (the one used for `BattingOver`). The only differences are:

### 3a  Bowler selector (replaces bowler text input)

Add a dropdown/selector **above the ball grid**:

```
Bowler:  [ ▾  A. Williamson (select…) ]
```

- Populated from `matchState.players` (our XI — these have integer `playerId` and `playerName`). This array was returned by `POST .../start` at the beginning of the match.
- Stores `selectedBowlerPlayerId: number`. All balls in the over get this `bowlerPlayerId`.
- Required before any ball can be recorded; show validation error if the scorer tries to submit without selecting.

### 3b  Batsman selector (replaces player ID lookup)

Replace any player-ID-based batsman selector with a **name dropdown** populated from `knownBatsmen` filtered to `state !== 'Out'`. Auto-populate the on-strike batter for each ball; swap automatically on odd-run deliveries (same rotation logic as the existing component).

### 3c  New batsman after a wicket

When a ball is marked as a wicket during over entry:

1. Mark the dismissed batter `state → 'Out'` in `knownBatsmen`.
2. Render a text input **directly below that ball row**:
   ```
   ↳ Next batsman: [                    ] (required)
   ```
3. On confirm (blur or Enter): add to `knownBatsmen`:
   ```ts
   knownBatsmen.push({ name: newName, position: nextPosition++, state: 'Batting' })
   ```
4. The new batsman is immediately available in the batsman selector for the next ball.

### 3d  Fielder selector (for caught / stumped / run-out)

When `modeOfDismissal` is `"caught"`, `"c&b"`, `"stumped"`, or `"run out"`:
- Show an **optional** fielder selector from our XI (`matchState.players`).
- For `"c&b"`: pre-fill with the current bowler and disable.
- Store as `fielderPlayerId: number | null`.

### 3e  Submitting the over

```ts
POST /api/LiveScoring/{matchId}/opposition-over

{
  lastCompletedOver: currentOverNumber - 1,
  onStrikeBatsmanName: computedOnStrikeName,  // who faces first next over
  over: {
    overNumber: currentOverNumber,
    balls: [ /* OppositionBallV1 per ball */ ],
    commentary: commentary || null
  },
  players: allKnownBatsmen.map(b => ({
    batsmanName: b.name,
    position: b.position,
    state: b.state,
    currentScore: tallied from balls,
    ballsFaced:   tallied from balls (exclude wides),
    fours:        tallied from balls,
    sixes:        tallied from balls,
    strikeRate:   computed,
  }))
}
```

`players` must include **every batsman encountered in the innings so far** (including those already Out), not just the current pair.

### 3f  Undo

Add an **"Undo last over"** button (same style as in our innings). Calls `DELETE .../last-opposition-over`. On `200`, update state from the returned `MatchStateV1` and resync `knownBatsmen` from `matchState.oppositionPlayers`.

---

## Change 4 — Live scorecard additions

When `inPlayData.theirInningsIsBallByBall === true`, add two collapsible panels to the live scorecard (below the existing Their Score / Wickets / Overs header):

### Panel A — At the crease

```
At the crease
─────────────────────────────────────────────
  Smith *   34 (28)  SR 121   4×4  0×6
  Jones     18 (14)  SR 129   2×4  0×6
```

Source: `inPlayData.theirOnStrikeBatsman` and `inPlayData.theirOtherBatsman`.  
Asterisk = on strike.

### Panel B — Their batting card (collapsed by default)

| Pos | Batsman | Dismissal | R | B | 4s | 6s | SR |
|-----|---------|-----------|---|---|----|----|-----|
| 1 | Smith | batting | 34 | 28 | 4 | 0 | 121.4 |
| 3 | Brown | b Williams | 12 | 9 | 1 | 0 | 133.3 |

Source: `inPlayData.theirLiveBattingCard` sorted by position.  
Dismissal: format `wicket` using the same helper as existing batting cards, but resolve `bowlerPlayerId` / `fielderPlayerId` to names via the XI player list.

### Panel C — Our bowling card (collapsed by default)

```
Our bowling
─────────────────────────────────────────────
  Williams   4-0-28-2   Econ 7.0
  Patel      3-1-14-1   Econ 4.7
```

Source: `inPlayData.theirLiveBowlingCard`. Format: `O-M-R-W`.

---

## Change 5 — Hydration on load/reconnect

When the app loads or reconnects mid-innings and `matchState.theirInningsIsBallByBall === true`:

```ts
knownBatsmen = (matchState.oppositionPlayers ?? []).map(p => ({
  name:     p.batsmanName,
  position: p.position,
  state:    p.state,
}));
nextPosition = Math.max(...knownBatsmen.map(b => b.position), 2) + 1;
onStrikeName  = matchState.oppositionOnStrikeBatsmanName ?? knownBatsmen[0]?.name ?? '';
currentOverNumber = matchState.oppositionLastCompletedOver + 1;
```

---

## Implementation notes

- The `matchState.players` array (our XI with player IDs) is available from the very first `GET .../matchId` and never changes during a match. Store it in component state at match start and use it throughout for bowler/fielder selection.
- `currentScore` / `ballsFaced` / `fours` / `sixes` in the `players` payload should be computed **cumulatively from all overs submitted so far**, not just the current over. Tally as you go.
- Do not attempt to infer the full batting order upfront. Names enter the local list exactly as typed — in the order wickets fall.
- The `"BowlingOver"` per-over entry form, `"EndOfBowlingInnings"` button, and all other `nextState` branches are unchanged.

