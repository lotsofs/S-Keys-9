# Done

Finished work, newest first. A finished item's detail section is deleted from `docs/` when it lands here. To find it in git history, run `git log -S "## CODE " -- docs/`.

| Date | Code | Area | Change | Why / how |
|---|---|---|---|---|
| 2026-11-01 | TGL | Heatmap | Skip the first move after movement logging comes on, so the off period adds no rest or distance | Moves are dropped while logging is off, so the next one was measured from a stale position and time; the mouse-move thread now skips one move when the setting has changed. Reproduced by reflection first, then verified by reflection and by a live run through the real settings window. |
| 2026-11-01 | DWL | Heatmap | Credit mouse rest time to the cell it rested in, timed from the hook's timestamps | The time since the previous move went to the new cell and was timed at dequeue; it now goes to the previous cell, and a timestamp running backwards adds nothing. Verified by reflection and by a live run where each logged rest matched its measured time within 11 ms. |
| 2026-11-01 | NEG | Heatmap | Keep heatmap cells apart at negative coordinates, and rename the click log to mouseinteractionmap.log | A negative y overwrote x in the packed key, and division rounded toward zero; the low half is now masked and coordinates round down. Verified live on a monitor left of the primary; negative y by reflection only, as this machine has no monitor above it. |
| 2026-11-01 | YAX | Heatmap | Save the y position in the movement heatmap, in a new mousemovemap.log | Cells were packed as (x, x), so y was never saved and old entries can't be repaired; the map restarts in a new file. Verified by reflection and by a live run that logged several y cells per column. |
| 2026-10-21 | MKN (partial) | Key names | Restore the 12 built-in key names missing from the shipped KeyNames.json | The 0.8556.30292 file left them out, and because the file replaces the built-in names, those keys showed as `?226` and the like. Loading the file with the built exe now gives every built-in name. |
