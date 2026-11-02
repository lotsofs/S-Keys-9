# Changelog

What changed for users of S Keys 9, per release. Downloads are on the [releases page](https://github.com/lotsofs/S-Keys-9/releases).

## Unreleased

### Action required

- **If you start S Keys 9 from another folder, move your `KeyNames.json` next to `SKeys9.exe`.** S Keys 9 now always reads it from there.

### Added

- `debuglog.log` next to `SKeys9.exe` records errors, when S Keys 9 had to reconnect to Windows, and when it couldn't see your input, such as on the lock screen or while a program running as administrator has focus, with the time. It only appears once there's something to record.
- Keys you let go of stay on the display in grey for a moment before they disappear, so a quick tap is easier to see.
- Key Display settings. Turn off **Keep Keys In Place** to have keys slide left when you release one, as they used to. Turn off **New Keys Fill Gaps** to always add a new key after the last one, so keys stay in the order you pressed them. Turn off **Grey Out Released Keys** to have keys disappear as soon as you let go.
- **Clear Delay** in Key Display sets how long released keys and scroll counters stay, in milliseconds. It starts at 300, which is how long scroll counters have always stayed.
- **Fade Opacity** and **Custom Fade Color** in Key Display set the colour of released keys: a blend between your background and text colours, or a colour of your own.

### Changed

- Held keys now stay where they are until you let go of them, instead of sliding left when you release an earlier key. A new key takes the first gap wide enough for it, otherwise it goes after the last key.
- Keys that don't fit now continue on the next row at the edge of the window, instead of running out of sight first.

### Fixed

- A held key with `&` in its name in `KeyNames.json` now shows the `&`.
- **Key names are back** for OEM102 (`?226`) and other keys that showed as a number. A key left out of `KeyNames.json` now keeps its normal name, so your own copy keeps working.
- Mouse movement logging now saves the vertical position as well as the horizontal one, and counts the time the mouse rests towards the spot where it rested. The log is now `mousemovemap.log`, which starts empty. `mousemove2map.log` is no longer used, so you can keep or delete it.
- Mouse click and movement logging now saves the right position on monitors above or to the left of your main monitor. The click log is now `mouseinteractionmap.log`, which starts empty. `mouseinteraction2map.log` is no longer used, so you can keep or delete it.
- Turning on mouse movement logging while S Keys 9 is running no longer adds a false jump to the distance, or counts the time logging was off as rest in one spot.
- Mouse input now comes back if Windows cuts S Keys 9 off while you're typing. `count.log` counts how often S Keys 9 has to reconnect, on the `9D` line.
- Saving can no longer leave a log cut short.
- Mouse movement from just before closing S Keys 9 is now saved.
- S Keys 9 no longer fails to start when `KeyNames.json` is broken, or when its start folder can't be written to. A broken file gets a message naming the problem, and the normal key names are used until it's fixed.
- A scroll direction you renamed in `KeyNames.json` now clears from the display like the others, instead of counting up forever.
- Keys now show at once while you hold the mouse on the title bar or a title bar button of an S Keys 9 window, instead of half a second later or only after you let go.

## 0.8556.30292 (2023-06-05)

### Added

- Custom key names: edit `KeyNames.json` next to `SKeys9.exe`. Start S Keys 9 from its own folder, or it won't find the file.
- Counter Display settings: font, text colour and background colour for the counter window.

### Changed

- On a fresh install, keys show in bold instead of regular.
- Key code 255 shows as `0xFF` instead of `?255`.
- The counter window opens about twice as large, in Microsoft Sans Serif 10 pt bold instead of Segoe UI 10 pt.
- Settings and the counter window no longer open twice.
- The About box now says "Made and maintained by".
- The About box shows the version as 9.0.8556.30292, instead of 9.0.1.0.
- The copyright in the file's properties now reads 2018-2023.

### Fixed

- Background Color changes the background, not the text colour.
- No crash at startup after choosing a font size that isn't a multiple of 3.

## 0.1.0 (2018-12-26)

First release. Needs .NET Framework 4.6.1.

- Shows the keys, mouse buttons and scroll steps you're using. Right-click for the menu.
- Settings for font, text colour, background colour, and minimizing or closing to the tray.
- A counter window that counts presses since it opened.
- Optional logging of key presses and hold times, mouse clicks and mouse movement, to files next to `SKeys9.exe`.
