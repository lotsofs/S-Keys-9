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

- Running the exe installs system-wide low-level input hooks. The app writes its settings and logs next to the exe (see Persistence), so a debug run leaves files in `bin/Debug/`.
- New source files must be added to `SKeys9.csproj` by hand (`<Compile Include>`), because the old-style project does not glob.
- `AssemblyVersion("9.0.*")` in `Properties/AssemblyInfo.cs` auto-increments the build and revision numbers on every build, and the About box shows the result. This only compiles with deterministic builds off, which is this project's default.

## Architecture

All source is in `SKeys9/SKeys9/`. Input flows one way: `Hooks` → `Input` → `MainForm` / `CounterForm`.

- **`Hooks`** P/Invokes `SetWindowsHookEx` (`WH_KEYBOARD_LL`, `WH_MOUSE_LL`) and turns raw messages into events that carry integer key codes (`EventArgs.cs`). It drops key auto-repeat by tracking what is already down. The callbacks run on the UI thread's message loop and must stay fast, because all input on the PC waits for them, for up to about 310 ms each. Microsoft also says Windows silently removes a hook that's too slow, but on Windows 10 22H2 neither 30 timeouts with the app frozen nor 40 with a callback overrunning did that. Windows can't be asked whether a hook is still installed, so a 1 s timer compares `GetLastInputInfo` with the last callback, and reinstalls both hooks if input arrived that neither saw.

  Windows keeps input from hooks on the lock screen and while an elevated window has focus, so the timer doesn't reinstall then. Instead it logs one line naming the cause, each time input is missed that way. A reinstall line can still be harmless: another program's slow hook seems to cause the odd one, and a cancelled shutdown should, because the app unhooks as soon as Windows asks to shut down.
- **`Input`** holds all state. It maps codes to display names and keeps the held-keys list and the scroll counters for each direction, which clear 300 ms after the last scroll. It also updates the statistics and raises `OnKeysChanged` for the displays. The `ChangeEventArgs` it sends holds references to the live list and counters, not copies.
- **`MainForm`** owns everything else. It wires the hooks to `Input`, renders the held keys, and handles the tray, exit and context-menu behaviour. Startup order matters: `Configuration.SetDirectories` (in `Program.Main`) → `ReadSettings` → `AddHooks` (which constructs `Input`, and `Input` loads the logs) → `Keys.ReadJson`.
- **`CounterForm`** is a press counter for the current session. It gets its counts by comparing each `ChangeEventArgs` with its own copy of the previous state, not from hook events, and is independent of the saved statistics.
- **`Log`** appends timestamped lines to `debuglog.log` and never throws. `Program` sends it unhandled errors, on top of the standard error dialog for the UI thread. `Hooks` sends it each reinstall and each case of input not reaching the hooks, `Input` each log it couldn't update, and `Keys` each problem with `KeyNames.json`.
- **`FrameworkS/`** holds small helpers in namespace `S`: the `S.Dictionaries.IncrementValue` overloads, `S.Bits.CombineInt` and `S.Math`.

### Key codes

Everything is keyed by Windows virtual-key (VK) code, 0x00–0xFF. Mouse buttons reuse the mouse-button VK codes (`MouseEventArgs.Buttons`: LMB=1, RMB=2, MMB=4, XMB1=5, XMB2=6). Unassigned VK slots hold pseudo-inputs:

- 0x97–0x9A: scroll up, down, left and right (`ScrollEventArgs.Directions` + 0x97)
- 0x9B: mouse movement, as a distance in centipixels
- 0x9C: the overflow counter for 0x9B
- 0x9D: `DbgHookTimeout`, the number of hook reinstalls, counted while `LogButtons` is on

A new pseudo-input needs a free slot and a name in `Keys.KeyNames`, and in `KeyNames.json` to keep that sample complete.

Display names come from `Keys.KeyNames`. `Keys.ReadJson` lays the contents of `KeyNames.json` over it, so a code the file leaves out or sets to `null` keeps its built-in name. The file's keys are decimal VK codes. If the file is missing, `ReadJson` writes the defaults to it. If it can't be read or parsed, the built-in names stay, and `ReadJson` logs the problem and shows it in a dialog, on every start until the file is fixed. In `Input`, the held-keys list and the scroll counters are keyed by display name, not by code.

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
| `debuglog.log` | Timestamped lines for users to open when something goes wrong: errors, logs that couldn't be updated, problems with `KeyNames.json`, hook reinstalls, and input that couldn't reach the hooks. It only appears once there's something to record, and has no setting or size limit |

- After loading the logs at startup, the app immediately saves them again and copies each one to `*.log.bak`. `ReadSettings` writes `skeys9.ini.bak`.
- Logs autosave every 60 s and on exit: on `Application.ApplicationExit`, and on `WM_QUERYENDSESSION` in `MainForm.WndProc` for Windows shutdown.
- Each log is written to `<log>.tmp` and swapped in with `File.Replace`, so a failed save leaves the real file intact. If something has the log open, the swap is skipped and the next save tries again.
- The `LogButtons`, `LogClicks` and `LogMovement` settings each turn collecting, saving and backing up their group on or off.
- `skeys9.ini` and `KeyNames.json` in the source tree are samples. Only `KeyNames.json` is copied to the build output.

### Settings flow

`SettingsForm` writes straight into the `Configuration` statics as each control changes and raises `OnSettingsChanged`, so the displays show a live preview. OK calls `Configuration.ApplySettings()`, which writes `skeys9.ini`. Cancel calls `ReadSettings()`, which reloads the file and so reverts the changes.

A new setting needs four things:

- a static field in `Configuration`
- a `case` in `LoadSettings`
- a line in `ApplySettings`
- a control and its handler in `SettingsForm`, with the control in `SettingsForm.Designer.cs`

## Code style

- Tabs for indentation, and opening braces on the same line.
- Private fields are `_camelCase`.
- Methods get `/// <summary>` comments, and members are grouped with `#region`.
- `*.Designer.cs` and `*.resx` are WinForms designer output. Event-handler wiring lives in `*.Designer.cs`.
