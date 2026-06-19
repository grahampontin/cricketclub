# UI Agent Task — Live Scorecard: Full Opposition Innings Display

## What to build

The live scorecard page already shows a basic opposition score header and (when
`theirInningsIsBallByBall === true`) an "At the crease" panel, a batting card,
and a bowling card. A recent backend update enriches the `InPlayScorecardV1`
response with a **full parallel set of "Their innings" fields**, mirroring every
element that exists for "Our innings". This task updates the live scorecard to
display all of that new data.

Read `docs/frontend-opposition-ball-by-ball-spec.md` for the original scoring
flow. **All scoring UI (over entry, mode selection, etc.) is unchanged.** This
task affects only the *read-only scorecard display* rendered alongside or below
the scoring controls.

---

## Updated API types

### Changed fields on `InPlayScorecardV1`

The following fields have been **upgraded to a richer type** — the previous
`OppositionBatterStateV1` is replaced by `OppositionBatterScorecardLineV1`:

```ts
// Before (old type — do NOT use for display any more):
theirOnStrikeBatsman?: OppositionBatterStateV1 | null;
theirOtherBatsman?:    OppositionBatterStateV1 | null;

// Now (richer type with dismissal info):
theirOnStrikeBatsman?: OppositionBatterScorecardLineV1 | null;
theirOtherBatsman?:    OppositionBatterScorecardLineV1 | null;
```

### New fields on `InPlayScorecardV1`

```ts
// ── New fields added in this release ────────────────────────────────────────

// Last dismissed opposition batter (null if no wickets have fallen)
theirLastBatsmanOut?:         OppositionBatterScorecardLineV1 | null;

// Current and previous partnerships
theirCurrentPartnership?:     OppositionPartnershipV1 | null;
theirPreviousPartnership?:    OppositionPartnershipV1 | null;
theirPartnerships?:           OppositionPartnershipV1[] | null;

// Fall of wickets
theirFallOfWickets?:          OppositionFallOfWicketV1[] | null;

// Per-over cumulative summaries (like CompletedOvers but for their innings)
theirBallByBallCompletedOvers?: OppositionOverSummaryV1[] | null;

// Current and previous bowlers (our VCC players bowling in their innings)
theirBowlerOneDetails?:       OppositionBowlerDetailsV1 | null;  // most recent bowler
theirBowlerTwoDetails?:       OppositionBowlerDetailsV1 | null;  // previous bowler
```

### New types

