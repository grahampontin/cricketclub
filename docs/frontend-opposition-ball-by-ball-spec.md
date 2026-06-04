# Frontend Spec — Opposition Ball-by-Ball Scoring

**Relates to backend changes in:** `POST /api/LiveScoring/{matchId}/start-opposition-ball-by-ball`, `POST /api/LiveScoring/{matchId}/opposition-over`, `DELETE /api/LiveScoring/{matchId}/last-opposition-over`  
**Date:** June 2026

---

## 1  Overview

Live scoring currently supports two modes for the opposition innings:

| Mode | What is recorded | When to use |
|------|-----------------|-------------|
| **Summary (existing)** | Cumulative score + wickets after each over | Scorebook is nearby; quick entry |
| **Ball-by-ball (new)** | Every ball: batsman, bowler, runs, extras, dismissals | Full commentary coverage |

The scorer chooses the mode at the **start of the opposition innings**. Summary mode is completely unchanged. Ball-by-ball mode reuses the existing ball-entry component, with two key differences:

- **Batters** — free-text string names. Only the two openers are entered upfront. New names are typed in as wickets fall — no need for a full batting order upfront.
- **Bowlers** — selected from our XI (the same XI entered at match start), identified by player ID.

---

## 2  API changes

### 2.1  New endpoints

```
POST /api/LiveScoring/{matchId}/start-opposition-ball-by-ball
Content-Type: application/json

{ "batsmanNames": ["Smith", "Jones"] }   // opening pair only
```

| Status | Meaning |
|--------|---------|
| `200` | Returns `MatchStateV1`; `nextState` will be `"OppositionBattingOver"` |
| `400` | Innings not in progress, or already in ball-by-ball mode |

```
POST /api/LiveScoring/{matchId}/opposition-over
Content-Type: application/json
```
Returns `MatchStateV1` (200) or error (400). Body: `OppositionInningsUpdateV1` (see §2.3).

```
DELETE /api/LiveScoring/{matchId}/last-opposition-over
```
Returns `MatchStateV1` (200) — undo the most recently submitted opposition over.

---

### 2.2  New `nextState` value

The existing `nextState` string gains one new value:

| `nextState` | Meaning |
|-------------|---------|
| `"OppositionBattingOver"` | Opposition innings is in ball-by-ball mode; submit the next over via `POST .../opposition-over` |

All other `nextState` values (`"BowlingOver"`, `"EndOfBowlingInnings"`, etc.) are unchanged.

---

### 2.3  TypeScript types

```ts
// ── New request types ───────────────────────────────────────────────────────

interface StartOppositionBallByBallInningsV1 {
  batsmanNames: string[];   // opening pair — 2 entries
}

interface OppositionWicketV1 {
  batsmanName:    string;
  bowlerPlayerId: number;
  fielderPlayerId?: number | null;   // our fielder (catch/stumping); null for run-outs etc.
  modeOfDismissal: string;           // "bowled" | "caught" | "c&b" | "lbw" | "stumped" | "run out" | "hit wicket" | "retired" | "retired hurt"
  description?:   string | null;
}

interface OppositionBallV1 {
  ballNumber:     number;
  batsmanName:    string;
  bowlerPlayerId: number;
  thing:          string;   // "" = runs; "wd" | "nb" | "b" | "lb" | "p"
  amount:         number;
  wicket?:        OppositionWicketV1 | null;
  angle?:         number | null;
  isWide:         boolean;
  isNoBall:       boolean;
  isBoundary:     boolean;
  isSix:          boolean;
}

interface OppositionOverV1 {
  overNumber:  number;
  balls:       OppositionBallV1[];
  commentary?: string | null;
}

interface OppositionBatterStateV1 {
  batsmanName:  string;
  position:     number;   // 1 = first to bat, 2 = opener 2, 3 = 1st wicket down, …
  state:        'Batting' | 'Waiting' | 'Out';
  currentScore: number;
  ballsFaced:   number;
  fours:        number;
  sixes:        number;
  strikeRate:   number;
}

interface OppositionInningsUpdateV1 {
  lastCompletedOver:    number;           // 0-based; over just submitted is lastCompletedOver+1
  onStrikeBatsmanName:  string;           // who will face first next over
  over:                 OppositionOverV1;
  players:              OppositionBatterStateV1[];  // FULL snapshot of all batsmen so far
}
```

