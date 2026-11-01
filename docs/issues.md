# Defects and tasks

Detail for the items in [TODO.md](TODO.md).

Dependencies between plans:

- KNP and SRN both change how key names load, so do them together.

## KNP — Key names file is read from the working directory

`Keys.ReadJson` ([Keys.cs](../SKeys9/SKeys9/Keys.cs)) builds its path from `Environment.CurrentDirectory`. Every other file uses the exe's folder. When the app is started from another working directory, it ignores the user's `keyNames.json` and writes a fresh defaults file into that directory instead.

Neither `File.WriteAllText` nor `JsonConvert.DeserializeObject` has any error handling, and both run inside the `MainForm` constructor. An unwritable working directory or a malformed file would therefore stop the app from starting. An example of an unwritable directory is `C:\Windows\System32`, which a Task Scheduler task uses when it has no start folder. A name set to `null` in the file gets past loading, and would then throw on a key press or scroll (read only).

**Evidence:** working-directory use reproduced 2026-10-21. With the exe in `bin/Debug` and the working directory set to a scratch folder, `ReadJson` created `keyNames.json` in the scratch folder. The startup failure is read only.

**Plan:** build the path from `Configuration.DirectoryPath`. `SetDirectories` already runs before `ReadJson`. If the file can't be written or parsed, fall back to the built-in names and tell the user once, naming the file.

**Verify:** start the exe from another working directory. The `keyNames.json` next to the exe is used, and no file appears in the working directory. Then break the file's JSON, start the app, and check that it falls back to the built-in names and shows the message.

**Docs when done:** CLAUDE.md § Key codes (drop the working-directory sentence). Changelog entry, because users who placed the file elsewhere must move it.

## SRN — Renamed scroll counters never reset

`Input` creates `_currentScrollCount` from `Keys.KeyNames[0x97..0x9A]` when it is constructed ([Input.cs](../SKeys9/SKeys9/Input.cs)). `MainForm` constructs it before `Keys.ReadJson` runs ([MainForm.cs](../SKeys9/SKeys9/MainForm.cs)), so the counters carry the built-in names. `AddScroll` looks up the name again after the JSON has loaded, so a renamed direction gets an extra counter. `ResetScrolls` then clears only the four hardcoded built-in names. The renamed counter is never cleared, and its count keeps growing on the display.

**Evidence:** reproduced 2026-10-21 by calling the built exe by reflection. After `ScrUp` was renamed to `WheelUp` in `keyNames.json`, one scroll up and a reset left `WheelUp = 1`, and the four built-in names at 0.

**Plan:** call `Keys.ReadJson` before `AddHooks`, so the counters are created with the file's names. Make `ResetScrolls` clear every entry instead of fixed names.

**Verify:** repeat the reflection check and get `WheelUp = 0` after the reset. Then, in the app, rename a scroll direction, scroll, and see the counter disappear after the clear delay.

**Docs when done:** CLAUDE.md § Architecture (the startup order). Changelog entry.

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
