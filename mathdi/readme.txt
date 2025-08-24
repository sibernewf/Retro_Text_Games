MATHDI — Pictorial Addition Practice
Overview
MATHDI is a pictorial math drill game that uses ASCII-art dice to help players practice basic addition skills.
It’s designed for beginners — especially children — because the answers can be found either by counting the spots or by remembering simple addition facts.

This version is based on the original program by Jim Gerrish for the Bernice A. Ray School in Hanover, NH.

How to Play
Game Start
The program explains the rules: you’ll see two dice printed side by side, followed by an equal sign (=) and a question mark (?).
Your goal is to type the sum of the two dice and press Enter.

First Attempt

If your answer is correct, the game prints:

  RIGHT!

and immediately rolls the dice again.

If your answer is wrong, you’ll get:

  NO, COUNT THE SPOTS AND GIVE ANOTHER ANSWER.

Then you can try again.

Second Attempt

If you get it right on the second try, you’ll still see RIGHT! and the game will roll again.

If you miss again, the program tells you the correct answer:

  NO. THE ANSWER IS 6

Repeat
The game continues with new random dice rolls for as long as you like.

Quitting the Game
In this C# version, type Q instead of a number to quit (the original required CTRL/C).

Example Play

. . .    . . .
. * .    .   .
. . .    . * .
.   .    .   .
. . .    . . .
= ?
7
RIGHT!

THE DICE ROLL AGAIN.....

. . .    . . .
.   .    . * .
. * .    .   .
.   .    . * .
. . .    . . .
= ?
8
NO, COUNT THE SPOTS AND GIVE ANOTHER ANSWER.
? 9
RIGHT!

Tips
If you’re not sure, count the dots — that’s what they’re there for.

This is a good way to get faster at adding small numbers in your head.

There’s no penalty for wrong answers — the goal is learning, not competition.

