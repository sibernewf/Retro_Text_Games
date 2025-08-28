Gravedigger — Player Documentation
Overview

In Gravedigger, you are trapped in a deadly graveyard. Each step you take digs a tunnel behind you. But beware: if you ever walk back into your own tunnel, you’ll fall into your grave! Random stones drop into the graveyard as obstacles, and your only hope is to escape by reaching the edge of the graveyard grid.

Installation & Running

Install the .NET 6 SDK or later.
Download here
.

Open a terminal/command prompt in the project folder.

Run:

dotnet run


The game launches directly in the terminal window.

Controls

Arrow Keys → Move up, down, left, or right
.

Gameplay Rules

You start at the center of a 20×20 grid graveyard
.

Each move leaves behind a dug tunnel (shown as a blank space).

If you step back into a dug tunnel, you immediately die:

YOU FELL INTO YOUR OWN GRAVE!


Randomly, stones (#) fall into the graveyard, blocking paths
.

Stones cannot be moved through.

The game ends successfully if you reach any border of the grid:

YOU ESCAPED THE GRAVEYARD!

Visual Symbols

☺ → You (the gravedigger).

. → Fresh ground (can be dug).

(space) → Tunnel you’ve dug (danger to re-enter).

# → Stone (obstacle).

Winning & Losing

Win Condition: Reach the edge of the grid and escape.

Lose Condition: Step into your own tunnel and fall into your grave.

Strategy Tips

Plan your route carefully — don’t trap yourself.

Watch out for random stone drops; they can block escape paths unexpectedly.

Avoid creating loops that you might accidentally walk back into.