```ts
interface OppositionBatterScorecardLineV1 {
  batsmanName: string;
  score:       number;
  ballsFaced:  number;
  fours:       number;
  sixes:       number;
  strikeRate:  number;
  wicket?:     OppositionWicketV1 | null;  // non-null when dismissed
}

interface OppositionPartnershipV1 {
  batsmanOneName:  string;
  batsmanTwoName:  string;
  score:           number;
  ballCount:       number;
  batsmanOneScore: number;
  batsmanTwoScore: number;
  fours:           number;
  sixes:           number;
  runRate:         number;
  oversAsString:   string;   // e.g. "3.2"
}

interface OppositionFallOfWicketV1 {
  wicketNumber:         number;
  teamScore:            number;
  overAsString:         string;
  bowlerPlayerId:       number;
  bowlerName:           string;   // empty string — resolve from XI using bowlerPlayerId
  outgoingBatsmanName:  string;
  outgoingBatsmanScore: number;
  notOutBatsmanName:    string;
  notOutBatsmanScore:   number;
  wicket?:              OppositionWicketV1 | null;
  partnership?:         OppositionPartnershipV1 | null;
}

interface OppositionOverSummaryV1 {
  over:               OppositionOverV1;   // full ball-by-ball data for this over
  scoreAtEndOfOver:   number;
  wicketsAtEndOfOver: number;
  scoreForThisOver:   number;
}

// Already defined (unchanged):
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

> **Note on `bowlerName` in `OppositionFallOfWicketV1`:** The backend currently
> returns an empty string for `bowlerName`. Always resolve the name client-side
> from `bowlerPlayerId` using the XI player list stored at match start.

---

## When to show

All new panels described below are only rendered when:

```ts
inPlayData.theirInningsIsBallByBall === true
```

If `theirInningsIsBallByBall` is `false` (per-over summary mode), the existing
their-score header is the only opposition display — no change required there.

---

## Change 1 — Upgrade "At the crease" panel

The existing panel already shows the on-strike and non-striking batters. Update
it to use the richer `OppositionBatterScorecardLineV1` type (same fields, no
layout change needed for the basic display).

**Additionally, add a "Last wicket" line** beneath the two batters when
`theirLastBatsmanOut` is non-null:

```
┌─ At the crease ──────────────────────────────────────────────┐
│  Smith *     34 (28)  SR 121  4×4  0×6                        │
│  Jones       18 (14)  SR 129  2×4  0×6                        │
│                                                               │
│  Last wicket: Brown  12 (9)  b Williams                       │
└───────────────────────────────────────────────────────────────┘
```

- Source: `inPlayData.theirLastBatsmanOut`
- Format dismissal using the same helper as the batting card (see Change 3).

---

## Change 2 — Current partnership

Add a **"Current partnership"** strip **below** the "At the crease" panel,
mirroring what is shown for our innings. Show it when
`theirCurrentPartnership` is non-null.

```
┌─ Current partnership ────────────────────────────────────────┐
│  Smith / Jones     Runs: 52   Balls: 42   RR: 7.4            │
│  (Smith 34, Jones 18)                                         │
└───────────────────────────────────────────────────────────────┘
```

- Source: `inPlayData.theirCurrentPartnership`
- Fields: `score` (total runs), `ballCount`, `runRate`, `batsmanOneScore` /
  `batsmanTwoScore` (individual contributions), `oversAsString`.
- If `theirPreviousPartnership` is non-null, show a one-line summary of it in a
  muted/smaller style:

  ```
  Previous: Brown / Smith   28 runs (21 balls)
  ```

---

## Change 3 — Their batting card (update existing panel)

The panel introduced in the previous release showed basic batting data. Update
it with the following improvements:

1. **Dismissal column** — `wicket` is now populated on each
   `OppositionBatterScorecardLineV1`. Format using the standard dismissal helper:

   | Mode of dismissal | Display |
   |---|---|
   | `"bowled"` | `b {bowlerName}` |
   | `"caught"` | `c {fielderName} b {bowlerName}` |
   | `"c&b"` | `c&b {bowlerName}` |
   | `"lbw"` | `lbw b {bowlerName}` |
   | `"stumped"` | `st {fielderName} b {bowlerName}` |
   | `"run out"` | `run out ({fielderName})` |
   | `"hit wicket"` | `hit wicket b {bowlerName}` |
   | `"retired"` / `"retired hurt"` | literal |
   | null | `batting` (currently at crease) or `not out` (innings complete) |

   Resolve `bowlerPlayerId` and `fielderPlayerId` to names from the XI player
   list. `fielderName` is optional — omit the fielder portion if
   `fielderPlayerId` is null.

2. **Not out suffix** — when the innings is complete
   (`inPlayData.theirInningsStatus === "Completed"`) and `wicket` is null,
   show `not out` instead of `batting`.

3. **Position column** — use the position from `theirLiveBattingCard` (list is
   already ordered by position from the server, but render the position number
   explicitly).

**Full layout:**

| Pos | Batsman | Dismissal | R | B | 4s | 6s | SR |
|-----|---------|-----------|---|---|----|----|-----|
| 1 | Smith | batting | 34 | 28 | 4 | 0 | 121.4 |
| 2 | Jones | batting | 18 | 14 | 2 | 0 | 128.6 |
| 3 | Brown | b Williams | 12 | 9 | 1 | 0 | 133.3 |

Source: `inPlayData.theirLiveBattingCard` (already sorted by position).

---

## Change 4 — Fall of wickets (new, collapsible)

Add a **"Fall of wickets"** collapsible panel, collapsed by default, below the
batting card. Show it when `theirFallOfWickets` is non-null and non-empty.

```
┌─ Fall of wickets ─────────────────────────────────────────────┐
│  1-28  (Brown 12, 5.4 ov)  b Williams                         │
│  2-51  (Davis 8, 9.1 ov)   c Patel b Jones                    │
└───────────────────────────────────────────────────────────────┘
```

- Source: `inPlayData.theirFallOfWickets`, sorted by `wicketNumber` (already
  sorted from server).
- Format each line:
  ```
  {wicketNumber}-{teamScore}  ({outgoingBatsmanName} {outgoingBatsmanScore},
  {overAsString} ov)  {dismissalText}
  ```
- Dismissal text: use the same helper as Change 3, using `fow.wicket` and
  resolving `bowlerPlayerId` / `fielderPlayerId` from the XI player list.

---

## Change 5 — Over-by-over wagon wheel / over summary (new, collapsible)

Add an **"Over by over"** collapsible panel showing the cumulative score at the
end of each completed opposition over. Collapsed by default.

```
┌─ Over by over ────────────────────────────────────────────────┐
│  Ov 1   6 runs   0 wkts   (Total: 6-0)   [ 1 4 1 . wd . ]   │
│  Ov 2   8 runs   0 wkts   (Total: 14-0)  [ 4 1 . 2 1 . ]    │
│  Ov 3   7 runs   1 wkt    (Total: 21-1)  [ . . W 4 2 . ]    │
└───────────────────────────────────────────────────────────────┘
```

- Source: `inPlayData.theirBallByBallCompletedOvers`.
- Each row shows:
  - Over number (`over.overNumber`)
  - Runs scored that over (`scoreForThisOver`)
  - Wickets fallen that over (derive: `wicketsAtEndOfOver` minus previous over's
    `wicketsAtEndOfOver`, or use `over.balls.filter(b => b.wicket).length`)
  - Cumulative total: `{scoreAtEndOfOver}-{wicketsAtEndOfOver}`
  - Ball-by-ball summary: map each ball in `over.balls` to a single character:
    `"."` = dot, `"4"` = four, `"6"` = six, `"W"` = wicket, `"wd"` = wide,
    `"nb"` = no ball, otherwise the `amount` as a digit.
- If `over.commentary` is non-null and non-empty, show it beneath the ball
  summary in a muted italic style.

---

## Change 6 — Upgrade bowling panel (update existing)

The existing "Our bowling" panel shows `theirLiveBowlingCard`. Add two new
sub-panels above the full list:

### 6a — Current and previous bowler highlight

Mirror the "Bowler one / bowler two" widget used for our innings. Show when
`theirBowlerOneDetails` is non-null:

```
┌─ Bowling ─────────────────────────────────────────────────────┐
│  Williams ►   3-0-18-1  (this spell: 3-0-18-1)   Econ 6.0    │
│  Patel        2-1-8-0   (this spell: 2-1-8-0)    Econ 4.0    │
│                                                               │
│  Full bowling card ▼                                          │
│  ─────────────────────────────────────────────────────────    │
│  Williams   3-0-18-1  Econ 6.0                                │
│  Patel      2-1-8-0   Econ 4.0                                │
└───────────────────────────────────────────────────────────────┘
```

- `theirBowlerOneDetails` = bowler who bowled the most recent over (► marker).
- `theirBowlerTwoDetails` = bowler who bowled the over before that (may be null
  if only one bowler has bowled).
- Display: `{playerName}  {overs}-{maidens}-{runs}-{wickets}  Econ {economy}`.
- `JustThisSpell` is not available for opposition bowlers — show full innings
  figures only.

### 6b — Full bowling card

Keep the existing collapsible table of all bowlers from `theirLiveBowlingCard`.
No change to content; just ensure the current/previous bowler highlight (6a)
appears above it.

---

## Change 7 — Partnerships history (new, collapsible)

Add a **"Partnerships"** collapsible panel, collapsed by default, showing all
completed and in-progress partnerships.

```
┌─ Partnerships ────────────────────────────────────────────────┐
│  1st  Smith / Jones*   ongoing  52 runs (42 balls)            │
│  Last: Brown / Smith   28 runs  (21 balls)  [ended: 1-28]     │
└───────────────────────────────────────────────────────────────┘
```

Or expanded as a table:

| # | Batters | Runs | Balls | RR | Ended at |
|---|---------|------|-------|-----|---------|
| 1st | Jones / Brown | 28 | 21 | 8.0 | 1-28 |
| 2nd | Smith / Davis* | 52 | 42 | 7.4 | ongoing |

- Source: `inPlayData.theirPartnerships`.
- The **last entry** in the list is always the current (in-progress) partnership.
- Mark the current partnership with asterisk or "ongoing" badge.
- For completed partnerships, show the fall-of-wicket score if available by
  cross-referencing `theirFallOfWickets[index - 1]`.
- Ordinal suffix: 1st, 2nd, 3rd, 4th, …

---

## Layout summary

The complete opposition innings scorecard section (when
`theirInningsIsBallByBall === true`) should render in this order:

```
Their innings: {score}/{wickets} after {theirLastCompletedOver} overs  (RR {theirRunRate})

  ┌─ At the crease ──────────────────────────────────────┐  ← existing, updated (Change 1)
  │  Smith *   34 (28)  SR 121  4×4  0×6                  │
  │  Jones     18 (14)  SR 129  2×4  0×6                  │
  │  Last wicket: Brown 12 b Williams                      │
  └────────────────────────────────────────────────────────┘

  ┌─ Current partnership ────────────────────────────────┐  ← new (Change 2)
  │  Smith / Jones   52 runs  42 balls  RR 7.4            │
  └────────────────────────────────────────────────────────┘

  ┌─ Bowling ────────────────────────────────────────────┐  ← updated (Change 6)
  │  Williams ►  3-0-18-1  Econ 6.0                      │
  │  Patel       2-1-8-0   Econ 4.0                      │
  │  Full card ▼                                          │
  └────────────────────────────────────────────────────────┘

  [ Their batting card ▼ ]                                  ← existing + updated (Change 3)
  [ Fall of wickets ▼ ]                                     ← new (Change 4)
  [ Over by over ▼ ]                                        ← new (Change 5)
  [ Partnerships ▼ ]                                        ← new (Change 7)
