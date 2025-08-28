Ghost Maze (Expanded) — Player Documentation
Overview

The Expanded Ghost Maze builds on the original game with a larger, more dangerous labyrinth. The maze is now 20×20, with longer corridors, more dead ends, and a wandering ghost that won’t stop until it finds you. You can only see a 3×7 slice ahead of you in the direction you’re facing.
Your mission: navigate the maze and find the exit (✚) without getting caught.

Installation & Running

Install the .NET 6 SDK or later.
Download here
.

Open a terminal/command prompt in the project folder.

Run:

dotnet run

Controls

X → Move forward (one step, unless blocked by a wall)
.

N → Turn left (rotate 90°)
.

M → Turn right (rotate 90°)
.

Q → Quit the game
.

Gameplay Rules

You begin in a random corridor, facing a random direction
.

The maze is 20×20, filled with walls (#), corridors (·), one exit (✚), and a ghost (G).

Each move reveals a 3×7 view ahead of you, depending on your facing direction
.

The ghost moves every 5 of your moves, drifting randomly through the maze
.

If you end a move adjacent to the ghost, you are teleported to a random corridor, with a random facing direction:

A GHOST IS BESIDE YOU! *WHOOSH* You are swept elsewhere!


You win by stepping onto the exit ✚.

Visual Symbols

# → Wall (impassable).

· → Corridor (open space).

✚ → Exit (find this to win).

Y → You (the player).

G → Ghost (stay away).

Winning & Losing

Win Condition: Reach the exit ✚. You’ll see:

YOU HAVE ESCAPED!
IN N MOVES.


Lose Condition: The ghost never directly kills you, but it may teleport you endlessly, making escape harder. If stuck, you can quit with Q.

Strategy Tips

Map the maze mentally: use turns and movement counts to keep track.

The ghost moves every 5 turns — watch for it to suddenly appear.

Don’t panic if teleported; just re-orient and keep exploring.

Because the maze is larger, plan carefully to avoid wasting moves retracing dead ends.