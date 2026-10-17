# FrameRoute Design System

Source of truth for the 2026 redesign. Every screen is built from these tokens. If a decision is not here, it has not been made yet.

## Goal

One thing a visitor should do before leaving: **log their first movie**. Suggestions only work after that.

## Direction

**Clean, Apple-like base + one signature taken from the logo.**

- Base: generous whitespace, thin/regular sans, obvious type and spacing system. Removing things is the default move.
- Signature (the one memorable element): **the logo's reel dots and its hand-drawn stroke**. It appears where it carries meaning, never as decoration:
  - Rating: 5 dots instead of stars
  - Active nav link: hand-drawn underline (same irregular stroke as the logo)
  - Sequences (the 3 steps): dots that fill up to show progress
  - Film strip perforation: a row of dots under poster groups
- Dose rule: **max 2 signature appearances per screen** (rating dots in lists do not count, they are data). The hand-drawn underline on the active tab or chip counts as one appearance, together with the nav (both mean "you are here").

Why: the logo is the only visual material that belongs only to FrameRoute. Gold on near-black is one of the default AI looks, so the signature is what keeps this from looking like every other dark site.

## Color

Warm blacks (projector light) + two golds: bright gold is reserved for actions, muted gold for the signature.

| Token | Hex | Use |
|---|---|---|
| `--fr-bg` | `#0D0C0A` | Page background |
| `--fr-surface` | `#171511` | Cards, panels |
| `--fr-surface-2` | `#221F19` | Chips, inputs, hover surfaces |
| `--fr-text` | `#F3EFE6` | Primary text |
| `--fr-muted` | `#9A9385` | Secondary text, metadata, inactive nav |
| `--fr-line` | `rgba(243,239,230,.09)` | Borders, dividers |
| `--fr-action` | `#F4D03F` | **Only** buttons and primary actions. Logo also uses it |
| `--fr-sig` | `#B8A15A` | Signature: rating dots, underline, perforation |
| `--fr-sig-off` | `rgba(184,161,90,.28)` | Empty rating dots, inactive perforation |
| `--fr-danger` | `#E5484D` | Destructive actions (delete) |

Rules:
- If it is bright gold, it is clickable and it is the main action. Nothing else is bright gold except the logo.
- Text on gold buttons is `#000`.
- No gradients, no glow, no decorative glassmorphism. Only exception: the sticky nav uses a light background blur so content stays legible underneath (functional, like apple.com).

## Typography

Two roles, both under the **SIL Open Font License 1.1**, self-hosted from `wwwroot/fonts` (license files included there, latin subset).

| Role | Font | Weights | Where |
|---|---|---|---|
| Interface | **Geist** | 300, 400, 500, 600 | Everything that is the app: nav, headings, body, buttons, metadata |
| Film voice | **Newsreader** | 400 | Movie titles (cards, detail pages, suggestions) **and page titles** ("My Movies", "Wishlist", "Log a movie") |

Only use open-license (OFL) fonts: the repository is public, and fonts under licenses like Fontshare's ITF FFL cannot be published in a public repo. Switzer + Zodiak were the first choice and were replaced for this reason.

Contrast by extremes: headlines pair 600 with 300 (e.g. "Every film you watch." 600 + "And the next one." 300 in `--fr-muted`).

Scale (px): 13 · 15 · 17 · 19 · 23 · 30 · 44 · 60 · 72. Letter-spacing: -0.035em for 44+, -0.02em for 23-30, 0 below.

## Spacing and shape

- Spacing in multiples of 8 (4 allowed for tight inline gaps).
- Radius: buttons `6px` (`--fr-radius-control`), tags `4px`, cards `14px`, posters `8px`. **No pills.** Pill-shaped everything (buttons, chips, tabs, fields all at 980px) was tried and read as a generic AI/template look; it also flattened the hierarchy between filters, tabs and actions.
- Tabs (`.fr-segmented`) and chips (`.fr-chip`) are **text**, muted when inactive; the active one is in `--fr-text` with the hand-drawn underline (same stroke as the nav).
- Fields (search, selects, the Add search) are **a line, not a box**: transparent, bottom border only.
- Buttons are always solid. Main action: gold (`.fr-btn`). Secondary action: quiet solid button, `--fr-surface-2` with a thin light border (`.fr-btn--secondary`). Text links with an underline were tried for secondary actions and got lost on the dark background.
- One bright gold element per screen whenever possible. In the nav, "Sign up" is text (`.fr-nav-link--strong`), not a gold button, so the hero CTA is the only gold.

## Motion

From `motion.md` (Marfil): ease-out entries `cubic-bezier(0.22, 1, 0.36, 1)`, exits ~75% of entry duration, never from `scale(0)`, hover motion only under `(hover: hover) and (pointer: fine)`, `prefers-reduced-motion` respected. No bounce.

## Home wireframe

```
+------------------------------------------------------------+
| [logo]  Home  My Movies  Wishlist  Suggestions      Log in |  nav: active link has hand-drawn underline
|         ~~~~                                               |
+------------------------------------------------------------+
|                                                            |
|  Every film you watch.            [poster][POSTER][poster] |  hero: 2 columns, text left,
|  And the next one.                  (slightly rotated)     |  fanned posters right
|                                     . . . . . . . . . .    |  perforation dots
|  Log what you see, build your                              |
|  taste, get suggestions...                                 |
|                                                            |
|  (Log a movie)  (Explore suggestions)                      |  gold button + quiet secondary button
|                                                            |
|  ---------------------------------------------------------  |
|  o.. Log what you watch   oo. Build your profile   ooo ... |  how it works: ONE quiet line,
|                                                            |  dots fill to show progress
+------------------------------------------------------------+
|  footer: quiet, warm black, small type, TMDB credit        |
+------------------------------------------------------------+
```

