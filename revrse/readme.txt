REVRSE ORDER A LIST OF NUMBERS
Description
The game of REVERSE requires you to arrange a list of numbers
in numerical order from left to right. To move, you tell the
computer how many numbers (counting from the left) to reverse.
For example, if the current list is:
2 345 1 6 789
and you reverse 4, the result will be:
5 432 1 6 789
Now if you reverse 5, you win!

There are many ways to beat the game, but approaches tend to be
either algorithmic or heuristic. The game thus offers the player
a chance to play with these concepts in a practical (rather than
theoretical) context.
An algorithmic approach guarantees a solution in a predictable
number of moves, given the number of items in the list. For
example, one method guarantees a solution in 2N - 3 moves when
the list contains N numbers. The essence of an algorithmic
approach is that you know in advance what your next move will
be. One could easily program a computer to do this.
A heuristic approach takes advantage of "partial orderings" in
the list at any moment. using this type of approach, your next
move is dependent on the way the list currently appears. This
way of solving the problem does not guarantee a solution in a
predictable number of moves, but if you are lucky and clever,
you may come out ahead of the algorithmic solutions. One could
not so easily program this method.
In practice, many players adopt a "mixed" strategy, with both
algorithmic and heuristic features. Is this better than either
"pure" strategy?
Program Author
Bob Albrecht
People's Computer Co.
Menlo Park, CA 94025

REVERSE — A Game of Skill
🎯 Objective

Your goal is to arrange a list of numbers in ascending numerical order (smallest to largest) from left to right by reversing sections of the list.

If you do it, you win!
The challenge is to do it in the fewest moves possible.

🕹 How to Play

The Game Start

The game will create a list of N random numbers from 1 to N (with no duplicates).

The numbers are shown left-to-right on the screen.

Example:

7 9 4 6 3 1 8 5 2


Making a Move

On your turn, type a number K (from 1 to N) to reverse the first K numbers in the list.

The numbers you specify will be flipped in order.

Example:
If the list is:

2 3 4 5 1 6 7 8 9


and you enter 4, the first four numbers are reversed:

5 4 3 2 1 6 7 8 9


Winning

If the numbers are sorted in order:

1 2 3 4 5 6 7 8 9


you win! 🎉

The game tells you how many moves it took you.

Quitting

If you want to give up, type 0 (zero) to quit.

📌 Tips & Strategies

You can approach REVERSE in three ways:

Algorithmic Strategy

Follow a fixed sequence of moves that will always sort the list in a known maximum number of steps.

One well-known algorithm guarantees sorting in 2N - 3 moves for N numbers.

Heuristic Strategy

Decide your move based on the current arrangement, trying to put the next number in its correct position quickly.

This can sometimes beat algorithmic solutions in fewer moves.

Hybrid Strategy

Start with a heuristic approach and switch to an algorithmic method for the last few numbers.

🧮 Example Playthrough
REVERSE — A Game of Skill
This is the list:
7 9 4 6 3 1 8 5 2

How many shall I reverse? 6
1 3 6 4 9 7 8 5 2

How many shall I reverse? 2
3 1 6 4 9 7 8 5 2

How many shall I reverse? 3
6 1 3 4 9 7 8 5 2

...

How many shall I reverse? 5
1 2 3 4 5 6 7 8 9
You win! Sorted in 22 moves!

💡 Player Notes

Large reversals (K close to N) can move big numbers to the back faster.

Small reversals help fine-tune the last few positions.

Avoid reversing 1 unless it’s part of a plan — it does nothing.

If you’re stuck, look for the largest number not in place and reverse it into position.


