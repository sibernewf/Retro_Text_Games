Mad House — Player Documentation
Overview

In Mad House, you are trapped in a sinister building with three locked rooms ahead of you. Each room has a moving doorway that slides up and down. Your goal is to align all three doors at the same row and make a daring dash for freedom before the sound of approaching footsteps reaches you…

Installation & Running

Ensure you have .NET 6 SDK or newer installed.
Download here
.

Open a terminal/command prompt in the project folder.

Run:

dotnet run

Controls

A or X → Reverse the movement of the near (left) door
.

N or M → Reverse the movement of the far (right) door
.

J → Dash through the doors (only works if all three are aligned)
.

The middle door moves on its own and cannot be controlled.

Gameplay Rules

The three doorways continuously move up and down.

The near and far doors can have their directions reversed by the player.

The middle door moves automatically — you must time your reversals carefully.

When the doors are aligned, a message appears:

>>> DOORWAYS ALIGNED! Press J **now** to dash! <<<


If you press J while they are aligned, you escape.

If you press J while misaligned, nothing happens.

Footsteps Timer

You begin with 300 footsteps on the counter
.

Each “tick” reduces the counter by 10.

If the counter reaches 0 before you dash, you lose:

TOO LATE... THE FOOTSTEPS HAVE STOPPED.
A cold hand grips your shoulder...

Winning & Losing

Win Condition: Align all three doors and press J in time.

YOU'RE OUT! YOU'RE FREE!!


Lose Condition: The footsteps counter runs out.

TOO LATE... THE FOOTSTEPS HAVE STOPPED.

Strategy Tips

Watch the rhythm of the doors: the middle door is the key to timing.

Use quick reversals on the near/far doors to “sync” them with the middle.

Don’t wait too long — the footsteps are always getting closer.
