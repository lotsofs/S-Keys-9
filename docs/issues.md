# Defects and tasks

Detail for the items in [TODO.md](TODO.md).

Dependencies between these plans:

- **DUP and ORD:** matching keys by code (DUP) makes ORD's fix cleaner, so DUP's change to `ChangeEventArgs` could come first if both are done together.
- **LIN and LBW:** drawing the scroll line in `DisplayText_Paint` (LIN) also solves LBW, so do LIN first.
- **STK and TMR:** STK's repair runs in the hook check, so the longer interval TMR allows would also delay clearing a stuck key.

## CSZ — The counter window's resize and lock need one real try

The counter window's saved size and Lock Size haven't met a real drag. They were checked on hidden windows, by sending the message Windows sends when a resize ends and by reading the window's styles.

**Evidence:** verified that way 2026-11-02, as 32-bit and 64-bit. That Windows refuses to drag a locked window is only read. So is what happens when the window is snapped to a screen edge: `ResizeEnd` may arrive before the snap, which would leave the snapped size unsaved.

**Verify:** run with Ctrl+F5 and open the counter window. Drag an edge, then close and reopen it: it keeps the size. Restart: it still does. Turn on Lock Size: the edges no longer drag, there's no maximise button, double-clicking the title bar does nothing, and dragging it to a screen edge doesn't snap it. Turn it off: all of that works again. Snap it to a screen edge, close and reopen it, and note which size comes back.

**Docs when done:** none, unless the snap needs a fix.

## STK — A lost key-up leaves a key stuck

Windows keeps input from the hooks on the lock screen and while an elevated window has focus. A key that was down when that started never gets its key-up. Examples are the Win key in Win+L, or Alt when Alt+Tab lands on an admin window.

- The key stays on the display, indefinitely.
- `Hooks._currentlyPressed` still holds it, so its next press is dropped as auto-repeat. Only the release of that next press clears it ([Hooks.cs](../SKeys9/SKeys9/Hooks.cs)).
- With `LogButtons` on, its stopwatch keeps running, so `duration.log` gets the whole time. A later press doesn't restart it, because `Stopwatches.KeyPressStart` skips a key that has one running ([Stopwatches.cs](../SKeys9/SKeys9/Stopwatches.cs)).

**Evidence:** reproduced 2026-11-02 with real key input, as 32-bit and 64-bit, losing the key-up by removing the hooks for a moment. 2.5 s after the release the key still showed, and pressing it again raised no event. The stopwatch was reproduced the same day by turning `LogButtons` off during a hold, which leaves the same state: a 50 ms hold then logged 1,563 ms. The real triggers, a lock and an admin window, were only read.

**Plan:** in `Hooks.OnHookCheck`, while input is in reach, check each code in `_currentlyPressed` with `GetAsyncKeyState`. For any that's up, remove it and raise `OnKeyUp` or `OnMouseUp`. Skip this while input is out of reach, because the state can't be trusted then. Also stop counting a key's duration at that point rather than at its next release.

**Verify:** hold Alt, Alt+Tab to an elevated window, release, and go back: the key clears within a second, and its next press shows. Do the same with Win+L and unlock. `duration.log` doesn't gain the locked time.

**Docs when done:** CLAUDE.md § Architecture (`Hooks`). Changelog entry.

## BST — One bad value stops the app from starting, silently

Settings and logs are parsed with `int.Parse`, `bool.Parse` and fixed `Substring` calls, and nothing catches a failure ([Configuration.cs](../SKeys9/SKeys9/Configuration.cs), `Input.LoadFiles` in [Input.cs](../SKeys9/SKeys9/Input.cs)). Both run while `MainForm` is being constructed, so the app dies before it has a window.

**Evidence:** reproduced 2026-11-02. The real exe with `Size=abc` exited within 6 s, with no window and no message. The only trace is a `Fatal error` line in `debuglog.log`. By reflection on a real form, these all throw:

- In `skeys9.ini`: `Size=abc`, `Size=0`, `Size=-5`, `Style=abc`, `Color=fff`, `Color=zzzzzz`, `BackColor=`, `MinimizeToTray=1`, `LogButtons=yes`, `ClearDelay=abc`, `ClearDelay=99999999999`, `FadeOpacity=`, `FadeColor=fff`.
- In logs: a blank line, short line, bad number or bad key code in `count.log`, a bad time in `duration.log`, and a short line in `mousemovemap.log`.

