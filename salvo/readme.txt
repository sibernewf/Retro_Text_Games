SALVO — Naval Gun Battle

A modern C# console port of Larry Siegel’s classic naval strategy game from BASIC Computer Games.

Objective

Sink all of your opponent’s ships before they sink yours.
Both you and the computer have separate 10×10 grids and fleets of ships.
You’ll fire salvos — multiple shots per turn — based on the number and type of ships you still have afloat.

Requirements

Windows, macOS, or Linux with .NET 6 or newer installed.

Runs in any terminal/command prompt that supports basic text output.

To check if you have .NET installed:

dotnet --version


If this prints a version number (6.0 or higher), you’re ready to go.

Starting the Game

Open a terminal/command prompt in the folder containing the compiled game (dotnet build will produce it in bin/Debug/net6.0 by default).

Run:

dotnet run


Read the introduction and choose whether to:

Auto-place your fleet (ships are placed randomly in valid positions)

Manually place your fleet by entering start and end coordinates for each ship.

Fleet Details

You and the computer each have:

Ship Name	Length	Shots if Afloat
Battleship	5	3
Cruiser	3	2
Destroyer (A)	2	1
Destroyer (B)	2	1

Ships can be placed horizontally, vertically, or diagonally.

Ships cannot overlap and must fit fully within the 10×10 grid.

Gameplay

The game board uses row,col coordinates from 1 to 10 (e.g., 4,7 means row 4, column 7).

Shots per turn = sum of shot values for your surviving ships.

Each turn:

You fire first — enter all your shot coordinates for that turn.

The game reports hits, misses, and if you sank any ships.

The computer fires back — it also fires multiple shots per turn.

You cannot fire twice at the same square.

The game ends when all ships on one side are sunk.

Input Format

When entering coordinates:

Use a comma or space: 4,7 or 4 7

Coordinates must be integers from 1 to 10.

For manual ship placement, enter both start and end coordinates for the ship’s length and direction.

Winning the Game

You win by sinking all four of the computer’s ships.

You lose if the computer sinks all of yours.

Example Session
SALVO — NAVAL GUN BATTLE
------------------------
10x10 grid, separate boards. Place these ships (H/V/diagonal):
  BATTLESHIP (5 squares)  → grants 3 shots while afloat
  CRUISER (3 squares)     → grants 2 shots while afloat
  DESTROYER(A) (2 squares)→ grants 1 shot while afloat
  DESTROYER(B) (2 squares)→ grants 1 shot while afloat

AUTO-PLACE YOUR SHIPS? (YES/NO) yes

YOUR FLEET IS SET:
  BATTLESHIP    len=5 at (2,1) (3,2) (4,3) (5,4) (6,5)
  CRUISER       len=3 at (8,1) (8,2) (8,3)
  DESTROYER(A)  len=2 at (5,8) (5,9)
  DESTROYER(B)  len=2 at (10,3) (10,4)

===== TURN 1 =====
YOU HAVE 7 SHOTS
Shot 1/7 (r,c): 4,4
Shot 2/7 (r,c): 4,5
...
HIT at 4,5 → CRUISER
MISS at 4,4
(You scored 1 hit.)

ENEMY HAS 7 SHOTS
THEY MISS at 7,3
THEY HIT YOUR DESTROYER(B) at 10,4
...

Tips & Strategy

Early on, you have 7 shots per turn — use them to scout the enemy board.

If you find one part of a ship, fire around it in subsequent turns to sink it quickly.

Remember: sinking bigger ships reduces the enemy’s firepower.

Use diagonal placements to make your ships harder to find.
