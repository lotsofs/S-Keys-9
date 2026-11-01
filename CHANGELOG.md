# Changelog

What changed for users of S Keys 9, per release. Downloads are on the [releases page](https://github.com/lotsofs/S-Keys-9/releases).

## Unreleased

### Fixed

- **Key names are back** for OEM102 (`?226`) and other keys that showed as a number. If you keep your own `KeyNames.json`, replace it with the new one.
- Mouse movement logging now saves the vertical position as well as the horizontal one. The log is now `mousemovemap.log`, which starts empty. `mousemove2map.log` is no longer used, so you can keep or delete it.
- Mouse click and movement logging now saves the right position on monitors above or to the left of your main monitor. The click log is now `mouseinteractionmap.log`, which starts empty. `mouseinteraction2map.log` is no longer used, so you can keep or delete it.

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