`ClearDelay` and `FadeOpacity` only clamp numbers that parse.

**Plan:** add `TryParse` helpers for settings that keep the default and log a line, and check the font size is above 0. In `LoadFiles`, skip a bad line and log it. Before the save that follows, copy the damaged file to `<log>.bad`, because that save and `BackupSave` would otherwise overwrite the only good data. In `Program.Main`, catch a failure during construction and show a message box that names the problem.

**Verify:** each value above starts the app with the default and a line in `debuglog.log`. A damaged `count.log` loads every good line, and the original is kept as `count.log.bad`.

**Docs when done:** CLAUDE.md § Persistence. Changelog entry.

## WRT — A file that can't be written stops the app or raises errors

Only `File.Replace` in `Input.WriteLog` is guarded. Opening the temporary file, `BackupSave` and `SaveSettings` aren't. This hits anyone who runs S Keys from a protected folder such as Program Files, or with a read-only or locked file.

**Evidence:** reproduced 2026-11-02 by reflection on a real form:

- A read-only `skeys9.ini.bak` makes `ReadSettings` throw, which stops startup.
- `count.log.bak` open in another program makes `BackupSave` throw, which also stops startup.
- A read-only `count.log.tmp` makes `SaveFiles` throw. An autosave would hit that every 60 s, through the error dialog in `Program.OnThreadException`. That last step was only read.

**Plan:** wrap the backup writes and the whole of `WriteLog` in the same `catch` that `File.Replace` has, and log the failure once, not on every attempt. Consider falling back to `%APPDATA%` when the exe's folder can't be written.

**Verify:** repeat the cases above: the app starts, keeps running without error dialogs, and `debuglog.log` has one line for each.

**Docs when done:** CLAUDE.md § Persistence. Changelog entry.

## WDT — Keys are placed with the wrong width when hidden or resized

`UpdateText` passes `ClientSize.Width` to `HeldKeyLayout.Update`, and nothing places keys again when that width changes.

- While the window is in the tray or minimised, the width is 0, so every key takes its own row. They stay stacked after the window comes back.
- After narrowing the window, keys past the new edge stay out of sight until released. The label used to reflow by itself.

**Evidence:** reproduced 2026-11-02 with real key input, as 32-bit and 64-bit. Three keys pressed in the tray landed on rows 0, 1 and 2 and stayed there after restoring. Five keys held in a 500 px window stayed at x positions up to 248 after narrowing to 170 px.

**Plan:** remember the last width above 0 and use that while hidden. In `MainForm_Resize`, once the window is back to normal, place again any key that no longer fits.

**Verify:** the two cases above: keys share a row after restoring, and narrowing moves hidden keys onto a new row.

**Docs when done:** none. This came with Keep Keys In Place, which is unreleased.

## RDR — The display doesn't refresh properly after settings or Disable Display

`MainForm.UpdateAppearance` clears the key layout on every settings event, although only a font change makes the measured widths wrong. So a colour, slider or logging change makes held keys jump back to press-order spots, breaking Keep Keys In Place, and drops greyed keys at once. That also means dragging Fade Opacity can't preview the fade on a real released key.

Turning Disable Display off clears the display but doesn't redraw, so keys held at that moment only appear at the next input.

**Evidence:** reproduced 2026-11-02 by reflection on a real form.

**Plan:** in `UpdateAppearance`, clear the layout only when the font's name, size or style changed. Otherwise just invalidate. In `ToolStripMenuItemDisableDisplay_CheckedChanged`, call `UpdateText(this, _lastChange)` when there is one.

**Verify:** hold keys with gaps and change a colour: nothing moves. Set a long Clear Delay, tap a key and drag the slider: the grey key changes colour and stays. Hold a key and turn the display back on: it shows at once.

**Docs when done:** none. This came with unreleased changes.

## ORD — Pressing a greyed key again breaks press order

`HeldKeyLayout.Update` matches a pressed key to its greyed entry, so the key returns to its old spot. That's right by default, but New Keys Fill Gaps off promises keys stay in press order, and so does Keep Keys In Place off.

