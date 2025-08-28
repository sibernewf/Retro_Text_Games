Ghost Maze — Player Documentation (Basic Version)
Overview

In Ghost Maze, you are trapped inside a twisting 12×12 maze.
You can only see a limited 3×5 window ahead of you in the direction you’re facing. Somewhere in the maze is the exit (✚), but beware — a roaming ghost (G) also lurks within. If you get too close to the ghost, it will teleport you to a random place in the maze, leaving you disoriented.

Your goal: find the exit and escape before the ghost catches you again.

Installation & Running

Install the .NET 6 SDK or newer.
Download here
.

Open a terminal/command prompt in the project folder.

Run:

dotnet run

Controls

X → Move forward one step (if not blocked by a wall)
.

N → Turn left (rotate 90°)
.

M → Turn right (rotate 90°)
.

Q → Quit the game
.

Gameplay Rules

You start at a random corridor in the maze, facing a random direction
.

Each move reveals a 3×5 slice ahead of you (walls #, corridors ·, exit ✚, ghost G, you Y).

Every 5 moves, the ghost drifts randomly to a new corridor location
.

If you ever end a move adjacent to the ghost, you are teleported to a random corridor, with a random facing direction
.

You win by stepping onto the exit ✚.

Visual Symbols

# → Wall (impassable).

· → Open corridor.

✚ → Exit (your way out).

Y → You (the player).

G → Ghost (avoid it).

Winning & Losing

Win Condition: Reach the exit ✚. You’ll see:

YOU HAVE ESCAPED!
IN N MOVES.


Lose Condition: There is no hard “game over” — but the ghost may teleport you endlessly, making escape difficult if you’re unlucky. You can always press Q to give up.

Strategy Tips

Map the maze in your head as you move. Turning left/right to build mental landmarks helps.

Keep track of move counts — the ghost shifts every 5 moves, so expect it to appear in new corridors.

Don’t panic if teleported: keep exploring until you relocate the exit.
