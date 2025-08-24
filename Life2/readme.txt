LIFE·2 — Player Instructions
Overview
LIFE·2 is a head-to-head variant of Conway’s Game of Life played on a 5×5 board.

Player 1 pieces = *

Player 2 pieces = #
You place pieces, then the board advances one Life generation after both players act. The last player with any live pieces wins.

Goal
Eliminate your opponent’s live cells (* or #) through careful placement and Life rule evolution.

Board & Neighbors
Each square has 8 neighbors (orthogonal + diagonal). Life updates happen simultaneously after both players place for the turn.

Life Rules (with color-majority births)
Survival: Any live cell with 2 or 3 neighbors survives; otherwise it dies (isolation: 0–1, overcrowding: 4+).

Birth: An empty square with exactly 3 neighbors becomes alive. Its owner is the player who holds the majority of those three neighbors (2 vs 1).

Example: neighbors #, #, * → the new cell is #.

Setup
Initial placement:

Each player chooses 3 coordinates (X,Y, 1–5) for starting pieces.

If both choose the same square, it remains empty.

The game prints the initial board, then applies one Life generation.

Turn Structure (each round)
Player 1 enters one empty square (X,Y).

Player 2 enters one empty square (X,Y).

If both select the same square, it stays empty (SAME COORD. SET TO 0).

If a chosen square is out of range or already occupied, you’ll be asked to RETYPE (ILLEGAL COORDS. RETYPE).

The board advances one generation by the rules above and is printed.

Repeat until a player has 0 live pieces.

Ending the Game
If a player reaches zero live cells, the other player wins.

If both players reach zero simultaneously, it’s a draw.

Reading the Board
Coordinates are shown along the top/bottom (columns 1–5) and left/right (rows 1–5).

Symbols: * = Player 1, # = Player 2, . = empty.

Tips & Strategy
Create 3-neighbor traps: aim to make empty squares with 2 of yours + 1 of theirs so births flip to your color.

Avoid 4+ neighbor clusters—they die of overcrowding next step.

Edge play can control births while limiting enemy adjacency.

Think one generation ahead: your placement should survive and/or create majority births after the update.

Example Mini-Turn

Before placements:
. . . . .
. * . . .
. . # . .
. # . . .
. . . . .

P1 places at 3,3   P2 places at 2,3
→ Apply Life generation
→ New births/survivals resolved, board printed