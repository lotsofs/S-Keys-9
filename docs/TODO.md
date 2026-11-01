# TODO

Open work, one row per item. Within each group, rows are in suggested order of work. Each item's detail is in the doc that its group heading names. Finished items move to [DONE.md](DONE.md).

## Defects and tasks — detail in [issues.md](issues.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [SRN](issues.md#srn--renamed-scroll-counters-never-reset) | Key names | Scroll counters are created before `keyNames.json` loads and are cleared by hardcoded names, so a renamed scroll direction never clears (reproduced) | Low |
| [TMR](issues.md#tmr--hook-check-interval-and-scroll-clear-delay-are-hardcoded) | Settings | Make the 1 s hook check interval and the 300 ms scroll-counter clear delay into settings | Low |
| [CNR](issues.md#cnr--settings-cancel-and-close-do-not-always-revert) | Settings | Cancel doesn't revert previewed settings while `skeys9.ini` doesn't exist, and closing the settings window with X never reverts them | Low |
