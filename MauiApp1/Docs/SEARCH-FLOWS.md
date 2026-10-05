# Search and filter flows

## Library
Open library -> type a class name -> update tiles and visible/total count ->
open a class, or clear search to restore all classes. No matches shows a
suggestion rather than an empty library message. Creating a class clears the
query so the new class is visible.

## Class
Open class -> search set names beside the results -> update tiles and count ->
open a set, or clear search. Creating a set clears the query. Returning from a
set retains the class query and refreshes its results.

## Deck: browsing cards
Open set -> search either English or Vietnamese -> optionally select a card
status -> show the intersection of text and status matches. The default list
filter is All. Reset clears both controls. Empty sets and zero matches have
different messages. Changing a card's star status immediately removes it if
it no longer matches the selected status. Counts always reflect visible cards.

## Deck: starting a session
Choose mode, direction, and study filter -> preview matching card count ->
Start -> confirm replacement if a session is unfinished -> study. Zero matches
disables Start; Use all cards resets only the study filter. Browsing search and
list filters never silently change which cards enter a session. The learning
engine still validates the latest stored data and applies its existing limits
and prompt grouping rules; matching cards are not a promised question count.

## Learning: changing session settings
Customize -> initialize controls from the current session -> change settings
and preview matches -> Apply -> confirm replacement of an unfinished session.
Declining confirmation preserves the current session and pending choices.
Cancel (or closing Customize) discards pending choices. Reopening starts from
the current session again. Use all cards only resets the pending study filter.

## Shared rules
- Local, immediate filtering; no network request or submit button required.
- Case, surrounding/repeated whitespace, Unicode composition, and Vietnamese
  accents are ignored for text search (including d/đ).
- Search normalization is separate from answer grading and duplicate detection.
- Empty query means all items allowed by the selected list filter.
- English/Vietnamese labels update with the application language.
- History, import, editor, and settings do not currently expose search/filter
  controls and are intentionally unchanged.

## Empty results and responsive layout

- Empty states are ordinary, auto-height content immediately below the search
  controls in the collection header. Do not use native `CollectionView.EmptyView`
  here: on Windows it can cover the interactive header, and on Android it can
  clip the recovery content. Search, result count, and reset remain reachable.
- View models keep a stable `ObservableCollection` and apply incremental changes
  without `Reset` events or replacing `ItemsSource` on each keystroke. This keeps
  the search field and its keyboard focus alive through zero-result transitions.
- Library/class tiles are grouped into virtualized linear rows containing one
  to three equal-width tiles. This avoids the native Windows `FormsGridView`
  arrange failure seen when resizing an empty grid. Row regrouping is deferred
  until after the current layout pass; header controls are not rebuilt.
- Layout uses available device-independent width/height, not a transform based
  on physical screen resolution. Home has responsive gutters, spacing, headings,
  and optional artwork, with a 1600 DIP content limit. System font scaling remains
  enabled and empty-state text has no fixed height or line limit.
- Windows class/deck pages use proportional 32/68 columns from 900 DIP width and
  520 DIP height. Smaller windows stack a bounded editor scroll area above the
  results. Android keeps one scrolling header/results region. Resizing changes
  grid placement only; it does not reparent the search controls or collection.

## Regression verification (September 21, 2026)

- Reproduced the Windows empty overlay, Android first-character focus loss, and
  Windows empty-grid resize crash before fixing their respective causes.
- Windows native UI: repeated queries, blank query, clearing from the empty
  state, class/deck navigation and return, combined list filters and reset.
- Window resize checks at 360x640, 390x844, 768x600, 900x600, 1366x768, and
  1920x1080 at the test machine's current DPI, including resizing with no results.
- Android emulator: continuous typing with keyboard focus, no results, and
  empty-state recovery on Library, Class, and Deck.
- 37 isolated source-level regression checks cover accent/case/space matching,
  stable collections, zero-to-results transitions, genuinely empty data, star
  changes under filters, independent study/list filters, cancel/decline/confirm
  session changes, and all four learning modes. The temporary harness substitutes
  framework command/event plumbing; native UI checks are performed separately.
- Debug builds for Windows and Android, XML/resource validation, and diff checks.
  Physical devices, alternate Windows DPI settings, and screen readers still
  require a separate accessibility/device pass.
