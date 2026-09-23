# VocabMate design system

## Direction

Quiet luxury for a focused, offline vocabulary workspace. Warm ivory, olive-charcoal and a restrained bronze accent replace the purple palette. Solid surfaces, fine borders and whitespace do the work; no gradients, blur, decorative motion or external font dependency.

The ui-ux-pro-max minimalism guidance informs the direction. Its initial Apple glass / marketing-page recommendation is intentionally not used: this is a Windows and Android learning tool, not a landing page.

## Shared foundations

Source of truth: `MauiApp1/Themes/StudyTheme.xaml`.

| Role | Light | Dark |
| --- | --- | --- |
| Page | `#F6F4EF` | `#181A18` |
| Surface | `#FFFFFF` | `#232622` |
| Field | `#EEECE6` | `#2C302A` |
| Main text | `#252A25` | `#F3F1EA` |
| Supporting text | `#616359` | `#BDBFB2` |
| Bronze text | `#74603B` | `#D9C398` |
| Primary action | `#303A30` | `#D9C398` |

- Open Sans Regular / Semibold, already bundled; retain system text scaling.
- Page headings: 30 DIP mobile, 38 DIP desktop; section headings: 17 / 19; body: 15; supporting text: 14.
- Card radius: 14; button radius: 10; dialog radius: 16 DIP.
- Buttons and popup actions: minimum 48 DIP; input fields / pickers / search: minimum 52 DIP.
- Primary actions have a filled surface; secondary actions have an outline. Destructive actions remain visually separate and keep confirmation.
- Both themes have explicit foreground/background pairs. Unit tests verify normal text contrast of at least 4.5:1 across the shared surfaces; this is not a substitute for a full accessibility audit.
- Focus, pointer-over, pressed and disabled states remain available. Decorative artwork is excluded from the accessibility tree.

## Adaptive layout

Source of truth: `MauiApp1/Controls/LayoutMetrics.cs`.

- Gutters: 20 DIP below 600 wide, 28 below 1200, then 40.
- Dashboard and collection workspaces: maximum 1360 DIP wide.
- Class, deck and import: two panes only at 1000 x 560 DIP or larger. Form gets 36%, list gets 64%; otherwise both share one vertical list viewport. This also applies to Android tablets.
- Library tiles use one to three columns, targeting 320 DIP minimum width when multiple columns are shown.
- Wide dashboards place the learning banner beside compact statistics. Narrow layouts stack them without horizontal scrolling.
- Settings / editor forms use adaptive columns. Learning content remains bounded to 800 DIP for reading comfort.
- Page safe areas account for system bars; scroll views account for the software keyboard. Native Shell navigation is retained on both platforms.

## Screen-level changes

- Library: calmer brand header, compact overview, balanced tile density and existing search/create/resume flows.
- Class and deck: clearer hierarchy, persistent form labels and a visually distinct study setup panel.
- Card editor: a visible save button and distinct inline validation messages; autosave is preserved.
- Import: visible choose-file, template and import buttons; preview and confirmation behavior are preserved.
- Learning: grouped session progress, larger flashcard typography, a quieter result panel. Existing flip, keyboard, answer, retry and resume actions are retained.
- History: compact empty state and grouped result summary.
- Settings: separate language / appearance cards, live theme sample and a distinct local-data section.
- Dialogs: larger comfortable actions, more breathing room, separate destructive actions and scrollable body with persistent footer.
- Brand SVGs, splash and Android native colors follow the same palette. Native colors live in tracked `PlatformConfiguration/AndroidColors.xml`, not the ignored platform scaffold.
- Windows and Android flashcard motion honors the platform animation setting.

## Validation

```powershell
dotnet test MauiApp1.Tests/MauiApp1.Tests.csproj -c Release
dotnet build MauiApp1/MauiApp1.csproj -c Release -f net10.0-windows10.0.19041.0
dotnet build MauiApp1/MauiApp1.csproj -c Release -f net10.0-android
```

Before release, exercise these scenarios on both platforms:

- Light, dark and system appearance; Vietnamese and English; restart to verify preferences.
- Phone portrait / landscape, small desktop window and large desktop or tablet; resize across the split-workspace breakpoint while editing.
- Large system text, long deck names and vocabulary terms; no clipped action labels or horizontal page overflow.
- Empty library, no search results, creation, editing, invalid input, Excel preview and confirmed import.
- Flashcard, multiple choice, written answers, matching, result, retry and resume.
- Keyboard Tab / Shift+Tab / Enter / Escape on Windows; TalkBack and software keyboard on Android.
- Dialogs with long content and keyboard open; cancel / back / outside tap must preserve the existing behavior.
- System animations disabled; flashcard face must still update immediately and correctly.

The repository's platform scaffold and bundled font directory are ignored by the existing `.gitignore`; these must be present in the local MAUI setup. This redesign does not change that repository policy.