```ts
// ── Extended response types (new fields on existing interfaces) ─────────────

// Added to MatchStateV1:
theirInningsIsBallByBall:      boolean;
oppositionPlayers?:            OppositionBatterStateV1[] | null;
oppositionOnStrikeBatsmanName?: string | null;
oppositionLastCompletedOver:   number;

// Added to InPlayScorecardV1:
theirInningsIsBallByBall:  boolean;
theirLastCompletedOver:    number;
theirOnStrikeBatsman?:     OppositionBatterStateV1 | null;
theirOtherBatsman?:        OppositionBatterStateV1 | null;
theirYetToBat?:            OppositionBatterStateV1[] | null;
theirLiveBattingCard?:     OppositionBatterScorecardLineV1[] | null;
theirLiveBowlingCard?:     OppositionBowlerDetailsV1[] | null;

interface OppositionBatterScorecardLineV1 {
  batsmanName: string;
  score:       number;
  ballsFaced:  number;
  fours:       number;
  sixes:       number;
  strikeRate:  number;
  wicket?:     OppositionWicketV1 | null;
}

interface OppositionBowlerDetailsV1 {
  playerId:   number;
  playerName: string;
  overs:      number;
  maidens:    number;
  runs:       number;
  wickets:    number;
  wides:      number;
  noBalls:    number;
  economy:    number;
}
```

---

## 3  Scoring mode selection

### 3.1  When to show

Show a mode-selector card **instead of** the existing per-over score entry form when ALL of the following are true:

```ts
nextState === "BowlingOver"
&& inPlayData.theirScore === 0
&& inPlayData.theirOver === 0
&& !inPlayData.theirInningsIsBallByBall
```

This is the exact moment when the opposition innings has just become `InProgress` but no data has been entered.

### 3.2  Mode selector UI

```
┌──────────────────────────────────────────────────┐
│  How do you want to score the opposition innings? │
│                                                   │
│  [ Summary — enter score each over ]              │
│  [ Ball by ball — record every delivery ]         │
└──────────────────────────────────────────────────┘
```

- **Summary** — dismiss the mode selector; render the existing per-over score entry UI (no API call, `nextState` remains `"BowlingOver"`).
- **Ball by ball** — show the **opening batters form** (§3.3).
- No "back" once a mode is active (per-over data or ball-by-ball data has been submitted).

### 3.3  Opening batters form

Shown immediately after selecting "Ball by ball":

```
┌──────────────────────────────────────────────────┐
│  Opening batsmen                                  │
│                                                   │
│  Batter 1 (on strike)   [________________]        │
│  Batter 2 (non-striker) [________________]        │
│                                                   │
│              [ Start ball-by-ball scoring ]       │
└──────────────────────────────────────────────────┘
```

- Both fields are **free-text** (plain `<input type="text">`). No lookup — type the name.
- Neither may be blank; validate before the button is enabled.
- On submit:
  1. Call `POST .../start-opposition-ball-by-ball` with `{ batsmanNames: [batter1, batter2] }`.
  2. On success: store the returned `MatchStateV1`, update `nextState` → `"OppositionBattingOver"`.
  3. On error: display the API error message; keep the form open for correction.

---

## 4  Ball-by-ball over entry (OppositionBattingOver)

### 4.1  Overview

`nextState === "OppositionBattingOver"` drives the same over-entry layout used for our own batting innings, with the following substitutions:

| Our innings | Opposition innings |
|---|---|
| Batsman selected from our XI (player ID) | Batsman selected by **name** (string) from a growing list of known batsmen |
| Bowler entered as free text (their player, string name) | Bowler **selected from our XI** (player ID + name) |
| On-strike derived from last ball logic | On-strike name tracked locally and pre-populated from `oppositionOnStrikeBatsmanName` |

### 4.2  Bowler selection

A **bowler selector** is shown at the top of the over-entry form, above the ball grid. It presents our XI as a scrollable/searchable list of names:

```
Bowler for this over:  [ ▾  A. Williamson  ]
```