**Evidence:** reproduced 2026-11-02 by reflection on a real form. Hold A then B, release A and press it again: the held order is `B, A`, but the display shows `A B`, in both modes.

**Plan:** when filling is off, or Keep Keys In Place is off, drop the greyed entry and place the key as new, at the end.

**Verify:** the case above shows `B A` in both modes, and still `A B` with the default settings.

**Docs when done:** none. This came with unreleased changes.

## HLD — Counts and timed clears still wait during a title bar hold

Windows holds the UI thread in a wait after a press on a title bar or title bar button of S Keys' own windows, for about 500 ms or until the mouse moves, and until release on the minimise button. `MainForm.UpdateText` shows held keys during that wait by painting inside the hook callback, but these still need the message loop:

- The counter window's numbers. `CounterForm.UpdateText` calls `Display.Invalidate()`, which leaves the repaint to the loop ([CounterForm.cs](../SKeys9/SKeys9/CounterForm.cs)).
- A released key going, and scroll counters clearing. `_keyRemoveTimer` in `MainForm` and `_scrollRemoveTimer` in `Input` are WinForms timers, which tick through the loop. A released key also goes on the next input, because that runs `UpdateText`.

**Evidence:** only read, 2026-11-02.

**Plan:** call `Display.Update()` after `Display.Invalidate()` in `CounterForm.UpdateText`, if the cost is worth it: that repaint would run inside the hook on each event, and it took 2.6 ms with 60 entries on show, measured 2026-11-02. Leave the timers unless the delay proves noticeable: the posted `WM_MOUSEMOVE` in STL's plan would end the title bar wait for them, but not the one on the minimise button.

**Verify:** hold a title bar and press a key: the counter window's number changes at once. Tap a key and hold a title bar before its delay is up: the key goes on time.

**Docs when done:** CLAUDE.md § Architecture (`Hooks`). Changelog entry.

## CNT — A closed counter window stays attached

`CounterForm`'s constructor adds `ChangeCount` to `input.OnKeysChanged`, and nothing removes it ([CounterForm.cs](../SKeys9/SKeys9/CounterForm.cs)). Each counter window that's closed stays in memory and keeps sorting and counting on every input, inside the hook path. `MainForm` also leaves its `OnSettingsChanged` link to a closed counter window.

**Evidence:** reproduced 2026-11-02 by reflection on a real form. Opening and closing it 10 times left 11 handlers on the event, with the closed forms disposed. Key presses and settings changes didn't throw, and those 10 added about 37 µs per key event.

**Plan:** remove both links in a `FormClosed` handler.

**Verify:** open and close the window repeatedly: the event keeps one handler, and memory doesn't grow.

**Docs when done:** none.

## STL — Slow work on the UI thread stalls all input on the PC

Low-level hooks run on the thread that installed them, which here is the UI thread. So anything slow on that thread makes every key press and mouse move on the PC wait.

**Evidence:** reproduced 2026-11-02 with real input as 32-bit, timing `SendInput`, which waits for the hooks. The Debug build was used, and saves wrote to an SSD, so a hard drive would be slower.

| Situation | Median | Worst |
|---|---|---|
| S Keys hooks off | 0.16 ms | 1.2 ms |
| Hooks on, idle | 0.22 ms | 1.3 ms |
| A save with 20,000 click cells | 0.22 ms | 31 ms |
| Opening the settings window | 0.22 ms | 27 ms |
| Opening the font dialog | 0.20 ms | 243 ms |
| A deliberate 100 ms job on the UI thread | 0.21 ms | 101 ms |

Normal running adds only about 0.06 ms per event. The concern is the stall with logging on, once a minute: saves took 13 ms with 600 click cells and 36 ms with 62,000, by direct timing. With logging off, a save writes nothing.

**Plan:** first, run saves on a background thread. `SaveFiles` already works from copies, except `_mouseInteractions`, which needs copying too. Allow one save at a time, and keep the exit save synchronous. Skip a save when nothing changed. Second, and only if still needed, run the hooks on their own thread with their own message loop and post events to the UI thread.

