# Feature ideas

Detail for the ideas in [TODO.md](TODO.md). None of these are agreed yet. The GitHub repo had no issues or pull requests to draw on as of 2026-11-02, so these come from reading the code and from what input displays are used for. Sizes are rough: small is an evening, medium a few evenings, large longer.

## POS — Remember the window's position and size

`skeys9.ini` holds no position or size for the main window, so S Keys opens in the default spot and size on every start. Anyone who sets up a layout, or an OBS window capture, has to redo it each time.

**Plan (small):** add `WindowX`, `WindowY`, `WindowWidth` and `WindowHeight`, saved on exit and on shutdown. On load, check the position is on a connected monitor, since this PC has screens at negative X.

**Verify:** move and resize, restart: it comes back there. Unplug that monitor: it opens on a connected one.

**Docs when done:** CLAUDE.md § Persistence. Changelog entry.

## PAU — A pause hotkey for privacy

An input display shows everything typed, including passwords on a stream, and logs count them too. Turning it off today takes a right click, and that's the moment you can't afford to forget.

**Plan (small):** add a global hotkey with `RegisterHotKey` to toggle Disable Display, and a tray menu item. Offer it pausing the statistics as well. Show a clear sign on the display while paused.

**Verify:** the hotkey works with any program focused, and no key shows or counts while paused.

**Docs when done:** README. Changelog entry.

## OVL — Overlay mode

S Keys is a normal window with a title bar, which can't sit on top of a game or be captured without its frame.

**Plan (medium):** add settings for always on top, no title bar, a transparent background using `TransparencyKey`, and click-through using `WS_EX_TRANSPARENT`. A click-through window can't be right-clicked, so keep the tray icon showing and put the menu there. Dragging needs its own handling with no title bar.

**Verify:** keys show over a borderless game, clicks pass through, and OBS captures only the keys.

**Docs when done:** CLAUDE.md § Settings flow if the steps change. Changelog entry.

## FAD — A smooth fade

Released keys hold one flat colour and then vanish. Now that the fade colour exists, they could ease out over the Clear Delay.

**Plan (small):** while a key is released, tick about every 30 ms and draw it at a colour that moves from the text colour through the fade colour to the background. Only run that timer while a key is fading, so an idle app stays idle. Offer it as a choice next to the flat colour.

**Verify:** a key fades smoothly at 300 ms and 2000 ms, the timer stops when nothing is fading, and a press during the fade restores full colour.

**Docs when done:** changelog entry.

## FIX — Fixed key slots

Keep Keys In Place stops keys moving while held, but W can still land in a different spot on each press. For games, muscle memory reads position, not text.

**Plan (medium):** a `KeySlots.json` lists keys with a row and column. Those keys always show there, greyed or dimmed when not held, and other keys fall back to normal placement. This is most of the way to a NohBoard-style layout.

**Verify:** W A S D keep their spots through any press order, and unlisted keys don't cover them.

**Docs when done:** README, CLAUDE.md § Architecture. Changelog entry.

## HIS — Key history

Very fast taps are gone in a blink, and tutorials need to show a sequence. A running history shows what was pressed, like Carnac or KeyCastr.

**Plan (medium):** add a display mode that appends keys along a line, joins repeats as `A x3`, and fades the oldest. Held modifiers can show combinations such as `Ctrl+Shift+S`.

**Verify:** 400 fast taps all appear in order, and old entries clear after the delay.

**Docs when done:** changelog entry.

## FLT — Hide chosen keys

Some setups want only mouse buttons, or only game keys, or never `LMB`. Today every key shows.

**Plan (small):** add a `HiddenKeys` list of codes, or a `null`-like marker in `KeyNames.json`, plus an "only these keys" option. Hidden keys should still count in the statistics unless asked otherwise.

**Verify:** a hidden key never shows but still counts, and the counter window follows the same choice.

**Docs when done:** README. Changelog entry.

## VEW — A statistics viewer

S Keys logs lifetime counts, hold times, and mouse movement and click maps, but nothing reads them. `mousemovemap.log` is hex, so the data is effectively invisible to users.

