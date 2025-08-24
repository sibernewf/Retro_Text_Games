NIM — Chinese Game of Nim

Two-player strategy game — you vs. the computer

📜 Objective

NIM is one of the oldest known strategy games, dating back to ancient China.
A number of piles (or “rows”) of sticks are arranged between you and your opponent.
On each turn, you must remove at least 1 stick from exactly one pile.
The player who takes the last stick wins.

🛠 Game Setup

When the game starts, you’ll be asked if you want to read the instructions.

You’ll then set the number of piles (1–10).

For each pile, you’ll set the number of sticks (1–20).

You’ll choose whether to go first or let the computer start.

In replayed games, you can keep the same arrangement or set a new one.

📌 Rules

On your turn:

Pick a pile that has at least one stick.

Choose how many sticks to remove (minimum 1, maximum = sticks in that pile).

You cannot:

Skip a turn.

Remove sticks from more than one pile at once.

Remove zero sticks.

The game ends immediately when all piles are empty.
The player who took the last stick is declared the winner.

🖥 Commands During Play

Enter a pile number → selects which pile to remove from.

Enter number of sticks → removes that many from the chosen pile.

Prompts will repeat if you enter an illegal pile or stick count.

🧠 Computer Strategy

The computer uses the optimal Nim strategy based on the “nim-sum” (binary XOR) of the pile sizes:

If the nim-sum is non-zero: the computer will make a move that forces it to zero, putting you at a disadvantage.

If the nim-sum is zero: you are already in a winning position, so the computer will take the smallest legal move and wait for a mistake.

💡 Tips for Winning

Write down the number of sticks in each pile in binary after every move.

Aim to leave the computer with a nim-sum of zero at the end of your turn.

Force the computer to break symmetry and give you control.

🔁 Replay Options

After a game ends:

You can choose to play again.

You can choose to keep the same arrangement or set a new number of piles and sticks.

Swap starting player for a different challenge.