Hero posters: the signed-in user's last 3 logged movies (latest in the middle). Visitors, or users with fewer than 3, get TMDB trending to fill the gap.

## Page hero (shared)

`Views/Shared/_PageHero.cshtml` + `PageHeroViewModel`: a film still (TMDB backdrop) with a small caps kicker, a title in Newsreader 600, one line of context and the gold main action. Without a still it renders as plain text. Each page picks a film that means something there:

| Page | Film in the hero | Action |
|---|---|---|
| My Movies | Last watched ("Last watched · Saturday, October 17") | Log a movie |
| Wishlist | Latest added ("Latest in your Wishlist · added October 17") | Find movies |
| Suggestions | A random film the user rated 4+ (changes every visit) | none |

Add has no hero: a small caps label ("LOG A MOVIE") with a hairline, and the big search field is the protagonist.

## My Movies

Built from real references (Letterboxd diary + MUBI/A24 film pages, captured 2026-10-17): no giant page title, warmth from the film's own image, density from a table.

- **Hero**: the last watched film over its TMDB still ("Last watched · Saturday, October 17" / title in Newsreader 600 / "12 entries in your diary · 2 rewatches") with the gold "Log a movie". A dark scrim over the still is the one allowed gradient (functional, for legibility). Without movies: plain title "My Movies".
- **Bar**: Diary / Collection tabs (text, 2px muted-gold line under the active one) and the filters on the right as small caps text with a caret ("ALL MONTHS ▾", "SORT BY RECENTLY WATCHED ▾", "FIRST WATCHES ONLY"). Each opens a menu below (`details.fr-filter`); items are plain links that change one query value. Search is a small inline field.
- **Diary**: a table. Columns Month · Day · Film · Released · Rating; the month is an almanac badge on the first row of each month (on mobile it becomes a small caps heading). Film titles in Newsreader 600.
- **Collection**: poster wall (6 / 4 / 3 columns) under the same hero and bar, stats as one line of text.
- Row actions in "•••"; rating is always `_RatingDots`.

## Suggestions

- Picking a starting point uses **chips** (`.fr-chip`), never a grid of icon boxes. Chips are text; the active one gets the hand-drawn underline.
- Results appear right below the chips. Heading in normal text (never gold), with one "Shuffle" secondary button next to it.
- Suggestion card: poster, title (Newsreader), year, optional "Watched" tag. Main action "Save to Wishlist" (secondary button); "I've seen it" and "Not interested" live in the "•••" menu.
- One result (Surprise me) sits in the middle column.
- Mobile: chips and result cards scroll sideways with scroll-snap, so the first result is on the first screen.

## Wishlist

One page, two tabs (`.fr-segmented`, like Journal/Collection), each answering one question:

- **Want to see** ("what do I want to watch?"): poster wall. Poster, title (Newsreader), "year · director" (year first, so truncation only cuts the name). Search + sort. "•••" menu: "I've seen it" (opens the log form) and "Remove from Wishlist".
- **Not interested** ("what did I hide from suggestions?"): compact list, newest first. Greyed thumb, title, "year · director", "Hidden today / 3 days ago / Oct 2", "•••" with "Show in suggestions again". It is the old Blacklist (action and URL kept), no longer in the nav.
- Tab labels carry the totals. Only gold: "Find movies" (goes to Suggestions).
- One result in the wall sits in the middle column.

## Log a movie (Add)

Two steps on one page:

1. **Search** ("Log a movie", placeholder "Which film?"): one big underlined field in Newsreader. Results show poster thumb, title (Newsreader) and "year · overview", so films with the same name can be told apart. Arrow keys move through results.
2. **The log**, two shapes for the same values:
   - Desktop: a sentence next to the poster. "I watched *Aftersun* [for the first time / again] [today / yesterday / on a date / some time ago], [at home / at the cinema / somewhere else], and I'd give it ●●●●○". Each choice is a native select underlined in muted gold.
   - Mobile: poster + title on top, then chips (When / Where), rating dots, a Rewatch toggle and a full-width "Log it".
- Rating is the interactive version of the signature dots: click a dot (left half = half point), click the current value to clear, arrow keys work.
- "again" is pre-selected when the user already logged the film.
- Only gold: "Log it". "Change movie" is a secondary button.

## Movie page (Preview and Details)

One layout for both: poster on the left (sticky while scrolling), everything else on the right; on mobile it stacks.

- Header: title (Newsreader, large), one meta line "year · runtime · genres · Directed by X · TMDB 7.7", tagline in Newsreader italic.
- Preview (any film): gold "Log it" ("Log again" if already logged) opens Add with the film. "Save to Wishlist" and "Trailer" are secondary; "Not interested" lives in "•••". Tags show "In your log", "In your Wishlist" or "Not interested".
- Details (one viewing): the log read back with the same words as the Add sentence, underlined in muted gold ("You watched it again on Sunday, August 30, at home, and gave it ●●●●○"). Gold "Log again"; "Edit this viewing" and "Delete this viewing" in "•••". No Wishlist actions (the film is already watched).
- Starring: small round faces with names. Where to watch: logos only for streaming; rent/buy as one line of text, plus "Data from JustWatch" (TMDB attribution requirement).

## Human touch (declared)

- Hand-drawn underline on the active nav link, derived from the logo's stroke.
- Slight rotation of fanned posters in the hero.

## Assets

- Posters come from TMDB (real content, not generated).
- No AI-generated images.
