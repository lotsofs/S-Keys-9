# Defects and tasks

Detail for the items in [TODO.md](TODO.md).

Dependencies between plans:

- RAC decides how the mouse-move thread shares data. MTS builds on that choice, and goes away if RAC drops the thread.
- MKN, KNP and SRN all change how key names load, so do them together.

## RAC — Mouse-move thread races the UI thread on the statistics

The mouse-move thread (`Input.ProcessQueuedMouseMovements` → `MouseMoveProcessing`, [Input.cs](../SKeys9/SKeys9/Input.cs)) writes to these:

- `_inputsCount` (0x9B, 0x9C)
- `_inputsDuration` (0x9B)
- `_mousePos`

The UI thread also writes the first two on every key press, and `SaveFiles` reads all three. Nothing locks them. Counter updates can be lost.

A save can also collide with the thread. If `SaveFiles` is enumerating `_mousePos` while the thread adds a cell, it throws `InvalidOperationException`. `SaveFiles` catches that and abandons the rest of the save, with these effects:

- `mousemovemap.log` is left part-written, because opening the `StreamWriter` already truncated it.
- `mouseinteractionmap.log` isn't saved that round.
- If the abandoned save was the one on exit, the next start loads the short file. The `.log.bak` copy only holds the data as of the last start.

**Evidence:** read 2026-10-21. Not reproduced.

**Plan:** guard the movement statistics with one lock. `MouseMoveProcessing` takes it for each event. `SaveFiles` takes it while copying the data, then writes the copy outside the lock. Write each log to a temporary file and swap it in with `File.Replace`, so a failed save can't truncate the real file.

The alternative is to drop the thread and process moves on the UI thread. Each event is cheap, but the thread was added so that short hangs wouldn't stall input. Choose between these here, since MTS follows from the choice.

**Verify:** use a debug build with the autosave interval cut to about 1 s. Run it with movement logging on and move the mouse constantly for a few minutes. "Collection modified" must never appear in the debug output, and every saved `mousemovemap.log` must load back with the same number of lines.

**Docs when done:** CLAUDE.md § Threading. Changelog entry, because users with movement logging on stop losing parts of `mousemovemap.log`.

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

## WDG — Hook watchdog not yet seen on real use

`Hooks.OnHookCheck` ([Hooks.cs](../SKeys9/SKeys9/Hooks.cs)) reinstalls both hooks when `GetLastInputInfo` reports input newer than the last callback. Two things about it are still unproven:

- **False reinstalls:** every test run used injected input. Hardware input might update `GetLastInputInfo` without reaching a hook, and each such case adds to the `9D` line of `count.log`. Typing at the lock screen or a UAC prompt is expected to do this.
- **A real drop:** recovery was only tested by unhooking behind `Hooks`' back, not with Windows removing a hook.

**Evidence:** reproduced 2026-11-01. With no input from the user, injected activity caused no reinstalls, and a simulated dead hook recovered within 2.3 s for the mouse and 0.6 s for the keyboard. The real exe, suspended while 9 moves were injected, kept its hook: each move waited out the hook timeout (about 310 ms), so a real drop wasn't seen. It reinstalled once on resuming, because it had missed that input, and `count.log` recorded it.

**Plan:** use the app normally for a day with Button Presses logging on, then read the `9D` line. If it climbs during ordinary use, raise `HookCheckMargin` or rethink the check. Note any time input stops showing and then comes back.

**Verify:** the `9D` line stays near 0 in normal use, apart from time at the lock screen or UAC prompts.

**Docs when done:** none.

## TMR — Hook check interval and scroll clear delay are hardcoded

The hook check interval is 1000 ms in the `Hooks` constructor ([Hooks.cs](../SKeys9/SKeys9/Hooks.cs)). The delay before scroll counters clear is 300 ms in `Input.StartScrollRemoveTimer` ([Input.cs](../SKeys9/SKeys9/Input.cs)). Both should be settings.

**Plan:** for each value, add a static field in `Configuration`, a `case` in `LoadSettings` and a line in `ApplySettings`. Set the timer intervals from these fields. `Hooks` and `Input` are created after `ReadSettings`, so startup already works, but a change made in the settings window must also update the running timers. Open decision: offer controls in `SettingsForm`, or only `skeys9.ini` keys.

**Verify:** set both values in `skeys9.ini` and restart. Scroll counters then linger for the set time. Press OK in the settings window, and both values are still in the file.

**Docs when done:** CLAUDE.md § Architecture (1 s and 300 ms become defaults). Changelog entry for the new settings.

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
