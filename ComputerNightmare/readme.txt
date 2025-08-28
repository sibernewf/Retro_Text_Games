Computer Nightmare — Player Documentation
Overview

In Computer Nightmare, you are trapped in a battle of wits against the computer. Numbers flash up on the screen, and you must react quickly: if you press the correct number key in time, your score rises. If you hesitate or press the wrong key, your score falls. Survive long enough and you may win — fail, and you become the computer’s slave.

Installation & Running

Install the .NET 6 SDK or newer.
Download here
.

Open a terminal/command prompt in the project folder.

Run:

dotnet run

Controls

When a number (1–9) appears on the left of the screen, press the matching number key on your keyboard
.

You have about half a second to react (default 500ms)
.

If you don’t press a key, or press the wrong number, you lose points.

Gameplay Rules

You start with a score of 300
.

Each round:

A random digit between 1–9 is displayed.

You have a short window to press the matching key.

After the window:

If you pressed the correct number:

Your score increases by 10 + (2 × number).

If you pressed the wrong number (or nothing):

Your score decreases by 10.

As your score changes, the computer taunts you with messages like:

** MICROS RULE! **

** A ROBOT FOR PRESIDENT! **

If your score drops below 0:

YOU'RE NOW MY SLAVE


…and the game ends.

If your score climbs above 500:

OK. YOU WIN (THIS TIME)


…and you escape.

Special Messages

If your score is under 60: <THERE'S NO HOPE>
.

If your score is above 440: URK! HELP!!
.

Winning & Losing

Win Condition: Score > 500.

Lose Condition: Score < 0.

Otherwise the game continues, with numbers and taunts appearing each round.

Strategy Tips

Keep your finger ready on the number keys — the reaction window is very short.

Higher numbers give larger rewards when hit (e.g., pressing 9 correctly adds 28 points).

Avoid letting your score drop below 60; the game warns you that all hope is lost.