That second step has to replace the repaint inside the hook callback. Windows holds the UI thread in a wait after a press on a title bar or title bar button, and `DisplayText.Update()` in `MainForm.UpdateText` is what shows keys during it, which only works while the hooks run on that thread. Instead, post a `WM_MOUSEMOVE` to the window from its `WndProc` on `WM_NCLBUTTONDOWN` with `HTCAPTION`, which ends the wait with nothing painted in the hook. Each window needs the override, and it relies on undocumented behaviour of that wait. Tried from a scratch harness on 2026-11-02: first paint at 14 to 22 ms on all three title bars and 1.8 ms for a key during a hold, and dragging and double-click to maximise still work. It doesn't help on the minimise button, which stays at about 1520 ms.

**Verify:** repeat the `SendInput` timing during saves: the worst case stays near 1 ms. No data is lost across an exit save, as in MTS.

**Docs when done:** CLAUDE.md § Architecture and § Threading. Changelog entry.

## TMS — Restarting the key timer is the costliest part of a key press

`MainForm.ScheduleKeyRemoval` stops and restarts `_keyRemoveTimer` on every key event. A WinForms timer destroys its hidden window on stop and creates one on start.

**Evidence:** measured 2026-11-02 on a real form, with reflection overhead included: about 40 µs per key event with greying off and about 210 µs with it on. The window explanation is only read. It's small against the callback limit of about 310 ms, but it's on the hook path, and easy to avoid.

**Plan:** leave a running timer alone when a key is released, because keys go in release order with one delay, so the next one due can't be sooner. Its tick reschedules anyway. Restart it only when the delay setting changes.

**Verify:** the timing above drops close to the greying-off figure, and each key still goes at its own time.

**Docs when done:** none.

## LIN — Key rows and the scroll line use different line heights

`DisplayText_Paint` spaces key rows by `Font.Height`, while the blank lines that hold the scroll line down use the label's GDI line height. These differ for most fonts, so with wrapped rows the keys drift from the scroll line, or overlap each other.

| Font | Rows use | Label lines use |
|---|---|---|
| Microsoft Sans Serif 18 pt bold | 28 px | 29 px |
| Segoe UI 24 pt | 43 px | 45 px |
| Consolas 24 pt | 38 px | 37 px |
| Times New Roman 36 pt | 56 px | 55 px |

The split also leaves `&` half fixed: held keys draw it, but a scroll direction with `&` in its name still goes through the label's mnemonic handling. That part was only read.

**Evidence:** line heights measured 2026-11-02.

**Plan:** draw the scroll line in `DisplayText_Paint` too, one measured line height below the last row, with `NoPrefix`, and wrapped at the window's width. The label then keeps only the startup and disabled messages, and the blank lines go. This also solves LBW.

**Verify:** at each font above, wrapped rows don't overlap and the scroll line sits one line under them. With a single row, 36 captures still match the old build.

**Docs when done:** CLAUDE.md § Architecture (`HeldKeyLayout`).

## DUP — Keys with the same name swap slots

`Input` keeps held keys by display name, so `HeldKeyLayout.Update` matches them by name in list order. When two held keys share a name, it can't tell which one was released. `0x0C` and `0xFE` are both `Clear` by default, and `KeyNames.json` can add more.

**Evidence:** reproduced 2026-11-02 by reflection on a real form. Hold both `Clear` keys and release the left one: the right slot greys instead.

**Plan:** add key codes to `ChangeEventArgs` next to the names and match placed keys by code. `CounterForm` can keep using names.

**Verify:** the case above greys the left slot, and two keys sharing a name still share a counter in the counter window.

**Docs when done:** CLAUDE.md § Key codes (the note about `Input` keying by name).

## WHL — Wheel events are counted instead of wheel notches

`Hooks.MHookCallback` only looks at the sign of the wheel delta, so one event counts as one scroll whatever its size. A fast wheel can send several notches in one event. A high-resolution wheel sends many small ones.

**Evidence:** reproduced 2026-11-02 with real input: one event with a delta of 360, which is three notches, showed as `ScrUp 1`. Over-counting on a high-resolution wheel was only read.

**Plan:** add up the delta per direction and count one scroll for each 120, carrying the remainder. This changes `count.log`'s scroll counts going forward.

