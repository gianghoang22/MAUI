# VocabMate visual design

## Direction

A calm, approachable learning workspace rather than a marketing website. The
ui-ux-pro-max minimalism guidance informs the clear hierarchy and restrained
decoration; the initial claymorphism/landing-page recommendation was not a fit
for a daily-use native learning app. Existing storage, learning modes, import,
pronunciation, and navigation routes remain in place.

## Visual system

- Primary action: indigo `#5147CF`, with white text.
- Light canvas / surface: `#F7F8FC` / `#FFFFFF`.
- Dark canvas / surface: `#111321` / `#1C2033`.
- Text: `#20243D` in light mode, `#F8FAFC` in dark mode.
- Secondary text: `#626A80` / `#B0B8D0`.
- Hero: `#363088`, with white and `#E1DEFF` text.
- Cards: 20 DIP corners; hero: 24; buttons: 14.
- Primary controls retain 48 DIP minimum height and native keyboard behavior.
- Focus, hover, pressed, and disabled button states are explicit.
- Colors and shared component styles live in `Themes/StudyTheme.xaml`.
- Custom SVGs live in `Assets/`, outside the repository's ignored `Resources/`.
  No downloaded images, icon packages, or additional dependencies are required.

## Screens and behavior

- **Library:** branded header, study encouragement, real class/set/card totals,
  class search, dismissible create form, and responsive class tiles. Search has
  a distinct no-results state; there are no invented streaks or statistics.
- **Class:** responsive study-set tiles and a clear create-set action.
- **Set:** study setup is visible by default; metadata editing and deletion are
  grouped in an explicitly toggled details panel.
- **Learning:** constrained reading width, prominent flashcard faces, progress,
  recall guidance, and the existing visible flip/rating controls.
- **History and settings:** shared surfaces, clearer introductions, and a more
  considered empty-history state.
- **Navigation:** native Shell navigation with matching labeled icons.
- All new interface copy is available in English and Vietnamese.

## Responsive rules

- The library uses one scrolling, virtualized collection with a dashboard header.
- Tile columns adapt to available control width: one, two, or three columns,
  targeting at least 280 DIP per tile with 16 DIP gaps.
- Class and set workspaces use a 320 DIP form column at widths of 900 DIP and
  above, on either platform. Below that width, the form joins the collection
  header so there is one scroll region instead of two competing narrow panes.
- Decorative hero artwork disappears below 800 DIP window width.
- Text remains wrap-capable and respects native font scaling.

## Validation checklist

Verified during implementation:

- Debug builds for Windows and Android: zero warnings and zero errors.
- Windows native UI: wide and 390 DIP layouts, filtering and clearing search,
  create/cancel with empty-name validation, and class navigation across the
  workspace breakpoint.
- Windows light/dark and Vietnamese/English switching; original preferences
  restored after checking.
- Android emulator: deployment, launch, dashboard rendering, and bottom tabs.
- XAML/SVG/resource XML parsing and matching English/Vietnamese resource keys.

Build both targets using the installed .NET 10 / MAUI toolchain:

```powershell
dotnet build MauiApp1/MauiApp1.csproj -f net10.0-windows10.0.19041.0
dotnet build MauiApp1/MauiApp1.csproj -f net10.0-android
```

Check light/dark themes, English/Vietnamese, zero-item and filtered states,
keyboard focus, create/cancel, class-to-set navigation, and window resizing
across the 900 DIP breakpoint. Check flashcard flipping, long terms, and large
system fonts on an Android device before release. The repository has no
automated test project; builds and native UI smoke checks are not a substitute
for a full device/accessibility regression pass.