**Plan (medium to large):** add a window with top keys, total presses, total time held, and mouse distance, and render the two maps as heatmap images with a PNG export. Show each monitor in place, using the signed cell coordinates.

**Verify:** the totals match the logs, and a known click pattern lands in the right place on all monitors.

**Docs when done:** README. Changelog entry.

## LAY — Key names from the user's keyboard layout

The built-in names are US: `Keys.cs` says so for `;`, `[`, `]` and `'`. On AZERTY, Dvorak or non-Latin layouts these labels are wrong until the user edits `KeyNames.json`.

**Plan (small to medium):** on first run, or by a setting, build the names with `MapVirtualKeyEx` and `GetKeyNameText` for the current layout. The built-in table stays as the fallback, and `KeyNames.json` still wins.

**Verify:** with a French layout active, the key labelled `A` on the keyboard shows `A`.

**Docs when done:** CLAUDE.md § Key codes. Changelog entry.

## KPS — Live speed and hold times

For rhythm games and typing, the live rate matters. S Keys already has the events and stopwatches for it.

**Plan (small):** show keys per second and clicks per second on the display, and optionally a hold timer next to each held key. Keep the timer on the UI thread, and only run it while a key is held.

**Verify:** a steady 10 taps per second reads 10, and a 1.5 s hold shows about 1.5 s.

**Docs when done:** changelog entry.

## ALN — Alignment and growth direction

The display always starts at the top left. An overlay in a screen corner wants to grow inward from that corner.

**Plan (small to medium):** add left, centre and right alignment, and top-down or bottom-up rows. Layout is already in pixels, so this mostly maps rows and X at paint time.

**Verify:** each combination keeps keys in place, and the scroll line stays clear of them.

**Docs when done:** changelog entry.

## KCP — Key cap look

Text on a plain background is hard to read at a glance, particularly over a game.

**Plan (medium):** optionally draw a rounded box behind each key, with colours per group, such as modifiers, mouse buttons and the rest.

**Verify:** boxes line up, repaint without flicker, and the fade applies to the box too.

**Docs when done:** changelog entry.

## INJ — Mark injected input

Low-level hooks can tell real input from input sent by macros and scripts, using a flag on each event. S Keys ignores that flag today. Speedrunners and competitive players could show that their input is real, and statistics could leave out macro noise.

**Plan (small):** read `LLKHF_INJECTED` and `LLMHF_INJECTED` in `Hooks`, pass it on the events, and offer a colour for injected keys and an option to leave them out of the logs.

**Verify:** a key sent by `SendInput` shows in that colour, and a real key doesn't.

**Docs when done:** changelog entry.

## ONE — Single instance

Nothing stops a second copy. Each one installs its own hooks and rewrites the same logs. Both see the same input, so little is lost, but it's confusing and a late starter misses what the other hasn't saved yet. This was only read.

**Plan (small):** take a named mutex in `Program.Main`. If it's taken, bring the running copy's window forward and exit.

**Verify:** a second launch shows the first window, and only one process keeps running.

**Docs when done:** changelog entry.

## AUT — Start with Windows

For a tool that logs lifetime statistics, a day it wasn't started is a gap in the data.

**Plan (small):** add a setting that writes to the `Run` registry key, and a "start in tray" setting.

**Verify:** after signing in, S Keys is running in the tray.

**Docs when done:** changelog entry.

## REC — Timestamped input recording

Lifetime totals can't answer "how long did I hold that jump". A recording can, and it helps speedrun analysis and comparing runs.

**Plan (medium):** on request, write each press and release with a millisecond timestamp to a CSV. Use the hook's own timestamps, and write on a background thread, as in STL.

**Verify:** a recording of known input matches it to within a few milliseconds, and a long recording doesn't stall input.

**Docs when done:** README. Changelog entry.

## GAM — Gamepad input

Controller players get nothing from S Keys today.

**Plan (medium to large):** poll XInput on a background thread only while a pad is connected, and raise button events with new pseudo-input names. Sticks need their own drawing.

**Verify:** each button shows and clears, and with no pad connected the polling costs nothing noticeable.

**Docs when done:** CLAUDE.md § Key codes. Changelog entry.
