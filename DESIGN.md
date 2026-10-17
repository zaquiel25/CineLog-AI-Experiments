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
- Dose rule: **max 2 signature appearances per screen** (rating dots in lists do not count, they are data).

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

Two roles, both from Fontshare (self-host as woff2 in production).

| Role | Font | Weights | Where |
|---|---|---|---|
| Interface | **Switzer** | 300, 400, 500, 600 | Everything that is the app: nav, headings, body, buttons, metadata |
| Film voice | **Zodiak** | 400 | **Only** movie titles (cards, detail pages, suggestions) |

Contrast by extremes: headlines pair 600 with 300 (e.g. "Every film you watch." 600 + "And the next one." 300 in `--fr-muted`).

Scale (px): 13 · 15 · 17 · 19 · 23 · 30 · 44 · 60 · 72. Letter-spacing: -0.035em for 44+, -0.02em for 23-30, 0 below.

## Spacing and shape

- Spacing in multiples of 8 (4 allowed for tight inline gaps).
- Radius: buttons and chips `980px` (pill), cards `14px`, posters `8px`.
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

## Human touch (declared)

- Hand-drawn underline on the active nav link, derived from the logo's stroke.
- Slight rotation of fanned posters in the hero.

## Assets

- Posters come from TMDB (real content, not generated).
- No AI-generated images.
