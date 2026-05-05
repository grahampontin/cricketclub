# Frontend Spec — Abandon Match (Live Scoring)

**Relates to backend changes in:** `POST /api/LiveScoring/{matchId}/abandon`  
**Date:** May 2026

---

## 1  Overview

When a live match is cut short by rain, bad light, or any other cause, the scorer needs a way to end it cleanly without going through the normal end-of-innings flow. This spec describes the single UI change required: an **Abandon Match** button (and its confirmation dialog) in the live scoring screen.

The backend will:
- Mark the match as abandoned.
- Close any innings that were in progress.
- Write all ball-by-ball data collected so far into the permanent scorecards, **without overwriting any data that was already manually entered**.

---

## 2  API endpoint

```
POST /api/LiveScoring/{matchId}/abandon
Content-Type: application/json

{
  "reason": "rain"      // optional free-text string; omit or send null for no reason
}
```

### Responses

| Status | Meaning |
|--------|---------|
| `204 No Content` | Match abandoned successfully. |
| `400 Bad Request` | No live ball-by-ball coverage is currently in progress for this match. Response body contains a plain-text error message. |

After a `204` the UI should behave identically to after a `force-end`: navigate away from the live scoring screen (or reload the scorecard view in read-only mode).

---

## 3  UI changes

### 3.1  Where to add the button

The **Abandon Match** button should appear on end of over screen for batting or the opposition score screen when we are bowling. It should be visually distinct from primary actions — use a warning/danger style (e.g. amber or red outline) so it is not accidentally tapped.

Suggested label: **Abandon Match**

Only show the button while `getIsBallByBallInProgress` is `true` (i.e. the same condition that gates all other live scoring actions).

---

### 3.2  Confirmation dialog

Because abandonment is irreversible, always prompt before calling the API.

**Dialog content:**

> **Abandon this match?**
>
> The match will be marked as abandoned and any ball-by-ball data recorded so far will be saved to the scorecard.  
> This cannot be undone.
>
> _(Optional)_ **Reason** `[text input — placeholder: "e.g. rain, bad light"]`
>
> \[**Cancel**\]   \[**Abandon Match**\]

- The **Reason** field is optional. If left blank, send `null` (or omit the field) in the request body.
- **Cancel** dismisses the dialog with no side-effects.
- **Abandon Match** calls the API (see §2).

---

### 3.3  Loading / error states

| State | Behaviour |
|-------|-----------|
| API call in flight | Disable both dialog buttons; show a spinner on the **Abandon Match** button. |
| `204` success | Close dialog → navigate away from live scoring view (same behaviour as after force-end). |
| `400` or network error | Keep dialog open; show the error message below the buttons. Allow the scorer to retry or cancel. |

---

## 4  What the scorer will see — example flow

1. It starts raining in over 17.
2. Scorer taps **Abandon Match**.
3. Confirmation dialog opens. Scorer types "rain" in the Reason field.
4. Scorer taps **Abandon Match** in the dialog.
5. Spinner shows briefly while the API call completes.
6. Dialog closes; scorer is returned to the match summary / scorecard view.
7. The match now shows as **abandoned** and the scorecard contains all 17 overs of data.

---

## 5  Out of scope

- There is no "undo abandon" feature. Once confirmed the match is permanently marked abandoned.
- No UI changes are needed to the scorecard view itself — the backend already sets `abandoned: true` on the match, and the existing result display already renders "abandoned" for such matches.
- No changes are needed to the opposition score entry screen.