- Source: the `players` array from the initial `MatchStateV1` returned by `POST .../start`. These are our team's players (have integer IDs). Keep a reference to this array in component state from the start of the match.
- The selector stores the selected player's `playerId` (int). This `bowlerPlayerId` is stamped on every `OppositionBallV1` in the over.
- Default: blank — the scorer must pick one before they can record the first ball.
- The bowler cannot be changed mid-over (all balls in one over have the same bowler). If the scorer needs to change mid-over (no-ball and free hit etc.) that is an edge case handled the same way as in our own scoring.

### 4.3  Batsman tracking

The component maintains a local list of **known batsmen** (grows through the innings):

```ts
interface KnownBatsman {
  name:     string;
  position: number;    // 1-indexed batting position
  state:    'Batting' | 'Out' | 'Waiting';  // Waiting = position assigned but not yet in
}
```

Initialise from the two openers entered in §3.3:
```ts
knownBatsmen = [
  { name: batter1, position: 1, state: 'Batting' },
  { name: batter2, position: 2, state: 'Batting' },
]
nextPosition = 3
```

**Batsman selector per ball:** a dropdown or button group of currently **non-Out** batsmen. Defaults to the on-strike batsman; auto-rotates on odd-run balls (same logic as our innings). The scorer can always manually set who faced a ball.

### 4.4  New batsman on wicket

When the scorer marks a ball as a wicket during over entry:
1. Mark the dismissed batter's `state` → `"Out"` in the local list.
2. Show a **"New batsman"** text field inline, immediately below that ball row:

```
  Ball 3: [1] [2] [3] [4] [6] [W] [wd] [nb] [b] [lb]
  ↳ Wicket ✓   How out: [ caught ▾ ]   Bowler: pre-filled
           Fielder (optional): [ ▾ our fielder ]
  ↳ Next batsman: [ _______________ ] ← free-text, required
```

3. When the name is confirmed (blur or Enter), add them to the local list:
   ```ts
   knownBatsmen.push({ name: newName, position: nextPosition++, state: 'Batting' })
   ```
4. They are immediately available in the batsman selector for subsequent balls.

**No pre-populated batting order required** — names are only collected as they are needed.

### 4.5  Fielder selection (for caught/stumped)

When `modeOfDismissal` is one of `"caught"`, `"c&b"`, `"stumped"`:
- Show an optional **Fielder** selector populated from our XI.
- For `"c&b"`: the fielder is the same player as the bowler — pre-fill and disable.
- For run-outs: `fielderPlayerId` may be set (the player who ran them out) or left null.
- Store as `fielderPlayerId: number | null`.

### 4.6  Building the submission payload

At end of over, assemble `OppositionInningsUpdateV1`:

```ts
const update: OppositionInningsUpdateV1 = {
  lastCompletedOver: currentOverNumber - 1,
  onStrikeBatsmanName: computedOnStrikeName,  // who is on strike at start of NEXT over
  over: {
    overNumber: currentOverNumber,
    balls: balls.map((b, i) => ({
      ballNumber: i + 1,
      batsmanName: b.batsmanName,
      bowlerPlayerId: selectedBowlerPlayerId,
      thing: b.thing,
      amount: b.amount,
      wicket: b.wicket ?? null,
      angle: b.angle ?? null,
      isWide: b.thing === 'wd',
      isNoBall: b.thing === 'nb',
      isBoundary: (b.thing === '' && b.amount === 4) || (b.thing === 'nb' && b.amount === 5),
      isSix: (b.thing === '' && b.amount === 6) || (b.thing === 'nb' && b.amount === 7),
    })),
    commentary: commentary || null,
  },
  players: knownBatsmen.map(b => ({
    batsmanName: b.name,
    position:    b.position,
    state:       b.state,
    currentScore: computedScore(b.name),
    ballsFaced:   computedBallsFaced(b.name),
    fours:        computedFours(b.name),
    sixes:        computedSixes(b.name),
    strikeRate:   computedStrikeRate(b.name),
  })),
};
```

**Important:** `players` must include ALL batsmen encountered so far — including those already Out — not just the two currently batting.

Call `POST .../opposition-over` with this payload. On success update state from the returned `MatchStateV1`.

