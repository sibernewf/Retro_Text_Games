QUEEN ONE CHESS QUEEN
Description
This game is based on the permissible moves of the chess queen-i.
e., along any vertical, horizontal, or diagonal. In this game,
the queen can only move to the left, down, and diagonally down
to the left.
The object of the game is to place the queen (one only) in the
lower left-hand square (no. 150), by alternating moves between
you and the computer. The one to place the queen there wins.
You go first and place the queen in anyone of the squares on
the top row or the right-hand column. That is your first move.
The computer is beatab1e, but it takes some figuring. See if
you can devise a winning strategy.
Source
Source and author are totally unknown.

QUEEN — One Chess Queen
Player Guide
1) What is this game?

This is a simplified, strategic version of chess where only one queen is in play.
The queen can move only:

Left (same row, toward column 1)

Down (same column, toward row 8)

Diagonally down-left (toward bottom-left corner)

The goal:
Get the queen to square #150 (the lower left-hand square).
The first player to place the queen there wins.

2) Board Layout

The game uses a numbered board rather than standard chess notation.
It’s an 8×8 grid of “squares,” numbered like this (sample from program listing):

 81  71  61  51  41  31  21  11
 92  82  72  62  52  42  32  22
103  93  83  73  63  53  43  33
114 104  94  84  74  64  54  44
125 115 105  95  85  75  65  55
136 126 116 106  96  86  76  66
147 137 127 117 107  97  87  77
150 148 138 128 118 108  98  88


Notes:

The bottom-left square is 150 (goal square).

The top row is 81 71 61 ... 11.

The rightmost column is 11 22 33 ... 88.

3) Starting position

You go first.

Your first move must place the queen in any square in the top row or the rightmost column.

The computer then moves.

You take turns until someone moves the queen to 150.

4) How to enter moves

When asked:

WHAT IS YOUR MOVE?


Type the square number (from the board above).

Example:

If the queen is currently at 81 (top-left) and you want to move down to 125, type:

125

5) Rules

Legal moves: Queen moves only left, down, or diagonally down-left.

Illegal move warning: If you try a move outside the rules, the game will remind you and you’ll have to try again.

First move restriction: You must start from the top row or right column. If you don’t, it’s an illegal start.

Forfeit option: If you can’t (or don’t want to) move, type:

0


to forfeit.
5. Win condition: First to land on 150 wins immediately.

6) Example Game
WHERE WOULD YOU LIKE TO START? 81
MACHINE MOVES TO SQUARE 138

YOUR MOVE? 126
MACHINE MOVES TO SQUARE 150
MACHINE WINS — GAME OVER

7) Strategy Tips

Think in reverse: Work backward from 150 to see the sequences that reach it.

Control the approach path so you can force the machine into giving you the win.

Avoid moves that let the machine land on 150 next turn.

Use diagonals to close distance quickly without giving the opponent the correct approach.

8) Common pitfalls

Forgetting the queen can’t move up or right — once you pass certain positions, there’s no going back.

Not looking at the entire board — the machine may reach 150 diagonally.

Giving away 150 by moving to a square that leaves only one obvious move for the computer.

9) End of game

Winner is announced.

Program will ask:

ANYONE ELSE CARE TO TRY?


You can type YES to play again, or NO to quit.