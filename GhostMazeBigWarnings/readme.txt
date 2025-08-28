Ghost Maze (Bigger, Spooky) — Player Documentation
Overview

The Spooky Ghost Maze is the expanded 20×20 version of the original Ghost Maze.
You must navigate twisting corridors, using only a limited first-person style view. Somewhere lies the exit (✚) — but the maze is haunted. A ghost drifts through the corridors, and each time it moves, you’ll hear an eerie warning. If it gets too close, it teleports you elsewhere in the maze.

Your goal: find the exit and escape before you are lost forever.

Installation & Running

Install the .NET 6 SDK or newer.
Download here
.

Open a terminal/command prompt in the project folder.

Run:

dotnet run

Controls

X → Move forward (one step, unless a wall blocks you)
.

N → Turn left (rotate 90° counter-clockwise)
.

M → Turn right (rotate 90° clockwise)
.

Q → Quit the game
.

Gameplay Rules

You begin in a random corridor, facing a random direction
.

The maze is 20×20, with walls (#), open corridors (·), an exit (✚), and a ghost (G).

Each move shows a 3×7 slice ahead of you, representing what you see in your current facing direction
.

The ghost moves every 5 player moves. When it moves, you will see a random spooky message, such as:

You hear footsteps echoing…

A chill runs down your spine…

The ghost whispers nearby…

If you end a move adjacent to the ghost, you are instantly teleported to a random corridor, facing a random direction:

A GHOST IS BESIDE YOU! *WHOOSH* You are swept elsewhere!


The ghost is also relocated after this teleport.

You win by reaching the exit ✚.

Visual Symbols

# → Wall (impassable).

· → Corridor (open space).

✚ → Exit (step here to escape).

Y → You (the player).

G → Ghost (avoid it).

Winning & Losing

Win Condition: Step onto the exit ✚. You’ll see:

YOU HAVE ESCAPED!
IN N MOVES.


Lose Condition: The ghost never kills you outright, but constant teleports can trap you in endless wandering. You can always quit with Q.

Strategy Tips

Use the spooky warnings as clues: the ghost is moving and may be near.

Count moves: every 5 steps, expect the ghost to shift.

If teleported, don’t panic. Take note of the new corridors and keep searching.

Larger maze = longer paths. Avoid wasting time retracing dead ends.
