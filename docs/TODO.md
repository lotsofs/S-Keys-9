# TODO

Open work, one row per item. Within each group, rows are in suggested order of work. Each item's detail is in the doc that its group heading names. Finished items move to [DONE.md](DONE.md).

## Defects and tasks — detail in [issues.md](issues.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [MKN](issues.md#mkn--key-names-file-replaces-the-built-in-names-instead-of-adding-to-them) | Key names | `Keys.ReadJson` replaces the built-in names with the file's instead of merging, so any key the file leaves out shows as `?` and its code; a user's own 0.8556.30292 copy leaves out OEM102 (`?226`) and 11 others (reproduced) | Low |
| [KNP](issues.md#knp--key-names-file-is-read-from-the-working-directory) | Key names | `Keys.ReadJson` uses the working directory instead of the exe folder, and a bad or unwritable file there stops startup | Medium |
| [SRN](issues.md#srn--renamed-scroll-counters-never-reset) | Key names | Scroll counters are created before `keyNames.json` loads and are cleared by hardcoded names, so a renamed scroll direction never clears (reproduced) | Low |
| [WDG](issues.md#wdg--hook-watchdog-not-yet-seen-on-real-use) | Hooks | Check the hook watchdog on real use: that hardware input doesn't cause false reinstalls (`debuglog.log` shows them), and that it recovers from a drop made by Windows itself rather than a simulated one | Low |
| [TMR](issues.md#tmr--hook-check-interval-and-scroll-clear-delay-are-hardcoded) | Settings | Make the 1 s hook check interval and the 300 ms scroll-counter clear delay into settings | Low |
| [CNR](issues.md#cnr--settings-cancel-and-close-do-not-always-revert) | Settings | Cancel doesn't revert previewed settings while `skeys9.ini` doesn't exist, and closing the settings window with X never reverts them | Low |
