LIFE — John Conway’s Game of Life
Overview
LIFE is a computer simulation of John Conway’s famous Game of Life, first described by Martin Gardner in Scientific American (October 1970).

It models the life cycle of a population of “cells” on a 24 × 70 grid according to Conway’s simple yet unpredictable rules.

You start by entering an initial pattern of live cells, and the computer will display successive generations as the population evolves.

The Grid
The play area is 24 cells tall (vertical) × 70 cells wide (horizontal).

Each position on the grid is either:

Alive (cell present)

Dead (empty space)

Every cell has 8 neighbors — 4 orthogonal and 4 diagonal.

Rules of Life
The population changes from one generation to the next according to these rules:

Survivals

Any living cell with 2 or 3 neighbors survives.

Deaths

Any living cell with 4 or more neighbors dies from overpopulation.

Any living cell with 0 or 1 neighbor dies from isolation.

Births

Any empty cell with exactly 3 neighbors becomes a live cell in the next generation.

All births and deaths happen simultaneously, forming a new generation.

How to Play
When prompted, enter your starting pattern using * for live cells and spaces for empty cells.

End your input with CTRL+Z (or your system’s end-of-input command).

The simulation begins at Generation 0 and displays:

The generation number.

The population (total number of live cells).

The computer then calculates and displays each new generation automatically.

Game Flow
The population may:

Die out (all cells gone).

Settle into a stable form (unchanging pattern).

Enter an oscillating cycle (repeating loop of 2+ shapes).

The simulation runs until you stop it.

Example Session

ENTER YOUR PATTERN:
  *
   *
 *** 

GENERATION: 0   POPULATION: 5
(24×70 grid display here)

GENERATION: 1   POPULATION: 6
(Updated pattern...)

GENERATION: 2   POPULATION: 7
...
Tips
Small, simple patterns can lead to surprisingly complex behavior.

Asymmetrical shapes often evolve toward symmetry.

Classic starting patterns to try:

Glider

Blinker

Toad

Pulsar

R-pentomino