**Verify:** a delta of 360 counts 3, and four deltas of 30 count 1.

**Docs when done:** changelog entry.

## SOR — Scroll counters don't show in the order they started

`Input.AddScrollCounters` adds a counter for each direction at startup, always in the order up, down, left, right ([Input.cs](../SKeys9/SKeys9/Input.cs)), and `MainForm.UpdateText` shows them in that order. The direction you started scrolling first should come first, the way held keys show in press order.

**Evidence:** reproduced 2026-11-02 on a real form: scrolling down, down, right, up shows `ScrUp 1 ScrDn 2 ScrRight 1`.

**Plan:** keep the counters as they are, because `CounterForm` compares each one with its previous value, so they have to stay zeroed rather than be removed. Have `Input` also keep a list of direction names in the order they started: `AddScroll` adds a name that isn't in it yet, and `ResetScrolls` empties it. Pass the list in `ChangeEventArgs` and have `UpdateText` loop over that. Directions with the same name share an entry, as they share a counter.

**Verify:** scroll down, right, up within the clear delay: the line shows `ScrDn`, `ScrRight`, `ScrUp`. Let it clear, then scroll up first: `ScrUp` comes first. The counter window's counts don't change.

**Docs when done:** CLAUDE.md § Architecture (`Input`). Changelog entry.

## SGO — Key Display settings are below Counter Display

The Key Display group is at the bottom of the settings window, below Counter Display ([SettingsForm.Designer.cs](../SKeys9/SKeys9/SettingsForm.Designer.cs)). It's about the main display, so it should come before the counter window's settings.

**Plan:** in the designer file, put `GroupBoxKeyDisplay` at (12, 196) and `groupBoxCounter` below it at (12, 326), and swap their `TabIndex` values. The window keeps its size.

**Verify:** the settings window shows Key Display under Font and Logging, with Counter Display below it. Nothing overlaps, and Tab goes through them in that order.

**Docs when done:** none while Key Display is unreleased.

## TMR — Hook check interval is hardcoded

The hook check interval is 1000 ms in the `Hooks` constructor ([Hooks.cs](../SKeys9/SKeys9/Hooks.cs)). It should be a setting.

**Plan:** add a static field in `Configuration`, a `case` in `LoadSettings` that clamps the value as `ClearDelay` does, and a line in `ApplySettings`. Set the timer interval from this field. `Hooks` is created after `ReadSettings`, so startup already works, but a change made in the settings window must also update the running timer. Open decision: offer a control in `SettingsForm`, or only a `skeys9.ini` key.

**Verify:** set the value in `skeys9.ini` and restart. The check then runs at the set interval. Press OK in the settings window, and the value is still in the file.

**Docs when done:** CLAUDE.md § Architecture (1 s becomes a default). Changelog entry for the new setting.

## CNR — Settings Cancel and close do not always revert

`SettingsForm` previews changes by writing straight into the `Configuration` statics. Cancel reverts by calling `Configuration.ReadSettings()`, which returns at once when `skeys9.ini` doesn't exist ([Configuration.cs](../SKeys9/SKeys9/Configuration.cs)). So until the file exists, Cancel keeps the previewed changes. `SettingsForm` also has no `FormClosing` handler, so closing it with the window's X always keeps them. `ReadSettings` also only restores what the file holds, so a setting that's missing from an older `skeys9.ini` isn't reverted either. In all these cases the changes are unsaved, so the display and the file disagree until the next OK or restart.

**Evidence:** read 2026-10-21, and the older-file case read 2026-11-02.

**Plan:** take a snapshot of the settings when `SettingsForm` opens. Restore it on Cancel and on any close that isn't OK, using a flag that OK sets before it closes the window. To build the snapshot, split `ApplySettings` into a part that serializes to a dictionary and a part that saves the file. `LoadSettings` can then restore from that dictionary.

**Verify:** delete `skeys9.ini`, open the settings window, change a colour and press Cancel: the colour reverts. Do the same and close with X: it reverts. Change a colour and press OK: it is saved.

**Docs when done:** CLAUDE.md § Settings flow. Changelog entry.

## NET — Move off .NET Framework 4.6.1

