# Done

Finished work, newest first. A finished item's detail section is deleted from `docs/` when it lands here. To find it in git history, run `git log -S "## CODE " -- docs/`.

| Date | Code | Area | Change | Why / how |
|---|---|---|---|---|
| 2026-11-01 | YAX | Heatmap | Save the y position in the movement heatmap, in a new mousemovemap.log | Cells were packed as (x, x), so y was never saved and old entries can't be repaired; the map restarts in a new file. Verified by reflection and by a live run that logged several y cells per column. |
| 2026-10-21 | MKN (partial) | Key names | Restore the 12 built-in key names missing from the shipped KeyNames.json | The 0.8556.30292 file left them out, and because the file replaces the built-in names, those keys showed as `?226` and the like. Loading the file with the built exe now gives every built-in name. |
