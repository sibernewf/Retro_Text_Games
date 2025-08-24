📘 Player Documentation – TOWER: Towers of Hanoi Puzzle
🎮 The Puzzle

The Towers of Hanoi is a classic logic game.
You are given three needles (rods) and a set of 2 to 7 disks of different sizes. All disks start stacked on the leftmost needle, largest at the bottom and smallest on top.

Your goal: Move the entire stack of disks to the rightmost needle, following these rules:

Move only one disk at a time.

Never place a larger disk on top of a smaller disk.

You win when all disks are stacked on the rightmost needle.

🕹️ How to Play

When the game starts, enter the number of disks you want to use (between 2 and 7).

Disks are labeled by size: 3 (smallest), 5, 7, 9, 11, 13, 15 (largest).

If you choose fewer than 7 disks, the program uses the largest n disks.

Example: with 2 disks, you will play with disks 13 and 15.

On each turn:

First, the program asks:
“WHICH DISK WOULD YOU LIKE TO MOVE?”
Enter the disk number (e.g., 11).

Then, the program asks:
“PLACE DISK ON WHICH NEEDLE?”
Enter 1, 2, or 3 for left, middle, or right.

Illegal moves are rejected with a message:

You cannot move a disk that is not on top.

You cannot put a larger disk on a smaller one.

You cannot move a disk onto the same needle.

The board is printed after each move, showing the three rods and their disks as stacks of * characters.

📋 Example Run
HOW MANY DISKS DO YOU WANT TO MOVE? 3

************
************
************
============
     N1         N2         N3

WHICH DISK WOULD YOU LIKE TO MOVE? 15
PLACE DISK ON WHICH NEEDLE? 3

🏆 Winning

When all disks are stacked in order on Needle 3, you win.

The program congratulates you and shows the number of moves you used.

It also tells you the minimum possible moves for that puzzle:

Formula: 2ⁿ − 1 moves (where n = number of disks).

💡 Tips

The puzzle can always be solved.

The strategy:

Move the top n−1 disks to the middle needle.

Move the largest disk to the right needle.

Move the n−1 disks from the middle to the right.

Try to match the optimal number of moves.

▶️ To Play

Double-click run.bat in the game’s folder.

This will build and run the program, and keep the window open when it finishes.
