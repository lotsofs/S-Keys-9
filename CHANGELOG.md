# Changelog

What changed for users of S Keys 9, per release. Downloads are on the [releases page](https://github.com/lotsofs/S-Keys-9/releases).

## Unreleased

### Fixed

- **Key names are back** for OEM102 (`?226`) and other keys that showed as a number. If you keep your own `KeyNames.json`, replace it with the new one.

## 0.8556.30292 (2023-06-05)

### Added

- Custom key names: edit `KeyNames.json` next to `SKeys9.exe`. Start S Keys 9 from its own folder, or it won't find the file.
- Counter Display settings: font, text colour and background colour for the counter window.

### Changed

- **Keep `Newtonsoft.Json.dll` next to `SKeys9.exe`.** S Keys 9 won't start without it.
- The download no longer includes `skeys9.ini`, so a fresh install shows keys in bold.
- The download now also includes `KeyNames.json`, `SKeys9.exe.config`, `SKeys9.pdb` and `Newtonsoft.Json.xml`.
- Key code 255 shows as `0xFF` instead of `?255`.
- The counter window opens about twice as large, in Microsoft Sans Serif 10 pt bold instead of Segoe UI 10 pt.
- Settings and the counter window no longer open twice.
- The About box now says "Made and maintained by".
- The version number changes with every build. This release is 9.0.8556.30292.
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
- The download includes a `skeys9.ini` with the starting settings.