```

---

## Implementation notes

1. **Player name resolution.** Several new types (`OppositionFallOfWicketV1`,
   `OppositionBatterScorecardLineV1` wicket, `OppositionOverSummaryV1` ball
   wickets) carry `bowlerPlayerId` / `fielderPlayerId` integers. Always resolve
   these to display names using the `matchState.players` array captured at match
   start — the same array used for the over-entry bowler/fielder selectors.

2. **`bowlerName` on `OppositionFallOfWicketV1`** is always an empty string from
   the server. Ignore it; use `bowlerPlayerId` + XI lookup instead.

3. **Null/empty guards.** All new arrays (`theirFallOfWickets`,
   `theirPartnerships`, `theirBallByBallCompletedOvers`) may be null when no
   data has been submitted yet (start of innings). Render nothing rather than an
   empty panel.

4. **Reuse existing VCC innings components** wherever possible (batting card
   row, over summary row, fall-of-wicket row, bowling card row, partnership
   strip). The data shapes are close enough that most can be adapted with a thin
   adapter/prop mapping.

5. **`theirYetToBat`** (already in the API) lists opposition batters whose
   names are known but have not yet batted. This can optionally be shown at the
   bottom of the batting card under a "Yet to bat" row group — the same
   treatment as `yetToBat` for our innings.

6. **Collapsed-by-default panels** should persist their open/closed state in
   component local state (not in localStorage). Panels reset to collapsed on
   navigation.

7. **No changes needed** to the scoring flow, `nextState` handling, or any API
   write paths. This task is display-only.