### 4.7  Undo (delete last over)

Show a **"Undo last over"** button (same style as in our innings scoring). Calls `DELETE .../last-opposition-over`. On success, re-render from the returned `MatchStateV1`.

Sync the local `knownBatsmen` list from `matchState.oppositionPlayers` after the undo response — the server is the source of truth for who has batted and at what position.

---

## 5  Live scorecard display changes

The live scorecard panel (shown alongside the scoring controls) gains new sections when `theirInningsIsBallByBall === true`.

### 5.1  Their innings header (existing)

Already shows: **Their Score / Wickets / Overs / Run rate**. No change required.

### 5.2  Current partnership / batsmen (new)

```
┌─ At the crease ─────────────────────────────────┐
│  Smith *    34 (28)   SR 121   4×4 0×6           │
│  Jones      18 (14)   SR 129   2×4 0×6           │
└────────────────────────────────────────────────────┘
```

Source: `inPlayData.theirOnStrikeBatsman` and `inPlayData.theirOtherBatsman`.  
Asterisk (*) marks the on-strike batter.

### 5.3  Their live batting card (new, collapsible)

Collapsed by default. Expand to show all batsmen who have batted:

| Pos | Batsman | Dismissal | Score | B | 4s | 6s | SR |
|-----|---------|-----------|-------|---|----|----|-----|
| 1 | Smith | batting | 34 | 28 | 4 | 0 | 121.4 |
| 3 | Brown | c Jones b Williams | 12 | 9 | 1 | 0 | 133.3 |
| 2 | Jones | batting | 18 | 14 | 2 | 0 | 128.6 |

Source: `inPlayData.theirLiveBattingCard`. 
- Sort by `position`.
- Dismissal: if `wicket` is non-null, format as the existing dismissal string builder (e.g. `"b Williams"`, `"c Jones b Williams"`), except the fielder/bowler names come from looking up the player ID in our XI.

### 5.4  Our bowling card (new, collapsible)

```
Our bowling
────────────────────────────────────────────────────
  Williams     4-0-28-2   Econ 7.0
  Patel        3-1-14-1   Econ 4.7
```

Source: `inPlayData.theirLiveBowlingCard`. Format: `O-M-R-W`.

---

## 6  End-of-innings

`nextState === "EndOfBowlingInnings"` behaves identically regardless of scoring mode. The existing "End innings" button and `POST .../end-innings` flow are unchanged.

---

## 7  State machine summary

```
Their innings InProgress, no data yet, B2B not active
    → show mode selector (§3.2)
        ↓ "Summary"          ↓ "Ball by ball"
        (existing flow)       opening batters form (§3.3)
                                  ↓  POST /start-opposition-ball-by-ball
                              nextState = "OppositionBattingOver"
                                  ↓  (each over)
                              POST /opposition-over  ──►  nextState = "OppositionBattingOver"
                                  ↓  (innings over)
                              nextState = "EndOfBowlingInnings"
                                  ↓
                              POST /end-innings  (existing)
```

---

## 8  Hydration from server state

On page load / reconnect, hydrate local state from `MatchStateV1`:

```ts
if (matchState.theirInningsIsBallByBall) {
  // Restore known batsmen list from server
  knownBatsmen = (matchState.oppositionPlayers ?? []).map(p => ({
    name:     p.batsmanName,
    position: p.position,
    state:    p.state as KnownBatsman['state'],
  }));
  nextPosition = Math.max(...knownBatsmen.map(b => b.position), 2) + 1;
  onStrikeName = matchState.oppositionOnStrikeBatsmanName ?? knownBatsmen[0]?.name ?? '';
  currentOverNumber = matchState.oppositionLastCompletedOver + 1;
}
```

This ensures the scorer can close and reopen the app mid-innings without losing context.

---

## 9  Out of scope

- No ability to switch from summary to ball-by-ball mid-innings (once summary overs have been entered).
- No ability to switch from ball-by-ball back to summary.
- No full-team batting order entry upfront (only the two openers + on-demand).
- No statistics derived from opposition B2B data for the stats pages (batting/bowling averages) — the ball-by-ball data is live scorecard only.

