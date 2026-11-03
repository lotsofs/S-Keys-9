# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

S Keys 9 is an input display for Windows: a WinForms app (C#, .NET Framework 4.6.1) that hooks global keyboard and mouse input and shows the currently held keys in a window. It can also log lifetime statistics to files: press counts, hold durations, and mouse movement and click heatmaps.

## Open work

Start at [docs/TODO.md](docs/TODO.md). It indexes open defects and tasks by three-letter code. Each item's evidence, plan and checks are in the doc that its group heading names. Finished items move to [docs/DONE.md](docs/DONE.md). Track new work there, not in `// TODO` comments. Changes that users will notice go under Unreleased in [CHANGELOG.md](CHANGELOG.md), written for users and without item codes.

## Build and run

The project is an old-style (non-SDK) `.csproj` that uses `packages.config`. Build it with Visual Studio's MSBuild. `dotnet build` fails on this project because its `GenerateResource` task can't build the .NET Framework resources. MSBuild isn't on PATH, so find it with vswhere (Git Bash):

```sh
MSB="$("/c/Program Files (x86)/Microsoft Visual Studio/Installer/vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | head -1)"
"$MSB" SKeys9/SKeys9.sln -t:restore -p:RestorePackagesConfig=true   # NuGet restore into SKeys9/packages/ (gitignored)
"$MSB" SKeys9/SKeys9.sln -p:Configuration=Debug                     # or Release
```

The build writes `SKeys9/SKeys9/bin/<Config>/SKeys9.exe`. There are no tests and no linter.

In VS Code, F5 restores, builds Debug and runs the exe under the C# extension's `clr` debugger, which can only debug 64-bit processes. That's why the Debug build sets `Prefer32Bit` to false and writes a portable PDB. Release doesn't set it, so it stays 32-bit, which is the default for an AnyCPU exe. Ctrl+F5 does the same without the debugger. While the debugger is stopped at a breakpoint, the hook callbacks can't run, so all input on the PC stalls.

- Running the exe installs system-wide low-level input hooks. The app writes its settings and logs next to the exe (see Persistence), so a debug run leaves files in `bin/Debug/`.
- New source files must be added to `SKeys9.csproj` by hand (`<Compile Include>`), because the old-style project does not glob.
- `AssemblyVersion("9.0.*")` in `Properties/AssemblyInfo.cs` auto-increments the build and revision numbers on every build, and the About box shows the result. This only compiles with deterministic builds off, which is this project's default.

## Architecture

All source is in `SKeys9/SKeys9/`. Input flows one way: `Hooks` → `Input` → `MainForm` / `CounterForm`.

- **`Hooks`** P/Invokes `SetWindowsHookEx` (`WH_KEYBOARD_LL`, `WH_MOUSE_LL`) and turns raw messages into events that carry integer key codes (`EventArgs.cs`). It drops key auto-repeat by tracking what is already down. The callbacks run on the UI thread's message loop and must stay fast, because all input on the PC waits for them, for up to about 310 ms each. Microsoft also says Windows silently removes a hook that's too slow, but on Windows 10 22H2 neither 30 timeouts with the app frozen nor 40 with a callback overrunning did that. Windows can't be asked whether a hook is still installed, so a 1 s timer compares `GetLastInputInfo` with the last callback, and reinstalls both hooks if input arrived that neither saw.

  Windows keeps input from hooks on the lock screen and while an elevated window has focus, so the timer doesn't reinstall then. Instead it logs one line naming the cause, each time input is missed that way. A reinstall line can still be harmless: another program's slow hook seems to cause the odd one, a key that another program's hook blocks before S Keys' hook sees it causes one (see BLK), and a cancelled shutdown should, because the app unhooks as soon as Windows asks to shut down.

  The same timer lets go of held keys and buttons whose release the hooks missed, and logs each one. `GetAsyncKeyState` can't be trusted while input is out of reach: it reported a held Shift as up while an admin window had focus, and kept reporting Win as down on the lock screen after its release (both seen 2026-11-03). So the first check out of reach releases everything held, even a key that's really still down, which its next auto-repeat brings back as a new press. Back in reach, a held key that `GetAsyncKeyState` has up is released only if Windows has shown it down, it was pressed while out of reach, or the hooks were reinstalled while it was held. That's because Windows never shows a key down that another program's hook blocks after S Keys' hook has seen it, as remappers do (reproduced 2026-11-03), and releasing it would make it flicker and count again at every check. Such a release's hold time ends at the last check that found the key down, or at its press. Windows reports the physical mouse buttons, which a left-handed setup swaps, so LMB and RMB both count as down while either button is.

  A press on a title bar or title bar button of S Keys' own windows holds the UI thread in a Windows wait, for about 500 ms or until the mouse moves, and until release on the minimise button. Callbacks still run during that wait, but the message loop doesn't. So `MainForm.UpdateText` repaints inside the callback with `Update()`, which adds about 0.4 ms to a key or wheel event, or about 1.5 ms with 20 keys held. Moving the hooks to their own thread would have to replace this (see STL).
- **`Input`** holds all state. It maps codes to display names and keeps the held-keys list and the scroll counters for each direction, which clear `ClearDelay` milliseconds after the last scroll (300 by default). It also updates the statistics and raises `OnKeysChanged` for the displays. The `ChangeEventArgs` it sends holds references to the live list and counters, not copies.
- **`MainForm`** owns everything else. It wires the hooks to `Input`, renders the held keys, and handles the tray, exit and context-menu behaviour. Startup order matters: `Configuration.SetDirectories` (in `Program.Main`) → `ReadSettings` → `Keys.ReadJson` → `AddHooks` (which constructs `Input`, which loads the logs and names its scroll counters, and then `Hooks`). `Program.Main` catches a failure while the form is constructed, logs it and shows it in a message box, because there's no window yet. That message box runs a message loop, so `Hooks` and its check timer come after the logs load, and the form subscribes to `Application.ApplicationExit` last, which leaves nothing running behind a failed start.
- **`HeldKeyLayout`** decides where each held key shows, so that a key can stay put until it's released. The positions are display state, so `Input` keeps its plain list in press order. `MainForm` draws the keys at those spots in a `Paint` handler on the `DisplayText` label. The label's own text holds only the scroll counters, on the line under the keys, and the startup and "Display disabled" messages. With `KeepKeysInPlace` off, every key is placed again on each change, which gives the same picture as joining them into one string.

  With `GreyOutReleasedKeys` on, a released key keeps its spot for `ClearDelay` milliseconds, drawn in `Configuration.ReleasedKeyColor()`, and each key goes at its own time. That colour is `FadeColor`, or with `UseFadeColor` off, `FadeOpacity` percent of the way from the background to the text colour. `Input` doesn't know about this, so the counter window and the statistics see the release at once. `MainForm`'s `_keyRemoveTimer` is set for the next key due to go and only runs while one is waiting, so an idle app doesn't wake up. Its tick goes through `UpdateText`, so a key's time running out takes the same path as an input.
- **`CounterForm`** is a press counter for the current session. It gets its counts by comparing each `ChangeEventArgs` with its own copy of the previous state, not from hook events, and is independent of the saved statistics. Only the sorting runs inside the hook callback. `Display_Paint` draws the list on the message loop in `CounterColumns` columns of equal width, one entry at a time, because text of several lines only gets an ellipsis on its last one. A list too long for the window fills it in order instead of being split evenly, so only the end of the list is left out, and `... (+N)` in the last spot says how many entries that is. `CounterWindowWidth` and `CounterWindowHeight` are the inside of the window, and `CounterSizeLocked` swaps its border for one Windows won't resize.
- **`FontPicker`** is the settings window's font dialog. Windows lists many fonts as a family and a style, such as "Zurich" with "Extended Ultra Black", while .NET knows them by one name that the dialog can't find. It would open with an empty font box and .NET would ignore the result, so `FontPicker` selects the family and style as it opens.
- **`Log`** appends timestamped lines to `debuglog.log` and never throws. `Program` sends it unhandled errors, on top of the standard error dialog for the UI thread, and a failure while starting. `Hooks` sends it each reinstall, each case of input not reaching the hooks, and each key it lets go of because the hooks missed its release, `Configuration` each setting it couldn't use, `Input` each log it couldn't update and each log with lines it couldn't read, and `Keys` each problem with `KeyNames.json`.
- **`FrameworkS/`** holds small helpers in namespace `S`: the `S.Dictionaries.IncrementValue` overloads, `S.Bits.CombineInt` and `S.Math`.

### Key codes

Everything is keyed by Windows virtual-key (VK) code, 0x00–0xFF. Mouse buttons reuse the mouse-button VK codes (`MouseEventArgs.Buttons`: LMB=1, RMB=2, MMB=4, XMB1=5, XMB2=6). Unassigned VK slots hold pseudo-inputs:

- 0x97–0x9A: scroll up, down, left and right (`ScrollEventArgs.Directions` + 0x97)
- 0x9B: mouse movement, as a distance in centipixels
- 0x9C: the overflow counter for 0x9B
- 0x9D: `DbgHookTimeout`, the number of hook reinstalls, counted while `LogButtons` is on

A new pseudo-input needs a free slot and a name in `Keys.KeyNames`, and in `KeyNames.json` to keep that sample complete.

Display names come from `Keys.KeyNames`. `Keys.ReadJson` lays the contents of `KeyNames.json` over it, so a code the file leaves out or sets to `null` keeps its built-in name. The file's keys are decimal VK codes. If the file is missing, `ReadJson` writes the defaults to it. If it can't be read or parsed, the built-in names stay, and `ReadJson` logs the problem and shows it in a dialog, on every start until the file is fixed. In `Input`, the held-keys list and the scroll counters are keyed by display name, not by code, so two scroll directions with the same name share a counter.

### Threading

Mouse-move events are queued (`ConcurrentQueue`) and processed on a background thread. The thread sleeps until a move is queued, then waits 50 ms so that moves are handled in batches instead of waking it for each one. It runs until the process ends, because `WM_QUERYENDSESSION` doesn't guarantee an exit: the shutdown can still be cancelled. So `PrepareProgramForExit` doesn't stop it, and calls `FinishQueuedMouseMovements` on the UI thread to empty the queue before the save.

That thread shares `_inputsCount`, `_inputsDuration` and `_mousePos` with the UI thread, so all access to them takes `_statsLock`: each move, which leaves the queue inside the lock, the UI thread through `AddToCount` and `AddToDuration`, and `SaveFiles` only while it copies them. Only the UI thread touches `_mouseInteractions`, so it needs no lock.

### Persistence

`Configuration` is a static class that holds every setting and file path. All files live next to the exe:

| File | Contents |
|---|---|
| `skeys9.ini` | `Key=Value` settings. Colours are 6-digit hex RGB, and alpha is forced to ff on load |
| `KeyNames.json` | Custom key names, as decimal VK code and name. Written with the defaults if missing |
| `count.log`, `duration.log` | One line per VK code 00–FF: `XX <count>` / `XX <TimeSpan>` |
| `mousemovemap.log`, `mouseinteractionmap.log` | `XXXXXXXX <hex value>`. The key is a screen cell packed by `S.Bits.CombineInt` (x in the high 16 bits, y in the low). Both halves are signed, because screens left of or above the primary have negative coordinates. Cells are 100 px for movement and 10 px for clicks, and coordinates round down to a cell (`S.Math.FloorDivide`) |
| `debuglog.log` | Timestamped lines for users to open when something goes wrong: errors, a failed start, settings that couldn't be used, logs with lines that couldn't be read, logs that couldn't be updated, problems with `KeyNames.json`, hook reinstalls, input that couldn't reach the hooks, and keys let go of because the hooks missed their release. It only appears once there's something to record, and has no setting or size limit |

- After loading the logs at startup, the app immediately saves them again and copies each one to `*.log.bak`. `ReadSettings` writes `skeys9.ini.bak`.
- A value in `skeys9.ini` that can't be read leaves its setting as it was, which at startup is the default, and gets a line in `debuglog.log`. Numbers that can be read are still clamped. After loading, `LoadSettings` tries to make each font, and if it can't, such as with a size of 0, it puts back all three of that font's settings and logs it.
- A log line that can't be read is skipped, and blank lines are skipped without a word. A log with such lines is copied as it was to `<log>.bad`, because the save after loading drops them, and its `.bak` isn't replaced at that start, because it may be the last copy that has them. Each such log gets one line in `debuglog.log`.
- Logs autosave every 60 s and on exit: on `Application.ApplicationExit`, and on `WM_QUERYENDSESSION` in `MainForm.WndProc` for Windows shutdown.
- Each log is written to `<log>.tmp` and swapped in with `File.Replace`, so a failed save leaves the real file intact. If something has the log open, the swap is skipped and the next save tries again.
- The `LogButtons`, `LogClicks` and `LogMovement` settings each turn collecting, saving and backing up their group on or off.
- `skeys9.ini` and `KeyNames.json` in the source tree are samples. Only `KeyNames.json` is copied to the build output.

### Settings flow

`SettingsForm` writes straight into the `Configuration` statics as each control changes and raises `OnSettingsChanged`, so the displays show a live preview. OK calls `Configuration.ApplySettings()`, which writes `skeys9.ini`. Cancel calls `ReadSettings()`, which reloads the file and so reverts the changes.

The counter window's size and lock are the exception, because they also change outside `SettingsForm`. `CounterForm` saves them straight away with `Configuration.SaveCounterWindow`, which writes only them, so settings still being previewed stay out of the file.

A new setting needs four things:

- a static field in `Configuration`
- a `case` in `LoadSettings` that reads the value with `ReadBool`, `ReadInt`, `ReadFloat` or `ReadColor`, which keep the old value and log the problem when the value can't be read
- a line in `ApplySettings`
- a control and its handler in `SettingsForm`, with the control in `SettingsForm.Designer.cs`

## Code style

- Tabs for indentation, and opening braces on the same line.
- Private fields are `_camelCase`.
- Methods get `/// <summary>` comments, and members are grouped with `#region`.
- `*.Designer.cs` and `*.resx` are WinForms designer output. Event-handler wiring lives in `*.Designer.cs`.