The project targets .NET Framework 4.6.1, which Microsoft no longer supports ([SKeys9.csproj](../SKeys9/SKeys9/SKeys9.csproj)). It also has no manifest and no DPI setting, so Windows stretches the window on a scaled display, which should make the text blurry.

**Evidence:** read 2026-11-02. The support dates are from memory, and the blur wasn't seen, because this PC runs at 100%.

**Plan:** target 4.8, which comes with current Windows 10 and 11. Add a manifest or `App.config` entry for per-monitor DPI awareness, then check the settings window and key spacing at 125% and 150%. Also check that the exe still starts on the oldest Windows version the project wants to support.

**Verify:** the app builds and passes a full check at 100%, and text is sharp at 150% on a real display.

**Docs when done:** CLAUDE.md § What this is. Changelog entry, because users on old Windows may need to install 4.8.

## RDM — The README is two lines

The README only says `S Keys 9` and `Input Display`. The project has real users, with about 1,100 release downloads, 10 stars and 3 forks as of 2026-11-02, but nothing explains the settings, `KeyNames.json`, what the log files hold, or that the logs have no viewer.

**Plan:** add what it is with a screenshot, how to install and run it, each setting, how to rename keys, and what each file next to the exe is for.

**Verify:** someone new can install it and rename a key from the README alone.

**Docs when done:** none beyond the README.

## CLN — Code cleanups

Small items from a read-through on 2026-11-02. None change behaviour.

- `Configuration.LoadSettings` repeats the hex colour block for each colour, so make a `ParseColor` helper. It would also fix the short-string crash from BST in one place.
- Clamping is written out with `Math.Min(Math.Max(...))` more than once. Add `S.Math.Clamp`.
- `Hooks.cs` and `EventArgs.cs` have an unused `using System.Windows.Input`, which is the only reason for the `WindowsBase` reference.
- `SKeys9.csproj` has unused references and ClickOnce leftovers: `System.Deployment`, `System.Net.Http`, `System.Data`, `System.Data.DataSetExtensions`, `Microsoft.CSharp`, `System.Xml.Linq`, the publish properties and the bootstrapper packages.
- `SettingsForm` has an unused `BackColorDialog`, and `ÁbortButton` is spelled with an accented letter.
- `Properties/Settings` and `Properties/Resources` appear to be unused.
- `FillKeyGapsCheckbox.Enabled` is set in the `SettingsForm` constructor as well as in the handler, which could become a shared update like `UpdateFadeControls`.
- `HeldKeyLayout.cs` has no `#region` grouping, and the settings handlers have no `<summary>` comments, against CLAUDE.md's code style.
- `HeldKeyLayout.Measure` could keep widths per name, cleared when the font changes. It isn't urgent, because a whole key press is about 40 µs.
- The Debug build's portable PDB may lose file and line numbers in logged stack traces while the app targets 4.6.1, unless `App.config` sets `Switch.System.Diagnostics.IgnorePortablePDBsInStackTraces` to false. That's from memory, not tested, and NET would remove it.
- Newtonsoft.Json is only used to read one dictionary of names. Dropping it would make the download a single exe.

**Verify:** the app builds, and a full check still passes.

**Docs when done:** none.

## LBW — Scroll line wraps past the window's edge

The `DisplayText` label is set to 767×160 in [MainForm.Designer.cs](../SKeys9/SKeys9/MainForm.Designer.cs), while the window's client area is 353×83. It's anchored on all sides, so it stays that much larger whatever size the window is. Text in the label wraps at the label's width, so a long scroll counter line runs past the window's edge and out of sight before it wraps. Held keys aren't affected, because they wrap at the window's width.

**Evidence:** the sizes were measured on a real form 2026-11-01. The effect on the scroll line was only read. It takes several scroll directions within the clear delay to make the line that long.

**Plan:** set the label's size to the window's client area in the designer file. First deal with the startup banner, which is probably wider than the window: it would then wrap to a third line, and the label hides a line that doesn't fully fit, so the last line would disappear in a window of the default size. Shorten the banner or reword it across its two lines.

**Verify:** in a window of the default size, the startup banner shows in full. Scroll in three directions within the clear delay, using a large font if needed: the counters wrap at the window's edge.

**Docs when done:** changelog entry.
