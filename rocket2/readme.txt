ROCKET2 — Player & Setup Guide
📜 Overview

ROCKET2 is a text-based Lunar Landing Simulation, adapted from a BASIC program in 101 BASIC Computer Games.
You are the pilot of the Apollo-era Lunar Excursion Module (LEM). Your mission: land safely on the Moon by controlling thrust, burn duration, and attitude angle.

Each turn, you issue a command to the onboard computer:

T — Time interval (seconds) for the burn

P — Percentage of maximum thrust (0–100)

A — Attitude angle (degrees) relative to vertical

Your goal: reduce both vertical and horizontal velocity to within safe limits before altitude reaches zero.

💻 Requirements

OS: Windows 10/11 (x64)

.NET:

Recommended: .NET 8 or 9 SDK (to build and run)

Minimum to run only: .NET 8 or 9 Runtime

Console Application — runs in a terminal (CMD, PowerShell, or similar)

No extra dependencies — everything is built into .NET

🛠 Installation
1. Install .NET

Download the .NET 8 SDK (or newer) from:
https://dotnet.microsoft.com/download

Confirm installation:

dotnet --info


You should see .NET SDK with version 8.x or 9.x.

2. Get the Game

Place Program.cs (ROCKET2 source) into a folder.

Open a terminal in that folder and run:

dotnet new console -n Rocket2
move Program.cs Rocket2\Program.cs
cd Rocket2
dotnet run -c Release


Or if you already have the full project:

Double-click run.bat (builds & runs automatically).

🎮 Gameplay
Starting the Game

You’ll be prompted for:

Apollo experience — purely cosmetic question.

Unit system — English (feet, ft/s, lb) or Metric (m, m/s, kg).

Instruction detail — Output only, short instructions, or complete instructions.

Controls

At each turn, you enter:

T — Burn time in seconds (must be > 0 to act)

P — Thrust as a % of maximum (0–100)

A — Attitude angle in degrees

0° = thrust straight up (against gravity)

+ = rotate thrust vector toward horizontal (to the right)

- = rotate thrust vector toward horizontal (to the left)

Range: -180° to +180°

Example input:

5, 60, 0


= burn for 5 seconds at 60% thrust, pointing straight up.

To abort the mission:

0, 0, 0

HUD (Output)

After each step you’ll see:

t (s)	ALT (ft/m)	V-vert (ft/s or m/s)	V-horz (ft/s or m/s)	FUEL (lb/kg)
Elapsed time	Current altitude	Vertical velocity (+up / -down)	Horizontal drift speed	Remaining fuel
Objective

Land softly before altitude reaches zero:

Safe vertical speed: ≤ 2 m/s (≈ 6.56 ft/s) downward

Safe horizontal speed: ≤ 1 m/s (≈ 3.28 ft/s)

Endings

Safe landing:
"TRANQUILLITY BASE HERE -- THE EAGLE HAS LANDED"
"CONGRATULATIONS -- THERE WAS NO SPACECRAFT DAMAGE."

Crash:
"CRASH !!!!!!!!!!!!!"
Your impact speed and crater depth are displayed.

Abort:
"MISSION ABORTED"

You can choose to fly again after each mission.

🧮 Physics Model

Gravity: Moon gravity (1.62 m/s² or 5.318 ft/s²)

Fuel burn: Approximated using the rocket equation and Isp.

Attitude: Changes how thrust is split between vertical and horizontal.

Out of fuel: Thrust drops to zero, descent continues under gravity.

💡 Tips for Landing

Bleed off horizontal speed early — rotate thrust vector sideways while you still have altitude to spare.

Don’t waste fuel hovering — burn efficiently in short bursts.

Final approach: Keep vertical speed low before the last 20–50 m (or ~150 ft).

Out of fuel = crash unless you’re already within safe limits.

🔧 Troubleshooting

App doesn’t start / .NET not found:
Install or repair the .NET SDK/Runtime.

Weird units or wrong outputs:
Make sure you chose the right measurement system at the start.

Numbers too hard to read:
Resize the console or run in full-screen mode.

📜 Credits

Original BASIC game: 101 BASIC Computer Games (David Ahl)

C# port: adapted and modernized, preserving the core gameplay and messages.
