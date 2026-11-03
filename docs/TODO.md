# TODO

Open work, one row per item. Within each group, rows are in suggested order of work. Each item's detail is in the doc that its group heading names. Finished items move to [DONE.md](DONE.md).

## Defects and tasks — detail in [issues.md](issues.md)

| Code | Area | Summary | Severity |
|---|---|---|---|
| [STK](issues.md#stk--a-lost-key-up-leaves-a-key-stuck) | Input | When a key-up never arrives, such as after an admin window takes focus or the PC locks, the key stays on the display, its next press is ignored, and `duration.log` gets inflated | Medium |
| [BST](issues.md#bst--one-bad-value-stops-the-app-from-starting-silently) | Startup | One bad value in `skeys9.ini` or one damaged line in a log makes the app vanish at startup with no window or message | Medium |
| [WRT](issues.md#wrt--a-file-that-cant-be-written-stops-the-app-or-raises-errors) | Startup | A file that can't be written stops the app from starting (`skeys9.ini.bak`, a `.log.bak`) or makes the autosave fail with an unhandled error every minute | Medium |
| [WDT](issues.md#wdt--keys-are-placed-with-the-wrong-width-when-hidden-or-resized) | Display | Keys pressed while in the tray each take their own row, and keys held through a resize go out of sight, until they're released | Medium |
| [RDR](issues.md#rdr--the-display-doesnt-refresh-properly-after-settings-or-disable-display) | Display | Any settings change throws away key positions and greyed keys, so the fade can't be previewed on a real key, and turning the display back on doesn't show held keys until the next input | Low |
| [SOR](issues.md#sor--scroll-counters-dont-show-in-the-order-they-started) | Display | Scroll counters always show as up, down, left, right, instead of in the order you started scrolling them | Low |
| [ORD](issues.md#ord--pressing-a-greyed-key-again-breaks-press-order) | Display | A greyed key pressed again keeps its old spot, even with the settings that promise press order | Low |
| [HLD](issues.md#hld--counts-and-timed-clears-still-wait-during-a-title-bar-hold) | Display | While a title bar of S Keys' own windows is held, the counter window's numbers, released keys going and scroll counters clearing still wait up to 0.5 s, or until release on the minimise button | Low |
| [SGO](issues.md#sgo--key-display-settings-are-below-counter-display) | Settings | Move the Key Display group above Counter Display in the settings window | Low |
| [CNT](issues.md#cnt--a-closed-counter-window-stays-attached) | Counter | Closing the counter window leaves it attached to the input event, so each one stays in memory and keeps counting | Low |
| [STL](issues.md#stl--slow-work-on-the-ui-thread-stalls-all-input-on-the-pc) | Performance | Hooks share the UI thread, so anything slow there freezes all input on the PC: up to 31 ms for each autosave with logging on, and 243 ms for the font dialog | Medium |
| [TMS](issues.md#tms--restarting-the-key-timer-is-the-costliest-part-of-a-key-press) | Performance | With greying on, restarting the key removal timer takes a key event from about 40 µs to about 210 µs inside the hook | Low |
| [LIN](issues.md#lin--key-rows-and-the-scroll-line-use-different-line-heights) | Display | Key rows and the scroll line use different line heights, so wrapped rows drift from the scroll line or overlap each other, and `&` still misbehaves in scroll names | Low |
| [LBW](issues.md#lbw--scroll-line-wraps-past-the-windows-edge) | Display | The main window's label is 414 px wider and 77 px taller than the window, so the scroll counter line wraps out of sight instead of at the window's edge | Low |
| [DUP](issues.md#dup--keys-with-the-same-name-swap-slots) | Display | When two held keys share a name, the wrong slot greys out, because keys are matched by name instead of by key code | Low |
| [WHL](issues.md#whl--wheel-events-are-counted-instead-of-wheel-notches) | Input | One wheel event counts as one scroll whatever its size, so fast mice under-count and smooth-scrolling mice would over-count | Low |
| [CNR](issues.md#cnr--settings-cancel-and-close-do-not-always-revert) | Settings | Cancel doesn't revert previewed settings that `skeys9.ini` doesn't hold, and closing the settings window with X never reverts them | Low |
| [TMR](issues.md#tmr--hook-check-interval-is-hardcoded) | Settings | Make the 1 s hook check interval a setting | Low |
| [NET](issues.md#net--move-off-net-framework-461) | Build | Move off .NET Framework 4.6.1, which is out of support, and declare DPI awareness so the window isn't blurry on scaled displays | Medium |
| [RDM](issues.md#rdm--the-readme-is-two-lines) | Docs | The README is two lines, so the settings, `KeyNames.json` and the log files are undocumented for users | Medium |
| [CLN](issues.md#cln--code-cleanups) | Code | Small cleanups: repeated colour parsing and clamping, unused references and leftovers, and style gaps in the new code | Low |

## Feature ideas — detail in [ideas.md](ideas.md)

None of these are agreed yet. The order is best value for the effort first.

| Code | Area | Summary | Value |
|---|---|---|---|
| [POS](ideas.md#pos--remember-the-windows-position-and-size) | Window | Remember the main window's position and size between runs | High |
| [PAU](ideas.md#pau--a-pause-hotkey-for-privacy) | Privacy | A global hotkey to pause the display, so a password typed on stream doesn't show | High |
| [OVL](ideas.md#ovl--overlay-mode) | Window | Overlay mode: always on top, no title bar, transparent background, click-through | High |
| [FAD](ideas.md#fad--a-smooth-fade) | Display | Fade released keys out smoothly over the Clear Delay, instead of one flat colour | Medium |
| [FIX](ideas.md#fix--fixed-key-slots) | Display | Fixed slots: chosen keys such as W A S D always show in the same place, even when not held | High |
| [HIS](ideas.md#his--key-history) | Display | A running history of recent keys, for tutorials and showing fast taps | Medium |
| [FLT](ideas.md#flt--hide-chosen-keys) | Display | Hide chosen keys, or show only modifiers or only chosen keys | Medium |
| [VEW](ideas.md#vew--a-statistics-viewer) | Statistics | A window that shows the logs: top keys, totals, mouse distance and heatmap images | High |
| [LAY](ideas.md#lay--key-names-from-the-users-keyboard-layout) | Key names | Take key names from the user's keyboard layout, instead of US names | Medium |
| [KPS](ideas.md#kps--live-speed-and-hold-times) | Display | Live keys per second, and a hold timer on each key | Medium |
| [ALN](ideas.md#aln--alignment-and-growth-direction) | Display | Right or centre alignment, and keys growing upwards | Low |
| [KCP](ideas.md#kcp--key-cap-look) | Display | Draw keys as key caps, with colours per group such as modifiers and mouse buttons | Medium |
| [INJ](ideas.md#inj--mark-injected-input) | Input | Mark input sent by macros and scripts, in colour or by leaving it out of the statistics | Medium |
| [ONE](ideas.md#one--single-instance) | Startup | A second launch brings up the running copy, instead of starting another | Low |
| [AUT](ideas.md#aut--start-with-windows) | Startup | Start with Windows, and optionally start in the tray | Low |
| [REC](ideas.md#rec--timestamped-input-recording) | Statistics | Record every press and release with a timestamp to a file, for speedrun and game analysis | Medium |
| [GAM](ideas.md#gam--gamepad-input) | Input | Show gamepad buttons and sticks alongside keys | Medium |
