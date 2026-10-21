# Defects and tasks

Detail for the items in [TODO.md](TODO.md).

Dependencies between plans:

- YAX, NEG and DWL all change how heatmap cells are computed. Doing them together means the saved format changes only once.
- RAC decides how the mouse-move thread shares data. MTS builds on that choice, and goes away if RAC drops the thread.
- MKN, KNP and SRN all change how key names load, so do them together.
- TMR builds on HKT, which may change what the hook timeout measures.

## YAX — Movement heatmap records x on both axes

`Input.MouseMoveProcessing` ([Input.cs](../SKeys9/SKeys9/Input.cs)) packs the cell key as `S.Bits.CombineInt(e.X / 100, e.X / 100)`. Every entry in `mousemove2map.log` therefore has the x cell in both halves, and the y position is never saved. Entries that are already saved can't be repaired.

**Evidence:** reproduced 2026-10-21. The built exe was called by reflection: `MouseMoveProcessing` with a move to (250, 730) stored the key `00020002`. The correct key is `00020007`.

**Plan:** pass the y cell as the second argument (or the previous position's cell, see DWL). Then decide what happens to existing `mousemove2map.log` files. One option is to keep adding to them, so the old entries stay on the x = y diagonal. The other is to save to a new file name so old and new data don't mix. That choice is the user's.

**Verify:** repeat the reflection check and get `00020007`. Then run the app with movement logging on, move the mouse up and down, and check that the log gains keys whose two halves differ.

**Docs when done:** CLAUDE.md § Persistence, if the file name changes. Users will notice the change, so it needs a changelog entry.

## NEG — Heatmap keys break for negative screen coordinates

`S.Bits.CombineInt(a, b)` ([Bits.cs](../SKeys9/SKeys9/FrameworkS/Bits.cs)) returns `a << 16 | b` without masking `b`. A negative `b` has all of its high bits set, so it overwrites `a`. Screens to the left of or above the primary monitor have negative coordinates:

- **Click map:** every cell with a negative y and the same y cell packs to the same key, whatever its x.
- **Movement map:** once YAX is fixed, the same happens there.

Integer division also truncates toward zero, so the two cells on either side of 0 merge into one double-width cell: −9 to 9 px for clicks, −99 to 99 px for movement.

**Evidence:** reproduced 2026-10-21 by calling `CombineInt` in the built exe by reflection. `CombineInt(3, -1)` and `CombineInt(7, -1)` both returned `FFFFFFFF`. `CombineInt(-2, 5)` returned `FFFE0005`, so a negative x survives.

**Plan:** mask the low half (`a << 16 | (b & 0xFFFF)`) and floor-divide the coordinates instead of truncating. Each half then reads back as a signed 16-bit cell. Offsetting by the virtual-screen origin would avoid negative cells, but that origin moves when monitors are rearranged, so masking is the better fix. Anything that reads the logs must treat each half as signed.

**Verify:** by reflection, `CombineInt(3, -1)` returns `0003FFFF` and `CombineInt(7, -1)` returns `0007FFFF`. Then click on a monitor above the primary and check that the click log gains distinct keys.

**Docs when done:** CLAUDE.md § Persistence (say that the halves are signed). Changelog entry, with YAX.

## DWL — Mouse rest time is credited to the wrong cell

`Input.MouseMoveProcessing` ([Input.cs](../SKeys9/SKeys9/Input.cs)) adds the stopwatch time since the previous move to the cell of the new position. That time is how long the mouse rested at the previous position, so it belongs to the previous cell. The time is also measured when the background thread takes the event off the queue, not when the event happened. Events processed in a burst get near-zero times, and the 50 ms polling sleep skews the rest.

**Evidence:** read 2026-10-21. A code comment on that line noted the first part.

**Plan:** credit the time to the cell of `_oldMouseX` and `_oldMouseY`. Carry the hook's event timestamp (`MSLLHOOKSTRUCT.time`, in ms) in `MoveEventArgs`, and use the difference between successive timestamps instead of the stopwatch. The timestamp wraps after about 49.7 days, so subtract it as unchecked `uint`. Do this together with YAX and NEG.

**Verify:** by reflection, feed two moves with known timestamps and check that the gap between them lands in the first position's cell.

**Docs when done:** none in CLAUDE.md. Changelog entry, with YAX.

## RAC — Mouse-move thread races the UI thread on the statistics

The mouse-move thread (`Input.ProcessQueuedMouseMovements` → `MouseMoveProcessing`, [Input.cs](../SKeys9/SKeys9/Input.cs)) writes to these:

- `_inputsCount` (0x9B, 0x9C)
- `_inputsDuration` (0x9B)
- `_mousePos`

The UI thread also writes the first two on every key press, and `SaveFiles` reads all three. Nothing locks them. Counter updates can be lost.

A save can also collide with the thread. If `SaveFiles` is enumerating `_mousePos` while the thread adds a cell, it throws `InvalidOperationException`. `SaveFiles` catches that and abandons the rest of the save, with these effects:

- `mousemove2map.log` is left part-written, because opening the `StreamWriter` already truncated it.
- `mouseinteraction2map.log` isn't saved that round.
- If the abandoned save was the one on exit, the next start loads the short file. The `.log.bak` copy only holds the data as of the last start.

**Evidence:** read 2026-10-21. Not reproduced.

**Plan:** guard the movement statistics with one lock. `MouseMoveProcessing` takes it for each event. `SaveFiles` takes it while copying the data, then writes the copy outside the lock. Write each log to a temporary file and swap it in with `File.Replace`, so a failed save can't truncate the real file.

The alternative is to drop the thread and process moves on the UI thread. Each event is cheap, but the thread was added so that short hangs wouldn't stall input. Choose between these here, since MTS follows from the choice.

**Verify:** use a debug build with the autosave interval cut to about 1 s. Run it with movement logging on and move the mouse constantly for a few minutes. "Collection modified" must never appear in the debug output, and every saved `mousemove2map.log` must load back with the same number of lines.

**Docs when done:** CLAUDE.md § Threading. Changelog entry, because users with movement logging on stop losing parts of `mousemove2map.log`.

## MTS — Mouse-move thread polls forever and cannot be stopped

`Input.ProcessQueuedMouseMovements` ([Input.cs](../SKeys9/SKeys9/Input.cs)) loops for the app's whole life. Whenever the queue is empty it sleeps 50 ms and checks again, so it wakes 20 times a second even with movement logging off. It is a background thread, so exit kills it mid-queue, and moves still queued at that point miss the exit save.

**Evidence:** read 2026-10-21.

**Plan:** replace the `ConcurrentQueue` and `Thread.Sleep` with a `BlockingCollection<MoveEventArgs>` consumed through `GetConsumingEnumerable`, which blocks without polling. In `MainForm.PrepareProgramForExit`, call `CompleteAdding` and join the thread with a short timeout before `SaveFiles`. Optionally, start the thread only while movement logging is on. If RAC drops the thread, log this item as resolved by RAC instead.

**Verify:** with movement logging off, the thread's context-switch count, as shown in Process Explorer, stays flat. Move the mouse just before exiting, and those moves appear in the saved log.

**Docs when done:** CLAUDE.md § Threading. Changelog entry, because the last moves before exit now reach the log.

## MKN — Key names file replaces the built-in names instead of adding to them

`Keys.ReadJson` ([Keys.cs](../SKeys9/SKeys9/Keys.cs)) replaces the whole built-in table with the contents of `keyNames.json`. So any code the file leaves out shows as `?` followed by its decimal code. This affects users who keep their own copy of the `KeyNames.json` shipped in 0.8556.30292, for example because they've customised it. That file leaves out 12 built-in names, so OEM102 (0xE2), for one, shows as `?226`. `KeyNames.json` also names 0xFF as `0xFF`, which the built-in table leaves unnamed.

**Evidence:** reproduced 2026-10-21 by loading the released exe and its own `KeyNames.json` from the release zip. After `ReadJson` ran, 0xE2, 0x15, 0x1C, 0x6C and 0xE5 had no name.

**Plan:** make `ReadJson` lay the file's entries over the built-in table instead of replacing it, so a missing entry keeps its built-in name. Open decision: whether 0xFF keeps the name `0xFF`. Do this together with KNP and SRN.

**Verify:** load the 0.8556.30292 `KeyNames.json` from the release zip with the built exe by reflection, and check that 0xE2 is `OEM102`. Then delete an entry from a copy of the file, start the app and press that key: it shows its built-in name.

**Docs when done:** CLAUDE.md § Key codes (say that the file overrides the built-in names rather than replacing them). Changelog entry, because the names come back even in users' own copies.

## KNP — Key names file is read from the working directory

`Keys.ReadJson` ([Keys.cs](../SKeys9/SKeys9/Keys.cs)) builds its path from `Environment.CurrentDirectory`. Every other file uses the exe's folder. When the app is started from another working directory, it ignores the user's `keyNames.json` and writes a fresh defaults file into that directory instead.

Neither `File.WriteAllText` nor `JsonConvert.DeserializeObject` has any error handling, and both run inside the `MainForm` constructor. An unwritable working directory or a malformed file would therefore stop the app from starting. An example of an unwritable directory is `C:\Windows\System32`, which a Task Scheduler task uses when it has no start folder.

**Evidence:** working-directory use reproduced 2026-10-21. With the exe in `bin/Debug` and the working directory set to a scratch folder, `ReadJson` created `keyNames.json` in the scratch folder. The startup failure is read only.

**Plan:** build the path from `Configuration.DirectoryPath`. `SetDirectories` already runs before `ReadJson`. If the file can't be written or parsed, fall back to the built-in names and tell the user once, naming the file.

**Verify:** start the exe from another working directory. The `keyNames.json` next to the exe is used, and no file appears in the working directory. Then break the file's JSON, start the app, and check that it falls back to the built-in names and shows the message.

**Docs when done:** CLAUDE.md § Key codes (drop the working-directory sentence). Changelog entry, because users who placed the file elsewhere must move it.

## SRN — Renamed scroll counters never reset

`Input` creates `_currentScrollCount` from `Keys.KeyNames[0x97..0x9A]` when it is constructed ([Input.cs](../SKeys9/SKeys9/Input.cs)). `MainForm` constructs it before `Keys.ReadJson` runs ([MainForm.cs](../SKeys9/SKeys9/MainForm.cs)), so the counters carry the built-in names. `AddScroll` looks up the name again after the JSON has loaded, so a renamed direction gets an extra counter. `ResetScrolls` then clears only the four hardcoded built-in names. The renamed counter is never cleared, and its count keeps growing on the display.

**Evidence:** reproduced 2026-10-21 by calling the built exe by reflection. After `ScrUp` was renamed to `WheelUp` in `keyNames.json`, one scroll up and a reset left `WheelUp = 1`, and the four built-in names at 0.

**Plan:** call `Keys.ReadJson` before `AddHooks`. Initialise the counters with the same name-or-`?code` lookup that `AddScroll` uses, so a JSON file without these codes doesn't throw. Make `ResetScrolls` clear every entry instead of fixed names.

**Verify:** repeat the reflection check and get `WheelUp = 0` after the reset. Then, in the app, rename a scroll direction, scroll, and see the counter disappear after the clear delay.

**Docs when done:** CLAUDE.md § Architecture (the startup order). Changelog entry.

## HKT — Hook watchdog only watches the keyboard

`Hooks.KbHookCallback` restarts the 5 s `_hookTimeoutTimer`, but `MHookCallback` does not ([Hooks.cs](../SKeys9/SKeys9/Hooks.cs)). When the timer fires, `OnHookTimeout` unhooks and reinstalls both hooks. This has two effects:

- **Mouse-only use:** both hooks are reinstalled every 5 s.
- **While typing:** a mouse hook that Windows has removed is never noticed.

The pseudo-key 0x9D `DbgHookTimeout` has a name, but nothing counts it, so reinstalls don't show anywhere.

**Evidence:** read 2026-10-21.

**Plan:** record the time of the last callback for each hook (`Environment.TickCount`). Tick the timer every second, and reinstall only a hook whose last callback is older than the timeout. Raise an event on each reinstall so that `Input` can count it under 0x9D.

**Verify:** use only the mouse for 30 s, and the mouse hook is not reinstalled (today both are, about six times). Type for 30 s with the mouse still, and only the mouse hook is reinstalled.

**Docs when done:** CLAUDE.md § Architecture (the watchdog sentence in the `Hooks` bullet). Changelog entry: mouse input now recovers if Windows drops the hook while you type, and `count.log` gains a hook-reinstall count.

## TMR — Hook timeout and scroll clear delay are hardcoded

The hook timeout is 5000 ms in the `Hooks` constructor ([Hooks.cs](../SKeys9/SKeys9/Hooks.cs)). The delay before scroll counters clear is 300 ms in `Input.StartScrollRemoveTimer` ([Input.cs](../SKeys9/SKeys9/Input.cs)). Both should be settings.

**Plan:** for each value, add a static field in `Configuration`, a `case` in `LoadSettings` and a line in `ApplySettings`. Set the timer intervals from these fields. `Hooks` and `Input` are created after `ReadSettings`, so startup already works, but a change made in the settings window must also update the running timers. Open decision: offer controls in `SettingsForm`, or only `skeys9.ini` keys. Do this after HKT.

**Verify:** set both values in `skeys9.ini` and restart. Scroll counters then linger for the set time. Press OK in the settings window, and both values are still in the file.

**Docs when done:** CLAUDE.md § Architecture (5 s and 300 ms become defaults). Changelog entry for the new settings.

## CNR — Settings Cancel and close do not always revert

`SettingsForm` previews changes by writing straight into the `Configuration` statics. Cancel reverts by calling `Configuration.ReadSettings()`, which returns at once when `skeys9.ini` doesn't exist ([Configuration.cs](../SKeys9/SKeys9/Configuration.cs)). So until the file exists, Cancel keeps the previewed changes. `SettingsForm` also has no `FormClosing` handler, so closing it with the window's X always keeps them. In both cases the changes are unsaved, so the display and the file disagree until the next OK or restart.

**Evidence:** read 2026-10-21.

**Plan:** take a snapshot of the settings when `SettingsForm` opens. Restore it on Cancel and on any close that isn't OK, using a flag that OK sets before it closes the window. To build the snapshot, split `ApplySettings` into a part that serializes to a dictionary and a part that saves the file. `LoadSettings` can then restore from that dictionary.

**Verify:** delete `skeys9.ini`, open the settings window, change a colour and press Cancel: the colour reverts. Do the same and close with X: it reverts. Change a colour and press OK: it is saved.

**Docs when done:** CLAUDE.md § Settings flow. Changelog entry.

## HHD — Mouse hook passes the keyboard hook handle

When `nCode < 0`, `Hooks.MHookCallback` returns `CallNextHookEx(_kbHookID, …)` ([Hooks.cs](../SKeys9/SKeys9/Hooks.cs)). It should pass `_mHookID`. Windows ignores this parameter, so nothing visible goes wrong.

**Evidence:** read 2026-10-21.

**Plan:** pass `_mHookID`.

**Verify:** the solution builds, and mouse input still shows.

**Docs when done:** none. Windows ignores the handle, so users can't see any difference.
