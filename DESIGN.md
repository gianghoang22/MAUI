# VocabMate design system

## Direction

Sage Paper for a focused, offline vocabulary workspace. Soft off-white reading surfaces, pale sage panels and muted green actions replace the dark olive / bronze direction. The home screen follows the supplied Quizlet reference's information hierarchy: navigation, search and actions, a useful resume card, then compact recent study and class lists. It does not copy Quizlet branding or enlarge tiles to fill empty space. Solid surfaces, fine borders and whitespace do the work; no gradients, blur, decorative motion or external font dependency. This aims for a calmer reading experience, not a guarantee against eye fatigue.

The ui-ux-pro-max education / LMS color guidance informs the direction, with lower saturation and verified text contrast. Its marketing-page pattern is intentionally not used: this is a Windows and Android learning tool, not a landing page. Existing system / light / dark preferences are preserved.

## Shared foundations

Source of truth: `MauiApp1/Themes/StudyTheme.xaml`.

| Role | Light | Dark |
| --- | --- | --- |
| Page | `#F2F6F2` | `#18231F` |
| Surface | `#FAFBF6` | `#222F29` |
| Field | `#E8F0EB` | `#2A3931` |
| Main text | `#293D36` | `#E9F2EB` |
| Supporting text | `#52675F` | `#B5C9BD` |
| Sage text | `#356553` | `#A7D3B6` |
| Primary action | `#3F7062` | `#A7D3B6` |
| Library banner | `#DDEDE3` | `#243E31` |

- Open Sans Regular / Semibold, already bundled; retain system text scaling.
- Page headings: 30 DIP mobile, 38 DIP desktop; section headings: 17 / 19; body: 16; supporting text: 14.
- Card radius: 14; button radius: 10; dialog radius: 16 DIP.
- Buttons and popup actions: minimum 48 DIP; input fields / pickers / search: minimum 52 DIP.
- Primary actions have a filled surface; secondary actions have an outline. Destructive actions remain visually separate and keep confirmation.
- Both themes have explicit foreground/background pairs. Unit tests verify normal text contrast of at least 4.5:1 across the shared surfaces; this is not a substitute for a full accessibility audit.
- Focus, pointer-over, pressed and disabled states remain available. Decorative artwork is excluded from the accessibility tree.
- Scroll indicators are hidden on Windows and Android, including lists, dialogs, workspace panes and multiline editors. Mouse wheel, touchpad, touch and keyboard scrolling remain enabled; only the visual bars are suppressed. Shared styles also apply to derived collection controls.

## Adaptive layout

Source of truth: `MauiApp1/Controls/LayoutMetrics.cs`.

- Gutters: 20 DIP below 600 wide, 28 below 1200, then 40.
- Home: maximum 1040 DIP wide. Library: maximum 1120 DIP. Other collection workspaces retain their 1360 DIP limit.
- At a window width of 1100 DIP or more, Shell uses a 224 DIP locked sidebar. Narrow windows retain native tabs (top on Windows, bottom on Android). Navigation mode is based on window width, not the shrinking content area, and updates on resize and navigation without recreating pages.
- Class, deck and import: two panes only at 1000 x 560 DIP or larger. Form gets 36%, list gets 64%; otherwise both share one vertical list viewport. This also applies to Android tablets.
- Library rows use one column below 640 DIP of available list width and two above it. Home lists use at most two columns and preserve an empty column rather than stretching a single item across the page. Detail-page tiles retain their existing one-to-three-column layout.
- The resume card is left-aligned, bounded to 620 DIP and visible only for an unfinished, nonempty session whose deck still exists. Other items use compact icon/name/count rows; there is no fixed-height introductory banner or separate oversized statistics cards.
- Settings / editor forms use adaptive columns. Learning content remains bounded to 800 DIP for reading comfort.
- Page safe areas account for system bars; scroll views account for the software keyboard. Native Shell navigation is retained on both platforms.

## Screen-level changes

- Home: separate landing page with class search, create/open-library actions, resume progress, up to six recently studied decks, and up to six classes. Recent study is derived from recorded results and the saved session, not inferred from alphabetical order or claimed as last-opened activity. Deleted decks are excluded. Empty accounts receive an actionable explanation, not fabricated content.
- Library: persistent search, compact summary, an on-demand creation form and compact class rows with existing open/rename/delete actions. Existing `//library` routes are preserved; home can pass a search query or open the creation form.
- Class and deck: clearer hierarchy, persistent form labels and a visually distinct study setup panel.
- Card editor: a visible save button and distinct inline validation messages; autosave is preserved.
- Import: visible choose-file, template and import buttons; preview and confirmation behavior are preserved.
- Learning: grouped session progress, larger flashcard typography, a quieter result panel. Existing flip, keyboard, answer, retry and resume actions are retained.
- History: compact empty state and grouped result summary.
- Settings: separate language / appearance cards, live theme sample and a distinct local-data section.
- Dialogs: larger comfortable actions, more breathing room, separate destructive actions and scrollable body with persistent footer.
- Brand SVGs and Android native colors follow the same sage palette; the splash uses the light page background. Native colors live in tracked `PlatformConfiguration/AndroidColors.xml`, not the ignored platform scaffold.
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
