QUBIC — Player Guide
1) What is QUBIC?

QUBIC is tic-tac-toe played in three dimensions on a 4×4×4 cube.
There are 64 squares arranged as 4 levels (like 4 boards), each level has 4 columns × 4 rows.

Win condition: first player to place four marks in a straight line wins.
Lines can be:

Straight across a row, a column, or straight up through levels

Any diagonal on a level (2D diagonals)

Any diagonal across levels (2D diagonals in vertical “slices”)

One of the 4 space diagonals corner-to-corner through the cube

The game automatically checks all 76 possible winning lines each move.

2) How do I run it?

Run the console app. You’ll see:

Optional instructions

A choice to move first or let the machine start

Continuous play with a Play Again prompt at the end

3) Input format (moves)

Every square is addressed by three digits: Level, Column, Row — each 1..4.

You can type either:

Compact: 234 (means Level 2, Column 3, Row 4), or

Comma-separated: 2,3,4

The game accepts either style.

Coordinates orientation

Level: 1 (bottom) → 4 (top)

Column: 1 (left) → 4 (right)

Row: 1 (top) → 4 (bottom)

If you enter an already-used square or an invalid coordinate, the game will tell you and ask again.

4) Seeing the board

The game is intentionally quiet to keep the console readable.
You can print the whole cube any time by typing:

SHOW


This prints each LEVEL 1..4 with a 4×4 grid:

X = your mark

O = machine mark

. = empty

Example:

LEVEL 2
  C1 C2 C3 C4
R1  .  X  .  .
R2  .  .  .  .
R3  .  .  O  .
R4  .  .  .  .

5) Turn order

You and the machine alternate turns.

You choose who starts at the beginning of each game.

6) End of game

If someone wins, the program prints:
“MACHINE WINS.” or “YOU WIN”
and shows the 4 coordinates that make the winning line.

If all 64 squares are filled without a line, the game declares a draw.

You’ll be asked whether you want to play another game.

7) Commands reference
Command	When to use	What it does
234 or 2,3,4	On your turn	Place your mark at L2 C3 R4
SHOW / BOARD	Anytime at your turn prompt	Prints all 4 levels of the cube
YES / NO	For prompts (instructions, first move, replay)	Confirms a yes/no choice
8) Winning lines (concept checklist)

You can win with four in a line along any of these patterns:

On a single level (like classic tic-tac-toe):

4 rows × 4 columns per level → 8 lines × 4 levels = 32

2 diagonals per level → 2 × 4 levels = 8

Through levels:

“Pillars” (same column & row across levels) → 4×4 = 16

“Vertical diagonals” on a constant column slice → 2 per slice × 4 slices = 8

“Vertical diagonals” on a constant row slice → 2 per slice × 4 slices = 8

Space diagonals through the cube corners → 4

Total = 76 lines. The program checks all of them for you.

9) Strategy tips

Beginner

Use SHOW frequently to keep your mental picture accurate.

Build two-in-a-row and three-in-a-row threats that extend in multiple directions.

Block obvious machine threats immediately.

Intermediate

Favor central squares (they lie on more potential lines).

Aim to create forks: two or more lines that can complete next turn.

Advanced

Think in slices (constant Level, constant Column, constant Row).
Try to control a slice while watching the two 2D diagonals in it.

Beware the 4 space diagonals; they’re easy to miss and deadly.

10) What’s the AI like?

The machine plays a strong pragmatic game:

It wins immediately if possible.

It blocks any immediate human win.

Otherwise it chooses a move that:

participates in many promising lines,

advances its own lines,

pressures your lines, and

prefers more central squares.

It’s beatable, but careless play will get punished.

11) Troubleshooting / FAQ

Q: I typed 24 and it rejected it. Why?
A: Moves need three coordinates (Level, Column, Row), each 1..4. Use 234 or 2,3,4.

Q: Can I see the board after the machine moves?
A: Yes. Just type SHOW on your next turn.

Q: Rows look upside-down to me.
A: Rows are printed top-to-bottom (R1 at top), which keeps coordinates human-friendly in the console.

Q: Can I change symbols?
A: In code, X = human, O = machine. You can tweak them in PrintBoard() if you like.

12) Optional house rules (easy tweaks)

Always show the board after every move (call PrintBoard() right after each move).

Timed turns (reject slow inputs after N seconds).

Undo (keep a move stack and allow one step back).

If you want any of these built in, say the word and I’ll wire them up.

13) Quick start checklist

Run the game.

Answer YES/NO for instructions.

Choose whether to move first.

On your turn, type a three-digit coordinate or SHOW.

Play to four in a line.

At the end, choose YES to play again, or NO to quit.