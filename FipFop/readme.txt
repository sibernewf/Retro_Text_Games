FIPFOP — Flip-Flop Game
Converted from BASIC to C#
Original Author: Michael Kass

Description
The goal of FIPFOP is to turn a line of X characters into 0 characters.
You start with:

  X X X X X X X X X X

and try to change it to:

  0 0 0 0 0 0 0 0 0 0

You do this by typing in the position number (1–10) of an X (or 0).
Typing a number flips that position from X→0 or 0→X, and may also flip a second position determined by a hidden formula. Sometimes only one position changes; sometimes two.

The fewer moves you use, the better — solving it in 12 moves or fewer is considered “VERY GOOD”.

Controls
 - 1–10 → Flip the character at that position (and possibly another position).
 - 0 → Reset to all X’s for the current puzzle seed (same sequence as before).
 - 11 → Start a new puzzle with a new random sequence.
 - Q → Quit the game immediately (works any time you are asked for a number or at the replay prompt).
 - Y / YES → Start another puzzle when prompted.
 - N / NO → End the game when prompted.

Gameplay Flow
At the start of a puzzle, the board is all X’s.

Enter the position number you want to flip.

That position flips X↔0.

A second position may also flip, depending on the puzzle’s hidden formula.

The board is reprinted so you can see the change.

Repeat until all positions are 0.

You’ll be told how many guesses it took:

≤12 → “VERY GOOD.”

12 → Encouraged to try harder.

Choose whether to play another puzzle.

Winning
The game ends when all characters are 0.

You win regardless of how many moves it takes, but the challenge is to optimize your strategy.

Tips
Watch for patterns: certain positions tend to flip the same second position each time in a given puzzle.

Reset with 0 to test theories without starting a new puzzle.

Use 11 if you want a completely fresh random puzzle.

Example Session:
THE OBJECT OF THIS PUZZLE IS TO CHANGE THIS:
X X X X X X X X X X

TO THIS:
0 0 0 0 0 0 0 0 0 0

HERE IS THE STARTING LINE OF X'S:
1 2 3 4 5 6 7 8 9 10
X X X X X X X X X X

INPUT THE NUMBER? 3
1 2 3 4 5 6 7 8 9 10
X X 0 X X X X 0 X X

INPUT THE NUMBER? 7
1 2 3 4 5 6 7 8 9 10
X X 0 X X X 0 0 X X

... (game continues) ...

VERY GOOD. YOU GUESSED IT IN ONLY 12 GUESSES!!!!
DO YOU WANT TO DO ANOTHER PUZZLE? Y

