RUSSIAN ROULETTE
📜 Overview

In this dark-humor chance game, you’re given a revolver with one bullet and five empty chambers.
Each turn, you may:

Spin the chamber and pull the trigger — risking your life.

Give up — walk away while you’re still alive.

Survive ten pulls and you win.
At any pull, there’s a 1-in-6 chance the gun will fire.

💻 Requirements

OS: Windows 10/11 (x64)

.NET:

Recommended: .NET 8 or 9 SDK (to build and run)

Minimum to run only: .NET 8 or 9 Runtime

Runs in a standard console/terminal window.

No extra libraries required.

🛠 Installation
1. Install .NET

Download the .NET 8 SDK or newer from:
https://dotnet.microsoft.com/download

Verify installation:

dotnet --info


You should see .NET SDK version 8.x or 9.x.

2. Get the Game

If you have the compiled project:

Double-click run.bat if included, or

In the project folder:

dotnet run -c Release


If you only have the source:

dotnet new console -n RussianRoulette
move Program.cs RussianRoulette\Program.cs
cd RussianRoulette
dotnet run -c Release

🎮 How to Play
Game Start

You’ll see:

THIS IS A GAME OF >>>>>>>>>>>>RUSSIAN ROULETTE

HERE IS A REVOLVER
HIT '1' TO SPIN CHAMBER AND PULL TRIGGER.
(HIT '2' TO GIVE UP)

Your Turn

Enter 1 → Spin chamber and pull trigger

If the bullet is in the firing chamber: BANG! YOU'RE DEAD!

Otherwise: "- CLICK -" appears and you live to pull again.

Enter 2 → Quit the game immediately

Win Condition

Survive 10 pulls without firing the bullet → YOU WIN!!!

You can still quit early by entering 2.

Death

If you hit the bullet:

BANG!!!!  YOU'RE DEAD!
CONDOLENCES WILL BE SENT TO YOUR RELATIVES.


Then the game offers you a chance to start over.

Replay

After dying, quitting, or winning:

The game asks: "GO AGAIN (YES/NO)?"

Type YES to restart, NO to exit.

📊 Probability

Each spin is independent — there’s always a 1/6 chance of dying on any given pull.

Because you spin each time, it doesn’t matter how many times you’ve survived — odds never improve.

💡 Tips for Players

The game is pure chance — there’s no strategy to survive beyond quitting.

Surviving all 10 pulls is rare; the odds are roughly (5/6)¹⁰ ≈ 16%.