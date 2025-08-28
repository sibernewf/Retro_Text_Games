Ghost Guzzler — Player Documentation
Overview

In Ghost Guzzler, numbers shaped as ghosts drift across the screen toward a barrier. Your task is to match your number (0–9) with the ghost’s number and press the guzzle button before it reaches the barrier. Each success adds to your score; each failure costs you lives. Can you guzzle enough ghosts before your lives run out?

Installation & Running

Install the .NET 6 SDK or later.
Download here
.

Open a terminal/command prompt in the project folder.

Run:

dotnet run

Controls

M → Change your number (cycles from 0 to 9, then back to 0)
.

X → Attempt to guzzle the ghost (only works if your number matches the ghost’s)
.

Gameplay Rules

You start with 3 lives (/// displayed at the top of the screen)
.

Each round, a ghost (number 0–9) appears on the left and drifts toward the barrier (:) on the right
.

Your number is displayed just after the barrier.

During the ghost’s movement window:

Press M to change your number.

Press X to guzzle the ghost.

If your number matches the ghost’s when you press X:

You guzzle it, and your score increases by (18 - ghost’s current position)
.

A new ghost spawns.

If your number doesn’t match, nothing happens, and the ghost keeps moving.

If the ghost reaches the barrier without being guzzled:

You lose 1 life.

If you still have lives, a new ghost appears.

If you run out of lives:

YOUR GHOST GUZZLING
SCORE IS <final score>


…and you are asked if you want another go.

Winning & Losing

There is no “final win” — the game continues until you lose all lives.

Your performance is measured by your score.

Higher score = better ghost guzzling!

Strategy Tips

Time your guzzle attempt when the ghost is still far from the barrier — you earn more points the earlier you catch it.

Cycle your number quickly with M to line up with the incoming ghost.

Don’t panic if you miss one; you have three lives to work with.

The ghost’s movement is fast — stay alert.
